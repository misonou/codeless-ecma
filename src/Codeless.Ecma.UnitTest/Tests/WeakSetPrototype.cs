using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class WeakSetPrototype : TestBase {
    [Test]
    public void Properties() {
      That(WeakSet.Prototype, UnitTest.Has.OwnProperty("constructor", WeakSet, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(WeakSet.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(WeakSet.Prototype), Is.EqualTo("[object WeakSet]"), "WeakSet prototype object: its [[Class]] must be 'WeakSet'");

      That(WeakSet.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.ToStringTag, "WeakSet", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Add(RuntimeFunction add) {
      IsUnconstructableFunctionWLength(add, "add", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, Object.Construct()), Throws.TypeError);
        Case((Null, Object.Construct()), Throws.TypeError);
        Case((false, Object.Construct()), Throws.TypeError);
        Case((0, Object.Construct()), Throws.TypeError);
        Case(("", Object.Construct()), Throws.TypeError);
        Case((new Symbol(), Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[WeakSetData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakSet.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when value is not object", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Null), Throws.TypeError);
        Case((set, Undefined), Throws.TypeError);
        Case((set, 1), Throws.TypeError);
        Case((set, false), Throws.TypeError);
        Case((set, "string"), Throws.TypeError);
        Case((set, new Symbol()), Throws.TypeError);
      });

      It("should return this", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Object.Construct()), set);
        Case((set, Object.Construct()), set);
      });

      It("should append value as the last element of entries", () => {
        EcmaValue set = WeakSet.Construct();
        EcmaValue foo = Object.Construct();
        EcmaValue bar = Object.Construct();
        EcmaValue baz = Object.Construct();

        set.Invoke("add", foo);
        set.Invoke("add", bar);
        set.Invoke("add", baz);

        That(set.Invoke("has", foo), Is.EqualTo(true));
        That(set.Invoke("has", bar), Is.EqualTo(true));
        That(set.Invoke("has", baz), Is.EqualTo(true));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Delete(RuntimeFunction delete) {
      IsUnconstructableFunctionWLength(delete, "delete", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, Object.Construct()), Throws.TypeError);
        Case((Null, Object.Construct()), Throws.TypeError);
        Case((false, Object.Construct()), Throws.TypeError);
        Case((0, Object.Construct()), Throws.TypeError);
        Case(("", Object.Construct()), Throws.TypeError);
        Case((new Symbol(), Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[WeakSetData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakSet.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should return true if value is an element of entries", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue set = WeakSet.Construct(EcmaArray.Of(foo));
        Case((set, foo), true);
        That(set.Invoke("has", foo), Is.EqualTo(false));

        set = WeakSet.Construct();
        set.Invoke("add", foo);
        Case((set, foo), true);
        That(set.Invoke("has", foo), Is.EqualTo(false));
      });

      It("should return false if value is not object", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Undefined), false);
        Case((set, Null), false);
        Case((set, 1), false);
        Case((set, true), false);
        Case((set, ""), false);
        Case((set, new Symbol()), false);
      });

      It("should return false if delete is noop", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Object.Construct()), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Has(RuntimeFunction has) {
      IsUnconstructableFunctionWLength(has, "has", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, Object.Construct()), Throws.TypeError);
        Case((Null, Object.Construct()), Throws.TypeError);
        Case((false, Object.Construct()), Throws.TypeError);
        Case((0, Object.Construct()), Throws.TypeError);
        Case(("", Object.Construct()), Throws.TypeError);
        Case((new Symbol(), Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[WeakSetData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakSet.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should return true if value is an element of entries", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue set = WeakSet.Construct(EcmaArray.Of(foo));
        Case((set, foo), true);

        set = WeakSet.Construct();
        set.Invoke("add", foo);
        Case((set, foo), true);
      });

      It("should return false if value is not object", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Undefined), false);
        Case((set, Null), false);
        Case((set, 1), false);
        Case((set, true), false);
        Case((set, ""), false);
        Case((set, new Symbol()), false);
      });

      It("should return false if value is not an element of entries", () => {
        EcmaValue set = WeakSet.Construct();
        Case((set, Object.Construct()), false);
      });
    }
  }
}
