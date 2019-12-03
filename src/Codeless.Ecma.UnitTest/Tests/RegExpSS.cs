using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class RegExpSS : TestBase {
    object undefined = Undefined;

    [Test]
    public void Pattern() {
      Throw("\\", "");
    }

    [Test]
    public void PatternCharacter() {
      It("should not recognize escape sequence not defined in specification", () => {
        VerifyMatch("\\a\\e\\g\\h\\i\\j\\k\\l\\m\\o\\p\\q\\y\\z\\A\\C\\E\\F\\G\\H\\I\\J\\K\\L\\M\\N\\O\\P\\Q\\R\\T\\U\\V\\X\\Y\\Z\\\\", "aeghijklmopqyzACEFGHIJKLMNOPQRTUVXYZ\\", true);
      });
    }

    [Test]
    public void CharacterClassEscape() {
      It("should detect a-z, A-Z, 0-9, _ using \\w", () => {
        EcmaValue re = RegExp.Construct("\\w");
        string wordChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        string result = "";
        for (int j = 0x0000; j < 0x10000; j++) {
          EcmaValue str = String.Invoke("fromCharCode", j);
          if (re.Invoke("test", str)) {
            result += (string)str;
          }
        }
        That(result, Is.EqualTo(wordChars));
      });

      It("should detect characters other than a-z, A-Z, 0-9, _ using \\W", () => {
        EcmaValue re = RegExp.Construct("\\W");
        string wordChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        string result = "";
        for (int j = 0x0000; j < 0x10000; j++) {
          EcmaValue str = String.Invoke("fromCharCode", j);
          if (!(bool)re.Invoke("test", str)) {
            result += (string)str;
          }
        }
        That(result, Is.EqualTo(wordChars));
      });

      It("should detect non WhiteSpace using \\S", () => {
        EcmaValue re = RegExp.Construct("\\S");
        int[] whitespaceChars = { 0x0009, 0x000A, 0x000B, 0x000C, 0x000D, 0x0020, 0x00A0, 0x1680, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 0x2009, 0x200A, 0x2028, 0x2029, 0x202F, 0x205F, 0x3000 };
        List<int> result = new List<int>();
        for (int j = 0x0000; j < 0x10000; j++) {
          if (j == 0x180E) { continue; } // Skip 0x180E, current test in a separate file
          if (j == 0xFEFF) { continue; } // Ignore BOM
          if (!(bool)re.Invoke("test", String.Invoke("fromCharCode", j))) {
            result.Add(j);
          }
        }
        That(result, Is.EquivalentTo(whitespaceChars));

        // The Mongolian Vowel Separator (u180e) became a non whitespace character since Unicode 6.3.0
        VerifyMatch(re, "\u180E", true);
      });

      It("should only recognize \\p{name} and \\P{name} escape sequence when [[Unicode]] flag is present", () => {
        VerifyMatch("\\p{Ll}", "a", false);
        VerifyMatch("\\P{Ll}", "A", false);
        VerifyMatch("\\p{Ll}", "u", "a", true);
        VerifyMatch("\\P{Ll}", "u", "A", true);
      });
    }

    [Test]
    public void ControlLetterEscape() {
      // should detect control letters (U+0000 to U+001F) using ControlEscape (\cx)
      foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") {
        VerifyMatch("\\c" + c, String.Invoke("fromCharCode", c % 32), true);
      }
    }

    [Test]
    public void HexEscape() {
      VerifyMatch(@"\x00", "\u0000", new[] { "\u0000" }, 0);
      VerifyMatch(@"\x01", "\u0001", new[] { "\u0001" }, 0);
      VerifyMatch(@"\x0A", "\u000A", new[] { "\u000A" }, 0);
      VerifyMatch(@"\xFF", "\u00FF", new[] { "\u00FF" }, 0);

      // should detect ASCII characters (U+0000 to U+00FF) using HexEscape (\xnn)
      for (int i = 0; i <= 0xFF; i++) {
        VerifyMatch("\\x" + i.ToString("x2"), String.Invoke("fromCharCode", i), true);
        VerifyMatch("\\x" + i.ToString("X2"), String.Invoke("fromCharCode", i), true);
      }
    }

    [Test]
    public void UnicodeEscape() {
      VerifyMatch(@"\u0000", "\u0000", new[] { "\u0000" }, 0);
      VerifyMatch(@"\u0001", "\u0001", new[] { "\u0001" }, 0);
      VerifyMatch(@"\u000A", "\u000A", new[] { "\u000A" }, 0);
      VerifyMatch(@"\u00FF", "\u00FF", new[] { "\u00FF" }, 0);
      VerifyMatch(@"\u0FFF", "\u0FFF", new[] { "\u0FFF" }, 0);
      VerifyMatch(@"\uFFFF", "\uFFFF", new[] { "\uFFFF" }, 0);

      // should detect BMP characters (U+0000 to U+FFFF) using UnicodeEscape (\unnnn)
      for (int i = 0; i <= 0xFFFF; i++) {
        VerifyMatch("\\u" + i.ToString("x4"), String.Invoke("fromCharCode", i), true);
        VerifyMatch("\\u" + i.ToString("X4"), String.Invoke("fromCharCode", i), true);
      }
    }

    [Test]
    public void IdentityEscape() {
      // should treat an incomplete HexEscape or UnicodeEscape as an IdentityEscape
      VerifyMatch(@"\x", "x", true);
      VerifyMatch(@"\xa", "xa", true);

      // should treat an incomplete HexEscape or UnicodeEscape as an IdentityEscape
      VerifyMatch(@"\u", "u", true);
      VerifyMatch(@"\ua", "ua", true);

      It("should detect source characters using \\ :: SourceCharacter but not IdentifierPart", () => {
        foreach (char c in "~`!@#$%^&*()-+={[}]|\\:;'<,>./?\"") {
          VerifyMatch("\\" + c, "" + c, true);
        }
      });

      It("IdentityEscape for Unicode RegExps is restricted to SyntaxCharacter and U+002F (SOLIDUS)", () => {
        // IdentityEscape in AtomEscape
        VerifyMatch(@"\^", "u", "^", true);
        VerifyMatch(@"\$", "u", "$", true);
        VerifyMatch(@"\\", "u", "\\",true);
        VerifyMatch(@"\.", "u", ".", true);
        VerifyMatch(@"\*", "u", "*", true);
        VerifyMatch(@"\+", "u", "+", true);
        VerifyMatch(@"\?", "u", "?", true);
        VerifyMatch(@"\(", "u", "(", true);
        VerifyMatch(@"\)", "u", ")", true);
        VerifyMatch(@"\[", "u", "[", true);
        VerifyMatch(@"\]", "u", "]", true);
        VerifyMatch(@"\{", "u", "{", true);
        VerifyMatch(@"\}", "u", "}", true);
        VerifyMatch(@"\|", "u", "|", true);
        VerifyMatch(@"\/", "u", "/", true);

        // IdentityEscape in ClassEscape
        VerifyMatch(@"[\^]", "u", "^", true);
        VerifyMatch(@"[\$]", "u", "$", true);
        VerifyMatch(@"[\\]", "u", "\\", true);
        VerifyMatch(@"[\.]", "u", ".", true);
        VerifyMatch(@"[\*]", "u", "*", true);
        VerifyMatch(@"[\+]", "u", "+", true);
        VerifyMatch(@"[\?]", "u", "?", true);
        VerifyMatch(@"[\(]", "u", "(", true);
        VerifyMatch(@"[\)]", "u", ")", true);
        VerifyMatch(@"[\[]", "u", "[", true);
        VerifyMatch(@"[\]]", "u", "]", true);
        VerifyMatch(@"[\{]", "u", "{", true);
        VerifyMatch(@"[\}]", "u", "}", true);
        VerifyMatch(@"[\|]", "u", "|", true);
        VerifyMatch(@"[\/]", "u", "/", true);
      });
    }

    [Test]
    public void ControlEscape() {
      VerifyMatch(@"\t", "\u0009", new[] { "\u0009" }, 0);
      VerifyMatch(@"\n", "\u000A", new[] { "\u000A" }, 0);
      VerifyMatch(@"\v", "\u000B", new[] { "\u000B" }, 0);
      VerifyMatch(@"\f", "\u000C", new[] { "\u000C" }, 0);
      VerifyMatch(@"\r", "\u000D", new[] { "\u000D" }, 0);
      VerifyMatch(@"\0", "\u0000", new[] { "\u0000" }, 0);
    }

    [Test]
    public void Wildcard() {
      VerifyMatch(@".+", "....", new[] { "...." }, 0);
      VerifyMatch(@".+", "abcdefghijklmnopqrstuvwxyz", new[] { "abcdefghijklmnopqrstuvwxyz" }, 0);
      VerifyMatch(@".+", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", new[] { "ABCDEFGHIJKLMNOPQRSTUVWXYZ" }, 0);
      VerifyMatch(@".+", "|\\[{]};:\"\',<>.?/", new[] { "|\\[{]};:\"\',<>.?/" }, 0);
      VerifyMatch(@".+", "`1234567890-=~!@#$%^&*()_+", new[] { "`1234567890-=~!@#$%^&*()_+" }, 0);
      VerifyMatch(@".+", "this is a *&^%$# test", new[] { "this is a *&^%$# test" }, 0);
      VerifyMatch(@".+", "line 1\nline 2", new[] { "line 1" }, 0);
      VerifyMatch(@"ab.de", "abcde", new[] { "abcde" }, 0);
      VerifyMatch(@".*a.*", "this is a test", new[] { "this is a test" }, 0);

      It("should not match LF, CR, LS and PS when dotAll is unset", () => {
        VerifyMatch(".", "\n", false);
        VerifyMatch(".", "\r", false);
        VerifyMatch(".", "\u2028", false);
        VerifyMatch(".", "\u2029", false);
      });

      It("should match LF, CR, LS and PS when dotAll is set", () => {
        VerifyMatch("a.b", "s", "a\nb", true);
        VerifyMatch("a.b", "s", "a\rb", true);
        VerifyMatch("a.b", "s", "a\u2028b", true);
        VerifyMatch("a.b", "s", "a\u2029b", true);
      });

      It("should match against a code point when unicode is set", () => {
        VerifyMatch("a.b", "u", "a\ud83d\ude00b", true);
      });
    }

    [Test]
    public void CharacterClass() {
      Throw("[a--z]", "");
      Throw("[{-z]", "");
      Throw(@"[b-ac-e]", "");
      Throw(@"[\\10b-G]", "");
      Throw(@"[\\bd-G]", "");
      Throw(@"[\\Bd-G]", "");
      Throw(@"[\\td-G]", "");
      Throw(@"[\\nd-G]", "");
      Throw(@"[\\vd-G]", "");
      Throw(@"[\\fd-G]", "");
      Throw(@"[\\rd-G]", "");
      Throw(@"[\\c0001d-G]", "");
      Throw(@"[\\x0061d-G]", "");
      Throw(@"[a-dc-b]", "");
      Throw(@"[\\u0061d-G]", "");
      Throw(@"[\\u0061d-G]", "");
      Throw(@"[\\ad-G]", "");
      Throw(@"[c-eb-a]", "");
      Throw(@"[b-G\\d]", "");
      Throw(@"[b-G\\D]", "");
      Throw(@"[b-G\\s]", "");
      Throw(@"[b-G\\S]", "");
      Throw(@"[b-G\\w]", "");
      Throw(@"[b-G\\W]", "");
      Throw(@"[b-G\\0]", "");
      Throw(@"[\\db-G]", "");
      Throw(@"[b-G\\10]", "");
      Throw(@"[d-G\\b]", "");
      Throw(@"[d-G\\B]", "");
      Throw(@"[d-G\\t]", "");
      Throw(@"[d-G\\n]", "");
      Throw(@"[d-G\\v]", "");
      Throw(@"[d-G\\f]", "");
      Throw(@"[d-G\\r]", "");
      Throw(@"[d-G\\c0001]", "");
      Throw(@"[d-G\\x0061]", "");
      Throw(@"[\\Db-G]", "");
      Throw(@"[d-G\\u0061]", "");
      Throw(@"[d-G\\a]", "");
      Throw(@"[\\sb-G]", "");
      Throw(@"[\\Sb-G]", "");
      Throw(@"[\\wb-G]", "");
      Throw(@"[\\Wb-G]", "");
      Throw(@"[\\0b-G]", "");

      VerifyMatch(@"a[b]c", "abc", new[] { "abc" }, 0);
      VerifyMatch(@"a[^b]c", "abc", null, 0);
      VerifyMatch(@"ab[.]?c", "abc", new[] { "abc" }, 0);
      VerifyMatch(@"[1234567].{2}", "abc6defghijkl", new[] { "6de" }, 3);
      VerifyMatch(@"ab[ercst]de", "abcde", new[] { "abcde" }, 0);
      VerifyMatch(@"ab[erst]de", "abcde", null, 0);
      VerifyMatch(@"[\d][\n][^\d]", "line1\nline2", new[] { "1\nl" }, 4);
      VerifyMatch(@"[*&$]{3}", "123*&$abc", new[] { "*&$" }, 3);
      VerifyMatch(@"[]a", "\0a\0a", null, 0);
      VerifyMatch(@"a[]", "\0a\0a", null, 0);
      VerifyMatch(@"[]", "a[b\n[]\tc]d", null, 0);
      VerifyMatch(@"[/]", "/", new[] { "/" }, 0);
      VerifyMatch(@"[//]", "/", new[] { "/" }, 0);

      // \b means the backspace character
      VerifyMatch(@"[^\[\b\]]+", "abcdef", new[] { "abcdef" }, 0);
      VerifyMatch(@"[^\[\b\]]+", "abc\bdef", new[] { "abc" }, 0);
      VerifyMatch(@"c[\b]{3}d", "abc\b\b\bdef", new[] { "c\b\b\bd" }, 2);
      VerifyMatch(@".[\b].", "abc\bdef", new[] { "c\bd" }, 2);

      VerifyMatch(@"[^]", "abc#$%def%&*@ghi", new[] { "a" }, 0);
      VerifyMatch(@"[^a-z]{4}", "abc#$%def%&*@ghi", new[] { "%&*@" }, 9);
      VerifyMatch(@"a[^1-9]c", "abc", new[] { "abc" }, 0);
      VerifyMatch(@"[^\b]+", "easy\bto\u0008ride", new[] { "easy" }, 0);
      VerifyMatch(@"a[^b-z]\s+", "ab an az aY n", new[] { "aY " }, 9);
      VerifyMatch(@"a[^]", "   a\t\n", new[] { "a\t" }, 3);
      VerifyMatch(@"[^]a", "m", "a\naba", new[] { "\na" }, 1);

      // CharacterRange
      VerifyMatch(@"[a-z]+", "ABC def ghi", new[] { "def" }, 4);
      VerifyMatch(@"[a-z]+", "ig", "ABC def ghi", new[] { "ABC" }, 0);
      VerifyMatch(@"[d-h]+", "abcdefghijkl", new[] { "defgh" }, 3);
      VerifyMatch(@"q[ax-zb](?=\s+)", "tqa\t  qy ", new[] { "qa" }, 1);
      VerifyMatch(@"q[ax-zb](?=\s+)", "tqaqy ", new[] { "qy" }, 3);
      VerifyMatch(@"q[ax-zb](?=\s+)", "qYqy ", new[] { "qy" }, 2);
      VerifyMatch(@"[a-z][^1-9][a-z]", "a1b  b2c  c3d  def  f4g", new[] { "def" }, 15);
      VerifyMatch(@"[a-c\d]+", "\n\nabc324234\n", new[] { "abc324234" }, 2);

      It("should handle character range in surrogate correctly", () => {
        VerifyMatch("a[\ud83d\ude00-\ud83e\ude95]b", "u", "a\ud83d\ude01b", true);
        VerifyMatch("a[\ud83d\ude00-\udbff\ude95]b", "u", "a\ud83d\ude01b", true);
      });
    }

    [Test]
    public void Quantifier() {
      Throw("a**", "");
      Throw("a***", "");
      Throw("a++", "");
      Throw("a+++", "");
      Throw("a???", "");
      Throw("a????", "");
      Throw("*a", "");
      Throw("**a", "");
      Throw("+a", "");
      Throw("++a", "");
      Throw("??", "");
      Throw("?a", "");
      Throw("??a", "");
      Throw("x{1}{1,}", "");
      Throw("x{1,2}{1}", "");
      Throw("x{1,}{1}", "");
      Throw("x{0,1}{1,}", "");
      Throw("0{2,1}", "");

      VerifyMatch(@"x{1,2}x{1,}", "xxxxxxx", new[] { "xxxxxxx" }, 0);
      VerifyMatch(@"\d{1,}", "wqe456646dsff", new[] { "456646" }, 3);
      VerifyMatch(@"(123){1,}", "123123", new[] { "123123", "123" }, 0);
      VerifyMatch(@"(123){1,}", "123123", new[] { "123123", "123" }, 0);
      VerifyMatch(@"(123){1,}x\1", "123123x123", new[] { "123123x123", "123" }, 0);
      VerifyMatch(@"\d{2,4}", "0a0\u0031\u003122b", new[] { "0112" }, 2);
      VerifyMatch(@"\d{2,4}", "0a0\u0031\u0031b", new[] { "011" }, 2);
      VerifyMatch(@"\d{2,4}", "the 1984 novel", new[] { "1984" }, 4);
      VerifyMatch(@"\d{2,4}", "the Fahrenheit 451 book", new[] { "451" }, 15);
      VerifyMatch(@"\d{2,4}", "the 20000 Leagues Under the Sea book", new[] { "2000" }, 4);
      VerifyMatch(@"\d{2,4}", "the answer is 42", new[] { "42" }, 14);
      VerifyMatch(@"\d{2,4}", "the 7 movie", null, 0);
      VerifyMatch(@".{0,93}", "weirwerdf", new[] { "weirwerdf" }, 0);
      VerifyMatch(@"bx{0,93}c", "aaabbbbcccddeeeefffff", new[] { "bc" }, 6);
      VerifyMatch(@"b{0,93}c", "aaabbbbcccddeeeefffff", new[] { "bbbbc" }, 3);
      VerifyMatch(@"b{2,3}c", "aaabbbbcccddeeeefffff", new[] { "bbbc" }, 4);
      VerifyMatch(@"b{2,}c", "aaabbbbcccddeeeefffff", new[] { "bbbbc" }, 3);
      VerifyMatch(@"b{8,}c", "aaabbbbcccddeeeefffff", null, 0);
      VerifyMatch(@"b{8}", "aaabbbbcccddeeeefffff", null, 0);
      VerifyMatch(@"b{42,93}", "aaabbbbcccddeeeefffff", null, 0);

      VerifyMatch(@"b{2}c", "aaabbbbcccddeeeefffff", new[] { "bbc" }, 5);
      VerifyMatch(@"\w{3}\d?", "CELL\uFFDDbox127", new[] { "CEL" }, 0);
      VerifyMatch(@"\w{3}\d?", "CE\uFFFFL\uFFDDbox127", new[] { "box1" }, 5);

      VerifyMatch(@"b?b?b?b", "abbbbc", new[] { "bbbb" }, 1);
      VerifyMatch(@"x?ay?bz?c", "abcd", new[] { "abc" }, 0);
      VerifyMatch(@"x?y?z?", "abcd", new[] { "" }, 0);
      VerifyMatch(@"o?pqrst", "pqrstuvw", new[] { "pqrst" }, 0);
      VerifyMatch(@"cdx?e", "abcdef", new[] { "cde" }, 2);
      VerifyMatch(@"cd?e", "abcdef", new[] { "cde" }, 2);
      VerifyMatch(@"java(script)?", "state: java and javascript are vastly different", new[] { "java", undefined }, 7);
      VerifyMatch(@"java(script)?", "state: javascript is extension of ecma script", new[] { "javascript", "script" }, 7);
      VerifyMatch(@"java(script)?", "state: both Java and JavaScript used in web development", null, 0);
      VerifyMatch(@".?.?.?.?.?.?.?", "test", new[] { "test" }, 0);
      VerifyMatch(@"\??\??\??\??\??", "?????", new[] { "?????" }, 0);
      VerifyMatch(@"ab?c?d?x?y?z", "123az789", new[] { "az" }, 3);

      VerifyMatch(@".*", "a1b2c3", new[] { "a1b2c3" }, 0);
      VerifyMatch(@"[xyz]*1", "a0.b2.c3", null, 0);
      VerifyMatch(@"[^""]*", "\"beast\"-nickname", new[] { "" }, 0);
      VerifyMatch(@"[^""]*", "alice said: \"don\'t\"", new[] { "alice said: " }, 0);
      VerifyMatch(@"[^""]*", "before\'i\'start", new[] { "before\'i\'start" }, 0);
      VerifyMatch(@"[^""]*", "alice \u0022sweep\u0022: \"don\'t\"", new[] { "alice " }, 0);
      VerifyMatch(@"[^""]*", "alice \"sweep\": \"don\'t\"", new[] { "alice " }, 0);
      VerifyMatch(@"[""'][^""']*[""']", "alice cries out:\"\"", new[] { "\"\"" }, 16);
      VerifyMatch(@"[""'][^""']*[""']", "alice cries out: \'don\'t\'", new[] { "\'don\'" }, 17);
      VerifyMatch(@"[""'][^""']*[""']", "alice \u0022sweep\u0022: \"don\'t\"", new[] { "\"sweep\"" }, 6);
      VerifyMatch(@"[""'][^""']*[""']", "alice cries out: don\'t", null, 0);
      VerifyMatch(@"bc..[\d]*[\s]*", "abcdef", new[] { "bcde" }, 1);
      VerifyMatch(@"[\d]*[\s]*bc.", "abcdef", new[] { "bcd" }, 1);
      VerifyMatch(@"d*", "abcddddefg", new[] { "" }, 0);
      VerifyMatch(@"cd*", "abcddddefg", new[] { "cdddd" }, 2);
      VerifyMatch(@"cx*d", "abcdefg", new[] { "cd" }, 2);
      VerifyMatch(@"x*y+$", "xxxxxxyyyyyy", new[] { "xxxxxxyyyyyy" }, 0);
      VerifyMatch(@"(x+)(x*)", "xxxxxxx", new[] { "xxxxxxx", "xxxxxxx", "" }, 0);
      VerifyMatch(@"(x*)(x+)", "xxxxxxx", new[] { "xxxxxxx", "xxxxxx", "x" }, 0);
      VerifyMatch(@"(\d*)(\d+)", "1234567890", new[] { "1234567890", "123456789", "0" }, 0);
      VerifyMatch(@"(\d*)\d(\d+)", "1234567890", new[] { "1234567890", "12345678", "0" }, 0);

      VerifyMatch(@"(a*)b\1+", "baaaac", new[] { "b", "" }, 0);
      VerifyMatch(@"(z)((a+)?(b+)?(c))*", "zaacbbbcac", new[] { "zaacbbbcac", "z", "ac", "a", undefined, "c" }, 0);
      VerifyMatch(@"(aa|aabaac|ba|b|c)*", "aabaac", new[] { "aaba", "ba" }, 0);
      VerifyMatch(@"a[a-z]{2,4}?", "abcdefghi", new[] { "abc" }, 0);
      VerifyMatch(@"a[a-z]{2,4}", "abcdefghi", new[] { "abcde" }, 0);

      VerifyMatch(@"d+", "abcdddddefg", new[] { "ddddd" }, 3);
      VerifyMatch(@"d+", "abcdefg", new[] { "d" }, 3);
      VerifyMatch(@"b*b+", "abbbbbbbc", new[] { "bbbbbbb" }, 1);
      VerifyMatch(@"(b+)(b*)", "abbbbbbbc", new[] { "bbbbbbb", "bbbbbbb", "" }, 1);
      VerifyMatch(@"(b+)(b+)(b+)", "abbbbbbbc", new[] { "bbbbbbb", "bbbbb", "b", "b" }, 1);
      VerifyMatch(@"[a-z]+\d+", "__abc123.0", new[] { "abc123" }, 2);
      VerifyMatch(@"[a-z]+\d+", "x 2 ff 55 x2 as1 z12 abc12.0", new[] { "x2" }, 10);
      VerifyMatch(@"[a-z]+(\d+)", "__abc123.0", new[] { "abc123", "123" }, 2);
      VerifyMatch(@"[a-z]+(\d+)", "x 2 ff 55 x2 as1 z12 abc12.0", new[] { "x2", "2" }, 10);
      VerifyMatch(@"\s+java\s+", "\t java object", new[] { "\t java " }, 0);
      VerifyMatch(@"\s+java\s+", "language  java\n", new[] { "  java\n" }, 8);
      VerifyMatch(@"\s+java\s+", "java\n\nobject", null, 0);
      VerifyMatch(@"\s+java\s+", "\t javax package", null, 0);

      It("should apply qualifers on the preceeding code point", () => {
        VerifyMatch("a\ud83d\ude00+b", "u", "a\ud83d\ude00\ud83d\ude00b", true);
        VerifyMatch("a\ud83d\ude00*b", "u", "a\ud83db", false);
        VerifyMatch("a\ud83d\ude00?b", "u", "a\ud83db", false);
        VerifyMatch("a\ud83d\ude00{2}b", "u", "a\ud83d\ude00\ud83d\ude00b", true);
      });
    }

    [Test]
    public void Alternation() {
      VerifyMatch(@"a|ab", "abc", new[] { "a" }, 0);
      VerifyMatch(@"ab|cd|ef", "i", "AEKFCD", new[] { "CD" }, 4);
      VerifyMatch(@"ab|cd|ef", "AEKFCD", null, 0);
      VerifyMatch(@"(?:ab|cd)+|ef", "i", "AEKFCDab", new[] { "CDab" }, 4);
      VerifyMatch(@"(?:ab|cd)+|ef", "i", "AEKFCD", new[] { "CD" }, 4);
      VerifyMatch(@"(?:ab|cd)+|ef", "i", "AEKeFCDab", new[] { "eF" }, 3);
      VerifyMatch(@"\d{3}|[a-z]{4}", "2, 12 and 234 AND of course repeat 12", new[] { "234" }, 10);
      VerifyMatch(@"\d{3}|[a-z]{4}", "2, 12 and of course repeat 12", new[] { "cour" }, 13);
      VerifyMatch(@"\d{3}|[a-z]{4}", "2, 12 and 23 AND 0.00.1", null, 0);
      VerifyMatch(@"((a)|(ab))((c)|(bc))", "abc", new[] { "abc", "a", "a", undefined, "bc", undefined, "bc" }, 0);
      VerifyMatch(@"|()", "", new[] { "", undefined }, 0);
      VerifyMatch(@"()|", "", new[] { "", "" }, 0);
      VerifyMatch(@"(Rob)|(Bob)|(Robert)|(Bobby)", "Hi Bob", new[] { "Bob", undefined, "Bob", undefined, undefined }, 3);
      VerifyMatch(@".+: gr(a|e)y", "color: grey", new[] { "color: grey", "e" }, 0);
      VerifyMatch(@"(.)..|abc", "abc", new[] { "abc", "a" }, 0);
      VerifyMatch(@"xyz|...", "abc", new[] { "abc" }, 0);
      VerifyMatch(@"11111|111", "1111111111111111", new[] { "11111" }, 0);
    }

    [Test]
    public void Disjunction() {
      VerifyMatch(@"a(..(..)..)", "abcdefgh", new[] { "abcdefg", "bcdefg", "de" }, 0);
      VerifyMatch(@"(a(b(c)))(d(e(f)))", "xabcdefg", new[] { "abcdef", "abc", "bc", "c", "def", "ef", "f" }, 1);
      VerifyMatch(@"([\S]+([ \t]+[\S]+)*)[ \t]*=[ \t]*[\S]+", "Course_Creator = Test", new[] { "Course_Creator = Test", "Course_Creator", undefined }, 0);
      VerifyMatch(@"([Jj]ava([Ss]cript)?)\sis\s(fun\w*)", "Developing with Java is fun, try it", new[] { "Java is fun", "Java", undefined, "fun" }, 16);
      VerifyMatch(@"([Jj]ava([Ss]cript)?)\sis\s(fun\w*)", "Learning javaScript is funny, really", new[] { "javaScript is funny", "javaScript", "Script", "funny" }, 9);
      VerifyMatch(@"([Jj]ava([Ss]cript)?)\sis\s(fun\w*)", "Developing with JavaScript is dangerous, do not try it without assistance", null, 0);
      VerifyMatch(@"^(A)?(A.*)$", "A", new[] { "A", undefined, "A" }, 0);
      VerifyMatch(@"^(A)?(A.*)$", "AA", new[] { "AA", "A", "A" }, 0);
      VerifyMatch(@"^(A)?(A.*)$", "AAA", new[] { "AAA", "A", "AA" }, 0);
      VerifyMatch(@"(A)?(A.*)", "zxcasd;fl\\  ^AaaAAaaaf;lrlrzs", new[] { "AaaAAaaaf;lrlrzs", undefined, "AaaAAaaaf;lrlrzs" }, 13);
      VerifyMatch(@"(A)?(A.*)", "zxcasd;fl\\  ^AAaaAAaaaf;lrlrzs", new[] { "AAaaAAaaaf;lrlrzs", "A", "AaaAAaaaf;lrlrzs" }, 13);
      VerifyMatch(@"(A)?(A.*)", "zxcasd;fl\\  ^AAAaaAAaaaf;lrlrzs", new[] { "AAAaaAAaaaf;lrlrzs", "A", "AAaaAAaaaf;lrlrzs" }, 13);
      VerifyMatch(@"(a)?a", "a", new[] { "a", undefined }, 0);
      VerifyMatch(@"a|(b)", "a", new[] { "a", undefined }, 0);
      VerifyMatch(@"(a)?(a)", "a", new[] { "a", undefined, "a" }, 0);
      VerifyMatch(@"^([a-z]+)*[a-z]$", "a", new[] { "a", undefined }, 0);
      VerifyMatch(@"^([a-z]+)*[a-z]$", "ab", new[] { "ab", "a" }, 0);
      VerifyMatch(@"^([a-z]+)*[a-z]$", "abc", new[] { "abc", "ab" }, 0);
      VerifyMatch(@"^(([a-z]+)*[a-z]\.)+[a-z]{2,}$", "www.netscape.com", new[] { "www.netscape.com", "netscape.", "netscap" }, 0);
      VerifyMatch(@"^(([a-z]+)*([a-z])\.)+[a-z]{2,}$", "www.netscape.com", new[] { "www.netscape.com", "netscape.", "netscap", "e" }, 0);
      VerifyMatch(@"(abc)", "abc", new[] { "abc", "abc" }, 0);
      VerifyMatch(@"a(bc)d(ef)g", "abcdefg", new[] { "abcdefg", "bc", "ef" }, 0);
      VerifyMatch(@"(.{3})(.{4})", "abcdefgh", new[] { "abcdefg", "abc", "defg" }, 0);
    }

    [Test]
    public void Assertion() {
      VerifyMatch(@"^\^+", "^^^x", new[] { "^^^" }, 0);
      VerifyMatch(@"^ab", "abcde", new[] { "ab" }, 0);
      VerifyMatch(@"^m", "m", "pairs\nmakes\tdouble\npesos", new[] { "m" }, 6);
      VerifyMatch(@"^[^p]", "m", "pairs\nmakes\tdouble\npesos", new[] { "m" }, 6);
      VerifyMatch(@"^p[b-z]", "m", "pairs\nmakes\tdouble\npesos", new[] { "pe" }, 19);
      VerifyMatch(@"^p[a-z]", "m", "pairs\nmakes\tdouble\npesos", new[] { "pa" }, 0);
      VerifyMatch(@"^\d+", "m", "abc\n123xyz", new[] { "123" }, 4);
      VerifyMatch(@"^xxx", "yyyyy", null, 0);
      VerifyMatch(@"^..^e", "ab\\ncde", null, 0);
      VerifyMatch(@"^m", "pairs\nmakes\tdouble", null, 0);

      VerifyMatch(@"es$", "mg", "pairs\nmakes\tdoubl\u0065s", new[] { "es" }, 17);
      VerifyMatch(@"e$", "m", "pairs\nmakes\tdouble", new[] { "e" }, 17);
      VerifyMatch(@"s$", "m", "pairs\nmakes\tdouble", new[] { "s" }, 4);
      VerifyMatch(@"[^e]$", "mg", "pairs\nmakes\tdouble", new[] { "s" }, 4);
      VerifyMatch(@"s$", "pairs\nmakes\tdouble", null, 0);

      VerifyMatch(@"\w\B", "devils arise\tfor\nrevil", new[] { "d" }, 0);
      VerifyMatch(@"\B\w", "devils arise\tfor\nrevil", new[] { "e" }, 1);
      VerifyMatch(@"\Bo\B", "i", "devils arise\tfOr\nrevil", new[] { "O" }, 14);
      VerifyMatch(@"\B\w\B", "devils arise\tfor\nrevil", new[] { "e" }, 1);
      VerifyMatch(@"\B\w{4}\B", "devil arise\tforzzx\nevils", new[] { "orzz" }, 13);
      VerifyMatch(@"\B[^z]{4}\B", "devil arise\tforzzx\nevils", new[] { "il a" }, 3);
      VerifyMatch(@"[f-z]e\B", "devils arise\tfor\nrevil", new[] { "re" }, 17);
      VerifyMatch(@"\Bevil\B", "devils arise\tfor\nrevil", new[] { "evil" }, 1);

      VerifyMatch(@"\bp", "pilot\nsoviet robot\topenoffice", new[] { "p" }, 0);
      VerifyMatch(@"\bro", "pilot\nsoviet robot\topenoffice", new[] { "ro" }, 13);
      VerifyMatch(@"\bso", "pilot\nsoviet robot\topenoffice", new[] { "so" }, 6);
      VerifyMatch(@"\bop", "pilot\nsoviet robot\topenoffice", new[] { "op" }, 19);
      VerifyMatch(@"\bot", "pilot\nsoviet robot\topenoffice", null, 0);
      VerifyMatch(@"ot\b", "pilot\nsoviet robot\topenoffice", new[] { "ot" }, 3);
      VerifyMatch(@"op\b", "pilot\nsoviet robot\topenoffice", null, 0);
      VerifyMatch(@"so\b", "pilot\nsoviet robot\topenoffice", null, 0);
      VerifyMatch(@"e\b", "pilot\nsoviet robot\topenoffic\u0065", new[] { "e" }, 28);
      VerifyMatch(@"\be", "pilot\nsoviet robot\topenoffic\u0065", null, 0);
      VerifyMatch(@"[^o]t\b", "i", "pilOt\nsoviet robot\topenoffice", new[] { "et" }, 10);
      VerifyMatch(@"[^o]t\b", "pilOt\nsoviet robot\topenoffice", new[] { "Ot" }, 3);
      VerifyMatch(@"\b\w{5}\b", "pilot\nsoviet robot\topenoffice", new[] { "pilot" }, 0);
      VerifyMatch(@"\brobot\b", "pilot\nsoviet robot\topenoffice", new[] { "robot" }, 13);
      VerifyMatch(@"r\b", "pilot\nsoviet robot\topenoffice", null, 0);

      VerifyMatch(@"^.*(:|$)", "Hello: World", new[] { "Hello: World", "" }, 0);
      VerifyMatch(@"^.*?(:|$)", "Hello: World", new[] { "Hello:", ":" }, 0);
      VerifyMatch(@"^.*?", "Hello World", new[] { "" }, 0);
      VerifyMatch(@"^.*?$", "Hello World", new[] { "Hello World" }, 0);
      VerifyMatch(@"\B\B\B\B\B\Bbot\b\b\b\b\b\b\b", "robot wall-e", new[] { "bot" }, 2);
      VerifyMatch(@"^^^^^^^robot$$$$", "robot", new[] { "robot" }, 0);
    }

    [Test]
    public void LookAhead() {
      VerifyMatch(@"(?!a|b)|c", "", new[] { "" }, 0);
      VerifyMatch(@"(?!a|b)|c", "d", new[] { "" }, 0);
      VerifyMatch(@"(?!a|b)|c", "bc", new[] { "" }, 1);
      VerifyMatch(@"(.*?)a(?!(a+)b\2c)\2(.*)", "baaabaac", new[] { "baaabaac", "ba", undefined, "abaac" }, 0);
      VerifyMatch(@"(\.(?!com|org)|\/)", "ah/info", new[] { "/", "/" }, 2);
      VerifyMatch(@"(\.(?!com|org)|\/)", "ah.info", new[] { ".", "." }, 2);
      VerifyMatch(@"(\.(?!com|org)|\/)", "ah.com", null, 0);
      VerifyMatch(@"Java(?!Script)([A-Z]\w*)", "JavaScr oops ipt ", new[] { "JavaScr", "Scr" }, 0);
      VerifyMatch(@"Java(?!Script)([A-Z]\w*)", "using of JavaBeans technology", new[] { "JavaBeans", "Beans" }, 9);
      VerifyMatch(@"Java(?!Script)([A-Z]\w*)", "i'm a JavaScripter ", null, 0);
      VerifyMatch(@"Java(?!Script)([A-Z]\w*)", "using of Java language", null, 0);
      VerifyMatch(@"[Jj]ava([Ss]cript)?(?=\:)", "taste of java: the cookbook ", new[] { "java", undefined }, 9);
      VerifyMatch(@"[Jj]ava([Ss]cript)?(?=\:)", "just Javascript: the way af jedi", new[] { "Javascript", "script" }, 5);
      VerifyMatch(@"[Jj]ava([Ss]cript)?(?=\:)", "rhino is JavaScript engine", null, 0);
      VerifyMatch(@"(?=(a+))a*b\1", "baaabac", new[] { "aba", "a" }, 3);
      VerifyMatch(@"(?=(a+))", "baaabac", new[] { "", "aaa" }, 1);
    }

    [Test]
    public void BackReference() {
      VerifyMatch(@"(aa)bcd\1", "aabcdaabcd", new[] { "aabcdaa", "aa" }, 0);
      VerifyMatch(@"(aa).+\1", "aabcdaabcd", new[] { "aabcdaa", "aa" }, 0);
      VerifyMatch(@"(.{2}).+\1", "aabcdaabcd", new[] { "aabcdaa", "aa" }, 0);
      VerifyMatch(@"\b(\w+) \1\b", "do you listen the the band", new[] { "the the", "the" }, 14);
      VerifyMatch(@"(a*)b\1+", "baaac", new[] { "b", "" }, 0);
      VerifyMatch(@"([xu]\d{2}([A-H]{2})?)\1", "x09x12x01x01u00FFu00FFx04x04x23", new[] { "x01x01", "x01", undefined }, 6);
      VerifyMatch(@"([xu]\d{2}([A-H]{2})?)\1", "x09x12x01x05u00FFu00FFx04x04x23", new[] { "u00FFu00FF", "u00FF", "FF" }, 12);
      VerifyMatch(@"a(.?)b\1c\1d\1", "abcd", new[] { "abcd", "" }, 0);
      VerifyMatch(@"(a(b(c)))(d(e(f)))\2\5", "xabcdefbcefg", new[] { "abcdefbcef", "abc", "bc", "c", "def", "ef", "f" }, 1);
      VerifyMatch(@"(\d{3})(\d{3})\1\2", "123456123456", new[] { "123456123456", "123", "456" }, 0);

      VerifyMatch(@"(A)\1", "AA", new[] { "AA", "A" }, 0);
      VerifyMatch(@"\1(A)", "AA", new[] { "A", "A" }, 0);
      VerifyMatch(@"(A)\1(B)\2", "AABB", new[] { "AABB", "A", "B" }, 0);
      VerifyMatch(@"\1(A)(B)\2", "AABB", new[] { "ABB", "A", "B" }, 1);
      VerifyMatch(@"((((((((((A))))))))))\1\2\3\4\5\6\7\8\9\10", "AAAAAAAAAAA", new[] { "AAAAAAAAAAA", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A" }, 0);
      VerifyMatch(@"((((((((((A))))))))))\10\9\8\7\6\5\4\3\2\1", "AAAAAAAAAAA", new[] { "AAAAAAAAAAA", "A", "A", "A", "A", "A", "A", "A", "A", "A", "A" }, 0);
    }

    [Test]
    public void NamedGroups() {
      // Duplicate named capture groups
      Throw("(?<name>.)(?<name>.)", "");

      It("Named groups in Unicode RegExps have some syntax errors and some compatibility escape fallback behavior", () => {
        VerifyMatch(@"\k<a>(?<=>)a", "k<a>a", true);
        VerifyMatch(@"\k<a>(?<!a)a", "k<a>a", true);
        VerifyMatch(@"\k<a>", "k<a>", true);
        VerifyMatch(@"\k<4>", "k<4>", true);
        VerifyMatch(@"\k<a", "k<a", true);
        VerifyMatch(@"\k", "k", true);
        VerifyMatch(@"(?<a>\a)", "a", true);

        VerifyMatch(@"\k<a>", "xxxk<a>xxx", new[] { "k<a>" }, 3);
        VerifyMatch(@"\k<a", "xxxk<a>xxx", new[] { "k<a" }, 3);

        // A couple of corner cases around '\k' as named back-references vs. identity escapes.
        VerifyMatch(@"\k<a>(?<=>)a", "k<a>a", true);
        VerifyMatch(@"\k<a>(?<!a)a", "k<a>a", true);
        VerifyMatch(@"\k<a>(<a>x)", "k<a><a>x", true);
      });
    }

    [Test, Ignore("Unicode flag is only with limited support")]
    public void AnnexB() {
      It("should throw when B.1.4 is not applied for Unicode RegExp", () => {
        // B.1.4 is not applied for Unicode RegExp - Standalone brackets
        Throw("(", "u");
        Throw(")", "u");
        Throw("[", "u");
        Throw("]", "u");
        Throw("{", "u");
        Throw("}", "u");

        // B.1.4 is not applied for Unicode RegExp - ClassEscape in range expression
        Throw("[\\d-a]", "u");
        Throw("[\\D-a]", "u");
        Throw("[\\s-a]", "u");
        Throw("[\\S-a]", "u");
        Throw("[\\w-a]", "u");
        Throw("[\\W-a]", "u");

        // Trailing CharacterClassEscape.
        Throw("[a-\\d]", "u");
        Throw("[a-\\D]", "u");
        Throw("[a-\\s]", "u");
        Throw("[a-\\S]", "u");
        Throw("[a-\\w]", "u");
        Throw("[a-\\W]", "u");

        // Leading and trailing CharacterClassEscape.
        Throw("[\\d-\\d]", "u");
        Throw("[\\D-\\D]", "u");
        Throw("[\\s-\\s]", "u");
        Throw("[\\S-\\S]", "u");
        Throw("[\\w-\\w]", "u");
        Throw("[\\W-\\W]", "u");

        // B.1.4 is not applied for Unicode RegExp - Quantifier without matching Atom
        Throw("*", "u");
        Throw("+", "u");
        Throw("?", "u");
        Throw("{1}", "u");
        Throw("{1,}", "u");
        Throw("{1,2}", "u");

        // B.1.4 is not applied for Unicode RegExp - Incomplete Unicode escape sequences
        // Incomplete RegExpUnicodeEscapeSequence in AtomEscape not parsed as IdentityEscape
        Throw("\\u1", "u");
        Throw("\\u12", "u");
        Throw("\\u123", "u");
        Throw("\\u{", "u");
        Throw("\\u{}", "u");
        Throw("\\u{1", "u");
        Throw("\\u{12", "u");
        Throw("\\u{123", "u");

        // Incomplete RegExpUnicodeEscapeSequence in ClassEscape not parsed as IdentityEscape
        Throw("[\\u]", "u");
        Throw("[\\u1]", "u");
        Throw("[\\u12]", "u");
        Throw("[\\u123]", "u");
        Throw("[\\u{]", "u");
        Throw("[\\u{}]", "u");
        Throw("[\\u{1]", "u");
        Throw("[\\u{12]", "u");
        Throw("[\\u{123]", "u");

        // B.1.4 is not applied for Unicode RegExp - Incomplete hexadecimal escape sequences
        Throw("\\x", "u");
        Throw("\\x1", "u");
        Throw("[\\x]", "u");
        Throw("[\\x1]", "u");

        // B.1.4 is not applied for Unicode RegExp - Incomplete quantifiers
        Throw("a{", "u");
        Throw("a{1", "u");
        Throw("a{1,", "u");
        Throw("a{1,2", "u");
        Throw("{", "u");
        Throw("{1", "u");
        Throw("{1,", "u");
        Throw("{1,2", "u");

        // Positive lookahead with quantifier.
        Throw("(?=.)*", "u");
        Throw("(?=.)+", "u");
        Throw("(?=.)?", "u");
        Throw("(?=.){1}", "u");
        Throw("(?=.){1,}", "u");
        Throw("(?=.){1,2}", "u");

        // Positive lookahead with reluctant quantifier.
        Throw("(?=.)*?", "u");
        Throw("(?=.)+?", "u");
        Throw("(?=.)??", "u");
        Throw("(?=.){1}?", "u");
        Throw("(?=.){1,}?", "u");
        Throw("(?=.){1,2}?", "u");

        // Negative lookahead with quantifier.
        Throw("(?!.)*", "u");
        Throw("(?!.)+", "u");
        Throw("(?!.)?", "u");
        Throw("(?!.){1}", "u");
        Throw("(?!.){1,}", "u");
        Throw("(?!.){1,2}", "u");

        // Negative lookahead with reluctant quantifier.
        Throw("(?!.)*?", "u");
        Throw("(?!.)+?", "u");
        Throw("(?!.)??", "u");
        Throw("(?!.){1}?", "u");
        Throw("(?!.){1,}?", "u");
        Throw("(?!.){1,2}?", "u");

        Throw("\\1", "u");
        Throw("\\2", "u");
        Throw("\\3", "u");
        Throw("\\4", "u");
        Throw("\\5", "u");
        Throw("\\6", "u");
        Throw("\\7", "u");
        Throw("\\8", "u");
        Throw("\\9", "u");

        // DecimalEscape without leading 0 in ClassEscape.
        Throw("[\\1]", "u");
        Throw("[\\2]", "u");
        Throw("[\\3]", "u");
        Throw("[\\4]", "u");
        Throw("[\\5]", "u");
        Throw("[\\6]", "u");
        Throw("[\\7]", "u");
        Throw("[\\8]", "u");
        Throw("[\\9]", "u");

        // DecimalEscape with leading 0 in AtomEscape.
        Throw("\\00", "u");
        Throw("\\01", "u");
        Throw("\\02", "u");
        Throw("\\03", "u");
        Throw("\\04", "u");
        Throw("\\05", "u");
        Throw("\\06", "u");
        Throw("\\07", "u");
        Throw("\\08", "u");
        Throw("\\09", "u");

        // DecimalEscape with leading 0 in ClassEscape.
        Throw("[\\00]", "u");
        Throw("[\\01]", "u");
        Throw("[\\02]", "u");
        Throw("[\\03]", "u");
        Throw("[\\04]", "u");
        Throw("[\\05]", "u");
        Throw("[\\06]", "u");
        Throw("[\\07]", "u");
        Throw("[\\08]", "u");
        Throw("[\\09]", "u");

        // B.1.4 is not applied for Unicode RegExp - Invalid control escape sequences
        Throw("[\\c]", "u");
        for (int cu = 0x00; cu <= 0x7f; ++cu) {
          EcmaValue s = String.Invoke("fromCharCode", cu);
          if (!(("A" <= s && s <= "Z") || ("a" <= s && s <= "z"))) {
            // "c ControlLetter" sequence in AtomEscape.
            Throw((string)("\\c" + s), "u");
            // "c ControlLetter" sequence in ClassEscape.
            Throw((string)("[\\c" + s + "]"), "u");
          }
        }

        // B.1.4 is not applied for Unicode RegExp - Identity escape with basic latin letters
        string isValidAlphaEscapeInAtom = "bBfnrtvdDsSwW";
        string isValidAlphaEscapeInClass = "bfnrtvdDsSwW";
        foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") {
          if (!isValidAlphaEscapeInAtom.Contains(c)) {
            // IdentityEscape in AtomEscape
            Throw("\\" + c, "u");
          }
          if (!isValidAlphaEscapeInClass.Contains(c)) {
            // IdentityEscape in ClassEscape
            Throw("[\\" + c + "]", "u");
          }
        }

        // B.1.4 is not applied for Unicode RegExp - Identity escape with basic latin characters
        string isSyntaxCharacter = "^$\\.*+?()[]{}|";
        string isAlphaDigit = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (char c = (char)0x00; c <= 0x7f; ++c) {
          if (!isAlphaDigit.Contains(c) && !isSyntaxCharacter.Contains(c) && c != '/') {
            // "c ControlLetter" sequence in AtomEscape.
            Throw("\\" + c, "u");
            // "c ControlLetter" sequence in ClassEscape.
            Throw("[\\" + c + "]", "u");
          }
        }
      });
    }

    #region Helper
    public static void Throw(string pattern, string flags) {
      That(() => RegExp.Construct(pattern, flags), Throws.SyntaxError, pattern);
    }

    public static void VerifyMatch(EcmaValue pattern, EcmaValue input, bool success) {
      That(RegExp.Construct(pattern).Invoke("test", input), Is.EqualTo(success), FormatMessage((pattern, new[] { input }), null));
    }

    public static void VerifyMatch(EcmaValue pattern, EcmaValue flags, EcmaValue input, bool success) {
      That(RegExp.Construct(pattern, flags).Invoke("test", input), Is.EqualTo(success), FormatMessage((pattern, new[] { input }), null));
    }

    public static void VerifyMatch(EcmaValue pattern, EcmaValue input, object[] expected, int index) {
      VerifyMatch(pattern, "", input, expected, index);
    }

    public static void VerifyMatch(EcmaValue pattern, EcmaValue flags, EcmaValue input, object[] expected, int index) {
      EcmaValue re = default;
      That(() => re = RegExp.Construct(pattern, flags), Throws.Nothing, FormatMessage((Undefined, new[] { pattern, flags }), null));

      RuntimeObject fn = TestContext.CurrentContext.Test.Arguments.FirstOrDefault() as RuntimeObject;
      if (fn == null || fn.IsIntrinsicFunction(WellKnownObject.RegExpPrototype, "test")) {
        fn = re.ToObject().GetMethod("exec");
      }
      EcmaValue actual = fn.Call(re, input);
      if (expected != null) {
        That(actual, Is.EquivalentTo(expected), "RegExp={0}, Input={1}", re, input);
        That(actual["index"], Is.EqualTo(index), "RegExp={0}, Input={1}", re, input);
      } else {
        That(actual, Is.EqualTo(Null), "RegExp={0}, Input={1}", re, input);
      }
    }

    public static void VerifyMatch(EcmaValue pattern, EcmaValue flags, EcmaValue input, params (object[], int)[] expected) {
      EcmaValue re = default;
      That(() => re = RegExp.Construct(pattern, flags), Throws.Nothing, FormatMessage((Undefined, new[] { pattern, flags }), null));

      EcmaValue actual;
      int i = 0;
      while ((actual = re.Invoke("exec", input)) != Null) {
        That(i, Is.LessThan(expected.Length));
        That(actual, Is.EquivalentTo(expected[i].Item1), "RegExp={0}, Input={1}", re, input);
        That(actual["index"], Is.EqualTo(expected[i].Item2), "RegExp={0}, Input={1}", re, input);
        i++;
      }
      That(i, Is.EqualTo(expected.Length));
    }
    #endregion
  }
}
