using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DatePrototype)]
  internal static class DatePrototype {
    [IntrinsicMember]
    public static EcmaValue GetTime([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.Value;
    }

    [IntrinsicMember]
    public static EcmaValue GetDate([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.Value;
    }

    [IntrinsicMember]
    public static EcmaValue GetDay([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.WeekDay);
    }

    [IntrinsicMember]
    public static EcmaValue GetFullYear([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Year);
    }

    [IntrinsicMember]
    public static EcmaValue GetHours([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Hours);
    }

    [IntrinsicMember]
    public static EcmaValue GetMilliseconds([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Milliseconds);
    }

    [IntrinsicMember]
    public static EcmaValue GetMinutes([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Minutes);
    }

    [IntrinsicMember]
    public static EcmaValue GetMonth([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Month);
    }

    [IntrinsicMember]
    public static EcmaValue GetSeconds([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponent(EcmaDateComponent.Seconds);
    }

    [IntrinsicMember]
    public static EcmaValue GetTimezoneOffset([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : EcmaTimestamp.TimezoneOffset;
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCDate([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : EcmaTimestamp.TimezoneOffset;
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCDay([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Date);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCFullYear([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Year);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCHours([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Hours);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCMilliseconds([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Milliseconds);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCMinutes([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Minutes);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCMonth([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Month);
    }

    [IntrinsicMember]
    public static EcmaValue GetUTCSeconds([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.GetComponentUtc(EcmaDateComponent.Seconds);
    }

    [IntrinsicMember]
    public static EcmaValue SetTime([This] EcmaValue value, EcmaValue time) {
      EcmaTimestamp dt = time.IsFinite ? new EcmaTimestamp(time.ToInt64()) : EcmaTimestamp.Invalid;
      EcmaValueUtility.SetIntrinsicValue(value, dt);
      return dt.IsValid ? EcmaValue.NaN : dt.Value;
    }

    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue SetFullYear([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponents(value, EcmaDateComponent.Year, 3, args);
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetMonth([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponents(value, EcmaDateComponent.Month, 2, args);
    }

    [IntrinsicMember]
    public static EcmaValue SetDate([This] EcmaValue value, EcmaValue date) {
      return SetComponents(value, EcmaDateComponent.Date, 1, new[] { date });
    }

    [IntrinsicMember(FunctionLength = 4)]
    public static EcmaValue SetHours([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponents(value, EcmaDateComponent.Hours, 4, args);
    }

    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue SetMinutes([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponents(value, EcmaDateComponent.Minutes, 3, args);
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetSeconds([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponents(value, EcmaDateComponent.Seconds, 2, args);
    }

    [IntrinsicMember]
    public static EcmaValue SetMilliseconds([This] EcmaValue value, EcmaValue ms) {
      return SetComponents(value, EcmaDateComponent.Milliseconds, 1, new[] { ms });
    }

    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue SetUTCFullYear([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponentsUtc(value, EcmaDateComponent.Year, 3, args);
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetUTCMonth([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponentsUtc(value, EcmaDateComponent.Month, 2, args);
    }

    [IntrinsicMember]
    public static EcmaValue SetUTCDate([This] EcmaValue value, EcmaValue date) {
      return SetComponentsUtc(value, EcmaDateComponent.Date, 1, new[] { date });
    }

    [IntrinsicMember(FunctionLength = 4)]
    public static EcmaValue SetUTCHours([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponentsUtc(value, EcmaDateComponent.Hours, 4, args);
    }

    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue SetUTCMinutes([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponentsUtc(value, EcmaDateComponent.Minutes, 3, args);
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetUTCSeconds([This] EcmaValue value, params EcmaValue[] args) {
      return SetComponentsUtc(value, EcmaDateComponent.Seconds, 2, args);
    }

    [IntrinsicMember]
    public static EcmaValue SetUTCMilliseconds([This] EcmaValue value, EcmaValue ms) {
      return SetComponentsUtc(value, EcmaDateComponent.Milliseconds, 1, new[] { ms });
    }

    [IntrinsicMember]
    public static EcmaValue ValueOf([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? EcmaValue.NaN : dt.Value;
    }

    [IntrinsicMember]
    public static string ToString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToString(DateTimeFormatInfo.InvariantInfo);
    }

    [IntrinsicMember]
    public static string ToISOString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToISOString();
    }

    [IntrinsicMember]
    public static string ToUTCString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToUTCString(DateTimeFormatInfo.InvariantInfo);
    }

    [IntrinsicMember]
    public static string ToDateString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToDateString(DateTimeFormatInfo.InvariantInfo);
    }

    [IntrinsicMember]
    public static string ToTimeString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToTimeString(DateTimeFormatInfo.InvariantInfo);
    }

    [IntrinsicMember]
    public static string ToLocaleString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToString(DateTimeFormatInfo.CurrentInfo);
    }

    [IntrinsicMember]
    public static string ToLocaleDateString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToDateString(DateTimeFormatInfo.CurrentInfo);
    }

    [IntrinsicMember]
    public static string ToLocaleTimeString([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return dt.ToTimeString(DateTimeFormatInfo.CurrentInfo);
    }

    [IntrinsicMember]
    public static EcmaValue ToJSON([This] EcmaValue value) {
      EcmaTimestamp dt = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
      return !dt.IsValid ? null : dt.ToISOString();
    }

    public static bool ValidateArgument(EcmaValue value, out long longValue) {
      value = value.ToNumber();
      if (!value.IsFinite) {
        longValue = 0;
        return false;
      }
      longValue = value.ToInt64();
      return true;
    }

    private static EcmaValue SetComponents(EcmaValue value, EcmaDateComponent start, int max, EcmaValue[] args) {
      EcmaTimestamp result = EcmaTimestamp.Invalid;
      long[] checkedValues = new long[Math.Min(args.Length, max)];
      int i = 0;
      for (int length = checkedValues.Length; i < length; i++) {
        if (!ValidateArgument(args[i], out checkedValues[i])) {
          break;
        }
      }
      if (i == checkedValues.Length) {
        EcmaTimestamp current = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
        if (!current.IsValid && start == EcmaDateComponent.Year) {
          current = EcmaTimestamp.LocalEpoch;
        }
        if (current.IsValid) {
          result = (EcmaTimestamp)EcmaTimestamp.GetTimestamp(current.Value, (int)start, checkedValues);
        }
      }
      EcmaValueUtility.SetIntrinsicValue(value, result);
      return result.Value;
    }

    private static EcmaValue SetComponentsUtc(EcmaValue value, EcmaDateComponent start, int max, EcmaValue[] args) {
      EcmaTimestamp result = EcmaTimestamp.Invalid;
      long[] checkedValues = new long[Math.Min(args.Length, max)];
      int i = 0;
      for (int length = checkedValues.Length; i < length; i++) {
        if (!ValidateArgument(args[i], out checkedValues[i])) {
          break;
        }
      }
      if (i == checkedValues.Length) {
        EcmaTimestamp current = EcmaValueUtility.GetIntrinsicValue<EcmaTimestamp>(value);
        if (!current.IsValid && start == EcmaDateComponent.Year) {
          current = EcmaTimestamp.LocalEpoch;
        }
        if (current.IsValid) {
          result = (EcmaTimestamp)EcmaTimestamp.GetTimestampUtc(current.Value, (int)start, checkedValues);
        }
      }
      EcmaValueUtility.SetIntrinsicValue(value, result);
      return result.Value;
    }
  }
}