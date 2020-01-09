using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpPrototype)]
  internal static class RegExpPrototype {
    public delegate IEcmaRegExpResult ExecCallback(string input);

    [IntrinsicMember]
    public static EcmaValue Compile([This] EcmaValue thisValue, EcmaValue pattern, EcmaValue flags) {
      Guard.ArgumentIsObject(thisValue);
      return RegExpConstructor.RegExp(RuntimeObject.GetSpeciesConstructor(thisValue.ToObject(), WellKnownObject.RegExpConstructor), thisValue, pattern, flags);
    }

    [IntrinsicMember]
    public static EcmaValue Exec([This] EcmaValue thisValue, EcmaValue input) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Test(input.ToStringOrThrow()) ? re.LastResult.ToValue() : EcmaValue.Null;
    }

    [IntrinsicMember]
    public static EcmaValue Test([This] EcmaValue thisValue, EcmaValue str) {
      Guard.ArgumentIsObject(thisValue);
      return CreateExecCallback(thisValue)(str.ToStringOrThrow()) != null;
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return String.Concat("/", re.Source, "/", re.Flags);
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Flags([This] EcmaValue thisValue) {
      if (thisValue.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
      }
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return "";
      }
      if (thisValue.GetUnderlyingObject() is EcmaRegExp re) {
        return re.Flags;
      }
      StringBuilder sb = new StringBuilder(6);
      if (thisValue[WellKnownProperty.Global].ToBoolean()) {
        sb.Append('g');
      }
      if (thisValue[WellKnownProperty.IgnoreCase].ToBoolean()) {
        sb.Append('i');
      }
      if (thisValue[WellKnownProperty.Multiline].ToBoolean()) {
        sb.Append('m');
      }
      if (thisValue[WellKnownProperty.DotAll].ToBoolean()) {
        sb.Append('s');
      }
      if (thisValue[WellKnownProperty.Unicode].ToBoolean()) {
        sb.Append('u');
      }
      if (thisValue[WellKnownProperty.Sticky].ToBoolean()) {
        sb.Append('y');
      }
      return sb.ToString();
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue DotAll([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.DotAll) != 0;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Global([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.Global) != 0;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue IgnoreCase([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.IgnoreCase) != 0;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Multiline([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.Multiline) != 0;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Source([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return "(?:)";
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return re.Source;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Sticky([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.Sticky) != 0;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Unicode([This] EcmaValue thisValue) {
      if (thisValue.ToObject().IsWellknownObject(WellKnownObject.RegExpPrototype)) {
        return default;
      }
      EcmaRegExp re = thisValue.GetUnderlyingObject<EcmaRegExp>();
      return (re.OriginalFlags & EcmaRegExpFlags.Unicode) != 0;
    }

    [IntrinsicMember(WellKnownSymbol.Match)]
    public static EcmaValue Match([This] EcmaValue thisValue, EcmaValue str) {
      Guard.ArgumentIsObject(thisValue);
      string inputString = str.ToStringOrThrow();
      ExecCallback exec = CreateExecCallback(thisValue);
      IEcmaRegExpResult result;
      if (thisValue[WellKnownProperty.Global]) {
        List<EcmaValue> arr = new List<EcmaValue>();
        bool unicode = thisValue[WellKnownProperty.Unicode].ToBoolean();
        thisValue[WellKnownProperty.LastIndex] = 0;
        while ((result = exec(inputString)) != null) {
          arr.Add(result.Value);
          if (result.Value.Length == 0) {
            AdvanceStringIndex(thisValue, inputString, unicode);
          }
        }
        return arr.Count == 0 ? EcmaValue.Null : new EcmaArray(arr);
      }
      result = exec(inputString);
      return result != null ? result.ToValue() : EcmaValue.Null;
    }

    [IntrinsicMember(WellKnownSymbol.MatchAll)]
    public static EcmaValue MatchAll([This] EcmaValue thisValue, EcmaValue str) {
      Guard.ArgumentIsObject(thisValue);
      EcmaValue re = SpeciesConstruct(thisValue, false);
      string inputString = str.ToStringOrThrow();
      return new EcmaIterator(new EcmaRegExpStringEnumerator(re, inputString), EcmaIteratorResultKind.Value, WellKnownObject.RegExpStringIteratorPrototype);
    }

    [IntrinsicMember(WellKnownSymbol.Replace)]
    public static EcmaValue Replace([This] EcmaValue thisValue, EcmaValue str, EcmaValue replacement) {
      Guard.ArgumentIsObject(thisValue);
      string input = str.ToStringOrThrow();
      EcmaRegExp re;
      if (IsCustomRegExpObject(thisValue, out re, out _) || thisValue[WellKnownProperty.Sticky]) {
        return ReplaceGeneric(thisValue, input, replacement);
      }
      if (replacement.IsCallable) {
        return re.Replace(input, replacement.ToObject());
      }
      return re.Replace(input, replacement.ToStringOrThrow());
    }

    [IntrinsicMember(WellKnownSymbol.Search)]
    public static EcmaValue Search([This] EcmaValue thisValue, EcmaValue str) {
      EcmaValue re = SpeciesConstruct(thisValue, false);
      IEcmaRegExpResult result = CreateExecCallback(re)(str.ToStringOrThrow());
      return result != null ? result.Index : -1;
    }

    [IntrinsicMember(WellKnownSymbol.Split)]
    public static EcmaValue Split([This] EcmaValue thisValue, EcmaValue str, EcmaValue limit) {
      EcmaValue re = SpeciesConstruct(thisValue, true);
      string inputString = str.ToStringOrThrow();
      int count = limit == default ? Int32.MaxValue : unchecked((int)limit.ToUInt32());

      ExecCallback exec = CreateExecCallback(re);
      List<EcmaValue> arr = new List<EcmaValue>();
      IEcmaRegExpResult result;
      int lastIndex = 0;
      int lastResultLength = -1;
      bool unicode = re[WellKnownProperty.Unicode].ToBoolean();
      while (arr.Count < count && (result = exec(inputString)) != null) {
        lastResultLength = result.Value.Length;
        if (lastResultLength > 0 || lastIndex != result.Index) {
          arr.Add(inputString.Substring(lastIndex, result.Index - lastIndex));
          arr.AddRange(result.Captures.Skip(1).Take(count - arr.Count));
        }
        lastIndex = (int)re[WellKnownProperty.LastIndex].ToUInt32();
        if (lastResultLength == 0) {
          AdvanceStringIndex(re, inputString, unicode, lastIndex);
        }
      }
      if (arr.Count < count && (lastResultLength != 0 || lastIndex != inputString.Length)) {
        arr.Add(inputString.Substring(lastIndex));
      }
      return new EcmaArray(arr);
    }

    [EcmaSpecification("RegExpExec", EcmaSpecificationKind.RuntimeSemantics)]
    public static ExecCallback CreateExecCallback(EcmaValue thisValue) {
      EcmaRegExp re;
      EcmaValue exec;
      if (IsCustomRegExpObject(thisValue, out re, out exec)) {
        return input => {
          EcmaValue returnValue = exec.Call(thisValue, input);
          bool success = returnValue != EcmaValue.Null;
          if (success && returnValue.Type != EcmaValueType.Object) {
            throw new EcmaTypeErrorException(InternalString.Error.RegExpExecReturnWrongType);
          }
          return success ? new ExecGenericResult(input, returnValue) : null;
        };
      }
      return re.Execute;
    }

    public static string ReplaceGeneric(EcmaValue re, string input, EcmaValue replacement) {
      List<Replacement> list = null;
      RuntimeObject callback = null;
      if (replacement.IsCallable) {
        callback = replacement.ToObject();
      } else {
        list = ParseReplacement(replacement.ToStringOrThrow());
      }

      ExecCallback exec = CreateExecCallback(re);
      bool global = re[WellKnownProperty.Global].ToBoolean();
      bool unicode = false;
      if (global) {
        re[WellKnownProperty.LastIndex] = 0;
        unicode = re[WellKnownProperty.Unicode].ToBoolean();
      }

      StringBuilder sb = new StringBuilder();
      IEcmaRegExpResult result;
      int lastIndex = 0;
      while ((result = exec(input)) != null) {
        if (result.Index >= lastIndex) {
          sb.Append(input, lastIndex, result.Index - lastIndex);
          if (callback != null) {
            sb.Append(InvokeReplacementCallback(callback, result));
          } else {
            EcmaValue[] captures = result.Captures.ToArray();
            EcmaValue groups = result.HasNamedGroups ? result.CreateNamedGroupObject() : default;
            foreach (Replacement repl in list) {
              switch (repl.Mode) {
                case 0:
                case -1:
                  sb.Append(repl.StringValue);
                  continue;
                case -2:
                  if (result.HasNamedGroups) {
                    EcmaValue value = groups[repl.StringValue];
                    sb.Append(value != default ? value.ToStringOrThrow() : "");
                  } else {
                    sb.Append("$<" + repl.StringValue + ">");
                  }
                  continue;
                case -3:
                  sb.Append(result.Value);
                  continue;
                case -4:
                  sb.Append(input.Substring(0, Math.Min(input.Length, result.Index)));
                  continue;
                case -5:
                  sb.Append(input.Substring(Math.Min(input.Length, result.Index + result.Value.Length)));
                  continue;
              }
              if (repl.Mode >= captures.Length) {
                sb.Append(repl.StringValue);
              } else {
                sb.Append(captures[repl.Mode].ToStringOrThrow());
              }
            }
          }
        }
        int newIndex = result.Index + result.Value.Length;
        if (!global) {
          lastIndex = newIndex;
          break;
        }
        if (newIndex <= lastIndex) {
          AdvanceStringIndex(re, input, unicode, lastIndex);
        } else {
          lastIndex = newIndex;
        }
      }
      if (lastIndex < input.Length) {
        sb.Append(input, lastIndex, input.Length - lastIndex);
      }
      return sb.ToString();
    }

    public static void AdvanceStringIndex(EcmaValue re, string inputString, bool unicode) {
      AdvanceStringIndex(re, inputString, unicode, (int)re[WellKnownProperty.LastIndex].ToUInt32());
    }

    [EcmaSpecification("AdvanceStringIndex", EcmaSpecificationKind.AbstractOperations)]
    public static void AdvanceStringIndex(EcmaValue re, string inputString, bool unicode, int lastIndex) {
      re[WellKnownProperty.LastIndex] = lastIndex + (unicode && lastIndex < inputString.Length && Char.IsSurrogatePair(inputString, lastIndex) ? 2 : 1);
    }

    public static string InvokeReplacementCallback(RuntimeObject callback, IEcmaRegExpResult result) {
      EcmaValue[] args = result.Captures.ToArray();
      bool hasNamedGroup = result.HasNamedGroups;
      int len = args.Length;
      Array.Resize(ref args, len + (hasNamedGroup ? 3 : 2));
      args[len] = result.Index;
      args[len + 1] = result.Input;
      if (hasNamedGroup) {
        args[len + 2] = result.CreateNamedGroupObject();
      }
      return callback.Call(EcmaValue.Undefined, args).ToStringOrThrow();
    }

    private static EcmaValue SpeciesConstruct(EcmaValue thisValue, bool forceGlobal) {
      Guard.ArgumentIsObject(thisValue);
      if (thisValue.GetUnderlyingObject() is EcmaRegExp re) {
        return forceGlobal ? re.Clone(true) : re.Clone();
      }
      return RuntimeObject.GetSpeciesConstructor(thisValue.ToObject(), WellKnownObject.RegExpConstructor).Construct(thisValue);
    }

    private static bool IsCustomRegExpObject(EcmaValue thisValue, out EcmaRegExp re, out EcmaValue exec) {
      re = thisValue.GetUnderlyingObject() as EcmaRegExp;
      exec = thisValue[WellKnownProperty.Exec];
      return re == null || (exec.IsCallable && !exec.ToObject().IsIntrinsicFunction(WellKnownObject.RegExpPrototype, "exec"));
    }

    private static List<Replacement> ParseReplacement(string replacement) {
      List<Replacement> list = new List<Replacement>();
      int index = 0;
      foreach (Match m in Regex.Matches(replacement, "\\$(?:([$&'`])|(\\d+)|<([^>]+)>)")) {
        if (index != m.Index) {
          list.Add(new Replacement { Mode = -1, StringValue = replacement.Substring(index, m.Index - index) });
        }
        if (m.Groups[1].Success) {
          switch (m.Groups[1].Value[0]) {
            case '$':
              list.Add(new Replacement { Mode = -1, StringValue = "$" });
              break;
            case '&':
              list.Add(new Replacement { Mode = -3 });
              break;
            case '`':
              list.Add(new Replacement { Mode = -4 });
              break;
            case '\'':
              list.Add(new Replacement { Mode = -5 });
              break;
          }
        } else if (m.Groups[2].Success) {
          list.Add(new Replacement { Mode = Int32.Parse(m.Groups[2].Value), StringValue = m.Value });
        } else {
          list.Add(new Replacement { Mode = -2, StringValue = m.Groups[3].Value });
        }
        index = m.Index + m.Value.Length;
      }
      if (index < replacement.Length) {
        list.Add(new Replacement { Mode = -1, StringValue = replacement.Substring(index) });
      }
      return list;
    }

    private struct Replacement {
      public int Mode;
      public string StringValue;
    }

    private class ExecGenericResult : IEcmaRegExpResult {
      private readonly EcmaValue result;

      public ExecGenericResult(string input, EcmaValue result) {
        this.result = result;
        this.Input = input;
        this.Index = unchecked((int)result[WellKnownProperty.Index].ToUInt32());
        this.Value = result[0].ToStringOrThrow();
      }

      public bool HasNamedGroups => result[WellKnownProperty.Groups] != default;
      public int Index { get; }
      public string Input { get; }
      public string Value { get; }

      public IEnumerable<EcmaValue> Captures {
        get {
          for (long i = 0, len = result[WellKnownProperty.Length].ToLength(); i < len; i++) {
            yield return result[i];
          }
        }
      }

      public EcmaValue CreateNamedGroupObject() {
        return result[WellKnownProperty.Groups];
      }

      public EcmaValue ToValue() {
        return result;
      }
    }
  }
}
