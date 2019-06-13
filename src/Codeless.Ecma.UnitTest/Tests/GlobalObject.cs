using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class GlobalObject : TestBase {
    [Test]
    public void Properties() {
      That(GlobalThis, Has.OwnProperty("Infinity", Infinity, EcmaPropertyAttributes.None));
      That(GlobalThis, Has.OwnProperty("NaN", NaN, EcmaPropertyAttributes.None));
      That(GlobalThis, Has.OwnProperty("undefined", Undefined, EcmaPropertyAttributes.None));
    }

    [Test, RuntimeFunctionInjection]
    public void ParseInt(RuntimeFunction parseInt) {
      IsUnconstructableFunctionWLength(parseInt, "parseInt", 2);
      IsAbruptedFromToPrimitive(parseInt.Bind(_));
      IsAbruptedFromToPrimitive(parseInt.Bind(_, ""));

      It("should coerce input argument by ToPrimitive(value, String)", () => {
        Case((_, Undefined), NaN);
        Case((_, Null), NaN);
        Case((_, ""), NaN);

        Case((_, true), NaN);
        Case((_, false), NaN);
        Case((_, Boolean.Construct(true)), NaN);
        Case((_, Boolean.Construct(false)), NaN);

        Case((_, -1), -1);
        Case((_, 0), 0);
        Case((_, -0d), 0);
        Case((_, 4.7, 10), 4);
        Case((_, 4.7 * 1e22, 10), 4);
        Case((_, 0.00000000000434, 10), 4);
        Case((_, Infinity), NaN);
        Case((_, NaN), NaN);
        Case((_, Number.Construct(Infinity)), NaN);
        Case((_, Number.Construct(NaN)), NaN);

        Case((_, "+"), NaN);
        Case((_, "-"), NaN);
        Case((_, String.Construct("-1")), -1);
        Case((_, String.Construct("Infinity")), NaN);
        Case((_, String.Construct("NaN")), NaN);
        Case((_, String.Construct("false")), NaN);

        Case((_, CreateObject(valueOf: () => 1)), NaN);
        Case((_, CreateObject(toString: () => 0, valueOf: () => 1)), 0);
        Case((_, CreateObject(toString: () => new EcmaObject(), valueOf: () => 1)), 1);
      });

      It("should remove leading StrWhiteSpaceChar", () => {
        foreach (string m in new[] { "\u0009", "\u00a0", "\u000A", "\u000B", "\u000C", "\u000D", "\u0020", "\u2028", "\u2029", "\u1680", "\u2000", "\u2001", "\u2002", "\u2003", "\u2004", "\u2005", "\u2006", "\u2007", "\u2008", "\u2009", "\u200A", "\u202F", "\u205F", "\u3000" }) {
          Case((_, $"{m}1"), 1);
          Case((_, $"{m}{m}{m}1"), 1);
          Case((_, $"{m}-1"), -1);
          Case((_, $"{m}{m}{m}-1"), -1);
          Case((_, m), NaN);
        }

        Case((_, "\u180E1"), NaN, "Single leading U+180E");
        Case((_, "\u180E\u180E\u180E1"), NaN, "Multiple leading U+180E");
        Case((_, "\u180E"), NaN, "Only U+180E");
      });

      It("should coerce radix argument by ToPrimitive(value, Number)", () => {
        Case((_, 11, false), 11);
        Case((_, 11, true), NaN);
        Case((_, 11, Boolean.Construct(false)), 11);
        Case((_, 11, Boolean.Construct(true)), NaN);
        Case((_, 11, "2"), 0b11);
        Case((_, 11, "0"), 11);
        Case((_, 11, ""), 11);
        Case((_, 11, Undefined), 11);
        Case((_, 11, Null), 11);
        Case((_, 11, String.Construct("2")), 0b11);
        Case((_, 11, String.Construct("Infinity")), 11);
        Case((_, 11, Number.Construct(2)), 0b11);
        Case((_, 11, Number.Construct(Infinity)), 11);

        Case((_, 11, CreateObject(valueOf: () => 2)), 0b11);
        Case((_, 11, CreateObject(toString: () => 0, valueOf: () => 2)), 0b11);
        Case((_, 11, CreateObject(toString: () => new EcmaObject(), valueOf: () => 2)), 0b11);
      });

      It("should use base 10 if radix is 0, undefined, or infinity", () => {
        Case((_, "11", NaN), 11);
        Case((_, "11", 0), 11);
        Case((_, "11", -0d), 11);
        Case((_, "11", Infinity), 11);
        Case((_, "11", -Infinity), 11);
      });

      It("should round radix using ToInt32", () => {
        Case((_, "11", 2.1), 0b11);
        Case((_, "11", 2.5), 0b11);
        Case((_, "11", 2.9), 0b11);
        Case((_, "11", 2.0000000001), 0b11);
        Case((_, "11", 2.9999999999), 0b11);
        Case((_, "11", 4294967298), 0b11);
        Case((_, "11", 4294967296), 11);
        Case((_, "11", -2147483650), NaN);
        Case((_, "11", -4294967294), 0b11);
      });

      It("should accept radix from 2 to 36", () => {
        string digits = "123456789abcdefghijklmnopqrstuvwxyz";
        for (int r = 2; r <= 36; r++) {
          Case((_, "10", r), r);
          Case((_, new string(digits[r - 2], 1), r), r - 1);
        }
      });

      It("should return NaN if radix is < 2 or > 36", () => {
        Case((_, "0", 1), NaN);
        Case((_, "1", 1), NaN);
        Case((_, "2", 1), NaN);
        Case((_, "3", 1), NaN);
        Case((_, "4", 1), NaN);
        Case((_, "5", 1), NaN);
        Case((_, "6", 1), NaN);
        Case((_, "7", 1), NaN);
        Case((_, "8", 1), NaN);
        Case((_, "9", 1), NaN);
        Case((_, "10", 1), NaN);
        Case((_, "11", 1), NaN);

        Case((_, "0", 37), NaN);
        Case((_, "1", 37), NaN);
        Case((_, "2", 37), NaN);
        Case((_, "3", 37), NaN);
        Case((_, "4", 37), NaN);
        Case((_, "5", 37), NaN);
        Case((_, "6", 37), NaN);
        Case((_, "7", 37), NaN);
        Case((_, "8", 37), NaN);
        Case((_, "9", 37), NaN);
        Case((_, "10", 37), NaN);
        Case((_, "11", 37), NaN);
      });

      It("should accept hexadecimal but not octal", () => {
        Case((_, "0x0", 0), 0x0);
        Case((_, "0x1", 0), 0x1);
        Case((_, "0x2", 0), 0x2);
        Case((_, "0x3", 0), 0x3);
        Case((_, "0x4", 0), 0x4);
        Case((_, "0x5", 0), 0x5);
        Case((_, "0x6", 0), 0x6);
        Case((_, "0x7", 0), 0x7);
        Case((_, "0x8", 0), 0x8);
        Case((_, "0x9", 0), 0x9);
        Case((_, "0xA", 0), 0xA);
        Case((_, "0xB", 0), 0xB);
        Case((_, "0xC", 0), 0xC);
        Case((_, "0xD", 0), 0xD);
        Case((_, "0xE", 0), 0xE);
        Case((_, "0xF", 0), 0xF);

        Case((_, "0X0", 0), 0x0);
        Case((_, "0X1", 0), 0x1);
        Case((_, "0X2", 0), 0x2);
        Case((_, "0X3", 0), 0x3);
        Case((_, "0X4", 0), 0x4);
        Case((_, "0X5", 0), 0x5);
        Case((_, "0X6", 0), 0x6);
        Case((_, "0X7", 0), 0x7);
        Case((_, "0X8", 0), 0x8);
        Case((_, "0X9", 0), 0x9);
        Case((_, "0XA", 0), 0xA);
        Case((_, "0XB", 0), 0xB);
        Case((_, "0XC", 0), 0xC);
        Case((_, "0XD", 0), 0xD);
        Case((_, "0XE", 0), 0xE);
        Case((_, "0XF", 0), 0xF);

        Case((_, "010"), 10);
      });

      It("should convert substring of S consisting of all characters before the first non radix-R digit", () => {
        Case((_, "0123456789", 2), 1);
        Case((_, "01234567890", 3), 5);
        Case((_, "01234567890", 4), 27);
        Case((_, "01234567890", 5), 194);
        Case((_, "01234567890", 6), 1865);
        Case((_, "01234567890", 7), 22875);
        Case((_, "01234567890", 8), 342391);
        Case((_, "01234567890", 9), 6053444);
        Case((_, "01234567890", 10), 1234567890);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ParseFloat(RuntimeFunction parseFloat) {
      IsUnconstructableFunctionWLength(parseFloat, "parseFloat", 1);
      IsAbruptedFromToPrimitive(parseFloat.Bind(_));

      It("should coerce input argument by ToPrimitive(value, String)", () => {
        Case((_, Undefined), NaN);
        Case((_, Null), NaN);
        Case((_, ""), NaN);

        Case((_, true), NaN);
        Case((_, false), NaN);
        Case((_, Boolean.Construct(true)), NaN);
        Case((_, Boolean.Construct(false)), NaN);

        Case((_, -1.1), -1.1);
        Case((_, 0), 0);
        Case((_, -0d), 0);
        Case((_, 0.01e+2), 0.01e+2);
        Case((_, Infinity), Infinity);
        Case((_, NaN), NaN);
        Case((_, Number.Construct(-1.1)), -1.1);
        Case((_, Number.Construct(0.01e+2)), 0.01e+2);
        Case((_, Number.Construct(Infinity)), Infinity);
        Case((_, Number.Construct(NaN)), NaN);

        Case((_, "+"), NaN);
        Case((_, "-"), NaN);
        Case((_, String.Construct("-1.1")), -1.1);
        Case((_, String.Construct("0.01e+2")), 0.01e+2);
        Case((_, String.Construct("Infinity")), Infinity);
        Case((_, String.Construct("NaN")), NaN);
        Case((_, String.Construct("false")), NaN);

        Case((_, CreateObject(valueOf: () => 1)), NaN);
        Case((_, CreateObject(toString: () => 0, valueOf: () => 1)), 0);
        Case((_, CreateObject(toString: () => new EcmaObject(), valueOf: () => 1)), 1);
      });

      It("should remove leading StrWhiteSpaceChar", () => {
        foreach (string m in new[] { "\u0009", "\u00a0", "\u000A", "\u000B", "\u000C", "\u000D", "\u0020", "\u2028", "\u2029", "\u1680", "\u2000", "\u2001", "\u2002", "\u2003", "\u2004", "\u2005", "\u2006", "\u2007", "\u2008", "\u2009", "\u200A", "\u202F", "\u205F", "\u3000" }) {
          Case((_, $"{m}1.1"), 1.1);
          Case((_, $"{m}{m}{m}1.1"), 1.1);
          Case((_, $"{m}-1.1"), -1.1);
          Case((_, $"{m}{m}{m}-1.1"), -1.1);
          Case((_, m), NaN);
        }

        Case((_, "\u180E1.1"), NaN, "Single leading U+180E");
        Case((_, "\u180E\u180E\u180E1.1"), NaN, "Multiple leading U+180E");
        Case((_, "\u180E"), NaN, "Only U+180E");
      });

      It("should return NaN for wrong number format", () => {
        Case((_, ".x"), NaN, ".x");
        Case((_, "+x"), NaN, "+x");
        Case((_, "infinity"), NaN, "infinity");
        Case((_, "A"), NaN, "A");
        Case((_, "e1"), NaN, "e1");
        Case((_, "e-1"), NaN, "e-1");
        Case((_, "E+1"), NaN, "E+1");
        Case((_, "E0"), NaN, "E0");
        Case((_, "-.e-1"), NaN, "-.e-1");
        Case((_, ".e1"), NaN, ".e1");
      });

      It("should convert substring of S consisting of all characters that is the production of correct number format", () => {
        Case((_, "0x"), 0);
        Case((_, "11x"), 11);
        Case((_, "11s1"), 11);
        Case((_, "11.s1"), 11);
        Case((_, ".0s1"), 0);
        Case((_, "1.s1"), 1);
        Case((_, "1..1"), 1);
        Case((_, "0.1.1"), 0.1);
        Case((_, "0. 1"), 0);

        Case((_, "1ex"), 1);
        Case((_, "1e-x"), 1);
        Case((_, "1e1x"), 10);
        Case((_, "1e-1x"), 0.1);
        Case((_, "0.1e-1x"), 0.01);

        Case((_, "-11.string"), -11);
        Case((_, "01.string"), 1);
        Case((_, "+11.1string"), 11.1);
        Case((_, "01.1string"), 1.1);
        Case((_, "-11.e-1string"), -1.1);
        Case((_, "01.e1string"), 10);
        Case((_, "+11.22e-1string"), 1.122);
        Case((_, "01.01e1string"), 10.1);
        Case((_, "001.string"), 1);
        Case((_, "010.string"), 10);

        Case((_, "+.1string"), 0.1);
        Case((_, ".01string"), 0.01);
        Case((_, "+.22e-1string"), 0.022);
        Case((_, "-11string"), -11);
        Case((_, "01string"), 1);
        Case((_, "-11e-1string"), -1.1);
        Case((_, "01e1string"), 10);
        Case((_, "001string"), 1);
        Case((_, "1e001string"), 10);
        Case((_, "010string"), 10);

        Case((_, "-11."), -11);
        Case((_, "01."), 1);
        Case((_, "+11.1"), 11.1);
        Case((_, "01.1"), 1.1);
        Case((_, "-11.e-1"), -1.1);
        Case((_, "01.e1"), 10);
        Case((_, "+11.22e-1"), 1.122);
        Case((_, "01.01e1"), 10.1);
        Case((_, "001."), 1);
        Case((_, "010."), 10);

        Case((_, "+.1"), 0.1);
        Case((_, ".01"), 0.01);
        Case((_, "+.22e-1"), 0.022);

        Case((_, "-11"), -11);
        Case((_, "01"), 1);
        Case((_, "-11e-1"), -1.1);
        Case((_, "01e1"), 10);
        Case((_, "001"), 1);
        Case((_, "1e001"), 10);
        Case((_, "010"), 10);
      });

      It("should not recognize hexadecimal interger", () => {
        Case((_, "0x0"), 0);
        Case((_, "0x1"), 0);
        Case((_, "0x2"), 0);
        Case((_, "0x3"), 0);
        Case((_, "0x4"), 0);
        Case((_, "0x5"), 0);
        Case((_, "0x6"), 0);
        Case((_, "0x7"), 0);
        Case((_, "0x8"), 0);
        Case((_, "0x9"), 0);
        Case((_, "0xA"), 0);
        Case((_, "0xB"), 0);
        Case((_, "0xC"), 0);
        Case((_, "0xD"), 0);
        Case((_, "0xE"), 0);
        Case((_, "0xF"), 0);
      });

      It("should recognize Infinity", () => {
        Case((_, "Infinity1"), Infinity);
        Case((_, "Infinityx"), Infinity);
        Case((_, "Infinity+1"), Infinity);
        Case((_, "+Infinity"), Infinity);
        Case((_, "-Infinity"), -Infinity);
      });

      It("should not recognize digit separator as part of a valid decimal number production", () => {
        Case((_, "1.0e-1_0"), 1.0e-1);
        Case((_, "1.0e-10_0"), 1.0e-10);
        Case((_, "1.0e+1_0"), 1.0e+1);
        Case((_, "1.0e+10_0"), 1.0e+10);

        Case((_, "1_0"), 1);
        Case((_, "1_1"), 1);
        Case((_, "1_2"), 1);
        Case((_, "1_3"), 1);
        Case((_, "1_4"), 1);
        Case((_, "1_5"), 1);
        Case((_, "1_6"), 1);
        Case((_, "1_7"), 1);
        Case((_, "1_8"), 1);
        Case((_, "1_9"), 1);

        Case((_, "10.00_01e2"), 10.00);

        Case((_, "123456789_0"), 123456789);
        Case((_, "123456789_1"), 123456789);
        Case((_, "123456789_2"), 123456789);
        Case((_, "123456789_3"), 123456789);
        Case((_, "123456789_4"), 123456789);
        Case((_, "123456789_5"), 123456789);
        Case((_, "123456789_6"), 123456789);
        Case((_, "123456789_7"), 123456789);
        Case((_, "123456789_8"), 123456789);
        Case((_, "123456789_9"), 123456789);

        Case((_, ".0_1e2"), .0);
        Case((_, ".1_01e2"), .1);
        Case((_, ".10_1e2"), .10);
        Case((_, ".00_01e2"), .00);

        Case((_, "1_0"), 1);
        Case((_, "1_1"), 1);
        Case((_, "2_2"), 2);
        Case((_, "3_3"), 3);
        Case((_, "4_4"), 4);
        Case((_, "5_5"), 5);
        Case((_, "6_6"), 6);
        Case((_, "7_7"), 7);
        Case((_, "8_8"), 8);
        Case((_, "9_9"), 9);
        Case((_, "1_1"), 1);
        Case((_, "1_0123456789"), 1);

        Case((_, "+123456789_0"), +123456789);
        Case((_, "+123456789_1"), +123456789);
        Case((_, "+123456789_2"), +123456789);
        Case((_, "+123456789_3"), +123456789);
        Case((_, "+123456789_4"), +123456789);
        Case((_, "+123456789_5"), +123456789);
        Case((_, "+123456789_6"), +123456789);
        Case((_, "+123456789_7"), +123456789);
        Case((_, "+123456789_8"), +123456789);
        Case((_, "+123456789_9"), +123456789);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsNaN(RuntimeFunction isNaN) {
      IsUnconstructableFunctionWLength(isNaN, "isNaN", 1);
      IsAbruptedFromToPrimitive(isNaN.Bind(_));
      IsAbruptedFromSymbolToNumber(isNaN.Bind(_));

      It("should return false for number other than NaN", () => {
        Case((_, NaN), true);
        Case((_, 0), false, "0");
        Case((_, -0), false, "-0");
        Case((_, 1L << 53), false, "Math.pow(2, 53)");
        Case((_, -(1L << 53)), false, "-Math.pow(2, 53)");
        Case((_, 1), false, "1");
        Case((_, -1), false, "-1");
        Case((_, 0.000001), false, "0.000001");
        Case((_, -0.000001), false, "-0.000001");
        Case((_, 1e42), false, "1e42");
        Case((_, -1e42), false, "-1e42");
        Case((_, Infinity), false, "Infinity");
        Case((_, -Infinity), false, "-Infinity");
      });

      It("should convert argument by ToNumber", () => {
        Case((_, "0"), false, "'0'");
        Case((_, ""), false, "the empty string");
        Case((_, "Infinity"), false, "'Infinity'");
        Case((_, "this is not a number"), true, "string");
        Case((_, true), false, "true");
        Case((_, false), false, "false");
        Case((_, EcmaArray.Of(1)), false, "Object [1]");
        Case((_, EcmaArray.Of(Infinity)), false, "Object [Infinity]");
        Case((_, EcmaArray.Of(NaN)), true, "Object [NaN]");
        Case((_, Null), false, "null");
        Case((_, Undefined), true, "undefined");
        Case(_, true, "no arg");

        Case((_, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262);
        Case((_, CreateObject(toPrimitive: () => EcmaArray.Of(42))), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => Symbol.ToPrimitive)), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, get: ThrowTest262Exception, set: null))), Throws.Test262);
        Case((_, CreateObject((Symbol.ToPrimitive, 42))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, ""))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, true))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, Symbol.ToPrimitive))), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsFinite(RuntimeFunction isFinite) {
      IsUnconstructableFunctionWLength(isFinite, "isFinite", 1);
      IsAbruptedFromToPrimitive(isFinite.Bind(_));
      IsAbruptedFromSymbolToNumber(isFinite.Bind(_));

      It("should return false for number other than NaN or Infinity", () => {
        Case((_, NaN), false);
        Case((_, 0), true, "0");
        Case((_, -0), true, "-0");
        Case((_, 1L << 53), true, "Math.pow(2, 53)");
        Case((_, -(1L << 53)), true, "-Math.pow(2, 53)");
        Case((_, 1), true, "1");
        Case((_, -1), true, "-1");
        Case((_, 0.000001), true, "0.000001");
        Case((_, -0.000001), true, "-0.000001");
        Case((_, 1e42), true, "1e42");
        Case((_, -1e42), true, "-1e42");
        Case((_, Infinity), false, "Infinity");
        Case((_, -Infinity), false, "-Infinity");
      });

      It("should convert argument by ToNumber", () => {
        Case((_, "0"), true, "'0'");
        Case((_, ""), true, "the empty string");
        Case((_, "Infinity"), false, "'Infinity'");
        Case((_, "this is not a number"), false, "string");
        Case((_, true), true, "true");
        Case((_, false), true, "false");
        Case((_, EcmaArray.Of(1)), true, "Object [1]");
        Case((_, EcmaArray.Of(Infinity)), false, "Object [Infinity]");
        Case((_, EcmaArray.Of(NaN)), false, "Object [NaN]");
        Case((_, Null), true, "null");
        Case((_, Undefined), false, "undefined");
        Case(_, false, "no arg");

        Case((_, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262);
        Case((_, CreateObject(toPrimitive: () => EcmaArray.Of(42))), Throws.TypeError);
        Case((_, CreateObject(toPrimitive: () => Symbol.ToPrimitive)), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, get: ThrowTest262Exception, set: null))), Throws.Test262);
        Case((_, CreateObject((Symbol.ToPrimitive, 42))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, ""))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, true))), Throws.TypeError);
        Case((_, CreateObject((Symbol.ToPrimitive, Symbol.ToPrimitive))), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void DecodeURI(RuntimeFunction decodeURI) {
      IsUnconstructableFunctionWLength(decodeURI, "decodeURI", 1);
      IsAbruptedFromToPrimitive(decodeURI.Bind(_));

      Case((_, "%3B"), "%3B");
      Case((_, "%2F"), "%2F");
      Case((_, "%3F"), "%3F");
      Case((_, "%3A"), "%3A");
      Case((_, "%40"), "%40");
      Case((_, "%26"), "%26");
      Case((_, "%3D"), "%3D");
      Case((_, "%2B"), "%2B");
      Case((_, "%24"), "%24");
      Case((_, "%2C"), "%2C");
      Case((_, "%23"), "%23");

      Case((_, "%3b"), "%3b");
      Case((_, "%2f"), "%2f");
      Case((_, "%3f"), "%3f");
      Case((_, "%3a"), "%3a");
      Case((_, "%40"), "%40");
      Case((_, "%26"), "%26");
      Case((_, "%3d"), "%3d");
      Case((_, "%2b"), "%2b");
      Case((_, "%24"), "%24");
      Case((_, "%2c"), "%2c");
      Case((_, "%23"), "%23");

      Case((_, "%3B%2F%3F%3A%40%26%3D%2B%24%2C%23"), "%3B%2F%3F%3A%40%26%3D%2B%24%2C%23");
      Case((_, "%3b%2f%3f%3a%40%26%3d%2b%24%2c%23"), "%3b%2f%3f%3a%40%26%3d%2b%24%2c%23");

      Case((_, "http://unipro.ru/0123456789"), "http://unipro.ru/0123456789");
      Case((_, "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A"), "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
      Case((_, "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A"), "abcdefghijklmnopqrstuvwxyz");

      Case((_, "http://ru.wikipedia.org/wiki/%d0%ae%D0%bd%D0%B8%D0%BA%D0%BE%D0%B4"), "http://ru.wikipedia.org/wiki/Юникод");
      Case((_, "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4#%D0%A1%D1%81%D1%8B%D0%BB%D0%BA%D0%B8"), "http://ru.wikipedia.org/wiki/Юникод#Ссылки");
      Case((_, "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%23%D0%92%D0%B5%D1%80%D1%81%D0%B8%D0%B8%20%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%D0%B0"), "http://ru.wikipedia.org/wiki/Юникод%23Версии Юникода");

      Case((_, "http://unipro.ru/%0Aabout"), "http://unipro.ru/\nabout");
      Case((_, "http://unipro.ru/%0Babout"), "http://unipro.ru/\vabout");
      Case((_, "http://unipro.ru/%0Cabout"), "http://unipro.ru/\fabout");
      Case((_, "http://unipro.ru/%0Dabout"), "http://unipro.ru/\rabout");

      Case((_, ""), "");
      Case((_, "http:%2f%2Funipro.ru"), "http:%2f%2Funipro.ru");
      Case((_, "http://www.google.ru/support/jobs/bin/static.py%3Fpage%3dwhy-ru.html%26sid%3Dliveandwork"), "http://www.google.ru/support/jobs/bin/static.py%3Fpage%3dwhy-ru.html%26sid%3Dliveandwork");
      Case((_, "http://en.wikipedia.org/wiki/UTF-8%23Description"), "http://en.wikipedia.org/wiki/UTF-8%23Description");

      Case((_, "%80"), Throws.URIError, "Incomplete UTF-8 doublet");
      Case((_, "%ZZ"), Throws.URIError, "Incomplete escape sequence");
      Case((_, "%8"), Throws.URIError, "Incomplete escape sequence");
      Case((_, "%"), Throws.URIError, "Incomplete escape sequence");
      Case((_, "%C2%80"), "\u0080");
      Case((_, "%DF%BF"), "\u07ff");
      Case((_, "%C0%80"), Throws.URIError, "Invalid UTF-8 doublet, should be singlet");
      Case((_, "%C1%FF"), Throws.URIError, "Invalid UTF-8 doublet, should be singlet");
      Case((_, "%E0%A0"), Throws.URIError, "Incomplete UTF-8 triplet");
      Case((_, "%E0%A0%80"), "\u0800");
      Case((_, "%ED%9F%BF"), "\ud7ff");
      Case((_, "%EE%80%80"), "\ue000");
      Case((_, "%EF%BF%BF"), "\uffff");
      Case((_, "%E0%80%80"), Throws.URIError, "Invalid UTF-8 triplet, should be singlet or doublet");
      Case((_, "%E0%9F%BF"), Throws.URIError, "Invalid UTF-8 triplet, should be singlet or doublet");
      Case((_, "%ED%A0%80"), Throws.URIError, "Surrogate");
      Case((_, "%ED%BF%BF"), Throws.URIError, "Surrogate");
      Case((_, "%F0%90%80"), Throws.URIError, "Incomplete UTF-8 quartet");
      Case((_, "%F0%90%80%80"), "\ud800\udc00");
      Case((_, "%F4%8F%BF%BF"), "\udbff\udfff");
    }

    [Test, RuntimeFunctionInjection]
    public void DecodeURIComponent(RuntimeFunction decodeURIComponent) {
      IsUnconstructableFunctionWLength(decodeURIComponent, "decodeURIComponent", 1);
      IsAbruptedFromToPrimitive(decodeURIComponent.Bind(_));

      Case((_, "%3B"), ";");
      Case((_, "%2F"), "/");
      Case((_, "%3F"), "?");
      Case((_, "%3A"), ":");
      Case((_, "%40"), "@");
      Case((_, "%26"), "&");
      Case((_, "%3D"), "=");
      Case((_, "%2B"), "+");
      Case((_, "%24"), "$");
      Case((_, "%2C"), ",");
      Case((_, "%23"), "#");

      Case((_, "%3b"), ";");
      Case((_, "%2f"), "/");
      Case((_, "%3f"), "?");
      Case((_, "%3a"), ":");
      Case((_, "%40"), "@");
      Case((_, "%26"), "&");
      Case((_, "%3d"), "=");
      Case((_, "%2b"), "+");
      Case((_, "%24"), "$");
      Case((_, "%2c"), ",");
      Case((_, "%23"), "#");

      Case((_, "%3B%2F%3F%3A%40%26%3D%2B%24%2C%23"), ";/?:@&=+$,#");
      Case((_, "%3b%2f%3f%3a%40%26%3d%2b%24%2c%23"), ";/?:@&=+$,#");

      Case((_, "http://unipro.ru/0123456789"), "http://unipro.ru/0123456789");
      Case((_, "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A"), "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
      Case((_, "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A"), "abcdefghijklmnopqrstuvwxyz");

      Case((_, "http://ru.wikipedia.org/wiki/%d0%ae%D0%bd%D0%B8%D0%BA%D0%BE%D0%B4"), "http://ru.wikipedia.org/wiki/Юникод");
      Case((_, "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4#%D0%A1%D1%81%D1%8B%D0%BB%D0%BA%D0%B8"), "http://ru.wikipedia.org/wiki/Юникод#Ссылки");
      Case((_, "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%23%D0%92%D0%B5%D1%80%D1%81%D0%B8%D0%B8%20%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%D0%B0"), "http://ru.wikipedia.org/wiki/Юникод#Версии Юникода");

      Case((_, "http://unipro.ru/%0Aabout"), "http://unipro.ru/\nabout");
      Case((_, "http://unipro.ru/%0Babout"), "http://unipro.ru/\vabout");
      Case((_, "http://unipro.ru/%0Cabout"), "http://unipro.ru/\fabout");
      Case((_, "http://unipro.ru/%0Dabout"), "http://unipro.ru/\rabout");

      Case((_, ""), "");
      Case((_, "http://unipro.ru"), "http://unipro.ru");
      Case((_, "http:%2f%2Fwww.google.ru/support/jobs/bin/static.py%3Fpage%3dwhy-ru.html%26sid%3Dliveandwork"), "http://www.google.ru/support/jobs/bin/static.py?page=why-ru.html&sid=liveandwork");
      Case((_, "http:%2F%2Fen.wikipedia.org/wiki/UTF-8%23Description"), "http://en.wikipedia.org/wiki/UTF-8#Description");
    }

    [Test, RuntimeFunctionInjection]
    public void EncodeURI(RuntimeFunction encodeURI) {
      IsUnconstructableFunctionWLength(encodeURI, "encodeURI", 1);
      IsAbruptedFromToPrimitive(encodeURI.Bind(_));

      Case((_, ";"), ";");
      Case((_, "/"), "/");
      Case((_, "?"), "?");
      Case((_, ":"), ":");
      Case((_, "@"), "@");
      Case((_, "&"), "&");
      Case((_, "="), "=");
      Case((_, "+"), "+");
      Case((_, "$"), "$");
      Case((_, ","), ",");
      Case((_, "#"), "#");

      Case((_, "-"), "-");
      Case((_, "_"), "_");
      Case((_, "."), ".");
      Case((_, "!"), "!");
      Case((_, "~"), "~");
      Case((_, "*"), "*");
      Case((_, "'"), "'");
      Case((_, "("), "(");
      Case((_, ")"), ")");

      Case((_, "http://unipro.ru/0123456789"), "http://unipro.ru/0123456789");
      Case((_, "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ"), "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ");
      Case((_, "aA_bB-cC.dD!eE~fF*gG'hH(iI)jJ;kK/lL?mM:nN@oO&pP=qQ+rR$sS,tT9uU8vV7wW6xX5yY4zZ"), "aA_bB-cC.dD!eE~fF*gG'hH(iI)jJ;kK/lL?mM:nN@oO&pP=qQ+rR$sS,tT9uU8vV7wW6xX5yY4zZ");

      Case((_, "http://ru.wikipedia.org/wiki/Юникод"), "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4");
      Case((_, "http://ru.wikipedia.org/wiki/Юникод#Ссылки"), "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4#%D0%A1%D1%81%D1%8B%D0%BB%D0%BA%D0%B8");
      Case((_, "http://ru.wikipedia.org/wiki/Юникод#Версии Юникода"), "http://ru.wikipedia.org/wiki/%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4#%D0%92%D0%B5%D1%80%D1%81%D0%B8%D0%B8%20%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%D0%B0");

      Case((_, "http://unipro.ru/\nabout"), "http://unipro.ru/%0Aabout");
      Case((_, "http://unipro.ru/\vabout"), "http://unipro.ru/%0Babout");
      Case((_, "http://unipro.ru/\fabout"), "http://unipro.ru/%0Cabout");
      Case((_, "http://unipro.ru/\rabout"), "http://unipro.ru/%0Dabout");

      Case((_, ""), "");
      Case((_, "http://unipro.ru"), "http://unipro.ru");
      Case((_, "http://www.google.ru/support/jobs/bin/static.py?page=why-ru.html&sid=liveandwork"), "http://www.google.ru/support/jobs/bin/static.py?page=why-ru.html&sid=liveandwork");
      Case((_, "http://en.wikipedia.org/wiki/UTF-8#Description"), "http://en.wikipedia.org/wiki/UTF-8#Description");

      Case((_, "\ud800"), Throws.URIError);
      Case((_, "\udfff"), Throws.URIError);
      Case((_, "\udfff\ud800"), Throws.URIError);
    }

    [Test, RuntimeFunctionInjection]
    public void EncodeURIComponent(RuntimeFunction encodeURIComponent) {
      IsUnconstructableFunctionWLength(encodeURIComponent, "encodeURIComponent", 1);
      IsAbruptedFromToPrimitive(encodeURIComponent.Bind(_));

      Case((_, ";"), "%3B");
      Case((_, "/"), "%2F");
      Case((_, "?"), "%3F");
      Case((_, ":"), "%3A");
      Case((_, "@"), "%40");
      Case((_, "&"), "%26");
      Case((_, "="), "%3D");
      Case((_, "+"), "%2B");
      Case((_, "$"), "%24");
      Case((_, ","), "%2C");
      Case((_, "#"), "%23");

      Case((_, "http://unipro.ru/0123456789"), "http%3A%2F%2Funipro.ru%2F0123456789");
      Case((_, "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ"), "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ");
      Case((_, ";/?:@&=+$,"), "%3B%2F%3F%3A%40%26%3D%2B%24%2C");

      Case((_, "http://ru.wikipedia.org/wiki/Юникод"), "http%3A%2F%2Fru.wikipedia.org%2Fwiki%2F%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4");
      Case((_, "http://ru.wikipedia.org/wiki/Юникод#Ссылки"), "http%3A%2F%2Fru.wikipedia.org%2Fwiki%2F%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%23%D0%A1%D1%81%D1%8B%D0%BB%D0%BA%D0%B8");
      Case((_, "http://ru.wikipedia.org/wiki/Юникод#Версии Юникода"), "http%3A%2F%2Fru.wikipedia.org%2Fwiki%2F%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%23%D0%92%D0%B5%D1%80%D1%81%D0%B8%D0%B8%20%D0%AE%D0%BD%D0%B8%D0%BA%D0%BE%D0%B4%D0%B0");

      Case((_, "http://unipro.ru/\nabout"), "http%3A%2F%2Funipro.ru%2F%0Aabout");
      Case((_, "http://unipro.ru/\vabout"), "http%3A%2F%2Funipro.ru%2F%0Babout");
      Case((_, "http://unipro.ru/\fabout"), "http%3A%2F%2Funipro.ru%2F%0Cabout");
      Case((_, "http://unipro.ru/\rabout"), "http%3A%2F%2Funipro.ru%2F%0Dabout");

      Case((_, ""), "");
      Case((_, "http://unipro.ru"), "http%3A%2F%2Funipro.ru");
      Case((_, "http://www.google.ru/support/jobs/bin/static.py?page=why-ru.html&sid=liveandwork"), "http%3A%2F%2Fwww.google.ru%2Fsupport%2Fjobs%2Fbin%2Fstatic.py%3Fpage%3Dwhy-ru.html%26sid%3Dliveandwork");
      Case((_, "http://en.wikipedia.org/wiki/UTF-8#Description"), "http%3A%2F%2Fen.wikipedia.org%2Fwiki%2FUTF-8%23Description");

      Case((_, "\ud800"), Throws.URIError);
      Case((_, "\udfff"), Throws.URIError);
      Case((_, "\udfff\ud800"), Throws.URIError);
    }

    [Test, RuntimeFunctionInjection]
    public void Escape(RuntimeFunction escape) {
      IsUnconstructableFunctionWLength(escape, "escape", 1);
      IsAbruptedFromToPrimitive(escape.Bind(_));

      Case((_, "abc123"), "abc123");
      Case((_, "äöü"), "%E4%F6%FC");
      Case((_, "ć"), "%u0107");
      Case((_, "@*_+-./"), "@*_+-./");
    }

    [Test, RuntimeFunctionInjection]
    public void Unescape(RuntimeFunction unescape) {
      IsUnconstructableFunctionWLength(unescape, "unescape", 1);
      IsAbruptedFromToPrimitive(unescape.Bind(_));

      Case((_, "abc123"), "abc123");
      Case((_, "%E4%F6%FC"), "äöü");
      Case((_, "%u0107"), "ć");
    }
  }
}
