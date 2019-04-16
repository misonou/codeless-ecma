using Codeless.Ecma.Runtime;
using System;
using System.Reflection;

namespace Codeless.Ecma.Native {
  internal class DelegateFunction : RuntimeFunction {
    private readonly Delegate callback;

    public DelegateFunction(Delegate callback) {
      Guard.ArgumentNotNull(callback, "callback");
      this.callback = callback;
      InitProperty(String.Empty, callback.Method.GetParameters().Length);
    }

    protected internal override RuntimeFunctionDelegate GetDelegate() {
      return Invoke;
    }

    private EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] args) {
      ParameterInfo[] parameters = callback.Method.GetParameters();
      object[] arguments = new object[parameters.Length];
      int i = 0, j = 0;
      foreach (ParameterInfo p in parameters) {
        if (p.HasAttribute(out ThisAttribute _)) {
          arguments[i] = invocation.ThisValue;
        } else if (p.HasAttribute(out NewTargetAttribute _)) {
          arguments[i] = default;
        } else {
          arguments[i] = j < args.Length ? args[j++] : default;
        }
        i++;
      }
      try {
        return new EcmaValue(callback.DynamicInvoke(arguments));
      } catch (TargetInvocationException ex) {
        throw ex.InnerException;
      }
    }
  }
}
