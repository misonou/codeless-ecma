using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringPrototype)]
  internal static class StringPrototype {
    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) {
      return EcmaValueUtility.GetIntrinsicPrimitiveValue(thisArg, EcmaValueType.String);
    }

    [IntrinsicMember]
    [EcmaSpecification("thisStringValue", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ValueOf([This] EcmaValue thisArg) {
      return EcmaValueUtility.GetIntrinsicPrimitiveValue(thisArg, EcmaValueType.String);
    }

    [IntrinsicMember]
    public static EcmaValue CharAt([This] EcmaValue value, EcmaValue index) {
      string str = value.RequireObjectCoercible().ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return String.Empty;
      }
      return new String(str[pos], 1);
    }

    [IntrinsicMember]
    public static EcmaValue CharCodeAt([This] EcmaValue value, EcmaValue index) {
      string str = value.RequireObjectCoercible().ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return EcmaValue.NaN;
      }
      return str[pos];
    }

    [IntrinsicMember]
    public static EcmaValue CodePointAt([This] EcmaValue value, EcmaValue index) {
      string str = value.RequireObjectCoercible().ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return EcmaValue.Undefined;
      }
      char ch = str[pos];
      if (ch < 0xdb00 || ch > 0xdbff || pos + 1 == str.Length) {
        return ch;
      }
      char ch2 = str[pos + 1];
      if (ch2 < 0xdc00 || ch2 > 0xdfff) {
        return ch;
      }
      return (ch - 0xD800) * 0x400 + (ch2 - 0xDC00) + 0x10000;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Concat([This] EcmaValue value, params EcmaValue[] args) {
      string str = value.RequireObjectCoercible().ToString();
      foreach (EcmaValue v in args) {
        str += v.ToString();
      }
      return str;
    }

    [IntrinsicMember]
    public static EcmaValue IndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      string str = value.RequireObjectCoercible().ToString();
      string searchString = needle.ToString();
      return str.IndexOf(searchString, ClampPosition(str, position.ToInt32()));
    }

    [IntrinsicMember]
    public static EcmaValue LastIndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      string str = value.RequireObjectCoercible().ToString();
      string searchString = needle.ToString();
      EcmaValue pos = position.ToNumber();
      if (pos == EcmaValue.NaN || pos == EcmaValue.Infinity) {
        return str.LastIndexOf(searchString);
      }
      return str.LastIndexOf(searchString, ClampPosition(str, position.ToInt32()));
    }

    [IntrinsicMember]
    public static EcmaValue LocaleCompare([This] EcmaValue value, EcmaValue comparand) {
      return String.Compare(value.RequireObjectCoercible().ToString(), comparand.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue ToLowerCase([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().ToLowerInvariant();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleLowerCase([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().ToLower();
    }

    [IntrinsicMember]
    public static EcmaValue ToUpperCase([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().ToUpperInvariant();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleUpperCase([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().ToUpper();
    }

    [IntrinsicMember]
    public static EcmaValue PadStart([This] EcmaValue value, EcmaValue maxLength, EcmaValue fillString) {
      string str = value.RequireObjectCoercible().ToString();
      string filler = fillString.Type == EcmaValueType.Undefined ? " " : fillString.ToString();
      if (filler.Length == 0) {
        return str;
      }
      int intMaxLength = maxLength.ToInt32();
      if (intMaxLength <= str.Length) {
        return str;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, filler, (intMaxLength - str.Length) / filler.Length + 1);
      sb.Length = intMaxLength - str.Length;
      return sb.ToString() + str;
    }

    [IntrinsicMember]
    public static EcmaValue PadEnd([This] EcmaValue value, EcmaValue maxLength, EcmaValue fillString) {
      string str = value.RequireObjectCoercible().ToString();
      string filler = fillString.Type == EcmaValueType.Undefined ? " " : fillString.ToString();
      if (filler.Length == 0) {
        return str;
      }
      int intMaxLength = maxLength.ToInt32();
      if (intMaxLength <= str.Length) {
        return str;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, filler, (intMaxLength - str.Length) / filler.Length + 1);
      sb.Length = intMaxLength - str.Length;
      return str + sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue EndsWith([This] EcmaValue value, EcmaValue searchString, EcmaValue endPosition) {
      string str = value.RequireObjectCoercible().ToString();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.endsWith must not be a regular expression");
      }
      if (endPosition.Type != EcmaValueType.Undefined) {
        str = str.Substring(0, ClampPosition(str, endPosition.ToInt32()));
      }
      return str.EndsWith(searchString.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue StartsWith([This] EcmaValue value, EcmaValue searchString, EcmaValue startPosition) {
      string str = value.RequireObjectCoercible().ToString();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.startsWith must not be a regular expression");
      }
      if (startPosition.Type != EcmaValueType.Undefined) {
        str = str.Substring(ClampPosition(str, startPosition.ToInt32()));
      }
      return str.StartsWith(searchString.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue Includes([This] EcmaValue value, EcmaValue searchString, EcmaValue position) {
      string str = value.RequireObjectCoercible().ToString();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.includes must not be a regular expression");
      }
      if (position.Type != EcmaValueType.Undefined) {
        str = str.Substring(ClampPosition(str, position.ToInt32()));
      }
      return str.Contains(searchString.ToString());
    }
    
    [IntrinsicMember]
    public static EcmaValue Repeat([This] EcmaValue value, EcmaValue count) {
      string str = value.RequireObjectCoercible().ToString();
      count = count.ToNumber();
      if (count < 0 || count == EcmaValue.Infinity) {
        throw new EcmaRangeErrorException("");
      }
      if (count == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, str, count.ToInt32());
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Normalize([This] EcmaValue value, EcmaValue form) {
      value.RequireObjectCoercible();
      NormalizationForm f;
      if (form.Type == EcmaValueType.Undefined) {
        f = NormalizationForm.FormC;
      } else {
        switch (form.ToString()) {
          case "NFC":
            f = NormalizationForm.FormC;
            break;
          case "NFD":
            f = NormalizationForm.FormD;
            break;
          case "NFKC":
            f = NormalizationForm.FormKC;
            break;
          case "NFKD":
            f = NormalizationForm.FormKD;
            break;
          default:
            throw new EcmaRangeErrorException("");
        }
      }
      return value.ToString().Normalize(f);
    }

    [IntrinsicMember]
    public static EcmaValue Substr([This] EcmaValue value, EcmaValue start, EcmaValue length) {
      string str = value.RequireObjectCoercible().ToString();
      int pos = ClampPosition(str, start.ToInt32());
      if (pos < 0) {
        pos = Math.Max(0, pos + str.Length);
      }
      int len = Math.Min(Math.Max(0, length.ToInt32()), str.Length - pos);
      if (len <= 0) {
        return String.Empty;
      }
      return str.Substring(pos, len);
    }

    [IntrinsicMember]
    public static EcmaValue Substring([This] EcmaValue value, EcmaValue start, EcmaValue end) {
      string str = value.RequireObjectCoercible().ToString();
      int spos = ClampPosition(str, start.ToInt32());
      int epos = ClampPosition(str, end.Type == EcmaValueType.Undefined ? str.Length : end.ToInt32());
      if (epos < spos) {
        return str.Substring(epos, spos - epos);
      }
      return str.Substring(spos, epos - spos);
    }

    [IntrinsicMember]
    public static EcmaValue Trim([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().Trim();
    }

    [IntrinsicMember]
    public static EcmaValue TrimLeft([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().TrimStart();
    }

    [IntrinsicMember]
    public static EcmaValue TrimRight([This] EcmaValue value) {
      return value.RequireObjectCoercible().ToString().TrimEnd();
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue value, EcmaValue start, EcmaValue end) {
      string str = value.RequireObjectCoercible().ToString();
      int spos = start.ToInt32();
      int epos = end.Type == EcmaValueType.Undefined ? str.Length : end.ToInt32();
      spos = spos < 0 ? Math.Max(spos + str.Length, 0) : Math.Min(spos, str.Length);
      epos = epos < 0 ? Math.Max(epos + str.Length, 0) : Math.Min(epos, str.Length);
      return str.Substring(spos, Math.Max(epos - spos, 0));
    }

    [IntrinsicMember]
    public static EcmaValue Match([This] EcmaValue value, EcmaValue regexp) {
      // TODO
      return CallRegExpGeneric(value, regexp, WellKnownSymbol.Match);
    }

    [IntrinsicMember]
    public static EcmaValue Replace([This] EcmaValue value, EcmaValue regexp, EcmaValue replacement) {
      // TODO
      return CallRegExpGeneric(value, regexp, WellKnownSymbol.Replace, replacement);
    }

    [IntrinsicMember]
    public static EcmaValue Search([This] EcmaValue value, EcmaValue regexp) {
      // TODO
      return CallRegExpGeneric(value, regexp, WellKnownSymbol.Search);
    }

    [IntrinsicMember]
    public static EcmaValue Split([This] EcmaValue value, EcmaValue regexp) {
      // TODO
      return CallRegExpGeneric(value, regexp, WellKnownSymbol.Split);
    }

    private static EcmaValue CallRegExpGeneric(EcmaValue value, EcmaValue regexp, WellKnownSymbol sym, params EcmaValue[] args) {
      value.RequireObjectCoercible();
      if (!regexp.IsNullOrUndefined) {
        EcmaValue method = regexp.ToRuntimeObject().GetMethod(sym);
        if (!method.IsNullOrUndefined) {
          return method.Call(regexp, value);
        }
      }
      // TODO: RegExpCreate
      // EcmaRegExp.TryParse();
      throw new NotSupportedException();
    }

    private static int ClampPosition(string str, int position) {
      return Math.Min(Math.Max(0, position), str.Length);
    }
  }
}