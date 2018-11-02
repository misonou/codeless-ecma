using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public enum EcmaSpecificationKind {
    AbstractOperations,
    InternalSlot,
    InternalMethod,
    IntrinsicObjects,
    RuntimeSemantics
  }

  [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
  public class EcmaSpecificationAttribute : Attribute {
    public EcmaSpecificationAttribute(string name, EcmaSpecificationKind kind) { }

    public string Description { get; set; }
    public bool NotImplemented { get; set; }
    public bool NotSupported { get; set; }
  }
}
