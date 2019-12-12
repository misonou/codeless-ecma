using System;
using System.Reflection;

namespace Codeless.Ecma.Runtime {
  internal class DelegateRuntimeFunction : NativeRuntimeFunction {
    [ThreadStatic]
    private static object _target;
    private readonly object target;

    public DelegateRuntimeFunction(Delegate callback)
      : base(String.Empty, GetInvokeMethod(callback, out _target)) {
      target = _target;
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
