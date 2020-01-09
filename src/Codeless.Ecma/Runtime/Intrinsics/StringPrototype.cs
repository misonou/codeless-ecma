using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringPrototype)]
  internal static class StringPrototype {
    private static readonly char[] trimChars = "\x09\x0A\x0B\x0C\x0D\x20\xA0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\u2028\u2029\uFEFF".ToCharArray();

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public const int Length = 0;

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
      string str = value.ToStringOrThrow();
      EcmaValue pos = index.ToInteger();
      if (pos < 0 || pos >= str.Length) {
        return String.Empty;
      }
      return new String(str[(int)pos], 1);
    }

    [IntrinsicMember]
    public static EcmaValue CharCodeAt([This] EcmaValue value, EcmaValue index) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      EcmaValue pos = index.ToInteger();
      if (pos < 0 || pos >= str.Length) {
        return EcmaValue.NaN;
      }
      return str[(int)pos];
    }

    [IntrinsicMember]
    public static EcmaValue CodePointAt([This] EcmaValue value, EcmaValue index) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      EcmaValue pos = index.ToInteger();
      if (pos < 0 || pos >= str.Length) {
        return default;
      }
      char ch = str[(int)pos];
      if (ch < 0xd800 || ch > 0xdbff || pos + 1 == str.Length) {
        return ch;
      }
      char ch2 = str[(int)pos + 1];
      if (ch2 < 0xdc00 || ch2 > 0xdfff) {
        return ch;
      }
      return (ch - 0xD800) * 0x400 + (ch2 - 0xDC00) + 0x10000;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Concat([This] EcmaValue value, params EcmaValue[] args) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      foreach (EcmaValue v in args) {
        str += v.ToStringOrThrow();
      }
      return str;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue IndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      string searchString = needle.ToStringOrThrow();
      return str.IndexOf(searchString, position.ToInteger(0, str.Length));
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue LastIndexOf([This] EcmaValue value, EcmaValue needle, EcmaValue position) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      string searchString = needle.ToStringOrThrow();
      EcmaValue pos = position.ToNumber();
      if (pos.IsNaN || !pos.IsFinite) {
        return searchString == "" ? str.Length : str.LastIndexOf(searchString);
      }
      int index = pos.ToInteger(0, str.Length);
      return searchString == "" ? index : str.LastIndexOf(searchString, index);
    }

    [IntrinsicMember]
    public static EcmaValue LocaleCompare([This] EcmaValue value, EcmaValue comparand) {
      Guard.RequireObjectCoercible(value);
      return String.Compare(value.ToStringOrThrow().Normalize(), comparand.ToStringOrThrow().Normalize());
    }

    [IntrinsicMember]
    public static EcmaValue ToLowerCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow().ToLowerInvariant();
      StringBuilder sb = new StringBuilder(str.Length);
      for (int i = 0, len = str.Length; i < len; i++) {
        char ch = str[i];
        string upper = MapLowerSpecialCase(ch);
        if (upper != null) {
          sb.Append(upper);
        } else if (ch == '\u03C3') {
          // conditional mapping for greek letter sigma
          bool useFinalForm = false;
          for (int j = i - 1; j >= 0; j--) {
            if (!IsCaseIgnoreOrSurrogate(str, j) && IsCasedLetter(str, j)) {
              useFinalForm = true;
              break;
            }
          }
          if (useFinalForm) {
            for (int j = i + 1; j < len; j++) {
              if (!IsCaseIgnoreOrSurrogate(str, j) && IsCasedLetter(str, j)) {
                useFinalForm = false;
                break;
              }
            }
          }
          sb.Append(useFinalForm ? '\u03C2' : '\u03C3');
        } else {
          sb.Append(ch);
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleLowerCase([This] EcmaValue value) {
      return ToLowerCase(value);
    }

    [IntrinsicMember]
    public static EcmaValue ToUpperCase([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow().ToUpperInvariant();
      StringBuilder sb = new StringBuilder(str.Length);
      for (int i = 0, len = str.Length; i < len; i++) {
        char ch = str[i];
        string upper = MapUpperSpecialCase(ch);
        if (upper != null) {
          sb.Append(upper);
        } else {
          sb.Append(ch);
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleUpperCase([This] EcmaValue value) {
      return ToUpperCase(value);
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue PadStart([This] EcmaValue value, EcmaValue maxLength, EcmaValue fillString) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      int intMaxLength = maxLength.ToInt32();
      string filler = fillString == default ? " " : fillString.ToStringOrThrow();
      if (intMaxLength <= str.Length) {
        return str;
      }
      if (filler.Length == 0) {
        return str;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, filler, (intMaxLength - str.Length) / filler.Length + 1);
      sb.Length = intMaxLength - str.Length;
      return sb.ToString() + str;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue PadEnd([This] EcmaValue value, EcmaValue maxLength, EcmaValue fillString) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      int intMaxLength = maxLength.ToInt32();
      string filler = fillString == default ? " " : fillString.ToStringOrThrow();
      if (intMaxLength <= str.Length) {
        return str;
      }
      if (filler.Length == 0) {
        return str;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, filler, (intMaxLength - str.Length) / filler.Length + 1);
      sb.Length = intMaxLength - str.Length;
      return str + sb.ToString();
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue EndsWith([This] EcmaValue value, EcmaValue searchString, EcmaValue endPosition) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.endsWith must not be a regular expression");
      }
      string needle = searchString.ToStringOrThrow();
      int pos = endPosition == default ? str.Length : endPosition.ToInteger(0, str.Length);
      if (needle.Length == 0) {
        return true;
      }
      if (pos < needle.Length) {
        return false;
      }
      return str.Substring(pos - needle.Length, needle.Length) == needle;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue StartsWith([This] EcmaValue value, EcmaValue searchString, EcmaValue startPosition) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.startsWith must not be a regular expression");
      }
      string needle = searchString.ToStringOrThrow();
      int pos = startPosition == default ? 0 : startPosition.ToInteger(0, str.Length);
      if (needle.Length == 0) {
        return true;
      }
      if (pos + needle.Length > str.Length) {
        return false;
      }
      return str.Substring(pos, needle.Length) == needle;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Includes([This] EcmaValue value, EcmaValue searchString, EcmaValue position) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      if (searchString.IsRegExp) {
        throw new EcmaTypeErrorException("First argument to String.prototype.includes must not be a regular expression");
      }
      if (position.Type != EcmaValueType.Undefined) {
        str = str.Substring(position.ToInteger(0, str.Length));
      }
      return str.Contains(searchString.ToStringOrThrow());
    }

    [IntrinsicMember]
    public static EcmaValue Repeat([This] EcmaValue value, EcmaValue count) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      EcmaValue num = count.ToInteger();
      if (num < 0 || !num.IsFinite) {
        throw new EcmaRangeErrorException("First argument to String.prototype.repeat must be positive and finite");
      }
      if (num == 0 || str.Length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      sb.Insert(0, str, (int)num);
      return sb.ToString();
    }

    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue Normalize([This] EcmaValue value, EcmaValue form) {
      Guard.RequireObjectCoercible(value);
      NormalizationForm f;
      if (form.Type == EcmaValueType.Undefined) {
        f = NormalizationForm.FormC;
      } else {
        switch (form.ToStringOrThrow()) {
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
      return value.ToStringOrThrow().Normalize(f);
    }

    [IntrinsicMember]
    public static EcmaValue Substr([This] EcmaValue value, EcmaValue start, EcmaValue length) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      int pos = start.ToInteger(-str.Length, str.Length);
      if (pos < 0) {
        pos = pos + str.Length;
      }
      int len = length.ToInteger(0, str.Length - pos);
      if (len <= 0) {
        return String.Empty;
      }
      return str.Substring(pos, len);
    }

    [IntrinsicMember]
    public static EcmaValue Substring([This] EcmaValue value, EcmaValue start, EcmaValue end) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      int spos = start.ToInteger(0, str.Length);
      int epos = end == default ? str.Length : end.ToInteger(0, str.Length);
      if (epos < spos) {
        return str.Substring(epos, spos - epos);
      }
      return str.Substring(spos, epos - spos);
    }

    [IntrinsicMember]
    public static EcmaValue Trim([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToStringOrThrow().Trim(trimChars);
    }

    [IntrinsicMember]
    [IntrinsicMember("TrimLeft")]
    public static EcmaValue TrimStart([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToStringOrThrow().TrimStart(trimChars);
    }

    [IntrinsicMember]
    [IntrinsicMember("TrimRight")]
    public static EcmaValue TrimEnd([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return value.ToStringOrThrow().TrimEnd(trimChars);
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue value, EcmaValue start, EcmaValue end) {
      Guard.RequireObjectCoercible(value);
      string str = value.ToStringOrThrow();
      int spos = start.ToInteger(-str.Length, str.Length);
      int epos = end == default ? str.Length : end.ToInteger(-str.Length, str.Length);
      spos = spos < 0 ? spos + str.Length : spos;
      epos = epos < 0 ? epos + str.Length : epos;
      return str.Substring(spos, Math.Max(epos - spos, 0));
    }

    [IntrinsicMember]
    public static EcmaValue Match([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (!searcher.IsNullOrUndefined) {
        RuntimeObject handler = searcher.ToObject().GetMethod(WellKnownSymbol.Match);
        if (handler != null) {
          return handler.Call(searcher, value);
        }
      }
      string str = value.ToStringOrThrow();
      EcmaValue regex = EcmaRegExp.Parse(searcher == default ? "" : searcher.ToStringOrThrow(), "");
      return regex.Invoke(WellKnownSymbol.Match, str);
    }

    [IntrinsicMember]
    public static EcmaValue MatchAll([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (!searcher.IsNullOrUndefined) {
        RuntimeObject handler = searcher.ToObject().GetMethod(WellKnownSymbol.MatchAll);
        if (handler != null) {
          return handler.Call(searcher, value);
        }
      }
      string str = value.ToStringOrThrow();
      EcmaValue regex = EcmaRegExp.Parse(searcher == default ? "" : searcher.ToStringOrThrow(), "g");
      return regex.Invoke(WellKnownSymbol.MatchAll, str);
    }

    [IntrinsicMember]
    public static EcmaValue Replace([This] EcmaValue value, EcmaValue searcher, EcmaValue replacement) {
      Guard.RequireObjectCoercible(value);
      if (!searcher.IsNullOrUndefined) {
        RuntimeObject handler = searcher.ToObject().GetMethod(WellKnownSymbol.Replace);
        if (handler != null) {
          return handler.Call(searcher, value, replacement);
        }
      }
      string str = value.ToStringOrThrow();
      string needle = searcher.ToStringOrThrow();
      int index = str.IndexOf(needle);
      if (index < 0) {
        return str;
      }
      string newStr = (replacement.IsCallable ? replacement.Call(EcmaValue.Undefined, needle, index, str) : replacement).ToStringOrThrow();
      return str.Substring(0, index) + newStr + str.Substring(index + needle.Length);
    }

    [IntrinsicMember]
    public static EcmaValue Search([This] EcmaValue value, EcmaValue searcher) {
      Guard.RequireObjectCoercible(value);
      if (!searcher.IsNullOrUndefined) {
        RuntimeObject handler = searcher.ToObject().GetMethod(WellKnownSymbol.Search);
        if (handler != null) {
          return handler.Call(searcher, value);
        }
      }
      string str = value.ToStringOrThrow();
      EcmaValue regex = EcmaRegExp.Parse(searcher == default ? "" : searcher.ToStringOrThrow(), "");
      return regex.Invoke(WellKnownSymbol.Search, str);
    }

    [IntrinsicMember]
    public static EcmaValue Split([This] EcmaValue value, EcmaValue searcher, EcmaValue limit) {
      Guard.RequireObjectCoercible(value);
      if (!searcher.IsNullOrUndefined) {
        RuntimeObject handler = searcher.ToObject().GetMethod(WellKnownSymbol.Split);
        if (handler != null) {
          return handler.Call(searcher, value, limit);
        }
      }
      string str = value.ToStringOrThrow();
      string separator = searcher.ToStringOrThrow();
      int count = unchecked((int)limit.ToUInt32());
      if (separator.Length == 0) {
        EcmaValue[] arr = new EcmaValue[str.Length];
        for (int i = 0, len = str.Length; i < len; i++) {
          arr[i] = str.Substring(i, 1);
        }
        return new EcmaArray(arr);
      }
      string[] result = str.Split(new[] { separator }, StringSplitOptions.None);
      if (limit == default) {
        return new EcmaArray(result.Select(v => new EcmaValue(v)).ToList());
      }
      return new EcmaArray(result.Take(count).Select(v => new EcmaValue(v)).ToList());
    }

    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Iterator([This] EcmaValue value) {
      Guard.RequireObjectCoercible(value);
      return new EcmaIterator(new EcmaStringEnumerator(value.ToStringOrThrow()), EcmaIteratorResultKind.Value, WellKnownObject.StringIteratorPrototype);
    }

    private static string MapLowerSpecialCase(char ch) {
      switch (ch) {
        case '\u0130': return "\u0069\u0307";
      }
      return null;
    }

    private static string MapUpperSpecialCase(char ch) {
      switch (ch) {
        case '\u00DF': return "\u0053\u0053";
        case '\uFB00': return "\u0046\u0046";
        case '\uFB01': return "\u0046\u0049";
        case '\uFB02': return "\u0046\u004C";
        case '\uFB03': return "\u0046\u0046\u0049";
        case '\uFB04': return "\u0046\u0046\u004C";
        case '\uFB05': return "\u0053\u0054";
        case '\uFB06': return "\u0053\u0054";
        case '\u0587': return "\u0535\u0552";
        case '\uFB13': return "\u0544\u0546";
        case '\uFB14': return "\u0544\u0535";
        case '\uFB15': return "\u0544\u053B";
        case '\uFB16': return "\u054E\u0546";
        case '\uFB17': return "\u0544\u053D";
        case '\u0149': return "\u02BC\u004E";
        case '\u0390': return "\u0399\u0308\u0301";
        case '\u03B0': return "\u03A5\u0308\u0301";
        case '\u01F0': return "\u004A\u030C";
        case '\u1E96': return "\u0048\u0331";
        case '\u1E97': return "\u0054\u0308";
        case '\u1E98': return "\u0057\u030A";
        case '\u1E99': return "\u0059\u030A";
        case '\u1E9A': return "\u0041\u02BE";
        case '\u1F50': return "\u03A5\u0313";
        case '\u1F52': return "\u03A5\u0313\u0300";
        case '\u1F54': return "\u03A5\u0313\u0301";
        case '\u1F56': return "\u03A5\u0313\u0342";
        case '\u1FB6': return "\u0391\u0342";
        case '\u1FC6': return "\u0397\u0342";
        case '\u1FD2': return "\u0399\u0308\u0300";
        case '\u1FD3': return "\u0399\u0308\u0301";
        case '\u1FD6': return "\u0399\u0342";
        case '\u1FD7': return "\u0399\u0308\u0342";
        case '\u1FE2': return "\u03A5\u0308\u0300";
        case '\u1FE3': return "\u03A5\u0308\u0301";
        case '\u1FE4': return "\u03A1\u0313";
        case '\u1FE6': return "\u03A5\u0342";
        case '\u1FE7': return "\u03A5\u0308\u0342";
        case '\u1FF6': return "\u03A9\u0342";
        case '\u1F80': return "\u1F08\u0399";
        case '\u1F81': return "\u1F09\u0399";
        case '\u1F82': return "\u1F0A\u0399";
        case '\u1F83': return "\u1F0B\u0399";
        case '\u1F84': return "\u1F0C\u0399";
        case '\u1F85': return "\u1F0D\u0399";
        case '\u1F86': return "\u1F0E\u0399";
        case '\u1F87': return "\u1F0F\u0399";
        case '\u1F88': return "\u1F08\u0399";
        case '\u1F89': return "\u1F09\u0399";
        case '\u1F8A': return "\u1F0A\u0399";
        case '\u1F8B': return "\u1F0B\u0399";
        case '\u1F8C': return "\u1F0C\u0399";
        case '\u1F8D': return "\u1F0D\u0399";
        case '\u1F8E': return "\u1F0E\u0399";
        case '\u1F8F': return "\u1F0F\u0399";
        case '\u1F90': return "\u1F28\u0399";
        case '\u1F91': return "\u1F29\u0399";
        case '\u1F92': return "\u1F2A\u0399";
        case '\u1F93': return "\u1F2B\u0399";
        case '\u1F94': return "\u1F2C\u0399";
        case '\u1F95': return "\u1F2D\u0399";
        case '\u1F96': return "\u1F2E\u0399";
        case '\u1F97': return "\u1F2F\u0399";
        case '\u1F98': return "\u1F28\u0399";
        case '\u1F99': return "\u1F29\u0399";
        case '\u1F9A': return "\u1F2A\u0399";
        case '\u1F9B': return "\u1F2B\u0399";
        case '\u1F9C': return "\u1F2C\u0399";
        case '\u1F9D': return "\u1F2D\u0399";
        case '\u1F9E': return "\u1F2E\u0399";
        case '\u1F9F': return "\u1F2F\u0399";
        case '\u1FA0': return "\u1F68\u0399";
        case '\u1FA1': return "\u1F69\u0399";
        case '\u1FA2': return "\u1F6A\u0399";
        case '\u1FA3': return "\u1F6B\u0399";
        case '\u1FA4': return "\u1F6C\u0399";
        case '\u1FA5': return "\u1F6D\u0399";
        case '\u1FA6': return "\u1F6E\u0399";
        case '\u1FA7': return "\u1F6F\u0399";
        case '\u1FA8': return "\u1F68\u0399";
        case '\u1FA9': return "\u1F69\u0399";
        case '\u1FAA': return "\u1F6A\u0399";
        case '\u1FAB': return "\u1F6B\u0399";
        case '\u1FAC': return "\u1F6C\u0399";
        case '\u1FAD': return "\u1F6D\u0399";
        case '\u1FAE': return "\u1F6E\u0399";
        case '\u1FAF': return "\u1F6F\u0399";
        case '\u1FB3': return "\u0391\u0399";
        case '\u1FBC': return "\u0391\u0399";
        case '\u1FC3': return "\u0397\u0399";
        case '\u1FCC': return "\u0397\u0399";
        case '\u1FF3': return "\u03A9\u0399";
        case '\u1FFC': return "\u03A9\u0399";
        case '\u1FB2': return "\u1FBA\u0399";
        case '\u1FB4': return "\u0386\u0399";
        case '\u1FC2': return "\u1FCA\u0399";
        case '\u1FC4': return "\u0389\u0399";
        case '\u1FF2': return "\u1FFA\u0399";
        case '\u1FF4': return "\u038F\u0399";
        case '\u1FB7': return "\u0391\u0342\u0399";
        case '\u1FC7': return "\u0397\u0342\u0399";
        case '\u1FF7': return "\u03A9\u0342\u0399";
      }
      return null;
    }

    private static bool IsCasedLetter(string str, int index) {
      switch (CharUnicodeInfo.GetUnicodeCategory(str, index)) {
        case UnicodeCategory.LowercaseLetter:
        case UnicodeCategory.UppercaseLetter:
        case UnicodeCategory.TitlecaseLetter:
          return true;
      }
      return false;
    }

    private static bool IsCaseIgnoreOrSurrogate(string str, int index) {
      switch (CharUnicodeInfo.GetUnicodeCategory(str, index)) {
        case UnicodeCategory.Surrogate:
        case UnicodeCategory.NonSpacingMark:
        case UnicodeCategory.EnclosingMark:
        case UnicodeCategory.Format:
        case UnicodeCategory.ModifierLetter:
        case UnicodeCategory.ModifierSymbol:
          return true;
      }
      switch ((int)str[index]) {
        // Word_Break=MidLetter
        case 58:
        case 183:
        case 727:
        case 903:
        case 1524:
        case 8231:
        case 65043:
        case 65109:
        case 65306:
        // Word_Break=MidNumLet
        case 46:
        case 8216:
        case 8217:
        case 8228:
        case 65106:
        case 65287:
        case 65294:
        // Word_Break=Single_Quote
        case 39:
          return true;
      }
      return false;
    }
  }
}
