using Codeless.Ecma.Runtime;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.UnitTest {
  public abstract class TestBase {
    public EcmaValue _;

    [SetUp]
    public void SetUp() {
      TestMethod method = TestExecutionContext.CurrentContext.CurrentTest as TestMethod;
      RuntimeFunctionInjectionAttribute attribute = method.Method.GetCustomAttributes<RuntimeFunctionInjectionAttribute>(false).FirstOrDefault();
      if (attribute != null) {
        EcmaPropertyDescriptor descriptor = null;
        RuntimeFunction function = null;
        if (attribute.Type != 0) {
          RuntimeObject obj = RuntimeRealm.Current.GetRuntimeObject(attribute.Type);
          descriptor = obj.GetOwnProperty(attribute.Name);
          _ = obj;
        } else {
          string typeName = method.TypeInfo.Name;
          if (typeName.EndsWith("Object")) {
            typeName = typeName.Substring(0, typeName.Length - 6);
          }
          object objectType;
          if (Enum.TryParse(typeof(WellKnownObject), typeName, out objectType) || Enum.TryParse(typeof(WellKnownObject), typeName.Replace("Constructor", ""), out objectType)) {
            RuntimeObject obj = RuntimeRealm.Current.GetRuntimeObject((WellKnownObject)objectType);
            if (method.MethodName == "Constructor") {
              function = (RuntimeFunction)obj;
              _ = Global.GlobalThis;
            } else {
              descriptor = obj.GetOwnProperty(Regex.Replace(method.MethodName, "^[A-Z](?=[a-z])", v => v.Value.ToLower()));
              if (descriptor == null && Enum.TryParse(typeof(WellKnownSymbol), method.MethodName, out object sym)) {
                descriptor = obj.GetOwnProperty((WellKnownSymbol)sym);
              }
              _ = obj;
            }
          }
        }
        if (descriptor != null) {
          function = (descriptor.IsDataDescriptor ? descriptor.Value : descriptor.Get).GetUnderlyingObject() as RuntimeFunction;
        }
        TestContext.CurrentContext.Test.Arguments[0] = function;
      }
    }

    [TearDown]
    public void TearDown() {
      RuntimeRealm.Current.Dispose();
    }
  }
}
