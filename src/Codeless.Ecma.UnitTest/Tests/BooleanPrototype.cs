using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class BooleanPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Boolean.Prototype, Has.OwnProperty("constructor", Boolean, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Boolean.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Boolean.Prototype), Is.EqualTo("[object Boolean]"), "Boolean prototype object: its [[Class]] must be 'Boolean'");
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);

      It("should return \"true\" if this boolean value is true,  \"false\" otherwise", () => {
        Case(Boolean.Construct(), "false");
        Case(Boolean.Construct(false), "false");
        Case(Boolean.Construct(true), "true");
        Case(Boolean.Construct(1), "true");
        Case(Boolean.Construct(0), "false");
        Case(Boolean.Construct(Object.Construct()), "true");

        Case((Boolean.Prototype, true), "false");
        Case((Boolean.Construct(), true), "false");
        Case((Boolean.Construct(false), true), "false");
        Case((Boolean.Construct(true), false), "true");
        Case((Boolean.Construct(1), false), "true");
        Case((Boolean.Construct(0), true), "false");
        Case((Boolean.Construct(Object.Construct()), false), "true");
      });

      It("should throw TypeError if this is not a Boolean object", () => {
        Case(String.Construct(), Throws.TypeError);
        Case(Number.Construct(), Throws.TypeError);
        Case(Date.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);

        EcmaValue obj = CreateObject(toString: toString);
        That(() => obj.Invoke("toString"), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);

      It("should return true if this boolean value is true, false otherwise", () => {
        Case(Boolean.Construct(), false);
        Case(Boolean.Construct(false), false);
        Case(Boolean.Construct(true), true);
        Case(Boolean.Construct(1), true);
        Case(Boolean.Construct(0), false);
        Case(Boolean.Construct(Object.Construct()), true);

        Case((Boolean.Prototype, true), false);
        Case((Boolean.Construct(), true), false);
        Case((Boolean.Construct(false), true), false);
        Case((Boolean.Construct(true), false), true);
        Case((Boolean.Construct(1), false), true);
        Case((Boolean.Construct(0), true), false);
        Case((Boolean.Construct(Object.Construct()), false), true);
      });

      It("should throw TypeError if this is not a Boolean object", () => {
        Case(String.Construct(), Throws.TypeError);
        Case(Number.Construct(), Throws.TypeError);
        Case(Date.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);

        EcmaValue obj = CreateObject(valueOf: valueOf);
        That(() => obj.Invoke("valueOf"), Throws.TypeError);
      });
    }
  }
}
