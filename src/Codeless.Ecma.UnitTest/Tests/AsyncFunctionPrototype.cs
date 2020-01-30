using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AsyncFunctionPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue AsyncFunction = FunctionLiteral(async () => { })["constructor"];
      EcmaValue AsyncFunctionPrototype = AsyncFunction["prototype"];

      That(Object.Invoke("isExtensible", AsyncFunctionPrototype), Is.True, "The initial value of the [[Extensible]] internal slot of the AsyncFunction prototype object is true");
      That(Object.Invoke("getPrototypeOf", AsyncFunctionPrototype), Is.EqualTo(Function.Prototype));
      That(AsyncFunctionPrototype, Has.OwnProperty(Symbol.ToStringTag, "AsyncFunction", EcmaPropertyAttributes.Configurable));
      That(AsyncFunctionPrototype, Has.OwnProperty("constructor", AsyncFunction, EcmaPropertyAttributes.Configurable));
      That(AsyncFunction, Has.OwnProperty("prototype", AsyncFunctionPrototype, EcmaPropertyAttributes.None));
    }
  }
}
