using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.Runtime {
  internal class BoundRuntimeFunction : RuntimeFunction {
    private readonly EcmaValue[] boundArgs;
    private readonly RuntimeFunction boundFunction;
    private readonly EcmaValue boundThis;

    public BoundRuntimeFunction(RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      Guard.ArgumentNotNull(fn, "fn");
      Guard.ArgumentNotNull(args, "args");
      this.boundFunction = fn;
      this.boundThis = thisValue;
      this.boundArgs = args;
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      EcmaValue[] args;
      if (arguments.Length == 0) {
        args = boundArgs;
      } else {
        args = new EcmaValue[boundArgs.Length + arguments.Length];
        Array.Copy(boundArgs, args, boundArgs.Length);
        Array.Copy(arguments, 0, args, boundArgs.Length, arguments.Length);
      }
      return base.Call(boundThis, args);
    }

    protected internal override RuntimeFunctionDelegate GetDelegate() {
      return boundFunction.GetDelegate();
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      throw new InvalidOperationException();
    }
  }
}
