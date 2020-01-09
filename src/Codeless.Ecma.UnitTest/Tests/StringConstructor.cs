using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class StringConstructor : TestBase {
    [Test]
    public void Instance() {
      It("should support indexing notation to look up non numeric property names", () => {
        EcmaValue str = "hello world";
        That(str[0], Is.EqualTo("h"));
        That(str["foo"], Is.Undefined);
        That(str[-1], Is.Undefined);
        That(str[11], Is.Undefined);
        That(str[NaN], Is.Undefined);
        That(str[Infinity], Is.Undefined);

        EcmaValue strObj = String.Construct(str);
        strObj["foo"] = 1;
        That(strObj["foo"], Is.EqualTo(1));

        EcmaValue strObj2 = String.Construct(str);
        That(strObj2["foo"], Is.Undefined);
      });

      It("should have the \"length\" property", () => {
        That(String.Construct(""), Has.OwnProperty("length", 0, EcmaPropertyAttributes.None));
        That(String.Construct(" "), Has.OwnProperty("length", 1, EcmaPropertyAttributes.None));
        That(String.Construct(" \b "), Has.OwnProperty("length", 3, EcmaPropertyAttributes.None));
        That(String.Construct("\ud834\udf06"), Has.OwnProperty("length", 2, EcmaPropertyAttributes.None));
      });

      It("should have property descriptor for numeric own properties of an exotic String object", () => {
        EcmaValue str = String.Construct("abc");
        That(str, Has.OwnProperty("0", "a", EcmaPropertyAttributes.Enumerable));
        That(str, Has.OwnProperty("1", "b", EcmaPropertyAttributes.Enumerable));
        That(str, Has.OwnProperty("2", "c", EcmaPropertyAttributes.Enumerable));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "String", 1, String.Prototype);

      It("should convert primitive value to a string", () => {
        Case((_, NaN), "NaN");
        Case((_, Number.Construct("asasa")), "NaN");
        Case((_, 1.2345), "1.2345");
        Case((_, 1.234567890), "1.23456789");
        Case((_, 0.12345), "0.12345");
        Case((_, .012345), "0.012345");
        Case((_, .0012345), "0.0012345");
        Case((_, .00012345), "0.00012345");
        Case((_, .000012345), "0.000012345");
        Case((_, .0000012345), "0.0000012345");
        Case((_, .00000012345), "1.2345e-7");
        Case((_, +0d), "0");
        Case((_, -0d), "0");
        Case((_, -1234567890), "-1234567890");
        Case((_, Infinity), "Infinity");
        Case((_, Number.Get("POSITIVE_INFINITY")), "Infinity");
        Case((_, -Infinity), "-Infinity");
        Case((_, Number.Get("NEGATIVE_INFINITY")), "-Infinity");
        Case((_, 1), "1");
        Case((_, 10), "10");
        Case((_, 100), "100");
        Case((_, 100000000000000000000d), "100000000000000000000");
        Case((_, 1e20), "100000000000000000000");
        Case((_, 12345), "12345");
        Case((_, 12345000), "12345000");
        Case((_, -1), "-1");
        Case((_, -10), "-10");
        Case((_, -100), "-100");
        Case((_, -100000000000000000000d), "-100000000000000000000");
        Case((_, -1e20), "-100000000000000000000");
        Case((_, -12345), "-12345");
        Case((_, -12345000), "-12345000");
        Case((_, 1E20), "100000000000000000000");
        Case((_, -1E20), "-100000000000000000000");
        Case((_, 1.0000001), "1.0000001");
        Case((_, -1.0000001), "-1.0000001");
        Case((_, 0.1), "0.1");
        Case((_, 0.000001), "0.000001");
        Case((_, 1e-6), "0.000001");
        Case((_, 1E-6), "0.000001");
        Case((_, -0.1), "-0.1");
        Case((_, -0.000001), "-0.000001");
        Case((_, -1e-6), "-0.000001");
        Case((_, -1E-6), "-0.000001");
        Case((_, 1000000000000000000000d), "1e+21");
        Case((_, 10000000000000000000000d), "1e+22");
        Case((_, 1e21), "1e+21");
        Case((_, 1.0e22), "1e+22");
        Case((_, 1E21), "1e+21");
        Case((_, 1.0E22), "1e+22");
        Case((_, -1000000000000000000000d), "-1e+21");
        Case((_, -10000000000000000000000d), "-1e+22");
        Case((_, -1e21), "-1e+21");
        Case((_, -1.0e22), "-1e+22");
        Case((_, -1E21), "-1e+21");
        Case((_, -1.0E22), "-1e+22");
        Case((_, 0.0000001), "1e-7");
        Case((_, 0.000000000100000000000), "1e-10");
        Case((_, 1e-7), "1e-7");
        Case((_, 1.0e-10), "1e-10");
        Case((_, 1E-7), "1e-7");
        Case((_, 1.0E-10), "1e-10");
        Case((_, -0.0000001), "-1e-7");
        Case((_, -0.000000000100000000000), "-1e-10");
        Case((_, -1e-7), "-1e-7");
        Case((_, -1.0e-10), "-1e-10");
        Case((_, -1E-7), "-1e-7");
        Case((_, -1.0E-10), "-1e-10");
        Case((_, Undefined), "undefined");
        Case((_, Null), "null");
        Case((_, false), "false");
        Case((_, true), "true");
      });

      It("should return the same string value when the input argument is a string", () => {
        Case((_, "abc"), "abc");
        Case((_, "abc"), Is.TypeOf("string"));
      });

      It("should call ToPrimitive with hint String", () => {
        Case((_, Number.Construct()), "0");
        Case((_, Number.Construct(0)), "0");
        Case((_, Number.Construct(NaN)), "NaN");
        Case((_, Number.Construct(Null)), "0");
        Case((_, Number.Construct(Undefined)), "NaN");
        Case((_, Number.Construct(true)), "1");
        Case((_, Number.Construct(false)), "0");
        Case((_, Boolean.Construct(true)), "true");
        Case((_, Boolean.Construct(false)), "false");
        Case((_, EcmaArray.Of(2, 4, 8, 16, 32)), "2,4,8,16,32");

        EcmaValue myobj1 = CreateObject(toString: () => 67890, valueOf: () => "[object MyObj]");
        Case((_, myobj1), "67890");

        EcmaValue myobj2 = CreateObject(toString: () => new EcmaObject(), valueOf: () => "[object MyObj]");
        Case((_, myobj2), "[object MyObj]");

        EcmaValue myobj3 = CreateObject(valueOf: () => "[object MyObj]");
        Case((_, myobj3), "[object Object]");
      });

      It("should convert Symbol value to a string when called without new expression", () => {
        Case((_, new Symbol("66")), "Symbol(66)");
        Case((_, new Symbol()), "Symbol()");
        Case((_, new Symbol("")), "Symbol()");
      });

      It("should not wrap Symbol value when called with new expression", () => {
        That(() => String.Construct(new Symbol("1")), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void FromCharCode(RuntimeFunction fromCharCode) {
      IsUnconstructableFunctionWLength(fromCharCode, "fromCharCode", 1);

      It("should return a string whose code units are, in order, the elements in the arguments", () => {
        Case(_, "");
        Case((_, 65, 66, 66, 65), "ABBA");
      });

      It("should perform ToUInt16 for each code unit", () => {
        Case((_, NaN), "\0");
        Case((_, Number.Construct("abc")), "\0");
        Case((_, 0), "\0");
        Case((_, -0d), "\0");
        Case((_, Infinity), "\0");
        Case((_, -Infinity), "\0");
        Case((_, 1), new string((char)1, 1));
        Case((_, -1), new string((char)65535, 1));
        Case((_, 65535), new string((char)65535, 1));
        Case((_, 65534), new string((char)65534, 1));
        Case((_, 65536), "\0");
        Case((_, 4294967295), new string((char)65535, 1));
        Case((_, 4294967294), new string((char)65534, 1));
        Case((_, 4294967296), "\0");
        Case((_, -32767), new string((char)32769, 1));
        Case((_, -32768), new string((char)32768, 1));
        Case((_, -32769), new string((char)32767, 1));
        Case((_, -65535), new string((char)1, 1));
        Case((_, -65536), "\0");
        Case((_, -65537), new string((char)65535, 1));

        Case((_, true), new string((char)1, 1));
        Case((_, false), "\0");
        Case((_, Boolean.Construct(true)), new string((char)1, 1));
        Case((_, Boolean.Construct(false)), "\0");
        Case((_, Number.Construct(1)), new string((char)1, 1));
        Case((_, 1.234), new string((char)1, 1));
        Case((_, -1.234), new string((char)65535, 1));
        Case((_, String.Construct("1")), new string((char)1, 1));
        Case((_, "-1.234"), new string((char)65535, 1));

        EcmaValue myobj1 = CreateObject(valueOf: () => 1);
        Case((_, myobj1), new string((char)1, 1));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void FromCharPoint(RuntimeFunction fromCharPoint) {
      IsUnconstructableFunctionWLength(fromCharPoint, "fromCharPoint", 1);

      It("should throw a RangeError if an argument is not equal to its Integer representation", () => {
        Case((_, 3.14), Throws.RangeError);
        Case((_, 42, 3.14), Throws.RangeError);
        Case((_, "3.14"), Throws.RangeError);
        Case((_, Undefined), Throws.RangeError);
        Case((_, "-1"), Throws.RangeError);
        Case((_, "1a"), Throws.RangeError);
        Case((_, -1), Throws.RangeError);
        Case((_, 1, -1), Throws.RangeError);
        Case((_, 1114112), Throws.RangeError);
        Case((_, Infinity), Throws.RangeError);
      });

      It("should abrupt from ToNumber", () => {
        Case((_, new Symbol()), Throws.TypeError);
        Case((_, 42, new Symbol()), Throws.TypeError);
        IsAbruptedFromToPrimitive(fromCharPoint.Bind(_));
        IsAbruptedFromToPrimitive(fromCharPoint.Bind(_, 42));
      });

      It("should return a string whose code units are, in order, the elements in the arguments", () => {
        Case(_, "");
        Case((_, 0), "\x00");
        Case((_, 42), "*");
        Case((_, 65, 90), "AZ");
        Case((_, 0x404), "\u0404");
        Case((_, 0x2F804), "\uD87E\uDC04");
        Case((_, 194564), "\uD87E\uDC04");
        Case((_, 0x1D306, 0x61, 0x1D307), "\uD834\uDF06a\uD834\uDF07");
        Case((_, 1114111), "\uDBFF\uDFFF");
        Case((_, Null), "\x00");
        Case((_, false), "\x00");
        Case((_, true), "\x01");
        Case((_, "42"), "\x2A");
        Case((_, "042"), "\x2A");
        Case((_, CreateObject(valueOf: () => 31)), "\x1F");
      });
    }
  }
}
