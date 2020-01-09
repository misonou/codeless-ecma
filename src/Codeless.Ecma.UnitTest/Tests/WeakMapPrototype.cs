using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class WeakMapPrototype : TestBase {
    [Test]
    public void Properties() {
      That(WeakMap.Prototype, UnitTest.Has.OwnProperty("constructor", WeakMap, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(WeakMap.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(WeakMap.Prototype), Is.EqualTo("[object WeakMap]"), "WeakMap prototype object: its [[Class]] must be 'WeakMap'");

      That(WeakMap.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.ToStringTag, "WeakMap", EcmaPropertyAttributes.Configurable));
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

      It("should throw a TypeError when this object has no [[WeakMapData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Global.Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakMap.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should return true if an entry is deleted", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue map = WeakMap.Construct(EcmaArray.Of(EcmaArray.Of(foo, 42)));
        Case((map, foo), true);
        That(map.Invoke("has", foo), Is.EqualTo(false));

        map = WeakMap.Construct();
        map.Invoke("set", foo, 42);
        Case((map, foo), true);
        That(map.Invoke("has", foo), Is.EqualTo(false));
      });

      It("should return false if value is not object", () => {
        EcmaValue map = WeakMap.Construct();
        Case((map, Undefined), false);
        Case((map, Null), false);
        Case((map, 1), false);
        Case((map, true), false);
        Case((map, ""), false);
        Case((map, new Symbol()), false);
      });

      It("should return false if delete is noop", () => {
        EcmaValue map = WeakMap.Construct();
        Case((map, Object.Construct()), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Get(RuntimeFunction get) {
      IsUnconstructableFunctionWLength(get, "get", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, Object.Construct()), Throws.TypeError);
        Case((Null, Object.Construct()), Throws.TypeError);
        Case((false, Object.Construct()), Throws.TypeError);
        Case((0, Object.Construct()), Throws.TypeError);
        Case(("", Object.Construct()), Throws.TypeError);
        Case((new Symbol(), Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[WeakMapData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Global.Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakMap.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should return value from the specified key", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue bar = Object.Construct();
        EcmaValue baz = Array.Construct();
        EcmaValue map = WeakMap.Construct(EcmaArray.Of(EcmaArray.Of(foo, 0)));
        Case((map, foo), 0);
        map.Invoke("set", bar, 1);
        Case((map, bar), 1);
        map.Invoke("set", baz, 2);
        Case((map, baz), 2);
        map.Invoke("set", foo, 3);
        Case((map, foo), 3);
      });

      It("should return undefined if key is not on the WeakMap object", () => {
        EcmaValue key = Object.Construct();
        EcmaValue map = WeakMap.Construct();
        Case((map, key), Undefined);
        map.Invoke("set", key, 1);
        map.Invoke("set", Object.Construct(), 2);
        map.Invoke("delete", key);
        map.Invoke("set", Object.Construct(), 3);
        Case((map, key), Undefined);
      });

      It("should return undefined if key is not object", () => {
        EcmaValue map = WeakMap.Construct();
        Case((map, Undefined), Undefined);
        Case((map, Null), Undefined);
        Case((map, 1), Undefined);
        Case((map, true), Undefined);
        Case((map, ""), Undefined);
        Case((map, new Symbol()), Undefined);
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

      It("should throw a TypeError when this object has no [[WeakMapData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Global.Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakMap.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should return true when key is present in the WeakMap entries list", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue map = WeakMap.Construct();
        map.Invoke("set", foo, 1);
        Case((map, foo), true);
      });

      It("should return undefined if key is not on the WeakMap object", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue bar = Object.Construct();
        EcmaValue map = WeakMap.Construct();
        Case((map, foo), false);
        map.Invoke("set", foo, 1);
        Case((map, bar), false);
        map.Invoke("delete", foo);
        Case((map, foo), false);
      });

      It("should return undefined if key is not object", () => {
        EcmaValue map = WeakMap.Construct();
        Case((map, Undefined), false);
        Case((map, Null), false);
        Case((map, 1), false);
        Case((map, true), false);
        Case((map, ""), false);
        Case((map, new Symbol()), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Set(RuntimeFunction set) {
      IsUnconstructableFunctionWLength(set, "set", 2);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, Object.Construct()), Throws.TypeError);
        Case((Null, Object.Construct()), Throws.TypeError);
        Case((false, Object.Construct()), Throws.TypeError);
        Case((0, Object.Construct()), Throws.TypeError);
        Case(("", Object.Construct()), Throws.TypeError);
        Case((new Symbol(), Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[WeakMapData]] internal slot", () => {
        Case((EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((Map.Construct(), Object.Construct()), Throws.TypeError);
        Case((Object.Construct(), Object.Construct()), Throws.TypeError);
        Case((Global.Set.Construct(), Object.Construct()), Throws.TypeError);
        Case((WeakMap.Prototype, Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError when key is not object", () => {
        EcmaValue map = WeakMap.Construct();
        Case((map, Null, 1), Throws.TypeError);
        Case((map, Undefined, 1), Throws.TypeError);
        Case((map, 1, 1), Throws.TypeError);
        Case((map, false, 1), Throws.TypeError);
        Case((map, "string", 1), Throws.TypeError);
        Case((map, new Symbol(), 1), Throws.TypeError);
      });

      It("should return this", () => {
        EcmaValue foo = Object.Construct();
        EcmaValue map = WeakMap.Construct();
        Case((map, foo, 1), map);
        Case((map, foo, 1), map);
      });

      It("should append value as the last element of entries", () => {
        EcmaValue map = WeakMap.Construct();
        EcmaValue foo = Object.Construct();
        EcmaValue bar = Object.Construct();
        EcmaValue baz = Object.Construct();

        map.Invoke("set", foo, 1);
        map.Invoke("set", bar, 2);
        map.Invoke("set", baz, 3);

        That(map.Invoke("has", foo), Is.EqualTo(true));
        That(map.Invoke("has", bar), Is.EqualTo(true));
        That(map.Invoke("has", baz), Is.EqualTo(true));
      });
    }
  }
}
