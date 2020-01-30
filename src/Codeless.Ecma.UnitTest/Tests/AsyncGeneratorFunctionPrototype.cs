using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AsyncGeneratorFunctionPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue AsyncGeneratorFunctionPrototype = Object.Invoke("getPrototypeOf", new AsyncGeneratorFunction(EmptyGenerator));
      EcmaValue AsyncGeneratorFunction = AsyncGeneratorFunctionPrototype["constructor"];

      That(Object.Invoke("isExtensible", AsyncGeneratorFunctionPrototype), Is.True, "The initial value of the [[Extensible]] internal slot of the AsyncGeneratorFunction prototype object is true");
      That(AsyncGeneratorFunctionPrototype, Has.OwnProperty(Symbol.ToStringTag, "AsyncGeneratorFunction", EcmaPropertyAttributes.Configurable));
      That(AsyncGeneratorFunctionPrototype, Has.OwnProperty("prototype", (RuntimeObject)WellKnownObject.AsyncGeneratorPrototype, EcmaPropertyAttributes.Configurable));
      That(AsyncGeneratorFunctionPrototype, Has.OwnProperty("constructor", AsyncGeneratorFunction, EcmaPropertyAttributes.Configurable));
      That(AsyncGeneratorFunction, Has.OwnProperty("prototype", AsyncGeneratorFunctionPrototype, EcmaPropertyAttributes.None));
    }
  }
}
