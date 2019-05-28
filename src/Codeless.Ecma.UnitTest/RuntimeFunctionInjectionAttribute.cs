using Codeless.Ecma.Runtime;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.UnitTest {
  public class RuntimeFunctionInjectionAttribute : Attribute, ITestBuilder {
    private readonly WellKnownObject type;
    private readonly EcmaPropertyKey name;

    public RuntimeFunctionInjectionAttribute() { }

    public RuntimeFunctionInjectionAttribute(WellKnownObject type, string property) {
      this.type = type;
      this.name = property;
    }

    public RuntimeFunctionInjectionAttribute(WellKnownObject type, WellKnownSymbol property) {
      this.type = type;
      this.name = property;
    }

    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test suite) {
      EcmaPropertyDescriptor descriptor = null;
      RuntimeFunction function = null;
      if (type != 0) {
        descriptor = RuntimeRealm.GetRuntimeObject(type).GetOwnProperty(name);
      } else {
        string typeName = method.TypeInfo.Name;
        if (typeName.EndsWith("Object")) {
          typeName = typeName.Substring(0, typeName.Length - 6);
        }
        RuntimeObject obj = RuntimeRealm.GetRuntimeObject((WellKnownObject)Enum.Parse(typeof(WellKnownObject), typeName));
        if (method.Name == "Constructor") {
          function = (RuntimeFunction)obj;
        } else {
          descriptor = obj.GetOwnProperty(Regex.Replace(method.Name, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
          if (descriptor == null && Enum.TryParse(typeof(WellKnownSymbol), method.Name, out object sym)) {
            descriptor = obj.GetOwnProperty((WellKnownSymbol)sym);
          }
        }
      }
      if (descriptor != null) {
        function = (descriptor.IsDataDescriptor ? descriptor.Value : descriptor.Get).GetUnderlyingObject() as RuntimeFunction;
      }
      yield return new NUnitTestCaseBuilder().BuildTestMethod(method, suite, new TestCaseParameters(new object[] { function }));
    }
  }
}
