using Codeless.Ecma.Runtime;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.UnitTest {
  public class RuntimeFunctionInjectionAttribute : Attribute, ITestBuilder {
    public WellKnownObject Type { get; set; }
    public EcmaPropertyKey Name { get; set; }

    public RuntimeFunctionInjectionAttribute() { }

    public RuntimeFunctionInjectionAttribute(WellKnownObject type, string property) {
      this.Type = type;
      this.Name = property;
    }

    public RuntimeFunctionInjectionAttribute(WellKnownObject type, WellKnownSymbol property) {
      this.Type = type;
      this.Name = property;
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite) {
      yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, new TestCaseParameters(new object[] { null }));
    }
  }
}
