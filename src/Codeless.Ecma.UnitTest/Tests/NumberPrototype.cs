using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class NumberPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Number.Prototype, Has.OwnProperty("constructor", Number, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Number.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Number.Prototype), Is.EqualTo("[object Number]"), "Number prototype object: its [[Class]] must be 'Number'");
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 1);

      Case((1234567890.123456789, 16), "499602d2.1f9adc");
      Case((0.0000123456789, 16), "0.0000cf204980dad01");

      It("should throw a RangeError if radix is not from 2 to 36", () => {
        Case((Number.Prototype, 0), Throws.RangeError);
        Case((0, 0), Throws.RangeError);
        Case((NaN, 0), Throws.RangeError);
        Case((Infinity, 0), Throws.RangeError);

        Case((Number.Prototype, 1), Throws.RangeError);
        Case((0, 1), Throws.RangeError);
        Case((NaN, 1), Throws.RangeError);
        Case((Infinity, 1), Throws.RangeError);

        Case((Number.Prototype, 37), Throws.RangeError);
        Case((0, 37), Throws.RangeError);
        Case((NaN, 37), Throws.RangeError);
        Case((Infinity, 37), Throws.RangeError);

        Case((Number.Prototype, Null), Throws.RangeError);
        Case((0, Null), Throws.RangeError);
        Case((NaN, Null), Throws.RangeError);
        Case((Infinity, Null), Throws.RangeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToPrecision(RuntimeFunction toPrecision) {
      IsUnconstructableFunctionWLength(toPrecision, "toPrecision", 1);

      It("should abrupt completion from ToInteger(fractionDigits)", () => {
        IsAbruptedFromToPrimitive(toPrecision.Bind(NaN));
        IsAbruptedFromSymbolToNumber(toPrecision.Bind(NaN));
      });

      It("should permit fractionDigits from 1 to 100", () => {
        Case((3, 1), "3");
        Case((3, 0), Throws.RangeError);
        Case((3, -1), Throws.RangeError);
        Case((3, 100), "3.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        Case((3, 101), Throws.RangeError);
      });

      It("should return regular string values", () => {
        Case((7, 1), "7");
        Case((7, 2), "7.0");
        Case((7, 3), "7.00");
        Case((7, 19), "7.000000000000000000");
        Case((7, 20), "7.0000000000000000000");
        Case((7, 21), "7.00000000000000000000");

        Case((-7, 1), "-7");
        Case((-7, 2), "-7.0");
        Case((-7, 3), "-7.00");
        Case((-7, 19), "-7.000000000000000000");
        Case((-7, 20), "-7.0000000000000000000");
        Case((-7, 21), "-7.00000000000000000000");

        Case((10, 2), "10");
        Case((11, 2), "11");
        Case((17, 2), "17");
        Case((19, 2), "19");
        Case((20, 2), "20");

        Case((-10, 2), "-10");
        Case((-11, 2), "-11");
        Case((-17, 2), "-17");
        Case((-19, 2), "-19");
        Case((-20, 2), "-20");

        Case((42, 2), "42");
        Case((-42, 2), "-42");

        Case((100, 3), "100");
        Case((100, 7), "100.0000");
        Case((1000, 7), "1000.000");
        Case((10000, 7), "10000.00");
        Case((100000, 7), "100000.0");

        Case((0.000001, 1), "0.000001");
        Case((0.000001, 2), "0.0000010");
        Case((0.000001, 3), "0.00000100");
      });

      It("should return string value for this value = 0 and precision is 1", () => {
        Case((0, 1), "0");
        Case((-0d, 1), "0");
      });

      It("should return string value for this value = 0 and precision is > 1", () => {
        Case((0, 2), "0.0");
        Case((0, 7), "0.000000");
        Case((0, 21), "0.00000000000000000000");
        Case((-0d, 2), "0.0");
        Case((-0d, 7), "0.000000");
        Case((-0d, 21), "0.00000000000000000000");
      });

      It("should return string values using exponential character", () => {
        Case((10, 1), "1e+1");
        Case((11, 1), "1e+1");
        Case((17, 1), "2e+1");
        Case((19, 1), "2e+1");
        Case((20, 1), "2e+1");

        Case((100, 1), "1e+2");
        Case((1000, 1), "1e+3");
        Case((10000, 1), "1e+4");
        Case((100000, 1), "1e+5");

        Case((100, 2), "1.0e+2");
        Case((1000, 2), "1.0e+3");
        Case((10000, 2), "1.0e+4");
        Case((100000, 2), "1.0e+5");

        Case((1000, 3), "1.00e+3");
        Case((10000, 3), "1.00e+4");
        Case((100000, 3), "1.00e+5");

        Case((42, 1), "4e+1");
        Case((-42, 1), "-4e+1");

        Case((1.2345e+27, 1), "1e+27");
        Case((1.2345e+27, 2), "1.2e+27");
        Case((1.2345e+27, 3), "1.23e+27");
        Case((1.2345e+27, 4), "1.234e+27");
        Case((1.2345e+27, 5), "1.2345e+27");
        Case((1.2345e+27, 6), "1.23450e+27");
        Case((1.2345e+27, 7), "1.234500e+27");
        Case((1.2345e+27, 16), "1.234500000000000e+27");
        Case((1.2345e+27, 17), "1.2345000000000000e+27");

        Case((-1.2345e+27, 1), "-1e+27");
        Case((-1.2345e+27, 2), "-1.2e+27");
        Case((-1.2345e+27, 3), "-1.23e+27");
        Case((-1.2345e+27, 4), "-1.234e+27");
        Case((-1.2345e+27, 5), "-1.2345e+27");
        Case((-1.2345e+27, 6), "-1.23450e+27");
        Case((-1.2345e+27, 7), "-1.234500e+27");
        Case((-1.2345e+27, 16), "-1.234500000000000e+27");
        Case((-1.2345e+27, 17), "-1.2345000000000000e+27");

        Case((1e+21, 1), "1e+21");
        Case((1e+21, 2), "1.0e+21");
        Case((1e+21, 3), "1.00e+21");
        Case((1e+21, 4), "1.000e+21");
        Case((1e+21, 5), "1.0000e+21");
        Case((1e+21, 6), "1.00000e+21");
        Case((1e+21, 7), "1.000000e+21");
        Case((1e+21, 16), "1.000000000000000e+21");
        Case((1e+21, 17), "1.0000000000000000e+21");
        Case((1e+21, 18), "1.00000000000000000e+21");
        Case((1e+21, 19), "1.000000000000000000e+21");
        Case((1e+21, 20), "1.0000000000000000000e+21");
        Case((1e+21, 21), "1.00000000000000000000e+21");

        Case((1e-21, 1), "1e-21");
        Case((1e-21, 2), "1.0e-21");
        Case((1e-21, 3), "1.00e-21");
        Case((1e-21, 4), "1.000e-21");
        Case((1e-21, 5), "1.0000e-21");
        Case((1e-21, 6), "1.00000e-21");
        Case((1e-21, 7), "1.000000e-21");
        Case((0.00000001, 1), "1e-8");
        Case((-0.00000001, 1), "-1e-8");

        // The following cases from Test Suite 262 is ignored
        // because of double high precision limitation
        // Case((1.2345e+27, 18), "1.23449999999999996e+27");
        // Case((1.2345e+27, 19), "1.234499999999999962e+27");
        // Case((1.2345e+27, 20), "1.2344999999999999618e+27");
        // Case((1.2345e+27, 21), "1.23449999999999996184e+27");
        // Case((-1.2345e+27, 18), "-1.23449999999999996e+27");
        // Case((-1.2345e+27, 19), "-1.234499999999999962e+27");
        // Case((-1.2345e+27, 20), "-1.2344999999999999618e+27");
        // Case((-1.2345e+27, 21), "-1.23449999999999996184e+27");
        // Case((1e-21, 16), "9.999999999999999e-22");
        // Case((1e-21, 17), "9.9999999999999991e-22");
        // Case((1e-21, 18), "9.99999999999999908e-22");
        // Case((1e-21, 19), "9.999999999999999075e-22");
        // Case((1e-21, 20), "9.9999999999999990754e-22");
        // Case((1e-21, 21), "9.99999999999999907537e-22");
      });

      It("should return signed Infinity string if this is Infinity", () => {
        Case((+Infinity, 1000), "Infinity");
        Case((Number.Construct(+Infinity), 1000), "Infinity");
        Case((-Infinity, 1000), "-Infinity");
        Case((Number.Construct(-Infinity), 1000), "-Infinity");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Case((NaN, Undefined), "NaN");

        int calls = 0;
        EcmaValue obj = CreateObject(valueOf: () => { calls++; return Infinity; });
        Case((NaN, obj), "NaN");
        That(calls, Is.EqualTo(1), "NaN is checked after ToInteger(precision)");
        Case((Number.Construct(NaN), obj), "NaN");
        That(calls, Is.EqualTo(2), "NaN is checked after ToInteger(precision)");
      });

      It("should throw a TypeError if this value is not a number object or value", () => {
        Case((new EcmaObject(), 1), Throws.TypeError);
        Case(("1", 1), Throws.TypeError);
        Case((Number, 1), Throws.TypeError);
        Case((true, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((Undefined, 1), Throws.TypeError);
        Case((new Symbol("1"), 1), Throws.TypeError);
        Case((EcmaArray.Of(), 1), Throws.TypeError);
      });

      It("should perform ToInteger(precision) operations", () => {
        Case((123.456, 1.1), "1e+2");
        Case((123.456, 1.9), "1e+2");
        Case((123.456, true), "1e+2");
        Case((123.456, "2"), "1.2e+2");
        Case((123.456, EcmaArray.Of(2)), "1.2e+2");
      });

      It("should return a string containing the the number value of this if precision is undefined", () => {
        Case((Number.Construct(7), Undefined), "7");
        Case((39, Undefined), "39");
        Case(Number.Prototype, "0");
        Case(42, "42");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToFixed(RuntimeFunction toFixed) {
      IsUnconstructableFunctionWLength(toFixed, "toFixed", 1);

      It("should not use ToString's cleaner rounding", () => {
        Case((1000000000000000128, 0), "1000000000000000128");
      });

      It("should permit fractionDigits from 0 to 100", () => {
        Case((3, 0), "3");
        Case((3, -1), Throws.RangeError);
        Case((3, 100), "3.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        Case((3, 101), Throws.RangeError);
      });

      It("should perform ToInteger(fractionDigits)", () => {
        Case((Number.Prototype, Undefined), "0");
        Case((Number.Prototype, 0), "0");
        Case((Number.Prototype, 1), "0.0");
        Case((Number.Prototype, 1.1), "0.0");
        Case((Number.Prototype, 0.9), "0");
        Case((Number.Prototype, "1"), "0.0");
        Case((Number.Prototype, "1.1"), "0.0");
        Case((Number.Prototype, "0.9"), "0");
        Case((Number.Prototype, NaN), "0");
        Case((Number.Prototype, "some string"), "0");
        Case((Number.Prototype, -0.1), "0");
      });

      It("should return string value on Number objects", () => {
        Case((Number.Construct(1), Undefined), "1");
        Case((Number.Construct(1), 0), "1");
        Case((Number.Construct(1), 1), "1.0");
        Case((Number.Construct(1), 1.1), "1.0");
        Case((Number.Construct(1), 0.9), "1");
        Case((Number.Construct(1), "1"), "1.0");
        Case((Number.Construct(1), "1.1"), "1.0");
        Case((Number.Construct(1), "0.9"), "1");
        Case((Number.Construct(1), NaN), "1");
        Case((Number.Construct(1), "some string"), "1");
        Case((Number.Construct(1), -0.1), "1");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Case((NaN, Undefined), "NaN");
        Case((NaN, 0), "NaN");
        Case((NaN, 1), "NaN");
        Case((NaN, 1.1), "NaN");
        Case((NaN, 0.9), "NaN");
        Case((NaN, "1"), "NaN");
        Case((NaN, "1.1"), "NaN");
        Case((NaN, "0.9"), "NaN");
        Case((NaN, NaN), "NaN");
        Case((NaN, "some string"), "NaN");
      });

      It("should use default ToString for this >= 1e21", () => {
        Case((1e21, 0), String.Call(_, 1e21));
        Case((1e21, 0), String.Call(_, 1e21));
        Case((1e21, 1), String.Call(_, 1e21));
        Case((1e21, 1.1), String.Call(_, 1e21));
        Case((1e21, 0.9), String.Call(_, 1e21));
        Case((1e21, "1"), String.Call(_, 1e21));
        Case((1e21, "1.1"), String.Call(_, 1e21));
        Case((1e21, "0.9"), String.Call(_, 1e21));
        Case((1e21, NaN), String.Call(_, 1e21));
        Case((1e21, "some string"), String.Call(_, 1e21));
      });

      It("should throw a RangeError when fractionDigits is out of range even if this is NaN or Infinity", () => {
        Case((NaN, Infinity), Throws.RangeError);
        Case((Number.Construct("a"), Infinity), Throws.RangeError);
        Case((Number.Construct(1e21), Infinity), Throws.RangeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToExponential(RuntimeFunction toExponential) {
      IsUnconstructableFunctionWLength(toExponential, "toExponential", 1);

      It("should abrupt completion from ToInteger(fractionDigits)", () => {
        IsAbruptedFromToPrimitive(toExponential.Bind(NaN));
        IsAbruptedFromSymbolToNumber(toExponential.Bind(NaN));
      });

      It("should return signed Infinity string if this is Infinity", () => {
        Case((+Infinity, 1000), "Infinity");
        Case((Number.Construct(+Infinity), 1000), "Infinity");
        Case((-Infinity, 1000), "-Infinity");
        Case((Number.Construct(-Infinity), 1000), "-Infinity");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Case((NaN, Infinity), "NaN");
        Case((Number.Construct(NaN), NaN), "NaN");
      });

      It("should permit fractionDigits from 0 to 100", () => {
        Case((3, 0), "3e+0");
        Case((3, -1), Throws.RangeError);
        Case((3, 100), "3.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000e+0");
        Case((3, 101), Throws.RangeError);
      });

      It("should return regular string values", () => {
        Case((123.456, 0), "1e+2");
        Case((123.456, 1), "1.2e+2");
        Case((123.456, 2), "1.23e+2");
        Case((123.456, 3), "1.235e+2");
        Case((123.456, 4), "1.2346e+2");
        Case((123.456, 5), "1.23456e+2");
        Case((123.456, 6), "1.234560e+2");
        Case((123.456, 7), "1.2345600e+2");
        Case((-123.456, 0), "-1e+2");
        Case((-123.456, 1), "-1.2e+2");
        Case((-123.456, 2), "-1.23e+2");
        Case((-123.456, 3), "-1.235e+2");
        Case((-123.456, 4), "-1.2346e+2");
        Case((-123.456, 5), "-1.23456e+2");
        Case((-123.456, 6), "-1.234560e+2");
        Case((-123.456, 7), "-1.2345600e+2");
        Case((0.0001, 0), "1e-4");
        Case((0.0001, 1), "1.0e-4");
        Case((0.0001, 2), "1.00e-4");
        Case((0.0001, 3), "1.000e-4");
        Case((0.0001, 4), "1.0000e-4");
        Case((0.9999, 0), "1e+0");
        Case((0.9999, 1), "1.0e+0");
        Case((0.9999, 2), "1.00e+0");
        Case((0.9999, 3), "9.999e-1");
        Case((0.9999, 4), "9.9990e-1");
        Case((25, 0), "3e+1");
        Case((12345, 3), "1.235e+4");

        // The following cases from Test Suite 262 is ignored
        // because of double high precision limitation
        // Case((123.456, 17), "1.23456000000000003e+2");
        // Case((123.456, 20), "1.23456000000000003070e+2");
        // Case((-123.456, 17), "-1.23456000000000003e+2");
        // Case((-123.456, 20), "-1.23456000000000003070e+2");
        // Case((0.0001, 16), "1.0000000000000000e-4");
        // Case((0.0001, 17), "1.00000000000000005e-4");
        // Case((0.0001, 18), "1.000000000000000048e-4");
        // Case((0.0001, 19), "1.0000000000000000479e-4");
        // Case((0.0001, 20), "1.00000000000000004792e-4");
        // Case((0.9999, 16), "9.9990000000000001e-1");
        // Case((0.9999, 17), "9.99900000000000011e-1");
        // Case((0.9999, 18), "9.999000000000000110e-1");
        // Case((0.9999, 19), "9.9990000000000001101e-1");
        // Case((0.9999, 20), "9.99900000000000011013e-1");
      });

      It("should return \"0\" if this value is 0 and ToInteger(fractionDigits) is 0", () => {
        Case((Number.Prototype, 0), "0e+0");
        Case((0, 0), "0e+0");
        Case((-0, 0), "0e+0");
        Case((0, -0), "0e+0");
        Case((-0, -0), "0e+0");
      });

      It("should return string value for this value = 0 and fractionDigits != 0", () => {
        Case((0, 1), "0.0e+0");
        Case((0, 2), "0.00e+0");
        Case((0, 7), "0.0000000e+0");
        Case((0, 20), "0.00000000000000000000e+0");
        Case((-0, 1), "0.0e+0");
        Case((-0, 2), "0.00e+0");
        Case((-0, 7), "0.0000000e+0");
        Case((-0, 20), "0.00000000000000000000e+0");
        Case((0.0, 4), "0.0000e+0");
        Case((-0.0, 4), "0.0000e+0");
      });

      It("should throw a TypeError if this value is not a number object or value", () => {
        Case((new EcmaObject(), 1), Throws.TypeError);
        Case(("1", 1), Throws.TypeError);
        Case((Number, 1), Throws.TypeError);
        Case((true, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((Undefined, 1), Throws.TypeError);
        Case((new Symbol("1"), 1), Throws.TypeError);
        Case((EcmaArray.Of(), 1), Throws.TypeError);
      });

      It("should perform ToInteger for fractionDigits that is not undefined", () => {
        Case((123.456, 0.1), "1e+2");
        Case((123.456, -0.1), "1e+2");
        Case((123.456, 0.9), "1e+2");
        Case((123.456, -0.9), "1e+2");
        Case((123.456, false), "1e+2");
        Case((123.456, true), "1.2e+2");
        Case((123.456, NaN), "1e+2");
        Case((123.456, Null), "1e+2");
        Case((123.456, "2"), "1.23e+2");
        Case((123.456, ""), "1e+2");
        Case((123.456, EcmaArray.Of()), "1e+2");
        Case((123.456, EcmaArray.Of(2)), "1.23e+2");
        Case((0, Undefined), "0e+0");
      });

      It("should handle undefined fractionDigits, not only casting it to 0", () => {
        Case((123.456, Undefined), "1.23456e+2");
        Case((123.456, 0), "1e+2");
        Case((1.1e-32, Undefined), "1.1e-32");
        Case((1.1e-32, 0), "1e-32");
        Case((100, Undefined), "1e+2");
        Case((100, 0), "1e+2");
      });
    }
  }
}
