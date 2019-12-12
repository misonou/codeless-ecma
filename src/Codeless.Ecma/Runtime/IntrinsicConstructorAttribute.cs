﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Method)]
  internal class IntrinsicConstructorAttribute : Attribute {
    private WellKnownObject? superClass;

    public IntrinsicConstructorAttribute() { }

    public IntrinsicConstructorAttribute(NativeRuntimeFunctionConstraint constraint) {
      this.Constraint = constraint;
    }

    public NativeRuntimeFunctionConstraint Constraint { get; private set; }
    public Type ObjectType { get; set; }
    public string Name { get; set; }

    public WellKnownObject SuperClass {
      get => superClass.GetValueOrDefault(WellKnownObject.FunctionPrototype);
      set => superClass = value;
    }
  }
}
