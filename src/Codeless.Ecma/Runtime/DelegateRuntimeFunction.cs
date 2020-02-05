using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class DelegateRuntimeFunction : NativeRuntimeFunction {
    [ThreadStatic]
    private static object _target;
    private readonly object target;

    public DelegateRuntimeFunction(Delegate callback)
      : this(String.Empty, callback) { }

    public DelegateRuntimeFunction(string name, Delegate callback)
      : this(name, callback, WellKnownObject.FunctionPrototype) { }

    public DelegateRuntimeFunction(string name, Delegate callback, WellKnownObject proto)
      : base(name, GetInvokeMethod(callback, out _target), false, proto) {
      target = _target;
    }

    public static RuntimeFunction FromDelegate(Delegate callback) {
      Guard.ArgumentNotNull(callback, "callback");
      if (typeof(Task).IsAssignableFrom(callback.Method.ReturnType)) {
        return new AsyncFunction(callback);
      }
      return new DelegateRuntimeFunction(callback);
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return GetDelegate()(invocation, arguments, target);
    }

    private static MethodInfo GetInvokeMethod(Delegate callback, out object target) {
      Guard.ArgumentNotNull(callback, "callback");
      if (callback is MulticastDelegate m && m.GetInvocationList().Length > 1) {
        target = callback;
        return callback.GetType().GetMethod("Invoke");
      }
      target = callback.Target;
      return callback.Method;
    }
  }
}
