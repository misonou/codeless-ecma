using System;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Class, Inherited = false)]
  public class CloneableAttribute : Attribute {
    public CloneableAttribute(bool copyProperties) {
      this.CopyOwnProperties = copyProperties;
    }

    public bool CopyOwnProperties { get; private set; }
    public bool Transferable { get; set; }
  }
}
