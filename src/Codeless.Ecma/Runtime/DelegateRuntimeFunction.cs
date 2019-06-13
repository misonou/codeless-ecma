using System;
using System.Reflection;

namespace Codeless.Ecma.Runtime {
  internal class DelegateRuntimeFunction : NativeRuntimeFunction {
    private readonly object target;

    public DelegateRuntimeFunction(Delegate callback)
      : base("", Guard.ArgumentNotNull(callback, "callback").GetType().GetMethod("Invoke")) {
      target = callback;
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return GetDelegate()(invocation, arguments, target);
    }
  }
}
