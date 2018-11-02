using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [AttributeUsage(AttributeTargets.Class)]
  internal class IntrinsicObjectAttribute : Attribute {
    public IntrinsicObjectAttribute(WellKnownObject objectType) {
      this.ObjectType = objectType;
    }

    public WellKnownObject ObjectType { get; private set; }
    public bool Global { get; set; }
    public string Name { get; set; }
  }
}
