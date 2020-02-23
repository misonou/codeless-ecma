using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  public abstract class RuntimeModule {
    private HashSet<Type> visitedTypes = new HashSet<Type>();
    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> globals = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[] properties;
    private RuntimeObject[] runtimeObjects;
    private int objectCount;

    public abstract Type EnumType { get; }

    public RuntimeRealm Realm { get; private set; }

    public ISharedObjectContainer Container { get; private set; }

    public SharedObjectHandle GetSharedObjectHandle(Enum value) {
      Guard.ArgumentNotNull(value, "value");
      if (value.GetType() != this.EnumType) {
        throw new ArgumentException(String.Format("Value must be of type {0}", this.EnumType.FullName));
      }
      ThrowIfNotInitialized();
      return new SharedObjectHandle(this.Container.ID, (int)(object)value);
    }

    internal void Init(RuntimeRealm.SharedObjectContainer container, RuntimeObject[] moduleObjects) {
      this.Container = container;
      this.Realm = RuntimeRealm.Current;
      this.runtimeObjects = moduleObjects;
      this.objectCount = moduleObjects.Length;
      this.properties = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[objectCount];
      for (int i = 0; i < objectCount; i++) {
        properties[i] = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
      }
      OnBeforeInitializing(moduleObjects);
      DefineIntrinsicObjectsFromAssembly(this.GetType().Assembly);
      for (int i = 0; i < objectCount; i++) {
        DefineAllProperties(EnsureObject(i), properties[i]);
      }
      DefineAllProperties(this.Realm.GetRuntimeObject(WellKnownObject.Global), globals);
      OnAfterInitializing(new ReadOnlyCollection<RuntimeObject>(container.FlushObjects()));
    }

    protected virtual void OnBeforeInitializing(IList<RuntimeObject> runtimeObject) {
    }

    protected virtual void OnAfterInitializing(IList<RuntimeObject> runtimeObject) {
    }

    protected void ThrowIfNotInitialized() {
      if (this.Container == null) {
        throw new InvalidOperationException("Module is not registered");
      }
    }

    protected virtual void DefineIntrinsicObjectsFromAssembly(Assembly assembly) {
      Type[] types;
      try {
        types = assembly.GetTypes();
      } catch (ReflectionTypeLoadException ex) {
        types = ex.Types.OfType<Type>().ToArray();
      }
      foreach (Type type in types) {
        DefineIntrinsicObjectFromType(type);
      }
    }

    protected virtual void DefineIntrinsicObjectFromType(Type type) {
      if (type.HasAttribute(out IntrinsicObjectAttribute typeAttr) && visitedTypes.Add(type)) {
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        DefineIntrinsicObjectFromType(type, typeAttr);
      }
    }

    private void DefineIntrinsicObjectFromType(Type type, IntrinsicObjectAttribute typeAttr) {
      int objectIndex = ToValidIndex((Enum)typeAttr.ObjectType);
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht = properties[objectIndex];
      SharedObjectHandle handle = new SharedObjectHandle(this.Container.ID, objectIndex);

      if (IsValidReference(typeAttr.Prototype, out SharedObjectHandle protoHandle)) {
        RuntimeObject thisObj = EnsureObject(objectIndex);
        thisObj.SetPrototypeOf(this.Realm.GetRuntimeObject(protoHandle));
      }
      if (typeAttr.Global) {
        globals[typeAttr.Name ?? type.Name] = CreateSharedObjectDescriptor(handle, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
      }
      foreach (MemberInfo member in type.GetMembers()) {
        // special handling if the type defines an instrinsic contructor
        // replace the default object created from EnsureWellKnownObject to InstrincFunction
        if (member.HasAttribute(out IntrinsicConstructorAttribute ctorAttr)) {
          string ctorName = ctorAttr.Name ?? member.Name;
          runtimeObjects[objectIndex] = CreateIntrinsicFunction(ctorName, (MethodInfo)member, ctorAttr.SuperClass as Enum, WellKnownObject.Global, ctorName);
          if (IsValidReference(ctorAttr.Prototype, out SharedObjectHandle p1)) {
            Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> hp = properties[ToValidIndex((Enum)ctorAttr.Prototype)];
            ht[WellKnownProperty.Prototype] = CreateSharedObjectDescriptor(p1, EcmaPropertyAttributes.None);
            hp[WellKnownProperty.Constructor] = CreateSharedObjectDescriptor(handle, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
          }
          if (ctorAttr.Global) {
            globals[ctorName] = CreateSharedObjectDescriptor(handle, EcmaPropertyAttributes.DefaultMethodProperty);
          }
          continue;
        }

        object[] propAttrs = member.GetCustomAttributes(typeof(IntrinsicMemberAttribute), false);
        if (propAttrs.Length > 0) {
          EcmaValue sharedValue = default;
          if (member.HasAttribute(out AliasOfAttribute aliasOf) && aliasOf.ObjectType is Enum aliasOfType) {
            EcmaPropertyKey aliasOfKey = aliasOf.Name != null ? (EcmaPropertyKey)aliasOf.Name : aliasOf.Symbol;
            if (!properties[ToValidIndex(aliasOfType)].TryGetValue(aliasOfKey, out EcmaPropertyDescriptor descriptor)) {
              // for sake of simplicity the aliased target should be defined on intrinsic object with smaller WellKnownObject enum value
              // to avoid the need of topological sort
              throw new InvalidOperationException();
            }
            sharedValue = descriptor.Value;
          }
          if (sharedValue == default) {
            switch (member.MemberType) {
              case MemberTypes.Method:
                IntrinsicMemberAttribute propAttr = (IntrinsicMemberAttribute)propAttrs[0];
                EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                string runtimeName = (propAttr.Getter ? "get " : propAttr.Setter ? "set " : "") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
                sharedValue = this.Container.Add(CreateIntrinsicFunction(runtimeName, (MethodInfo)member, null, typeAttr.ObjectType as Enum, name)).ToValue();
                break;
              case MemberTypes.Field:
                object fieldValue = ((FieldInfo)member).GetValue(null);
                sharedValue = IsValidReference(fieldValue, out SharedObjectHandle p1) ? p1.ToValue() : new EcmaValue(fieldValue);
                break;
              case MemberTypes.Property:
                object propertyValue = ((PropertyInfo)member).GetValue(null, null);
                sharedValue = IsValidReference(propertyValue, out SharedObjectHandle p2) ? p2.ToValue() : new EcmaValue(propertyValue);
                break;
            }
          }
          foreach (IntrinsicMemberAttribute propAttr in propAttrs) {
            EcmaPropertyKey name = GetNameFromMember(propAttr, member);
            if (propAttr.Getter) {
              DefineIntrinsicAccessorProperty(ht, name, sharedValue, propAttr.Attributes, true);
            } else if (propAttr.Setter) {
              DefineIntrinsicAccessorProperty(ht, name, sharedValue, propAttr.Attributes, false);
            } else if (member.MemberType != MemberTypes.Method) {
              DefineIntrinsicDataProperty(ht, name, sharedValue, propAttr.Attributes);
            } else {
              DefineIntrinsicMethodProperty(ht, name, sharedValue, propAttr.Attributes);
              if (propAttr.Global) {
                DefineIntrinsicMethodProperty(globals, name, sharedValue, propAttr.Attributes);
              }
            }
          }
        }
      }
    }

    private void DefineIntrinsicDataProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, EcmaValue value, EcmaPropertyAttributes? attributes) {
      ht[name] = new EcmaPropertyDescriptor(value, attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty) & (EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
    }

    private void DefineIntrinsicAccessorProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, EcmaValue sharedValue, EcmaPropertyAttributes? attributes, bool isGetter) {
      attributes = attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty);
      if (name.IsSymbol) {
        attributes &= ~EcmaPropertyAttributes.Enumerable;
      }
      EcmaPropertyDescriptor descriptor;
      if (!ht.TryGetValue(name, out descriptor)) {
        descriptor = new EcmaPropertyDescriptor(attributes.Value);
        ht[name] = descriptor;
      }
      if (isGetter) {
        descriptor.Get = sharedValue;
      } else {
        descriptor.Set = sharedValue;
      }
    }

    private void DefineIntrinsicMethodProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, EcmaValue sharedValue, EcmaPropertyAttributes? attributes) {
      ht[name] = new EcmaPropertyDescriptor(sharedValue, attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultMethodProperty));
    }

    private RuntimeFunction CreateIntrinsicFunction(string name, MethodInfo method, Enum superClass, Enum parentObjectType, EcmaPropertyKey propertyKey) {
      RuntimeFunction fn = parentObjectType is WellKnownObject knownObject ? new IntrinsicFunction(name, method, knownObject, propertyKey) : new NativeRuntimeFunction(name, method, true);
      if (IsValidReference(superClass, out SharedObjectHandle handle)) {
        fn.SetPrototypeOf(this.Realm.GetRuntimeObject(handle));
      }
      return fn;
    }

    private RuntimeObject EnsureObject(int index) {
      if (runtimeObjects[index] == null) {
        runtimeObjects[index] = new EcmaObject();
      }
      return runtimeObjects[index];
    }

    private int ToValidIndex(Enum enumValue) {
      if (enumValue.GetType() != this.EnumType) {
        throw new ArgumentException("Reference enum type must be of the same type", "enumValue");
      }
      int value = Convert.ToInt32(enumValue);
      if (value < 0 || value >= objectCount) {
        throw new ArgumentOutOfRangeException("Invalid enum value", "enumValue");
      }
      return value;
    }

    private static bool IsValidReference(object value, out SharedObjectHandle handle) {
      if (value is Enum p1) {
        handle = RuntimeRealm.GetSharedObjectHandle(p1);
        return true;
      }
      handle = default;
      return false;
    }

    private static void DefineAllProperties(RuntimeObject target, Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> properties) {
      foreach (KeyValuePair<EcmaPropertyKey, EcmaPropertyDescriptor> e in properties) {
        target.DefinePropertyOrThrow(e.Key, e.Value);
      }
    }

    private static EcmaPropertyDescriptor CreateSharedObjectDescriptor(SharedObjectHandle handle, EcmaPropertyAttributes attributes) {
      return new EcmaPropertyDescriptor(handle.ToValue(), attributes);
    }

    private static EcmaPropertyKey GetNameFromMember(IntrinsicMemberAttribute propAttr, MemberInfo member) {
      if (propAttr.Symbol != 0) {
        return new EcmaPropertyKey(propAttr.Symbol);
      }
      return new EcmaPropertyKey(propAttr.Name ?? Regex.Replace(member.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
    }
  }

  public abstract class RuntimeModule<T> : RuntimeModule where T : struct, Enum {
    public override Type EnumType => typeof(T);

    public SharedObjectHandle GetSharedObjectHandle(T value) {
      ThrowIfNotInitialized();
      return new SharedObjectHandle(this.Container.ID, (int)(object)value);
    }
  }
}
