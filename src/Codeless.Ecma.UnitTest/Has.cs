using Codeless.Ecma.UnitTest.Constraints;
using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest {
  public class Has : NUnit.Framework.Has {
    public static IConstraint OwnProperty(EcmaPropertyKey name, EcmaPropertyAttributes attributes) {
      return new DataPropertyConstraint(name, null, attributes);
    }

    public static IConstraint OwnProperty(EcmaPropertyKey name, EcmaValue value, EcmaPropertyAttributes attributes) {
      return new DataPropertyConstraint(name, value, attributes);
    }
  }
}
