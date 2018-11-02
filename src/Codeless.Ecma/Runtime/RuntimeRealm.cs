using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.Runtime {
  public class RuntimeRealm : IDisposable {
    private static readonly RuntimeObject[] sharedIntrinsics = new RuntimeObject[(int)WellKnownObject.MaxValue];
    private static readonly bool inited;
    [ThreadStatic]
    private static RuntimeRealm current;

    private readonly WeakKeyedCollection dictionary = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private readonly RuntimeRealm previous;
    private bool disposed;

    static RuntimeRealm() {
      using (new RuntimeRealm()) {
        EnsureWellKnownObject(WellKnownObject.ObjectPrototype);
        EnsureWellKnownObject(WellKnownObject.FunctionPrototype);

        List<RuntimeObject> definedObj = DefineIntrinsicObjectsFromAssembly(Assembly.GetExecutingAssembly());
        Array.Resize(ref sharedIntrinsics, sharedIntrinsics.Length + definedObj.Count);
        definedObj.CopyTo(sharedIntrinsics, sharedIntrinsics.Length - definedObj.Count);
        EnsureWellKnownObjectsShared();
      }
      inited = true;
    }

    public RuntimeRealm() {
      this.previous = current;
      current = this;
      if (inited) {
        foreach (RuntimeObject obj in sharedIntrinsics) {
          dictionary.GetOrAdd(obj);
        }
      }
    }

    public bool Disposed {
      get { return disposed; }
    }

    public Hashtable Properties {
      get {
        if (disposed) {
          throw new ObjectDisposedException("");
        }
        return ht;
      }
    }

    public static RuntimeRealm Current {
      get {
        if (current == null) {
          current = new RuntimeRealm();
        }
        return current;
      }
    }

    public static RuntimeRealm GetRealm(RuntimeObject obj) {
      Guard.ArgumentNotNull(obj, "obj");
      return obj.Realm;
    }

    public static RuntimeObject GetRuntimeObject(object target, bool createIfNotExist) {
      if (target == null) {
        return null;
      }
      if (target is EcmaValue) {
        target = ((EcmaValue)target).GetUnderlyingObject();
      }
      if (target.GetType().IsValueType) {
        throw new ArgumentException();
      }
      if (createIfNotExist) {
        RuntimeObject newO = RuntimeObject.CreateForNativeObject(target);
        RuntimeObject cur = Current.dictionary.GetOrAdd(newO);
        return cur;
      }
      return Current.dictionary.TryGet<RuntimeObject>(target);
    }

    public static RuntimeObject GetWellKnownObject(WellKnownObject type) {
      if ((int)type < 1 || (int)type >= (int)WellKnownObject.MaxValue) {
        throw new ArgumentException();
      }
      return sharedIntrinsics[(int)type - 1];
    }

    public void Dispose() {
      if (!disposed) {
        current = previous;
        disposed = true;
      }
    }

    private static WellKnownObject GetPrototypeOf(WellKnownObject type) {
      switch (type) {
        case WellKnownObject.ArrayConstructor:
        case WellKnownObject.BooleanConstructor:
        case WellKnownObject.DateConstructor:
        case WellKnownObject.ErrorConstructor:
        case WellKnownObject.FunctionConstructor:
        case WellKnownObject.Map:
        case WellKnownObject.NumberConstructor:
        case WellKnownObject.ObjectConstructor:
        case WellKnownObject.RegExp:
        case WellKnownObject.Set:
        case WellKnownObject.StringConstructor:
        case WellKnownObject.SymbolConstructor:
        case WellKnownObject.WeakMap:
        case WellKnownObject.WeakSet:
        case WellKnownObject.EvalError:
        case WellKnownObject.RangeError:
        case WellKnownObject.ReferenceError:
        case WellKnownObject.SyntaxError:
        case WellKnownObject.TypeError:
        case WellKnownObject.UriError:
          return (WellKnownObject)((int)type + 1);
        case WellKnownObject.EvalErrorPrototype:
        case WellKnownObject.RangeErrorPrototype:
        case WellKnownObject.ReferenceErrorPrototype:
        case WellKnownObject.SyntaxErrorPrototype:
        case WellKnownObject.UriErrorPrototype:
        case WellKnownObject.TypeErrorPrototype:
          return WellKnownObject.ErrorPrototype;
        case WellKnownObject.ObjectPrototype:
          return 0;
      }
      return WellKnownObject.ObjectPrototype;
    }

    private static RuntimeObject EnsureWellKnownObject(WellKnownObject type) {
      int index = (int)type - 1;
      RuntimeObject obj = sharedIntrinsics[index];
      if (obj == null) {
        if (type == WellKnownObject.ObjectPrototype) {
          obj = RuntimeObject.Create((RuntimeObject)null);
        } else {
          obj = new RuntimeObject(GetPrototypeOf(type));
        }
        sharedIntrinsics[index] = obj;
      }
      return obj;
    }

    private static void EnsureWellKnownObjectsShared() {
      for (int i = 0, length = sharedIntrinsics.Length; i < length; i++) {
        EnsureWellKnownObject((WellKnownObject)(i + 1));
      }
    }

    private static List<RuntimeObject> DefineIntrinsicObjectsFromAssembly(Assembly assembly) {
      List<RuntimeObject> definedObj = new List<RuntimeObject>();
      foreach (Type t in assembly.GetTypes()) {
        IntrinsicObjectAttribute typeAttr;
        if (t.HasAttribute(out typeAttr)) {
          WellKnownObject typeProto = GetPrototypeOf(typeAttr.ObjectType);
          if (typeAttr.Global) {
            RuntimeObject global = EnsureWellKnownObject(WellKnownObject.Global);
            global.DefineOwnProperty(typeAttr.Name, new EcmaPropertyDescriptor(new EcmaValue(EnsureWellKnownObject(typeAttr.ObjectType)), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
          }
          foreach (MemberInfo member in t.GetMembers()) {
            IntrinsicConstructorAttribute ctorAttr;
            if (member.HasAttribute(out ctorAttr)) {
              DefineIntrinsicConstructor(typeAttr.ObjectType, member.Name, (MethodInfo)member, GetPrototypeOf(typeAttr.ObjectType));
              continue;
            }
            IntrinsicMemberAttribute propAttr;
            if (member.HasAttribute(out propAttr)) {
              EcmaPropertyKey name;
              if (propAttr.Symbol != 0) {
                name = new EcmaPropertyKey(propAttr.Symbol);
              } else {
                name = new EcmaPropertyKey(propAttr.Name ?? Regex.Replace(member.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
              }
              switch (member.MemberType) {
                case MemberTypes.Method:
                  RuntimeObject fn;
                  if (propAttr.Getter) {
                    fn = DefineIntrinsicAccessorProperty(typeAttr.ObjectType, name, (MethodInfo)member, propAttr.Attributes, true);
                  } else if (propAttr.Setter) {
                    fn = DefineIntrinsicAccessorProperty(typeAttr.ObjectType, name, (MethodInfo)member, propAttr.Attributes, false);
                  } else if (((MethodInfo)member).IsStatic) {
                    fn = DefineIntrinsicFunction(typeAttr.ObjectType, name, (MethodInfo)member);
                  } else {
                    fn = DefineIntrinsicFunction(typeProto, name, (MethodInfo)member);
                  }
                  definedObj.Add(fn);
                  break;
                case MemberTypes.Property:
                  DefineIntrinsicDataProperty(typeAttr.ObjectType, name, new EcmaValue(((PropertyInfo)member).GetValue(null, null)), propAttr.Attributes);
                  break;
                case MemberTypes.Field:
                  DefineIntrinsicDataProperty(typeAttr.ObjectType, name, new EcmaValue(((FieldInfo)member).GetValue(null)), propAttr.Attributes);
                  break;
              }
            }
          }
        }
      }
      return definedObj;
    }

    private static void DefineIntrinsicDataProperty(WellKnownObject type, EcmaPropertyKey name, EcmaValue value, EcmaPropertyAttributes state) {
      RuntimeObject obj = EnsureWellKnownObject(type);
      if (name.IsSymbol) {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(value, state & EcmaPropertyAttributes.Configurable));
      } else if (type != WellKnownObject.Global) {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(value, state & (EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable)));
      } else {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.None));
      }
    }

    private static RuntimeObject DefineIntrinsicAccessorProperty(WellKnownObject type, EcmaPropertyKey name, MethodInfo method, EcmaPropertyAttributes state, bool isGetter) {
      string runtimeName = (isGetter ? "get " : "set ") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
      RuntimeObject fn = new NativeRuntimeFunction(runtimeName, method);
      RuntimeObject obj = EnsureWellKnownObject(type);
      if (name.IsSymbol) {
        state &= ~EcmaPropertyAttributes.Enumerable;
      }
      if (isGetter) {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(fn, EcmaValue.Undefined, state));
      } else {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(EcmaValue.Undefined, fn, state));
      }
      return fn;
    }

    private static RuntimeObject DefineIntrinsicFunction(WellKnownObject type, EcmaPropertyKey name, MethodInfo method) {
      string runtimeName = name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name;
      RuntimeObject fn = new NativeRuntimeFunction(runtimeName, method);
      RuntimeObject obj = EnsureWellKnownObject(type);
      if (name.IsSymbol) {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(new EcmaValue(fn), EcmaPropertyAttributes.Configurable));
      } else {
        obj.CreateMethodProperty(name, new EcmaValue(fn));
      }
      return fn;
    }

    private static RuntimeObject DefineIntrinsicConstructor(WellKnownObject ctorType, EcmaPropertyKey name, MethodInfo mi, WellKnownObject fnProto) {
      RuntimeObject ctor = DefineIntrinsicFunction(WellKnownObject.Global, name, mi);

      RuntimeObject proto = EnsureWellKnownObject(fnProto);
      proto.DefineOwnProperty("constructor", new EcmaPropertyDescriptor(new EcmaValue(ctor), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
      ctor.DefineOwnProperty("prototype", new EcmaPropertyDescriptor(new EcmaValue(proto), EcmaPropertyAttributes.None));

      RuntimeObject cur = EnsureWellKnownObject(ctorType);
      foreach (EcmaPropertyKey key in cur.OwnPropertyKeys) {
        ctor.DefineOwnProperty(key, cur.GetOwnProperty(key));
      }
      sharedIntrinsics[(int)ctorType - 1] = ctor;
      return ctor;
    }
  }
}
