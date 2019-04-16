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
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaRegExp))]
    public static EcmaValue RegExp([This] EcmaValue thisValue, EcmaValue pattern, EcmaValue flags) {
      if (EcmaRegExp.TryParse(String.Concat("/", pattern, "/", flags), out EcmaRegExp re)) {
        thisValue.GetUnderlyingObject<EcmaRegExp>().Init(re);
        return thisValue;
      }
      throw new EcmaSyntaxErrorException("Invalid regular expression");
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
        return cur.Input;
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
      if (mi.LastMatch != regexp.LastMatch) {
        mi.Input = regexp.LastInput;
        mi.LastMatch = regexp.LastMatch;
      }
      return mi;
    }

    private class MatchInfo {
      private Match lastMatch;

      public MatchInfo() {
        this.MatchedValues = new List<string>();
      }

      public string Input { get; set; }

      public Match LastMatch {
        get {
          return lastMatch;
        }
        set {
          if (lastMatch != value) {
            if (value.Success) {
              this.LastValue = value.Value;
              this.LeftContext = this.Input.Substring(0, value.Index);
              this.RightContext = this.Input.Substring(value.Index + value.Length);
              this.MatchedValues.Clear();
              foreach (Group group in value.Groups) {
                this.MatchedValues.Add(group.Value);
              }
              if (value.Groups.Count > 0) {
                this.LastParen = value.Groups[value.Groups.Count - 1].Value;
              }
            }
          }
          lastMatch = value;
        }
      }

      public List<string> MatchedValues { get; private set; }
      public string LastValue { get; private set; }
      public string LastParen { get; private set; }
      public string LeftContext { get; private set; }
      public string RightContext { get; private set; }
    }
  }
}
