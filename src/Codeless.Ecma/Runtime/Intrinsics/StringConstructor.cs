using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringConstructor)]
  internal static class StringConstructor {
    [IntrinsicConstructor(ObjectType = typeof(PrimitiveObject), Prototype = WellKnownObject.StringPrototype)]
    public static EcmaValue String([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor == null) {
        return value.ToString();
      }
      ((PrimitiveObject)thisValue.ToObject()).PrimitiveValue = value.ToStringOrThrow();
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue FromCharCode(params EcmaValue[] codeUnits) {
      StringBuilder sb = new StringBuilder(codeUnits.Length);
      foreach (EcmaValue v in codeUnits) {
        sb.Append((char)v.ToUInt16());
      }
      return sb.ToString();
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue FromCharPoint(params EcmaValue[] codeUnits) {
      StringBuilder sb = new StringBuilder(codeUnits.Length);
      foreach (EcmaValue v in codeUnits) {
        EcmaValue num = v.ToNumber();
        if (!num.IsInteger || num < 0 || num > 0x10FFFF) {
          throw new EcmaRangeErrorException("Invalid code point {0}", num);
        }
        int intValue = (int)num;
        if (intValue <= 0xFFFF) {
          sb.Append((char)intValue);
        } else {
          sb.Append((char)((intValue - 0x10000) / 0x400 + 0xD800));
          sb.Append((char)((intValue - 0x10000) % 0x400 + 0xDC00));
        }
      }
      return sb.ToString();
    }
  }
}
