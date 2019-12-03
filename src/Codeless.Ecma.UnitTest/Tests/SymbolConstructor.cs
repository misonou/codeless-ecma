using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SymbolConstructor : TestBase {
    RuntimeFunction Symbol => Global.Symbol;

    [Test]
    public void Properties() {
      RuntimeRealm realm = new RuntimeRealm();
      EcmaValue Symbol = Global.Symbol;
      EcmaValue OSymbol = realm.GetRuntimeObject(WellKnownObject.SymbolConstructor);

      foreach (var i in new[] { "asyncIterator", "hasInstance", "isConcatSpreadable", "iterator", "match", "matchAll", "replace", "search", "species", "split", "toPrimitive", "toStringTag", "unscopables" }) {
        That(Symbol, Has.OwnProperty(i, EcmaPropertyAttributes.None), "Symbol.{0} descriptor", i);
        That(Symbol[i], Is.EqualTo(OSymbol[i]), "well-known symbols values are shared by all realms");
      }
    }

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(Symbol, "Symbol", 0, Symbol.Prototype);
      That(GlobalThis, Has.OwnProperty("Symbol", Symbol, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("should return a unique value", () => {
        That(ctor.Call(_), Is.Not.EqualTo(ctor.Call(_)));
        That(ctor.Call(_, ""), Is.Not.EqualTo(ctor.Call(_, "")));
        That(ctor.Call(_, "x"), Is.Not.EqualTo(ctor.Call(_, "x")));
        That(ctor.Call(_, Null), Is.Not.EqualTo(ctor.Call(_, Null)));
      });

      It("should coerce first argument to a string value", () => {
        That(() => ctor.Call(_, ctor.Call(_, "1")), Throws.TypeError);

        Logs.Clear();
        ctor.Call(_, CreateObject(toString: Intercept(() => Object.Construct(), "toString"), valueOf: Intercept(() => Undefined, "valueOf")));
        That(Logs, Is.EquivalentTo(new[] { "toString", "valueOf" }));

        Logs.Clear();
        ctor.Call(_, CreateObject(toString: Intercept(() => Undefined, "toString"), valueOf: Intercept(() => Undefined, "valueOf")));
        That(Logs, Is.EquivalentTo(new[] { "toString" }));

        Logs.Clear();
        ctor.Call(_, CreateObject(new { toString = Null, valueOf = Intercept(() => Undefined, "valueOf") }));
        That(Logs, Is.EquivalentTo(new[] { "valueOf" }));

        Logs.Clear();
        That(() => ctor.Call(_, CreateObject(new { toString = Null, valueOf = Intercept(() => Object.Construct(), "valueOf") })), Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "valueOf" }));

        Logs.Clear();
        That(() => ctor.Call(_, CreateObject(toString: Intercept(() => Object.Construct(), "toString"), valueOf: Intercept(() => Object.Construct(), "valueOf"))), Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "toString", "valueOf" }));
      });

      It("should not be invoked with new", () => {
        That(() => ctor.Construct(), Throws.TypeError);
        That(() => ctor.Construct(""), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void For(RuntimeFunction @for) {
      IsUnconstructableFunctionWLength(@for, "for", 1);

      It("should return same symbol using Symbol.for", () => {
        EcmaValue canonical = @for.Call(_, "s");
        Case((_, "s"), canonical);
        That(Symbol.Call(_, "s"), Is.Not.EqualTo(canonical));
        That(Symbol.Call(_, "s"), Is.Not.EqualTo(@for.Call(_, "y")));
      });

      It("should return same symbol across realms", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue OSymbol = realm.GetRuntimeObject(WellKnownObject.SymbolConstructor);
        EcmaValue parent = Symbol.Invoke("for", "parent");
        EcmaValue child = OSymbol.Invoke("for", "child");
        That(Symbol["for"], Is.Not.EqualTo(OSymbol["for"]));
        That(parent, Is.EqualTo(OSymbol.Invoke("for", "parent")));
        That(child, Is.EqualTo(Symbol.Invoke("for", "child")));
      });

      It("should coerce first argument to a string value", () => {
        Case((_, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
        Case((_, Symbol.Call(_)), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void KeyFor(RuntimeFunction keyFor) {
      IsUnconstructableFunctionWLength(keyFor, "keyFor", 1);

      It("should throw a TypeError if argument is not symbol", () => {
        Case((_, Undefined), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, 1), Throws.TypeError);
        Case((_, true), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, Object.Construct()), Throws.TypeError);
        Case((_, Array.Construct()), Throws.TypeError);
        Case((_, Object.Call(Object, new Symbol())), Throws.TypeError);
      });

      It("should return the description string if hit or undefined if missed", () => {
        EcmaValue canonical = Symbol.Invoke("for", "s");
        Case((_, canonical), "s");
        Case((_, Symbol.Call(_)), Undefined);
        Case((_, Symbol["iterator"]), Undefined);
      });

      It("should allow cross-realm lookup since global symbol registry is shared by all realms", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue OSymbol = realm.GetRuntimeObject(WellKnownObject.SymbolConstructor);
        EcmaValue parent = Symbol.Invoke("for", "parent");
        EcmaValue child = OSymbol.Invoke("for", "child");
        That(Symbol["keyFor"], Is.Not.EqualTo(OSymbol["keyFor"]));
        That(OSymbol.Invoke("keyFor", parent), Is.EqualTo("parent"));
        That(Symbol.Invoke("keyFor", child), Is.EqualTo("child"));
      });
    }
  }
}
