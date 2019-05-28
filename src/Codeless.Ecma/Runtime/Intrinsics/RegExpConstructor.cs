using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpConstructor)]
  internal static class RegExpConstructor {
    [IntrinsicConstructor(ObjectType = typeof(EcmaRegExp))]
    public static EcmaValue RegExp([NewTarget] RuntimeObject newTarget, [This] EcmaValue thisValue, EcmaValue pattern, EcmaValue flags) {
      bool patternIsRegExp = pattern.IsRegExp;
      if (newTarget == null && patternIsRegExp && flags == default) {
        EcmaValue constructor = pattern[WellKnownPropertyName.Constructor];
        if (constructor.Type == EcmaValueType.Object && constructor.ToObject().IsWellknownObject(WellKnownObject.RegExpConstructor)) {
          return pattern;
        }
      }
      string strPattern;
      string strFlags;
      if (pattern.Type == EcmaValueType.Object && pattern.ToObject() is EcmaRegExp other) {
        strPattern = other.Source;
        strFlags = flags == default ? (string)RegExpPrototype.Flags(other) : flags.ToString(true);
      } else if (patternIsRegExp) {
        strPattern = pattern["source"].ToString(true);
        strFlags = flags == default ? pattern["flags"].ToString(true) : flags.ToString(true);
      } else {
        strPattern = pattern == default ? "" : pattern.ToString(true);
        strFlags = flags == default ? "" : flags.ToString(true);
      }
      EcmaRegExp re = EcmaRegExp.Parse(strPattern, strFlags);
      if (newTarget == null) {
        return re.Clone();
      }
      thisValue.GetUnderlyingObject<EcmaRegExp>().Init(re);
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LastMatch([This] EcmaValue thisValue) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.LastValue;
      }
      return default;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Input([This] EcmaValue thisValue) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.LastResult.Input;
      }
      return default;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LastParen([This] EcmaValue thisValue) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.LastParen;
      }
      return default;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LeftContext([This] EcmaValue thisValue) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.LeftContext;
      }
      return default;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue RightContext([This] EcmaValue thisValue) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.RightContext;
      }
      return default;
    }

    [IntrinsicMember("$_", Getter = true)]
    public static EcmaValue MatchInput([This] EcmaValue thisValue) {
      return Input(thisValue);
    }

    [IntrinsicMember("$1", Getter = true)]
    public static EcmaValue MatchParen1([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 1);
    }

    [IntrinsicMember("$2", Getter = true)]
    public static EcmaValue MatchParen2([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 2);
    }

    [IntrinsicMember("$3", Getter = true)]
    public static EcmaValue MatchParen3([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 3);
    }

    [IntrinsicMember("$4", Getter = true)]
    public static EcmaValue MatchParen4([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 4);
    }

    [IntrinsicMember("$5", Getter = true)]
    public static EcmaValue MatchParen5([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 5);
    }

    [IntrinsicMember("$6", Getter = true)]
    public static EcmaValue MatchParen6([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 6);
    }

    [IntrinsicMember("$7", Getter = true)]
    public static EcmaValue MatchParen7([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 7);
    }

    [IntrinsicMember("$8", Getter = true)]
    public static EcmaValue MatchParen8([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 8);
    }

    [IntrinsicMember("$9", Getter = true)]
    public static EcmaValue MatchParen9([This] EcmaValue thisValue) {
      return MatchParen(thisValue, 9);
    }

    private static EcmaValue MatchParen(EcmaValue thisValue, int index) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisValue));
      if (cur != null) {
        return cur.MatchedValues.ElementAtOrDefault(index);
      }
      return default;
    }

    private static MatchInfo CheckAndPopulate(RuntimeRealm realm) {
      EcmaRegExp regexp = (EcmaRegExp)realm.Properties[typeof(EcmaRegExp)];
      if (regexp == null) {
        return null;
      }
      if (realm.Properties[typeof(MatchInfo)] == null) {
        realm.Properties[typeof(MatchInfo)] = new MatchInfo();
      }
      MatchInfo mi = (MatchInfo)realm.Properties[typeof(MatchInfo)];
      if (mi.LastResult != regexp.LastResult) {
        mi.LastResult = regexp.LastResult;
      }
      return mi;
    }

    private class MatchInfo {
      private IEcmaRegExpResult lastResult;

      public MatchInfo() {
        this.MatchedValues = new List<EcmaValue>();
      }

      public IEcmaRegExpResult LastResult {
        get {
          return lastResult;
        }
        set {
          if (lastResult != value && value != null) {
            this.LastValue = value.Value;
            this.LeftContext = value.Input.Substring(0, value.Index);
            this.RightContext = value.Input.Substring(value.Index + value.Value.Length);
            this.MatchedValues.Clear();
            this.MatchedValues.AddRange(value.Captures.Take(10));
          }
          lastResult = value;
        }
      }

      public List<EcmaValue> MatchedValues { get; private set; }
      public string LastValue { get; private set; }
      public string LastParen { get; private set; }
      public string LeftContext { get; private set; }
      public string RightContext { get; private set; }
    }
  }
}
