using System;

namespace Codeless.Ecma.Runtime {
  public class IntrinsicObjectEnumAttribute : Attribute {
    public IntrinsicObjectEnumAttribute(int objectCount) {
      this.ObjectCount = objectCount;
    }

    public int ObjectCount { get; }
  }
}
