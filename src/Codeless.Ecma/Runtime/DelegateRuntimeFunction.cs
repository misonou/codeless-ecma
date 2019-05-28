using System;
using System.Reflection;

namespace Codeless.Ecma.Runtime {
  internal class DelegateRuntimeFunction : NativeRuntimeFunction {
    private readonly object target;

    public DelegateRuntimeFunction(Delegate callback)
      : base("", callback?.Method) {
      Guard.ArgumentNotNull(callback, "callback");
      target = callback.Target;
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return GetDelegate()(invocation, arguments, target);
    }
  }
}
