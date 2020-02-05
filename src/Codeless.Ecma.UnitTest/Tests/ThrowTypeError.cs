using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ThrowTypeError : TestBase {
    [Test]
    public void Properties() {
      EcmaValue argumentsDesc = Object.Invoke("getOwnPropertyDescriptor", Function.Prototype, "arguments");
      EcmaValue callerDesc = Object.Invoke("getOwnPropertyDescriptor", Function.Prototype, "caller");
      EcmaValue throwTypeError = argumentsDesc["get"];

      That(throwTypeError, Is.TypeOf("function"));
      That(throwTypeError["name"], Is.EqualTo(""));
      That(throwTypeError["length"], Is.EqualTo(0));
      That(Object.Invoke("isExtensible", throwTypeError), Is.False, "%ThrowTypeError% is not extensible");
      That(Object.Invoke("isFrozen", throwTypeError), Is.True, "The integrity level of %ThrowTypeError% is \"frozen\"");
      That(Object.Invoke("getPrototypeOf", throwTypeError), Is.EqualTo(Function.Prototype));

      It("should throw a TypeError when called", () => {
        That(() => throwTypeError.Call(), Throws.TypeError);
      });

      It("does not have own caller and arguments property", () => {
        That(throwTypeError.Invoke("hasOwnProperty", "caller"), Is.False, "caller");
        That(throwTypeError.Invoke("hasOwnProperty", "arguments"), Is.False, "arguments");
      });

      It("is defined once for each realm", () => {
        That(argumentsDesc["get"], Is.EqualTo(throwTypeError), "arguments.get");
        That(argumentsDesc["get"], Is.EqualTo(throwTypeError), "arguments.set");
        That(callerDesc["get"], Is.EqualTo(throwTypeError), "caller.set");
        That(callerDesc["get"], Is.EqualTo(throwTypeError), "caller.get");

        RuntimeRealm other = new RuntimeRealm();
        EcmaValue argumentsDesc1 = Object.Invoke("getOwnPropertyDescriptor", Function.Prototype, "arguments");
        EcmaValue argumentsDesc2 = Object.Invoke("getOwnPropertyDescriptor", other.Global["Function"]["prototype"], "arguments");
        That(argumentsDesc1["get"], Is.Not.EqualTo(argumentsDesc2["get"]));
      });
    }
  }
}
