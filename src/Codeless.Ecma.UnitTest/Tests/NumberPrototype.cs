using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;

namespace Codeless.Ecma.UnitTest.Tests {
  public class NumberPrototype {
    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 1);

      Expect((1234567890.123456789, 16), gives: "499602d2.1f9adc");
      Expect((0.0000123456789, 16), gives: "0.0000cf204980dad01");

      It("should throw a RangeError if radix is not from 2 to 36", () => {
        Expect((Number.Prototype, 0), gives: RangeError);
        Expect((0, 0), gives: RangeError);
        Expect((NaN, 0), gives: RangeError);
        Expect((Infinity, 0), gives: RangeError);

        Expect((Number.Prototype, 1), gives: RangeError);
        Expect((0, 1), gives: RangeError);
        Expect((NaN, 1), gives: RangeError);
        Expect((Infinity, 1), gives: RangeError);

        Expect((Number.Prototype, 37), gives: RangeError);
        Expect((0, 37), gives: RangeError);
        Expect((NaN, 37), gives: RangeError);
        Expect((Infinity, 37), gives: RangeError);

        Expect((Number.Prototype, Null), gives: RangeError);
        Expect((0, Null), gives: RangeError);
        Expect((NaN, Null), gives: RangeError);
        Expect((Infinity, Null), gives: RangeError);
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
        Expect((3, 1), gives: "3");
        Expect((3, 0), gives: RangeError);
        Expect((3, -1), gives: RangeError);
        Expect((3, 100), gives: "3.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        Expect((3, 101), gives: RangeError);
      });

      It("should return regular string values", () => {
        Expect((7, 1), "7");
        Expect((7, 2), "7.0");
        Expect((7, 3), "7.00");
        Expect((7, 19), "7.000000000000000000");
        Expect((7, 20), "7.0000000000000000000");
        Expect((7, 21), "7.00000000000000000000");

        Expect((-7, 1), "-7");
        Expect((-7, 2), "-7.0");
        Expect((-7, 3), "-7.00");
        Expect((-7, 19), "-7.000000000000000000");
        Expect((-7, 20), "-7.0000000000000000000");
        Expect((-7, 21), "-7.00000000000000000000");

        Expect((10, 2), "10");
        Expect((11, 2), "11");
        Expect((17, 2), "17");
        Expect((19, 2), "19");
        Expect((20, 2), "20");

        Expect((-10, 2), "-10");
        Expect((-11, 2), "-11");
        Expect((-17, 2), "-17");
        Expect((-19, 2), "-19");
        Expect((-20, 2), "-20");

        Expect((42, 2), "42");
        Expect((-42, 2), "-42");

        Expect((100, 3), "100");
        Expect((100, 7), "100.0000");
        Expect((1000, 7), "1000.000");
        Expect((10000, 7), "10000.00");
        Expect((100000, 7), "100000.0");

        Expect((0.000001, 1), "0.000001");
        Expect((0.000001, 2), "0.0000010");
        Expect((0.000001, 3), "0.00000100");
      });

      It("should return string value for this value = 0 and precision is 1", () => {
        Expect((0, 1), gives: "0");
        Expect((-0d, 1), gives: "0");
      });

      It("should return string value for this value = 0 and precision is > 1", () => {
        Expect((0, 2), gives: "0.0");
        Expect((0, 7), gives: "0.000000");
        Expect((0, 21), gives: "0.00000000000000000000");
        Expect((-0d, 2), gives: "0.0");
        Expect((-0d, 7), gives: "0.000000");
        Expect((-0d, 21), gives: "0.00000000000000000000");
      });

      It("should return string values using exponential character", () => {
        Expect((10, 1), "1e+1");
        Expect((11, 1), "1e+1");
        Expect((17, 1), "2e+1");
        Expect((19, 1), "2e+1");
        Expect((20, 1), "2e+1");

        Expect((100, 1), "1e+2");
        Expect((1000, 1), "1e+3");
        Expect((10000, 1), "1e+4");
        Expect((100000, 1), "1e+5");

        Expect((100, 2), "1.0e+2");
        Expect((1000, 2), "1.0e+3");
        Expect((10000, 2), "1.0e+4");
        Expect((100000, 2), "1.0e+5");

        Expect((1000, 3), "1.00e+3");
        Expect((10000, 3), "1.00e+4");
        Expect((100000, 3), "1.00e+5");

        Expect((42, 1), "4e+1");
        Expect((-42, 1), "-4e+1");

        Expect((1.2345e+27, 1), "1e+27");
        Expect((1.2345e+27, 2), "1.2e+27");
        Expect((1.2345e+27, 3), "1.23e+27");
        Expect((1.2345e+27, 4), "1.234e+27");
        Expect((1.2345e+27, 5), "1.2345e+27");
        Expect((1.2345e+27, 6), "1.23450e+27");
        Expect((1.2345e+27, 7), "1.234500e+27");
        Expect((1.2345e+27, 16), "1.234500000000000e+27");
        Expect((1.2345e+27, 17), "1.2345000000000000e+27");

        Expect((-1.2345e+27, 1), "-1e+27");
        Expect((-1.2345e+27, 2), "-1.2e+27");
        Expect((-1.2345e+27, 3), "-1.23e+27");
        Expect((-1.2345e+27, 4), "-1.234e+27");
        Expect((-1.2345e+27, 5), "-1.2345e+27");
        Expect((-1.2345e+27, 6), "-1.23450e+27");
        Expect((-1.2345e+27, 7), "-1.234500e+27");
        Expect((-1.2345e+27, 16), "-1.234500000000000e+27");
        Expect((-1.2345e+27, 17), "-1.2345000000000000e+27");

        Expect((1e+21, 1), "1e+21");
        Expect((1e+21, 2), "1.0e+21");
        Expect((1e+21, 3), "1.00e+21");
        Expect((1e+21, 4), "1.000e+21");
        Expect((1e+21, 5), "1.0000e+21");
        Expect((1e+21, 6), "1.00000e+21");
        Expect((1e+21, 7), "1.000000e+21");
        Expect((1e+21, 16), "1.000000000000000e+21");
        Expect((1e+21, 17), "1.0000000000000000e+21");
        Expect((1e+21, 18), "1.00000000000000000e+21");
        Expect((1e+21, 19), "1.000000000000000000e+21");
        Expect((1e+21, 20), "1.0000000000000000000e+21");
        Expect((1e+21, 21), "1.00000000000000000000e+21");

        Expect((1e-21, 1), "1e-21");
        Expect((1e-21, 2), "1.0e-21");
        Expect((1e-21, 3), "1.00e-21");
        Expect((1e-21, 4), "1.000e-21");
        Expect((1e-21, 5), "1.0000e-21");
        Expect((1e-21, 6), "1.00000e-21");
        Expect((1e-21, 7), "1.000000e-21");
        Expect((0.00000001, 1), "1e-8");
        Expect((-0.00000001, 1), "-1e-8");

        // The following cases from Test Suite 262 is ignored
        // because of double high precision limitation
        // Expect((1.2345e+27, 18), "1.23449999999999996e+27");
        // Expect((1.2345e+27, 19), "1.234499999999999962e+27");
        // Expect((1.2345e+27, 20), "1.2344999999999999618e+27");
        // Expect((1.2345e+27, 21), "1.23449999999999996184e+27");
        // Expect((-1.2345e+27, 18), "-1.23449999999999996e+27");
        // Expect((-1.2345e+27, 19), "-1.234499999999999962e+27");
        // Expect((-1.2345e+27, 20), "-1.2344999999999999618e+27");
        // Expect((-1.2345e+27, 21), "-1.23449999999999996184e+27");
        // Expect((1e-21, 16), "9.999999999999999e-22");
        // Expect((1e-21, 17), "9.9999999999999991e-22");
        // Expect((1e-21, 18), "9.99999999999999908e-22");
        // Expect((1e-21, 19), "9.999999999999999075e-22");
        // Expect((1e-21, 20), "9.9999999999999990754e-22");
        // Expect((1e-21, 21), "9.99999999999999907537e-22");
      });

      It("should return signed Infinity string if this is Infinity", () => {
        Expect((+Infinity, 1000), gives: "Infinity");
        Expect((Number.Construct(+Infinity), 1000), gives: "Infinity");
        Expect((-Infinity, 1000), gives: "-Infinity");
        Expect((Number.Construct(-Infinity), 1000), gives: "-Infinity");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Expect((NaN, _), "NaN");

        int calls = 0;
        var obj = new EcmaObject(new Hashtable { { "valueOf", RuntimeFunction.FromDelegate(() => { calls++; return Infinity; }) } });
        Expect((NaN, obj), "NaN");
        That(calls, Is.EqualTo(1), "NaN is checked after ToInteger(precision)");
        Expect((Number.Construct(NaN), obj), "NaN");
        That(calls, Is.EqualTo(2), "NaN is checked after ToInteger(precision)");
      });

      It("should throw a TypeError if this value is not a number object or value", () => {
        Expect((new EcmaObject(), 1), gives: TypeError);
        Expect(("1", 1), gives: TypeError);
        Expect((Number, 1), gives: TypeError);
        Expect((true, 1), gives: TypeError);
        Expect((false, 1), gives: TypeError);
        Expect((Null, 1), gives: TypeError);
        Expect((Undefined, 1), gives: TypeError);
        Expect((SymbolConstructor.Symbol("1"), 1), gives: TypeError);
        Expect((EcmaArray.Of(), 1), gives: TypeError);
      });

      It("should perform ToInteger(precision) operations", () => {
        Expect((123.456, 1.1), "1e+2");
        Expect((123.456, 1.9), "1e+2");
        Expect((123.456, true), "1e+2");
        Expect((123.456, "2"), "1.2e+2");
        Expect((123.456, EcmaArray.Of(2)), "1.2e+2");
      });

      It("should return a string containing the the number value of this if precision is undefined", () => {
        Expect((Number.Construct(7), _), gives: "7");
        Expect((39, _), gives: "39");
        Expect((Number.Prototype), gives: "0");
        Expect((42), gives: "42");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToFixed(RuntimeFunction toFixed) {
      IsUnconstructableFunctionWLength(toFixed, "toFixed", 1);

      It("should not use ToString's cleaner rounding", () => {
        Expect((1000000000000000128, 0), gives: "1000000000000000128");
      });

      It("should permit fractionDigits from 0 to 100", () => {
        Expect((3, 0), gives: "3");
        Expect((3, -1), gives: RangeError);
        Expect((3, 100), gives: "3.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        Expect((3, 101), gives: RangeError);
      });

      It("should perform ToInteger(fractionDigits)", () => {
        Expect((Number.Prototype, _), gives: "0");
        Expect((Number.Prototype, 0), gives: "0");
        Expect((Number.Prototype, 1), gives: "0.0");
        Expect((Number.Prototype, 1.1), gives: "0.0");
        Expect((Number.Prototype, 0.9), gives: "0");
        Expect((Number.Prototype, "1"), gives: "0.0");
        Expect((Number.Prototype, "1.1"), gives: "0.0");
        Expect((Number.Prototype, "0.9"), gives: "0");
        Expect((Number.Prototype, NaN), gives: "0");
        Expect((Number.Prototype, "some string"), gives: "0");
        Expect((Number.Prototype, -0.1), gives: "0");
      });

      It("should return string value on Number objects", () => {
        Expect((Number.Construct(1), _), gives: "1");
        Expect((Number.Construct(1), 0), gives: "1");
        Expect((Number.Construct(1), 1), gives: "1.0");
        Expect((Number.Construct(1), 1.1), gives: "1.0");
        Expect((Number.Construct(1), 0.9), gives: "1");
        Expect((Number.Construct(1), "1"), gives: "1.0");
        Expect((Number.Construct(1), "1.1"), gives: "1.0");
        Expect((Number.Construct(1), "0.9"), gives: "1");
        Expect((Number.Construct(1), NaN), gives: "1");
        Expect((Number.Construct(1), "some string"), gives: "1");
        Expect((Number.Construct(1), -0.1), gives: "1");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Expect((NaN, _), gives: "NaN");
        Expect((NaN, 0), gives: "NaN");
        Expect((NaN, 1), gives: "NaN");
        Expect((NaN, 1.1), gives: "NaN");
        Expect((NaN, 0.9), gives: "NaN");
        Expect((NaN, "1"), gives: "NaN");
        Expect((NaN, "1.1"), gives: "NaN");
        Expect((NaN, "0.9"), gives: "NaN");
        Expect((NaN, NaN), gives: "NaN");
        Expect((NaN, "some string"), gives: "NaN");
      });

      It("should use default ToString for this >= 1e21", () => {
        Expect((1e21, 0), gives: String.Call(_, 1e21));
        Expect((1e21, 0), gives: String.Call(_, 1e21));
        Expect((1e21, 1), gives: String.Call(_, 1e21));
        Expect((1e21, 1.1), gives: String.Call(_, 1e21));
        Expect((1e21, 0.9), gives: String.Call(_, 1e21));
        Expect((1e21, "1"), gives: String.Call(_, 1e21));
        Expect((1e21, "1.1"), gives: String.Call(_, 1e21));
        Expect((1e21, "0.9"), gives: String.Call(_, 1e21));
        Expect((1e21, NaN), gives: String.Call(_, 1e21));
        Expect((1e21, "some string"), gives: String.Call(_, 1e21));
      });

      It("should throw a RangeError when fractionDigits is out of range even if this is NaN or Infinity", () => {
        Expect((NaN, Infinity), gives: RangeError);
        Expect((Number.Construct("a"), Infinity), gives: RangeError);
        Expect((Number.Construct(1e21), Infinity), gives: RangeError);
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
        Expect((+Infinity, 1000), gives: "Infinity");
        Expect((Number.Construct(+Infinity), 1000), gives: "Infinity");
        Expect((-Infinity, 1000), gives: "-Infinity");
        Expect((Number.Construct(-Infinity), 1000), gives: "-Infinity");
      });

      It("should return \"NaN\" if this is NaN", () => {
        Expect((NaN, Infinity), gives: "NaN");
        Expect((Number.Construct(NaN), NaN), gives: "NaN");
      });

      It("should permit fractionDigits from 0 to 100", () => {
        Expect((3, 0), gives: "3e+0");
        Expect((3, -1), gives: RangeError);
        Expect((3, 100), gives: "3.0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000e+0");
        Expect((3, 101), gives: RangeError);
      });

      It("should return regular string values", () => {
        Expect((123.456, 0), gives: "1e+2");
        Expect((123.456, 1), gives: "1.2e+2");
        Expect((123.456, 2), gives: "1.23e+2");
        Expect((123.456, 3), gives: "1.235e+2");
        Expect((123.456, 4), gives: "1.2346e+2");
        Expect((123.456, 5), gives: "1.23456e+2");
        Expect((123.456, 6), gives: "1.234560e+2");
        Expect((123.456, 7), gives: "1.2345600e+2");
        Expect((-123.456, 0), gives: "-1e+2");
        Expect((-123.456, 1), gives: "-1.2e+2");
        Expect((-123.456, 2), gives: "-1.23e+2");
        Expect((-123.456, 3), gives: "-1.235e+2");
        Expect((-123.456, 4), gives: "-1.2346e+2");
        Expect((-123.456, 5), gives: "-1.23456e+2");
        Expect((-123.456, 6), gives: "-1.234560e+2");
        Expect((-123.456, 7), gives: "-1.2345600e+2");
        Expect((0.0001, 0), gives: "1e-4");
        Expect((0.0001, 1), gives: "1.0e-4");
        Expect((0.0001, 2), gives: "1.00e-4");
        Expect((0.0001, 3), gives: "1.000e-4");
        Expect((0.0001, 4), gives: "1.0000e-4");
        Expect((0.9999, 0), gives: "1e+0");
        Expect((0.9999, 1), gives: "1.0e+0");
        Expect((0.9999, 2), gives: "1.00e+0");
        Expect((0.9999, 3), gives: "9.999e-1");
        Expect((0.9999, 4), gives: "9.9990e-1");
        Expect((25, 0), gives: "3e+1");
        Expect((12345, 3), gives: "1.235e+4");

        // The following cases from Test Suite 262 is ignored
        // because of double high precision limitation
        // Expect((123.456, 17), ("1.23456000000000003e+2"));
        // Expect((123.456, 20), ("1.23456000000000003070e+2"));
        // Expect((-123.456, 17), ("-1.23456000000000003e+2"));
        // Expect((-123.456, 20), ("-1.23456000000000003070e+2"));
        // Expect((0.0001, 16), ("1.0000000000000000e-4"));
        // Expect((0.0001, 17), ("1.00000000000000005e-4"));
        // Expect((0.0001, 18), ("1.000000000000000048e-4"));
        // Expect((0.0001, 19), ("1.0000000000000000479e-4"));
        // Expect((0.0001, 20), ("1.00000000000000004792e-4"));
        // Expect((0.9999, 16), ("9.9990000000000001e-1"));
        // Expect((0.9999, 17), ("9.99900000000000011e-1"));
        // Expect((0.9999, 18), ("9.999000000000000110e-1"));
        // Expect((0.9999, 19), ("9.9990000000000001101e-1"));
        // Expect((0.9999, 20), ("9.99900000000000011013e-1"));
      });

      It("should return \"0\" if this value is 0 and ToInteger(fractionDigits) is 0", () => {
        Expect((Number.Prototype, 0), gives: "0e+0");
        Expect((0, 0), gives: "0e+0");
        Expect((-0, 0), gives: "0e+0");
        Expect((0, -0), gives: "0e+0");
        Expect((-0, -0), gives: "0e+0");
      });

      It("should return string value for this value = 0 and fractionDigits != 0", () => {
        Expect((0, 1), gives: "0.0e+0");
        Expect((0, 2), gives: "0.00e+0");
        Expect((0, 7), gives: "0.0000000e+0");
        Expect((0, 20), gives: "0.00000000000000000000e+0");
        Expect((-0, 1), gives: "0.0e+0");
        Expect((-0, 2), gives: "0.00e+0");
        Expect((-0, 7), gives: "0.0000000e+0");
        Expect((-0, 20), gives: "0.00000000000000000000e+0");
        Expect((0.0, 4), gives: "0.0000e+0");
        Expect((-0.0, 4), gives: "0.0000e+0");
      });

      It("should throw a TypeError if this value is not a number object or value", () => {
        Expect((new EcmaObject(), 1), gives: TypeError);
        Expect(("1", 1), gives: TypeError);
        Expect((Number, 1), gives: TypeError);
        Expect((true, 1), gives: TypeError);
        Expect((false, 1), gives: TypeError);
        Expect((Null, 1), gives: TypeError);
        Expect((Undefined, 1), gives: TypeError);
        Expect((SymbolConstructor.Symbol("1"), 1), gives: TypeError);
        Expect((EcmaArray.Of(), 1), gives: TypeError);
      });

      It("should perform ToInteger for fractionDigits that is not undefined", () => {
        Expect((123.456, 0.1), gives: "1e+2");
        Expect((123.456, -0.1), gives: "1e+2");
        Expect((123.456, 0.9), gives: "1e+2");
        Expect((123.456, -0.9), gives: "1e+2");
        Expect((123.456, false), gives: "1e+2");
        Expect((123.456, true), gives: "1.2e+2");
        Expect((123.456, NaN), gives: "1e+2");
        Expect((123.456, Null), gives: "1e+2");
        Expect((123.456, "2"), gives: "1.23e+2");
        Expect((123.456, ""), gives: "1e+2");
        Expect((123.456, EcmaArray.Of()), gives: "1e+2");
        Expect((123.456, EcmaArray.Of(2)), gives: "1.23e+2");
        Expect((0, Undefined), gives: "0e+0");
      });

      It("should handle undefined fractionDigits, not only casting it to 0", () => {
        Expect((123.456, _), gives: "1.23456e+2");
        Expect((123.456, 0), gives: "1e+2");
        Expect((1.1e-32, _), gives: "1.1e-32");
        Expect((1.1e-32, 0), gives: "1e-32");
        Expect((100, _), gives: "1e+2");
        Expect((100, 0), gives: "1e+2");
      });
    }
  }
}
