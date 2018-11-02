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

    public BoundRuntimeFunction(RuntimeFunction fn, EcmaValue thisArg, params EcmaValue[] args) {
      this.boundFunction = fn;
      this.boundThis = thisArg;
      this.boundArgs = args;
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
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

    public override EcmaValue Construct(EcmaValue newTarget, params EcmaValue[] arguments) {
      return base.Construct(newTarget, arguments);
    }

    public override RuntimeFunctionDelegate GetDelegate() {
      return boundFunction.GetDelegate();
    }
  }
}
