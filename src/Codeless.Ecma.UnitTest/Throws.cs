using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public class Throws : NUnit.Framework.Throws {
    public static IResolveConstraint TypeError => InstanceOf<EcmaTypeErrorException>();

    public static IResolveConstraint RangeError => InstanceOf<EcmaRangeErrorException>();

    public static IResolveConstraint SyntaxError => InstanceOf<EcmaSyntaxErrorException>();

    public static IResolveConstraint URIError => InstanceOf<EcmaUriErrorException>();

    public static IResolveConstraint Test262 => InstanceOf<Test262Exception>();
  }
}
