#if !BIGINTEGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  internal static class BigIntHelper {
    public const bool Supported = false;

    public static EcmaValue One => throw new NotSupportedException();

    public static EcmaValue Zero => throw new NotSupportedException();

    public static EcmaValue ToBigInt(long value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigInt(ulong value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigInt(double value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigInt(string inputString) {
      throw new NotSupportedException();
    }

    public static bool TryParse(string inputString, out EcmaValue value) {
      throw new NotSupportedException();
    }

    public static long ToInt64(EcmaValue value) {
      throw new NotSupportedException();
    }

    public static ulong ToUInt64(EcmaValue value) {
      throw new NotSupportedException();
    }

    public static string ToString(EcmaValue value, int radix) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigInt(this EcmaValue value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigInt64(this EcmaValue value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigUInt64(this EcmaValue value) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigIntN(EcmaValue value, int bits) {
      throw new NotSupportedException();
    }

    public static EcmaValue ToBigUIntN(EcmaValue value, int bits) {
      throw new NotSupportedException();
    }

    public static int Compare(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Add(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Add64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Subtract(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Subtract64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Multiply(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Multiply64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Divide(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Divide64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Mod(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Mod64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Pow(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Pow64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue BitwiseAnd(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue BitwiseAnd64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue BitwiseOr(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue BitwiseOr64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue ExclusiveOr(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue ExclusiveOr64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue LeftShift(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue LeftShift64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue RightShift(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue RightShift64(EcmaValue x, EcmaValue y) {
      throw new NotSupportedException();
    }

    public static EcmaValue Negate(EcmaValue x) {
      throw new NotSupportedException();
    }

    public static EcmaValue Negate64(EcmaValue x) {
      throw new NotSupportedException();
    }
  }
}
#endif
