using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class TransientPrimitiveObject : PrimitiveObject {
    public TransientPrimitiveObject(EcmaValue value, WellKnownObject defaultProto)
      : base(value, defaultProto) { }

    public override EcmaValue PrimitiveValue {
      get { return base.PrimitiveValue; }
      set { throw new InvalidOperationException(); }
    }

    public override bool IsExtensible {
      get { return false; }
    }
  }
}
