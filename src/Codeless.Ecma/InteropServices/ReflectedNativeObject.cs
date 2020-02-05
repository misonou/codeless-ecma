using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.InteropServices {
  internal class ReflectedNativeObject : RuntimeObject, INativeObjectWrapper {
    public ReflectedNativeObject(object target)
      : base(target) {
      SetPrototypeOf(ReflectedNativeObjectPrototype.FromType(target.GetType()));
      this.Target = target;
    }

    public new object Target { get; }
  }
}
