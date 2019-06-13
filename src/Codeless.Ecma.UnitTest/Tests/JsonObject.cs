using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class JsonObject : TestBase {
    [Test]
    public void Properties() {
      That(GlobalThis, Has.OwnProperty("JSON", Global.Json, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Global.Json, Has.OwnProperty(WellKnownSymbol.ToStringTag, "JSON", EcmaPropertyAttributes.Configurable));
      That(Global.Json, Is.TypeOf("object"));
      That(() => Global.Json.Call(This), Throws.TypeError);
      That(() => Global.Json.Construct(), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Parse(RuntimeFunction parse) {
      IsUnconstructableFunctionWLength(parse, "parse", 2);

      It("should throws a SyntaxError for whitespace characters other than \\t, \\r, \\n, and ' '", () => {
        Case((_, "\u000b1234"), Throws.SyntaxError);
        Case((_, "\u000c1234"), Throws.SyntaxError);
        Case((_, "\u00a01234"), Throws.SyntaxError);
        Case((_, "\u16801234"), Throws.SyntaxError);
        Case((_, "\u180e1234"), Throws.SyntaxError);
        Case((_, "\u20001234"), Throws.SyntaxError);
        Case((_, "\u20011234"), Throws.SyntaxError);
        Case((_, "\u20021234"), Throws.SyntaxError);
        Case((_, "\u20031234"), Throws.SyntaxError);
        Case((_, "\u20041234"), Throws.SyntaxError);
        Case((_, "\u20051234"), Throws.SyntaxError);
        Case((_, "\u20061234"), Throws.SyntaxError);
        Case((_, "\u20071234"), Throws.SyntaxError);
        Case((_, "\u20081234"), Throws.SyntaxError);
        Case((_, "\u20091234"), Throws.SyntaxError);
        Case((_, "\u200a1234"), Throws.SyntaxError);
        Case((_, "\u200b1234"), Throws.SyntaxError);
        Case((_, "\u202f1234"), Throws.SyntaxError);
        Case((_, "\u205f1234"), Throws.SyntaxError);
        Case((_, "\u30001234"), Throws.SyntaxError);
        Case((_, "\ufeff1234"), Throws.SyntaxError);
        Case((_, "\u2028\u20291234"), Throws.SyntaxError);
      });

      It("should ignore preceding or trailing whitespace", () => {
        Case((_, "\t1234"), 1234);
        Case((_, "\r1234"), 1234);
        Case((_, "\n1234"), 1234);
        Case((_, " 1234"), 1234);
        Case((_, "1234\t"), 1234);
        Case((_, "1234\r"), 1234);
        Case((_, "1234\n"), 1234);
        Case((_, "1234 "), 1234);
        Case((_, "\t\t1234\t\t"), 1234);
        Case((_, "\r\r1234\r\r"), 1234);
        Case((_, "\n\n1234\n\n"), 1234);
        Case((_, "  1234  "), 1234);
      });

      It("should throw a SyntaxError when whitespace results in two tokens", () => {
        Case((_, "12 34"), Throws.SyntaxError);
        Case((_, "12\t34"), Throws.SyntaxError);
        Case((_, "12\r34"), Throws.SyntaxError);
        Case((_, "12\n34"), Throws.SyntaxError);
        Case((_, "12\t\r\n 34"), Throws.SyntaxError);
      });

      It("should parse string enclosed in double quotes only", () => {
        Case((_, "\"\""), "");
        Case((_, "\"abc\""), "abc");
        Case((_, "'abc'"), Throws.SyntaxError);
        Case((_, "\\u0022abc\\u0022"), Throws.SyntaxError);
        Case((_, "\"abc'"), Throws.SyntaxError);
      });

      It("should parse string with allowed escape sequence", () => {
        Case((_, "\"\\/\""), "/");
        Case((_, "\"\\\\\""), "\\");
        Case((_, "\"\\b\""), "\b");
        Case((_, "\"\\f\""), "\f");
        Case((_, "\"\\n\""), "\n");
        Case((_, "\"\\r\""), "\r");
        Case((_, "\"\\t\""), "\t");
        Case((_, "\"\\v\""), Throws.SyntaxError);
        Case((_, "\"\\u0058\""), "X");
        Case((_, "\"\\u005\""), Throws.SyntaxError);
        Case((_, "\"\\u0X50\""), Throws.SyntaxError);
      });

      It("should parse number in decimal and scientific notation", () => {
        Case((_, "0"), 0);
        Case((_, "1230"), 1230);
        Case((_, "0.123"), 0.123);
        Case((_, "123.456"), 123.456);
        Case((_, "1.23e1"), 12.3);
        Case((_, "1.23E1"), 12.3);
        Case((_, "-1230"), -1230);
        Case((_, "-0.123"), -0.123);
        Case((_, "-123.456"), -123.456);
        Case((_, "-1.23e1"), -12.3);
        Case((_, "-1.23E1"), -12.3);
        Case((_, "0x1"), Throws.SyntaxError);
        Case((_, "0123"), Throws.SyntaxError);
      });

      It("should parse 'true', 'false', and 'null'", () => {
        Case((_, "[true]"), new[] { true });
        Case((_, "[false]"), new[] { false });
        Case((_, "[null]"), new[] { Null });
      });

      It("should throw a SyntaxError if there a string middles control characters", () => {
        for (int i = 0; i < 0x20; i++) {
          Case((_, "\"" + (char)i + "\""), Throws.SyntaxError);
          Case((_, "{\"" + (char)i + "\":\"John\"}"), Throws.SyntaxError);
          Case((_, "{\"name\":\"" + (char)i + "\"}"), Throws.SyntaxError);
        }
      });

      It("should treat '__proto__' as a normal property key", () => {
        EcmaValue x = parse.Call(_, "{\"__proto__\":[]}");
        That(Object.Invoke("getPrototypeOf", x), Is.EqualTo(Object.Prototype));
        That(EcmaArray.IsArray(x["__proto__"]));
      });

      It("should return abrupt completion from reviver", () => {
        Case((_, "[0,0]", ThrowTest262Exception), Throws.Test262);
        Case((_, "{\"0\":0,\"1\":0}", RuntimeFunction.Create(v => DefineProperty(This, "1", get: ThrowTest262Exception))), Throws.Test262);
        Case((_, "{\"0\":0,\"1\":0}", RuntimeFunction.Create(v => Void(This.ToObject()[1] = Proxy.Construct(new EcmaObject(), CreateObject(("ownKeys", ThrowTest262Exception)))))), Throws.Test262);
        Case((_, "{\"0\":0,\"1\":0}", RuntimeFunction.Create(v => Void(This.ToObject()[1] = Proxy.Construct(CreateObject(("a", 1)), CreateObject(("deleteProperty", ThrowTest262Exception)))))), Throws.Test262);
        Case((_, "{\"0\":0,\"1\":0}", RuntimeFunction.Create((k, v) => v == 0 ? Void(This.ToObject()[1] = Proxy.Construct(CreateObject(("0", Null)), CreateObject(("defineProperty", ThrowTest262Exception)))) : v)), Throws.Test262);

        EcmaValue revoked = Proxy.Invoke("revocable", new EcmaObject(), new EcmaObject());
        revoked.Invoke("revoke");
        int returnCount = 0;
        Case((_, "{\"0\":0,\"1\":0}", RuntimeFunction.Create(v => Void(This.ToObject()[1] = revoked, returnCount++))), Throws.TypeError);
        That(returnCount, Is.EqualTo(1));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Stringify(RuntimeFunction stringify) {
      IsUnconstructableFunctionWLength(stringify, "stringify", 3);

      It("should format primitive values to according format", () => {
        Case((_, "a string"), "\"a string\"");
        Case((_, 123), "123");
        Case((_, true), "true");
        Case((_, Null), "null");
        Case((_, Number.Construct(42)), "42");
        Case((_, String.Construct("wrappered")), "\"wrappered\"");
        Case((_, Boolean.Construct(false)), "false");
      });

      It("should not serialize root object or properties which values are undefined or functions", () => {
        Case((_, Undefined), Undefined);
        Case((_, RuntimeFunction.Create(() => Undefined)), Undefined);
      });

      It("should replace values by that returned from the replacer function", () => {
        Case((_, Undefined, RuntimeFunction.Create((k, v) => "replacement")), "\"replacement\"");
        Case((_, 42, RuntimeFunction.Create((k, v) => Undefined)), Undefined);
        Case((_, 42, RuntimeFunction.Create((k, v) => v == 42 ? EcmaArray.Of(4, 2) : v)), "[4,2]");
        Case((_, 42, RuntimeFunction.Create((k, v) => v == 42 ? CreateObject(("forty", 2)) : v)), "{\"forty\":2}");
        Case((_, RuntimeFunction.Create(() => Undefined), RuntimeFunction.Create((k, v) => 99)), "99");
        Case((_, CreateObject(("prop", 1)), RuntimeFunction.Create((k, v) => Undefined)), Undefined);
      });

      It("should ignore replacer argument that are not functions or arrays", () => {
        Case((_, EcmaArray.Of(42), new EcmaObject()), "[42]");
      });

      It("should coerce Number, Boolean and String objects returned from replacer to their internal values", () => {
        Case((_, EcmaArray.Of(42), RuntimeFunction.Create((k, v) => v == 42 ? String.Construct("fortytwo") : v)), "[\"fortytwo\"]");
        Case((_, EcmaArray.Of(42), RuntimeFunction.Create((k, v) => v == 42 ? Number.Construct(84) : v)), "[84]");
        Case((_, EcmaArray.Of(42), RuntimeFunction.Create((k, v) => v == 42 ? Boolean.Construct(false) : v)), "[false]");
      });

      It("should replace object by value returned from toJSON()", () => {
        EcmaValue obj = new EcmaObject();
        obj["prop"] = 42;

        obj["toJSON"] = RuntimeFunction.Create(() => Boolean.Construct(true));
        Case((_, EcmaArray.Of(obj)), "[true]");

        obj["toJSON"] = RuntimeFunction.Create(() => Number.Construct(42));
        Case((_, EcmaArray.Of(obj)), "[42]");

        obj["toJSON"] = RuntimeFunction.Create(() => "fortytwo objects");
        Case((_, EcmaArray.Of(obj)), "[\"fortytwo objects\"]");
      });

      It("should throw a TypeError when there is circular reference", () => {
        EcmaValue obj = new EcmaObject();
        obj["prop"] = obj;
        Case((_, obj), Throws.TypeError);

        EcmaValue inner = new EcmaObject();
        EcmaValue circularObj = CreateObject(("p1", CreateObject(("p2", inner))));
        inner["prop"] = circularObj;
        Case((_, circularObj), Throws.TypeError);
      });

      It("should ignore indentString argument that are not strings or numbers", () => {
        EcmaValue obj = CreateObject(("a1", CreateObject(("b1", EcmaArray.Of(1, 2, 3, 4)), ("b2", CreateObject(("c1", 1), ("c2", 2))))), ("a2", "a2"));
        That(stringify.Call(_, obj), Is.EqualTo(stringify.Call(_, obj, Null, "")));
        That(stringify.Call(_, obj), Is.EqualTo(stringify.Call(_, obj, Null, true)));
        That(stringify.Call(_, obj), Is.EqualTo(stringify.Call(_, obj, Null, Null)));
        That(stringify.Call(_, obj), Is.EqualTo(stringify.Call(_, obj, Null, obj)));
        That(stringify.Call(_, obj), Is.EqualTo(stringify.Call(_, obj, Null, Boolean.Construct(true))));
      });

      It("should clamp indentString to at most 10 characters", () => {
        EcmaValue obj = CreateObject(("a1", CreateObject(("b1", EcmaArray.Of(1, 2, 3, 4)), ("b2", CreateObject(("c1", 1), ("c2", 2))))), ("a2", "a2"));
        That(stringify.Call(_, obj, Null, "0123456789xxxxxxxxx"), Is.EqualTo(stringify.Call(_, obj, Null, "0123456789")));
        That(stringify.Call(_, obj, Null, String.Construct("xxx")), Is.EqualTo(stringify.Call(_, obj, Null, "xxx")));
        That(stringify.Call(_, obj, Null, Number.Construct(5)), Is.EqualTo(stringify.Call(_, obj, Null, "     ")));
        That(stringify.Call(_, obj, Null, 5), Is.EqualTo(stringify.Call(_, obj, Null, "     ")));
        That(stringify.Call(_, obj, Null, 5.999999), Is.EqualTo(stringify.Call(_, obj, Null, "     ")));
        That(stringify.Call(_, obj, Null, -5), Is.EqualTo(stringify.Call(_, obj)));
        That(stringify.Call(_, obj, Null, 0), Is.EqualTo(stringify.Call(_, obj)));
        That(stringify.Call(_, obj, Null, 0.999999), Is.EqualTo(stringify.Call(_, obj)));
        That(stringify.Call(_, obj, Null, 100), Is.EqualTo(stringify.Call(_, obj, Null, 10)));
      });

      It("should properly escape code points in property names and string values", () => {
        Hashtable dictionary = new Hashtable {
          { "\"", "\\\"" },
          { "\\", "\\\\" },
          { "\x00", "\\u0000" },
          { "\x01", "\\u0001"},
          { "\x02", "\\u0002"},
          { "\x03", "\\u0003"},
          { "\x04", "\\u0004"},
          { "\x05", "\\u0005"},
          { "\x06", "\\u0006"},
          { "\x07", "\\u0007"},
          { "\x08", "\\b" },
          { "\x09", "\\t" },
          { "\x0A", "\\n" },
          { "\x0B", "\\u000b"},
          { "\x0C", "\\f" },
          { "\x0D", "\\r" },
          { "\x0E", "\\u000e"},
          { "\x0F", "\\u000f"},
          { "\x10", "\\u0010"},
          { "\x11", "\\u0011"},
          { "\x12", "\\u0012"},
          { "\x13", "\\u0013"},
          { "\x14", "\\u0014"},
          { "\x15", "\\u0015"},
          { "\x16", "\\u0016"},
          { "\x17", "\\u0017"},
          { "\x18", "\\u0018"},
          { "\x19", "\\u0019"},
          { "\x1A", "\\u001a"},
          { "\x1B", "\\u001b"},
          { "\x1C", "\\u001c"},
          { "\x1D", "\\u001d"},
          { "\x1E", "\\u001e"},
          { "\x1F", "\\u001f"},
          { "\uD834", "\\ud834" },
          { "\uDF06", "\\udf06" },
          { "\uD834\uDF06", "𝌆" },
          { "\uD834\uD834\uDF06\uD834", "\\ud834𝌆\\ud834" },
          { "\uD834\uD834\uDF06\uDF06", "\\ud834𝌆\\udf06" },
          { "\uDF06\uD834\uDF06\uD834", "\\udf06𝌆\\ud834" },
          { "\uDF06\uD834\uDF06\uDF06", "\\udf06𝌆\\udf06" },
          { "\uDF06\uD834", "\\udf06\\ud834" },
          { "\uD834\uDF06\uD834\uD834", "𝌆\\ud834\\ud834" },
          { "\uD834\uDF06\uD834\uDF06", "𝌆𝌆" },
          { "\uDF06\uDF06\uD834\uD834", "\\udf06\\udf06\\ud834\\ud834" },
          { "\uDF06\uDF06\uD834\uDF06", "\\udf06\\udf06𝌆" },
        };
        foreach (DictionaryEntry e in dictionary) {
          Case((_, (string)e.Key), "\"" + e.Value + "\"");
          Case((_, CreateObject(((string)e.Key, 0))), "{\"" + e.Value + "\":0}");
        }
      });
    }
  }
}
