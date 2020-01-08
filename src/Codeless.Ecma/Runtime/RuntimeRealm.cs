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
  public class RuntimeRealm : IDisposable {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly RuntimeObject[] sharedIntrinsics = new RuntimeObject[(int)WellKnownObject.MaxValue];
    private static readonly bool inited;
    private static int accum;
    [ThreadStatic]
    private static RuntimeRealm current;

    private readonly Dictionary<RuntimeObject, int> objectIndex = new Dictionary<RuntimeObject, int>();
    private readonly WeakKeyedCollection nativeWrappers = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private RuntimeObject[] intrinsics = new RuntimeObject[sharedIntrinsics.Length];
    private bool disposed;

    static RuntimeRealm() {
      using (current = new RuntimeRealm()) {
        EnsureWellKnownObject(WellKnownObject.ObjectPrototype);
        EnsureWellKnownObject(WellKnownObject.FunctionPrototype);
        DefineIntrinsicObjectsFromAssembly(ref sharedIntrinsics, Assembly.GetExecutingAssembly());
        for (int i = 1; i < sharedIntrinsics.Length; i++) {
          current.objectIndex.Add(sharedIntrinsics[i], i);
        }
        current.intrinsics = sharedIntrinsics;
      }
      inited = true;
    }

    public RuntimeRealm() {
      if (inited) {
        this.ExecutionContext = RuntimeExecution.Current;
      }
    }

    public event EventHandler BeforeDisposed = delegate { };

    public int ID { get; } = Interlocked.Increment(ref accum);

    public RuntimeExecution ExecutionContext { get; }

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
      get { return sharedIntrinsics[1].Realm; }
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
        if (runtimeObject.Realm.objectIndex.TryGetValue(runtimeObject, out int index)) {
          return GetRuntimeObject((WellKnownObject)index);
        }
        return runtimeObject;
      }
      return nativeWrappers.GetOrAdd(NativeObject.Create(target));
    }

    public RuntimeObject GetRuntimeObject(WellKnownObject type) {
      if (!inited) {
        return null;
      }
      int index = (int)type;
      if (index <= 0 || index > sharedIntrinsics.Length) {
        throw new ArgumentOutOfRangeException("type");
      }
      RuntimeObject obj = intrinsics[index];
      if (obj == null) {
        if (!inited) {
          EnsureWellKnownObject(type);
        }
        obj = sharedIntrinsics[index].Clone(this);
        objectIndex.Add(obj, index);
        intrinsics[index] = obj;
      }
      return obj;
    }

    public void Execute(Action action) {
      RuntimeRealm previous = current;
      try {
        current = this;
        action();
      } finally {
        current = previous;
      }
    }

    public void Dispose() {
      if (!disposed) {
        try {
          BeforeDisposed(this, EventArgs.Empty);
        } finally {
          if (current == this) {
            current = null;
          }
          disposed = true;
        }
      }
    }

    public static WellKnownObject GetPrototypeOf(WellKnownObject type) {
      switch (type) {
        case WellKnownObject.ArrayConstructor:
        case WellKnownObject.BooleanConstructor:
        case WellKnownObject.DateConstructor:
        case WellKnownObject.ErrorConstructor:
        case WellKnownObject.FunctionConstructor:
        case WellKnownObject.MapConstructor:
        case WellKnownObject.NumberConstructor:
        case WellKnownObject.ObjectConstructor:
        case WellKnownObject.RegExpConstructor:
        case WellKnownObject.SetConstructor:
        case WellKnownObject.StringConstructor:
        case WellKnownObject.SymbolConstructor:
        case WellKnownObject.WeakMapConstructor:
        case WellKnownObject.WeakSetConstructor:
        case WellKnownObject.EvalError:
        case WellKnownObject.RangeError:
        case WellKnownObject.ReferenceError:
        case WellKnownObject.SyntaxError:
        case WellKnownObject.TypeError:
        case WellKnownObject.UriError:
        case WellKnownObject.PromiseConstructor:
        case WellKnownObject.Uint8Array:
        case WellKnownObject.Uint8ClampedArray:
        case WellKnownObject.Uint16Array:
        case WellKnownObject.Uint32Array:
        case WellKnownObject.DataView:
        case WellKnownObject.SharedArrayBuffer:
        case WellKnownObject.ArrayBuffer:
        case WellKnownObject.TypedArray:
        case WellKnownObject.Float32Array:
        case WellKnownObject.Float64Array:
        case WellKnownObject.Int8Array:
        case WellKnownObject.Int16Array:
        case WellKnownObject.Int32Array:
          return (WellKnownObject)((int)type + 1);
        case WellKnownObject.Float32ArrayPrototype:
        case WellKnownObject.Float64ArrayPrototype:
        case WellKnownObject.Uint8ArrayPrototype:
        case WellKnownObject.Uint8ClampedArrayPrototype:
        case WellKnownObject.Uint16ArrayPrototype:
        case WellKnownObject.Uint32ArrayPrototype:
        case WellKnownObject.Int8ArrayPrototype:
        case WellKnownObject.Int16ArrayPrototype:
        case WellKnownObject.Int32ArrayPrototype:
          return WellKnownObject.TypedArrayPrototype;
        case WellKnownObject.EvalErrorPrototype:
        case WellKnownObject.RangeErrorPrototype:
        case WellKnownObject.ReferenceErrorPrototype:
        case WellKnownObject.SyntaxErrorPrototype:
        case WellKnownObject.UriErrorPrototype:
        case WellKnownObject.TypeErrorPrototype:
          return WellKnownObject.ErrorPrototype;
        case WellKnownObject.ArrayIteratorPrototype:
        case WellKnownObject.MapIteratorPrototype:
        case WellKnownObject.SetIteratorPrototype:
        case WellKnownObject.StringIteratorPrototype:
        case WellKnownObject.RegExpStringIteratorPrototype:
          return WellKnownObject.IteratorPrototype;
        case WellKnownObject.ObjectPrototype:
          return 0;
      }
      return WellKnownObject.ObjectPrototype;
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
            obj = new RuntimeObject(GetPrototypeOf(type));
            obj.SetPrototypeOf(EnsureWellKnownObject(GetPrototypeOf(type)));
            break;
        }
        sharedIntrinsics[index] = obj;
      }
      return obj;
    }

    private static void DefineIntrinsicObjectsFromAssembly(ref RuntimeObject[] sharedIntrinsics, Assembly assembly) {
      List<RuntimeObject> definedObj = new List<RuntimeObject>();
      int objectIndex = sharedIntrinsics.Length;

      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[] properties = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>[sharedIntrinsics.Length];
      for (int i = 1; i < sharedIntrinsics.Length; i++) {
        properties[i] = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
      }
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> globals = properties[(int)WellKnownObject.Global];

      foreach (Type t in assembly.GetTypes()) {
        if (t.HasAttribute(out IntrinsicObjectAttribute typeAttr)) {
          int objectType = (int)typeAttr.ObjectType;
          int objectProto = (int)GetPrototypeOf(typeAttr.ObjectType);
          Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht = properties[objectType];
          Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> hp = properties[objectProto];

          if (typeAttr.Global) {
            globals[typeAttr.Name ?? t.Name] = CreateSharedDescriptor(objectType, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
          }
          foreach (MemberInfo member in t.GetMembers()) {
            if (member.HasAttribute(out IntrinsicConstructorAttribute ctorAttr)) {
              string ctorName = ctorAttr.Name ?? member.Name;
              sharedIntrinsics[objectType] = CreateIntrinsicFunction(ctorName, member, WellKnownObject.Global, ctorName, ctorAttr.SuperClass);
              if (objectProto != (int)WellKnownObject.ObjectPrototype || objectType == (int)WellKnownObject.ObjectConstructor) {
                hp[WellKnownProperty.Constructor] = CreateSharedDescriptor(objectType, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
                ht[WellKnownProperty.Prototype] = CreateSharedDescriptor(objectProto, EcmaPropertyAttributes.None);
              }
              if (ctorAttr.Global) {
                DefineIntrinsicFunction(globals, ctorName, objectType, EcmaPropertyAttributes.DefaultMethodProperty);
              }
            } else {
              object[] propAttrs = member.GetCustomAttributes(typeof(IntrinsicMemberAttribute), false);
              if (propAttrs.Length > 0) {
                int thisIndex = objectIndex;
                if (member.HasAttribute(out AliasOfAttribute aliasOf)) {
                  EcmaPropertyKey aliasOfKey = aliasOf.Name != null ? (EcmaPropertyKey)aliasOf.Name : aliasOf.Symbol;
                  if (!properties[(int)aliasOf.ObjectType].TryGetValue(aliasOfKey, out EcmaPropertyDescriptor descriptor)) {
                    throw new InvalidOperationException();
                  }
                  thisIndex = descriptor.Value.ToInt32();
                }
                foreach (IntrinsicMemberAttribute propAttr in propAttrs) {
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  switch (member.MemberType) {
                    case MemberTypes.Method:
                      if (propAttr.Getter) {
                        DefineIntrinsicAccessorProperty(ht, name, thisIndex, propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty), true);
                      } else if (propAttr.Setter) {
                        DefineIntrinsicAccessorProperty(ht, name, thisIndex, propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty), false);
                      } else if (((MethodInfo)member).IsStatic) {
                        DefineIntrinsicFunction(ht, name, thisIndex, propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultMethodProperty));
                        if (propAttr.Global) {
                          DefineIntrinsicFunction(globals, name, thisIndex, propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultMethodProperty));
                        }
                      } else {
                        DefineIntrinsicFunction(hp, name, thisIndex, propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultMethodProperty));
                      }
                      break;
                    case MemberTypes.Property:
                      DefineIntrinsicDataProperty(ht, name, ((PropertyInfo)member).GetValue(null, null), propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty));
                      break;
                    case MemberTypes.Field:
                      DefineIntrinsicDataProperty(ht, name, ((FieldInfo)member).GetValue(null), propAttr.Attributes.GetValueOrDefault(EcmaPropertyAttributes.DefaultDataProperty));
                      break;
                  }
                }
                if (member.MemberType == MemberTypes.Method && thisIndex == objectIndex) {
                  IntrinsicMemberAttribute propAttr = (IntrinsicMemberAttribute)propAttrs[0];
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  string runtimeName = (propAttr.Getter ? "get " : propAttr.Setter ? "set " : "") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
                  definedObj.Add(CreateIntrinsicFunction(runtimeName, member, (WellKnownObject)objectType, name, null));
                  objectIndex++;
                }
              }
            }
          }
        }
      }

      for (int i = 1; i < sharedIntrinsics.Length; i++) {
        RuntimeObject obj = EnsureWellKnownObject((WellKnownObject)i);
        foreach (KeyValuePair<EcmaPropertyKey, EcmaPropertyDescriptor> e in properties[i]) {
          obj.DefinePropertyOrThrow(e.Key, e.Value);
        }
      }
      Array.Resize(ref sharedIntrinsics, sharedIntrinsics.Length + definedObj.Count);
      definedObj.CopyTo(sharedIntrinsics, sharedIntrinsics.Length - definedObj.Count);
    }

    private static void DefineIntrinsicDataProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, object value, EcmaPropertyAttributes attributes) {
      ht[name] = new EcmaPropertyDescriptor(new EcmaValue(value), attributes & (EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
    }

    private static void DefineIntrinsicAccessorProperty(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, int sharedIndex, EcmaPropertyAttributes attributes, bool isGetter) {
      if (name.IsSymbol) {
        attributes &= ~EcmaPropertyAttributes.Enumerable;
      }
      EcmaPropertyDescriptor descriptor;
      if (!ht.TryGetValue(name, out descriptor)) {
        descriptor = new EcmaPropertyDescriptor(attributes);
        ht[name] = descriptor;
      }
      if (isGetter) {
        descriptor.Get = CreateSharedValue(sharedIndex);
      } else {
        descriptor.Set = CreateSharedValue(sharedIndex);
      }
    }

    private static void DefineIntrinsicFunction(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, int sharedIndex, EcmaPropertyAttributes attributes) {
      ht[name] = CreateSharedDescriptor(sharedIndex, attributes);
    }

    private static IntrinsicFunction CreateIntrinsicFunction(string runtimeName, MemberInfo member, WellKnownObject objectType, EcmaPropertyKey name, WellKnownObject? superClass) {
      IntrinsicFunction fn = new IntrinsicFunction(runtimeName, (MethodInfo)member, objectType, name);
      fn.SetPrototypeOf(EnsureWellKnownObject(superClass.GetValueOrDefault(WellKnownObject.FunctionPrototype)));
      return fn;
    }

    private static EcmaValue CreateSharedValue(int sharedIndex) {
      return new EcmaValue(new EcmaValueHandle(sharedIndex), SharedIntrinsicObjectBinder.Default);
    }

    private static EcmaPropertyDescriptor CreateSharedDescriptor(int sharedIndex, EcmaPropertyAttributes attributes) {
      return new EcmaPropertyDescriptor(CreateSharedValue(sharedIndex), attributes);
    }

    private static EcmaPropertyKey GetNameFromMember(IntrinsicMemberAttribute propAttr, MemberInfo member) {
      if (propAttr.Symbol != 0) {
        return new EcmaPropertyKey(propAttr.Symbol);
      }
      return new EcmaPropertyKey(propAttr.Name ?? Regex.Replace(member.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
    }
  }
}
