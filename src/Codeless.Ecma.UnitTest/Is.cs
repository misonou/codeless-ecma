using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public class Is : NUnit.Framework.Is {
    public static IConstraint Undefined => EqualTo(EcmaValue.Undefined);
  }
}
