using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExp)]
  internal static class RegExpConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue RegExp(EcmaValue pattern, EcmaValue flags) {
      EcmaRegExp re;
      if (EcmaRegExp.TryParse(String.Concat("/", pattern, "/", flags), out re)) {
        return new EcmaValue(re);
      }
      throw new EcmaSyntaxErrorException("Invalid regular expression");
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisArg) {
      return thisArg;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LastMatch([This] EcmaValue thisArg) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.LastValue;
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Input([This] EcmaValue thisArg) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.Input;
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LastParen([This] EcmaValue thisArg) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.LastParen;
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue LeftContext([This] EcmaValue thisArg) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.LeftContext;
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue RightContext([This] EcmaValue thisArg) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.RightContext;
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember("$_", Getter = true)]
    public static EcmaValue MatchInput([This] EcmaValue thisArg) {
      return Input(thisArg);
    }

    [IntrinsicMember("$1", Getter = true)]
    public static EcmaValue MatchParen1([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 1);
    }

    [IntrinsicMember("$2", Getter = true)]
    public static EcmaValue MatchParen2([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 2);
    }

    [IntrinsicMember("$3", Getter = true)]
    public static EcmaValue MatchParen3([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 3);
    }

    [IntrinsicMember("$4", Getter = true)]
    public static EcmaValue MatchParen4([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 4);
    }

    [IntrinsicMember("$5", Getter = true)]
    public static EcmaValue MatchParen5([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 5);
    }

    [IntrinsicMember("$6", Getter = true)]
    public static EcmaValue MatchParen6([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 6);
    }

    [IntrinsicMember("$7", Getter = true)]
    public static EcmaValue MatchParen7([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 7);
    }

    [IntrinsicMember("$8", Getter = true)]
    public static EcmaValue MatchParen8([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 8);
    }

    [IntrinsicMember("$9", Getter = true)]
    public static EcmaValue MatchParen9([This] EcmaValue thisArg) {
      return MatchParen(thisArg, 9);
    }

    private static EcmaValue MatchParen(EcmaValue thisArg, int index) {
      MatchInfo cur = CheckAndPopulate(RuntimeRealm.GetRealm(thisArg.ToRuntimeObject()));
      if (cur != null) {
        return cur.MatchedValues.ElementAtOrDefault(index);
      }
      return EcmaValue.Undefined;
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
