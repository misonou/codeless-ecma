using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [DebuggerDisplay("RuntimeRealm ID={ID}")]
  public class RuntimeRealm : IDisposable {
    private static readonly int sharedIntrinsicLength = (int)WellKnownObject.MaxValue;
    private static readonly RuntimeObject[] sharedIntrinsics = new RuntimeObject[sharedIntrinsicLength];
    private static readonly RuntimeRealm sharedRealm = new RuntimeRealm();
    private static readonly bool inited;

    private static RuntimeObject[][] sharedObjects = { sharedIntrinsics };
    private static int accum;
    [ThreadStatic]
    private static RuntimeRealm current;
    [ThreadStatic]
    private static bool useSharedRealm;

    private readonly Dictionary<RuntimeObject, long> objectIndex = new Dictionary<RuntimeObject, long>();
    private readonly WeakKeyedCollection nativeWrappers = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private RuntimeObject[][] intrinsics = { };

    static RuntimeRealm() {
      current = sharedRealm;
      EnsureWellKnownObject(WellKnownObject.ObjectPrototype);
      EnsureWellKnownObject(WellKnownObject.FunctionPrototype);
      using (SharedObjectContainer container = new SharedObjectContainer()) {
        DefineIntrinsicObjectsFromAssembly(container, Assembly.GetExecutingAssembly());
      }
      for (int i = 1; i < sharedIntrinsicLength; i++) {
        current.objectIndex.Add(sharedIntrinsics[i], i);
      }
      current.intrinsics = sharedObjects;
      current = null;
      inited = true;
    }

    public RuntimeRealm() {
      if (inited) {
        this.ID = Interlocked.Increment(ref accum);
        this.ExecutionContext = RuntimeExecution.Current;
      }
    }

    public event EventHandler BeforeDisposed = delegate { };

    public int ID { get; }

    public RuntimeExecution ExecutionContext { get; }

    public bool Disposed { get; private set; }

    public RuntimeObject Global {
      get { return GetRuntimeObject(WellKnownObject.Global); }
    }

    public Hashtable Properties {
      get {
        if (this.Disposed) {
          throw new ObjectDisposedException("");
        }
        return ht;
      }
    }

    public static RuntimeRealm Current {
      get {
        if (useSharedRealm) {
          return sharedRealm;
        }
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        if (invocation != null) {
          return invocation.FunctionObject.Realm;
        }
        if (current == null) {
          current = RuntimeExecution.Current.DefaultRealm;
        }
        return current;
      }
    }

    internal static RuntimeRealm SharedRealm {
      get { return sharedRealm; }
    }

    public static RuntimeRealm GetRealm(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        return obj.ToObject().Realm;
      }
      return RuntimeRealm.Current;
    }

    public RuntimeObject GetRuntimeObject(object target) {
      if (target == null) {
        return null;
      }
      if (target is EcmaValue value) {
        target = value.GetUnderlyingObject();
      }
      if (target is RuntimeObject runtimeObject) {
        if (runtimeObject.Realm == this) {
          return runtimeObject;
        }
        if (TryGetSharedObjectInRealm(runtimeObject, out RuntimeObject sharedObject)) {
          return sharedObject;
        }
        return runtimeObject.Clone(this);
      }
      return nativeWrappers.GetOrAdd(NativeObject.Create(target));
    }

    public RuntimeObject GetRuntimeObject(WellKnownObject type) {
      if (!inited) {
        return null;
      }
      return GetSharedObject((long)type);
    }

    public RuntimeObject GetSharedObjectInRealm(RuntimeObject sourceObj) {
      if (TryGetSharedObjectInRealm(sourceObj, out RuntimeObject sharedObject)) {
        return sharedObject;
      }
      throw new ArgumentException("Object is not shared", "souceObj");
    }

    internal bool TryGetSharedObjectInRealm(RuntimeObject sourceObj, out RuntimeObject runtimeObject) {
      Guard.ArgumentNotNull(sourceObj, "sourceObj");
      if (sourceObj.Realm.objectIndex.TryGetValue(sourceObj, out long index)) {
        runtimeObject = GetSharedObject(index);
        return true;
      }
      runtimeObject = null;
      return false;
    }

    public void Execute(Action action) {
      if (this == sharedRealm) {
        throw new InvalidOperationException("Execution in shared realm is disallowed");
      }
      if (Thread.CurrentThread != this.ExecutionContext.Thread) {
        throw new InvalidOperationException("Execution in another thread is disallowed");
      }
      RuntimeRealm previous = current;
      try {
        current = this;
        action();
      } finally {
        current = previous;
      }
    }

    public void Enqueue(Action action) {
      this.ExecutionContext.Enqueue(this, action, 0, RuntimeExecutionFlags.None);
    }

    public void Enqueue(Action action, Task task) {
      this.ExecutionContext.Enqueue(this, action, task);
    }

    public RuntimeExecutionHandle Enqueue(Action action, int milliseconds, RuntimeExecutionFlags flags) {
      return this.ExecutionContext.Enqueue(this, action, milliseconds, flags);
    }

    public void Dispose() {
      if (!this.Disposed) {
        try {
          BeforeDisposed(this, EventArgs.Empty);
        } finally {
          if (current == this) {
            current = null;
          }
          this.Disposed = true;
        }
      }
    }

    private RuntimeObject GetSharedObject(long index) {
      int i = (int)(index >> 32);
      int j = (int)(index & 0xFFFFFFFF);
      if (i >= sharedObjects.Length) {
        throw new ArgumentOutOfRangeException("index");
      }
      if (i >= intrinsics.Length && intrinsics.Length != sharedObjects.Length) {
        Array.Resize(ref intrinsics, sharedObjects.Length);
      }
      RuntimeObject[] arr = intrinsics[i];
      if (arr == null) {
        arr = new RuntimeObject[sharedObjects[i].Length];
        intrinsics[i] = arr;
      }
      if (j >= arr.Length) {
        throw new ArgumentOutOfRangeException("index");
      }
      RuntimeObject obj = arr[j];
      if (obj == null) {
        obj = sharedObjects[i][j].CloneSlim(this);
        objectIndex.Add(obj, index);
        arr[j] = obj;
      }
      return obj;
    }

    private static RuntimeObject EnsureWellKnownObject(WellKnownObject type) {
      int index = (int)type;
      RuntimeObject obj = sharedIntrinsics[index];
      if (obj == null) {
        switch (type) {
          case WellKnownObject.ObjectPrototype:
            obj = RuntimeObject.Create(null);
            break;
          case WellKnownObject.NumberPrototype:
            obj = new PrimitiveObject(0, WellKnownObject.ObjectPrototype);
            obj.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.ObjectPrototype));
            break;
          case WellKnownObject.StringPrototype:
            obj = new PrimitiveObject("", WellKnownObject.ObjectPrototype);
            obj.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.ObjectPrototype));
            break;
          case WellKnownObject.BooleanPrototype:
            obj = new PrimitiveObject(false, WellKnownObject.ObjectPrototype);
            obj.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.ObjectPrototype));
            break;
          case WellKnownObject.ArrayPrototype:
            obj = new EcmaArray();
            obj.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.ObjectPrototype));
            break;
          default:
            obj = new RuntimeObject(WellKnownObject.ObjectPrototype);
            obj.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.ObjectPrototype));
            break;
        }
        sharedIntrinsics[index] = obj;
      }
      return obj;
    }

    private static void DefineIntrinsicObjectsFromAssembly(SharedObjectContainer container, Assembly assembly) {
      // properties are first stored in dictionary while in the foreach loop
      // since EnsureWellKnownObject will return different objects during the loop
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[] properties = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[sharedIntrinsicLength];
      for (int i = 1; i < sharedIntrinsicLength; i++) {
        properties[i] = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
      }
      DefineIntrinsicObjectFromType(container, typeof(FunctionConstructor), properties);
      foreach (Type t in assembly.GetTypes()) {
        if (t != typeof(FunctionConstructor)) {
          DefineIntrinsicObjectFromType(container, t, properties);
        }
      }
      for (int i = 1; i < sharedIntrinsicLength; i++) {
        RuntimeObject obj = EnsureWellKnownObject((WellKnownObject)i);
        foreach (KeyValuePair<EcmaPropertyKey, EcmaPropertyDescriptor> e in properties[i]) {
          obj.DefinePropertyOrThrow(e.Key, e.Value);
        }
      }
    }

    private static void DefineIntrinsicObjectFromType(SharedObjectContainer container, Type type, Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[] properties) {
      if (type.HasAttribute(out IntrinsicObjectAttribute typeAttr)) {
        DefineIntrinsicObjectFromType(container, type, typeAttr, properties);
      }
    }

    private static void DefineIntrinsicObjectFromType(SharedObjectContainer container, Type type, IntrinsicObjectAttribute typeAttr, Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[] properties) {
      int objectIndex = (int)typeAttr.ObjectType;
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht = properties[objectIndex];
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> globals = properties[(int)WellKnownObject.Global];

      if (typeAttr.Prototype != 0) {
        EnsureWellKnownObject(typeAttr.ObjectType).SetPrototypeOf(EnsureWellKnownObject(typeAttr.Prototype));
      }
      if (typeAttr.Global) {
        globals[typeAttr.Name ?? type.Name] = CreateWellknownObjectSharedDescriptor(typeAttr.ObjectType, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
      }
      foreach (MemberInfo member in type.GetMembers()) {
        // special handling if the type defines an instrinsic contructor
        // replace the default object created from EnsureWellKnownObject to InstrincFunction
        if (member.HasAttribute(out IntrinsicConstructorAttribute ctorAttr)) {
          string ctorName = ctorAttr.Name ?? member.Name;
          int protoIndex = (int)ctorAttr.Prototype;
          sharedIntrinsics[objectIndex] = CreateIntrinsicFunction(ctorName, member, WellKnownObject.Global, ctorName, ctorAttr.SuperClass);
          if (protoIndex != 0) {
            ht[WellKnownProperty.Prototype] = CreateWellknownObjectSharedDescriptor(ctorAttr.Prototype, EcmaPropertyAttributes.None);
            properties[protoIndex][WellKnownProperty.Constructor] = CreateWellknownObjectSharedDescriptor(typeAttr.ObjectType, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
          }
          if (ctorAttr.Global) {
            globals[ctorName] = CreateWellknownObjectSharedDescriptor(typeAttr.ObjectType, EcmaPropertyAttributes.DefaultMethodProperty);
          }
          continue;
        }

        object[] propAttrs = member.GetCustomAttributes(typeof(IntrinsicMemberAttribute), false);
        if (propAttrs.Length > 0) {
          EcmaValue sharedValue = default;
          if (member.HasAttribute(out AliasOfAttribute aliasOf)) {
            EcmaPropertyKey aliasOfKey = aliasOf.Name != null ? (EcmaPropertyKey)aliasOf.Name : aliasOf.Symbol;
            if (!properties[(int)aliasOf.ObjectType].TryGetValue(aliasOfKey, out EcmaPropertyDescriptor descriptor)) {
              // for sake of simplicity the aliased target should be defined on intrinsic object with smaller WellKnownObject enum value
              // to avoid the need of topological sort
              throw new InvalidOperationException();
            }
            sharedValue = descriptor.Value;
          }
          if (member.MemberType == MemberTypes.Method && sharedValue == default) {
            IntrinsicMemberAttribute propAttr = (IntrinsicMemberAttribute)propAttrs[0];
            EcmaPropertyKey name = GetNameFromMember(propAttr, member);
            string runtimeName = (propAttr.Getter ? "get " : propAttr.Setter ? "set " : "") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
            sharedValue = container.Add(CreateIntrinsicFunction(runtimeName, member, typeAttr.ObjectType, name, null)).ToValue();
          }
          foreach (IntrinsicMemberAttribute propAttr in propAttrs) {
            EcmaPropertyKey name = GetNameFromMember(propAttr, member);
            switch (member.MemberType) {
              case MemberTypes.Method:
                if (propAttr.Getter) {
                  DefineIntrinsicAccessorProperty(ht, name, sharedValue, propAttr.Attributes, true);
                } else if (propAttr.Setter) {
                  DefineIntrinsicAccessorProperty(ht, name, sharedValue, propAttr.Attributes, false);
                } else if (((MethodInfo)member).IsStatic) {
                  DefineIntrinsicMethodProperty(ht, name, sharedValue, propAttr.Attributes);
                  if (propAttr.Global) {
                    DefineIntrinsicMethodProperty(globals, name, sharedValue, propAttr.Attributes);
                  }
                }
                break;
              case MemberTypes.Property:
                DefineIntrinsicDataProperty(ht, name, ((PropertyInfo)member).GetValue(null, null), propAttr.Attributes);
                break;
              case MemberTypes.Field:
                DefineIntrinsicDataProperty(ht, name, ((FieldInfo)member).GetValue(null), propAttr.Attributes);
                break;
            }
          }
        }
      }
    }

    private static void DefineIntrinsicDataProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, object value, EcmaPropertyAttributes? attributes) {
      attributes = attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty) & (EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
      ht[name] = value is WellKnownObject type ? CreateWellknownObjectSharedDescriptor(type, attributes.Value) : new EcmaPropertyDescriptor(new EcmaValue(value), attributes.Value);
    }

    private static void DefineIntrinsicAccessorProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, EcmaValue sharedValue, EcmaPropertyAttributes? attributes, bool isGetter) {
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

    private static void DefineIntrinsicMethodProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, EcmaValue sharedValue, EcmaPropertyAttributes? attributes) {
      ht[name] = new EcmaPropertyDescriptor(sharedValue, attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultMethodProperty));
    }

    private static IntrinsicFunction CreateIntrinsicFunction(string runtimeName, MemberInfo member, WellKnownObject objectType, EcmaPropertyKey name, WellKnownObject? superClass) {
      IntrinsicFunction fn = new IntrinsicFunction(runtimeName, (MethodInfo)member, objectType, name);
      fn.SetPrototypeOf(EnsureWellKnownObject(superClass.GetValueOrDefault(WellKnownObject.FunctionPrototype)));
      return fn;
    }

    private static EcmaPropertyDescriptor CreateWellknownObjectSharedDescriptor(WellKnownObject type, EcmaPropertyAttributes attributes) {
      return new EcmaPropertyDescriptor(new SharedObjectHandle((long)type).ToValue(), attributes);
    }

    private static EcmaPropertyKey GetNameFromMember(IntrinsicMemberAttribute propAttr, MemberInfo member) {
      if (propAttr.Symbol != 0) {
        return new EcmaPropertyKey(propAttr.Symbol);
      }
      return new EcmaPropertyKey(propAttr.Name ?? Regex.Replace(member.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
    }

    internal struct SharedObjectHandle {
      private EcmaValue sharedValue;

      internal SharedObjectHandle(long value) {
        sharedValue = new EcmaValue(new EcmaValueHandle(value), SharedIntrinsicObjectBinder.Default);
      }

      public EcmaValue ToValue() {
        return sharedValue;
      }

      public RuntimeObject GetRuntimeObject(RuntimeRealm realm) {
        return realm.GetSharedObject(sharedValue.ToInt64());
      }

      public static SharedObjectHandle FromValue(EcmaValue sharedValue) {
        if (sharedValue.Type != SharedIntrinsicObjectBinder.SharedValue) {
          throw new ArgumentException("Value must be created from SharedObjectHandle.ToValue()", "sharedValue");
        }
        SharedObjectHandle handle = default;
        handle.sharedValue = sharedValue;
        return handle;
      }
    }

    internal class SharedObjectContainer : IDisposable {
      private readonly List<RuntimeObject> definedObj = new List<RuntimeObject>();
      private readonly long prefix = sharedObjects.LongLength << 32;
      private readonly object syncRoot = sharedRealm;

      public SharedObjectContainer() {
        Monitor.Enter(syncRoot);
        useSharedRealm = true;
      }

      public SharedObjectHandle Add(RuntimeObject obj) {
        Guard.ArgumentNotNull(obj, "obj");
        if (obj.Realm != RuntimeRealm.SharedRealm) {
          throw new ArgumentException();
        }
        long sharedIndex = definedObj.IndexOf(obj);
        if (sharedIndex < 0) {
          sharedIndex = definedObj.Count;
          definedObj.Add(obj);
        }
        return new SharedObjectHandle(prefix | sharedIndex);
      }

      public void Dispose() {
        try {
          int index = sharedObjects.Length;
          Array.Resize(ref sharedObjects, index + 1);
          sharedObjects[index] = definedObj.ToArray();
          sharedRealm.intrinsics = sharedObjects;
          for (int i = 0, len = definedObj.Count; i < len; i++) {
            sharedRealm.objectIndex.Add(definedObj[i], prefix | i);
          }
        } finally {
          useSharedRealm = false;
          Monitor.Exit(syncRoot);
        }
      }
    }
  }
}
