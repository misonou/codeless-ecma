using Codeless.Ecma.Runtime;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;

namespace Codeless.Ecma.UnitTest {
  public class RuntimeFunctionInjectionAttribute : Attribute, ITestBuilder {
    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite) {
      RuntimeFunction function = RuntimeRealm.GetRuntimeObject((WellKnownObject)Enum.Parse(typeof(WellKnownObject), method.TypeInfo.Name)).GetMethod(
        method.Name.Substring(0, 1).ToLower() + method.Name.Substring(1));
      yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, new TestCaseParameters(new object[] { function }));
    }
  }
}
