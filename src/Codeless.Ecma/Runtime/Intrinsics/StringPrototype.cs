using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringPrototype)]
  internal static class StringPrototype {
    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      return thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.String);
    }

    [IntrinsicMember]
    [EcmaSpecification("thisStringValue", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ValueOf([This] EcmaValue thisValue) {
      return thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.String);
    }

    [IntrinsicMember]
    public static EcmaValue CharAt([This] EcmaValue value, EcmaValue index) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return String.Empty;
      }
      return new String(str[pos], 1);
    }

    [IntrinsicMember]
    public static EcmaValue CharCodeAt([This] EcmaValue value, EcmaValue index) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return EcmaValue.NaN;
      }
      return str[pos];
    }

    [IntrinsicMember]
    public static EcmaValue CodePointAt([This] EcmaValue value, EcmaValue index) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int pos = index.ToInt32();
      if (pos < 0 || pos >= str.Length) {
        return default;
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      foreach (EcmaValue v in args) {
        str += v.ToString();
      }
      return str;
    }

    [IntrinsicMember]
    public static EcmaValue IndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      string searchString = needle.ToString();
      return str.IndexOf(searchString, ClampPosition(str, position.ToInt32()));
    }

    [IntrinsicMember]
    public static EcmaValue LastIndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      string searchString = needle.ToString();
      EcmaValue pos = position.ToNumber();
      if (pos == EcmaValue.NaN || pos == EcmaValue.Infinity) {
        return str.LastIndexOf(searchString);
      }
      return str.LastIndexOf(searchString, ClampPosition(str, position.ToInt32()));
    }

    [IntrinsicMember]
    public static EcmaValue LocaleCompare([This] EcmaValue value, EcmaValue comparand) {
      Guard.RequireObjectCoercible(value);
      return String.Compare(value.ToString(), comparand.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue ToLowerCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().ToLowerInvariant();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleLowerCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().ToLower();
    }

    [IntrinsicMember]
    public static EcmaValue ToUpperCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().ToUpperInvariant();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleUpperCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().ToUpper();
    }

    [IntrinsicMember]
    public static EcmaValue PadStart([This] EcmaValue value, EcmaValue maxLength, EcmaValue fillString) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int num = count.ToInt32Checked();
      if (num < 0) {
        throw new EcmaRangeErrorException("First argument to String.prototype.repeat must be positive");
      }
      if (num == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, str, num);
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Normalize([This] EcmaValue value, EcmaValue form) {
      Guard.RequireObjectCoercible(value);
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
            throw new EcmaRangeErrorException("First argument to String.prototype.normalize must be one of the following values: NFC, NFD, NFKC, NFKD");
        }
      }
      return value.ToString().Normalize(f);
    }

    [IntrinsicMember]
    public static EcmaValue Substr([This] EcmaValue value, EcmaValue start, EcmaValue length) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
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
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int spos = ClampPosition(str, start.ToInt32());
      int epos = ClampPosition(str, end.Type == EcmaValueType.Undefined ? str.Length : end.ToInt32());
      if (epos < spos) {
        return str.Substring(epos, spos - epos);
      }
      return str.Substring(spos, epos - spos);
    }

    [IntrinsicMember]
    public static EcmaValue Trim([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().Trim();
    }

    [IntrinsicMember]
    public static EcmaValue TrimLeft([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().TrimStart();
    }

    [IntrinsicMember]
    public static EcmaValue TrimRight([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToString().TrimEnd();
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue value, EcmaValue start, EcmaValue end) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToString();
      int spos = start.ToInt32();
      int epos = end.Type == EcmaValueType.Undefined ? str.Length : end.ToInt32();
      spos = spos < 0 ? Math.Max(spos + str.Length, 0) : Math.Min(spos, str.Length);
      epos = epos < 0 ? Math.Max(epos + str.Length, 0) : Math.Min(epos, str.Length);
      return str.Substring(spos, Math.Max(epos - spos, 0));
    }

    [IntrinsicMember]
    public static EcmaValue Match([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (searcher.HasProperty(WellKnownSymbol.Match)) {
        return CallRegExpGeneric(value, searcher, WellKnownSymbol.Match);
      }
      return value.ToString().IndexOf(searcher.ToString()) >= 0;
    }

    [IntrinsicMember]
    public static EcmaValue Replace([This] EcmaValue value, EcmaValue searcher, EcmaValue replacement) {
      Guard.RequireObjectCoercible(value);
      if (searcher.HasProperty(WellKnownSymbol.Replace)) {
        return CallRegExpGeneric(value, searcher, WellKnownSymbol.Replace, replacement);
      }
      string str = value.ToString();
      string needle = searcher.ToString();
      int index = str.IndexOf(needle);
      return index >= 0 ? str.Substring(0, index) + replacement.ToString() + str.Substring(index + needle.Length) : str;
    }

    [IntrinsicMember]
    public static EcmaValue Search([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (searcher.HasProperty(WellKnownSymbol.Search)) {
        return CallRegExpGeneric(value, searcher, WellKnownSymbol.Search);
      }
      return value.ToString().IndexOf(searcher.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue Split([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (searcher.HasProperty(WellKnownSymbol.Split)) {
        return CallRegExpGeneric(value, searcher, WellKnownSymbol.Split);
      }
      return new EcmaArray(value.ToString().Split(new[] { searcher.ToString() }, StringSplitOptions.None).Select(v => new EcmaValue(v)));
    }

    private static EcmaValue CallRegExpGeneric(EcmaValue value, EcmaValue regexp, WellKnownSymbol sym,  EcmaValue arg = default) {
      RuntimeObject method = regexp.ToObject().GetMethod(sym);
      return method.Call(regexp, value, arg);
    }

    private static int ClampPosition(string str, int position) {
      return Math.Min(Math.Max(0, position), str.Length);
    }
  }
}