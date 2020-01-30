using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class BigIntPrototype : TestBase {
    [Test]
    public void Properties() {
      That(BigInt, Has.OwnProperty("prototype", BigInt.Prototype, EcmaPropertyAttributes.None));
      That(BigInt.Prototype, Has.OwnProperty("constructor", BigInt, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(BigInt.Prototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "BigInt", EcmaPropertyAttributes.Configurable));
      That(BigInt.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);
      That(BigInt.Prototype, Has.OwnProperty("toString", toString, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError if the this value is not a BigInt", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case("", Throws.TypeError);
        Case("1n", Throws.TypeError);
        Case(0, Throws.TypeError);
        Case(1, Throws.TypeError);
        Case(NaN, Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);

        Case(EcmaArray.Of(BigIntLiteral(1)), Throws.TypeError);
        Case(CreateObject(new { x = BigIntLiteral(1) }), Throws.TypeError);
        Case(CreateObject(valueOf: ThrowTest262Exception, toString: ThrowTest262Exception, toPrimitive: ThrowTest262Exception), Throws.TypeError);
      });

      It("should throw a RangeError if radix is invalid", () => {
        Case((BigIntLiteral(0), 0), Throws.RangeError);
        Case((BigIntLiteral(-1), 0), Throws.RangeError);
        Case((BigIntLiteral(1), 0), Throws.RangeError);

        Case((BigIntLiteral(0), 1), Throws.RangeError);
        Case((BigIntLiteral(-1), 1), Throws.RangeError);
        Case((BigIntLiteral(1), 1), Throws.RangeError);

        Case((BigIntLiteral(0), 37), Throws.RangeError);
        Case((BigIntLiteral(-1), 37), Throws.RangeError);
        Case((BigIntLiteral(1), 37), Throws.RangeError);

        Case((BigIntLiteral(0), Null), Throws.RangeError);
        Case((BigIntLiteral(-1), Null), Throws.RangeError);
        Case((BigIntLiteral(1), Null), Throws.RangeError);
      });

      It("should return only decimal digits, does not include BigIntLiteralSuffix", () => {
        Case(BigInt.Call(default, 0), "0");
        Case(BigInt.Call(default, BigIntLiteral(0)), "0");
        Case(BigIntLiteral(0), "0");
      });

      It("should return with default radix", () => {
        Case(BigIntLiteral(-100), "-100", "(-100n).toString() === '-100'");
        Case(BigIntLiteral(0), "0", "(0n).toString() === '0'");
        Case(BigIntLiteral(100), "100", "(100n).toString() === '100'");

        Case((BigIntLiteral(-100), Undefined), "-100", "(-100n).toString(undefined) === '-100'");
        Case((BigIntLiteral(0), Undefined), "0", "(0n).toString(undefined) === '0'");
        Case((BigIntLiteral(100), Undefined), "100", "(100n).toString(undefined) === '100'");
      });

      It("should return with radix between 2 and 36", () => {
        for (int r = 2; r <= 36; r++) {
          Case((BigIntLiteral(0), r), "0");
          Case((BigIntLiteral(-1), r), "-1");
          Case((BigIntLiteral(1), r), "1");
        }
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);
      That(BigInt.Prototype, Has.OwnProperty("valueOf", valueOf, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError if this is not a BigInt neither an Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case("", Throws.TypeError);
        Case("1n", Throws.TypeError);
        Case(0, Throws.TypeError);
        Case(1, Throws.TypeError);
        Case(NaN, Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError if this is an Object without a [[BigIntData]] internal", () => {
        Case(BigInt.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Object.Call(default, 1), Throws.TypeError);
      });

      It("should return the primitive BigInt value", () => {
        Case(BigIntLiteral(0), BigIntLiteral(0));
        Case(Object.Call(default, BigIntLiteral(0)), BigIntLiteral(0));

        EcmaValue other = new RuntimeRealm().Global;
        EcmaValue wrapped = other.Invoke("Object", other.Invoke("BigInt", 0));
        Case(wrapped, BigIntLiteral(0));
      });
    }
  }
}
