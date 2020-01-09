using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class StringPrototype : TestBase {
    [Test]
    public void Properties() {
      That(String.Prototype, Has.OwnProperty("constructor", String, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(String.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(String.Prototype), Is.EqualTo("[object String]"), "String prototype object: its [[Class]] must be 'String'");
    }

    [Test, RuntimeFunctionInjection]
    public void CharAt(RuntimeFunction charAt) {
      IsUnconstructableFunctionWLength(charAt, "charAt", 1);
      IsAbruptedFromToPrimitive(charAt);
      IsAbruptedFromToPrimitive(charAt.Bind(""));
      IsAbruptedFromToObject(charAt);

      It("should return the n-th character or an empty string when n is < 0 or >= length of string", () => {
        Case("abcd", "a");
        Case(("abcd", Undefined), "a");
        Case(("abcd", 0), "a");
        Case(("abcd", -0d), "a");
        Case(("abcd", -1), "");
        Case(("abcd", 4), "");
        Case(String.Construct("abcd"), "a");
        Case((String.Construct("abcd"), 0), "a");
        Case((String.Construct("abcd"), -1), "");
        Case((String.Construct("abcd"), 4), "");
      });

      It("should coerce \"pos\" value into number", () => {
        Case(("abcd", "   +00200.0000E-0002   "), "c");
        Case(("abcd", Null), "a");
        Case(("abcd", Undefined), "a");
        Case(("abcd", NaN), "a");
        Case(("abcd", "x"), "a");
        Case(("abcd", false), "a");
        Case(("abcd", true), "b");
        Case(("abcd", CreateObject(toString: () => 1)), "b");
      });

      It("should round the provided \"pos\" number", () => {
        Case(("abc", -0.99999), "a", "-0.99999");
        Case(("abc", -0.00001), "a", "-0.00001");
        Case(("abc", 0.00001), "a", "0.00001");
        Case(("abc", 0.99999), "a", "0.99999");
        Case(("abc", 1.00001), "b", "1.00001");
        Case(("abc", 1.99999), "b", "1.99999");
      });

      It("should coerce this value to a string", () => {
        Case((true, 0), "t");
        Case((false, 0), "f");
        Case((Boolean.Construct(true), 0), "t");
        Case((Boolean.Construct(false), 0), "f");
        Case((Object.Construct(42), 0), "4");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void CharCodeAt(RuntimeFunction charCodeAt) {
      IsUnconstructableFunctionWLength(charCodeAt, "charCodeAt", 1);
      IsAbruptedFromToPrimitive(charCodeAt);
      IsAbruptedFromToPrimitive(charCodeAt.Bind(""));
      IsAbruptedFromToObject(charCodeAt);

      It("should return the n-th character or an empty string when n is < 0 or >= length of string", () => {
        Case("abcd", (int)'a');
        Case(("abcd", Undefined), (int)'a');
        Case(("abcd", 0), (int)'a');
        Case(("abcd", -0d), (int)'a');
        Case(("abcd", -1), NaN);
        Case(("abcd", 4), NaN);
        Case(String.Construct("abcd"), (int)'a');
        Case((String.Construct("abcd"), 0), (int)'a');
        Case((String.Construct("abcd"), -1), NaN);
        Case((String.Construct("abcd"), 4), NaN);
      });

      It("should coerce \"pos\" value into number", () => {
        Case(("abcd", "   +00200.0000E-0002   "), (int)'c');
        Case(("abcd", Null), (int)'a');
        Case(("abcd", Undefined), (int)'a');
        Case(("abcd", NaN), (int)'a');
        Case(("abcd", "x"), (int)'a');
        Case(("abcd", false), (int)'a');
        Case(("abcd", true), (int)'b');
        Case(("abcd", CreateObject(toString: () => 1)), (int)'b');
      });

      It("should round the provided \"pos\" number", () => {
        Case(("abc", -0.99999), (int)'a', "-0.99999");
        Case(("abc", -0.00001), (int)'a', "-0.00001");
        Case(("abc", 0.00001), (int)'a', "0.00001");
        Case(("abc", 0.99999), (int)'a', "0.99999");
        Case(("abc", 1.00001), (int)'b', "1.00001");
        Case(("abc", 1.99999), (int)'b', "1.99999");
      });

      It("should coerce this value to a string", () => {
        Case((true, 0), (int)'t');
        Case((false, 0), (int)'f');
        Case((Boolean.Construct(true), 0), (int)'t');
        Case((Boolean.Construct(false), 0), (int)'f');
        Case((Object.Construct(42), 0), (int)'4');
      });
    }

    [Test, RuntimeFunctionInjection]
    public void CodePointAt(RuntimeFunction codePointAt) {
      IsUnconstructableFunctionWLength(codePointAt, "codePointAt", 1);
      IsAbruptedFromToPrimitive(codePointAt);
      IsAbruptedFromToPrimitive(codePointAt.Bind(""));
      IsAbruptedFromToObject(codePointAt);

      It("should return value on coerced values on ToInteger(position)", () => {
        Case(("\uD800\uDC00", ""), 65536);
        Case(("\uD800\uDC00", "0"), 65536);
        Case(("\uD800\uDC00", NaN), 65536);
        Case(("\uD800\uDC00", false), 65536);
        Case(("\uD800\uDC00", Null), 65536);
        Case(("\uD800\uDC00", Undefined), 65536);
        Case(("\uD800\uDC00", EcmaArray.Of()), 65536);
        Case(("\uD800\uDC00", "1"), 56320);
        Case(("\uD800\uDC00", true), 56320);
        Case(("\uD800\uDC00", EcmaArray.Of(1)), 56320);
      });

      It("should return code point of LeadSurrogate if not followed by a valid TrailSurrogate", () => {
        Case(("\uD800\uDBFF", 0), 0xD800);
        Case(("\uD800\uE000", 0), 0xD800);
        Case(("\uDAAA\uDBFF", 0), 0xDAAA);
        Case(("\uDAAA\uE000", 0), 0xDAAA);
        Case(("\uDBFF\uDBFF", 0), 0xDBFF);
        Case(("\uDBFF\uE000", 0), 0xDBFF);
        Case(("\uD800\u0000", 0), 0xD800);
        Case(("\uD800\uFFFF", 0), 0xD800);
        Case(("\uDAAA\u0000", 0), 0xDAAA);
        Case(("\uDAAA\uFFFF", 0), 0xDAAA);
        Case(("\uDBFF\uDBFF", 0), 0xDBFF);
        Case(("\uDBFF\uFFFF", 0), 0xDBFF);
      });

      It("should return single code unit value of the element at index position", () => {
        Case(("abc", 0), 97);
        Case(("abc", 1), 98);
        Case(("abc", 2), 99);
        Case(("\uAAAA\uBBBB", 0), 0xAAAA);
        Case(("\uD7FF\uAAAA", 0), 0xD7FF);
        Case(("\uDC00\uAAAA", 0), 0xDC00);
        Case(("\uAAAA\uBBBB", 0), 0xAAAA);
        Case(("123\uD800", 3), 0xD800);
        Case(("123\uDAAA", 3), 0xDAAA);
        Case(("123\uDBFF", 3), 0xDBFF);
      });

      It("should convert two code units, lead and trail, that form a UTF-16 surrogate pair to a code point ", () => {
        Case(("\uD800\uDC00", 0), 65536, "U+10000");
        Case(("\uD800\uDDD0", 0), 66000, "U+101D0");
        Case(("\uD800\uDFFF", 0), 66559, "U+103FF");
        Case(("\uDAAA\uDC00", 0), 763904, "U+BA800");
        Case(("\uDAAA\uDDD0", 0), 764368, "U+BA9D0");
        Case(("\uDAAA\uDFFF", 0), 764927, "U+BABFF");
        Case(("\uDBFF\uDC00", 0), 1113088, "U+10FC00");
        Case(("\uDBFF\uDDD0", 0), 1113552, "U+10FDD0");
        Case(("\uDBFF\uDFFF", 0), 1114111, "U+10FFFF");
      });

      It("should return undefined pos >= size or < 0", () => {
        Case(("abc", 3), Undefined);
        Case(("abc", 4), Undefined);
        Case(("abc", Infinity), Undefined);
        Case(("abc", -4), Undefined);
        Case(("abc", -Infinity), Undefined);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Concat(RuntimeFunction concat) {
      IsUnconstructableFunctionWLength(concat, "concat", 1);
      IsAbruptedFromToPrimitive(concat);
      IsAbruptedFromToPrimitive(concat.Bind(""));
      IsAbruptedFromToObject(concat);

      It("should accept arbitrary many arguments", () => {
        Case(("0", new EcmaValue[] {
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
          0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF }),
          "001234567891011121314150123456789101112131415012345678910111213141501234567891011121314150123456789101112131415012345678910111213141501234567891011121314150123456789101112131415");
      });

      It("should convert this value or all arguments to string values", () => {
        Case(("lego", Null), "legonull");
        Case(("lego", Undefined), "legoundefined");
        Case((Object.Construct(42), false, true), "42falsetrue");

        var obj1 = CreateObject(toString: () => "\u0041", valueOf: () => "_\u0041_");
        var obj2 = CreateObject(toString: () => true);
        var obj3 = CreateObject(toString: () => 42);
        Case(("lego", obj1, obj2, obj3, Undefined), "legoAtrue42undefined");
      });

      It("should not change string value of the this instance", () => {
        EcmaValue strObj = String.Construct("one");
        Case((strObj, "two"), "onetwo");
        That(strObj.Equals("one", EcmaValueComparison.Abstract));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void EndsWith(RuntimeFunction endsWith) {
      IsUnconstructableFunctionWLength(endsWith, "endsWith", 1);
      IsAbruptedFromToPrimitive(endsWith);
      IsAbruptedFromToPrimitive(endsWith.Bind(""));
      IsAbruptedFromToPrimitive(endsWith.Bind("", ""));
      IsAbruptedFromToObject(endsWith);

      It("should return based on coerced values of endPosition", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "", NaN), true, "NaN coerced to 0");
        Case((str, "", Null), true, "null coerced to 0");
        Case((str, "", false), true, "false coerced to 0");
        Case((str, "", ""), true, "\"\" coerced to 0");
        Case((str, "", "0"), true, "\"0\" coerced to 0");
        Case((str, "", Undefined), true, "undefined coerced to 0");
        Case((str, "The future", 10.4), true, "10.4 coerced to 10");
        Case((str, "T", true), true, "true coerced to 1");
        Case((str, "The future", "10"), true, "\"10\" coerced to 10");
      });

      It("should return false if search start is less than 0", () => {
        Case(("web", "w", 0), false);
        Case(("Bob", "  Bob"), false);
      });

      It("should return true if searchString.length == 0", () => {
        EcmaValue str = "The future is cool!";
        Case((str, ""), true);
        Case((str, "", str["length"]), true);
        Case((str, "", Infinity), true);
        Case((str, "", -1), true);
        Case((str, "", -Infinity), true);
      });

      It("should return true if searchString appears as a substring of the given string with a given position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", 10), true);
        Case((str, "future", 10), true);
        Case((str, " is cool!", str["length"]), true);
      });

      It("should return true if searchString appears as a substring of the given string", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "cool!"), true);
        Case((str, "!"), true);
        Case((str, str), true);
      });

      It("should throw a TypeError if searchString is a RegExp", () => {
        Case(("", RegExp.Construct(".")), Throws.TypeError);
      });

      It("should return false if searchString is not found", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "is cool!", str["length"] - 1), false);
        Case((str, "!", 1), false);
        Case((str, "is Flash!"), false);
        Case((str, "IS COOL!"), false);
        Case((str, "The future"), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Includes(RuntimeFunction includes) {
      IsUnconstructableFunctionWLength(includes, "includes", 1);
      IsAbruptedFromToPrimitive(includes);
      IsAbruptedFromToPrimitive(includes.Bind(""));
      IsAbruptedFromToPrimitive(includes.Bind("", ""));
      IsAbruptedFromToObject(includes);

      It("should return based on coerced values of position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "", NaN), true, "NaN coerced to 0");
        Case((str, "", Null), true, "null coerced to 0");
        Case((str, "", false), true, "false coerced to 0");
        Case((str, "", ""), true, "\"\" coerced to 0");
        Case((str, "", "0"), true, "\"0\" coerced to 0");
        Case((str, "", Undefined), true, "undefined coerced to 0");
        Case((str, "The future", 0.4), true, "10.4 coerced to 10");
        Case((str, "The future", true), false, "true coerced to 1");
        Case((str, "The future", "1"), false, "\"10\" coerced to 10");
        Case((str, "The future", 1.4), false, "\"10\" coerced to 10");
      });

      It("should Returns false if position is >= this.length and searchString.length > 0", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "!", str["length"]), false);
        Case((str, "!", str["length"] + 1), false);
        Case((str, "!", 100), false);
        Case((str, "!", Infinity), false);
      });

      It("should Returns true if searchString.length == 0", () => {
        EcmaValue str = "The future is cool!";
        Case((str, ""), true);
        Case((str, "", str["length"]), true);
        Case((str, "", Infinity), true);
      });

      It("should Returns true if searchString appears as a substring of the given string with a given position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", 0), true);
        Case((str, " is ", 1), true);
        Case((str, "cool!", 10), true);
      });

      It("should Returns true if searchString appears as a substring of the given string", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future"), true);
        Case((str, "is cool!"), true);
        Case((str, str), true);
      });

      It("should throw a TypeError if searchString is a RegExp", () => {
        Case(("", RegExp.Construct(".")), Throws.TypeError);
      });

      It("should Returns false if searchString is not found with a given position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", 1), false);
        Case((str, str, 1), false);
      });

      It("should Returns false if searchString is not found", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "Flash"), false);
        Case((str, "FUTURE"), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IndexOf(RuntimeFunction indexOf) {
      IsUnconstructableFunctionWLength(indexOf, "indexOf", 1);
      IsAbruptedFromToPrimitive(indexOf);
      IsAbruptedFromToPrimitive(indexOf.Bind(""));
      IsAbruptedFromToPrimitive(indexOf.Bind("", ""));
      IsAbruptedFromToObject(indexOf);

      It("should return based on coerced values of position", () => {
        Case(("aaaa", "aa", 0), 0);
        Case(("aaaa", "aa", 1), 1);
        Case(("aaaa", "aa", -0.9), 0, "ToInteger: truncate towards 0");
        Case(("aaaa", "aa", 0.9), 0, "ToInteger: truncate towards 0");
        Case(("aaaa", "aa", 1.9), 1, "ToInteger: truncate towards 0");
        Case(("aaaa", "aa", NaN), 0, "ToInteger: NaN => 0");
        Case(("aaaa", "aa", Infinity), -1);
        Case(("aaaa", "aa", Undefined), 0, "ToInteger: undefined => NaN => 0");
        Case(("aaaa", "aa", Null), 0, "ToInteger: null => 0");
        Case(("aaaa", "aa", false), 0, "ToInteger: false => 0");
        Case(("aaaa", "aa", true), 1, "ToInteger: true => 1");
        Case(("aaaa", "aa", "0"), 0, "ToInteger: parse Number");
        Case(("aaaa", "aa", "1.9"), 1, "ToInteger: parse Number => 1.9 => 1");
        Case(("aaaa", "aa", "Infinity"), -1, "ToInteger: parse Number");
        Case(("aaaa", "aa", ""), 0, "ToInteger: unparseable string => NaN => 0");
        Case(("aaaa", "aa", "foo"), 0, "ToInteger: unparseable string => NaN => 0");
        Case(("aaaa", "aa", "true"), 0, "ToInteger: unparseable string => NaN => 0");
        Case(("aaaa", "aa", 2), 2);
        Case(("aaaa", "aa", "2"), 2, "ToInteger: parse Number");
        Case(("aaaa", "aa", 2.9), 2, "ToInteger: truncate towards 0");
        Case(("aaaa", "aa", "2.9"), 2, "ToInteger: parse Number => truncate towards 0");
        Case(("aaaa", "aa", EcmaArray.Of(0)), 0);
        Case(("aaaa", "aa", EcmaArray.Of("1")), 1);
        Case(("aaaa", "aa", new EcmaObject()), 0);
        Case(("aaaa", "aa", EcmaArray.Of()), 0);
      });

      It("should return the position of searchString", () => {
        Case(("foo", ""), 0);
        Case(("__foo__", "foo"), 2);
        Case(("__undefined__", Undefined), 2);
        Case(("__null__", Null), 2);
        Case(("__true__", true), 2);
        Case(("__false__", false), 2);
        Case(("__0__", 0), 2, "ToString: Number to String");
        Case(("__0__", -0), 2);
        Case(("__Infinity__", Infinity), 2);
        Case(("__-Infinity__", -Infinity), 2);
        Case(("__NaN__", NaN), 2);
        Case(("__123.456__", 123.456), 2, "ToString: Number to String");
        Case(("__-123.456__", -123.456), 2, "ToString: Number to String");
        Case(("foo", EcmaArray.Of()), 0, "ToString: .toString()");
        Case(("__foo,bar__", EcmaArray.Of("foo", "bar")), 2, "ToString: .toString()");
        Case(("__[object Object]__", new EcmaObject()), 2, "ToString: .toString()");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Iterator(RuntimeFunction iterator) {
      IsUnconstructableFunctionWLength(iterator, "[Symbol.iterator]", 0);
      IsAbruptedFromToPrimitive(iterator);
      IsAbruptedFromToObject(iterator);
    }

    [Test, RuntimeFunctionInjection]
    public void LastIndexOf(RuntimeFunction lastIndexOf) {
      IsUnconstructableFunctionWLength(lastIndexOf, "lastIndexOf", 1);
      IsAbruptedFromToPrimitive(lastIndexOf);
      IsAbruptedFromToPrimitive(lastIndexOf.Bind(""));
      IsAbruptedFromToPrimitive(lastIndexOf.Bind("", ""));
      IsAbruptedFromToObject(lastIndexOf);

      It("should return based on coerced values of position", () => {
        Case(("gnullunazzgnull", Null), 11);
        Case(("undefined", Undefined), 0);
      });

      It("should return the position of searchString", () => {
        Case(("canal", "a"), 3);
        Case(("canal", "a", 2), 1);
        Case(("canal", "a", 0), -1);
        Case(("canal", "x"), -1);
        Case(("canal", "c", -5), 0);
        Case(("canal", "c", 0), 0);
        Case(("canal", ""), 5);
        Case(("canal", "", 2), 2);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void LocaleCompare(RuntimeFunction localeCompare) {
      IsUnconstructableFunctionWLength(localeCompare, "localeCompare", 1);
      IsAbruptedFromToPrimitive(localeCompare);
      IsAbruptedFromToPrimitive(localeCompare.Bind(""));
      IsAbruptedFromToObject(localeCompare);

      It("should compare base on coerced values of that", () => {
        Case(("0", 0), 0);
        Case(("true", true), 0);
        Case(("false", false), 0);
        Case(("null", Null), 0);
        Case(("undefined", Undefined), 0);
      });

      It("should return 0 when comparing Strings that are considered canonically equivalent by the Unicode standard", () => {
        // examples from Unicode 5.0, section 3.7, definition D70
        Case(("o\u0308", "ö"), 0);

        // examples from Unicode 5.0, chapter 3.11
        Case(("ä\u0323", "a\u0323\u0308"), 0);
        Case(("a\u0308\u0323", "a\u0323\u0308"), 0);
        Case(("ạ\u0308", "a\u0323\u0308"), 0);
        Case(("ä\u0306", "a\u0308\u0306"), 0);
        Case(("ă\u0308", "a\u0306\u0308"), 0);

        // examples from Unicode 5.0, chapter 3.12
        Case(("\u1111\u1171\u11B6", "퓛"), 0);

        // examples from UTS 10, Unicode Collation Algorithm
        Case(("Å", "Å"), 0);
        Case(("Å", "A\u030A"), 0);
        Case(("x\u031B\u0323", "x\u0323\u031B"), 0);
        Case(("ự", "ụ\u031B"), 0);
        Case(("ự", "u\u031B\u0323"), 0);
        Case(("ự", "ư\u0323"), 0);
        Case(("ự", "u\u0323\u031B"), 0);

        // examples from UAX 15, Unicode Normalization Forms
        Case(("Ç", "C\u0327"), 0);
        Case(("q\u0307\u0323", "q\u0323\u0307"), 0);
        Case(("가", "\u1100\u1161"), 0);
        Case(("Å", "A\u030A"), 0);
        Case(("Ω", "Ω"), 0);
        Case(("Å", "A\u030A"), 0);
        Case(("ô", "o\u0302"), 0);
        Case(("ṩ", "s\u0323\u0307"), 0);
        Case(("ḋ\u0323", "d\u0323\u0307"), 0);
        Case(("ḋ\u0323", "ḍ\u0307"), 0);
        Case(("q\u0307\u0323", "q\u0323\u0307"), 0);

        // examples involving supplementary characters from UCD NormalizationTest.txt
        Case(("\uD834\uDD5E", "\uD834\uDD57\uD834\uDD65"), 0);
        Case(("\uD87E\uDC2B", "北"), 0);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Match(RuntimeFunction match) {
      IsUnconstructableFunctionWLength(match, "match", 1);
      IsAbruptedFromToPrimitive(match);
      IsAbruptedFromToPrimitive(match.Bind(""));
      IsAbruptedFromToObject(match);

      It("should invoke @@match property of user-supplied objects", () => {
        EcmaValue returnVal = new EcmaObject();
        EcmaValue separator = new EcmaObject();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        int callCount = 0;
        separator[WellKnownSymbol.Match] = RuntimeFunction.Create(() => {
          callCount += 1;
          args = Arguments;
          thisValue = This;
          return returnVal;
        });
        Case(("", separator), returnVal);
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.EqualTo(separator));
        That(args, Is.EquivalentTo(new[] { "" }));
      });

      It("should returns array as specified", () => {
        Case(("343443444", RegExp.Construct("34", "g")), new[] { "34", "34", "34" });
        Case(("123456abcde7890", RegExp.Construct("\\d{1}", "g")), new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" });
        Case(("123456abcde7890", RegExp.Construct("\\d{2}", "g")), new[] { "12", "34", "56", "78", "90" });
        Case(("123456abcde7890", RegExp.Construct("\\D{2}", "g")), new[] { "ab", "cd" });

        EcmaValue result = match.Call("Boston, Mass. 02134", RegExp.Construct(@"([\d]{5})([-\ ]?[\d]{4})?$"));
        That(result, Is.EquivalentTo(new object[] { "02134", "02134", Undefined }));
        That(result["index"], Is.EqualTo(14));
        That(result["input"], Is.EqualTo("Boston, Mass. 02134"));
      });

      It("should convert searcher to a RegExp object if a string is given", () => {
        Case(("a.b.c", "."), new[] { "a" });
        Case(("a.b.c", Undefined), new[] { "" });
        Case(("a.b.c", Null), Null);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void MatchAll(RuntimeFunction matchAll) {
      IsUnconstructableFunctionWLength(matchAll, "matchAll", 1);
      IsAbruptedFromToPrimitive(matchAll);
      IsAbruptedFromToPrimitive(matchAll.Bind(""));
      IsAbruptedFromToObject(matchAll);

      It("should invoke @@matchAll property of user-supplied objects", () => {
        EcmaValue returnVal = new EcmaObject();
        EcmaValue separator = new EcmaObject();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        int callCount = 0;
        separator[WellKnownSymbol.MatchAll] = RuntimeFunction.Create(() => {
          callCount += 1;
          args = Arguments;
          thisValue = This;
          return returnVal;
        });
        Case(("", separator), returnVal);
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.EqualTo(separator));
        That(args, Is.EquivalentTo(new[] { "" }));
      });

      It("should finish iteration even if searcher is empty", () => {
        EcmaValue iterator = matchAll.Call("a");
        EcmaValue result;

        result = iterator.Invoke("next");
        That(result["done"], Is.EqualTo(false));
        That(result["value"], Is.EquivalentTo(new[] { "" }));
        That(result["value"]["index"], Is.EqualTo(0));
        That(result["value"]["input"], Is.EqualTo("a"));

        result = iterator.Invoke("next");
        That(result["done"], Is.EqualTo(false));
        That(result["value"], Is.EquivalentTo(new[] { "" }));
        That(result["value"]["index"], Is.EqualTo(1));
        That(result["value"]["input"], Is.EqualTo("a"));

        result = iterator.Invoke("next");
        That(result["done"], Is.EqualTo(true));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Normalize(RuntimeFunction normalize) {
      IsUnconstructableFunctionWLength(normalize, "normalize", 0);
      IsAbruptedFromToPrimitive(normalize);
      IsAbruptedFromToPrimitive(normalize.Bind(""));
      IsAbruptedFromToObject(normalize);

      It("should return normalized string from coerced form", () => {
        EcmaValue str = "\u00C5\u2ADC\u0958\u2126\u0344";
        EcmaValue nfc = "\xC5\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301";
        EcmaValue nfd = "A\u030A\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301";

        Case(str, nfc);
        Case((str, Undefined), nfc);
        Case((str, EcmaArray.Of("NFC")), nfc);
        Case((str, CreateObject(toString: () => "NFC")), nfc);
        Case((str, EcmaArray.Of("NFD")), nfd);
        Case((str, CreateObject(toString: () => "NFD")), nfd);
      });

      It("should return normalized string", () => {
        Case(("\u1E9B\u0323", "NFC"), "\u1E9B\u0323", "Normalized on NFC");
        Case(("\u1E9B\u0323", "NFD"), "\u017F\u0323\u0307", "Normalized on NFD");
        Case(("\u1E9B\u0323", "NFKC"), "\u1E69", "Normalized on NFKC");
        Case(("\u1E9B\u0323", "NFKD"), "\u0073\u0323\u0307", "Normalized on NFKD");

        Case(("\u00C5\u2ADC\u0958\u2126\u0344", "NFC"), "\xC5\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301", "Normalized on NFC");
        Case(("\u00C5\u2ADC\u0958\u2126\u0344", "NFD"), "A\u030A\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301", "Normalized on NFD");
        Case(("\u00C5\u2ADC\u0958\u2126\u0344", "NFKC"), "\xC5\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301", "Normalized on NFKC");
        Case(("\u00C5\u2ADC\u0958\u2126\u0344", "NFKD"), "A\u030A\u2ADD\u0338\u0915\u093C\u03A9\u0308\u0301", "Normalized on NFKD");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void PadEnd(RuntimeFunction padEnd) {
      IsUnconstructableFunctionWLength(padEnd, "padEnd", 1);
      IsAbruptedFromToPrimitive(padEnd);
      IsAbruptedFromToPrimitive(padEnd.Bind(""));
      IsAbruptedFromToPrimitive(padEnd.Bind("", 0));
      IsAbruptedFromToObject(padEnd);

      It("should return the string unchanged when an explicit empty string is provided", () => {
        Case(("abc", 5, ""), "abc");
      });

      It("should stringify a non-string fillString value", () => {
        Case(("abc", 10, false), "abcfalsefa");
        Case(("abc", 10, true), "abctruetru");
        Case(("abc", 10, Null), "abcnullnul");
        Case(("abc", 10, 0), "abc0000000");
        Case(("abc", 10, -0), "abc0000000");
        Case(("abc", 10, NaN), "abcNaNNaNN");
      });

      It("should default to a fillString of \" \" when omitted", () => {
        Case(("abc", 5), "abc  ");
        Case(("abc", 5, Undefined), "abc  ");
      });

      It("should return the string unchanged when an integer max length is not greater than the string length", () => {
        Case(("abc", Undefined, "def"), "abc");
        Case(("abc", Null, "def"), "abc");
        Case(("abc", NaN, "def"), "abc");
        Case(("abc", -Infinity, "def"), "abc");
        Case(("abc", 0, "def"), "abc");
        Case(("abc", -1, "def"), "abc");
        Case(("abc", 3, "def"), "abc");
        Case(("abc", 3.9999, "def"), "abc");
      });

      It("should work in the general case", () => {
        Case(("abc", 7, "def"), "abcdefd");
        Case(("abc", 5, "*"), "abc**");
        Case(("abc", 6, "\uD83D\uDCA9"), "abc\uD83D\uDCA9\uD83D");
      });

      It("should perform observable operations in the correct order", () => {
        Logs.Clear();
        System.Func<string, EcmaValue, EcmaValue, EcmaValue> createPrimitiveObserver = (name, toString, valueOf) => CreateObject(
            toString: Intercept(() => toString, "toString:" + name),
            valueOf: Intercept(() => valueOf, "valueOf:" + name));
        Case((createPrimitiveObserver("receiver", new EcmaObject(), "abc"),
                createPrimitiveObserver("maxLength", 11, new EcmaObject()),
                createPrimitiveObserver("fillString", new EcmaObject(), "def")), "abcdefdefde");
        CollectionAssert.AreEqual(new[] { "toString:receiver", "valueOf:receiver", "valueOf:maxLength", "toString:maxLength", "toString:fillString", "valueOf:fillString" }, Logs);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void PadStart(RuntimeFunction padStart) {
      IsUnconstructableFunctionWLength(padStart, "padStart", 1);
      IsAbruptedFromToPrimitive(padStart);
      IsAbruptedFromToPrimitive(padStart.Bind(""));
      IsAbruptedFromToPrimitive(padStart.Bind("", 0));
      IsAbruptedFromToObject(padStart);

      It("should return the string unchanged when an explicit empty string is provided", () => {
        Case(("abc", 5, ""), "abc");
      });

      It("should stringify a non-string fillString value", () => {
        Case(("abc", 10, false), "falsefaabc");
        Case(("abc", 10, true), "truetruabc");
        Case(("abc", 10, Null), "nullnulabc");
        Case(("abc", 10, 0), "0000000abc");
        Case(("abc", 10, -0), "0000000abc");
        Case(("abc", 10, NaN), "NaNNaNNabc");
      });

      It("should default to a fillString of \" \" when omitted", () => {
        Case(("abc", 5), "  abc");
        Case(("abc", 5, Undefined), "  abc");
      });

      It("should return the string unchanged when an integer max length is not greater than the string length", () => {
        Case(("abc", Undefined, "def"), "abc");
        Case(("abc", Null, "def"), "abc");
        Case(("abc", NaN, "def"), "abc");
        Case(("abc", -Infinity, "def"), "abc");
        Case(("abc", 0, "def"), "abc");
        Case(("abc", -1, "def"), "abc");
        Case(("abc", 3, "def"), "abc");
        Case(("abc", 3.9999, "def"), "abc");
      });

      It("should work in the general case", () => {
        Case(("abc", 7, "def"), "defdabc");
        Case(("abc", 5, "*"), "**abc");
        Case(("abc", 6, "\uD83D\uDCA9"), "\uD83D\uDCA9\uD83Dabc");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Repeat(RuntimeFunction repeat) {
      IsUnconstructableFunctionWLength(repeat, "repeat", 1);
      IsAbruptedFromToPrimitive(repeat);
      IsAbruptedFromToPrimitive(repeat.Bind(""));
      IsAbruptedFromToObject(repeat);

      It("should returns an empty String if ToInteger(count) is zero", () => {
        Case(("ES2015", NaN), "", "str.repeat(NaN) returns \"\"");
        Case(("ES2015", Null), "", "str.repeat(null) returns \"\"");
        Case(("ES2015", Undefined), "", "str.repeat(undefined) returns \"\"");
        Case(("ES2015", false), "", "str.repeat(false) returns \"\"");
        Case(("ES2015", "0"), "", "str.repeat(\"0\") returns \"\"");
        Case(("ES2015", 0.9), "", "str.repeat(0.9) returns \"\"");
      });

      It("should throw a RangeError if count < 0 or is infinite", () => {
        Case(("", -1), Throws.RangeError);
        Case(("", Infinity), Throws.RangeError);
        Case(("", -Infinity), Throws.RangeError);
      });

      It("should return an empty string if this string is empty", () => {
        Case(("", 1), "");
        Case(("", 3), "");
        Case(("", 2147483647), "");
      });

      It("should return a String made from n copies of the original String appended together", () => {
        Case(("abc", 1), "abc");
        Case(("abc", 3), "abcabcabc");
        Case((".", 10000), new string('.', 10000));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Replace(RuntimeFunction replace) {
      IsUnconstructableFunctionWLength(replace, "replace", 2);
      IsAbruptedFromToPrimitive(replace);
      IsAbruptedFromToPrimitive(replace.Bind(""));
      IsAbruptedFromToPrimitive(replace.Bind("", ""));
      IsAbruptedFromToObject(replace);

      It("should invoke @@replace property of user-supplied objects", () => {
        EcmaValue returnVal = new EcmaObject();
        EcmaValue separator = new EcmaObject();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        int callCount = 0;
        separator[WellKnownSymbol.Replace] = RuntimeFunction.Create(() => {
          callCount += 1;
          args = Arguments;
          thisValue = This;
          return returnVal;
        });
        Case(("", separator, "replace value"), returnVal);
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.EqualTo(separator));
        That(args, Is.EquivalentTo(new[] { "", "replace value" }));
      });

      It("should do replacement left-to-right, when such are placement is performed, the new replacement text is not subject to further replacements", () => {
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh", "g"), "sch"), "She sells seaschells by the seaschore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh", "g"), "$$sch"), "She sells sea$schells by the sea$schore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh", "g"), "$&sch"), "She sells seashschells by the seashschore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh", "g"), "$`sch"), "She sells seaShe sells seaschells by the seaShe sells seashells by the seaschore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh", "g"), "$'sch"), "She sells seaells by the seashore.schells by the seaore.schore.");

        Case(("She sells seashells by the seashore.", RegExp.Construct("sh"), "sch"), "She sells seaschells by the seashore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh"), "$$sch"), "She sells sea$schells by the seashore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh"), "$&sch"), "She sells seashschells by the seashore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh"), "$`sch"), "She sells seaShe sells seaschells by the seashore.");
        Case(("She sells seashells by the seashore.", RegExp.Construct("sh"), "$'sch"), "She sells seaells by the seashore.schells by the seashore.");

        Case(("uid=31", RegExp.Construct("(uid=)(\\d+)"), "$1115"), "uid=115");
      });

      It("should invoke replacement function and replacement is done with the returned string", () => {
        Case(("abc12 def34", RegExp.Construct("([a-z]+)([0-9]+)"), RuntimeFunction.Create((v, a, b) => b + a)), "12abc def34");
        Case(("abc12 def34", RegExp.Construct("([a-z]+)([0-9]+)", "g"), RuntimeFunction.Create((v, a, b) => b + a)), "12abc 34def");
        Case(("aBc12 def34", RegExp.Construct("([a-z]+)([0-9]+)", "i"), RuntimeFunction.Create((v, a, b) => b + a)), "12aBc def34");
        Case(("aBc12 dEf34", RegExp.Construct("([a-z]+)([0-9]+)", "ig"), RuntimeFunction.Create((v, a, b) => b + a)), "12aBc 34dEf");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Search(RuntimeFunction search) {
      IsUnconstructableFunctionWLength(search, "search", 1);
      IsAbruptedFromToPrimitive(search);
      IsAbruptedFromToPrimitive(search.Bind(""));
      IsAbruptedFromToObject(search);

      It("should invoke @@search property of user-supplied objects", () => {
        EcmaValue returnVal = new EcmaObject();
        EcmaValue separator = new EcmaObject();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        int callCount = 0;
        separator[WellKnownSymbol.Search] = RuntimeFunction.Create(() => {
          callCount += 1;
          args = Arguments;
          thisValue = This;
          return returnVal;
        });
        Case(("", separator), returnVal);
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.EqualTo(separator));
        That(args, Is.EquivalentTo(new[] { "" }));
      });

      It("should return the start index of the matched substring by the RegExp object", () => {
        Case(("one two three four five", RegExp.Construct("four")), 14);
        Case(("one two three four five", RegExp.Construct("Four")), -1);
        Case(("one two three four five", RegExp.Construct("Four", "i")), 14);
      });

      It("should ignore global properties of RegExp object", () => {
        EcmaValue re = RegExp.Construct(".", "g");
        Case(("abc", re), 0);
        Case(("abc", re), 0);
        Case(("abc", re), 0);
      });

      It("should return the start index of the first occurence of the given string", () => {
        Case(("test string probe", "string pro"), 5);
        Case(("test string probe", "notexist"), -1);
        Case(("test string probe", "String pro"), -1);
      });

      It("should convert searcher to a RegExp object if a string is given", () => {
        Case(("a.b.c", "."), 0);
        Case(("a.b.c", Undefined), 0);
        Case(("a.b.c", Null), -1);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Slice(RuntimeFunction slice) {
      IsUnconstructableFunctionWLength(slice, "slice", 2);
      IsAbruptedFromToPrimitive(slice);
      IsAbruptedFromToPrimitive(slice.Bind(""));
      IsAbruptedFromToPrimitive(slice.Bind("", 0));
      IsAbruptedFromToObject(slice);

      It("should coerce this value to string and arguments to integers", () => {
        Case(("report", Undefined), "report");
        Case(("report", Null), "report");
        Case(("report", NaN), "report");
        Case(("report", false), "report");
        Case(("report", true), "eport");
        Case(("report", "e"), "report");

        Case(("", 1, 0), "");
        Case(("gnulluna", Infinity, NaN), "");
        Case(("gnulluna", Infinity, Infinity), "");
        Case(("gnulluna", -Infinity, -Infinity), "");
        Case(("gnulluna", -0.01, 0), "");
        Case(("gnulluna", 9, 9), "");
        Case(("gnulluna", 9, 0), "");
        Case(("gnulluna", 0, 100), "gnulluna");
        Case(("gnulluna", Null, -3), "gnull");
        Case(("gnulluna", NaN, Infinity), "gnulluna");

        Case((11.001002, Undefined), "11.001002");
        Case((Object.Construct(true), false, true), "t");
        Case((Boolean.Construct(), true, Undefined), "alse");
        Case((String.Construct("undefined"), Undefined, 3), "und");
        Case((new EcmaObject(), 0, 8), "[object ");
        Case((new EcmaObject(), 8), "Object]");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Split(RuntimeFunction split) {
      IsUnconstructableFunctionWLength(split, "split", 2);
      IsAbruptedFromToPrimitive(split);
      IsAbruptedFromToPrimitive(split.Bind(""));
      IsAbruptedFromToPrimitive(split.Bind("", ""));
      IsAbruptedFromToObject(split);

      It("should invoke @@split property of user-supplied objects", () => {
        EcmaValue returnVal = new EcmaObject();
        EcmaValue separator = new EcmaObject();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        int callCount = 0;
        separator[WellKnownSymbol.Split] = RuntimeFunction.Create(() => {
          callCount += 1;
          args = Arguments;
          thisValue = This;
          return returnVal;
        });
        Case(("", separator, "limit"), returnVal);
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.EqualTo(separator));
        That(args, Is.EquivalentTo(new[] { "", "limit" }));
      });

      It("should return an Array object containing substrings separated by the matching substrings of the RegExp object", () => {
        EcmaValue result;

        result = split.Call("hello", RegExp.Construct("l"));
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", RegExp.Construct("l"), 0);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(EcmaValue.EmptyArray));

        result = split.Call("hello", RegExp.Construct("l"), 1);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he" }));

        result = split.Call("hello", RegExp.Construct("l"), 2);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "" }));

        result = split.Call("hello", RegExp.Construct("l"), 3);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", RegExp.Construct("l"), 4);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", RegExp.Construct("l"), Undefined);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", RegExp.Construct("l"), "hi");
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(EcmaValue.EmptyArray));
      });

      It("should return an Array object containing substrings separated by the separater", () => {
        EcmaValue result;
        Case(("a.b.c", "."), new object[] { "a", "b", "c" });

        result = split.Call("hello", "l");
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", "l", 0);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(EcmaValue.EmptyArray));

        result = split.Call("hello", "l", 1);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he" }));

        result = split.Call("hello", "l", 2);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "" }));

        result = split.Call("hello", "l", 3);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", "l", 4);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", "l", Undefined);
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(new[] { "he", "", "o" }));

        result = split.Call("hello", "l", "hi");
        That(EcmaArray.IsArray(result));
        That(result, Is.EquivalentTo(EcmaValue.EmptyArray));
      });

      It("should perform SplitMatch correctly", () => {
        Case(("", ""), EcmaValue.EmptyArray);
        Case(("abc", ""), new[] { "a", "b", "c" });
        Case(("abc", "abc"), new[] { "", "" });
        Case(("abc", "abcd"), new[] { "abc" });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void StartsWith(RuntimeFunction startsWith) {
      IsUnconstructableFunctionWLength(startsWith, "startsWith", 1);
      IsAbruptedFromToPrimitive(startsWith);
      IsAbruptedFromToPrimitive(startsWith.Bind(""));
      IsAbruptedFromToPrimitive(startsWith.Bind("", ""));
      IsAbruptedFromToObject(startsWith);

      It("should return based on coerced values of position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", NaN), true);
        Case((str, "The future", Null), true);
        Case((str, "The future", false), true);
        Case((str, "The future", ""), true);
        Case((str, "The future", "0"), true);
        Case((str, "The future", Undefined), true);
        Case((str, "The future", 0.4), true);
        Case((str, "The future", true), false);
        Case((str, "The future", "1"), false);
        Case((str, "The future", 1.4), false);
        Case((str, "!", str["length"]), false);
        Case((str, "!", 100), false);
        Case((str, "!", Infinity), false);
        Case((str, "The future", -1), true);
        Case((str, "The future", -Infinity), true);
      });

      It("should return true if searchString.length == 0", () => {
        EcmaValue str = "The future is cool!";
        Case((str, ""), true);
        Case((str, "", str["length"]), true);
        Case((str, "", Infinity), true);
      });

      It("should return true if searchString appears as a substring of the given string with a given position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", 0), true);
        Case((str, "future", 4), true);
        Case((str, " is cool!", 10), true);
      });

      It("should return true if searchString appears as a substring of the given string", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The"), true);
        Case((str, "The future"), true);
        Case((str, str), true);
      });

      It("should throw a TypeError if searchString is a RegExp", () => {
        Case(("", RegExp.Construct(".")), Throws.TypeError);
      });

      It("should return false if searchString is not found with a given position", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "The future", 1), false);
        Case((str, str, 1), false);
      });

      It("should return false if searchString is not found", () => {
        EcmaValue str = "The future is cool!";
        Case((str, "Flash"), false);
        Case((str, "THE FUTURE"), false);
        Case((str, "future is cool!"), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Substring(RuntimeFunction substring) {
      IsUnconstructableFunctionWLength(substring, "substring", 2);
      IsAbruptedFromToPrimitive(substring);
      IsAbruptedFromToPrimitive(substring.Bind(""));
      IsAbruptedFromToPrimitive(substring.Bind("", 0));
      IsAbruptedFromToObject(substring);

      It("should coerce this value to string and arguments to integers", () => {
        Case(("report", Undefined), "report");
        Case(("report", Null), "report");
        Case(("report", NaN), "report");
        Case(("report", false), "report");
        Case(("report", true), "eport");
        Case(("report", "e"), "report");

        Case(("", 1, 0), "");
        Case(("this is a string object", 0, 8), "this is ");
        Case(("this is a string object", NaN, Infinity), "this is a string object");
        Case(("this is a string object", 1, 0), "t");
        Case(("this is a string object", Infinity, NaN), "this is a string object");
        Case(("this is a string object", Infinity, Infinity), "");
        Case(("this is a string object", -Infinity, -Infinity), "");
        Case(("this is a string object", -0.01, 0), "");
        Case(("this is a string object", "this is a string object".Length, "this is a string object".Length), "");
        Case(("this is a string object", "this is a string object".Length + 1, 0), "this is a string object");

        Case((11.001002, Undefined), "11.001002");
        Case((Object.Construct(true), false, true), "t");
        Case((Boolean.Construct(), true, Undefined), "alse");
        Case((String.Construct("undefined"), Undefined, 3), "und");
        Case((new EcmaObject(), 0, 8), "[object ");
        Case((new EcmaObject(), 8), "Object]");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToLowerCase(RuntimeFunction toLowerCase) {
      IsUnconstructableFunctionWLength(toLowerCase, "toLowerCase", 0);
      IsAbruptedFromToPrimitive(toLowerCase);
      IsAbruptedFromToObject(toLowerCase);

      It("should handle all locale-insensitive mappings in the SpecialCasings.txt", () => {
        Case("\u00DF", "\u00DF", "LATIN SMALL LETTER SHARP S");

        Case("\u0130", "\u0069\u0307", "LATIN CAPITAL LETTER I WITH DOT ABOVE");

        Case("\uFB00", "\uFB00", "LATIN SMALL LIGATURE FF");
        Case("\uFB01", "\uFB01", "LATIN SMALL LIGATURE FI");
        Case("\uFB02", "\uFB02", "LATIN SMALL LIGATURE FL");
        Case("\uFB03", "\uFB03", "LATIN SMALL LIGATURE FFI");
        Case("\uFB04", "\uFB04", "LATIN SMALL LIGATURE FFL");
        Case("\uFB05", "\uFB05", "LATIN SMALL LIGATURE LONG S T");
        Case("\uFB06", "\uFB06", "LATIN SMALL LIGATURE ST");

        Case("\u0587", "\u0587", "ARMENIAN SMALL LIGATURE ECH YIWN");
        Case("\uFB13", "\uFB13", "ARMENIAN SMALL LIGATURE MEN NOW");
        Case("\uFB14", "\uFB14", "ARMENIAN SMALL LIGATURE MEN ECH");
        Case("\uFB15", "\uFB15", "ARMENIAN SMALL LIGATURE MEN INI");
        Case("\uFB16", "\uFB16", "ARMENIAN SMALL LIGATURE VEW NOW");
        Case("\uFB17", "\uFB17", "ARMENIAN SMALL LIGATURE MEN XEH");

        Case("\u0149", "\u0149", "LATIN SMALL LETTER N PRECEDED BY APOSTROPHE");

        Case("\u0390", "\u0390", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND TONOS");
        Case("\u03B0", "\u03B0", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND TONOS");

        Case("\u01F0", "\u01F0", "LATIN SMALL LETTER J WITH CARON");
        Case("\u1E96", "\u1E96", "LATIN SMALL LETTER H WITH LINE BELOW");
        Case("\u1E97", "\u1E97", "LATIN SMALL LETTER T WITH DIAERESIS");
        Case("\u1E98", "\u1E98", "LATIN SMALL LETTER W WITH RING ABOVE");
        Case("\u1E99", "\u1E99", "LATIN SMALL LETTER Y WITH RING ABOVE");
        Case("\u1E9A", "\u1E9A", "LATIN SMALL LETTER A WITH RIGHT HALF RING");

        Case("\u1F50", "\u1F50", "GREEK SMALL LETTER UPSILON WITH PSILI");
        Case("\u1F52", "\u1F52", "GREEK SMALL LETTER UPSILON WITH PSILI AND VARIA");
        Case("\u1F54", "\u1F54", "GREEK SMALL LETTER UPSILON WITH PSILI AND OXIA");
        Case("\u1F56", "\u1F56", "GREEK SMALL LETTER UPSILON WITH PSILI AND PERISPOMENI");
        Case("\u1FB6", "\u1FB6", "GREEK SMALL LETTER ALPHA WITH PERISPOMENI");
        Case("\u1FC6", "\u1FC6", "GREEK SMALL LETTER ETA WITH PERISPOMENI");
        Case("\u1FD2", "\u1FD2", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND VARIA");
        Case("\u1FD3", "\u1FD3", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND OXIA");
        Case("\u1FD6", "\u1FD6", "GREEK SMALL LETTER IOTA WITH PERISPOMENI");
        Case("\u1FD7", "\u1FD7", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND PERISPOMENI");
        Case("\u1FE2", "\u1FE2", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND VARIA");
        Case("\u1FE3", "\u1FE3", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND OXIA");
        Case("\u1FE4", "\u1FE4", "GREEK SMALL LETTER RHO WITH PSILI");
        Case("\u1FE6", "\u1FE6", "GREEK SMALL LETTER UPSILON WITH PERISPOMENI");
        Case("\u1FE7", "\u1FE7", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND PERISPOMENI");
        Case("\u1FF6", "\u1FF6", "GREEK SMALL LETTER OMEGA WITH PERISPOMENI");

        Case("\u1F80", "\u1F80", "GREEK SMALL LETTER ALPHA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1F81", "\u1F81", "GREEK SMALL LETTER ALPHA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1F82", "\u1F82", "GREEK SMALL LETTER ALPHA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F83", "\u1F83", "GREEK SMALL LETTER ALPHA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F84", "\u1F84", "GREEK SMALL LETTER ALPHA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F85", "\u1F85", "GREEK SMALL LETTER ALPHA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F86", "\u1F86", "GREEK SMALL LETTER ALPHA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1F87", "\u1F87", "GREEK SMALL LETTER ALPHA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1F88", "\u1F80", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1F89", "\u1F81", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1F8A", "\u1F82", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F8B", "\u1F83", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F8C", "\u1F84", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F8D", "\u1F85", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F8E", "\u1F86", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1F8F", "\u1F87", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1F90", "\u1F90", "GREEK SMALL LETTER ETA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1F91", "\u1F91", "GREEK SMALL LETTER ETA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1F92", "\u1F92", "GREEK SMALL LETTER ETA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F93", "\u1F93", "GREEK SMALL LETTER ETA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F94", "\u1F94", "GREEK SMALL LETTER ETA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F95", "\u1F95", "GREEK SMALL LETTER ETA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F96", "\u1F96", "GREEK SMALL LETTER ETA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1F97", "\u1F97", "GREEK SMALL LETTER ETA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1F98", "\u1F90", "GREEK CAPITAL LETTER ETA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1F99", "\u1F91", "GREEK CAPITAL LETTER ETA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1F9A", "\u1F92", "GREEK CAPITAL LETTER ETA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F9B", "\u1F93", "GREEK CAPITAL LETTER ETA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F9C", "\u1F94", "GREEK CAPITAL LETTER ETA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F9D", "\u1F95", "GREEK CAPITAL LETTER ETA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F9E", "\u1F96", "GREEK CAPITAL LETTER ETA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1F9F", "\u1F97", "GREEK CAPITAL LETTER ETA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1FA0", "\u1FA0", "GREEK SMALL LETTER OMEGA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1FA1", "\u1FA1", "GREEK SMALL LETTER OMEGA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1FA2", "\u1FA2", "GREEK SMALL LETTER OMEGA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1FA3", "\u1FA3", "GREEK SMALL LETTER OMEGA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1FA4", "\u1FA4", "GREEK SMALL LETTER OMEGA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1FA5", "\u1FA5", "GREEK SMALL LETTER OMEGA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1FA6", "\u1FA6", "GREEK SMALL LETTER OMEGA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FA7", "\u1FA7", "GREEK SMALL LETTER OMEGA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1FA8", "\u1FA0", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1FA9", "\u1FA1", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1FAA", "\u1FA2", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1FAB", "\u1FA3", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1FAC", "\u1FA4", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1FAD", "\u1FA5", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1FAE", "\u1FA6", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1FAF", "\u1FA7", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1FB3", "\u1FB3", "GREEK SMALL LETTER ALPHA WITH YPOGEGRAMMENI");
        Case("\u1FBC", "\u1FB3", "GREEK CAPITAL LETTER ALPHA WITH PROSGEGRAMMENI");
        Case("\u1FC3", "\u1FC3", "GREEK SMALL LETTER ETA WITH YPOGEGRAMMENI");
        Case("\u1FCC", "\u1FC3", "GREEK CAPITAL LETTER ETA WITH PROSGEGRAMMENI");
        Case("\u1FF3", "\u1FF3", "GREEK SMALL LETTER OMEGA WITH YPOGEGRAMMENI");
        Case("\u1FFC", "\u1FF3", "GREEK CAPITAL LETTER OMEGA WITH PROSGEGRAMMENI");

        Case("\u1FB2", "\u1FB2", "GREEK SMALL LETTER ALPHA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FB4", "\u1FB4", "GREEK SMALL LETTER ALPHA WITH OXIA AND YPOGEGRAMMENI");
        Case("\u1FC2", "\u1FC2", "GREEK SMALL LETTER ETA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FC4", "\u1FC4", "GREEK SMALL LETTER ETA WITH OXIA AND YPOGEGRAMMENI");
        Case("\u1FF2", "\u1FF2", "GREEK SMALL LETTER OMEGA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FF4", "\u1FF4", "GREEK SMALL LETTER OMEGA WITH OXIA AND YPOGEGRAMMENI");

        Case("\u1FB7", "\u1FB7", "GREEK SMALL LETTER ALPHA WITH PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FC7", "\u1FC7", "GREEK SMALL LETTER ETA WITH PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FF7", "\u1FF7", "GREEK SMALL LETTER OMEGA WITH PERISPOMENI AND YPOGEGRAMMENI");
      });

      It("should support conditional mappings defined in SpecialCasings", () => {
        Case("\u03A3", "\u03C3", "Single GREEK CAPITAL LETTER SIGMA");

        // Sigma preceded by Cased and zero or more Case_Ignorable.
        Case("A\u03A3", "a\u03C2", "Sigma preceded by LATIN CAPITAL LETTER A");
        Case("\uD835\uDCA2\u03A3", "\uD835\uDCA2\u03C2", "Sigma preceded by MATHEMATICAL SCRIPT CAPITAL G (D835 DCA2 = 1D4A2)");
        Case("A.\u03A3", "a.\u03C2", "Sigma preceded by FULL STOP");
        Case("A\u00AD\u03A3", "a\u00AD\u03C2", "Sigma preceded by SOFT HYPHEN (00AD)");
        Case("A\uD834\uDE42\u03A3", "a\uD834\uDE42\u03C2", "Sigma preceded by COMBINING GREEK MUSICAL TRISEME (D834 DE42 = 1D242)");
        Case("\u0345\u03A3", "\u0345\u03C3", "Sigma preceded by COMBINING GREEK YPOGEGRAMMENI (0345)");
        Case("\u0391\u0345\u03A3", "\u03B1\u0345\u03C2", "Sigma preceded by GREEK CAPITAL LETTER ALPHA (0391), COMBINING GREEK YPOGEGRAMMENI (0345)");

        // Sigma not followed by zero or more Case_Ignorable and then Cased.
        Case("A\u03A3B", "a\u03C3b", "Sigma followed by LATIN CAPITAL LETTER B");
        Case("A\u03A3\uD835\uDCA2", "a\u03C3\uD835\uDCA2", "Sigma followed by MATHEMATICAL SCRIPT CAPITAL G (D835 DCA2 = 1D4A2)");
        Case("A\u03A3.b", "a\u03C3.b", "Sigma followed by FULL STOP");
        Case("A\u03A3\u00ADB", "a\u03C3\u00ADb", "Sigma followed by SOFT HYPHEN (00AD)");
        Case("A\u03A3\uD834\uDE42B", "a\u03C3\uD834\uDE42b", "Sigma followed by COMBINING GREEK MUSICAL TRISEME (D834 DE42 = 1D242)");
        Case("A\u03A3\u0345", "a\u03C2\u0345", "Sigma followed by COMBINING GREEK YPOGEGRAMMENI (0345)");
        Case("A\u03A3\u0345\u0391", "a\u03C3\u0345\u03B1", "Sigma followed by COMBINING GREEK YPOGEGRAMMENI (0345), GREEK CAPITAL LETTER ALPHA (0391)");
      });

      It("should iterate cover code points", () => {
        Case("\uD801\uDC00", "\uD801\uDC28", "DESERET CAPITAL LETTER LONG I");
        Case("\uD801\uDC01", "\uD801\uDC29", "DESERET CAPITAL LETTER LONG E");
        Case("\uD801\uDC02", "\uD801\uDC2A", "DESERET CAPITAL LETTER LONG A");
        Case("\uD801\uDC03", "\uD801\uDC2B", "DESERET CAPITAL LETTER LONG AH");
        Case("\uD801\uDC04", "\uD801\uDC2C", "DESERET CAPITAL LETTER LONG O");
        Case("\uD801\uDC05", "\uD801\uDC2D", "DESERET CAPITAL LETTER LONG OO");
        Case("\uD801\uDC06", "\uD801\uDC2E", "DESERET CAPITAL LETTER SHORT I");
        Case("\uD801\uDC07", "\uD801\uDC2F", "DESERET CAPITAL LETTER SHORT E");
        Case("\uD801\uDC08", "\uD801\uDC30", "DESERET CAPITAL LETTER SHORT A");
        Case("\uD801\uDC09", "\uD801\uDC31", "DESERET CAPITAL LETTER SHORT AH");
        Case("\uD801\uDC0A", "\uD801\uDC32", "DESERET CAPITAL LETTER SHORT O");
        Case("\uD801\uDC0B", "\uD801\uDC33", "DESERET CAPITAL LETTER SHORT OO");
        Case("\uD801\uDC0C", "\uD801\uDC34", "DESERET CAPITAL LETTER AY");
        Case("\uD801\uDC0D", "\uD801\uDC35", "DESERET CAPITAL LETTER OW");
        Case("\uD801\uDC0E", "\uD801\uDC36", "DESERET CAPITAL LETTER WU");
        Case("\uD801\uDC0F", "\uD801\uDC37", "DESERET CAPITAL LETTER YEE");
        Case("\uD801\uDC10", "\uD801\uDC38", "DESERET CAPITAL LETTER H");
        Case("\uD801\uDC11", "\uD801\uDC39", "DESERET CAPITAL LETTER PEE");
        Case("\uD801\uDC12", "\uD801\uDC3A", "DESERET CAPITAL LETTER BEE");
        Case("\uD801\uDC13", "\uD801\uDC3B", "DESERET CAPITAL LETTER TEE");
        Case("\uD801\uDC14", "\uD801\uDC3C", "DESERET CAPITAL LETTER DEE");
        Case("\uD801\uDC15", "\uD801\uDC3D", "DESERET CAPITAL LETTER CHEE");
        Case("\uD801\uDC16", "\uD801\uDC3E", "DESERET CAPITAL LETTER JEE");
        Case("\uD801\uDC17", "\uD801\uDC3F", "DESERET CAPITAL LETTER KAY");
        Case("\uD801\uDC18", "\uD801\uDC40", "DESERET CAPITAL LETTER GAY");
        Case("\uD801\uDC19", "\uD801\uDC41", "DESERET CAPITAL LETTER EF");
        Case("\uD801\uDC1A", "\uD801\uDC42", "DESERET CAPITAL LETTER VEE");
        Case("\uD801\uDC1B", "\uD801\uDC43", "DESERET CAPITAL LETTER ETH");
        Case("\uD801\uDC1C", "\uD801\uDC44", "DESERET CAPITAL LETTER THEE");
        Case("\uD801\uDC1D", "\uD801\uDC45", "DESERET CAPITAL LETTER ES");
        Case("\uD801\uDC1E", "\uD801\uDC46", "DESERET CAPITAL LETTER ZEE");
        Case("\uD801\uDC1F", "\uD801\uDC47", "DESERET CAPITAL LETTER ESH");
        Case("\uD801\uDC20", "\uD801\uDC48", "DESERET CAPITAL LETTER ZHEE");
        Case("\uD801\uDC21", "\uD801\uDC49", "DESERET CAPITAL LETTER ER");
        Case("\uD801\uDC22", "\uD801\uDC4A", "DESERET CAPITAL LETTER EL");
        Case("\uD801\uDC23", "\uD801\uDC4B", "DESERET CAPITAL LETTER EM");
        Case("\uD801\uDC24", "\uD801\uDC4C", "DESERET CAPITAL LETTER EN");
        Case("\uD801\uDC25", "\uD801\uDC4D", "DESERET CAPITAL LETTER ENG");
        Case("\uD801\uDC26", "\uD801\uDC4E", "DESERET CAPITAL LETTER OI");
        Case("\uD801\uDC27", "\uD801\uDC4F", "DESERET CAPITAL LETTER EW");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);

      It("is equal String.prototype.valueOf()", () => {
        Case(String.Construct(1), "1");
        Case(String.Construct("1"), "1");
        Case(String.Construct(true), "true");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToUpperCase(RuntimeFunction toUpperCase) {
      IsUnconstructableFunctionWLength(toUpperCase, "toUpperCase", 0);
      IsAbruptedFromToPrimitive(toUpperCase);
      IsAbruptedFromToObject(toUpperCase);

      It("should handle all locale-insensitive mappings in the SpecialCasings.txt", () => {
        Case("\u00DF", "\u0053\u0053", "LATIN SMALL LETTER SHARP S");

        Case("\u0130", "\u0130", "LATIN CAPITAL LETTER I WITH DOT ABOVE");

        Case("\uFB00", "\u0046\u0046", "LATIN SMALL LIGATURE FF");
        Case("\uFB01", "\u0046\u0049", "LATIN SMALL LIGATURE FI");
        Case("\uFB02", "\u0046\u004C", "LATIN SMALL LIGATURE FL");
        Case("\uFB03", "\u0046\u0046\u0049", "LATIN SMALL LIGATURE FFI");
        Case("\uFB04", "\u0046\u0046\u004C", "LATIN SMALL LIGATURE FFL");
        Case("\uFB05", "\u0053\u0054", "LATIN SMALL LIGATURE LONG S T");
        Case("\uFB06", "\u0053\u0054", "LATIN SMALL LIGATURE ST");

        Case("\u0587", "\u0535\u0552", "ARMENIAN SMALL LIGATURE ECH YIWN");
        Case("\uFB13", "\u0544\u0546", "ARMENIAN SMALL LIGATURE MEN NOW");
        Case("\uFB14", "\u0544\u0535", "ARMENIAN SMALL LIGATURE MEN ECH");
        Case("\uFB15", "\u0544\u053B", "ARMENIAN SMALL LIGATURE MEN INI");
        Case("\uFB16", "\u054E\u0546", "ARMENIAN SMALL LIGATURE VEW NOW");
        Case("\uFB17", "\u0544\u053D", "ARMENIAN SMALL LIGATURE MEN XEH");

        Case("\u0149", "\u02BC\u004E", "LATIN SMALL LETTER N PRECEDED BY APOSTROPHE");

        Case("\u0390", "\u0399\u0308\u0301", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND TONOS");
        Case("\u03B0", "\u03A5\u0308\u0301", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND TONOS");

        Case("\u01F0", "\u004A\u030C", "LATIN SMALL LETTER J WITH CARON");
        Case("\u1E96", "\u0048\u0331", "LATIN SMALL LETTER H WITH LINE BELOW");
        Case("\u1E97", "\u0054\u0308", "LATIN SMALL LETTER T WITH DIAERESIS");
        Case("\u1E98", "\u0057\u030A", "LATIN SMALL LETTER W WITH RING ABOVE");
        Case("\u1E99", "\u0059\u030A", "LATIN SMALL LETTER Y WITH RING ABOVE");
        Case("\u1E9A", "\u0041\u02BE", "LATIN SMALL LETTER A WITH RIGHT HALF RING");

        Case("\u1F50", "\u03A5\u0313", "GREEK SMALL LETTER UPSILON WITH PSILI");
        Case("\u1F52", "\u03A5\u0313\u0300", "GREEK SMALL LETTER UPSILON WITH PSILI AND VARIA");
        Case("\u1F54", "\u03A5\u0313\u0301", "GREEK SMALL LETTER UPSILON WITH PSILI AND OXIA");
        Case("\u1F56", "\u03A5\u0313\u0342", "GREEK SMALL LETTER UPSILON WITH PSILI AND PERISPOMENI");
        Case("\u1FB6", "\u0391\u0342", "GREEK SMALL LETTER ALPHA WITH PERISPOMENI");
        Case("\u1FC6", "\u0397\u0342", "GREEK SMALL LETTER ETA WITH PERISPOMENI");
        Case("\u1FD2", "\u0399\u0308\u0300", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND VARIA");
        Case("\u1FD3", "\u0399\u0308\u0301", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND OXIA");
        Case("\u1FD6", "\u0399\u0342", "GREEK SMALL LETTER IOTA WITH PERISPOMENI");
        Case("\u1FD7", "\u0399\u0308\u0342", "GREEK SMALL LETTER IOTA WITH DIALYTIKA AND PERISPOMENI");
        Case("\u1FE2", "\u03A5\u0308\u0300", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND VARIA");
        Case("\u1FE3", "\u03A5\u0308\u0301", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND OXIA");
        Case("\u1FE4", "\u03A1\u0313", "GREEK SMALL LETTER RHO WITH PSILI");
        Case("\u1FE6", "\u03A5\u0342", "GREEK SMALL LETTER UPSILON WITH PERISPOMENI");
        Case("\u1FE7", "\u03A5\u0308\u0342", "GREEK SMALL LETTER UPSILON WITH DIALYTIKA AND PERISPOMENI");
        Case("\u1FF6", "\u03A9\u0342", "GREEK SMALL LETTER OMEGA WITH PERISPOMENI");

        Case("\u1F80", "\u1F08\u0399", "GREEK SMALL LETTER ALPHA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1F81", "\u1F09\u0399", "GREEK SMALL LETTER ALPHA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1F82", "\u1F0A\u0399", "GREEK SMALL LETTER ALPHA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F83", "\u1F0B\u0399", "GREEK SMALL LETTER ALPHA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F84", "\u1F0C\u0399", "GREEK SMALL LETTER ALPHA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F85", "\u1F0D\u0399", "GREEK SMALL LETTER ALPHA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F86", "\u1F0E\u0399", "GREEK SMALL LETTER ALPHA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1F87", "\u1F0F\u0399", "GREEK SMALL LETTER ALPHA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1F88", "\u1F08\u0399", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1F89", "\u1F09\u0399", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1F8A", "\u1F0A\u0399", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F8B", "\u1F0B\u0399", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F8C", "\u1F0C\u0399", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F8D", "\u1F0D\u0399", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F8E", "\u1F0E\u0399", "GREEK CAPITAL LETTER ALPHA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1F8F", "\u1F0F\u0399", "GREEK CAPITAL LETTER ALPHA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1F90", "\u1F28\u0399", "GREEK SMALL LETTER ETA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1F91", "\u1F29\u0399", "GREEK SMALL LETTER ETA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1F92", "\u1F2A\u0399", "GREEK SMALL LETTER ETA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F93", "\u1F2B\u0399", "GREEK SMALL LETTER ETA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1F94", "\u1F2C\u0399", "GREEK SMALL LETTER ETA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F95", "\u1F2D\u0399", "GREEK SMALL LETTER ETA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1F96", "\u1F2E\u0399", "GREEK SMALL LETTER ETA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1F97", "\u1F2F\u0399", "GREEK SMALL LETTER ETA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1F98", "\u1F28\u0399", "GREEK CAPITAL LETTER ETA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1F99", "\u1F29\u0399", "GREEK CAPITAL LETTER ETA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1F9A", "\u1F2A\u0399", "GREEK CAPITAL LETTER ETA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F9B", "\u1F2B\u0399", "GREEK CAPITAL LETTER ETA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1F9C", "\u1F2C\u0399", "GREEK CAPITAL LETTER ETA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F9D", "\u1F2D\u0399", "GREEK CAPITAL LETTER ETA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1F9E", "\u1F2E\u0399", "GREEK CAPITAL LETTER ETA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1F9F", "\u1F2F\u0399", "GREEK CAPITAL LETTER ETA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1FA0", "\u1F68\u0399", "GREEK SMALL LETTER OMEGA WITH PSILI AND YPOGEGRAMMENI");
        Case("\u1FA1", "\u1F69\u0399", "GREEK SMALL LETTER OMEGA WITH DASIA AND YPOGEGRAMMENI");
        Case("\u1FA2", "\u1F6A\u0399", "GREEK SMALL LETTER OMEGA WITH PSILI AND VARIA AND YPOGEGRAMMENI");
        Case("\u1FA3", "\u1F6B\u0399", "GREEK SMALL LETTER OMEGA WITH DASIA AND VARIA AND YPOGEGRAMMENI");
        Case("\u1FA4", "\u1F6C\u0399", "GREEK SMALL LETTER OMEGA WITH PSILI AND OXIA AND YPOGEGRAMMENI");
        Case("\u1FA5", "\u1F6D\u0399", "GREEK SMALL LETTER OMEGA WITH DASIA AND OXIA AND YPOGEGRAMMENI");
        Case("\u1FA6", "\u1F6E\u0399", "GREEK SMALL LETTER OMEGA WITH PSILI AND PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FA7", "\u1F6F\u0399", "GREEK SMALL LETTER OMEGA WITH DASIA AND PERISPOMENI AND YPOGEGRAMMENI");

        Case("\u1FA8", "\u1F68\u0399", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND PROSGEGRAMMENI");
        Case("\u1FA9", "\u1F69\u0399", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND PROSGEGRAMMENI");
        Case("\u1FAA", "\u1F6A\u0399", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND VARIA AND PROSGEGRAMMENI");
        Case("\u1FAB", "\u1F6B\u0399", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND VARIA AND PROSGEGRAMMENI");
        Case("\u1FAC", "\u1F6C\u0399", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND OXIA AND PROSGEGRAMMENI");
        Case("\u1FAD", "\u1F6D\u0399", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND OXIA AND PROSGEGRAMMENI");
        Case("\u1FAE", "\u1F6E\u0399", "GREEK CAPITAL LETTER OMEGA WITH PSILI AND PERISPOMENI AND PROSGEGRAMMENI");
        Case("\u1FAF", "\u1F6F\u0399", "GREEK CAPITAL LETTER OMEGA WITH DASIA AND PERISPOMENI AND PROSGEGRAMMENI");

        Case("\u1FB3", "\u0391\u0399", "GREEK SMALL LETTER ALPHA WITH YPOGEGRAMMENI");
        Case("\u1FBC", "\u0391\u0399", "GREEK CAPITAL LETTER ALPHA WITH PROSGEGRAMMENI");
        Case("\u1FC3", "\u0397\u0399", "GREEK SMALL LETTER ETA WITH YPOGEGRAMMENI");
        Case("\u1FCC", "\u0397\u0399", "GREEK CAPITAL LETTER ETA WITH PROSGEGRAMMENI");
        Case("\u1FF3", "\u03A9\u0399", "GREEK SMALL LETTER OMEGA WITH YPOGEGRAMMENI");
        Case("\u1FFC", "\u03A9\u0399", "GREEK CAPITAL LETTER OMEGA WITH PROSGEGRAMMENI");

        Case("\u1FB2", "\u1FBA\u0399", "GREEK SMALL LETTER ALPHA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FB4", "\u0386\u0399", "GREEK SMALL LETTER ALPHA WITH OXIA AND YPOGEGRAMMENI");
        Case("\u1FC2", "\u1FCA\u0399", "GREEK SMALL LETTER ETA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FC4", "\u0389\u0399", "GREEK SMALL LETTER ETA WITH OXIA AND YPOGEGRAMMENI");
        Case("\u1FF2", "\u1FFA\u0399", "GREEK SMALL LETTER OMEGA WITH VARIA AND YPOGEGRAMMENI");
        Case("\u1FF4", "\u038F\u0399", "GREEK SMALL LETTER OMEGA WITH OXIA AND YPOGEGRAMMENI");

        Case("\u1FB7", "\u0391\u0342\u0399", "GREEK SMALL LETTER ALPHA WITH PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FC7", "\u0397\u0342\u0399", "GREEK SMALL LETTER ETA WITH PERISPOMENI AND YPOGEGRAMMENI");
        Case("\u1FF7", "\u03A9\u0342\u0399", "GREEK SMALL LETTER OMEGA WITH PERISPOMENI AND YPOGEGRAMMENI");
      });

      It("should iterate cover code points", () => {
        Case("\uD801\uDC28", "\uD801\uDC00", "DESERET SMALL LETTER LONG I");
        Case("\uD801\uDC29", "\uD801\uDC01", "DESERET SMALL LETTER LONG E");
        Case("\uD801\uDC2A", "\uD801\uDC02", "DESERET SMALL LETTER LONG A");
        Case("\uD801\uDC2B", "\uD801\uDC03", "DESERET SMALL LETTER LONG AH");
        Case("\uD801\uDC2C", "\uD801\uDC04", "DESERET SMALL LETTER LONG O");
        Case("\uD801\uDC2D", "\uD801\uDC05", "DESERET SMALL LETTER LONG OO");
        Case("\uD801\uDC2E", "\uD801\uDC06", "DESERET SMALL LETTER SHORT I");
        Case("\uD801\uDC2F", "\uD801\uDC07", "DESERET SMALL LETTER SHORT E");
        Case("\uD801\uDC30", "\uD801\uDC08", "DESERET SMALL LETTER SHORT A");
        Case("\uD801\uDC31", "\uD801\uDC09", "DESERET SMALL LETTER SHORT AH");
        Case("\uD801\uDC32", "\uD801\uDC0A", "DESERET SMALL LETTER SHORT O");
        Case("\uD801\uDC33", "\uD801\uDC0B", "DESERET SMALL LETTER SHORT OO");
        Case("\uD801\uDC34", "\uD801\uDC0C", "DESERET SMALL LETTER AY");
        Case("\uD801\uDC35", "\uD801\uDC0D", "DESERET SMALL LETTER OW");
        Case("\uD801\uDC36", "\uD801\uDC0E", "DESERET SMALL LETTER WU");
        Case("\uD801\uDC37", "\uD801\uDC0F", "DESERET SMALL LETTER YEE");
        Case("\uD801\uDC38", "\uD801\uDC10", "DESERET SMALL LETTER H");
        Case("\uD801\uDC39", "\uD801\uDC11", "DESERET SMALL LETTER PEE");
        Case("\uD801\uDC3A", "\uD801\uDC12", "DESERET SMALL LETTER BEE");
        Case("\uD801\uDC3B", "\uD801\uDC13", "DESERET SMALL LETTER TEE");
        Case("\uD801\uDC3C", "\uD801\uDC14", "DESERET SMALL LETTER DEE");
        Case("\uD801\uDC3D", "\uD801\uDC15", "DESERET SMALL LETTER CHEE");
        Case("\uD801\uDC3E", "\uD801\uDC16", "DESERET SMALL LETTER JEE");
        Case("\uD801\uDC3F", "\uD801\uDC17", "DESERET SMALL LETTER KAY");
        Case("\uD801\uDC40", "\uD801\uDC18", "DESERET SMALL LETTER GAY");
        Case("\uD801\uDC41", "\uD801\uDC19", "DESERET SMALL LETTER EF");
        Case("\uD801\uDC42", "\uD801\uDC1A", "DESERET SMALL LETTER VEE");
        Case("\uD801\uDC43", "\uD801\uDC1B", "DESERET SMALL LETTER ETH");
        Case("\uD801\uDC44", "\uD801\uDC1C", "DESERET SMALL LETTER THEE");
        Case("\uD801\uDC45", "\uD801\uDC1D", "DESERET SMALL LETTER ES");
        Case("\uD801\uDC46", "\uD801\uDC1E", "DESERET SMALL LETTER ZEE");
        Case("\uD801\uDC47", "\uD801\uDC1F", "DESERET SMALL LETTER ESH");
        Case("\uD801\uDC48", "\uD801\uDC20", "DESERET SMALL LETTER ZHEE");
        Case("\uD801\uDC49", "\uD801\uDC21", "DESERET SMALL LETTER ER");
        Case("\uD801\uDC4A", "\uD801\uDC22", "DESERET SMALL LETTER EL");
        Case("\uD801\uDC4B", "\uD801\uDC23", "DESERET SMALL LETTER EM");
        Case("\uD801\uDC4C", "\uD801\uDC24", "DESERET SMALL LETTER EN");
        Case("\uD801\uDC4D", "\uD801\uDC25", "DESERET SMALL LETTER ENG");
        Case("\uD801\uDC4E", "\uD801\uDC26", "DESERET SMALL LETTER OI");
        Case("\uD801\uDC4F", "\uD801\uDC27", "DESERET SMALL LETTER EW");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Trim(RuntimeFunction trim) {
      IsUnconstructableFunctionWLength(trim, "trim", 0);
      IsAbruptedFromToPrimitive(trim);
      IsAbruptedFromToObject(trim);

      It("should remove all whitespace from the start of a string", () => {
        string wspc = "\x09\x0A\x0B\x0C\x0D\x20\xA0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\u2028\u2029\uFEFF";
        Case(wspc + "a" + wspc + "b" + wspc, "a" + wspc + "b");
      });

      It("should convert this value to a string", () => {
        Case(true, "true");
        Case(false, "false");
        Case(NaN, "NaN");
        Case(Infinity, "Infinity");
        Case(-Infinity, "-Infinity");
        Case(-0, "0");
        Case(1, "1");
        Case(-1, "-1");
        Case(Boolean.Construct(false), "false");
        Case(Number.Construct(0), "0");
        Case(new EcmaObject(), "[object Object]");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void TrimEnd(RuntimeFunction trimEnd) {
      IsUnconstructableFunctionWLength(trimEnd, "trimEnd", 0);
      IsAbruptedFromToPrimitive(trimEnd);
      IsAbruptedFromToObject(trimEnd);

      It("should remove all whitespace from the start of a string", () => {
        string wspc = "\x09\x0A\x0B\x0C\x0D\x20\xA0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\u2028\u2029\uFEFF";
        Case(wspc + "a" + wspc + "b" + wspc, wspc + "a" + wspc + "b");
      });

      It("should convert this value to a string", () => {
        Case(true, "true");
        Case(false, "false");
        Case(NaN, "NaN");
        Case(Infinity, "Infinity");
        Case(-Infinity, "-Infinity");
        Case(-0, "0");
        Case(1, "1");
        Case(-1, "-1");
        Case(Boolean.Construct(false), "false");
        Case(Number.Construct(0), "0");
        Case(new EcmaObject(), "[object Object]");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void TrimStart(RuntimeFunction trimStart) {
      IsUnconstructableFunctionWLength(trimStart, "trimStart", 0);
      IsAbruptedFromToPrimitive(trimStart);
      IsAbruptedFromToObject(trimStart);

      It("should remove all whitespace from the start of a string", () => {
        string wspc = "\x09\x0A\x0B\x0C\x0D\x20\xA0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000\u2028\u2029\uFEFF";
        Case(wspc + "a" + wspc + "b" + wspc, "a" + wspc + "b" + wspc);
      });

      It("should convert this value to a string", () => {
        Case(true, "true");
        Case(false, "false");
        Case(NaN, "NaN");
        Case(Infinity, "Infinity");
        Case(-Infinity, "-Infinity");
        Case(-0, "0");
        Case(1, "1");
        Case(-1, "-1");
        Case(Boolean.Construct(false), "false");
        Case(Number.Construct(0), "0");
        Case(new EcmaObject(), "[object Object]");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);

      It("should return this string value", () => {
        Case(String.Construct(1), "1");
        Case(String.Construct("1"), "1");
        Case(String.Construct(true), "true");
      });
    }
  }
}
