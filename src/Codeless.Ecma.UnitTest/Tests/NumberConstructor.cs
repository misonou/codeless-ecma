using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class NumberConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Number", 1, Number.Prototype);

      It("should return value of type \"number\"", () => {
        Case((_, "10"), Is.TypeOf("number"));
        Case((_, 10), Is.TypeOf("number"));
        Case((_, String.Construct("10")), Is.TypeOf("number"));
        Case((_, Object.Construct(10)), Is.TypeOf("number"));
      });

      It("should return NaN for non-numeric string", () => {
        Case((_, "abc"), NaN);
        Case((_, "\u180E"), NaN);
        Case((_, "\u180EInfinity\u180E"), NaN);
        Case((_, "\u180E-Infinity\u180E"), NaN);
        Case((_, Undefined), NaN);
      });

      It("should return the same numeric value", () => {
        Case((_, NaN), NaN);
        Case((_, 0), 0);
        Case((_, EcmaValue.NegativeZero), EcmaValue.NegativeZero);
        Case((_, EcmaValue.Infinity), EcmaValue.Infinity);
        Case((_, EcmaValue.NegativeInfinity), EcmaValue.NegativeInfinity);
        Case((_, Number.Get("MAX_VALUE")), Number.Get("MAX_VALUE"));
        Case((_, Number.Get("MIN_VALUE")), Number.Get("MIN_VALUE"));
      });

      It("should return the result of conversion from primitive value", () => {
        Case((_, Number.Construct()), 0);
        Case((_, Number.Construct(0)), 0);
        Case((_, Number.Construct(NaN)), NaN);
        Case((_, Number.Construct(Null)), 0);
        Case((_, Number.Construct(_)), NaN);
        Case((_, Number.Construct(true)), 1);
        Case((_, Number.Construct(false)), 0);
        Case((_, Boolean.Construct(true)), 1);
        Case((_, Boolean.Construct(false)), 0);
      });

      It("should convert to Number by explicit transformation", () => {
        Case((_, EcmaArray.Of(2, 4, 8, 16, 32)), NaN);
        Case((_, CreateObject(toString: () => "67890", valueOf: () => "[object MyObj]")), NaN);
        Case((_, CreateObject(toString: () => "67890", valueOf: () => "9876543210")), 9876543210);
        Case((_, CreateObject(toString: () => "67890")), 67890);
      });

      It("should return MV of valid numeric literal", () => {
        Case((_, "0b2"), NaN, "invalid digit");
        Case((_, "00b0"), NaN, "leading zero");
        Case((_, "0b"), NaN, "omitted digits");
        Case((_, "+0b1"), NaN, "plus sign");
        Case((_, "-0b1"), NaN, "minus sign");
        Case((_, "0b1.01"), NaN, "fractional part");
        Case((_, "0b1e10"), NaN, "exponent part");
        Case((_, "0b1e-10"), NaN, "exponent part with a minus sign");
        Case((_, "0b1e+10"), NaN, "exponent part with a plus sign");

        Case((_, "0b0"), 0, "lower-case head");
        Case((_, "0B0"), 0, "upper-case head");
        Case((_, "0b00"), 0, "lower-case head with leading zeros");
        Case((_, "0B00"), 0, "upper-case head with leading zeros");
        Case((_, "0b1"), 1, "lower-case head");
        Case((_, "0B1"), 1, "upper-case head");
        Case((_, "0b01"), 1, "lower-case head with leading zeros");
        Case((_, "0B01"), 1, "upper-case head with leading zeros");
        Case((_, "0b10"), 2, "lower-case head");
        Case((_, "0B10"), 2, "upper-case head");
        Case((_, "0b010"), 2, "lower-case head with leading zeros");
        Case((_, "0B010"), 2, "upper-case head with leading zeros");
        Case((_, "0b11"), 3, "lower-case head");
        Case((_, "0B11"), 3, "upper-case head");
        Case((_, "0b011"), 3, "lower-case head with leading zeros");
        Case((_, "0B011"), 3, "upper-case head with leading zeros");

        Case((_, "0xG"), NaN, "invalid digit");
        Case((_, "00x0"), NaN, "leading zero");
        Case((_, "0x"), NaN, "omitted digits");
        Case((_, "+0x10"), NaN, "plus sign");
        Case((_, "-0x10"), NaN, "minus sign");
        Case((_, "0x10.01"), NaN, "fractional part");
        Case((_, "0x1e-10"), NaN, "exponent part with a minus sign");
        Case((_, "0x1e+10"), NaN, "exponent part with a plus sign");

        Case((_, "0b0_1"), NaN, "0b0_1");
        Case((_, "0B0_1"), NaN, "0B0_1");
        Case((_, "0b0_10"), NaN, "0b0_10");
        Case((_, "0B0_10"), NaN, "0B0_10");
        Case((_, "0b01_0"), NaN, "0b01_0");
        Case((_, "0B01_0"), NaN, "0B01_0");
        Case((_, "0b01_00"), NaN, "0b01_00");
        Case((_, "0B01_00"), NaN, "0B01_00");

        Case((_, "1.0e-1_0"), NaN, "1.0e-1_0");
        Case((_, "1.0e-10_0"), NaN, "1.0e-10_0");
        Case((_, "1.0e+1_0"), NaN, "1.0e+1_0");
        Case((_, "1.0e+10_0"), NaN, "1.0e+10_0");

        Case((_, "1_0"), NaN, "1_0");
        Case((_, "1_1"), NaN, "1_1");
        Case((_, "1_2"), NaN, "1_2");
        Case((_, "1_3"), NaN, "1_3");
        Case((_, "1_4"), NaN, "1_4");
        Case((_, "1_5"), NaN, "1_5");
        Case((_, "1_6"), NaN, "1_6");
        Case((_, "1_7"), NaN, "1_7");
        Case((_, "1_8"), NaN, "1_8");
        Case((_, "1_9"), NaN, "1_9");

        Case((_, "10.00_01e2"), NaN, "10.00_01e2");

        Case((_, "123456789_0"), NaN, "123456789_0");
        Case((_, "123456789_1"), NaN, "123456789_1");
        Case((_, "123456789_2"), NaN, "123456789_2");
        Case((_, "123456789_3"), NaN, "123456789_3");
        Case((_, "123456789_4"), NaN, "123456789_4");
        Case((_, "123456789_5"), NaN, "123456789_5");
        Case((_, "123456789_6"), NaN, "123456789_6");
        Case((_, "123456789_7"), NaN, "123456789_7");
        Case((_, "123456789_8"), NaN, "123456789_8");
        Case((_, "123456789_9"), NaN, "123456789_9");

        Case((_, ".0_1e2"), NaN, ".0_1e2");
        Case((_, ".1_01e2"), NaN, ".1_01e2");
        Case((_, ".10_1e2"), NaN, ".10_1e2");
        Case((_, ".00_01e2"), NaN, ".00_01e2");

        Case((_, "0x0_1"), NaN, "0x0_1");
        Case((_, "0X0_1"), NaN, "0X0_1");
        Case((_, "0x0_10"), NaN, "0x0_10");
        Case((_, "0X0_10"), NaN, "0X0_10");
        Case((_, "0x01_0"), NaN, "0x01_0");
        Case((_, "0X01_0"), NaN, "0X01_0");
        Case((_, "0x01_00"), NaN, "0x01_00");
        Case((_, "0X01_00"), NaN, "0X01_00");

        Case((_, "0x0_0"), NaN, "0x0_0");
        Case((_, "0x1_1"), NaN, "0x1_1");
        Case((_, "0x2_2"), NaN, "0x2_2");
        Case((_, "0x3_3"), NaN, "0x3_3");
        Case((_, "0x4_4"), NaN, "0x4_4");
        Case((_, "0x5_5"), NaN, "0x5_5");
        Case((_, "0x6_6"), NaN, "0x6_6");
        Case((_, "0x7_7"), NaN, "0x7_7");
        Case((_, "0x8_8"), NaN, "0x8_8");
        Case((_, "0x9_9"), NaN, "0x9_9");
        Case((_, "0xa_a"), NaN, "0xa_a");
        Case((_, "0xb_b"), NaN, "0xb_b");
        Case((_, "0xc_c"), NaN, "0xc_c");
        Case((_, "0xd_d"), NaN, "0xd_d");
        Case((_, "0xe_e"), NaN, "0xe_e");
        Case((_, "0xf_f"), NaN, "0xf_f");
        Case((_, "0xA_A"), NaN, "0xA_A");
        Case((_, "0xB_B"), NaN, "0xB_B");
        Case((_, "0xC_C"), NaN, "0xC_C");
        Case((_, "0xD_D"), NaN, "0xD_D");
        Case((_, "0xE_E"), NaN, "0xE_E");
        Case((_, "0xF_F"), NaN, "0xF_F");

        Case((_, "1_0"), NaN, "1_0");
        Case((_, "1_1"), NaN, "1_1");
        Case((_, "2_2"), NaN, "2_2");
        Case((_, "3_3"), NaN, "3_3");
        Case((_, "4_4"), NaN, "4_4");
        Case((_, "5_5"), NaN, "5_5");
        Case((_, "6_6"), NaN, "6_6");
        Case((_, "7_7"), NaN, "7_7");
        Case((_, "8_8"), NaN, "8_8");
        Case((_, "9_9"), NaN, "9_9");

        Case((_, "1_0123456789"), NaN, "1_0123456789");

        Case((_, "0o0_0"), NaN, "0o0_0");
        Case((_, "0o1_1"), NaN, "0o1_1");
        Case((_, "0o2_2"), NaN, "0o2_2");
        Case((_, "0o3_3"), NaN, "0o3_3");
        Case((_, "0o4_4"), NaN, "0o4_4");
        Case((_, "0o5_5"), NaN, "0o5_5");
        Case((_, "0o6_6"), NaN, "0o6_6");
        Case((_, "0o7_7"), NaN, "0o7_7");

        Case((_, "0o0_1"), NaN, "0o0_1");
        Case((_, "0O0_1"), NaN, "0O0_1");
        Case((_, "0o0_10"), NaN, "0o0_10");
        Case((_, "0O0_10"), NaN, "0O0_10");
        Case((_, "0o01_0"), NaN, "0o01_0");
        Case((_, "0O01_0"), NaN, "0O01_0");
        Case((_, "0o01_00"), NaN, "0o01_00");
        Case((_, "0O01_00"), NaN, "0O01_00");

        Case((_, "-123456789_0"), NaN, "-123456789_0");
        Case((_, "-123456789_1"), NaN, "-123456789_1");
        Case((_, "-123456789_2"), NaN, "-123456789_2");
        Case((_, "-123456789_3"), NaN, "-123456789_3");
        Case((_, "-123456789_4"), NaN, "-123456789_4");
        Case((_, "-123456789_5"), NaN, "-123456789_5");
        Case((_, "-123456789_6"), NaN, "-123456789_6");
        Case((_, "-123456789_7"), NaN, "-123456789_7");
        Case((_, "-123456789_8"), NaN, "-123456789_8");
        Case((_, "-123456789_9"), NaN, "-123456789_9");

        Case((_, "+123456789_0"), NaN, "+123456789_0");
        Case((_, "+123456789_1"), NaN, "+123456789_1");
        Case((_, "+123456789_2"), NaN, "+123456789_2");
        Case((_, "+123456789_3"), NaN, "+123456789_3");
        Case((_, "+123456789_4"), NaN, "+123456789_4");
        Case((_, "+123456789_5"), NaN, "+123456789_5");
        Case((_, "+123456789_6"), NaN, "+123456789_6");
        Case((_, "+123456789_7"), NaN, "+123456789_7");
        Case((_, "+123456789_8"), NaN, "+123456789_8");
        Case((_, "+123456789_9"), NaN, "+123456789_9");

        Case((_, "0o8"), NaN, "invalid digit");
        Case((_, "00o0"), NaN, "leading zero");
        Case((_, "0o"), NaN, "omitted digits");
        Case((_, "+0o10"), NaN, "plus sign");
        Case((_, "-0o10"), NaN, "minus sign");
        Case((_, "0o10.01"), NaN, "fractional part");
        Case((_, "0o1e10"), NaN, "exponent part");
        Case((_, "0o1e-10"), NaN, "exponent part with a minus sign");
        Case((_, "0o1e+10"), NaN, "exponent part with a plus sign");

        Case((_, "0o0"), 0, "lower-case head");
        Case((_, "0O0"), 0, "upper-case head");
        Case((_, "0o00"), 0, "lower-case head with leading zeros");
        Case((_, "0O00"), 0, "upper-case head with leading zeros");
        Case((_, "0o1"), 1, "lower-case head");
        Case((_, "0O1"), 1, "upper-case head");
        Case((_, "0o01"), 1, "lower-case head with leading zeros");
        Case((_, "0O01"), 1, "upper-case head with leading zeros");
        Case((_, "0o7"), 7, "lower-case head");
        Case((_, "0O7"), 7, "upper-case head");
        Case((_, "0o07"), 7, "lower-case head with leading zeros");
        Case((_, "0O07"), 7, "upper-case head with leading zeros");
        Case((_, "0o10"), 8, "lower-case head");
        Case((_, "0O10"), 8, "upper-case head");
        Case((_, "0o010"), 8, "lower-case head with leading zeros");
        Case((_, "0O010"), 8, "upper-case head with leading zeros");
        Case((_, "0o11"), 9, "lower-case head");
        Case((_, "0O11"), 9, "upper-case head");
        Case((_, "0o011"), 9, "lower-case head with leading zeros");
        Case((_, "0O011"), 9, "upper-case head with leading zeros");
        Case((_, "0o77"), 63, "lower-case head");
        Case((_, "0O77"), 63, "upper-case head");
        Case((_, "0o077"), 63, "lower-case head with leading zeros");
        Case((_, "0O077"), 63, "upper-case head with leading zeros");
      });
    }

    [Test]
    public void Constants() {
      That(Number, Has.OwnProperty("MAX_SAFE_INTEGER", 9007199254740991, EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("MIN_SAFE_INTEGER", -9007199254740991, EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("NaN", NaN, EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("NEGATIVE_INFINITY", -Infinity, EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("POSITIVE_INFINITY", Infinity, EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("MAX_VALUE", EcmaPropertyAttributes.None));
      That(Number, Has.OwnProperty("MIN_VALUE", EcmaPropertyAttributes.None));
    }

    [Test]
    public void ParseFloat() {
      That(Number.Get("parseFloat") == GlobalThis.Get("parseFloat"), "The value of the Number.parseInt data property is the same built-in function object that is the value of the parseInt property of the global object");
      That(Number, Has.OwnProperty("parseFloat", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
    }

    [Test]
    public void ParseInt() {
      That(Number.Get("parseInt") == GlobalThis.Get("parseInt"), "The value of the Number.parseFloat data property is the same built-in function object that is the value of the parseFloat property of the global object");
      That(Number, Has.OwnProperty("parseInt", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void IsFinite(RuntimeFunction isFinite) {
      IsUnconstructableFunctionWLength(isFinite, "isFinite", 1);

      It("should return false if argument is not a number", () => {
        Case((_, "1"), false, "string");
        Case((_, new EcmaArray(1)), false, "[1]");
        Case((_, Number.Construct(42)), false, "Number object");
        Case((_, false), false, "false");
        Case((_, true), false, "true");
        Case((_, Undefined), false, "undefined");
        Case((_, Null), false, "null");
        Case((_, new Symbol("1")), false, "symbol");
        Case(_, false, "no arg");
      });

      It("should return true for integer", () => {
        Case((_, -10), true, "-10");
        Case((_, -0), true, "-0");
        Case((_, 0), true, "0");
        Case((_, 10), true, "10");
        Case((_, 1e10), true, "1e10");
        Case((_, 10.10), true, "10.10");
        Case((_, 9007199254740991), true, "9007199254740991");
        Case((_, -9007199254740991), true, "-9007199254740991");
        Case((_, Number.Get("MAX_VALUE")), true, "Number.MAX_VALUE");
      });

      It("should return false for Infinity", () => {
        Case((_, Infinity), false);
        Case((_, -Infinity), false);
      });

      It("should return false for NaN", () => {
        Case((_, NaN), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsInteger(RuntimeFunction isInteger) {
      IsUnconstructableFunctionWLength(isInteger, "isInteger", 1);

      It("should return false if argument is not a number", () => {
        Case((_, "1"), false, "string");
        Case((_, new EcmaArray(1)), false, "[1]");
        Case((_, Number.Construct(42)), false, "Number object");
        Case((_, false), false, "false");
        Case((_, true), false, "true");
        Case((_, Undefined), false, "undefined");
        Case((_, Null), false, "null");
        Case((_, new Symbol("1")), false, "symbol");
        Case(_, false, "no arg");
      });

      It("should return true for finite number", () => {
        Case((_, 478), true, "-10");
        Case((_, -0), true, "-0");
        Case((_, 0), true, "0");
        Case((_, -1), true, "0");
        Case((_, 9007199254740991), true, "9007199254740991");
        Case((_, -9007199254740991), true, "-9007199254740991");
        Case((_, 9007199254740992), true, "9007199254740992");
        Case((_, -9007199254740992), true, "-9007199254740992");
      });

      It("should return false for Infinity", () => {
        Case((_, Infinity), false);
        Case((_, -Infinity), false);
      });

      It("should return false for NaN", () => {
        Case((_, NaN), false);
      });

      It("should return false for non integer", () => {
        Case((_, 6.75), false);
        Case((_, 0.000001), false);
        Case((_, -0.000001), false);
        Case((_, 11e-1), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsNaN(RuntimeFunction isNaN) {
      IsUnconstructableFunctionWLength(isNaN, "isNaN", 1);

      It("should return false if argument is not a number", () => {
        Case((_, "NaN"), false, "string");
        Case((_, EcmaArray.Of(NaN)), false, "[NaN]");
        Case((_, Number.Construct(NaN)), false, "Number object");
        Case((_, false), false, "false");
        Case((_, true), false, "true");
        Case((_, Undefined), false, "undefined");
        Case((_, Null), false, "null");
        Case((_, new Symbol("1")), false, "symbol");
        Case(_, false, "no arg");
      });

      It("should return true if argument is NaN", () => {
        Case((_, NaN), true);
      });

      It("should return false if argument is a number", () => {
        Case((_, 0), false, "0");
        Case((_, -0), false, "-0");
        Case((_, 1), false, "1");
        Case((_, -1), false, "-1");
        Case((_, 1.1), false, "1.1");
        Case((_, 1e10), false, "1e10");
        Case((_, Infinity), false, "Infinity");
        Case((_, -Infinity), false, "-Infinity");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsSafeInteger(RuntimeFunction isSafeInteger) {
      IsUnconstructableFunctionWLength(isSafeInteger, "isSafeInteger", 1);

      It("should return false if argument is not a number", () => {
        Case((_, "1"), false, "string");
        Case((_, EcmaArray.Of(1)), false, "[NaN]");
        Case((_, Number.Construct(42)), false, "Number object");
        Case((_, false), false, "false");
        Case((_, true), false, "true");
        Case((_, Undefined), false, "undefined");
        Case((_, Null), false, "null");
        Case((_, new Symbol("1")), false, "symbol");
        Case(_, false, "no arg");
      });

      It("should return false for Infinity", () => {
        Case((_, Infinity), false);
        Case((_, -Infinity), false);
      });

      It("should return false for NaN", () => {
        Case((_, NaN), false);
      });

      It("should return false for non integer", () => {
        Case((_, 1.1), false);
        Case((_, 0.000001), false);
        Case((_, -0.000001), false);
        Case((_, 11e-1), false);
      });

      It("should return false for integer with magnitude >= 2^53", () => {
        Case((_, 9007199254740992), false);
        Case((_, -9007199254740992), false);
      });

      It("should return true for integer with magnitude < 2^53", () => {
        Case((_, 1), true);
        Case((_, -0), true);
        Case((_, 0), true);
        Case((_, -1), true);
        Case((_, 9007199254740991), true);
        Case((_, -9007199254740991), true);
      });
    }
  }
}
