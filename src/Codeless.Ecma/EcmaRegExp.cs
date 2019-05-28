using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Codeless.Ecma {
  [Flags]
  internal enum EcmaRegExpFlags {
    Global = 1,
    IgnoreCase = 2,
    Multiline = 4,
    Unicode = 8,
    Sticky = 16,
    DotAll = 32
  }

  public interface IEcmaRegExpResult {
    bool HasNamedGroups { get; }
    int Index { get; }
    string Input { get; }
    string Value { get; }
    IEnumerable<EcmaValue> Captures { get; }
    EcmaValue ToValue();
    EcmaValue CreateNamedGroupObject();
  }

  /// <summary>
  /// Represents a ECMAScript-like regular expression object.
  /// </summary>
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class EcmaRegExp : RuntimeObject {
    private const int HighSurrogateStart = 0xD800;
    private const int LowSurrogateEnd = 0xDFFF;
    private const int LowSurrogateStart = 0xDC00;

    private static readonly ConcurrentDictionary<string, EcmaRegExp> cache = new ConcurrentDictionary<string, EcmaRegExp>();
    private static readonly ConcurrentDictionary<string, StickyRegex> stickyRegexs = new ConcurrentDictionary<string, StickyRegex>();

    private static readonly Regex reGroups = new Regex(@"\\(\d+)|\\.|\((?:(?=\()|[^?]|\?<([^>]+)>)", RegexOptions.Compiled);
    private static readonly Regex reUnsupportedEscape = new Regex(@"\\(?:(\\)|([^^$\\.*+?()\[\]{}|/bfnrtvxucwdWDsSB0-9pP]))", RegexOptions.Compiled);
    private static readonly Regex reUnsupportedEscapeNonUnicode = new Regex(@"\\(?:(\\)|(u\{|u(?=[0-9a-fA-F]{0,3}(?:$|[^0-9a-fA-F]))|x(?=[0-9a-fA-F]{0,1}(?:$|[^0-9a-fA-F]))|[^^$\\.*+?()\[\]{}|/bfnrtvxucwdWDsSB0-9]))", RegexOptions.Compiled);
    private static readonly Regex reCharClass = new Regex(@"\\(?:[wWsS]|u\{[0-9a-fA-F]+\}|u[0-9a-fA-F]{4}|.)|\.|\[(\^?)(\\(?:[wWsS]|u\{[0-9a-fA-F]+\}|u[0-9a-fA-F]{4}|.)|[^\\\]]+)*\]", RegexOptions.Compiled);
    private static readonly Regex reCodePoints = new Regex(@"\\.|\[(\^?)((?:[^\]]|\\\])+)\]|([\ud800-\udbff][\udc00-\udfff]|[\ud800-\udbff](?![\udc00-\udfff])|(?<![\ud800-\udbff])[\udc00-\udfff])([+*?]|\{\d+(,\d+)?\})?", RegexOptions.Compiled);

    private int numericGroupCount;
    private string[] captureGroups;
    private Regex nativeRegexp;

    public EcmaRegExp()
      : this(String.Empty, String.Empty) { }

    public EcmaRegExp(RuntimeObject constructor)
      : base(WellKnownObject.RegExpPrototype, constructor) {
      DefineOwnPropertyNoChecked(WellKnownPropertyName.LastIndex, new EcmaPropertyDescriptor(0, EcmaPropertyAttributes.Writable));
    }

    public EcmaRegExp(string pattern)
      : this(pattern, String.Empty) { }

    public EcmaRegExp(string pattern, string flags)
      : base(WellKnownObject.RegExpPrototype) {
      Init(Parse(pattern, flags));
      DefineOwnPropertyNoChecked(WellKnownPropertyName.LastIndex, new EcmaPropertyDescriptor(0, EcmaPropertyAttributes.Writable));
    }

    private EcmaRegExp(Regex nativeRegexp, string pattern, string canonFlags, EcmaRegExpFlags flags, int numericGroupCount, string[] captureGroups)
      : base(WellKnownObject.RegExpPrototype) {
      Guard.ArgumentNotNull(nativeRegexp, "nativeRegexp");
      Guard.ArgumentNotNull(pattern, "pattern");
      Guard.ArgumentNotNull(canonFlags, "canonFlags");
      Guard.ArgumentNotNull(captureGroups, "captureGroups");

      this.nativeRegexp = nativeRegexp;
      this.OriginalFlags = flags;
      this.Flags = canonFlags;
      this.numericGroupCount = numericGroupCount;
      this.captureGroups = captureGroups;
      this.Source = pattern.Length == 0 ? "(?:)" : Regex.Replace(pattern, "(?<!\\\\)/|[\n\r]", m => {
        switch (m.Value[0]) {
          case '/': return "\\/";
          case '\n': return "\\n";
          case '\r': return "\\r";
        }
        return m.Value;
      });
      DefineOwnPropertyNoChecked(WellKnownPropertyName.LastIndex, new EcmaPropertyDescriptor(0, EcmaPropertyAttributes.Writable));
    }

    /// <summary>
    /// Indicates that the regular expression should be tested against all possible matches in a string.
    /// </summary>
    public bool Global {
      get { return this.Get("global").ToBoolean(); }
    }

    /// <summary>
    /// Indicates that a multiline input string should be treated as multiple lines. 
    /// In such case "^" and "$" change from matching at only the start or end of the entire string to the start or end of any line within the string.
    /// </summary>
    public bool Multiline {
      get { return this.Get("multiline").ToBoolean(); }
    }

    /// <summary>
    /// Indicates that case should be ignored while attempting a match in a string.
    /// </summary>
    public bool IgnoreCase {
      get { return this.Get("ignoreCase").ToBoolean(); }
    }

    public bool Sticky {
      get { return this.Get("sticky").ToBoolean(); }
    }

    public bool Unicode {
      get { return this.Get("unicode").ToBoolean(); }
    }

    public bool DotAll {
      get { return this.Get("dotAll").ToBoolean(); }
    }

    public string Source { get; private set; }

    public string Flags { get; private set; }

    internal EcmaRegExpFlags OriginalFlags { get; private set; }

    public int LastIndex {
      get { return (int)this.Get(WellKnownPropertyName.LastIndex).ToLength(); }
      set { this.Set(WellKnownPropertyName.LastIndex, value); }
    }

    public IEcmaRegExpResult LastResult { get; set; }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.RegExp; }
    }

    internal void Init(EcmaRegExp other) {
      this.nativeRegexp = other.nativeRegexp;
      this.OriginalFlags = other.OriginalFlags;
      this.captureGroups = other.captureGroups;
      this.numericGroupCount = other.numericGroupCount;
      this.Source = other.Source;
      this.Flags = other.Flags;
    }

    internal EcmaRegExp Clone() {
      EcmaRegExp clone = new EcmaRegExp(GetSpeciesConstructor(this, WellKnownObject.RegExpConstructor));
      clone.Init(this);
      return clone;
    }

    internal EcmaRegExp Clone(bool global) {
      EcmaRegExp clone = Clone();
      if (clone.Global != global) {
        clone.OriginalFlags ^= EcmaRegExpFlags.Global;
      }
      return clone;
    }

    /// <summary>
    /// Tests whether there is any occurences in the specified string that matches the pattern.
    /// </summary>
    /// <param name="input">A string to test against.</param>
    /// <returns></returns>
    [EcmaSpecification("RegExpBuiltinExec", EcmaSpecificationKind.RuntimeSemantics)]
    public bool Test(string input) {
      int lastIndex = this.LastIndex;
      bool global = this.Global;
      bool sticky = this.Sticky;
      IEcmaRegExpResult result = null;

      if (!global && !sticky) {
        lastIndex = 0;
      }
      if (lastIndex <= input.Length) {
        if (sticky) {
          bool matchStartAnchor = lastIndex == 0 || (this.Multiline && input[lastIndex - 1] == '\n');
          StickyRegex stickyRegexp = stickyRegexs.GetOrAdd(this.DebuggerDisplay + "/" + matchStartAnchor, _ => CreateStickyRegex(nativeRegexp.ToString(), matchStartAnchor, nativeRegexp.Options));
          if (stickyRegexp != null) {
            Match m = stickyRegexp.Regexp.Match(input.Substring(lastIndex));
            result = m.Success ? new MatchResult(this, input, m, stickyRegexp.GroupNameMap, lastIndex) : null;
          }
        } else {
          Match m = nativeRegexp.Match(input, lastIndex);
          result = m.Success ? new MatchResult(this, input, m, null, 0) : null;
        }
      }
      if (global || sticky) {
        if (!this.Set(WellKnownPropertyName.LastIndex, result != null ? result.Index + result.Value.Length : 0) && sticky) {
          throw new EcmaTypeErrorException(InternalString.Error.LastIndexNotWritable);
        }
      }
      this.LastResult = result;
      return result != null;
    }

    public IEcmaRegExpResult Execute(string input) {
      Test(input);
      return this.LastResult;
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of pipe function argument.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A pipe function argument.</param>
    /// <returns></returns>
    public string Replace(string input, RuntimeFunction replacement) {
      return Replace(input, new MatchEvaluatorClass(this, input, replacement).MatchEvaluator);
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of native method.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A delegate escapulating a method that returns replacement string for the specifc occurence.</param>
    /// <returns></returns>
    public string Replace(string input, MatchEvaluator replacement) {
      if (Global) {
        if (!this.Set(WellKnownPropertyName.LastIndex, 0)) {
          throw new EcmaTypeErrorException(InternalString.Error.LastIndexNotWritable);
        }
        return GetInstanceForGlobalReplace().nativeRegexp.Replace(input, replacement);
      }
      this.LastIndex = 0;
      return nativeRegexp.Replace(input, replacement, 1);
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the specified replacement.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">Replacement string.</param>
    /// <returns></returns>
    public string Replace(string input, string replacement) {
      replacement = FixReplacementString(replacement);
      if (Global) {
        if (!this.Set(WellKnownPropertyName.LastIndex, 0)) {
          throw new EcmaTypeErrorException(InternalString.Error.LastIndexNotWritable);
        }
        return GetInstanceForGlobalReplace().nativeRegexp.Replace(input, replacement);
      }
      this.LastIndex = 0;
      return nativeRegexp.Replace(input, replacement, 1);
    }

    /// <summary>
    /// Parses the given string into an instance of the <see cref="EcmaRegExp"/> class if the string represents a valid ECMAScript-compatible regular expression.
    /// </summary>
    /// <param name="pattern">A string representing a valid ECMAScript-compatible regular expression.</param>
    /// <param name="flags">A string representing valid regular expression flags.</param>
    /// <returns>Returns *true* if the given string represents a valid ECMAScript-compatible regular expression; or *false* otherwise.</returns>
    [EcmaSpecification("RegExpInitialize", EcmaSpecificationKind.RuntimeSemantics)]
    public static EcmaRegExp Parse(string pattern, string flags) {
      Guard.ArgumentNotNull(pattern, "pattern");
      Guard.ArgumentNotNull(flags, "flags");
      string key = String.Concat("/", pattern, "/", flags);
      if (!cache.TryGetValue(key, out EcmaRegExp re)) {
        EcmaRegExpFlags options = 0;
        string canonFlags = "";
        canonFlags += AddFlag(flags, "g", EcmaRegExpFlags.Global, ref options);
        canonFlags += AddFlag(flags, "i", EcmaRegExpFlags.IgnoreCase, ref options);
        canonFlags += AddFlag(flags, "m", EcmaRegExpFlags.Multiline, ref options);
        canonFlags += AddFlag(flags, "s", EcmaRegExpFlags.DotAll, ref options);
        canonFlags += AddFlag(flags, "u", EcmaRegExpFlags.Unicode, ref options);
        canonFlags += AddFlag(flags, "y", EcmaRegExpFlags.Sticky, ref options);
        if (flags.Length != canonFlags.Length) {
          throw new EcmaSyntaxErrorException(InternalString.Error.InvalidRegexFlags);
        }

        string nPattern = pattern;
        int numericGroupCount = 1;
        List<string> captureGroups = new List<string> { "0" };
        nPattern = reGroups.Replace(nPattern, m => {
          switch (m.Value[0]) {
            case '(':
              // .NET has different ordering of numeric and named groups
              // and also detect duplicated group names which are allowed in .NET
              string name = m.Groups[2].Success ? m.Groups[2].Value : (numericGroupCount++).ToString();
              if (captureGroups.Contains(name)) {
                throw new EcmaSyntaxErrorException(InternalString.Error.RegExpDuplicatedNameGroup);
              }
              captureGroups.Add(name);
              break;
            case '\\':
              // .NET consider having an invalid back reference (backref to capture at the right) as failure
              if (m.Groups[1].Success && Int32.Parse(m.Groups[1].Value) >= captureGroups.Count) {
                return String.Empty;
              }
              break;
          }
          return m.Value;
        });

        bool unicode = (options & EcmaRegExpFlags.Unicode) != 0;
        string allChars = unicode ? "(?:[\0-\uD7FF\uE000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF])" : "[\0-\uFFFF]";
        string wildcardChars = (options & EcmaRegExpFlags.DotAll) != 0 ? allChars : unicode ? "(?:[\0-\t\x0B\f\x0E-\u2027\u202A-\uD7FF\uE000-\uFFFF]|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF])" : "[\0-\t\x0B\f\x0E-\u2027\u202A-\uFFFF]";

        // replace escape sequences that are not supported in ECMAScript but has semantic meaning in .NET 
        nPattern = (unicode ? reUnsupportedEscape : reUnsupportedEscapeNonUnicode).Replace(nPattern, "$1$1$2");

        // convert character class \w, \W, \s, \S and wildcard to explicit character set
        // and UnicodeEscape (\u{nnnnnn}) which is not supported in .NET
        nPattern = reCharClass.Replace(nPattern, m => {
          if (m.Value[0] == '\\') {
            switch (m.Value[1]) {
              case 'w':
                return "[a-zA-Z0-9_]";
              case 'W':
                return "[^a-zA-Z0-9_]";
              case 's':
                return "[\f\n\r\t\v\u2028\u2029\\p{Zs}]";
              case 'S':
                return "[^\f\n\r\t\v\u2028\u2029\\p{Zs}]";
              case 'u':
                return ConvertUnicodeEscape(m.Value);
            }
            return m.Value;
          }
          if (m.Value[0] == '.') {
            return wildcardChars;
          }
          if (m.Groups[2].Captures.Count == 0) {
            // ECMAScript allows empty CharacterClass in pattern
            // a negated empty CharacterClass means all code units or code points
            if (m.Groups[1].Length != 0) {
              return allChars;
            }
            return "(?!)";
          }
          StringBuilder sb = new StringBuilder();
          sb.Append('[');
          sb.Append(m.Groups[1].Value);
          foreach (Capture c in m.Groups[2].Captures) {
            if (c.Value[0] == '\\') {
              switch (c.Value[1]) {
                case 'w':
                  sb.Append("a-zA-Z0-9_");
                  continue;
                case 'W':
                  sb.Append(unicode ? "\0-/:-@\\[-^`{-\uDBFF\uDFFF" : "\0-/:-@\\[-^`{-\uFFFF");
                  continue;
                case 's':
                  sb.Append("\f\n\r\t\v\u2028\u2029\\p{Zs}");
                  continue;
                case 'S':
                  sb.Append(unicode ? "\x00-\x08\x0E-\x19\x21-\x99\u00A1-\u1679\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uDBFF\uDFFF" :
                                      "\x00-\x08\x0E-\x19\x21-\x99\u00A1-\u1679\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uFFFF");
                  continue;
                case 'u':
                  sb.Append(ConvertUnicodeEscape(c.Value));
                  continue;
              }
            }
            sb.Append(c.Value);
          }
          sb.Append(']');
          return sb.ToString();
        });

        // convert surrogate pairs (non-BMP character) and lone surrogates, and character class which contains such characters
        // to appropriate pattern to correctly match code points
        if (unicode && Regex.IsMatch(nPattern, "[\uD800-\uDFFF]")) {
          nPattern = reCodePoints.Replace(nPattern, m => {
            if (m.Groups[1].Success) {
              return TransformCharacterRange(m.Value, m.Groups[2].Value, m.Groups[1].Length > 0);
            }
            if (m.Groups[3].Success) {
              string chars = m.Groups[3].Value;
              if (chars.Length == 1) {
                chars = Char.IsHighSurrogate(chars[0]) ? chars + "(?![\udc00-\udfff])" : "(?<![\ud800-\udbff])" + chars;
              }
              return m.Groups[4].Success ? "(?:" + chars + ")" + m.Groups[4].Value : chars;
            }
            return m.Value;
          });
        }

        RegexOptions nOptions = RegexOptions.ECMAScript;
        if ((options & EcmaRegExpFlags.IgnoreCase) != 0) {
          nOptions |= RegexOptions.IgnoreCase;
        }
        if ((options & EcmaRegExpFlags.Multiline) != 0) {
          nOptions |= RegexOptions.Multiline;
        }
        Regex nativeRegexp;
        try {
          nativeRegexp = new Regex(nPattern, nOptions);
        } catch (ArgumentException) {
          throw new EcmaSyntaxErrorException(InternalString.Error.InvalidRegex);
        }
        re = new EcmaRegExp(nativeRegexp, pattern, canonFlags, options, numericGroupCount, captureGroups.ToArray());
        cache.TryAdd(key, re);
      }
      return re.Clone();
    }

    private string DebuggerDisplay {
      get { return (string)RegExpPrototype.ToString(this); }
    }

    private EcmaRegExp GetInstanceForGlobalReplace() {
      bool unicode = this.Unicode;
      if (unicode != ((this.OriginalFlags & EcmaRegExpFlags.Unicode) != 0)) {
        return Parse(this.Source, unicode ? this.Flags + "u" : this.Flags.Replace("u", ""));
      }
      return this;
    }

    private string FixReplacementString(string replacement) {
      // ECMA-script index substitution leaves extra digits as replacement text
      // when it is larger than the number of capture groups
      replacement = Regex.Replace(replacement, "\\$([+_{$]|\\d+|<([^>]+)>)", m => {
        switch (m.Groups[1].Value[0]) {
          case '$':
            return "$$";
          case '{':
          case '+':
          case '_':
            return "$" + m.Value;
          case '<':
            if (captureGroups.Length != numericGroupCount) {
              int index = nativeRegexp.GroupNumberFromName(m.Groups[2].Value);
              return index >= 0 ? "${" + index + "}" : "";
            }
            return m.Value;
        }
        int k = Int32.Parse(m.Groups[1].Value);
        if (k == 0) {
          return "$" + m.Value;
        }
        int d = 1;
        while (k > captureGroups.Length) {
          d++;
          k /= 10;
        }
        if (k >= captureGroups.Length) {
          return "$" + m.Value;
        }
        return "${" + nativeRegexp.GroupNumberFromName(captureGroups[k]) + "}" + m.Value.Substring(m.Length - d + 1);
      });
      return replacement;
    }

    private static string AddFlag(string flags, string ch, EcmaRegExpFlags flag, ref EcmaRegExpFlags value) {
      if (flags.Contains(ch)) {
        value |= flag;
        return ch;
      }
      return String.Empty;
    }

    private static string ConvertUnicodeEscape(string m) {
      int ch;
      if (!Int32.TryParse(m[2] == '{' ? m.Substring(3, m.Length - 4) : m.Substring(2), NumberStyles.HexNumber, null, out ch) || ch > 0x10FFFF) {
        throw new EcmaSyntaxErrorException(InternalString.Error.RegExpInvalidCodePoint);
      }
      return ch > 0xFFFF ? Char.ConvertFromUtf32(ch) : ("^$\\.*+?()[]{}".IndexOf((char)ch) >= 0 ? "\\" : "") + (char)ch;
    }

    private static string TransformCharacterRange(string originalText, string charset, bool negate) {
      unsafe {
        bool hasSurrogatePair = false;
        uint* flags = stackalloc uint[32];
        Dictionary<int, string> dict = new Dictionary<int, string>();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < 32; i++) {
          flags[i] = 0;
        }
        for (int i = 0, len = charset.Length; i < len; i++) {
          int startChar;
          string startCharStr;
          if (Char.IsSurrogatePair(charset, i)) {
            startChar = Char.ConvertToUtf32(charset, i);
            startCharStr = charset.Substring(i, 2);
            hasSurrogatePair = true;
            i++;
          } else {
            startChar = charset[i];
            startCharStr = charset.Substring(i, 1);
          }
          if (i < len - 2 && charset[i + 1] == '-') {
            i += 2;
            int endChar = Char.IsSurrogatePair(charset, i) ? Char.ConvertToUtf32(charset, i) : charset[i];
            if (endChar < startChar) {
              throw new EcmaSyntaxErrorException(InternalString.Error.InvalidRegex);
            }
            if (endChar > 0xFFFF) {
              hasSurrogatePair = true;
              if (startChar <= 0xFFFF) {
                sb.Append((char)startChar + "-\uFFFF");
                startChar = 0x10000;
                startCharStr = "\uD800\uDC00";
              }
              int startHi = startCharStr[0];
              int startLo = startCharStr[1];
              int endHi = charset[i];
              int endLo = charset[i + 1];
              if (startLo == LowSurrogateStart && (endLo == LowSurrogateEnd || endHi != startHi)) {
                SetHighSurrogateFull(flags, startHi, dict);
              } else {
                SetLowSurrogateRange(flags, startHi, dict, (char)startLo + "-" + (char)LowSurrogateEnd, negate);
              }
              if (endLo == LowSurrogateEnd && (startHi == LowSurrogateStart || endHi != startHi)) {
                SetHighSurrogateFull(flags, endHi, dict);
              } else {
                SetLowSurrogateRange(flags, endHi, dict, (char)LowSurrogateStart + "-" + (char)endLo, negate);
              }
              for (int j = startChar + 1, k = endHi - 1; j <= k; j++) {
                SetHighSurrogateFull(flags, j, dict);
              }
              i++;
            } else {
              sb.Append(new char[] { (char)startChar, '-', charset[i] });
            }
          } else if (startChar > 0xFFFF) {
            SetLowSurrogateRange(flags, startCharStr[0], dict, startCharStr.Substring(1, 1), negate);
          } else {
            sb.Append(startCharStr);
          }
        }
        if (!hasSurrogatePair) {
          return originalText;
        }

        bool hasFullPair = false;
        int start = 0;
        if (sb.Length > 0) {
          sb.Insert(0, "[");
          if (negate) {
            sb.Append('^');
          }
          sb.Append("]");
        }

        int cp = HighSurrogateStart;
        for (int i = 0; i < 32; i++) {
          ulong flag = flags[i];
          if ((flag == 0xFFFFFFFF) ^ negate) {
            if (start == 0) {
              start = cp;
            }
            cp += 32;
            continue;
          }
          if ((flag == 0) ^ negate) {
            AppendHighSurrogateRange(ref start, cp, sb);
            cp += 32;
            continue;
          }
          ulong mask = 1;
          for (int j = 0; j < 32; j++, cp++, mask <<= 1) {
            if (((flag & mask) > 0) ^ negate) {
              if (!hasFullPair) {
                if (sb.Length > 0) {
                  sb.Append('|');
                }
                sb.Append("[");
              }
              hasFullPair = true;
              if (start == 0) {
                start = cp;
              }
            } else {
              AppendHighSurrogateRange(ref start, cp, sb);
            }
          }
        }
        AppendHighSurrogateRange(ref start, cp, sb);
        if (hasFullPair) {
          sb.Append("][\uDC00-\uDFFF]");
        }
        foreach (KeyValuePair<int, string> p in dict) {
          if (sb.Length > 0) {
            sb.Append('|');
          }
          sb.AppendFormat(negate ? "{0}[^{1}]" : "{0}[{1}]", (char)p.Key, p.Value);
        }
        sb.Insert(0, "(?:");
        sb.Append(')');
        return sb.ToString();
      }
    }

    private static unsafe void SetLowSurrogateRange(uint* flags, int cp, Dictionary<int, string> dict, string range, bool negate) {
      if (!negate && IsHighSurrogateFull(flags, cp)) {
        return;
      }
      dict.TryGetValue(cp, out string current);
      if (negate) {
        SetHighSurrogateFull(flags, cp, dict);
      }
      dict[cp] = current + range;
    }

    private unsafe static void SetHighSurrogateFull(uint* flags, int cp, Dictionary<int, string> dict) {
      dict.Remove(cp);
      cp -= HighSurrogateStart;
      flags[cp >> 5] |= 1u << cp;
    }

    private unsafe static bool IsHighSurrogateFull(uint* flags, int cp) {
      cp -= HighSurrogateStart;
      return (flags[cp >> 5] &= 1u << cp) != 0;
    }

    private static void AppendHighSurrogateRange(ref int start, int cp, StringBuilder sb) {
      if (start > 0) {
        sb.Append((char)start);
        if (cp - start > 1) {
          sb.Append('-');
          sb.Append((char)(cp - 1));
        }
      }
      start = 0;
    }

    private static StickyRegex CreateStickyRegex(string pattern, bool matchStartAnchor, RegexOptions options) {
      List<MatchScope> stack = new List<MatchScope> { new MatchScope { IsStartingPattern = true } };
      Hashtable captureGroupMap = new Hashtable();
      StringBuilder sb = new StringBuilder();
      int numericGroupIndex = 0;
      int mappedNumericGroupIndex = 0;
      int lastIndex = 0;

      foreach (Match m in Regex.Matches(pattern, @"\\.|\[(?:[^\]]|\\\])+\]|(\((\?(?:[:=!]|<[=!]|<([^>])+>))?|\||\))|$")) {
        if (m.Value.Length > 0 && (m.Value[0] == '\\' || m.Value[0] == '[')) {
          continue;
        }
        MatchScope current = stack[0];
        int index = m.Groups[1].Success ? m.Groups[1].Index : m.Index;

        if (!current.IgnoreCurrentPattern && index != lastIndex && (pattern[lastIndex] != '^' || (matchStartAnchor ^ current.Negate))) {
          if (pattern[lastIndex] == '^') {
            lastIndex++;
          }
          if (current.PatternCount > 0) {
            sb.Append('|');
          }
          if (current.IsStartingPattern) {
            sb.Append('^');
          }
          if (current.PatternCount == 0) {
            sb.Append(current.ParenStart);
            current.Index = sb.Length;
          }
          current.PatternCount++;
          sb.Append(pattern.Substring(lastIndex, index - lastIndex));
        }
        if (m.Groups[1].Success) {
          switch (m.Groups[1].Value[0]) {
            case '(':
              stack.Insert(0, new MatchScope {
                Negate = m.Groups[2].Length > 0 && m.Groups[2].Value[1] == '!',
                IgnoreCurrentPattern = current.IgnoreCurrentPattern,
                IsStartingPattern = current.IsStartingPattern && lastIndex == index,
                ParenStart = m.Groups[1].Value,
              });
              if (m.Groups[2].Length == 0) {
                numericGroupIndex++;
                if (!current.IgnoreCurrentPattern) {
                  mappedNumericGroupIndex++;
                  captureGroupMap.Add(numericGroupIndex.ToString(), mappedNumericGroupIndex.ToString());
                } else {
                  captureGroupMap.Add(numericGroupIndex.ToString(), null);
                }
              } else if (m.Groups[3].Success) {
                string name = m.Groups[3].Value;
                captureGroupMap.Add(name, current.IgnoreCurrentPattern ? null : name);
              }
              break;
            case '|':
              if (stack.Count == 1 || !stack[1].IgnoreCurrentPattern) {
                current.IgnoreCurrentPattern = false;
              }
              break;
            case ')':
              MatchScope prev = stack[1];
              stack.RemoveAt(0);
              if (current.PatternCount > 0) {
                if (sb.Length == current.Index) {
                  if (prev.IsStartingPattern) {
                    sb.Length--;
                  }
                  if (prev.PatternCount > 0) {
                    sb.Length--;
                    prev.PatternCount--;
                  }
                  sb.Length -= current.ParenStart.Length;
                } else {
                  prev.PatternCount++;
                  sb.Append(')');
                }
              } else {
                prev.IgnoreCurrentPattern = true;
              }
              break;
          }
          lastIndex = index + m.Groups[1].Length;
        }
      }
      return stack[0].PatternCount > 0 ? new StickyRegex(new Regex(sb.ToString(), options), captureGroupMap) : null;
    }

    private class StickyRegex {
      public StickyRegex(Regex re, Hashtable map) {
        this.Regexp = re;
        this.GroupNameMap = map;
      }

      public Regex Regexp { get; }
      public Hashtable GroupNameMap { get; }
    }

    private class MatchScope {
      public bool Negate { get; set; }
      public int Index { get; set; }
      public int PatternCount { get; set; }
      public bool IgnoreCurrentPattern { get; set; }
      public string ParenStart { get; set; }
      public bool IsStartingPattern { get; set; }
    }

    private class MatchEvaluatorClass {
      private readonly EcmaRegExp re;
      private readonly string input;
      private readonly RuntimeFunction replacement;

      public MatchEvaluatorClass(EcmaRegExp re, string input, RuntimeFunction replacement) {
        this.re = re;
        this.input = input;
        this.replacement = replacement;
      }

      public string MatchEvaluator(Match m) {
        return RegExpPrototype.InvokeReplacementCallback(replacement, new MatchResult(re, input, m, null, 0));
      }
    }

    private class MatchResult : IEcmaRegExpResult {
      private readonly EcmaRegExp re;
      private readonly Match result;
      private readonly Hashtable groupNameMap;
      private readonly int stickyOffset;
      private EcmaObject obj;

      public MatchResult(EcmaRegExp re, string input, Match result, Hashtable groupNameMap, int offset) {
        this.re = re;
        this.Input = input;
        this.result = result;
        this.groupNameMap = groupNameMap;
        this.stickyOffset = offset;
      }

      public bool HasNamedGroups {
        get { return re.captureGroups.Length != re.numericGroupCount; }
      }

      public int Index {
        get { return result.Index + stickyOffset; }
      }

      public string Value {
        get { return result.Value; }
      }

      public string Input { get; }

      public IEnumerable<EcmaValue> Captures {
        get {
          int lastIndex = 0;
          for (int i = 0, len = re.captureGroups.Length; i < len; i++) {
            string name = re.captureGroups[i];
            if (i > 0 && groupNameMap != null) {
              name = (string)groupNameMap[name];
              if (name == null) {
                yield return EcmaValue.Undefined;
                continue;
              }
            }
            Group group = result.Groups[name];
            if (group.Success) {
              // ECMAScript RegExp always returns the last captured value for each capturing group
              // therefore index for each success group must be in increasing order
              Capture c = group.Captures[group.Captures.Count - 1];
              if (c.Index >= lastIndex) {
                lastIndex = c.Index;
                yield return c.Value;
                continue;
              }
            }
            yield return EcmaValue.Undefined;
          }
        }
      }

      public EcmaValue ToValue() {
        if (result.Success) {
          EcmaArray arr = new EcmaArray(Captures.ToArray());
          arr.CreateDataPropertyOrThrow("index", this.Index);
          arr.CreateDataPropertyOrThrow("input", this.Input);
          arr.CreateDataPropertyOrThrow("groups", CreateNamedGroupObject());
          return arr;
        }
        return EcmaValue.Null;
      }

      public EcmaValue CreateNamedGroupObject() {
        if (!this.HasNamedGroups) {
          return default;
        }
        if (obj == null) {
          obj = new EcmaObject();
          string[] names = re.nativeRegexp.GetGroupNames();
          for (int i = re.numericGroupCount, len = names.Length; i < len; i++) {
            string name = names[i];
            if (groupNameMap != null) {
              name = (string)groupNameMap[name];
              if (name == null) {
                obj.CreateDataPropertyOrThrow(names[i], EcmaValue.Undefined);
                continue;
              }
            }
            Group group = result.Groups[name];
            obj.CreateDataPropertyOrThrow(name, group.Success ? group.Value : EcmaValue.Undefined);
          }
        }
        return obj;
      }
    }
  }
}
