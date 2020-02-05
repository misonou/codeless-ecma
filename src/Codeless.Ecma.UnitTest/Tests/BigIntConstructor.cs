using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class BigIntConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "BigInt", 1, BigInt.Prototype);
      That(GlobalThis, Has.OwnProperty("BigInt", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("does not have parseInt", () => {
        That(BigInt["parseInt"], Is.Undefined);
      });

      It("should throw a TypeError if BigInt is called with a new target", () => {
        That(() => BigInt.Construct(), Throws.TypeError);
        That(() => BigInt.Construct(CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a SyntaxError for invalid string", () => {
        Case((_, "10n"), Throws.SyntaxError);
        Case((_, "10x"), Throws.SyntaxError);
        Case((_, "10b"), Throws.SyntaxError);
        Case((_, "10.5"), Throws.SyntaxError);
        Case((_, "0b"), Throws.SyntaxError);
        Case((_, "-0x1"), Throws.SyntaxError);
        Case((_, "-0XFFab"), Throws.SyntaxError);
        Case((_, "0oa"), Throws.SyntaxError);
        Case((_, "000 12"), Throws.SyntaxError);
        Case((_, "0o"), Throws.SyntaxError);
        Case((_, "0x"), Throws.SyntaxError);
        Case((_, "00o"), Throws.SyntaxError);
        Case((_, "00b"), Throws.SyntaxError);
        Case((_, "00x"), Throws.SyntaxError);
      });

      It("should throw a RangeError for non integer number values", () => {
        Case((_, 0.00005), Throws.RangeError);
        Case((_, -0.00005), Throws.RangeError);
        Case((_, 0.1), Throws.RangeError);
        Case((_, -0.1), Throws.RangeError);
        Case((_, 1.1), Throws.RangeError);
        Case((_, -1.1), Throws.RangeError);
        Case((_, Number["MIN_VALUE"]), Throws.RangeError);

        Case((_, Infinity), Throws.RangeError);
        Case((_, -Infinity), Throws.RangeError);
        Case((_, NaN), Throws.RangeError);
      });

      It("should parse string to BigInt according StringToBigInt", () => {
        Case((_, ""), BigIntLiteral(0));
        Case((_, " "), BigIntLiteral(0));

        Case((_, "0b1111"), BigIntLiteral(15));
        Case((_, "0b10"), BigIntLiteral(2));
        Case((_, "0b0"), BigIntLiteral(0));
        Case((_, "0b1"), BigIntLiteral(1));

        string binaryString = "0b1";
        for (int i = 0; i < 128; i++) {
          binaryString += "0";
        }
        Case((_, binaryString), BigIntLiteral("340282366920938463463374607431768211456"));

        Case((_, "0B1111"), BigIntLiteral(15));
        Case((_, "0B10"), BigIntLiteral(2));
        Case((_, "0B0"), BigIntLiteral(0));
        Case((_, "0B1"), BigIntLiteral(1));

        binaryString = "0B1";
        for (int i = 0; i < 128; i++) {
          binaryString += "0";
        }
        Case((_, binaryString), BigIntLiteral("340282366920938463463374607431768211456"));

        Case((_, "10"), BigIntLiteral("10"));
        Case((_, "18446744073709551616"), BigIntLiteral("18446744073709551616"));
        Case((_, "7"), BigIntLiteral("7"));
        Case((_, "88"), BigIntLiteral("88"));
        Case((_, "900"), BigIntLiteral("900"));

        Case((_, "-10"), BigIntLiteral("-10"));
        Case((_, "-18446744073709551616"), BigIntLiteral("-18446744073709551616"));
        Case((_, "-7"), BigIntLiteral("-7"));
        Case((_, "-88"), BigIntLiteral("-88"));
        Case((_, "-900"), BigIntLiteral("-900"));

        Case((_, "0xa"), BigIntLiteral("10"));
        Case((_, "0xff"), BigIntLiteral("255"));
        Case((_, "0xfabc"), BigIntLiteral("64188"));
        Case((_, "0xfffffffffffffffffff"), BigIntLiteral("75557863725914323419135"));

        Case((_, "0Xa"), BigIntLiteral("10"));
        Case((_, "0Xff"), BigIntLiteral("255"));
        Case((_, "0Xfabc"), BigIntLiteral("64188"));
        Case((_, "0Xfffffffffffffffffff"), BigIntLiteral("75557863725914323419135"));

        Case((_, "0o7"), BigIntLiteral("7"));
        Case((_, "0o10"), BigIntLiteral("8"));
        Case((_, "0o20"), BigIntLiteral("16"));

        Case((_, "0O7"), BigIntLiteral("7"));
        Case((_, "0O10"), BigIntLiteral("8"));
        Case((_, "0O20"), BigIntLiteral("16"));
      });

      It("should ignore trailing and leading spaces", () => {
        Case((_, "   0b1111"), BigIntLiteral(15));
        Case((_, "18446744073709551616   "), BigIntLiteral("18446744073709551616"));
        Case((_, "   7   "), BigIntLiteral(7));
        Case((_, "   -197   "), BigIntLiteral(-197));
        Case((_, "     "), BigIntLiteral(0));
      });

      It("should convert integer argument to BigInt", () => {
        Case((_, Number["MAX_SAFE_INTEGER"]), BigIntLiteral(9007199254740991), "BigInt(Number.MAX_SAFE_INTEGER) === 9007199254740991n");
        Case((_, -Number["MAX_SAFE_INTEGER"]), BigIntLiteral(-9007199254740991), "BigInt(-Number.MAX_SAFE_INTEGER) === -9007199254740991n");
        Case((_, Number["MAX_SAFE_INTEGER"] + 1), BigIntLiteral(9007199254740992), "BigInt(Number.MAX_SAFE_INTEGER + 1) === 9007199254740992n");
        Case((_, -Number["MAX_SAFE_INTEGER"] - 1), BigIntLiteral(-9007199254740992), "BigInt(-Number.MAX_SAFE_INTEGER - 1) === -9007199254740992n");
        Case((_, Number["MAX_SAFE_INTEGER"] + 2), BigIntLiteral(9007199254740992), "BigInt(Number.MAX_SAFE_INTEGER + 2) === 9007199254740992n");
        Case((_, -Number["MAX_SAFE_INTEGER"] - 2), BigIntLiteral(-9007199254740992), "BigInt(-Number.MAX_SAFE_INTEGER - 2) === -9007199254740992n");
        Case((_, Number["MAX_SAFE_INTEGER"] + 3), BigIntLiteral(9007199254740994), "BigInt(Number.MAX_SAFE_INTEGER + 3) === 9007199254740994n");
        Case((_, -Number["MAX_SAFE_INTEGER"] - 3), BigIntLiteral(-9007199254740994), "BigInt(-Number.MAX_SAFE_INTEGER - 3) === -9007199254740994n");
      });

      It("should coerce value to primitive with hint Number", () => {
        Case((_, CreateObject(valueOf: () => 44, toString: ThrowTest262Exception)), BigIntLiteral(44));
      });

      It("should return abrupt from ToPrimitive(value)", () => {
        Case((_, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
        Case((_, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void AsIntN(RuntimeFunction asIntN) {
      IsUnconstructableFunctionWLength(asIntN, "asIntN", 2);
      That(BigInt, Has.OwnProperty("asIntN", asIntN, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a BigInt representing bigint -2**bits - 1 to 2**bits - 1 exclusive", () => {
        Case((_, 0, BigIntLiteral(-2)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(-1)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(1)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(2)), BigIntLiteral(0));

        Case((_, 1, BigIntLiteral(-3)), BigIntLiteral(-1));
        Case((_, 1, BigIntLiteral(-2)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(-1)), BigIntLiteral(-1));
        Case((_, 1, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(1)), BigIntLiteral(-1));
        Case((_, 1, BigIntLiteral(2)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(3)), BigIntLiteral(-1));
        Case((_, 1, BigIntLiteral("-123456789012345678901")), BigIntLiteral(-1));
        Case((_, 1, BigIntLiteral("-123456789012345678900")), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral("123456789012345678900")), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral("123456789012345678901")), BigIntLiteral(-1));

        Case((_, 2, BigIntLiteral(-3)), BigIntLiteral(1));
        Case((_, 2, BigIntLiteral(-2)), BigIntLiteral(-2));
        Case((_, 2, BigIntLiteral(-1)), BigIntLiteral(-1));
        Case((_, 2, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral(1)), BigIntLiteral(1));
        Case((_, 2, BigIntLiteral(2)), BigIntLiteral(-2));
        Case((_, 2, BigIntLiteral(3)), BigIntLiteral(-1));
        Case((_, 2, BigIntLiteral("-123456789012345678901")), BigIntLiteral(-1));
        Case((_, 2, BigIntLiteral("-123456789012345678900")), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral("123456789012345678900")), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral("123456789012345678901")), BigIntLiteral(1));

        Case((_, 8, BigIntLiteral(0xab)), BigIntLiteral(-0x55));
        Case((_, 8, BigIntLiteral(0xabcd)), BigIntLiteral(-0x33));
        Case((_, 8, BigIntLiteral(0xabcdef01)), BigIntLiteral(0x01));
        Case((_, 8, BigIntLiteral("0xabcdef0123456789abcdef0123")), BigIntLiteral(0x23));
        Case((_, 8, BigIntLiteral("0xabcdef0123456789abcdef0183")), BigIntLiteral(-0x7d));

        Case((_, 64, BigIntLiteral("0xabcdef0123456789abcdef")), BigIntLiteral("0x0123456789abcdef"));
        Case((_, 65, BigIntLiteral("0xabcdef0123456789abcdef")), BigIntLiteral("-0xfedcba9876543211"));

        Case((_, 200, BigIntLiteral("0xcffffffffffffffffffffffffffffffffffffffffffffffffff")), BigIntLiteral("-0x00000000000000000000000000000000000000000000000001"));
        Case((_, 201, BigIntLiteral("0xcffffffffffffffffffffffffffffffffffffffffffffffffff")), BigIntLiteral("0xffffffffffffffffffffffffffffffffffffffffffffffffff"));

        Case((_, 200, BigIntLiteral("0xc89e081df68b65fedb32cffea660e55df9605650a603ad5fc54")), BigIntLiteral("-0x761f7e209749a0124cd3001599f1aa2069fa9af59fc52a03ac"));
        Case((_, 201, BigIntLiteral("0xc89e081df68b65fedb32cffea660e55df9605650a603ad5fc54")), BigIntLiteral("0x89e081df68b65fedb32cffea660e55df9605650a603ad5fc54"));
      });

      It("should coerce bits parameter with ToIndex", () => {
        Case((_, 0, BigIntLiteral(1)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(1)), BigIntLiteral(-1));
        Case((_, -0.9, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: truncate towards 0");
        Case((_, 0.9, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: truncate towards 0");
        Case((_, NaN, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: NaN => 0");
        Case((_, Undefined, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: undefined => NaN => 0");
        Case((_, Null, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: null => 0");
        Case((_, false, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: false => 0");
        Case((_, true, BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: true => 1");
        Case((_, "0", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number");
        Case((_, "1", BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: parse Number");
        Case((_, "", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, "foo", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, "true", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, 3, BigIntLiteral(10)), BigIntLiteral(2));
        Case((_, "3", BigIntLiteral(10)), BigIntLiteral(2), "toIndex: parse Number");
        Case((_, 3.9, BigIntLiteral(10)), BigIntLiteral(2), "toIndex: truncate towards 0");
        Case((_, "3.9", BigIntLiteral(10)), BigIntLiteral(2), "toIndex: parse Number => truncate towards 0");
        Case((_, EcmaArray.Of(0), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: [0].toString() => \"0\" => 0");
        Case((_, EcmaArray.Of("1"), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: [\"1\"].toString() => \"1\" => 1");
        Case((_, Object.Construct(), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: ({}).toString() => \"[object Object]\" => NaN => 0");
        Case((_, EcmaArray.Of(), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: [].toString() => \"\" => NaN => 0");

        Case((_, Object.Call(default, 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: unbox object with internal slot");
        Case((_, CreateObject(toPrimitive: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: @@toPrimitive");
        Case((_, CreateObject(valueOf: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: valueOf");
        Case((_, CreateObject(toString: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: toString");
        Case((_, Object.Call(default, NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: unbox object with internal slot => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => NaN => 0");
        Case((_, CreateObject(valueOf: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => NaN => 0");
        Case((_, CreateObject(toString: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => undefined => NaN => 0");
        Case((_, CreateObject(valueOf: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => undefined => NaN => 0");
        Case((_, CreateObject(toString: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => undefined => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => null => 0");
        Case((_, CreateObject(valueOf: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => null => 0");
        Case((_, CreateObject(toString: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => null => 0");
        Case((_, Object.Call(default, true), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: unbox object with internal slot => true => 1");
        Case((_, CreateObject(toPrimitive: () => true), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: @@toPrimitive => true => 1");
        Case((_, CreateObject(valueOf: () => true), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: valueOf => true => 1");
        Case((_, CreateObject(toString: () => true), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: toString => true => 1");
        Case((_, Object.Call(default, "1"), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: unbox object with internal slot => parse Number");
        Case((_, CreateObject(toPrimitive: () => "1"), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: @@toPrimitive => parse Number");
        Case((_, CreateObject(valueOf: () => "1"), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: valueOf => parse Number");
        Case((_, CreateObject(toString: () => "1"), BigIntLiteral(1)), BigIntLiteral(-1), "ToIndex: toString => parse Number");

        Case((_, CreateObject(toPrimitive: () => 1, valueOf: ThrowTest262Exception, toString: ThrowTest262Exception), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: @@toPrimitive takes precedence");
        Case((_, CreateObject(valueOf: () => 1, toString: ThrowTest262Exception), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: valueOf takes precedence over toString");
        Case((_, CreateObject(toString: () => 1), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: toString with no valueOf");
        Case((_, CreateObject((Symbol.ToPrimitive, Undefined), ("valueOf", FunctionLiteral(() => 1))), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip @@toPrimitive when it's undefined");
        Case((_, CreateObject((Symbol.ToPrimitive, Null), ("valueOf", FunctionLiteral(() => 1))), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip @@toPrimitive when it's null");
        Case((_, CreateObject(new { valueOf = Null, toString = FunctionLiteral(() => 1) }), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(new { valueOf = 1, toString = FunctionLiteral(() => 1) }), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(new { valueOf = Object.Construct(), toString = FunctionLiteral(() => 1) }), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(new { valueOf = FunctionLiteral(() => Object.Construct()), toString = FunctionLiteral(() => 1) }), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip valueOf when it returns an object");
        Case((_, CreateObject(new { valueOf = FunctionLiteral(() => Object.Call(default, 12345)), toString = FunctionLiteral(() => 1) }), BigIntLiteral(1)), BigIntLiteral(-1), "ToPrimitive: skip valueOf when it returns an object");
      });

      It("should throw a RangeError if bits is not an integer index", () => {
        Case((_, -1, BigIntLiteral(0)), Throws.RangeError);
        Case((_, -2.5, BigIntLiteral(0)), Throws.RangeError);
        Case((_, "-2.5", BigIntLiteral(0)), Throws.RangeError);
        Case((_, -Infinity, BigIntLiteral(0)), Throws.RangeError);
        Case((_, 9007199254740992, BigIntLiteral(0)), Throws.RangeError);
        Case((_, Infinity, BigIntLiteral(0)), Throws.RangeError);
      });

      It("should return abrupt from ToIndex(bits)", () => {
        Case((_, CreateObject(toPrimitive: 1), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, CreateObject(toPrimitive: Object.Construct()), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, CreateObject(toPrimitive: () => Object.Call(default, 1)), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, CreateObject(toPrimitive: () => Object.Construct()), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, CreateObject(toPrimitive: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from @@toPrimitive");
        Case((_, CreateObject(valueOf: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from valueOf");
        Case((_, CreateObject(toString: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from toString");
        Case((_, CreateObject(valueOf: Null, toString: Null), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: 1, toString: 1), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: Object.Construct(), toString: Object.Construct()), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Call(default, 1)), toString: FunctionLiteral(() => Object.Call(default, 1))), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Construct()), toString: FunctionLiteral(() => Object.Construct())), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");

        Case((_, Object.Call(default, BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(valueOf: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toString: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, new Symbol("1"), BigIntLiteral(0)), Throws.TypeError);
        Case((_, Object.Call(default, new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(valueOf: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toString: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
      });

      It("should coerce bigint parameter", () => {
        Case((_, 2, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, -BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, false), BigIntLiteral(0), "ToBigInt: false => 0n");
        Case((_, 2, true), BigIntLiteral(1), "ToBigInt: true => 1n");
        Case((_, 2, "1"), BigIntLiteral(1), "ToBigInt: parse BigInt");
        Case((_, 2, "-0"), BigIntLiteral(0), "ToBigInt: parse BigInt");
        Case((_, 2, ""), BigIntLiteral(0), "ToBigInt: empty String => 0n");
        Case((_, 2, "     "), BigIntLiteral(0), "ToBigInt: String with only whitespace => 0n");
        Case((_, 2, EcmaArray.Of()), BigIntLiteral(0), "ToBigInt: .toString() => empty String => 0n");
        Case((_, 2, EcmaArray.Of(1)), BigIntLiteral(1), "ToBigInt: .toString() => parse BigInt");
        Case((_, 3, BigIntLiteral(10)), BigIntLiteral(2));
        Case((_, 3, "10"), BigIntLiteral(2), "ToBigInt: parse BigInt");
        Case((_, 3, "0b1010"), BigIntLiteral(2), "ToBigInt: parse BigInt binary");
        Case((_, 3, "0o12"), BigIntLiteral(2), "ToBigInt: parse BigInt octal");
        Case((_, 3, "0xa"), BigIntLiteral(2), "ToBigInt: parse BigInt hex");
        Case((_, 3, "    0xa    "), BigIntLiteral(2), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 3, "     10     "), BigIntLiteral(2), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 3, EcmaArray.Of(BigIntLiteral(10))), BigIntLiteral(2), "ToBigInt: .toString() => parse BigInt");
        Case((_, 3, EcmaArray.Of("10")), BigIntLiteral(2), "ToBigInt: .toString() => parse BigInt");
        Case((_, 4, BigIntLiteral("12345678901234567890003")), BigIntLiteral(3));
        Case((_, 4, "12345678901234567890003"), BigIntLiteral(3), "ToBigInt: parse BigInt");
        Case((_, 4, "0b10100111010100001010110110010011100111011001110001010000100100010001010011"), BigIntLiteral(3), "ToBigInt: parse BigInt binary");
        Case((_, 4, "0o2472412662347316120442123"), BigIntLiteral(3), "ToBigInt: parse BigInt octal");
        Case((_, 4, "0x29d42b64e7671424453"), BigIntLiteral(3), "ToBigInt: parse BigInt hex");
        Case((_, 4, "    0x29d42b64e7671424453    "), BigIntLiteral(3), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 4, "     12345678901234567890003     "), BigIntLiteral(3), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 4, EcmaArray.Of(BigIntLiteral("12345678901234567890003"))), BigIntLiteral(3), "ToBigInt: .toString() => parse BigInt");
        Case((_, 4, EcmaArray.Of("12345678901234567890003")), BigIntLiteral(3), "ToBigInt: .toString() => parse BigInt");

        Case((_, 2, Object.Call(default, BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: unbox object with internal slot");
        Case((_, 2, CreateObject(toPrimitive: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: @@toPrimitive");
        Case((_, 2, CreateObject(valueOf: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: valueOf");
        Case((_, 2, CreateObject(toString: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: toString");
        Case((_, 2, Object.Call(default, true)), BigIntLiteral(1), "ToBigInt: unbox object with internal slot => true => 1n");
        Case((_, 2, CreateObject(toPrimitive: () => true)), BigIntLiteral(1), "ToBigInt: @@toPrimitive => true => 1n");
        Case((_, 2, CreateObject(valueOf: () => true)), BigIntLiteral(1), "ToBigInt: valueOf => true => 1n");
        Case((_, 2, CreateObject(toString: () => true)), BigIntLiteral(1), "ToBigInt: toString => true => 1n");
        Case((_, 2, Object.Call(default, "1")), BigIntLiteral(1), "ToBigInt: unbox object with internal slot => parse BigInt");
        Case((_, 2, CreateObject(toPrimitive: () => "1")), BigIntLiteral(1), "ToBigInt: @@toPrimitive => parse BigInt");
        Case((_, 2, CreateObject(valueOf: () => "1")), BigIntLiteral(1), "ToBigInt: valueOf => parse BigInt");
        Case((_, 2, CreateObject(toString: () => "1")), BigIntLiteral(1), "ToBigInt: toString => parse BigInt");

        Case((_, 2, CreateObject(toPrimitive: () => "1", valueOf: ThrowTest262Exception, toString: ThrowTest262Exception)), BigIntLiteral(1), "ToPrimitive: @@toPrimitive takes precedence");
        Case((_, 2, CreateObject(valueOf: () => "1", toString: ThrowTest262Exception)), BigIntLiteral(1), "ToPrimitive: valueOf takes precedence over toString");
        Case((_, 2, CreateObject(toString: () => "1")), BigIntLiteral(1), "ToPrimitive: toString with no valueOf");
        Case((_, 2, CreateObject(toPrimitive: Undefined, valueOf: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's undefined");
        Case((_, 2, CreateObject(toPrimitive: Null, valueOf: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's null");
        Case((_, 2, CreateObject(valueOf: Null, toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: 1, toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: Object.Construct(), toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: () => Object.Construct(), toString: () => "1")), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
        Case((_, 2, CreateObject(valueOf: () => Object.Call(default, 12345), toString: () => "1")), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
      });

      It("should return abrupt from ToBigInt(bigint)", () => {
        Case((_, 0, CreateObject(toPrimitive: 1)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, 0, CreateObject(toPrimitive: Object.Construct())), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, 0, CreateObject(toPrimitive: () => Object.Call(default, 1))), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, 0, CreateObject(toPrimitive: () => Object.Construct())), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, 0, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from @@toPrimitive");
        Case((_, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from valueOf");
        Case((_, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from toString");
        Case((_, 0, CreateObject(valueOf: Null, toString: Null)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: 1, toString: 1)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: Object.Construct(), toString: Object.Construct())), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: () => Object.Call(default, 1), toString: () => Object.Call(default, 1))), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: () => Object.Construct(), toString: () => Object.Construct())), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");

        Case(_, Throws.TypeError, "ToBigInt: no argument => undefined => TypeError");
        Case((_, 0), Throws.TypeError, "ToBigInt: no argument => undefined => TypeError");
        Case((_, 0, Undefined), Throws.TypeError, "ToBigInt: undefined => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => Undefined)), Throws.TypeError, "ToBigInt: @@toPrimitive => undefined => TypeError");
        Case((_, 0, CreateObject(valueOf: () => Undefined)), Throws.TypeError, "ToBigInt: valueOf => undefined => TypeError");
        Case((_, 0, CreateObject(toString: () => Undefined)), Throws.TypeError, "ToBigInt: toString => undefined => TypeError");
        Case((_, 0, Null), Throws.TypeError, "ToBigInt: null => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => Null)), Throws.TypeError, "ToBigInt: @@toPrimitive => null => TypeError");
        Case((_, 0, CreateObject(valueOf: () => Null)), Throws.TypeError, "ToBigInt: valueOf => null => TypeError");
        Case((_, 0, CreateObject(toString: () => Null)), Throws.TypeError, "ToBigInt: toString => null => TypeError");
        Case((_, 0, 0), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, Object.Call(default, 0)), Throws.TypeError, "ToBigInt: unbox object with internal slot => Number => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => 0)), Throws.TypeError, "ToBigInt: @@toPrimitive => Number => TypeError");
        Case((_, 0, CreateObject(valueOf: () => 0)), Throws.TypeError, "ToBigInt: valueOf => Number => TypeError");
        Case((_, 0, CreateObject(toString: () => 0)), Throws.TypeError, "ToBigInt: toString => Number => TypeError");
        Case((_, 0, NaN), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, Infinity), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, new Symbol("1")), Throws.TypeError, "ToBigInt: Symbol => TypeError");
        Case((_, 0, Object.Call(default, new Symbol("1"))), Throws.TypeError, "ToBigInt: unbox object with internal slot => Symbol => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: @@toPrimitive => Symbol => TypeError");
        Case((_, 0, CreateObject(valueOf: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: valueOf => Symbol => TypeError");
        Case((_, 0, CreateObject(toString: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: toString => Symbol => TypeError");

        Case((_, 0, "a"), Throws.SyntaxError, "ToBigInt: unparseable BigInt");
        Case((_, 0, "0b2"), Throws.SyntaxError, "ToBigInt: unparseable BigInt binary");
        Case((_, 0, Object.Call(default, "0b2")), Throws.SyntaxError, "ToBigInt: unbox object with internal slot => unparseable BigInt binary");
        Case((_, 0, CreateObject(toPrimitive: () => "0b2")), Throws.SyntaxError, "ToBigInt: @@toPrimitive => unparseable BigInt binary");
        Case((_, 0, CreateObject(valueOf: () => "0b2")), Throws.SyntaxError, "ToBigInt: valueOf => unparseable BigInt binary");
        Case((_, 0, CreateObject(toString: () => "0b2")), Throws.SyntaxError, "ToBigInt: toString => unparseable BigInt binary");
        Case((_, 0, "   0b2   "), Throws.SyntaxError, "ToBigInt: unparseable BigInt with leading/trailing whitespace");
        Case((_, 0, "0o8"), Throws.SyntaxError, "ToBigInt: unparseable BigInt octal");
        Case((_, 0, "0xg"), Throws.SyntaxError, "ToBigInt: unparseable BigInt hex");
        Case((_, 0, "1n"), Throws.SyntaxError, "ToBigInt: unparseable BigInt due to literal suffix");
      });

      It("should coerce bits before bigint", () => {
        asIntN.Call(_, CreateObject(valueOf: Intercept(() => 0, "bits")), CreateObject(valueOf: Intercept(() => BigIntLiteral(0), "bigint")));
        That(Logs, Is.EquivalentTo(new[] { "bits", "bigint" }));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void AsUintN(RuntimeFunction asUintN) {
      IsUnconstructableFunctionWLength(asUintN, "asUintN", 2);
      That(BigInt, Has.OwnProperty("asUintN", asUintN, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a BigInt representing bigint modulo 2**bits", () => {
        Case((_, 0, BigIntLiteral(-2)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(-1)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(1)), BigIntLiteral(0));
        Case((_, 0, BigIntLiteral(2)), BigIntLiteral(0));

        Case((_, 1, BigIntLiteral(-3)), BigIntLiteral(1));
        Case((_, 1, BigIntLiteral(-2)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(-1)), BigIntLiteral(1));
        Case((_, 1, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(1)), BigIntLiteral(1));
        Case((_, 1, BigIntLiteral(2)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(3)), BigIntLiteral(1));
        Case((_, 1, BigIntLiteral("-123456789012345678901")), BigIntLiteral(1));
        Case((_, 1, BigIntLiteral("-123456789012345678900")), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral("123456789012345678900")), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral("123456789012345678901")), BigIntLiteral(1));

        Case((_, 2, BigIntLiteral(-3)), BigIntLiteral(1));
        Case((_, 2, BigIntLiteral(-2)), BigIntLiteral(2));
        Case((_, 2, BigIntLiteral(-1)), BigIntLiteral(3));
        Case((_, 2, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral(1)), BigIntLiteral(1));
        Case((_, 2, BigIntLiteral(2)), BigIntLiteral(2));
        Case((_, 2, BigIntLiteral(3)), BigIntLiteral(3));
        Case((_, 2, BigIntLiteral("-123456789012345678901")), BigIntLiteral(3));
        Case((_, 2, BigIntLiteral("-123456789012345678900")), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral("123456789012345678900")), BigIntLiteral(0));
        Case((_, 2, BigIntLiteral("123456789012345678901")), BigIntLiteral(1));

        Case((_, 8, BigIntLiteral(0xab)), BigIntLiteral(0xab));
        Case((_, 8, BigIntLiteral(0xabcd)), BigIntLiteral(0xcd));
        Case((_, 8, BigIntLiteral(0xabcdef01)), BigIntLiteral(0x01));
        Case((_, 8, BigIntLiteral("0xabcdef0123456789abcdef0123")), BigIntLiteral(0x23));
        Case((_, 8, BigIntLiteral("0xabcdef0123456789abcdef0183")), BigIntLiteral(0x83));

        Case((_, 64, BigIntLiteral("0xabcdef0123456789abcdef")), BigIntLiteral("0x0123456789abcdef"));
        Case((_, 65, BigIntLiteral("0xabcdef0123456789abcdef")), BigIntLiteral("0x10123456789abcdef"));

        Case((_, 200, BigIntLiteral("0xbffffffffffffffffffffffffffffffffffffffffffffffffff")), BigIntLiteral("0x0ffffffffffffffffffffffffffffffffffffffffffffffffff"));
        Case((_, 201, BigIntLiteral("0xbffffffffffffffffffffffffffffffffffffffffffffffffff")), BigIntLiteral("0x1ffffffffffffffffffffffffffffffffffffffffffffffffff"));

        Case((_, 200, BigIntLiteral("0xb89e081df68b65fedb32cffea660e55df9605650a603ad5fc54")), BigIntLiteral("0x089e081df68b65fedb32cffea660e55df9605650a603ad5fc54"));
        Case((_, 201, BigIntLiteral("0xb89e081df68b65fedb32cffea660e55df9605650a603ad5fc54")), BigIntLiteral("0x189e081df68b65fedb32cffea660e55df9605650a603ad5fc54"));
      });

      It("should coerce bits parameter with ToIndex", () => {
        Case((_, 0, BigIntLiteral(1)), BigIntLiteral(0));
        Case((_, 1, BigIntLiteral(1)), BigIntLiteral(1));
        Case((_, -0.9, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: truncate towards 0");
        Case((_, 0.9, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: truncate towards 0");
        Case((_, NaN, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: NaN => 0");
        Case((_, Undefined, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: undefined => NaN => 0");
        Case((_, Null, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: null => 0");
        Case((_, false, BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: false => 0");
        Case((_, true, BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: true => 1");
        Case((_, "0", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number");
        Case((_, "1", BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: parse Number");
        Case((_, "", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, "foo", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, "true", BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: parse Number => NaN => 0");
        Case((_, 3, BigIntLiteral(10)), BigIntLiteral(2));
        Case((_, "3", BigIntLiteral(10)), BigIntLiteral(2), "toIndex: parse Number");
        Case((_, 3.9, BigIntLiteral(10)), BigIntLiteral(2), "toIndex: truncate towards 0");
        Case((_, "3.9", BigIntLiteral(10)), BigIntLiteral(2), "toIndex: parse Number => truncate towards 0");
        Case((_, EcmaArray.Of(0), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: [0].toString() => \"0\" => 0");
        Case((_, EcmaArray.Of("1"), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: [\"1\"].toString() => \"1\" => 1");
        Case((_, Object.Construct(), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: ({}).toString() => \"[object Object]\" => NaN => 0");
        Case((_, EcmaArray.Of(), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: [].toString() => \"\" => NaN => 0");

        Case((_, Object.Call(default, 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: unbox object with internal slot");
        Case((_, CreateObject(toPrimitive: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: @@toPrimitive");
        Case((_, CreateObject(valueOf: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: valueOf");
        Case((_, CreateObject(toString: () => 0), BigIntLiteral(1)), BigIntLiteral(0), "ToPrimitive: toString");
        Case((_, Object.Call(default, NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: unbox object with internal slot => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => NaN => 0");
        Case((_, CreateObject(valueOf: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => NaN => 0");
        Case((_, CreateObject(toString: () => NaN), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => undefined => NaN => 0");
        Case((_, CreateObject(valueOf: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => undefined => NaN => 0");
        Case((_, CreateObject(toString: () => Undefined), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => undefined => NaN => 0");
        Case((_, CreateObject(toPrimitive: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: @@toPrimitive => null => 0");
        Case((_, CreateObject(valueOf: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: valueOf => null => 0");
        Case((_, CreateObject(toString: () => Null), BigIntLiteral(1)), BigIntLiteral(0), "ToIndex: toString => null => 0");
        Case((_, Object.Call(default, true), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: unbox object with internal slot => true => 1");
        Case((_, CreateObject(toPrimitive: () => true), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: @@toPrimitive => true => 1");
        Case((_, CreateObject(valueOf: () => true), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: valueOf => true => 1");
        Case((_, CreateObject(toString: () => true), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: toString => true => 1");
        Case((_, Object.Call(default, "1"), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: unbox object with internal slot => parse Number");
        Case((_, CreateObject(toPrimitive: () => "1"), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: @@toPrimitive => parse Number");
        Case((_, CreateObject(valueOf: () => "1"), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: valueOf => parse Number");
        Case((_, CreateObject(toString: () => "1"), BigIntLiteral(1)), BigIntLiteral(1), "ToIndex: toString => parse Number");

        Case((_, CreateObject(toPrimitive: () => 1, valueOf: ThrowTest262Exception, toString: ThrowTest262Exception), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: @@toPrimitive takes precedence");
        Case((_, CreateObject(valueOf: () => 1, toString: ThrowTest262Exception), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: valueOf takes precedence over toString");
        Case((_, CreateObject(toString: () => 1), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: toString with no valueOf");
        Case((_, CreateObject(toPrimitive: Undefined, valueOf: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's undefined");
        Case((_, CreateObject(toPrimitive: Null, valueOf: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's null");
        Case((_, CreateObject(valueOf: Null, toString: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(valueOf: 1, toString: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(valueOf: Object.Construct(), toString: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Construct()), toString: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Call(default, 12345)), toString: FunctionLiteral(() => 1)), BigIntLiteral(1)), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
      });

      It("should throw a RangeError if bits is not an integer index", () => {
        Case((_, -1, BigIntLiteral(0)), Throws.RangeError);
        Case((_, -2.5, BigIntLiteral(0)), Throws.RangeError);
        Case((_, "-2.5", BigIntLiteral(0)), Throws.RangeError);
        Case((_, -Infinity, BigIntLiteral(0)), Throws.RangeError);
        Case((_, 9007199254740992, BigIntLiteral(0)), Throws.RangeError);
        Case((_, Infinity, BigIntLiteral(0)), Throws.RangeError);
      });

      It("should return abrupt from ToIndex(bits)", () => {
        Case((_, CreateObject((Symbol.ToPrimitive, 1)), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, CreateObject((Symbol.ToPrimitive, Object.Construct())), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, CreateObject(toPrimitive: () => Object.Call(default, 1)), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, CreateObject(toPrimitive: () => Object.Construct()), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, CreateObject(toPrimitive: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from @@toPrimitive");
        Case((_, CreateObject(valueOf: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from valueOf");
        Case((_, CreateObject(toString: ThrowTest262Exception), BigIntLiteral(0)), Throws.Test262, "ToPrimitive: propagate errors from toString");
        Case((_, CreateObject(valueOf: Null, toString: Null), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: 1, toString: 1), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: Object.Construct(), toString: Object.Construct()), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Call(default, 1)), toString: FunctionLiteral(() => Object.Call(default, 1))), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, CreateObject(valueOf: FunctionLiteral(() => Object.Construct()), toString: FunctionLiteral(() => Object.Construct())), BigIntLiteral(0)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");

        Case((_, Object.Call(default, BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(valueOf: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toString: () => BigIntLiteral(0)), BigIntLiteral(0)), Throws.TypeError);
        Case((_, new Symbol("1"), BigIntLiteral(0)), Throws.TypeError);
        Case((_, Object.Call(default, new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(valueOf: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
        Case((_, CreateObject(toString: () => new Symbol("1")), BigIntLiteral(0)), Throws.TypeError);
      });

      It("should coerce bigint parameter", () => {
        Case((_, 2, BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, -BigIntLiteral(0)), BigIntLiteral(0));
        Case((_, 2, false), BigIntLiteral(0), "ToBigInt: false => 0n");
        Case((_, 2, true), BigIntLiteral(1), "ToBigInt: true => 1n");
        Case((_, 2, "1"), BigIntLiteral(1), "ToBigInt: parse BigInt");
        Case((_, 2, "-0"), BigIntLiteral(0), "ToBigInt: parse BigInt");
        Case((_, 2, ""), BigIntLiteral(0), "ToBigInt: empty String => 0n");
        Case((_, 2, "     "), BigIntLiteral(0), "ToBigInt: String with only whitespace => 0n");
        Case((_, 2, EcmaArray.Of()), BigIntLiteral(0), "ToBigInt: .toString() => empty String => 0n");
        Case((_, 2, EcmaArray.Of(1)), BigIntLiteral(1), "ToBigInt: .toString() => parse BigInt");
        Case((_, 3, BigIntLiteral(10)), BigIntLiteral(2));
        Case((_, 3, "10"), BigIntLiteral(2), "ToBigInt: parse BigInt");
        Case((_, 3, "0b1010"), BigIntLiteral(2), "ToBigInt: parse BigInt binary");
        Case((_, 3, "0o12"), BigIntLiteral(2), "ToBigInt: parse BigInt octal");
        Case((_, 3, "0xa"), BigIntLiteral(2), "ToBigInt: parse BigInt hex");
        Case((_, 3, "    0xa    "), BigIntLiteral(2), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 3, "     10     "), BigIntLiteral(2), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 3, EcmaArray.Of(BigIntLiteral(10))), BigIntLiteral(2), "ToBigInt: .toString() => parse BigInt");
        Case((_, 3, EcmaArray.Of("10")), BigIntLiteral(2), "ToBigInt: .toString() => parse BigInt");
        Case((_, 4, BigIntLiteral("12345678901234567890003")), BigIntLiteral(3));
        Case((_, 4, "12345678901234567890003"), BigIntLiteral(3), "ToBigInt: parse BigInt");
        Case((_, 4, "0b10100111010100001010110110010011100111011001110001010000100100010001010011"), BigIntLiteral(3), "ToBigInt: parse BigInt binary");
        Case((_, 4, "0o2472412662347316120442123"), BigIntLiteral(3), "ToBigInt: parse BigInt octal");
        Case((_, 4, "0x29d42b64e7671424453"), BigIntLiteral(3), "ToBigInt: parse BigInt hex");
        Case((_, 4, "    0x29d42b64e7671424453    "), BigIntLiteral(3), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 4, "     12345678901234567890003     "), BigIntLiteral(3), "ToBigInt: parse BigInt ignore leading/trailing whitespace");
        Case((_, 4, EcmaArray.Of(BigIntLiteral("12345678901234567890003"))), BigIntLiteral(3), "ToBigInt: .toString() => parse BigInt");
        Case((_, 4, EcmaArray.Of("12345678901234567890003")), BigIntLiteral(3), "ToBigInt: .toString() => parse BigInt");

        Case((_, 2, Object.Call(default, BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: unbox object with internal slot");
        Case((_, 2, CreateObject(toPrimitive: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: @@toPrimitive");
        Case((_, 2, CreateObject(valueOf: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: valueOf");
        Case((_, 2, CreateObject(toString: () => BigIntLiteral(0))), BigIntLiteral(0), "ToPrimitive: toString");
        Case((_, 2, Object.Call(default, true)), BigIntLiteral(1), "ToBigInt: unbox object with internal slot => true => 1n");
        Case((_, 2, CreateObject(toPrimitive: () => true)), BigIntLiteral(1), "ToBigInt: @@toPrimitive => true => 1n");
        Case((_, 2, CreateObject(valueOf: () => true)), BigIntLiteral(1), "ToBigInt: valueOf => true => 1n");
        Case((_, 2, CreateObject(toString: () => true)), BigIntLiteral(1), "ToBigInt: toString => true => 1n");
        Case((_, 2, Object.Call(default, "1")), BigIntLiteral(1), "ToBigInt: unbox object with internal slot => parse BigInt");
        Case((_, 2, CreateObject(toPrimitive: () => "1")), BigIntLiteral(1), "ToBigInt: @@toPrimitive => parse BigInt");
        Case((_, 2, CreateObject(valueOf: () => "1")), BigIntLiteral(1), "ToBigInt: valueOf => parse BigInt");
        Case((_, 2, CreateObject(toString: () => "1")), BigIntLiteral(1), "ToBigInt: toString => parse BigInt");

        Case((_, 2, CreateObject(toPrimitive: () => "1", valueOf: ThrowTest262Exception, toString: ThrowTest262Exception)), BigIntLiteral(1), "ToPrimitive: @@toPrimitive takes precedence");
        Case((_, 2, CreateObject(valueOf: () => "1", toString: ThrowTest262Exception)), BigIntLiteral(1), "ToPrimitive: valueOf takes precedence over toString");
        Case((_, 2, CreateObject(toString: () => "1")), BigIntLiteral(1), "ToPrimitive: toString with no valueOf");
        Case((_, 2, CreateObject(toPrimitive: Undefined, valueOf: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's undefined");
        Case((_, 2, CreateObject(toPrimitive: Null, valueOf: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip @@toPrimitive when it's null");
        Case((_, 2, CreateObject(valueOf: Null, toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: 1, toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: Object.Construct(), toString: FunctionLiteral(() => "1"))), BigIntLiteral(1), "ToPrimitive: skip valueOf when it's not callable");
        Case((_, 2, CreateObject(valueOf: () => Object.Construct(), toString: () => "1")), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
        Case((_, 2, CreateObject(valueOf: () => Object.Call(default, 12345), toString: () => "1")), BigIntLiteral(1), "ToPrimitive: skip valueOf when it returns an object");
      });

      It("should return abrupt from ToBigInt(bigint)", () => {
        Case((_, 0, CreateObject(toPrimitive: 1)), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, 0, CreateObject(toPrimitive: Object.Construct())), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive is not callable");
        Case((_, 0, CreateObject(toPrimitive: () => Object.Call(default, 1))), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, 0, CreateObject(toPrimitive: () => Object.Construct())), Throws.TypeError, "ToPrimitive: throw when @@toPrimitive returns an object");
        Case((_, 0, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from @@toPrimitive");
        Case((_, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from valueOf");
        Case((_, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262, "ToPrimitive: propagate errors from toString");
        Case((_, 0, CreateObject(valueOf: Null, toString: Null)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: 1, toString: 1)), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: Object.Construct(), toString: Object.Construct())), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: () => Object.Call(default, 1), toString: () => Object.Call(default, 1))), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");
        Case((_, 0, CreateObject(valueOf: () => Object.Construct(), toString: () => Object.Construct())), Throws.TypeError, "ToPrimitive: throw when skipping both valueOf and toString");

        Case(_, Throws.TypeError, "ToBigInt: no argument => undefined => TypeError");
        Case((_, 0), Throws.TypeError, "ToBigInt: no argument => undefined => TypeError");
        Case((_, 0, Undefined), Throws.TypeError, "ToBigInt: undefined => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => Undefined)), Throws.TypeError, "ToBigInt: @@toPrimitive => undefined => TypeError");
        Case((_, 0, CreateObject(valueOf: () => Undefined)), Throws.TypeError, "ToBigInt: valueOf => undefined => TypeError");
        Case((_, 0, CreateObject(toString: () => Undefined)), Throws.TypeError, "ToBigInt: toString => undefined => TypeError");
        Case((_, 0, Null), Throws.TypeError, "ToBigInt: null => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => Null)), Throws.TypeError, "ToBigInt: @@toPrimitive => null => TypeError");
        Case((_, 0, CreateObject(valueOf: () => Null)), Throws.TypeError, "ToBigInt: valueOf => null => TypeError");
        Case((_, 0, CreateObject(toString: () => Null)), Throws.TypeError, "ToBigInt: toString => null => TypeError");
        Case((_, 0, 0), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, Object.Call(default, 0)), Throws.TypeError, "ToBigInt: unbox object with internal slot => Number => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => 0)), Throws.TypeError, "ToBigInt: @@toPrimitive => Number => TypeError");
        Case((_, 0, CreateObject(valueOf: () => 0)), Throws.TypeError, "ToBigInt: valueOf => Number => TypeError");
        Case((_, 0, CreateObject(toString: () => 0)), Throws.TypeError, "ToBigInt: toString => Number => TypeError");
        Case((_, 0, NaN), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, Infinity), Throws.TypeError, "ToBigInt: Number => TypeError");
        Case((_, 0, new Symbol("1")), Throws.TypeError, "ToBigInt: Symbol => TypeError");
        Case((_, 0, Object.Call(default, new Symbol("1"))), Throws.TypeError, "ToBigInt: unbox object with internal slot => Symbol => TypeError");
        Case((_, 0, CreateObject(toPrimitive: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: @@toPrimitive => Symbol => TypeError");
        Case((_, 0, CreateObject(valueOf: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: valueOf => Symbol => TypeError");
        Case((_, 0, CreateObject(toString: () => new Symbol("1"))), Throws.TypeError, "ToBigInt: toString => Symbol => TypeError");

        Case((_, 0, "a"), Throws.SyntaxError, "ToBigInt: unparseable BigInt");
        Case((_, 0, "0b2"), Throws.SyntaxError, "ToBigInt: unparseable BigInt binary");
        Case((_, 0, Object.Call(default, "0b2")), Throws.SyntaxError, "ToBigInt: unbox object with internal slot => unparseable BigInt binary");
        Case((_, 0, CreateObject(toPrimitive: () => "0b2")), Throws.SyntaxError, "ToBigInt: @@toPrimitive => unparseable BigInt binary");
        Case((_, 0, CreateObject(valueOf: () => "0b2")), Throws.SyntaxError, "ToBigInt: valueOf => unparseable BigInt binary");
        Case((_, 0, CreateObject(toString: () => "0b2")), Throws.SyntaxError, "ToBigInt: toString => unparseable BigInt binary");
        Case((_, 0, "   0b2   "), Throws.SyntaxError, "ToBigInt: unparseable BigInt with leading/trailing whitespace");
        Case((_, 0, "0o8"), Throws.SyntaxError, "ToBigInt: unparseable BigInt octal");
        Case((_, 0, "0xg"), Throws.SyntaxError, "ToBigInt: unparseable BigInt hex");
        Case((_, 0, "1n"), Throws.SyntaxError, "ToBigInt: unparseable BigInt due to literal suffix");
      });

      It("should coerce bits before bigint", () => {
        asUintN.Call(_, CreateObject(valueOf: Intercept(() => 0, "bits")), CreateObject(valueOf: Intercept(() => BigIntLiteral(0), "bigint")));
        That(Logs, Is.EquivalentTo(new[] { "bits", "bigint" }));
      });
    }
  }
}
