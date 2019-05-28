using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;
using static Codeless.Ecma.UnitTest.Tests.RegExpSS;

namespace Codeless.Ecma.UnitTest.Tests {
  public class RegExpPrototype {
    [Test, RuntimeFunctionInjection]
    public void Exec(RuntimeFunction exec) {
      IsUnconstructableFunctionWLength(exec, "exec", 1);
      IsAbruptedFromToPrimitive(exec.Bind(RegExp.Construct()));
      RequireThisRegExpObject();

      It("should coerce its argument to string", () => {
        object undefined = _;
        VerifyMatch(@"1|12", "123", new[] { "1" }, 0);
        VerifyMatch(@"1|12", 1.01, new[] { "1" }, 0);
        VerifyMatch(@"2|12", Number.Construct(1.012), new[] { "12" }, 3);
        VerifyMatch(@"\.14", CreateObject(toString: () => EcmaMath.PI), new[] { ".14" }, 1);
        VerifyMatch(@"t[a-b|q-s]", true, new[] { "tr" }, 0);
        VerifyMatch(@"AL|se", Boolean.Construct(), new[] { "se" }, 3);
        VerifyMatch(@"LS", "i", CreateObject(toString: () => false), new[] { "ls" }, 2);
        VerifyMatch(@"ll|l", Null, new[] { "ll" }, 2);
        VerifyMatch(@"nd|ne", _, new[] { "nd" }, 1);
        VerifyMatch(@"((1)|(12))((3)|(23))", String.Construct("123"), new[] { "123", "1", "1", undefined, "23", undefined, "23" }, 0);
        VerifyMatch(@"a[a-z]{2,4}", Object.Construct("abcdefghi"), new[] { "abcde" }, 0);
        VerifyMatch(@"a[a-z]{2,4}?", CreateObject(toString: () => "abcdefghi"), new[] { "abc" }, 0);
        VerifyMatch(@"(aa|aabaac|ba|b|c)*", CreateObject(toString: () => new EcmaObject(), valueOf: () => "aabaac"), new[] { "aaba", "ba" }, 0);
      });

      It("should read and reset lastIndex to 0 when global is set and the match fails", () => {
        EcmaValue re = RegExp.Construct("a", "g");
        int lastIndexReads = 0;

        re["lastIndex"] = CreateObject(valueOf: () => { lastIndexReads++; return 42; });
        Case((re, "abc"), Null);
        That(re["lastIndex"], Is.EqualTo(0));
        That(lastIndexReads, Is.EqualTo(1));

        lastIndexReads = 0;
        re["lastIndex"] = CreateObject(valueOf: () => { lastIndexReads++; return -1; });
        Case((re, "nbc"), Null);
        That(re["lastIndex"], Is.EqualTo(0));
        That(lastIndexReads, Is.EqualTo(1));
      });

      It("should read but not write lastIndex when global and sticky are unset", () => {
        EcmaValue re = RegExp.Construct("a");
        int lastIndexReads = 0;

        EcmaValue obj = CreateObject(valueOf: () => { lastIndexReads++; return 0; });
        re["lastIndex"] = obj;
        Case((re, "nbc"), Null);
        That(re["lastIndex"], Is.EqualTo(obj));
        That(lastIndexReads, Is.EqualTo(1));
      });

      It("should honor the global flag", () => {
        VerifyMatch(@"(?:ab|cd)\d?", "g", "aacd2233ab12nm444ab42", (new[] { "cd2" }, 2), (new[] { "ab1" }, 8), (new[] { "ab4" }, 17));
      });

      It("should honor the sticky flag", () => {
        EcmaValue re;
        Case((RegExp.Construct("b", "y"), "ab"), Null, "Stops match execution after first match failure");

        re = RegExp.Construct("c", "y");
        DefineProperty(re, "lastIndex", writable: false);
        Case((re, "abc"), Throws.TypeError, "Match failure with non-writable `lastIndex` property");

        re = RegExp.Construct("c", "y");
        re["lastIndex"] = 1;
        exec.Call(re, "abc");
        That(re["lastIndex"], Is.EqualTo(0), "Resets the `lastIndex` property to zero after a match failure");

        re = RegExp.Construct(".", "y");
        re["lastIndex"] = 1;
        Case((re, "a"), Null, "Honors initial value of the `lastIndex` property");

        re = RegExp.Construct("abc", "y");
        exec.Call(re, "abc");
        That(re["lastIndex"], Is.EqualTo(3), "Sets the `lastIndex` property to the end index of the first match");
      });

      It("should only match pattern starting at lastIndex when sticky is set", () => {
        EcmaValue re;
        re = RegExp.Construct("foo", "y");
        Case((re, "afoo"), Null);
        re["lastIndex"] = 1;
        Case((re, "afoo"), new[] { "foo" });

        re = RegExp.Construct("^foo|bar", "y");
        Case((re, "foobar"), new[] { "foo" });
        Case((re, "foobar"), new[] { "bar" });

        re = RegExp.Construct("(?!^)foo", "y");
        Case((re, "foofoo"), Null);
        re["lastIndex"] = 3;
        Case((re, "foofoo"), new[] { "foo" });
      });

      It("should honor the unicode flag", () => {
        Case((RegExp.Construct("\\udf06", "u"), "\ud834\udf06"), Null);
        VerifyMatch(".", "u", "\ud834\udf06", new[] { "\ud834\udf06" }, 0);

        EcmaValue re = RegExp.Construct(".", "ug");
        exec.Call(re, "\ud834\udf06");
        That(re["lastIndex"], Is.EqualTo(2));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Test(RuntimeFunction test) {
      IsUnconstructableFunctionWLength(test, "test", 1);
      IsAbruptedFromToPrimitive(test.Bind(RegExp.Construct()));
      RequireThisObject();

      It("is equivalent to the expression RegExp.prototype.exec(string) != null", () => {
        void CheckEquivalence(EcmaValue re, EcmaValue str) => Case((re, str), re.Invoke("exec", str) != EcmaValue.Null);

        CheckEquivalence(RegExp.Construct("1|12"), "123");
        CheckEquivalence(RegExp.Construct("1|12"), String.Construct("123"));
        CheckEquivalence(RegExp.Construct("1|12"), CreateObject(toString: () => "123"));
        CheckEquivalence(RegExp.Construct("1|12"), 1.01);
        CheckEquivalence(RegExp.Construct("1|12"), Number.Construct(1.012));
        CheckEquivalence(RegExp.Construct("1|12"), CreateObject(toString: () => 1.012));
        CheckEquivalence(RegExp.Construct("t[a-b|q-s]"), true);
        CheckEquivalence(RegExp.Construct("AL|se"), Boolean.Construct());
        CheckEquivalence(RegExp.Construct("AL|se"), CreateObject(toString: () => false));
        CheckEquivalence(RegExp.Construct("undefined"), _);
        CheckEquivalence(RegExp.Construct("ll|l"), Null);
        CheckEquivalence(RegExp.Construct("[a-z]n"), RuntimeFunction.Create(() => _));
        CheckEquivalence(RegExp.Construct("a[a-z]{2,4}"), Object.Construct("abcdefghi"));
        CheckEquivalence(RegExp.Construct("(aa|aabaac|ba|b|c)*"), CreateObject(toString: () => new EcmaObject(), valueOf: () => "aabaac"));
      });

      It("should start searching from 0 when lastIndex is negative", () => {
        EcmaValue re = RegExp.Construct("(?:ab|cd)\\d?", "g");

        re["lastIndex"] = -1;
        Case((re, "aacd22 "), true, "lastIndex = -1");
        That(re["lastIndex"], Is.EqualTo(5), "lastIndex = -1");

        re["lastIndex"] = -100;
        Case((re, "aacd22 "), true, "lastIndex = -100");
        That(re["lastIndex"], Is.EqualTo(5), "lastIndex = -100");
      });

      It("should honor the sticky flag", () => {
        EcmaValue re;
        Case((RegExp.Construct("b", "y"), "ab"), false, "Stops match execution after first match failure");

        re = RegExp.Construct("c", "y");
        DefineProperty(re, "lastIndex", writable: false);
        Case((re, "abc"), Throws.TypeError, "Match failure with non-writable `lastIndex` property");

        re = RegExp.Construct("c", "y");
        re["lastIndex"] = 1;
        test.Call(re, "abc");
        That(re["lastIndex"], Is.EqualTo(0), "Resets the `lastIndex` property to zero after a match failure");

        re = RegExp.Construct(".", "y");
        re["lastIndex"] = 1;
        Case((re, "a"), false, "Honors initial value of the `lastIndex` property");

        re = RegExp.Construct("abc", "y");
        test.Call(re, "abc");
        That(re["lastIndex"], Is.EqualTo(3), "Sets the `lastIndex` property to the end index of the first match");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);
    }

    [Test, RuntimeFunctionInjection]
    public void Match(RuntimeFunction match) {
      IsUnconstructableFunctionWLength(match, "[Symbol.match]", 1);
      IsAbruptedFromToPrimitive(match.Bind(RegExp.Construct()));
      RequireThisObject();
      EcmaValue re;

      // Return value after successful match
      VerifyMatch(@"b.", "abcd", new[] { "bc" }, 1);
      VerifyMatch(@"b(.).(.).", "abcdefg", new[] { "bcdef", "c", "e" }, 1);

      // String coercion of first parameter
      Case((RegExp.Construct("toString value"), CreateObject(toString: () => "toString value", valueOf: () => ThrowTest262Exception.Call())), Is.Not.EqualTo(Null));

      // Return value should not have `index` and `input` property
      EcmaValue result = match.Call(RegExp.Construct(".(.).", "g"), "abcdefghi");
      That(EcmaArray.IsArray(result));
      That(result.HasOwnProperty("index"), Is.EqualTo(false));
      That(result.HasOwnProperty("input"), Is.EqualTo(false));
      That(result["index"], Is.Undefined);
      That(result["input"], Is.Undefined);

      // global: Return value when no matches occur with the `global` flag
      Case((RegExp.Construct("a", "g"), "b"), Null);

      // global: Return an array containing all matches
      Case((RegExp.Construct(".(.).", "g"), "abcdefghi"), new[] { "abc", "def", "ghi" });

      // global: lastIndex is explicitly advanced for zero-length matches for "global" instances
      Case((RegExp.Construct("", "g"), "abc"), new[] { "", "", "", "" });

      // global: Initializing the value of `lastIndex` on "global" instances
      re = RegExp.Construct(".", "g");
      re["lastIndex"] = 1;
      Case((re, "a"), Is.Not.EqualTo(Null));

      // unicode: lastIndex is advanced according to width of astral symbols
      Case((RegExp.Construct("^|\\udf06", "ug"), "\ud834\udf06"), new[] { "" });

      // unicode: Return value after successful match with extended unicode capturing groups
      VerifyMatch(@"b(.).(.).", "u", "ab\ud834\udf06defg", new[] { "b\ud834\udf06def", "\ud834\udf06", "e" }, 1);

      // sticky: Type coercion of `lastIndex` property value
      re = RegExp.Construct(".", "y");
      re["lastIndex"] = "1.9";
      Case((re, "abc"), new[] { "b" });

      // sticky: Accumulates consecutive matches when `g` flag is present
      Case((RegExp.Construct("a", "yg"), "aaba"), new[] { "a", "a" });

      // sticky: Stops match execution after first match failure
      Case((RegExp.Construct("a", "yg"), "ba"), Null);

      // sticky: Honors initial value of the `lastIndex` property
      re = RegExp.Construct(".", "y");
      re["lastIndex"] = 1;
      Case((re, "abc"), new[] { "b" });

      // sticky: Resets the `lastIndex` property to zero after a match failure
      re = RegExp.Construct("c", "y");
      re["lastIndex"] = 1;
      match.Call(re, "abc");
      That(re["lastIndex"], Is.EqualTo(0));

      // sticky: Sets the `lastIndex` property to the end index of the first match
      re = RegExp.Construct("abc", "y");
      match.Call(re, "abc");
      That(re["lastIndex"], Is.EqualTo(3));

      // sticky: Match failure with non-writable `lastIndex` property
      re = RegExp.Construct("c", "y");
      DefineProperty(re, "lastIndex", writable: false);
      Case((re, "abc"), Throws.TypeError);

      // sticky: Behavior when coercion of `lastIndex` attribute throws an error
      re = RegExp.Construct("c", "y");
      re["lastIndex"] = CreateObject(valueOf: ThrowTest262Exception);
      Case((re, "abc"), Throws.Test262);

      // Boolean coercion of `global` property
      // ---
    }

    [Test, RuntimeFunctionInjection]
    public void MatchAll(RuntimeFunction matchAll) {
      IsUnconstructableFunctionWLength(matchAll, "[Symbol.matchAll]", 1);
      IsAbruptedFromToPrimitive(matchAll.Bind(RegExp.Construct()));
      RequireThisObject();
    }

    [Test, RuntimeFunctionInjection]
    public void Replace(RuntimeFunction replace) {
      IsUnconstructableFunctionWLength(replace, "[Symbol.replace]", 2);
      IsAbruptedFromToPrimitive(replace.Bind(RegExp.Construct()));
      IsAbruptedFromToPrimitive(replace.Bind(RegExp.Construct(), ""));
      RequireThisObject();
      EcmaValue re;

      // Return original string when no matches occur
      Case((RegExp.Construct("x"), "abc", "X"), "abc");

      // Return value when replacement pattern does not match final code point
      Case((RegExp.Construct("abc"), "abcd", "X"), "Xd");
      Case((RegExp.Construct("bc"), "abcd", "X"), "aXd");
      Case((RegExp.Construct("c"), "abcd", "X"), "abXd");
      Case((RegExp.Construct("ab"), "abcd", "X"), "Xcd");
      Case((RegExp.Construct("b"), "abcd", "X"), "aXcd");
      Case((RegExp.Construct("a"), "abcd", "X"), "Xbcd");

      // Return value when replacement pattern matches final code point
      Case((RegExp.Construct("abcd"), "abcd", "X"), "X");
      Case((RegExp.Construct("bcd"), "abcd", "X"), "aX");
      Case((RegExp.Construct("cd"), "abcd", "X"), "abX");
      Case((RegExp.Construct("d"), "abcd", "X"), "abcX");

      // subsitution
      Case((RegExp.Construct("c"), "abc", "[$']"), "ab[]");
      Case((RegExp.Construct("b"), "abc", "[$']"), "a[c]c");
      Case((RegExp.Construct("a"), "abc", "[$`]"), "[]bc");
      Case((RegExp.Construct("b"), "abc", "[$`]"), "a[a]c");
      Case((RegExp.Construct("b(c)(z)?(.)"), "abcde", "[$1$2$3]"), "a[cd]e");
      Case((RegExp.Construct("b(c)(z)?(.)"), "abcde", "[$1$2$3$4$0]"), "a[cd$4$0]e");
      Case((RegExp.Construct("b(c)(z)?(.)"), "abcde", "[$01$02$03]"), "a[cd]e");
      Case((RegExp.Construct("b(c)(z)?(.)"), "abcde", "[$01$02$03$04$00]"), "a[cd$04$00]e");
      Case((RegExp.Construct("."), "abc", "$$"), "$bc");
      Case((RegExp.Construct("."), "abc", "$"), "$bc");
      Case((RegExp.Construct("."), "abc", "\\$"), "\\$bc");
      Case((RegExp.Construct("."), "abc", "$$$"), "$$bc");
      Case((RegExp.Construct(".4?."), "abc", "[$&]"), "[ab]c");

      // Arguments of functional replaceValue
      EcmaValue args = default;
      replace.Call(RegExp.Construct("b(.).(.)"), "abcdef", RuntimeFunction.Create(() => args = Arguments));
      That(args, Is.EquivalentTo(new object[] { "bcde", "c", "e", 1, "abcdef" }));

      // String coercion of the value returned by functional replaceValue
      Case((RegExp.Construct("x"), "[x]", RuntimeFunction.Create(() => CreateObject(toString: () => "toString value"))), "[toString value]");

      // Behavior when error is thrown by functional replaceValue.
      Case((RegExp.Construct("x"), "[x]", ThrowTest262Exception), Throws.Test262);
      Case((RegExp.Construct("x"), "[x]", RuntimeFunction.Create(() => CreateObject(toString: ThrowTest262Exception))), Throws.Test262);

      // unicode: lastIndex is advanced according to width of astral symbols
      Case((RegExp.Construct("^|\\udf06", "ug"), "\ud834\udf06", "XXX"), "XXX\ud834\udf06");

      // custom exec result
      re = RegExp.Construct(".");
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("0", CreateObject(toString: () => "toString value"))));
      Case((re, "", "foo[$&]bar"), "foo[toString value]bar");
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("0", CreateObject(toString: ThrowTest262Exception))));
      Case((re, "a", "b"), Throws.Test262);
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("3", CreateObject(toString: () => "toString value")), ("length", 4)));
      Case((re, "", "foo[$3]bar"), "foo[toString value]bar");
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("3", CreateObject(toString: ThrowTest262Exception)), ("length", 4)));
      Case((re, "", "foo[$3]bar"), Throws.Test262);
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("0", ""), ("1", "foo"), ("2", "bar"), ("3", "baz"), ("length", CreateObject(valueOf: () => 3.9))));
      Case((re, "", "$1$2$3"), "foobar$3");
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("length", CreateObject(toString: ThrowTest262Exception))));
      Case((re, "a", "b"), Throws.Test262);
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("index", CreateObject(valueOf: () => 2.9))));
      Case((re, "abcd", ""), "ab");
      re["exec"] = RuntimeFunction.Create(() => CreateObject(("index", CreateObject(toString: ThrowTest262Exception))));
      Case((re, "a", "b"), Throws.Test262);

      // global: Behavior when error is thrown while initializing `lastIndex` property for "global" instances
      re = RegExp.Construct(".", "g");
      DefineProperty(re, "lastIndex", writable: false);
      Case((re, "x", "x"), Throws.TypeError);

      // global: Initialization of `lastIndex` property for "global" instances
      re = RegExp.Construct(".", "g");
      re["lastIndex"] = 1;
      Case((re, "aa", "x"), "xx");

      // global: Behavior when position is incremented during result accumulation
      int callCount = 0;
      re = RegExp.Construct(".", "g");
      re["exec"] = RuntimeFunction.Create(() => {
        callCount++;
        switch (callCount) {
          case 1:
            return CreateObject(("index", 1), ("length", 1), ("0", 0));
          case 2:
            return CreateObject(("index", 3), ("length", 1), ("0", 0));
        }
        return Null;
      });
      Case((re, "abcde", "X"), "aXcXe");
      That(callCount, Is.EqualTo(3));

      // global: Behavior when position is decremented during result accumulation
      callCount = 0;
      re = RegExp.Construct(".", "g");
      re["exec"] = RuntimeFunction.Create(() => {
        callCount++;
        switch (callCount) {
          case 1:
            return CreateObject(("index", 3), ("length", 1), ("0", 0));
          case 2:
            return CreateObject(("index", 1), ("length", 1), ("0", 0));
        }
        return Null;
      });
      Case((re, "abcde", "X"), "abcXe");
      That(callCount, Is.EqualTo(3));

      // global: Behavior when there is an error thrown while accessing the `exec` method of "global" instances
      re = RegExp.Construct(".");
      DefineProperty(re, "exec", get: ThrowTest262Exception.Call);
      Case(re, Throws.Test262);
      That(re["lastIndex"], Is.EqualTo(0));

      // global: Behavior when error is thrown during retrieval of `global` property
      EcmaValue obj = new EcmaObject();
      DefineProperty(obj, "global", get: ThrowTest262Exception.Call);
      Case(obj, Throws.Test262);

      // global: Errors thrown by `unicode` accessor are forwarded to the runtime for global patterns
      EcmaValue nonGlobalRe = RegExp.Construct(".");
      EcmaValue globalRe = RegExp.Construct(".", "g");
      DefineProperty(nonGlobalRe, "unicode", get: ThrowTest262Exception.Call);
      DefineProperty(globalRe, "unicode", get: ThrowTest262Exception.Call);
      Case(nonGlobalRe, Throws.Nothing);
      Case(globalRe, Throws.Test262);

      // sticky:  Stops match execution after first match failure
      Case((RegExp.Construct("b", "y"), "ab", "x"), "ab");
      Case((RegExp.Construct("a", "yg"), "aaba", "x"), "xxba");

      // sticky: Honors initial value of the `lastIndex` property
      re = RegExp.Construct(".", "y");
      re["lastIndex"] = 1;
      Case((re, "aaa", "x"), "axa");

      // sticky: Resets the `lastIndex` property to zero after a match failure
      re = RegExp.Construct("c", "y");
      re["lastIndex"] = 1;
      replace.Call(re, "abc", "x");
      That(re["lastIndex"], Is.EqualTo(0));

      // sticky: Sets the `lastIndex` property to the end index of the first match
      re = RegExp.Construct("abc", "y");
      replace.Call(re, "abc", "x");
      That(re["lastIndex"], Is.EqualTo(3));

      // sticky: Match failure with non-writable `lastIndex` property
      re = RegExp.Construct("c", "y");
      DefineProperty(re, "lastIndex", writable: false);
      Case((re, "abc", "x"), Throws.TypeError);

      // Boolean coercion of `global` property
      re = RegExp.Construct("a", "g");
      DefineProperty(re, "global", writable: true);

      re["lastIndex"] = 0;
      re["global"] = _;
      Case((re, "aa", "b"), "ba", "value: undefined");

      re["lastIndex"] = 0;
      re["global"] = Null;
      Case((re, "aa", "b"), "ba", "value: null");

      re["lastIndex"] = 0;
      re["global"] = false;
      Case((re, "aa", "b"), "ba", "value: false");

      re["lastIndex"] = 0;
      re["global"] = NaN;
      Case((re, "aa", "b"), "ba", "value: NaN");

      re["lastIndex"] = 0;
      re["global"] = 0;
      Case((re, "aa", "b"), "ba", "value: 0");

      re["lastIndex"] = 0;
      re["global"] = "";
      Case((re, "aa", "b"), "ba", "value: \"\"");

      re["lastIndex"] = 0;
      re["global"] = true;
      Case((re, "aa", "b"), "bb", "value: true");

      re["lastIndex"] = 0;
      re["global"] = 86;
      Case((re, "aa", "b"), "bb", "value: 86");

      re["lastIndex"] = 0;
      re["global"] = new Symbol();
      Case((re, "aa", "b"), "bb", "value: Symbol");

      re["lastIndex"] = 0;
      re["global"] = new EcmaObject();
      Case((re, "aa", "b"), "bb", "value: {}");

      // Boolean coercion of `unicode` property
      re = RegExp.Construct("^|\\udf06", "g");
      DefineProperty(re, "unicode", writable: true);

      re["unicode"] = _;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = Null;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = false;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = NaN;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = 0;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = "";
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834XXX");

      re["unicode"] = true;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834\udf06");

      re["unicode"] = 86;
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834\udf06");

      re["unicode"] = new Symbol();
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834\udf06");

      re["unicode"] = new EcmaObject();
      Case((re, "\ud834\udf06", "XXX"), "XXX\ud834\udf06");

      It("should perform numeric and named capture group substitution", () => {
        Case((RegExp.Construct("(.)"), "a", "|$1|"), "|a|");
        Case((RegExp.Construct("(.)"), "a", "|$10|"), "|a0|");
        Case((RegExp.Construct("(.)"), "a", "|$<name>|"), "|$<name>|");
        Case((RegExp.Construct("(?<name>.)"), "a", "|$<name>|"), "|a|");
        Case((RegExp.Construct("(.)(?<a>.)(.)(?<b>.)"), "abcd", "$1|$2|$3|$4|$<a>|$<b>"), "a|b|c|d|b|d");
      });

      It("should ignore tokens that are only available in .NET regular expression", () => {
        Case((RegExp.Construct("."), "a", "$_"), "$_");
        Case((RegExp.Construct("."), "a", "$+"), "$+");
        Case((RegExp.Construct("."), "a", "${0}"), "${0}");
        Case((RegExp.Construct("(?<name>.)"), "a", "${name}"), "${name}");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Search(RuntimeFunction search) {
      IsUnconstructableFunctionWLength(search, "[Symbol.search]", 1);
      IsAbruptedFromToPrimitive(search.Bind(RegExp.Construct()));
      RequireThisObject();

      Case((RegExp.Construct("a"), "abc"), 0);
      Case((RegExp.Construct("b"), "abc"), 1);
      Case((RegExp.Construct("c"), "abc"), 2);
      Case((RegExp.Construct("z"), "abc"), -1);

      // String coercion of `string` argument
      Case((RegExp.Construct("ring"), CreateObject(toString: () => "toString value")), 4);

      // unicode: Advancement of lastIndex
      Case((RegExp.Construct("\\\udf06", "u"), "\ud834\udf06"), -1);

      // sticky: Stops match execution after first match failure
      Case((RegExp.Construct("a", "y"), "ba"), -1);
    }

    [Test, RuntimeFunctionInjection]
    public void Split(RuntimeFunction split) {
      IsUnconstructableFunctionWLength(split, "[Symbol.split]", 2);
      IsAbruptedFromToPrimitive(split.Bind(RegExp.Construct()));
      IsAbruptedFromToPrimitive(split.Bind(RegExp.Construct(), ""));
      RequireThisObject();

      // Return value should be an array
      That(EcmaArray.IsArray(split.Call(RegExp.Construct(""), "")));

      // Characters after the final match are appended to the result
      Case((RegExp.Construct("d"), "abcdefg"), new[] { "abc", "efg" });

      // lastIndex is advanced according to width of astral symbols following match success
      Case((RegExp.Construct(".", "u"), "\ud834\udf06"), new[] { "", "" });
      Case((RegExp.Construct("\\udf06", "u"), "\ud834\udf06"), new[] { "\ud834\udf06" });

      // lastIndex is explicitly advanced following an empty match
      Case((RegExp.Construct(""), "abc"), new[] { "a", "b", "c" });

      // Successful match of empty string
      Case((RegExp.Construct(""), ""), new object[] { });

      // Unsuccessful match of empty string
      Case((RegExp.Construct("."), ""), new[] { "" });

      // Results limited to number specified as second argument
      Case((RegExp.Construct("x"), "axbxcxdxe", 3), new[] { "a", "b", "c" });

      // The `limit` argument is applied to capturing groups
      Case((RegExp.Construct("c(d)(e)"), "abcdefg", 2), new[] { "ab", "d" });
    }

    [Test, RuntimeFunctionInjection]
    public void Source(RuntimeFunction source) {
      That(RegExp.Prototype, Has.OwnProperty("source", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("source").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(source, "get source", 0);
      Case(RegExp.Prototype, "(?:)");
      Case(RegExp.Construct(""), "(?:)");

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      It("should return value that can be used to create an equivalent RegExp", () => {
        EcmaValue re;
        That(RegExp.Construct(RegExp.Construct("")["source"]).Invoke("test", ""), Is.EqualTo(true));

        re = RegExp.Construct(RegExp.Construct("\n")["source"]);
        That(re.Invoke("test", "\n"), Is.EqualTo(true), "input: \"\\n\"");
        That(re.Invoke("test", "_\n_"), Is.EqualTo(true), "input: \"_\\n_\"");
        That(re.Invoke("test", "\\n"), Is.EqualTo(false), "input: \"\\\\n\"");
        That(re.Invoke("test", "\r"), Is.EqualTo(false), "input: \"\\r\"");
        That(re.Invoke("test", "n"), Is.EqualTo(false), "input: \"n\"");

        re = RegExp.Construct(RegExp.Construct("/")["source"]);
        That(re.Invoke("test", "/"), Is.EqualTo(true), "input: \"/\"");
        That(re.Invoke("test", "_/_"), Is.EqualTo(true), "input: \"_/_\"");
        That(re.Invoke("test", "\\"), Is.EqualTo(false), "input: \"\\\"");

        re = RegExp.Construct(RegExp.Construct("\\ud834\\udf06", "u")["source"], "u");
        That(re.Invoke("test", "\ud834\udf06"), Is.EqualTo(true));
        That(re.Invoke("test", char.ConvertFromUtf32(119558)), Is.EqualTo(true));

        re = RegExp.Construct(RegExp.Construct("\\u{1d306}", "u")["source"], "u");
        That(re.Invoke("test", "\ud834\udf06"), Is.EqualTo(true));
        That(re.Invoke("test", char.ConvertFromUtf32(119558)), Is.EqualTo(true));

        re = RegExp.Construct(RegExp.Construct("ab{2,4}c$")["source"]);
        That(re.Invoke("test", "abbc"), Is.EqualTo(true), "input: abbc");
        That(re.Invoke("test", "abbbc"), Is.EqualTo(true), "input: abbbc");
        That(re.Invoke("test", "abbbbc"), Is.EqualTo(true), "input: abbbbc");
        That(re.Invoke("test", "xabbc"), Is.EqualTo(true), "input: xabbc");
        That(re.Invoke("test", "xabbbc"), Is.EqualTo(true), "input: xabbbc");
        That(re.Invoke("test", "xabbbbc"), Is.EqualTo(true), "input: xabbbbc");
        That(re.Invoke("test", "ac"), Is.EqualTo(false), "input: ac");
        That(re.Invoke("test", "abc"), Is.EqualTo(false), "input: abc");
        That(re.Invoke("test", "abbcx"), Is.EqualTo(false), "input: abbcx");
        That(re.Invoke("test", "bbc"), Is.EqualTo(false), "input: bbc");
        That(re.Invoke("test", "abb"), Is.EqualTo(false), "input: abb");
        That(re.Invoke("test", "abbbbbc"), Is.EqualTo(false), "input: abbbbbc");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Flags(RuntimeFunction flags) {
      That(RegExp.Prototype, Has.OwnProperty("flags", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("flags").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(flags, "get flags", 0);
      Case(RegExp.Prototype, "");

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);

      Case(RegExp.Construct(".", ""), "");
      Case(RegExp.Construct(".", "g"), "g");
      Case(RegExp.Construct(".", "i"), "i");
      Case(RegExp.Construct(".", "m"), "m");
      Case(RegExp.Construct(".", "s"), "s");
      Case(RegExp.Construct(".", "u"), "u");
      Case(RegExp.Construct(".", "y"), "y");
      Case(RegExp.Construct(".", "gimsuy"), "gimsuy");
      Case(RegExp.Construct(".", "yusmig"), "gimsuy");

      It("should get property in specified order", () => {
        EcmaValue obj = new EcmaObject();
        ArrayList calls = new ArrayList();
        DefineProperty(obj, "global", get: CreateFunction(calls, "g"));
        DefineProperty(obj, "ignoreCase", get: CreateFunction(calls, "i"));
        DefineProperty(obj, "multiline", get: CreateFunction(calls, "m"));
        DefineProperty(obj, "dotAll", get: CreateFunction(calls, "s"));
        DefineProperty(obj, "unicode", get: CreateFunction(calls, "u"));
        DefineProperty(obj, "sticky", get: CreateFunction(calls, "y"));
        flags.Call(obj);
        That(calls, Is.EquivalentTo(new[] { "g", "i", "m", "s", "u", "y" }));
      });

      It("should rethrow exceptions raised in property gets", () => {
        EcmaValue obj;

        obj = new EcmaObject();
        DefineProperty(obj, "global", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);

        obj = new EcmaObject();
        DefineProperty(obj, "ignoreCase", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);

        obj = new EcmaObject();
        DefineProperty(obj, "multiline", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);

        obj = new EcmaObject();
        DefineProperty(obj, "dotAll", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);

        obj = new EcmaObject();
        DefineProperty(obj, "unicode", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);

        obj = new EcmaObject();
        DefineProperty(obj, "sticky", get: ThrowTest262Exception.Call);
        Case(obj, Throws.Test262);
      });

      It("should coerce dotAll to boolean", () => {
        EcmaValue obj = new EcmaObject();

        obj["dotAll"] = Null;
        Case(obj, "");

        obj["dotAll"] = Undefined;
        Case(obj, "");

        obj["dotAll"] = "";
        Case(obj, "");

        obj["dotAll"] = "string";
        Case(obj, "s");

        obj["dotAll"] = 86;
        Case(obj, "s");

        obj["dotAll"] = new Symbol();
        Case(obj, "s");

        obj["dotAll"] = new EcmaObject();
        Case(obj, "s");

        obj["dotAll"] = new EcmaArray();
        Case(obj, "s");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Global(RuntimeFunction global) {
      That(RegExp.Prototype, Has.OwnProperty("global", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("global").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(global, "get global", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "i"), false);
      Case(RegExp.Construct(".", "m"), false);
      Case(RegExp.Construct(".", "mi"), false);

      Case(RegExp.Construct(".", "g"), true);
      Case(RegExp.Construct(".", "gi"), true);
      Case(RegExp.Construct(".", "gm"), true);
      Case(RegExp.Construct(".", "gmi"), true);
    }

    [Test, RuntimeFunctionInjection]
    public void DotAll(RuntimeFunction dotAll) {
      That(RegExp.Prototype, Has.OwnProperty("dotAll", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("dotAll").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(dotAll, "get dotAll", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "i"), false);
      Case(RegExp.Construct(".", "g"), false);
      Case(RegExp.Construct(".", "gi"), false);

      Case(RegExp.Construct(".", "s"), true);
      Case(RegExp.Construct(".", "si"), true);
      Case(RegExp.Construct(".", "sg"), true);
      Case(RegExp.Construct(".", "sgi"), true);
    }

    [Test, RuntimeFunctionInjection]
    public void IgnoreCase(RuntimeFunction ignoreCase) {
      That(RegExp.Prototype, Has.OwnProperty("ignoreCase", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("ignoreCase").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(ignoreCase, "get ignoreCase", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "m"), false);
      Case(RegExp.Construct(".", "g"), false);
      Case(RegExp.Construct(".", "gm"), false);

      Case(RegExp.Construct(".", "i"), true);
      Case(RegExp.Construct(".", "im"), true);
      Case(RegExp.Construct(".", "ig"), true);
      Case(RegExp.Construct(".", "igm"), true);
    }

    [Test, RuntimeFunctionInjection]
    public void Multiline(RuntimeFunction multiline) {
      That(RegExp.Prototype, Has.OwnProperty("multiline", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("multiline").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(multiline, "get multiline", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "i"), false);
      Case(RegExp.Construct(".", "g"), false);
      Case(RegExp.Construct(".", "gi"), false);

      Case(RegExp.Construct(".", "m"), true);
      Case(RegExp.Construct(".", "mi"), true);
      Case(RegExp.Construct(".", "mg"), true);
      Case(RegExp.Construct(".", "mgi"), true);
    }

    [Test, RuntimeFunctionInjection]
    public void Sticky(RuntimeFunction sticky) {
      That(RegExp.Prototype, Has.OwnProperty("sticky", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("sticky").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(sticky, "get sticky", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "i"), false);
      Case(RegExp.Construct(".", "g"), false);
      Case(RegExp.Construct(".", "gi"), false);

      Case(RegExp.Construct(".", "y"), true);
      Case(RegExp.Construct(".", "yi"), true);
      Case(RegExp.Construct(".", "yg"), true);
      Case(RegExp.Construct(".", "ygi"), true);
    }

    [Test, RuntimeFunctionInjection]
    public void Unicode(RuntimeFunction unicode) {
      That(RegExp.Prototype, Has.OwnProperty("unicode", EcmaPropertyAttributes.Configurable));
      That(RegExp.Prototype.GetOwnProperty("unicode").Set, Is.Undefined);

      IsUnconstructableFunctionWLength(unicode, "get unicode", 0);
      Case(RegExp.Prototype, Is.Undefined);

      Case(_, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(1, Throws.TypeError);
      Case("string", Throws.TypeError);
      Case(new Symbol(), Throws.TypeError);
      Case(new EcmaObject(), Throws.TypeError);
      Case(new EcmaArray(), Throws.TypeError);

      Case(RegExp.Construct("."), false);
      Case(RegExp.Construct(".", "i"), false);
      Case(RegExp.Construct(".", "g"), false);
      Case(RegExp.Construct(".", "gi"), false);

      Case(RegExp.Construct(".", "u"), true);
      Case(RegExp.Construct(".", "ui"), true);
      Case(RegExp.Construct(".", "ug"), true);
      Case(RegExp.Construct(".", "ugi"), true);
    }

    #region Helper
    private static void RequireThisObject() {
      It("should throw a TypeError if this value is not an Object", () => {
        EcmaValue obj = CreateObject(toString: ThrowTest262Exception);
        Case((_, obj), Throws.TypeError);
        Case((Null, obj), Throws.TypeError);
        Case((true, obj), Throws.TypeError);
        Case((1, obj), Throws.TypeError);
        Case(("string", obj), Throws.TypeError);
        Case((new Symbol(), obj), Throws.TypeError);
      });
    }

    private static void RequireThisRegExpObject() {
      It("should throw a TypeError if this value has no [[RegExpMatcher]] internal slot", () => {
        EcmaValue obj = CreateObject(toString: ThrowTest262Exception);
        Case((_, obj), Throws.TypeError);
        Case((Null, obj), Throws.TypeError);
        Case((true, obj), Throws.TypeError);
        Case((1, obj), Throws.TypeError);
        Case(("string", obj), Throws.TypeError);
        Case((new Symbol(), obj), Throws.TypeError);
        Case((new EcmaObject(), obj), Throws.TypeError);
        Case((new EcmaArray(), obj), Throws.TypeError);
        Case((RegExp.Prototype, obj), Throws.TypeError);
      });
    }
    #endregion
  }
}
