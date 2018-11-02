using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public delegate EcmaValue RuntimeFunctionDelegate(RuntimeFunctionInvocation invocation, EcmaValue[] args);

  public abstract class RuntimeFunction : RuntimeObject {
    public RuntimeFunction()
      : base(WellKnownObject.FunctionPrototype) { }

    public override sealed bool IsCallable {
      get { return true; }
    }

    public static RuntimeFunction GetExecutingFunction() {
      RuntimeFunctionInvocation current = RuntimeFunctionInvocation.Current;
      if (current == null) {
        return null;
      }
      return current.FunctionObject;
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this) { ThisValue = thisValue }) {
        return GetDelegate()(invocation, arguments);
      }
    }

    public override EcmaValue Construct(EcmaValue newTarget, params EcmaValue[] arguments) {
      using (RuntimeFunctionInvocation invocation = new RuntimeFunctionInvocation(this) { NewTarget = newTarget.ToRuntimeObject() }) {
        return GetDelegate()(invocation, arguments);
      }
    }

    public abstract RuntimeFunctionDelegate GetDelegate();
  }
}
