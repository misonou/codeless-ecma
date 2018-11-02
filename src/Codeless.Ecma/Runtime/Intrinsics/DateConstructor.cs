using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DateConstructor)]
  internal static class DateConstructor {
    [IntrinsicConstructor]
    [IntrinsicMember(FunctionLength = 7)]
    public static EcmaValue Date([NewTarget] RuntimeObject constructor, params EcmaValue[] args) {
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

      RuntimeObject proto = RuntimeObject.GetPrototypeFromConstructor(constructor, WellKnownObject.DatePrototype);
      if (args.Length == 0) {
        return new EcmaDate(EcmaTimestamp.FromNativeDateTime(DateTime.UtcNow), proto);
      }
      if (args[0].Type == EcmaValueType.String) {
        EcmaValue parsedValue = Parse(args[0]);
        if (parsedValue.IsNaN) {
          return new EcmaDate(EcmaTimestamp.Invalid.Value, proto);
        }
        return new EcmaDate((long)parsedValue, proto);
      }
      long[] checkedValues = new long[args.Length];
      for (int i = 0, length = args.Length; i < length; i++) {
        if (!DatePrototype.ValidateArgument(args[i], out checkedValues[i])) {
          return new EcmaDate(EcmaTimestamp.Invalid.Value, proto);
        }
      }
      return new EcmaDate(EcmaTimestamp.GetTimestamp(0, 0, checkedValues), proto);
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
      throw new NotImplementedException();
    }
  }
}