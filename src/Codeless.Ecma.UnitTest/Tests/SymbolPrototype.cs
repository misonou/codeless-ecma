using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SymbolPrototype : TestBase {
    RuntimeFunction Symbol => Global.Symbol;

    [Test]
    public void Properties() {
      That(Symbol.Prototype, Has.OwnProperty("constructor", Symbol, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Symbol.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Symbol.Prototype), Is.EqualTo("[object Symbol]"), "Symbol prototype object: its [[Class]] must be 'Symbol'");

      That(Symbol.Prototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Symbol", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Description(RuntimeFunction description) {
      IsUnconstructableFunctionWLength(description, "get description", 0);
      That(Symbol.Prototype, UnitTest.Has.OwnProperty("description", EcmaPropertyAttributes.Configurable));
      That(Symbol.Prototype.GetOwnProperty("description").Set, Is.Undefined);

      EcmaValue sym = Symbol.Call(_, "foo");
      That(sym["description"], Is.EqualTo("foo"));
      That(sym.Invoke("toString"), Is.EqualTo("Symbol(foo)"));
      That(sym.Invoke("hasOwnProperty", "description"), Is.EqualTo(false), "'description' is not an own property of Symbols");

      Case(Symbol.Call(_), Undefined);
      Case(Symbol.Call(_, Undefined), Undefined);
      Case(Symbol.Call(_, Null), "null");
      Case(Symbol.Call(_, ""), "");
      Case(Object.Call(Object, sym), "foo");

      Case(Undefined, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(123, Throws.TypeError);
      Case("test", Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(Object.Construct(), Throws.TypeError);
      Case(Proxy.Construct(Object.Construct(), Object.Construct()), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void ToPrimitive(RuntimeFunction toPrimitive) {
      IsUnconstructableFunctionWLength(toPrimitive, "[Symbol.toPrimitive]", 1);
      That(Symbol.Prototype, Has.OwnProperty(WellKnownSymbol.ToPrimitive, EcmaPropertyAttributes.Configurable));

      Case(Symbol["toPrimitive"], Symbol["toPrimitive"]);
      Case(Object.Call(Object, Symbol["toPrimitive"]), Symbol["toPrimitive"]);

      Case(Undefined, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(123, Throws.TypeError);
      Case("test", Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(Object.Construct(), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);

      Case(Symbol.Call(_), "Symbol()");
      Case(Symbol.Call(_, Undefined), "Symbol()");
      Case(Symbol.Call(_, Null), "Symbol(null)");
      Case(Symbol.Call(_, ""), "Symbol()");
      Case(Symbol.Call(_, "66"), "Symbol(66)");
      Case(Object.Call(Object, Symbol.Call(_, "66")), "Symbol(66)");

      Case(Undefined, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(123, Throws.TypeError);
      Case("test", Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(Object.Construct(), Throws.TypeError);
      Case(Proxy.Construct(Object.Construct(), Object.Construct()), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);

      Case(Symbol["toPrimitive"], Symbol["toPrimitive"]);
      Case(Object.Call(Object, Symbol["toPrimitive"]), Symbol["toPrimitive"]);

      Case(Undefined, Throws.TypeError);
      Case(Null, Throws.TypeError);
      Case(123, Throws.TypeError);
      Case("test", Throws.TypeError);
      Case(true, Throws.TypeError);
      Case(Object.Construct(), Throws.TypeError);
    }
  }
}
