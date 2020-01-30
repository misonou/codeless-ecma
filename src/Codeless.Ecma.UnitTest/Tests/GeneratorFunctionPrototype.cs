using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class GeneratorFunctionPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue GeneratorFunctionPrototype = Object.Invoke("getPrototypeOf", new GeneratorFunction(EmptyGenerator));
      EcmaValue GeneratorFunction = GeneratorFunctionPrototype["constructor"];

      That(Object.Invoke("isExtensible", GeneratorFunctionPrototype), Is.True, "The initial value of the [[Extensible]] internal slot of the GeneratorFunction prototype object is true");
      That(GeneratorFunctionPrototype, Has.OwnProperty(Symbol.ToStringTag, "GeneratorFunction", EcmaPropertyAttributes.Configurable));
      That(GeneratorFunctionPrototype, Has.OwnProperty("prototype", (RuntimeObject)WellKnownObject.GeneratorPrototype, EcmaPropertyAttributes.Configurable));
      That(GeneratorFunctionPrototype, Has.OwnProperty("constructor", GeneratorFunction, EcmaPropertyAttributes.Configurable));
      That(GeneratorFunction, Has.OwnProperty("prototype", GeneratorFunctionPrototype, EcmaPropertyAttributes.None));
    }
  }
}
