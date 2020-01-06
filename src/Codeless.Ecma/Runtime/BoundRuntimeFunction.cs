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
      this.Realm = fn.Realm;
      this.BoundThis = thisValue;
      this.BoundArgs = args;
    }

    public RuntimeFunction TargetFunction { get; }
    public EcmaValue BoundThis { get; }
    public EcmaValue[] BoundArgs { get; }

    public override bool IsConstructor {
      get { return this.TargetFunction.IsConstructor; }
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      return this.TargetFunction.Call(this.BoundThis, ArrayHelper.Combine(this.BoundArgs, arguments));
    }

    public override EcmaValue Construct(EcmaValue[] arguments, RuntimeObject newTarget) {
      if (newTarget == this) {
        newTarget = this.TargetFunction;
      }
      return this.TargetFunction.Construct(ArrayHelper.Combine(this.BoundArgs, arguments), newTarget);
    }
  }
}
