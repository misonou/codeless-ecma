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

namespace Codeless.Ecma.Runtime {
  public class RuntimeExceptionEventArgs : EventArgs {
    public RuntimeExceptionEventArgs(Exception ex) {
      this.Exception = ex;
    }

    public Exception Exception { get; }
  }

  public class RuntimeRealm : IDisposable {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly RuntimeObject[] sharedIntrinsics = new RuntimeObject[(int)WellKnownObject.MaxValue];
    private static readonly bool inited;
    [ThreadStatic]
    private static RuntimeRealm current;

    private readonly Dictionary<RuntimeObject, int> objectIndex = new Dictionary<RuntimeObject, int>();
    private readonly WeakKeyedCollection nativeWrappers = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private readonly List<Action> queuedJobs = new List<Action>();
    private readonly int threadId = Thread.CurrentThread.ManagedThreadId;
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

    public EventHandler<RuntimeExceptionEventArgs> ExceptionThrown = delegate { };

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
          current = new RuntimeRealm();
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

    public void Enqueue(Action action) {
      if (disposed) {
        throw new InvalidOperationException("Realm has been disposed");
      }
      queuedJobs.Add(action);
    }

    public void RunQueuedJobs() {
      if (Thread.CurrentThread.ManagedThreadId != threadId) {
        throw new InvalidOperationException("Event loop must be executed in the same thread");
      }
      if (disposed) {
        throw new InvalidOperationException("Realm has been disposed");
      }
      RuntimeRealm before = current;
      current = this;
      try {
        List<Action> actions = new List<Action>(queuedJobs);
        queuedJobs.Clear();
        foreach (Action action in actions) {
          if (!disposed) {
            try {
              action();
            } catch (Exception ex) {
              ExceptionThrown.Invoke(this, new RuntimeExceptionEventArgs(ex));
            }
          }
        }
      } finally {
        current = before;
      }
    }

    public void Dispose() {
      if (!disposed) {
        if (current == this) {
          current = null;
        }
        disposed = true;
        queuedJobs.Clear();
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
          return (WellKnownObject)((int)type + 1);
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
              sharedIntrinsics[objectType] = CreateIntrinsicFunction(ctorName, member, WellKnownObject.Global, ctorName);
              if (objectProto != (int)WellKnownObject.ObjectPrototype || objectType == (int)WellKnownObject.ObjectConstructor) {
                hp[WellKnownProperty.Constructor] = CreateSharedDescriptor(objectType, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
                ht[WellKnownProperty.Prototype] = CreateSharedDescriptor(objectProto, EcmaPropertyAttributes.None);
              }
              DefineIntrinsicFunction(globals, ctorName, objectType);
            } else {
              object[] propAttrs = member.GetCustomAttributes(typeof(IntrinsicMemberAttribute), false);
              if (propAttrs.Length > 0) {
                foreach (IntrinsicMemberAttribute propAttr in propAttrs) {
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  switch (member.MemberType) {
                    case MemberTypes.Method:
                      if (propAttr.Getter) {
                        DefineIntrinsicAccessorProperty(ht, name, objectIndex, propAttr.Attributes, true);
                      } else if (propAttr.Setter) {
                        DefineIntrinsicAccessorProperty(ht, name, objectIndex, propAttr.Attributes, false);
                      } else if (((MethodInfo)member).IsStatic) {
                        DefineIntrinsicFunction(ht, name, objectIndex);
                        if (propAttr.Global) {
                          DefineIntrinsicFunction(globals, name, objectIndex);
                        }
                      } else {
                        DefineIntrinsicFunction(hp, name, objectIndex);
                      }
                      break;
                    case MemberTypes.Property:
                      DefineIntrinsicDataProperty(ht, name, ((PropertyInfo)member).GetValue(null), propAttr.Attributes);
                      break;
                    case MemberTypes.Field:
                      DefineIntrinsicDataProperty(ht, name, ((FieldInfo)member).GetValue(null), propAttr.Attributes);
                      break;
                  }
                }
                if (member.MemberType == MemberTypes.Method) {
                  IntrinsicMemberAttribute propAttr = (IntrinsicMemberAttribute)propAttrs[0];
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  string runtimeName = (propAttr.Getter ? "get " : propAttr.Setter ? "set " : "") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
                  definedObj.Add(CreateIntrinsicFunction(runtimeName, member, (WellKnownObject)objectType, name));
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

    private static void DefineIntrinsicFunction(Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> ht, EcmaPropertyKey name, int sharedIndex) {
      ht[name] = CreateSharedDescriptor(sharedIndex, EcmaPropertyAttributes.DefaultMethodProperty);
    }

    private static IntrinsicFunction CreateIntrinsicFunction(string runtimeName, MemberInfo member, WellKnownObject objectType, EcmaPropertyKey name) {
      IntrinsicFunction fn = new IntrinsicFunction(runtimeName, (MethodInfo)member, objectType, name);
      fn.SetPrototypeOf(EnsureWellKnownObject(WellKnownObject.FunctionPrototype));
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
