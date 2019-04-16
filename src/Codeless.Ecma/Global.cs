using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Global)]
  public static class Global {
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Infinity = EcmaValue.Infinity;

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly EcmaValue NaN = EcmaValue.NaN;

    [IntrinsicMember("undefined", EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Undefined = EcmaValue.Undefined;

    public static readonly EcmaValue Null = EcmaValue.Null;

    public static RuntimeObject This => (RuntimeObject)WellKnownObject.Global;

    public static RuntimeFunction Array => (RuntimeFunction)WellKnownObject.ArrayConstructor;

    public static RuntimeFunction Boolean => (RuntimeFunction)WellKnownObject.BooleanConstructor;

    public static RuntimeFunction Date => (RuntimeFunction)WellKnownObject.DateConstructor;

    public static RuntimeFunction Error => (RuntimeFunction)WellKnownObject.ErrorConstructor;

    public static RuntimeFunction Function => (RuntimeFunction)WellKnownObject.FunctionConstructor;

    public static RuntimeFunction Map => (RuntimeFunction)WellKnownObject.MapConstructor;

    public static RuntimeFunction Number => (RuntimeFunction)WellKnownObject.NumberConstructor;

    public static RuntimeFunction Object => (RuntimeFunction)WellKnownObject.ObjectConstructor;

    public static RuntimeFunction Promise => (RuntimeFunction)WellKnownObject.PromiseConstructor;

    public static RuntimeFunction Set => (RuntimeFunction)WellKnownObject.SetConstructor;

    public static RuntimeFunction String => (RuntimeFunction)WellKnownObject.StringConstructor;

    public static RuntimeFunction Symbol => (RuntimeFunction)WellKnownObject.SymbolConstructor;

    public static RuntimeFunction WeakMap => (RuntimeFunction)WellKnownObject.WeakMapConstructor;

    public static RuntimeFunction WeakSet => (RuntimeFunction)WellKnownObject.WeakSetConstructor;

    [IntrinsicMember]
    public static EcmaValue IsFinite(EcmaValue value) {
      return value.ToNumber().IsFinite;
    }

    [IntrinsicMember]
    public static EcmaValue IsNaN(EcmaValue value) {
      return value.ToNumber().IsNaN;
    }

    public static EcmaValue ParseInt(EcmaValue str, EcmaValue radix) {
      string inputString = str.ToString();
      return EcmaValueUtility.ParseIntInternal(inputString, radix.ToInt32(), true);
    }

    public static EcmaValue ParseFloat(EcmaValue str) {
      string inputString = str.ToString();
      return EcmaValueUtility.ParseFloatInternal(inputString, true);
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURI(EcmaValue str) {
      return Uri.EscapeUriString(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURI(EcmaValue str) {
      return Uri.UnescapeDataString(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURIComponent(EcmaValue str) {
      return Uri.EscapeDataString(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURIComponent(EcmaValue str) {
      return Uri.UnescapeDataString(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue Escape(EcmaValue str) {
      StringBuilder sb = new StringBuilder();
      foreach (char ch in str.ToString()) {
        if (ch == '*' || ch == '+' || ch == '-' || ch == '.' || ch == '/' || ch == '@' || ch == '_' || IsAlphaNumericCharPoint(ch)) {
          sb.Append(ch);
        } else if (ch < 256) {
          sb.AppendFormat("%{0:X2}", (int)ch);
        } else {
          sb.AppendFormat("%u{0:X4}", (int)ch);
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Unescape(EcmaValue str) {
      StringBuilder sb = new StringBuilder();
      StringBuilder seq = new StringBuilder(4);
      using (TextReader reader = new StringReader(str.ToString())) {
        while (true) {
          nextChar:
          int ch = reader.Read();
          if (ch == -1) {
            break;
          }
          if (ch != '%') {
            sb.Append(ch);
            continue;
          }
          int c2 = reader.Read();
          if (c2 == -1) {
            break;
          }
          seq.Remove(0, seq.Length);
          if (c2 == 'u') {
            for (int i = 0; i < 4; i++) {
              int c3 = reader.Read();
              if (c3 == -1) {
                sb.Append(seq.ToString());
                goto end;
              }
              seq.Append((char)c3);
              if (!IsAlphaNumericCharPoint(c3, 'f')) {
                sb.Append(seq.ToString());
                goto nextChar;
              }
            }
            sb.Append((char)Int32.Parse(seq.ToString(), NumberStyles.HexNumber));
          } else if (IsAlphaNumericCharPoint(c2, 'f')) {
            sb.Append((char)c2);
            int c3 = reader.Read();
            if (c3 == -1) {
              sb.Append(seq.ToString());
              goto end;
            }
            seq.Append((char)c3);
            if (!IsAlphaNumericCharPoint(c3, 'f')) {
              sb.Append(seq.ToString());
              goto nextChar;
            }
            sb.Append((char)Int32.Parse(seq.ToString(), NumberStyles.HexNumber));
          } else {
            sb.Append((char)c2);
          }
        }
      }
      end:
      return sb.ToString();
    }

    private static bool IsAlphaNumericCharPoint(int ch, char end = 'z') {
      return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= end) || (ch >= 'A' && ch <= (end - 32));
    }
  }
}
