using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DateConstructor)]
  internal static class DateConstructor {
    [IntrinsicConstructor(ObjectType = typeof(EcmaDate))]
    [IntrinsicMember(FunctionLength = 7)]
    public static EcmaValue Date([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, params EcmaValue[] args) {
      if (constructor == null) {
        if (args.Length == 0) {
          return new EcmaTimestamp().ToString();
        }
        long value;
        if (DatePrototype.ValidateArgument(args[0], out value)) {
          return new EcmaTimestamp(value).ToString();
        }
        if (args[0].Type == EcmaValueType.String) {
          return Parse(args[0]);
        }
        return EcmaTimestamp.Invalid.ToString();
      }

      EcmaTimestamp? timestamp = null;
      if (args.Length == 0) {
        timestamp = EcmaTimestamp.FromNativeDateTime(DateTime.UtcNow);
      } else if (args[0].Type == EcmaValueType.String) {
        EcmaValue parsedValue = Parse(args[0]);
        timestamp = parsedValue.IsNaN ? EcmaTimestamp.Invalid : new EcmaTimestamp((long)parsedValue);
      } else {
        long[] checkedValues = new long[args.Length];
        for (int i = 0, length = args.Length; i < length; i++) {
          if (!DatePrototype.ValidateArgument(args[i], out checkedValues[i])) {
            timestamp = EcmaTimestamp.Invalid;
          }
        }
        if (timestamp == null) {
          timestamp = new EcmaTimestamp(EcmaTimestamp.GetTimestamp(0, 0, checkedValues));
        }
      }
      ((EcmaDate)thisValue.ToObject()).Timestamp = timestamp.Value;
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 7)]
    public static EcmaValue UTC(params EcmaValue[] args) {
      if (args.Length == 0) {
        return new EcmaTimestamp().Value;
      }
      long[] checkedValues = new long[args.Length];
      for (int i = 0, length = args.Length; i < length; i++) {
        if (!DatePrototype.ValidateArgument(args[i], out checkedValues[i])) {
          return EcmaValue.NaN;
        }
      }
      return EcmaTimestamp.GetTimestampUtc(0, 0, checkedValues);
    }

    [IntrinsicMember]
    public static EcmaValue Parse(EcmaValue str) {
      string inputString = str.ToString();
      DateTime dt;
      if (DateTime.TryParseExact(inputString, "ddd, dd MMM yyyy HH:mm:ss \\G\\M\\T", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dt) ||
          DateTime.TryParseExact(inputString, "yyyy-MM-dd\\THH:mm:ss\\Z", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dt) ||
          DateTime.TryParseExact(inputString, "yyyy-MM-dd\\THH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dt) ||
          DateTime.TryParseExact(inputString, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dt)) {
        return EcmaTimestamp.FromNativeDateTime(dt).Value;
      }
      int index1 = inputString.IndexOf('(');
      int index2 = inputString.IndexOf(')');
      if (index1 >= 0 && index2 >= 0 && DateTime.TryParseExact(inputString.Substring(0, index1), "ddd MMM dd yyyy HH:mm:ss \\G\\M\\Tzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out dt)) {
        return EcmaTimestamp.FromNativeDateTime(dt).Value;
      }
      return DateTime.TryParse(inputString, out dt) ? EcmaTimestamp.FromNativeDateTime(dt).Value : EcmaTimestamp.Invalid.Value;
    }
  }
}