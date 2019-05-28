using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private static readonly RuntimeObject[] sharedIntrinsics = new RuntimeObject[(int)WellKnownObject.MaxValue];
    private static readonly bool inited;
    [ThreadStatic]
    private static RuntimeRealm current;

    private readonly WeakKeyedCollection dictionary = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private readonly RuntimeRealm previous;
    private readonly List<Action> queuedJobs = new List<Action>();
    private readonly int threadId = Thread.CurrentThread.ManagedThreadId;
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
        if (current == null) {
          current = new RuntimeRealm();
        }
        return current;
      }
    }

    public static RuntimeRealm GetRealm(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        return obj.ToObject().Realm;
      }
      return RuntimeRealm.Current;
    }

    public static RuntimeObject GetRuntimeObject(object target) {
      if (target == null) {
        return null;
      }
      if (target is EcmaValue value) {
        target = value.GetUnderlyingObject();
      }
      if (target is RuntimeObject runtimeObject) {
        return runtimeObject;
      }
      return Current.dictionary.GetOrAdd(NativeObject.Create(target));
    }

    public static RuntimeObject GetRuntimeObject(WellKnownObject type) {
      if ((int)type < 1 || (int)type >= (int)WellKnownObject.MaxValue) {
        throw new ArgumentException();
      }
      return sharedIntrinsics[(int)type - 1];
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
        current = previous;
        queuedJobs.Clear();
        disposed = true;
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
      int index = (int)type - 1;
      RuntimeObject obj = sharedIntrinsics[index];
      if (obj == null) {
        switch (type) {
          case WellKnownObject.ObjectPrototype:
            obj = RuntimeObject.Create(null);
            break;
          case WellKnownObject.NumberPrototype:
            obj = new PrimitiveObject(0, WellKnownObject.ObjectPrototype);
            break;
          case WellKnownObject.StringPrototype:
            obj = new PrimitiveObject("", WellKnownObject.ObjectPrototype);
            break;
          case WellKnownObject.BooleanPrototype:
            obj = new PrimitiveObject(false, WellKnownObject.ObjectPrototype);
            break;
          default:
            obj = new RuntimeObject(GetPrototypeOf(type));
            break;
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
            global.DefineOwnProperty(typeAttr.Name ?? t.Name, new EcmaPropertyDescriptor(new EcmaValue(EnsureWellKnownObject(typeAttr.ObjectType)), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
          }
          foreach (MemberInfo member in t.GetMembers()) {
            IntrinsicConstructorAttribute ctorAttr;
            if (member.HasAttribute(out ctorAttr)) {
              string ctorName = ctorAttr.Name ?? member.Name;
              NativeRuntimeFunction fn = new IntrinsicFunction(ctorName, (MethodInfo)member, WellKnownObject.Global, ctorName);
              definedObj.Add(fn);
              DefineIntrinsicConstructor(typeAttr.ObjectType, ctorName, fn, GetPrototypeOf(typeAttr.ObjectType));
            } else {
              object[] propAttrs = member.GetCustomAttributes(typeof(IntrinsicMemberAttribute), false);
              if (propAttrs.Length > 0) {
                NativeRuntimeFunction fn = null;
                if (member.MemberType == MemberTypes.Method) {
                  IntrinsicMemberAttribute propAttr = (IntrinsicMemberAttribute)propAttrs[0];
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  string runtimeName = (propAttr.Getter ? "get " : propAttr.Setter ? "set " : "") + (name.IsSymbol ? "[" + name.Symbol.Description + "]" : name.Name);
                  fn = new IntrinsicFunction(runtimeName, (MethodInfo)member, typeAttr.ObjectType, name);
                  definedObj.Add(fn);
                }
                foreach (IntrinsicMemberAttribute propAttr in propAttrs) {
                  EcmaPropertyKey name = GetNameFromMember(propAttr, member);
                  switch (member.MemberType) {
                    case MemberTypes.Method:
                      if (propAttr.Getter) {
                        DefineIntrinsicAccessorProperty(typeAttr.ObjectType, name, fn, propAttr.Attributes, true);
                      } else if (propAttr.Setter) {
                        DefineIntrinsicAccessorProperty(typeAttr.ObjectType, name, fn, propAttr.Attributes, false);
                      } else if (((MethodInfo)member).IsStatic) {
                        DefineIntrinsicFunction(typeAttr.ObjectType, name, fn);
                        if (propAttr.Global) {
                          DefineIntrinsicFunction(WellKnownObject.Global, name, fn);
                        }
                      } else {
                        DefineIntrinsicFunction(typeProto, name, fn);
                      }
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
        }
      }
      return definedObj;
    }

    private static EcmaPropertyKey GetNameFromMember(IntrinsicMemberAttribute propAttr, MemberInfo member) {
      if (propAttr.Symbol != 0) {
        return new EcmaPropertyKey(propAttr.Symbol);
      }
      return new EcmaPropertyKey(propAttr.Name ?? Regex.Replace(member.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
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

    private static void DefineIntrinsicAccessorProperty(WellKnownObject type, EcmaPropertyKey name, NativeRuntimeFunction fn, EcmaPropertyAttributes state, bool isGetter) {
      RuntimeObject obj = EnsureWellKnownObject(type);
      if (name.IsSymbol) {
        state &= ~EcmaPropertyAttributes.Enumerable;
      }
      if (isGetter) {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(state) { Get = fn });
      } else {
        obj.DefineOwnProperty(name, new EcmaPropertyDescriptor(state) { Set = fn });
      }
    }

    private static void DefineIntrinsicFunction(WellKnownObject type, EcmaPropertyKey name, NativeRuntimeFunction fn) {
      RuntimeObject obj = EnsureWellKnownObject(type);
      obj.CreateMethodProperty(name, new EcmaValue(fn));
    }

    private static void DefineIntrinsicConstructor(WellKnownObject ctorType, EcmaPropertyKey name, NativeRuntimeFunction ctor, WellKnownObject fnProto) {
      DefineIntrinsicFunction(WellKnownObject.Global, name, ctor);
      if (fnProto != WellKnownObject.ObjectPrototype || ctorType == WellKnownObject.ObjectConstructor) {
        RuntimeObject proto = EnsureWellKnownObject(fnProto);
        ctor.SetPrototypeInternal(proto, EcmaPropertyAttributes.None);
      }
      RuntimeObject cur = EnsureWellKnownObject(ctorType);
      foreach (EcmaPropertyKey key in cur.GetOwnPropertyKeys()) {
        ctor.DefineOwnProperty(key, cur.GetOwnProperty(key));
      }
      sharedIntrinsics[(int)ctorType - 1] = ctor;
    }
  }
}
