using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class TransientIntrinsicObject : IntrinsicObject {
    public TransientIntrinsicObject(EcmaValue value, WellKnownObject defaultProto)
      : base(value, defaultProto) { }

    public override EcmaValue IntrinsicValue {
      get { return base.IntrinsicValue; }
      set { throw new InvalidOperationException(); }
    }

    public override bool IsExtensible {
      get { return false; }
    }
  }
}
