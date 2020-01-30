using Codeless.Ecma.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class AsyncFunction : DelegateRuntimeFunction {
    public AsyncFunction(Delegate callback)
      : this(String.Empty, callback) { }

    public AsyncFunction(string name, Delegate callback)
      : base(name, callback, WellKnownObject.AsyncFunctionPrototype) {
      this.Source = "async " + this.Source;
    }

    public override bool IsConstructor => false;
  }
}
