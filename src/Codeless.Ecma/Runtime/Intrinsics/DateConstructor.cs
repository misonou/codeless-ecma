using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DateConstructor)]
  internal static class DateConstructor {
    [IntrinsicConstructor(ObjectType = typeof(EcmaDate))]
    [IntrinsicMember(FunctionLength = 7)]
    public static EcmaValue Date([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, params EcmaValue[] args) {
      if (constructor == null) {
        return EcmaTimestamp.FromNativeDateTime(DateTime.Now).ToString();
      }
      EcmaTimestamp timestamp = default;
      if (args.Length == 0) {
        timestamp = EcmaTimestamp.FromNativeDateTime(DateTime.UtcNow);
      } else if (args.Length == 1) {
        if (args[0].GetUnderlyingObject() is EcmaDate dt) {
          timestamp = dt.Timestamp;
        } else {
          EcmaValue primitive = args[0].ToPrimitive();
          if (primitive.Type == EcmaValueType.String) {
            timestamp = ParseInternal((string)args[0]);
          } else {
            EcmaValue num = primitive.ToNumber();
            timestamp = num.IsFinite ? new EcmaTimestamp(num.ToInt64()) : EcmaTimestamp.Invalid;
          }
        }
      } else {
        long[] checkedValues = new long[args.Length];
        for (int i = 0, length = args.Length; i < length; i++) {
          if (!DatePrototype.ValidateArgument(args[i], out checkedValues[i])) {
            timestamp = EcmaTimestamp.Invalid;
          }
        }
        if (timestamp.Value != EcmaTimestamp.Invalid.Value) {
          timestamp = new EcmaTimestamp(EcmaTimestamp.GetTimestamp(EcmaTimestamp.LocalEpoch.Value, 0, checkedValues));
        }
      }
      thisValue.GetUnderlyingObject<EcmaDate>().Timestamp = timestamp;
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Now() {
      return EcmaTimestamp.FromNativeDateTime(DateTime.Now).Value;
    }

    [IntrinsicMember(FunctionLength = 7)]
    public static EcmaValue UTC(params EcmaValue[] args) {
      if (args.Length == 0) {
        return EcmaValue.NaN;
      }
      long[] checkedValues = new long[args.Length];
      for (int i = 0, length = args.Length; i < length; i++) {
        if (!DatePrototype.ValidateArgument(args[i], out checkedValues[i])) {
          return EcmaValue.NaN;
        }
      }
      if (checkedValues[0] >= 0 && checkedValues[0] <= 99) {
        checkedValues[0] += 1900;
      }
      EcmaTimestamp timestamp = new EcmaTimestamp(EcmaTimestamp.GetTimestampUtc(0, 0, checkedValues));
      if (timestamp.IsValid) {
        return timestamp.Value;
      }
      return EcmaValue.NaN;
    }

    [IntrinsicMember]
    public static EcmaValue Parse(EcmaValue str) {
      EcmaTimestamp ts = ParseInternal(str.ToString(true));
      return ts.IsValid ? ts.Value : EcmaValue.NaN;
    }

    public static EcmaTimestamp ParseInternal(string inputString) {
      Guard.ArgumentNotNull(inputString, "inputString");
      Match r = Regex.Match(inputString, "([+-]?\\d{3,6})-(\\d{2})-(\\d{2})(?:T(\\d{2}):(\\d{2}):(\\d{2})(?:\\.(\\d{3}))?Z?)?");
      if (r.Success) {
        long y = Int64.Parse(r.Groups[1].Value);
        long m = Int64.Parse(r.Groups[2].Value);
        long d = Int64.Parse(r.Groups[3].Value);
        if (r.Groups[4].Success) {
          long h = Int64.Parse(r.Groups[4].Value);
          long n = Int64.Parse(r.Groups[5].Value);
          long s = Int64.Parse(r.Groups[6].Value);
          long ms = r.Groups[7].Success ? Int64.Parse(r.Groups[7].Value) : 0;
          return new EcmaTimestamp(EcmaTimestamp.GetTimestampUtc(0, 0, y, m - 1, d, h, n, s, ms));
        }
        return new EcmaTimestamp(EcmaTimestamp.GetTimestampUtc(0, 0, y, m - 1, d));
      }
      DateTime dt;
      if (DateTime.TryParseExact(inputString, "ddd, dd MMM yyyy HH:mm:ss \\G\\M\\T", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dt)) {
        return EcmaTimestamp.FromNativeDateTime(dt);
      }
      int index1 = inputString.IndexOf('(');
      int index2 = inputString.IndexOf(')');
      if (index1 >= 0 && index2 >= 0 && DateTime.TryParseExact(inputString.Substring(0, index1), "ddd MMM dd yyyy HH:mm:ss \\G\\M\\Tzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, out dt)) {
        return EcmaTimestamp.FromNativeDateTime(dt);
      }
      return DateTime.TryParse(inputString, out dt) ? EcmaTimestamp.FromNativeDateTime(dt) : EcmaTimestamp.Invalid;
    }
  }
}
