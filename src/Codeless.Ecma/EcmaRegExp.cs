using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Codeless.Ecma {
  public delegate EcmaValue EcmaRegexMatchEvaluator(EcmaValue value);

  /// <summary>
  /// Represents a ECMAScript-like regular expression object.
  /// </summary>
  public class EcmaRegExp : RuntimeObject {
    private static readonly ConcurrentDictionary<string, EcmaRegExp> cache = new ConcurrentDictionary<string, EcmaRegExp>();
    private Regex nativeRegexp;

    private EcmaRegExp(Regex nativeRegexp, bool global)
      : base(WellKnownObject.RegExpPrototype) {
      this.nativeRegexp = nativeRegexp;
      this.Global = global;
      DefineOwnPropertyNoChecked(WellKnownPropertyName.LastIndex, new EcmaPropertyDescriptor(0, EcmaPropertyAttributes.Writable));
    }

    private EcmaRegExp(EcmaRegExp source)
      : base(WellKnownObject.RegExpPrototype) {
      Init(source);
      DefineOwnPropertyNoChecked(WellKnownPropertyName.LastIndex, new EcmaPropertyDescriptor(0, EcmaPropertyAttributes.Writable));
    }

    public string Pattern {
      get { return nativeRegexp.ToString(); }
    }

    /// <summary>
    /// Indicates that the regular expression should be tested against all possible matches in a string.
    /// </summary>
    public bool Global { get; private set; }

    /// <summary>
    /// Indicates that a multiline input string should be treated as multiple lines. 
    /// In such case "^" and "$" change from matching at only the start or end of the entire string to the start or end of any line within the string.
    /// </summary>
    public bool Multiline {
      get { return (nativeRegexp.Options & RegexOptions.Multiline) > 0; }
    }

    /// <summary>
    /// Indicates that case should be ignored while attempting a match in a string.
    /// </summary>
    public bool IgnoreCase {
      get { return (nativeRegexp.Options & RegexOptions.IgnoreCase) > 0; }
    }

    public bool Sticky {
      get { return false; }
    }

    public bool IsUnicode {
      get { return false; }
    }

    public int LastIndex { get; private set; }

    public string LastInput { get; private set; }

    public Match LastMatch { get; private set; }

    internal void Init(EcmaRegExp re) {
      nativeRegexp = re.nativeRegexp;
      this.Global = re.Global;
    }

    /// <summary>
    /// Tests whether there is any occurences in the specified string that matches the pattern.
    /// </summary>
    /// <param name="input">A string to test against.</param>
    /// <returns></returns>
    public bool Test(string input) {
      Match m = nativeRegexp.Match(input, this.LastIndex);
      SetLastResult(input, m);
      return m.Success;
    }

    public Match Execute(string input) {
      Match m = nativeRegexp.Match(input, this.LastIndex);
      SetLastResult(input, m);
      return m;
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of pipe function argument.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A pipe function argument.</param>
    /// <returns></returns>
    public string Replace(string input, EcmaRegexMatchEvaluator replacement) {
      return Replace(input, new MatchEvaluatorClass(input, replacement).MatchEvaluator);
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of native method.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A delegate escapulating a method that returns replacement string for the specifc occurence.</param>
    /// <returns></returns>
    public string Replace(string input, MatchEvaluator replacement) {
      if (Global) {
        return nativeRegexp.Replace(input, replacement);
      } else {
        return nativeRegexp.Replace(input, replacement, 1);
      }
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the specified replacement.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">Replacement string.</param>
    /// <returns></returns>
    public string Replace(string input, string replacement) {
      if (Global) {
        return nativeRegexp.Replace(input, replacement);
      } else {
        return nativeRegexp.Replace(input, replacement, 1);
      }
    }

    /// <summary>
    /// Parses the given string into an instance of the <see cref="EcmaRegExp"/> class if the string represents a valid ECMAScript-compatible regular expression.
    /// </summary>
    /// <param name="str">A string representing a valid ECMAScript-compatible regular expression.</param>
    /// <param name="re">A reference to a variable that the parsed regular expression object is set to.</param>
    /// <returns>Returns *true* if the given string represents a valid ECMAScript-compatible regular expression; or *false* otherwise.</returns>
    public static bool TryParse(string str, out EcmaRegExp re) {
      Guard.ArgumentNotNull(str, "str");
      if (str.Length > 0 && str[0] == '/') {
        if (!cache.TryGetValue(str, out re)) {
          Match m = Regex.Match(str, @"\/((?![*+?])(?:[^\r\n\[/\\]|\\.|\[(?:[^\r\n\]\\]|\\.)*\])+)\/((?:g(?:im?|mi?)?|i(?:gm?|mg?)?|m(?:gi?|ig?)?)?)");
          if (m.Success) {
            RegexOptions options = 0;
            if (m.Groups[2].Value.Contains('i')) {
              options |= RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
            }
            if (m.Groups[2].Value.Contains('m')) {
              options |= RegexOptions.Multiline | RegexOptions.ECMAScript;
            }
            re = new EcmaRegExp(new Regex(m.Groups[1].Value, options), m.Groups[2].Value.Contains('g'));
            cache.TryAdd(str, re);
          }
          cache.TryAdd(str, null);
        }
        if (re != null) {
          re = new EcmaRegExp(re);
          return true;
        }
      }
      re = null;
      return false;
    }

    private void SetLastResult(string input, Match result) {
      this.Realm.Properties[typeof(EcmaRegExp)] = this;
      this.LastInput = input;
      this.LastMatch = result;
      this.LastIndex = this.Global ? result.Index + result.Length : 0;
      this.Set("lastIndex", this.LastIndex);
    }

    private class MatchEvaluatorClass {
      private readonly string input;
      private readonly EcmaRegexMatchEvaluator replacement;

      public MatchEvaluatorClass(string input, EcmaRegexMatchEvaluator replacement) {
        this.input = input;
        this.replacement = replacement;
      }

      public string MatchEvaluator(Match m) {
        Hashtable collection = new Hashtable();
        foreach (Group group in m.Groups) {
          collection.Add(collection.Count.ToString(), group.Value);
        }
        collection.Add("index", m.Index);
        collection.Add("input", input);
        return (string)replacement(new EcmaObject(collection));
      }
    }
  }
}
