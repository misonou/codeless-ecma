using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Method)]
  internal class IntrinsicConstructorAttribute : Attribute {
    private WellKnownObject? superClass;

    public IntrinsicConstructorAttribute() {
      this.Global = true;
    }

    public IntrinsicConstructorAttribute(NativeRuntimeFunctionConstraint constraint)
      : this() {
      this.Constraint = constraint;
    }

    public NativeRuntimeFunctionConstraint Constraint { get; private set; }
    public WellKnownObject Prototype { get; set; }
    public Type ObjectType { get; set; }
    public string Name { get; set; }
    public bool Global { get; set; }

    public WellKnownObject SuperClass {
      get => superClass.GetValueOrDefault(WellKnownObject.FunctionPrototype);
      set => superClass = value;
    }
  }
}
