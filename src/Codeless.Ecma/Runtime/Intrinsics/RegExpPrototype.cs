using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpPrototype)]
  internal static class RegExpPrototype {
    [IntrinsicMember]
    public static EcmaValue Compile([This] EcmaValue thisValue, EcmaValue pattern, EcmaValue flags) {
      return RegExpConstructor.RegExp(thisValue, pattern, flags);
    }

    [IntrinsicMember]
    public static EcmaValue Exec([This] EcmaValue thisValue, EcmaValue input) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      Match m = re.Execute(input.ToString());
      return MatchToResult(m);
    }

    [IntrinsicMember]
    public static EcmaValue Test([This] EcmaValue thisValue, EcmaValue str) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Test(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      return String.Concat("/", Source(thisValue), "/", Flags(thisValue));
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Flags([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      StringBuilder sb = new StringBuilder(5);
      if (re.Global) {
        sb.Append('g');
      }
      if (re.IgnoreCase) {
        sb.Append('i');
      }
      if (re.Multiline) {
        sb.Append('m');
      }
      if (re.IsUnicode) {
        sb.Append('u');
      }
      if (re.Sticky) {
        sb.Append('y');
      }
      return sb.ToString();
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Global([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Global;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue IgnoreCase([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.IgnoreCase;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Multiline([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Multiline;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Source([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Pattern;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Sticky([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Sticky;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Unicode([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.IsUnicode;
    }

    [IntrinsicMember(WellKnownSymbol.Match)]
    public static EcmaValue Match([This] EcmaValue thisValue, EcmaValue str) {
      return Exec(thisValue, str);
    }

    [IntrinsicMember(WellKnownSymbol.MatchAll)]
    public static EcmaValue MatchAll([This] EcmaValue thisValue, EcmaValue str) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      string inputString = str.ToString();
      List<EcmaValue> arr = new List<EcmaValue>();
      while (re.Execute(inputString).Success) {
        arr.Add(MatchToResult(re.LastMatch));
      }
      return new EcmaArray(arr);
    }

    [IntrinsicMember(WellKnownSymbol.Replace)]
    public static EcmaValue Replace([This] EcmaValue thisValue, EcmaValue str, EcmaValue replacement) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      if (replacement.IsCallable) {
        return re.Replace(str.ToString(), v => replacement.Call(EcmaValue.Undefined, v));
      }
      return re.Replace(str.ToString(), replacement.ToString());
    }

    [IntrinsicMember(WellKnownSymbol.Search)]
    public static EcmaValue Search([This] EcmaValue thisValue, EcmaValue str) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Execute(str.ToString()).Success ? re.LastIndex - re.LastMatch.Length : -1;
    }

    [IntrinsicMember(WellKnownSymbol.Split)]
    public static EcmaValue Split([This] EcmaValue thisValue, EcmaValue str) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      string inputString = str.ToString();
      int lastIndex = 0;
      List<EcmaValue> arr = new List<EcmaValue>();
      while (re.Execute(inputString).Success) {
        int startIndex = re.LastIndex - re.LastMatch.Length;
        arr.Add(inputString.Substring(startIndex, lastIndex - startIndex));
        lastIndex = re.LastIndex;
      }
      arr.Add(inputString.Substring(lastIndex));
      return new EcmaArray(arr);
    }

    private static EcmaValue MatchToResult(Match m) {
      if (!m.Success) {
        return EcmaValue.Null;
      }
      List<EcmaValue> values = new List<EcmaValue>();
      values.Add(m.Value);
      foreach (Group group in m.Groups) {
        values.Add(group.Value);
      }
      return new EcmaArray(values);
    }
  }
}
