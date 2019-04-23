using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest {
  public class Has : NUnit.Framework.Has {
    public static IConstraint DataProperty(string name, EcmaPropertyAttributes attributes) {
      return new DataPropertyConstraint(name, attributes);
    }
  }
}
