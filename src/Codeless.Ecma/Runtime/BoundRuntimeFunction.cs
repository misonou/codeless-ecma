using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma.Runtime {
  internal class BoundRuntimeFunction : RuntimeFunction {
    public BoundRuntimeFunction(RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      Guard.ArgumentNotNull(fn, "fn");
      Guard.ArgumentNotNull(args, "args");
      this.TargetFunction = fn;
      this.BoundThis = thisValue;
      this.BoundArgs = args;
    }

    public RuntimeFunction TargetFunction { get; }
    public EcmaValue BoundThis { get; }
    public EcmaValue[] BoundArgs { get; }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      Guard.ArgumentNotNull(arguments, "arguments");
      EcmaValue[] boundArgs = this.BoundArgs;
      EcmaValue[] args;
      if (arguments.Length == 0) {
        args = boundArgs;
      } else {
        args = new EcmaValue[boundArgs.Length + arguments.Length];
        Array.Copy(boundArgs, args, boundArgs.Length);
        Array.Copy(arguments, 0, args, boundArgs.Length, arguments.Length);
      }
      return this.TargetFunction.Call(this.BoundThis, args);
    }
  }
}
