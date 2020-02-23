using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Class)]
  public class IntrinsicObjectAttribute : Attribute {
    public IntrinsicObjectAttribute(object objectType) {
      Guard.ArgumentNotNull(objectType, "objectType");
      this.ObjectType = objectType;
    }

    public object ObjectType { get; private set; }
    public object Prototype { get; set; }
    public bool Global { get; set; }
    public string Name { get; set; }
  }
}
