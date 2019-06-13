using Codeless.Ecma.UnitTest.Constraints;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public class Is : NUnit.Framework.Is {
    public static IConstraint Undefined => EqualTo(EcmaValue.Undefined);

    public static IConstraint TypeOf(string typeOfStr) {
      return new TypeOfConstraint(typeOfStr);
    }

    public new static IConstraint EqualTo(object expected) {
      return NUnit.Framework.Is.EqualTo(expected).Using<object>((a, b) => EcmaValue.Equals(new EcmaValue(a), new EcmaValue(b), EcmaValueComparison.SameValue));
    }

    public static IConstraint EquivalentTo(Array expected) {
      return NUnit.Framework.Is.EquivalentTo(expected).Using(new RecursiveArrayEqualityComparer());
    }
  }
}
