using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class MathObject : TestBase {
    [Test]
    public void Properties() {
      That(Math.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(() => Math.Call(), Throws.TypeError);
      That(() => Math.Construct(), Throws.TypeError);

      That(Math, Has.OwnProperty("E", EcmaPropertyAttributes.None));
      That(Math.Get("E"), Is.TypeOf("number"));
      That(Math.Get("E"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("LN10", EcmaPropertyAttributes.None));
      That(Math.Get("LN10"), Is.TypeOf("number"));
      That(Math.Get("LN10"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("LN2", EcmaPropertyAttributes.None));
      That(Math.Get("LN2"), Is.TypeOf("number"));
      That(Math.Get("LN2"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("LOG10E", EcmaPropertyAttributes.None));
      That(Math.Get("LOG10E"), Is.TypeOf("number"));
      That(Math.Get("LOG10E"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("LOG2E", EcmaPropertyAttributes.None));
      That(Math.Get("LOG2E"), Is.TypeOf("number"));
      That(Math.Get("LOG2E"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("PI", EcmaPropertyAttributes.None));
      That(Math.Get("PI"), Is.TypeOf("number"));
      That(Math.Get("PI"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("SQRT1_2", EcmaPropertyAttributes.None));
      That(Math.Get("SQRT1_2"), Is.TypeOf("number"));
      That(Math.Get("SQRT1_2"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty("SQRT2", EcmaPropertyAttributes.None));
      That(Math.Get("SQRT2"), Is.TypeOf("number"));
      That(Math.Get("SQRT2"), Is.Not.EqualTo(NaN));

      That(Math, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Math", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Abs(RuntimeFunction abs) {
      IsUnconstructableFunctionWLength(abs, "abs", 1);

      Case((_, -42), 42, "-42");
      Case((_, 42), 42, "42");
      Case((_, -0.000001), 0.000001, "-0.000001");
      Case((_, 0.000001), 0.000001, "0.000001");
      Case((_, -1e-17), 1e-17, "-1e-17");
      Case((_, 1e-17), 1e-17, "1e-17");
      Case((_, -9007199254740991), 9007199254740991, "-(2**53-1)");
      Case((_, 9007199254740991), 9007199254740991, "2**53-1");

      Case((_, NaN), NaN);
      Case((_, -Infinity), Infinity);
      Case((_, -0d), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Acos(RuntimeFunction acos) {
      IsUnconstructableFunctionWLength(acos, "acos", 1);

      Case((_, NaN), NaN);
      Case((_, 1.000000000000001), NaN, "1.000000000000001");
      Case((_, 2), NaN, "2");
      Case((_, Infinity), NaN, "Infinity");
      Case((_, -1.000000000000001), NaN, "-1.000000000000001");
      Case((_, -2), NaN, "-2");
      Case((_, -Infinity), NaN, "-Infinity");
      Case((_, 1), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Acosh(RuntimeFunction acosh) {
      IsUnconstructableFunctionWLength(acosh, "acosh", 1);

      Case((_, Infinity), Infinity);
      Case((_, +1), 0, "Math.acosh should produce 0 for +1");
      Case((_, NaN), NaN, "NaN");
      Case((_, 0.999999), NaN, "0.999999");
      Case((_, 0), NaN, "0");
      Case((_, -1), NaN, "-1");
      Case((_, -Infinity), NaN, "-Infinity");
    }

    [Test, RuntimeFunctionInjection]
    public void Asin(RuntimeFunction asin) {
      IsUnconstructableFunctionWLength(asin, "asin", 1);

      Case((_, NaN), NaN, "NaN");
      Case((_, 1.000000000000001), NaN, "1.000000000000001");
      Case((_, 2), NaN, "2");
      Case((_, Infinity), NaN, "Infinity");
      Case((_, -1.000000000000001), NaN, "-1.000000000000001");
      Case((_, -2), NaN, "-2");
      Case((_, -Infinity), NaN, "-Infinity");
      Case((_, 0), 0);
      Case((_, -0d), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Asinh(RuntimeFunction asinh) {
      IsUnconstructableFunctionWLength(asinh, "asinh", 1);

      Case((_, NaN), NaN);
      Case((_, +Infinity), +Infinity);
      Case((_, -Infinity), -Infinity);
      Case((_, 0), 0);
      Case((_, -0d), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Atan(RuntimeFunction atan) {
      IsUnconstructableFunctionWLength(atan, "atan", 1);

      Case((_, NaN), NaN);
      Case((_, 0), 0);
      Case((_, -0d), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Atan2(RuntimeFunction atan2) {
      IsUnconstructableFunctionWLength(atan2, "atan2", 2);

      Case((_, NaN, -Infinity), NaN);
      Case((_, NaN, -0.000000000000001), NaN);
      Case((_, NaN, -0d), NaN);
      Case((_, NaN, 0), NaN);
      Case((_, NaN, 0.000000000000001), NaN);
      Case((_, NaN, Infinity), NaN);
      Case((_, -Infinity, NaN), NaN);
      Case((_, -0.000000000000001, NaN), NaN);
      Case((_, -0d, NaN), NaN);
      Case((_, 0, NaN), NaN);
      Case((_, 0.000000000000001, NaN), NaN);
      Case((_, Infinity, NaN), NaN);
      Case((_, NaN, NaN), NaN);

      Case((_, 0, 0), 0);
      Case((_, 0, 0.000000000000001), 0);
      Case((_, 0, 1), 0);
      Case((_, 0, Infinity), 0);

      Case((_, -0d, 0), -0d);
      Case((_, -0d, 0.000000000000001), -0d);
      Case((_, -0d, 1), -0d);
      Case((_, -0d, Infinity), -0d);

      Case((_, 0.000000000000001, Infinity), 0);
      Case((_, 1, Infinity), 0);
      Case((_, 1.7976931348623157E308, Infinity), 0);

      Case((_, -0.000000000000001, Infinity), -0d);
      Case((_, -1, Infinity), -0d);
      Case((_, -1.7976931348623157E308, Infinity), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Atanh(RuntimeFunction atanh) {
      IsUnconstructableFunctionWLength(atanh, "atanh", 1);

      Case((_, -1.9), NaN);
      Case((_, NaN), NaN);
      Case((_, -10), NaN);
      Case((_, -Infinity), NaN);
      Case((_, 1.9), NaN);
      Case((_, 10), NaN);
      Case((_, Infinity), NaN);
      Case((_, -1), -Infinity);
      Case((_, +1), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Cbrt(RuntimeFunction cbrt) {
      IsUnconstructableFunctionWLength(cbrt, "cbrt", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Ceil(RuntimeFunction ceil) {
      IsUnconstructableFunctionWLength(ceil, "ceil", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
      Case((_, -0.000000000000001), -0d, "-0.000000000000001");
      Case((_, -0.999999999999999), -0d, "-0.999999999999999");
      Case((_, -0.5), -0d, "-0.5");

      for (double i = -1000; i < 1000; i++) {
        double x = i / 10.0;
        That(ceil.Call(_, x) == -Math.Invoke("floor", -x), x.ToString());
      }
    }

    [Test, RuntimeFunctionInjection]
    public void Clz32(RuntimeFunction clz32) {
      IsUnconstructableFunctionWLength(clz32, "clz32", 1);

      Case((_, NaN), 32, "Infinity");
      Case((_, Infinity), 32, "Infinity");
      Case((_, -Infinity), 32, "-Infinity");
      Case((_, -0d), 32, "Infinity");
      Case((_, 0), 32, "Infinity");
      Case((_, 1), 31, "Infinity");
      Case((_, 2147483648), 0, "2147483648");
      Case((_, 4294967295), 0, "2**32-1");
      Case((_, 4294967296), 32, "2**32");
      Case((_, 4294967297), 31, "2**32+1");
      Case((_, 65535), 16, "2**16-1");
      Case((_, 65536), 15, "2**16");
      Case((_, 65537), 15, "2**16+1");
      Case((_, 255), 24, "2**8-1");
      Case((_, 256), 23, "2**8");
      Case((_, 257), 23, "2**8+1");
      Case((_, -4294967295), 31, "-(2**32-1)");
      Case((_, -4294967296), 32, "-(2**32)");
      Case((_, -4294967297), 0, "-(2**32+1)");
      Case((_, -65535), 0, "-(2**16-1)");
      Case((_, -65536), 0, "-(2**16)");
      Case((_, -65537), 0, "-(2**16+1)");
      Case((_, -255), 0, "-(2**8-1)");
      Case((_, -256), 0, "-(2**8)");
      Case((_, -257), 0, "-(2**8+1)");
    }

    [Test, RuntimeFunctionInjection]
    public void Cos(RuntimeFunction cos) {
      IsUnconstructableFunctionWLength(cos, "cos", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), NaN);
      Case((_, -0d), 1);
      Case((_, 0), 1);
    }

    [Test, RuntimeFunctionInjection]
    public void Cosh(RuntimeFunction cosh) {
      IsUnconstructableFunctionWLength(cosh, "cosh", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), 1);
      Case((_, 0), 1);
    }

    [Test, RuntimeFunctionInjection]
    public void Exp(RuntimeFunction exp) {
      IsUnconstructableFunctionWLength(exp, "exp", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), 0);
      Case((_, Infinity), Infinity);
      Case((_, -0d), 1);
      Case((_, 0), 1);
    }

    [Test, RuntimeFunctionInjection]
    public void Expm1(RuntimeFunction expm1) {
      IsUnconstructableFunctionWLength(expm1, "expm1", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -1);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Floor(RuntimeFunction floor) {
      IsUnconstructableFunctionWLength(floor, "floor", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
      Case((_, 0.000000000000001), -0, "-0.000000000000001");
      Case((_, 0.999999999999999), -0, "-0.999999999999999");
      Case((_, 0.5), -0, "-0.5");

      for (double i = -1000; i < 1000; i++) {
        double x = i / 10.0;
        That(floor.Call(_, x) == -Math.Invoke("ceil", -x), x.ToString());
      }
    }

    [Test, RuntimeFunctionInjection]
    public void Fround(RuntimeFunction fround) {
      IsUnconstructableFunctionWLength(fround, "fround", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);

      var a0 = 1.0;
      var a1 = 1.0000000596046448;
      var a2 = 1.0000001192092896;
      var a3 = 1.0000001788139343;
      var a4 = 1.000000238418579;
      var a5 = 1.0000002980232239;
      var a6 = 1.0000003576278687;

      Case((_, a0), a0, "Math.fround(a0)");
      Case((_, a1), a0, "Math.fround(a1)");
      Case((_, a2), a2, "Math.fround(a2)");
      Case((_, a3), a4, "Math.fround(a3)");
      Case((_, a4), a4, "Math.fround(a4)");
      Case((_, a5), a4, "Math.fround(a5)");
      Case((_, a6), a6, "Math.fround(a6)");
      Case((_, -a0), -a0, "Math.fround(-a0)");
      Case((_, -a1), -a0, "Math.fround(-a1)");
      Case((_, -a2), -a2, "Math.fround(-a2)");
      Case((_, -a3), -a4, "Math.fround(-a3)");
      Case((_, -a4), -a4, "Math.fround(-a4)");
      Case((_, -a5), -a4, "Math.fround(-a5)");
      Case((_, -a6), -a6, "Math.fround(-a6)");

      Case((_, 4294967295), 4294967296, "2**32-1");
      Case((_, 4294967296), 4294967296, "2**32");
      Case((_, 4294967297), 4294967296, "2**32+1");

      Case((_, 0.1), 0.10000000149011612, "0.1");
      Case((_, 0.2), 0.20000000298023224, "0.2");
      Case((_, 0.5), 0.5, "0.5");
    }

    [Test, RuntimeFunctionInjection]
    public void Hypot(RuntimeFunction hypot) {
      IsUnconstructableFunctionWLength(hypot, "hypot", 2);

      Case((_, 3, Infinity), Infinity);
      Case((_, NaN, Infinity), Infinity);
      Case((_, NaN, 3), NaN);
      Case((_, 3, -Infinity), Infinity);
      Case((_, 3, 4), 5);

      Case(_, 0);
      Case((_, 0, 0), 0);
      Case((_, 0, -0d), 0);
      Case((_, -0d, 0), 0);
      Case((_, -0d, -0d), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Imul(RuntimeFunction imul) {
      IsUnconstructableFunctionWLength(imul, "imul", 2);

      Case((_, 2, 4), 8, "(2, 4)");
      Case((_, -1, 8), -8, "(-1, 8)");
      Case((_, -2, -2), 4, "(-2, -2)");
      Case((_, 0xffffffff, 5), -5, "(0xffffffff, 5)");
      Case((_, 0xfffffffe, 5), -10, "(0xfffffffe, 5)");
      Case((_, -0, 7), 0);
      Case((_, 7, -0), 0);
      Case((_, 0.1, 7), 0);
      Case((_, 7, 0.1), 0);
      Case((_, 0.9, 7), 0);
      Case((_, 7, 0.9), 0);
      Case((_, 1.1, 7), 7);
      Case((_, 7, 1.1), 7);
      Case((_, 1.9, 7), 7);
      Case((_, 7, 1.9), 7);
      Case((_, 1073741824, 7), -1073741824);
      Case((_, 7, 1073741824), -1073741824);
      Case((_, 1073741824, 1073741824), 0);
      Case((_, -1073741824, 7), 1073741824);
      Case((_, 7, -1073741824), 1073741824);
      Case((_, -1073741824, -1073741824), 0);
      Case((_, 2147483648, 7), -2147483648);
      Case((_, 7, 2147483648), -2147483648);
      Case((_, 2147483648, 2147483648), 0);
      Case((_, -2147483648, 7), -2147483648);
      Case((_, 7, -2147483648), -2147483648);
      Case((_, -2147483648, -2147483648), 0);
      Case((_, 2147483647, 7), 2147483641);
      Case((_, 7, 2147483647), 2147483641);
      Case((_, 2147483647, 2147483647), 1);
      Case((_, 65536, 65536), 0);
      Case((_, 65535, 65536), -65536);
      Case((_, 65536, 65535), -65536);
      Case((_, 65535, 65535), -131071);
    }

    [Test, RuntimeFunctionInjection]
    public void Log(RuntimeFunction log) {
      IsUnconstructableFunctionWLength(log, "log", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), Infinity);
      Case((_, -1), NaN);
      Case((_, -0.000000000000001), NaN);
      Case((_, -0d), -Infinity);
      Case((_, 0), -Infinity);
      Case((_, 1), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Log10(RuntimeFunction log10) {
      IsUnconstructableFunctionWLength(log10, "log10", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), Infinity);
      Case((_, -1), NaN);
      Case((_, -0.000000000000001), NaN);
      Case((_, -0d), -Infinity);
      Case((_, 1), 0);
      Case((_, 10.00), 1);
      Case((_, 100.00), 2);
      Case((_, 1000.00), 3);
    }

    [Test, RuntimeFunctionInjection]
    public void Log1p(RuntimeFunction log1p) {
      IsUnconstructableFunctionWLength(log1p, "log1p", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), Infinity);
      Case((_, -2), NaN);
      Case((_, -1.000001), NaN);
      Case((_, -1), -Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), -0);
    }

    [Test, RuntimeFunctionInjection]
    public void Log2(RuntimeFunction log2) {
      IsUnconstructableFunctionWLength(log2, "log2", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), Infinity);
      Case((_, -1), NaN);
      Case((_, -0.000000000000001), NaN);
      Case((_, -0d), -Infinity);
      Case((_, 1), 0);
      Case((_, 2), 1);
      Case((_, 4), 2);
      Case((_, 8), 3);
    }

    [Test, RuntimeFunctionInjection]
    public void Max(RuntimeFunction max) {
      IsUnconstructableFunctionWLength(max, "max", 2);

      Case(_, -Infinity);
      Case((_, NaN), NaN);
      Case((_, new EcmaObject()), NaN);

      Case((_, NaN, -Infinity), NaN);
      Case((_, NaN, -0.000000000000001), NaN);
      Case((_, NaN, -0d), NaN);
      Case((_, NaN, 0), NaN);
      Case((_, NaN, 0.000000000000001), NaN);
      Case((_, NaN, Infinity), NaN);
      Case((_, -Infinity, NaN), NaN);
      Case((_, -0.000000000000001, NaN), NaN);
      Case((_, -0d, NaN), NaN);
      Case((_, 0, NaN), NaN);
      Case((_, 0.000000000000001, NaN), NaN);
      Case((_, Infinity, NaN), NaN);
      Case((_, NaN, NaN), NaN);

      Case((_, 0, 0), 0);
      Case((_, 0, -0d), 0);
      Case((_, -0d, 0), 0);
      Case((_, -0d, -0d), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Min(RuntimeFunction min) {
      IsUnconstructableFunctionWLength(min, "min", 2);

      Case(_, Infinity);
      Case((_, NaN), NaN);
      Case((_, new EcmaObject()), NaN);

      Case((_, NaN, -Infinity), NaN);
      Case((_, NaN, -0.000000000000001), NaN);
      Case((_, NaN, -0d), NaN);
      Case((_, NaN, 0), NaN);
      Case((_, NaN, 0.000000000000001), NaN);
      Case((_, NaN, Infinity), NaN);
      Case((_, -Infinity, NaN), NaN);
      Case((_, -0.000000000000001, NaN), NaN);
      Case((_, -0d, NaN), NaN);
      Case((_, 0, NaN), NaN);
      Case((_, 0.000000000000001, NaN), NaN);
      Case((_, Infinity, NaN), NaN);
      Case((_, NaN, NaN), NaN);

      Case((_, 0, 0), 0);
      Case((_, 0, -0d), -0d);
      Case((_, -0d, 0), -0d);
      Case((_, -0d, -0d), -0d);
    }

    [Test, RuntimeFunctionInjection]
    public void Pow(RuntimeFunction pow) {
      IsUnconstructableFunctionWLength(pow, "pow", 2);

      // If exponent is NaN, the result is NaN.
      Case((_, -Infinity, NaN), NaN);
      Case((_, -1.7976931348623157E308, NaN), NaN);
      Case((_, -0.000000000000001, NaN), NaN);
      Case((_, -0d, NaN), NaN);
      Case((_, +0d, NaN), NaN);
      Case((_, 0.000000000000001, NaN), NaN);
      Case((_, 1.7976931348623157E308, NaN), NaN);
      Case((_, Infinity, NaN), NaN);
      Case((_, NaN, NaN), NaN);

      // If abs(base) < 1 and exponent is −∞, the result is +∞.
      Case((_, 0.999999999999999, -Infinity), Infinity);
      Case((_, 0.5, -Infinity), Infinity);
      Case((_, +0d, -Infinity), Infinity);
      Case((_, -0d, -Infinity), Infinity);
      Case((_, -0.5, -Infinity), Infinity);
      Case((_, -0.999999999999999, -Infinity), Infinity);

      // If abs(base) > 1 and exponent is +∞, the result is +∞.
      Case((_, -Infinity, Infinity), Infinity);
      Case((_, -1.7976931348623157E308, Infinity), Infinity);
      Case((_, -1.000000000000001, Infinity), Infinity);
      Case((_, 1.000000000000001, Infinity), Infinity);
      Case((_, 1.7976931348623157E308, Infinity), Infinity);
      Case((_, Infinity, Infinity), Infinity);

      // If abs(base) > 1 and exponent is −∞, the result is +0.
      Case((_, -Infinity, -Infinity), 0);
      Case((_, -1.7976931348623157E308, -Infinity), 0);
      Case((_, -1.000000000000001, -Infinity), 0);
      Case((_, 1.000000000000001, -Infinity), 0);
      Case((_, 1.7976931348623157E308, -Infinity), 0);
      Case((_, Infinity, -Infinity), 0);

      // If exponent is +0, the result is 1, even if base is NaN.
      Case((_, -Infinity, 0), 1);
      Case((_, -1.7976931348623157E308, 0), 1);
      Case((_, -0.000000000000001, 0), 1);
      Case((_, -0d, 0), 1);
      Case((_, +0d, 0), 1);
      Case((_, 0.000000000000001, 0), 1);
      Case((_, 1.7976931348623157E308, 0), 1);
      Case((_, Infinity, 0), 1);
      Case((_, NaN, 0), 1);

      //  If exponent is −0, the result is 1, even if base is NaN.
      Case((_, -Infinity, -0d), 1);
      Case((_, -1.7976931348623157E308, -0d), 1);
      Case((_, -0.000000000000001, -0d), 1);
      Case((_, -0d, -0d), 1);
      Case((_, +0d, -0d), 1);
      Case((_, 0.000000000000001, -0d), 1);
      Case((_, 1.7976931348623157E308, -0d), 1);
      Case((_, Infinity, -0d), 1);
      Case((_, NaN, -0d), 1);

      // If base is NaN and exponent is nonzero, the result is NaN.
      Case((_, NaN, -Infinity), NaN);
      Case((_, NaN, -1.7976931348623157E308), NaN);
      Case((_, NaN, -0.000000000000001), NaN);
      Case((_, NaN, 0.000000000000001), NaN);
      Case((_, NaN, 1.7976931348623157E308), NaN);
      Case((_, NaN, Infinity), NaN);
      Case((_, NaN, NaN), NaN);

      // If base is +∞ and exponent < 0, the result is +0.
      Case((_, Infinity, -Infinity), 0);
      Case((_, Infinity, -1.7976931348623157E308), 0);
      Case((_, Infinity, -1), 0);
      Case((_, Infinity, -0.000000000000001), 0);

      // If base is +∞ and exponent > 0, the result is +∞.
      Case((_, Infinity, Infinity), Infinity);
      Case((_, Infinity, 1.7976931348623157E308), Infinity);
      Case((_, Infinity, 1), Infinity);
      Case((_, Infinity, 0.000000000000001), Infinity);

      // If base is −∞ and exponent > 0 and exponent is an odd integer, the result is −∞.
      Case((_, -Infinity, 1), -Infinity);
      Case((_, -Infinity, 111), -Infinity);
      Case((_, -Infinity, 111111), -Infinity);

      // If base is −∞ and exponent < 0 and exponent is an odd integer, the result is −0.
      Case((_, -Infinity, -1), -0d);
      Case((_, -Infinity, -111), -0d);
      Case((_, -Infinity, -111111), -0d);

      // If base is −∞ and exponent > 0 and exponent is not an odd integer, the result is +∞.
      Case((_, -Infinity, 0.000000000000001), Infinity);
      Case((_, -Infinity, 2), Infinity);
      Case((_, -Infinity, Math.Get("PI")), Infinity);
      Case((_, -Infinity, 1.7976931348623157E308), Infinity);
      Case((_, -Infinity, Infinity), Infinity);

      // If base is −∞ and exponent < 0 and exponent is not an odd integer, the result is +0.
      Case((_, -Infinity, -0.000000000000001), 0);
      Case((_, -Infinity, -2), 0);
      Case((_, -Infinity, -Math.Get("PI")), 0);
      Case((_, -Infinity, -1.7976931348623157E308), 0);
      Case((_, -Infinity, -Infinity), 0);

      // If base is +0 and exponent > 0, the result is +0.
      Case((_, 0, Infinity), 0);
      Case((_, 0, 1.7976931348623157E308), 0);
      Case((_, 0, 1), 0);
      Case((_, 0, 0.000000000000001), 0);

      // If base is +0 and exponent < 0, the result is +∞.
      Case((_, 0, -Infinity), Infinity);
      Case((_, 0, -1.7976931348623157E308), Infinity);
      Case((_, 0, -1), Infinity);
      Case((_, 0, -0.000000000000001), Infinity);

      //  If base is −0 and exponent > 0 and exponent is an odd integer, the result is −0.
      Case((_, -0d, 1), -0d);
      Case((_, -0d, 111), -0d);
      Case((_, -0d, 111111), -0d);

      //  If base is −0 and exponent < 0 and exponent is an odd integer, the result is −∞.
      Case((_, -0d, -1), -Infinity);
      Case((_, -0d, -111), -Infinity);
      Case((_, -0d, -111111), -Infinity);

      // If base is −0 and exponent > 0 and exponent is not an odd integer, the result is +0.
      Case((_, -0d, 0.000000000000001), 0);
      Case((_, -0d, 2), 0);
      Case((_, -0d, Math.Get("PI")), 0);
      Case((_, -0d, 1.7976931348623157E308), 0);
      Case((_, -0d, Infinity), 0);

      // If base is −0 and exponent < 0 and exponent is not an odd integer, the result is +∞.
      Case((_, -0d, -0.000000000000001), Infinity);
      Case((_, -0d, -2), Infinity);
      Case((_, -0d, -Math.Get("PI")), Infinity);
      Case((_, -0d, -1.7976931348623157E308), Infinity);
      Case((_, -0d, -Infinity), Infinity);

      // If abs(base) is 1 and exponent is +∞, the result is NaN.
      Case((_, -1, Infinity), NaN);
      Case((_, 1, Infinity), NaN);

      // If abs(base) is 1 and exponent is −∞, the result is NaN.
      Case((_, -1, -Infinity), NaN);
      Case((_, 1, -Infinity), NaN);

      // If abs(base) < 1 and exponent is +∞, the result is +0.
      Case((_, 0.999999999999999, Infinity), 0);
      Case((_, 0.5, Infinity), 0);
      Case((_, +0d, Infinity), 0);
      Case((_, -0d, Infinity), 0);
      Case((_, -0.5, Infinity), 0);
      Case((_, -0.999999999999999, Infinity), 0);

      // If base < 0 and base is finite and exponent is finite and exponent is not an integer, the result is NaN.
      Case((_, -1.7976931348623157E308, -1.000000000000001), NaN);
      Case((_, -1.7976931348623157E308, -0.000000000000001), NaN);
      Case((_, -1.7976931348623157E308, 0.000000000000001), NaN);
      Case((_, -1.7976931348623157E308, 1.000000000000001), NaN);
      Case((_, -1, -1.000000000000001), NaN);
      Case((_, -1, -0.000000000000001), NaN);
      Case((_, -1, 0.000000000000001), NaN);
      Case((_, -1, 1.000000000000001), NaN);
      Case((_, -0.000000000000001, -1.000000000000001), NaN);
      Case((_, -0.000000000000001, -0.000000000000001), NaN);
      Case((_, -0.000000000000001, 0.000000000000001), NaN);
      Case((_, -0.000000000000001, 1.000000000000001), NaN);

      Case((_, 2, -2147483648), 0);
      Case((_, 1, -2147483648), 1);
    }

    [Test, RuntimeFunctionInjection]
    public void Random(RuntimeFunction random) {
      IsUnconstructableFunctionWLength(random, "random", 0);

      for (int i = 0; i < 100; i++) {
        EcmaValue value = random.Call();
        That(value, Is.TypeOf("number"));
        That(value, Is.GreaterThanOrEqualTo(0).And.LessThan(1));
      }
    }

    [Test, RuntimeFunctionInjection]
    public void Round(RuntimeFunction round) {
      IsUnconstructableFunctionWLength(round, "round", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);

      Case((_, -0.5), -0d);
      Case((_, -0.25), -0d);
      Case((_, -0.25), -0d);


      for (int i = 0; i <= 1000; i++) {
        double x = i / 10.0;
        Case((_, x), Math.Invoke("floor", x + 0.5));
      }

      for (int i = -5; i >= -1000; i--) {
        double x = i == -5 ? -0.500000000000001 : i / 10.0;
        Case((_, x), Math.Invoke("floor", x + 0.5));
      }
    }

    [Test, RuntimeFunctionInjection]
    public void Sign(RuntimeFunction sign) {
      IsUnconstructableFunctionWLength(sign, "sign", 1);

      Case((_, NaN), NaN, "NaN");
      Case((_, -0d), -0d, "-0");
      Case((_, 0), 0, "0");
      Case((_, -0.000001), -1, "-0.000001");
      Case((_, -1), -1, "-1");
      Case((_, -Infinity), -1, "-Infinity");
      Case((_, 0.000001), 1, "0.000001");
      Case((_, 1), 1, "1");
      Case((_, Infinity), 1, "Infinity");
    }

    [Test, RuntimeFunctionInjection]
    public void Sin(RuntimeFunction sin) {
      IsUnconstructableFunctionWLength(sin, "sin", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), NaN);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Sinh(RuntimeFunction sinh) {
      IsUnconstructableFunctionWLength(sinh, "sinh", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Sqrt(RuntimeFunction sqrt) {
      IsUnconstructableFunctionWLength(sqrt, "sqrt", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), Infinity);
      Case((_, -0.000000000000001), NaN);
      Case((_, -1), NaN);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Tan(RuntimeFunction tan) {
      IsUnconstructableFunctionWLength(tan, "tan", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), NaN);
      Case((_, Infinity), NaN);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Tanh(RuntimeFunction tanh) {
      IsUnconstructableFunctionWLength(tanh, "tanh", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -1);
      Case((_, Infinity), 1);
      Case((_, -0d), -0d);
      Case((_, 0), 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Trunc(RuntimeFunction trunc) {
      IsUnconstructableFunctionWLength(trunc, "trunc", 1);

      Case((_, NaN), NaN);
      Case((_, -Infinity), -Infinity);
      Case((_, Infinity), Infinity);
      Case((_, -0.9), -0d);
      Case((_, 0.9), 0);
      Case((_, 4578.584949), 4578);
      Case((_, 0), 0);

      Case((_, 0.02047410048544407), 0,
         "Math.trunc should produce +0 for values between 0 and 1");
      Case((_, 0.00000000000000001), 0,
         "Math.trunc should produce +0 for values between 0 and 1");
      Case((_, 0.9999999999999999), 0,
         "Math.trunc should produce +0 for values between 0 and 1");
      Case((_, Number.Get("EPSILON")), 0,
         "Math.trunc should produce +0 for values between 0 and 1");
      Case((_, Number.Get("MIN_VALUE")), 0,
         "Math.trunc should produce +0 for values between 0 and 1");

      Case((_, -0.02047410048544407), -0d,
         "Math.trunc should produce -0 for values between -1 and 0");
      Case((_, -0.00000000000000001), -0d,
         "Math.trunc should produce -0 for values between -1 and 0");
      Case((_, -0.9999999999999999), -0d,
         "Math.trunc should produce -0 for values between -1 and 0");
      Case((_, -Number.Get("EPSILON")), -0d,
         "Math.trunc should produce -0 for values between -1 and 0");
      Case((_, -Number.Get("MIN_VALUE")), -0d,
         "Math.trunc should produce -0 for values between -1 and 0");

      Case((_, Number.Get("MAX_VALUE")), Math.Invoke("floor", Number.Get("MAX_VALUE")),
        "Math.trunc produces incorrect result for Number.MAX_VALUE");
      Case((_, 10), Math.Invoke("floor", 10),
        "Math.trunc produces incorrect result for 10");
      Case((_, 3.9), Math.Invoke("floor", 3.9),
        "Math.trunc produces incorrect result for 3.9");
      Case((_, 4.9), Math.Invoke("floor", 4.9),
        "Math.trunc produces incorrect result for 4.9");

      Case((_, -Number.Get("MAX_VALUE")), Math.Invoke("ceil", -Number.Get("MAX_VALUE")),
        "Math.trunc produces incorrect result for -Number.MAX_VALUE");
      Case((_, -10), Math.Invoke("ceil", -10),
        "Math.trunc produces incorrect result for -10");
      Case((_, -3.9), Math.Invoke("ceil", -3.9),
        "Math.trunc produces incorrect result for -3.9");
      Case((_, -4.9), Math.Invoke("ceil", -4.9),
        "Math.trunc produces incorrect result for -4.9");
    }
  }
}
