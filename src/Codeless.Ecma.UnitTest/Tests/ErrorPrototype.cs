using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ErrorPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Error.Prototype, Has.OwnProperty("constructor", Error, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Error.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Error.Prototype), Is.EqualTo("[object Object]"), "The value of the internal [[Class]] property of Error prototype object is 'Object'");

      That(Error.Prototype, Has.OwnProperty("name", "Error", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Error.Prototype, Has.OwnProperty("message", "", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      foreach (string derived in new[] { "EvalError", "RangeError", "ReferenceError", "SyntaxError", "TypeError", "URIError" }) {
        RuntimeFunction CDerived = (RuntimeFunction)GlobalThis[derived].ToObject();
        That(CDerived.Prototype, Has.OwnProperty("constructor", CDerived, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
        That(CDerived.Prototype.GetPrototypeOf(), Is.EqualTo(Error.Prototype));
        That(Object.Prototype.Get("toString").Call(CDerived.Prototype), Is.EqualTo("[object Object]"),
          "Each NativeError prototype object is an ordinary object. It is not an Error instance and does not have an [[ErrorData]] internal slot.");

        That(CDerived.Prototype, Has.OwnProperty("name", derived, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
        That(CDerived.Prototype, Has.OwnProperty("message", "", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      }
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);
      That(Error.Prototype, Has.OwnProperty("toString", toString, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return name and message", () => {
        Case(Error.Construct("ErrorMessage"), "Error: ErrorMessage");
        Case(Object.Invoke("assign", Error.Construct("ErrorMessage"), CreateObject(new { name = "ErrorName" })), "ErrorName: ErrorMessage");
      });

      It("should return name property if message is undefined", () => {
        Case(Error.Construct(), "Error");
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = "" })), "");
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = "ErrorName" })), "ErrorName");
      });

      It("should return message property if name is empty string", () => {
        Case(Object.Invoke("assign", Error.Construct("ErrorMessage"), CreateObject(new { name = "" })), "ErrorMessage");
      });

      It("should use Error as name if name is undefined", () => {
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = Undefined })), "Error");
      });

      It("should coerce name to string", () => {
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = Null })), Is.EqualTo("null"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = 0 })), Is.EqualTo("0"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = true })), Is.EqualTo("true"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = Object.Construct() })), Is.EqualTo("[object Object]"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = CreateObject(toString: () => "foo") })), Is.EqualTo("foo"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = CreateObject(toString: () => Object.Construct(), valueOf: () => 1) })), Is.EqualTo("1"));
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = CreateObject(toPrimitive: () => "foo") })), Is.EqualTo("foo"));

        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = new Symbol() })), Throws.TypeError);
        Case(Object.Invoke("assign", Error.Construct(), CreateObject(new { name = CreateObject(toString: () => Object.Construct(), valueOf: () => Object.Construct()) })), Throws.TypeError);
      });
    }
  }
}
