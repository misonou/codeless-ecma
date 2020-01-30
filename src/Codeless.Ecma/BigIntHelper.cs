#if BIGINTEGER
using Codeless.Ecma.Primitives;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  internal static class BigIntHelper {
    private static readonly BigInteger int64max = new BigInteger(Int64.MaxValue);
    private static readonly BigInteger int64min = new BigInteger(Int64.MinValue);
    private static readonly BigInteger mod64 = BigInteger.Pow(new BigInteger(2), 64);
    private static readonly BigInteger mod63 = BigInteger.Pow(new BigInteger(2), 63);
    private static readonly EcmaValue value0n = ToBigInt(0L);
    private static readonly EcmaValue value1n = ToBigInt(1L);
    private static readonly int[] biggestLong = { 63, 39, 31, 27, 24, 22, 21, 19, 18, 18, 17, 17, 16, 16, 15, 15, 15, 14, 14, 14, 14, 13, 13, 13, 13, 13, 13, 12, 12, 12, 12, 12, 12, 12, 12 };

    public const bool Supported = true;

    public static EcmaValue One => value1n;

    public static EcmaValue Zero => value0n;

    public static EcmaValue ToBigInt(BigInteger value) {
      if (value >= int64min && value <= int64max) {
        return ToBigInt((long)value);
      }
      BigIntBinder binder = new BigIntBinder(value);
      return binder.ToValue();
    }

    public static EcmaValue ToBigInt(long value) {
      return BigInt64Binder.Default.CreateValue(value);
    }

    public static EcmaValue ToBigInt(ulong value) {
      if (value <= Int64.MaxValue) {
        return ToBigInt((long)value);
      }
      BigIntBinder binder = new BigIntBinder(value);
      return binder.ToValue();
    }

    public static EcmaValue ToBigInt(double value) {
      if (Double.IsNaN(value) || Double.IsInfinity(value) || Math.Truncate(value) != value) {
        throw new EcmaRangeErrorException(InternalString.Error.NumberNotConvertibleToBigInt, value);
      }
      if (value >= Int64.MinValue && value <= Int64.MaxValue) {
        return ToBigInt((long)value);
      }
      BigIntBinder binder = new BigIntBinder(new BigInteger(value));
      return binder.ToValue();
    }

    public static EcmaValue ToBigInt(string inputString) {
      if (TryParse(inputString, out EcmaValue value)) {
        return value;
      }
      throw new EcmaSyntaxErrorException(InternalString.Error.InvalidBigIntSyntax, inputString);
    }

    [EcmaSpecification("StringToBigInt", EcmaSpecificationKind.AbstractOperations)]
    public static bool TryParse(string inputString, out EcmaValue value) {
      Guard.ArgumentNotNull(inputString, "inputString");
      inputString = inputString.Trim();
      if (inputString.Length == 0) {
        value = value0n;
        return true;
      }
      if (inputString[0] == '+') {
        value = default;
        return false;
      }
      int charIndex = 0;
      if (inputString[0] == '-') {
        charIndex = 1;
      } else if (inputString.Length > 2 && inputString[0] == '0') {
        switch (inputString[1]) {
          case 'x':
          case 'X':
            return TryParse(inputString.Substring(2), 16, 15, out value);
          case 'o':
          case 'O':
            return TryParse(inputString.Substring(2), 8, 21, out value);
          case 'b':
          case 'B':
            return TryParse(inputString.Substring(2), 2, 63, out value);
        }
      }
      // shortcut to 64-bit integer storage if decimal is atmost 18 digits which is less than Int64.MaxValue
      if (inputString.Length < charIndex + 18 && Int64.TryParse(inputString, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture.NumberFormat, out long longValue)) {
        value = ToBigInt(longValue);
        return true;
      }
      if (BigInteger.TryParse(inputString, out BigInteger bigInt)) {
        value = ToBigInt(bigInt);
        return true;
      }
      value = default;
      return false;
    }

    public static long ToInt64(EcmaValue value) {
      value = value.ToBigInt();
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        return value.ToInt64();
      }
      byte[] byteArray = ((BigInteger)value.GetUnderlyingObject()).ToByteArray();
      return BitConverter.ToInt64(byteArray, 0);
    }

    public static ulong ToUInt64(EcmaValue value) {
      value = ToBigUInt64(value);
      object obj = value.GetUnderlyingObject();
      return obj is BigInteger bigInt ? (ulong)bigInt : (ulong)(long)obj;
    }

    public static string ToString(EcmaValue value, int radix) {
      ThrowIfArgumentNotBigInt(value);
      if (radix < 2 || radix > 36) {
        throw new EcmaRangeErrorException(InternalString.Error.InvalidToStringRadix);
      }
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        return NumberPrototype.GetString(value.ToInt64(), radix);
      }
      BigInteger bigInt = ToBigInteger(value);
      if (radix == 10) {
        return bigInt.ToString();
      }
      if (radix == 16) {
        return bigInt.ToString("x");
      }
      int digits = biggestLong[radix - 2];
      bool isMinus = bigInt < 0;
      if (isMinus) {
        bigInt = -bigInt;
      }
      BigInteger divisor = BigInteger.Pow(radix, digits);
      StringBuilder sb = new StringBuilder();
      while (bigInt > divisor) {
        bigInt = BigInteger.DivRem(bigInt, divisor, out BigInteger remainder);
        string s = NumberPrototype.GetString((long)remainder, radix);
        sb.Insert(0, s);
        if (s.Length < digits) {
          sb.Insert(0, "0", digits - s.Length);
        }
      }
      sb.Insert(0, NumberPrototype.GetString((long)bigInt, radix));
      if (isMinus) {
        sb.Insert(0, '-');
      }
      return sb.ToString();
    }

    [EcmaSpecification("ToBigInt", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ToBigInt(this EcmaValue value) {
      value = value.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      switch (value.Type) {
        case EcmaValueType.Undefined:
        case EcmaValueType.Null:
        case EcmaValueType.Number:
        case EcmaValueType.Symbol:
          throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToBigInt, value);
        case EcmaValueType.Boolean:
          return value.ToBoolean() ? value1n : value0n;
        case EcmaValueType.BigInt:
          return value;
        case EcmaValueType.String:
          return ToBigInt(value.ToStringOrThrow());
      }
      throw new ArgumentException("Unknown value type", "value");
    }

    [EcmaSpecification("ToBigInt64", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ToBigInt64(this EcmaValue value) {
      value = value.ToBigInt();
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        return value;
      }
      byte[] byteArray = ((BigInteger)value.GetUnderlyingObject()).ToByteArray();
      return ToBigInt(BitConverter.ToInt64(byteArray, 0));
    }

    [EcmaSpecification("ToBigUInt64", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ToBigUInt64(this EcmaValue value) {
      value = value.ToBigInt();
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        long v = value.ToInt64();
        return v >= mod63 ? ToBigInt(new BigInteger(unchecked((ulong)v))) : value;
      }
      BigInteger rem = BigInteger.Remainder((BigInteger)value.GetUnderlyingObject(), mod64);
      return ToBigInt(rem < 0 ? rem + mod64 : rem);
    }

    public static EcmaValue ToBigIntN(EcmaValue value, int bits) {
      value = value.ToBigInt();
      if (bits <= 0) {
        return value0n;
      }
      if (bits == 64) {
        return ToBigInt64(value);
      }
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        if (bits > 64) {
          return value;
        }
        long r = 1 << bits;
        long m = 1 << (bits - 1);
        long v = value.ToInt64() % r;
        return ToBigInt(v < -m ? v + r : v >= m ? v - r : v);
      }
      BigInteger u = ToBigInteger(value);
      BigInteger bitsm0 = BigInteger.Pow(2, bits);
      BigInteger bitsm1 = BigInteger.Pow(2, bits - 1);
      BigInteger rem = BigInteger.Remainder(u, bitsm0);
      return ToBigInt(rem < -bitsm1 ? rem + bitsm1 : rem >= bitsm1 ? rem - bitsm0 : rem);
    }

    public static EcmaValue ToBigUIntN(EcmaValue value, int bits) {
      value = value.ToBigInt();
      if (bits <= 0) {
        return value0n;
      }
      if (bits == 64) {
        return ToBigUInt64(value);
      }
      if (EcmaValue.GetNumberCoercion(value) == EcmaNumberType.BigInt64) {
        if (bits > 64) {
          return ToBigInt(unchecked((ulong)value.ToInt64()));
        }
        long v = value.ToInt64();
        long mask = (1 << bits) - 1;
        return ToBigInt(unchecked((ulong)(v & mask)));
      }
      BigInteger u = ToBigInteger(value);
      BigInteger bitsm0 = BigInteger.Pow(2, bits);
      BigInteger rem = BigInteger.Remainder(u, bitsm0);
      return ToBigInt(rem < 0 ? rem + bitsm0 : rem);
    }

    public static int Compare(EcmaValue x, EcmaValue y) {
      if (x.Type != EcmaValueType.BigInt) {
        return -Compare(y, x);
      }
      BigInteger bigInt = ToBigInteger(x);
      if (EcmaValue.GetNumberCoercion(y) == EcmaNumberType.Double) {
        return ((double)bigInt).CompareTo(y.ToDouble());
      }
      return bigInt.CompareTo(y.ToInt64());
    }

    public static EcmaValue Add(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      try {
        return ToBigInt(u + v);
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue Add64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      try {
        return ToBigInt(checked(u + v));
      } catch (OverflowException) {
        BigIntBinder bigInt = new BigIntBinder(new BigInteger(u) + new BigInteger(v));
        return bigInt.ToValue();
      }
    }

    public static EcmaValue Subtract(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u - v);
    }

    public static EcmaValue Subtract64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      try {
        return ToBigInt(checked(u - v));
      } catch (OverflowException) {
        BigIntBinder bigInt = new BigIntBinder(new BigInteger(u) - new BigInteger(v));
        return bigInt.ToValue();
      }
    }

    public static EcmaValue Multiply(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      try {
        return ToBigInt(u * v);
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue Multiply64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long a = x.ToInt64();
      long b = y.ToInt64();
      try {
        return ToBigInt(checked(a * b));
      } catch (OverflowException) {
        BigIntBinder bigInt = new BigIntBinder(new BigInteger(a) * new BigInteger(b));
        return bigInt.ToValue();
      }
    }

    public static EcmaValue Divide(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u / v);
    }

    public static EcmaValue Divide64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      return ToBigInt(u / v);
    }

    public static EcmaValue Mod(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u % v);
    }

    public static EcmaValue Mod64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      return ToBigInt(u % v);
    }

    public static EcmaValue Pow(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      if (v < 0) {
        throw new EcmaRangeErrorException(InternalString.Error.ExponentMustBePositive);
      }
      if (u == BigInteger.Zero || u == BigInteger.One) {
        return x;
      }
      if (u == BigInteger.MinusOne) {
        return v.IsEven ? value1n : x;
      }
      if (v > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
      try {
        return ToBigInt(BigInteger.Pow(u, (int)v));
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue Pow64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      if (v < 0) {
        throw new EcmaRangeErrorException(InternalString.Error.ExponentMustBePositive);
      }
      try {
        return ToBigInt(Math.Pow(u, v));
      } catch (OverflowException) { }
      if (v > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
      try {
        return ToBigInt(BigInteger.Pow(u, (int)v));
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue BitwiseAnd(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u & v);
    }

    public static EcmaValue BitwiseAnd64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      return ToBigInt(u & v);
    }

    public static EcmaValue BitwiseOr(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u | v);
    }

    public static EcmaValue BitwiseOr64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      return ToBigInt(u | v);
    }

    public static EcmaValue ExclusiveOr(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      return ToBigInt(u ^ v);
    }

    public static EcmaValue ExclusiveOr64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      return ToBigInt(u ^ v);
    }

    public static EcmaValue LeftShift(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      BigInteger u = ToBigInteger(x);
      BigInteger v = ToBigInteger(y);
      if (v > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
      try {
        return ToBigInt(u << (int)v);
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue LeftShift64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      if (u == 0 || v <= -64) {
        return value0n;
      }
      if (v < 0) {
        return ToBigInt(u >> -(int)v);
      }
      if (v < 64) {
        long w = u << (int)v;
        if (u > 0 ? w > u : w < u) {
          return ToBigInt(w);
        }
      }
      if (v > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
      try {
        return ToBigInt(new BigInteger(u) << (int)v);
      } catch (OutOfMemoryException) {
        throw new EcmaRangeErrorException(InternalString.Error.BigIntOverflow);
      }
    }

    public static EcmaValue RightShift(EcmaValue x, EcmaValue y) {
      return LeftShift(x, -y);
    }

    public static EcmaValue RightShift64(EcmaValue x, EcmaValue y) {
      ThrowIfArgumentNotBigInt(x);
      ThrowIfArgumentNotBigInt(y);
      long u = x.ToInt64();
      long v = y.ToInt64();
      if (v < 0) {
        return LeftShift64(x, -v);
      }
      if (v >= 64) {
        return value0n;
      }
      return ToBigInt(u >> (int)v);
    }

    public static EcmaValue Negate(EcmaValue x) {
      ThrowIfArgumentNotBigInt(x);
      BigInteger u = ToBigInteger(x);
      return ToBigInt(-u);
    }

    public static EcmaValue Negate64(EcmaValue x) {
      ThrowIfArgumentNotBigInt(x);
      long u = x.ToInt64();
      return ToBigInt(-u);
    }

    private static bool TryParse(string inputString, int radix, int int64MaxLen, out EcmaValue result) {
      int len = inputString.Length;
      if (len <= int64MaxLen) {
        // shortcut to 64-bit integer storage if decimal is atmost N digits in base R which is less than Int64.MaxValue
        if (TryParseInt64(inputString, radix, out long longValue)) {
          result = ToBigInt(longValue);
          return true;
        }
        result = default;
        return false;
      }
      List<long> trunks = new List<long>();
      BigInteger value = BigInteger.Zero;
      BigInteger factor = BigInteger.Pow(radix, int64MaxLen);
      for (; len > int64MaxLen; len -= int64MaxLen) {
        if (!TryParseInt64(inputString.Substring(len - int64MaxLen, int64MaxLen), radix, out long longValue)) {
          result = default;
          return false;
        }
        trunks.Insert(0, longValue);
      }
      if (len > 0) {
        if (!TryParseInt64(inputString.Substring(0, len), radix, out long longValue)) {
          result = default;
          return false;
        }
        trunks.Insert(0, longValue);
      }
      foreach (long t in trunks) {
        value = value * factor + t;
      }
      result = ToBigInt(value);
      return true;
    }

    private static bool TryParseInt64(string inputString, int radix, out long result) {
      int maxDigit = '0' + Math.Min(radix - 1, 9);
      int maxAlphaL = 'a' + (radix - 11);
      int maxAlphaC = 'A' + (radix - 11);
      long value = 0;
      for (int i = 0, len = inputString.Length; i < len; i++) {
        char ch = inputString[i];
        if (ch >= '0' && ch <= maxDigit) {
          value = value * radix + (ch - '0');
          continue;
        }
        if (ch >= 'a' && ch <= maxAlphaL) {
          value = value * radix + (ch - 'a' + 10);
          continue;
        }
        if (ch >= 'A' && ch <= maxAlphaC) {
          value = value * radix + (ch - 'A' + 10);
          continue;
        }
        result = default;
        return false;
      }
      result = value;
      return true;
    }

    private static BigInteger ToBigInteger(EcmaValue x) {
      object obj = x.GetUnderlyingObject();
      return obj is BigInteger bigInt ? bigInt : (long)obj;
    }

    private static void ThrowIfArgumentNotBigInt(EcmaValue x) {
      if (x.Type != EcmaValueType.BigInt) {
        throw new EcmaTypeErrorException(InternalString.Error.CannotMixBigIntAndNumber);
      }
    }
  }
}
#endif
