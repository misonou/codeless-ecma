using NUnit.Framework.Constraints;
using System;

namespace Codeless.Ecma.UnitTest.Constraints {
  internal class ThrowInstanceOfTypeConstraint : InstanceOfTypeConstraint {
    private readonly EcmaValue fn;

    public ThrowInstanceOfTypeConstraint(EcmaValue fn)
      : base(typeof(Exception)) {
      this.fn = fn;
    }

    protected override bool Matches(object actual) {
      EcmaValue value = EcmaValueUtility.GetValueFromException((Exception)actual);
      return value.InstanceOf(fn);
    }
  }
}
