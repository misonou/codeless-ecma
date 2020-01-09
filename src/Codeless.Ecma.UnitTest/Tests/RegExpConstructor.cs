using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class RegExpConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "RegExp", 2, RegExp.Prototype);

      It("should not throw TypeError when pattern has a [[RegExpMatcher]] internal slot", () => {
        EcmaValue re = RegExp.Construct(RegExp.Construct(), "g");
        That(re["global"], Is.EqualTo(true));
      });

      It("should return its input argument if ToBoolean(@@match) is true and constructor is RegExp", () => {
        EcmaValue obj = new EcmaObject();
        obj["constructor"] = RegExp;

        obj[WellKnownSymbol.Match] = true;
        Case((RegExp, obj), obj);
        obj[WellKnownSymbol.Match] = "string";
        Case((RegExp, obj), obj);
        obj[WellKnownSymbol.Match] = new EcmaArray();
        Case((RegExp, obj), obj);
        obj[WellKnownSymbol.Match] = new Symbol();
        Case((RegExp, obj), obj);
        obj[WellKnownSymbol.Match] = 86;
        Case((RegExp, obj), obj);

        EcmaValue re1 = RegExp.Construct("(?:)");
        re1[WellKnownSymbol.Match] = false;
        Case((RegExp, re1), Is.Not.EqualTo(re1));

        EcmaValue re2 = RegExp.Construct("(?:)");
        re2["constructor"] = Null;
        Case((RegExp, re2), Is.Not.EqualTo(re2));
      });

      It("should coerce its input argument to String", () => {
        EcmaValue re;
        re = RegExp.Construct(1, Object.Construct("gi"));
        That(re["source"], Is.EqualTo("1"));
        That(re["ignoreCase"], Is.EqualTo(true));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(true));
        That(re, Has.OwnProperty("lastIndex", 0, EcmaPropertyAttributes.Writable));

        re = RegExp.Construct();
        That(re["source"], Is.EqualTo("(?:)"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(Undefined);
        That(re["source"], Is.EqualTo("(?:)"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(Undefined, Undefined);
        That(re["source"], Is.EqualTo("(?:)"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(Null, Undefined);
        That(re["source"], Is.EqualTo("null"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(true, Undefined);
        That(re["source"], Is.EqualTo("true"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(CreateObject(toString: () => true), CreateObject(toString: () => "i"));
        That(re["source"], Is.EqualTo("true"));
        That(re["ignoreCase"], Is.EqualTo(true));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        re = RegExp.Construct(CreateObject(toString: () => Undefined));
        That(re["source"], Is.EqualTo("undefined"));
        That(re["ignoreCase"], Is.EqualTo(false));
        That(re["multiline"], Is.EqualTo(false));
        That(re["global"], Is.EqualTo(false));

        That(() => RegExp.Construct(CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
        That(() => RegExp.Construct(CreateObject(toString: () => new EcmaObject(), valueOf: ThrowTest262Exception)), Throws.Test262);

        That(() => RegExp.Construct("", Null), Throws.SyntaxError);
        That(() => RegExp.Construct("", true), Throws.SyntaxError);
        That(() => RegExp.Construct("", 1.0), Throws.SyntaxError);
        That(() => RegExp.Construct("", new EcmaObject()), Throws.SyntaxError);
        That(() => RegExp.Construct("", CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
        That(() => RegExp.Construct("", CreateObject(toString: () => new EcmaObject(), valueOf: ThrowTest262Exception)), Throws.Test262);
      });

      It("should initialize RegExp object with [[OriginalPattern]] and optionally [[OriginalFlag]]", () => {
        EcmaValue re1 = RegExp.Construct("1", "mig");
        EcmaValue re2 = RegExp.Construct(re1);
        That(re2["source"], Is.EqualTo(re1["source"]));
        That(re2["ignoreCase"], Is.EqualTo(true));
        That(re2["multiline"], Is.EqualTo(true));
        That(re2["global"], Is.EqualTo(true));

        re2 = RegExp.Construct(re1, Undefined);
        That(re2["source"], Is.EqualTo(re1["source"]));
        That(re2["ignoreCase"], Is.EqualTo(true));
        That(re2["multiline"], Is.EqualTo(true));
        That(re2["global"], Is.EqualTo(true));

        EcmaValue re3 = RegExp.Construct(re1, "");
        That(re3["source"], Is.EqualTo(re1["source"]));
        That(re3["ignoreCase"], Is.EqualTo(false));
        That(re3["multiline"], Is.EqualTo(false));
        That(re3["global"], Is.EqualTo(false));
      });

      It("should initialize from a RegExp-like object", () => {
        EcmaValue obj = new EcmaObject();
        obj["source"] = "source text";
        obj["flags"] = "i";
        obj[Symbol.Match] = true;

        EcmaValue re = RegExp.Construct(obj);
        That(re["source"], Is.EqualTo("source text"));
        That(re["flags"], Is.EqualTo("i"));

        DefineProperty(obj, "flags", get: ThrowTest262Exception);
        re = RegExp.Construct(obj, "g");
        That(re["source"], Is.EqualTo("source text"));
        That(re["flags"], Is.EqualTo("g"));

        // `constructor` should be referenced
        DefineProperty(obj, "constructor", get: ThrowTest262Exception);
        That(() => RegExp.Call(_, obj), Throws.Test262);
        That(() => RegExp.Construct(obj), Throws.Test262);

        // the `flags` property should not be referenced before `source`
        obj = new EcmaObject();
        DefineProperty(obj, "source", get: ThrowTest262Exception);
        DefineProperty(obj, "flags", get: () => throw new System.Exception());
        obj[Symbol.Match] = true;
        That(() => RegExp.Construct(obj), Throws.Test262);
      });

      It("should throw a SyntaxError if invalid flag is given", () => {
        That(() => RegExp.Construct("", "a"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "gg"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "ii"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "mm"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "ss"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "uu"), Throws.SyntaxError);
        That(() => RegExp.Construct("", "yy"), Throws.SyntaxError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(RegExp, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(RegExp.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(RegExp, Is.EqualTo(RegExp));
    }
  }
}
