using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class MapPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Map.Prototype, UnitTest.Has.OwnProperty("constructor", Map, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Map.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Map.Prototype), Is.EqualTo("[object Map]"), "Map prototype object: its [[Class]] must be 'Map'");

      That(Map.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.Iterator, Map.Prototype.Get("entries"), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Map.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.ToStringTag, "Map", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Clear(RuntimeFunction clear) {
      IsUnconstructableFunctionWLength(clear, "clear", 0);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should clear a Map object", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("foo", "bar"),
          EcmaArray.Of(1, 1)
        ));
        clear.Call(map);
        That(map["size"], Is.EqualTo(0));

        map = Map.Construct();
        map.Invoke("set", "foo", "bar");
        map.Invoke("set", 1, 1);
        clear.Call(map);
        That(map["size"], Is.EqualTo(0));
      });

      It("should return undefined", () => {
        EcmaValue map = Map.Construct();
        Case(map, Undefined);
        Case(map, Undefined);
      });

      It("should not break iterator", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("a", 1),
          EcmaArray.Of("b", 2),
          EcmaArray.Of("c", 3)
        ));
        EcmaValue iterator = map.Invoke("entries");
        iterator.Invoke("next");
        clear.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Delete(RuntimeFunction delete) {
      IsUnconstructableFunctionWLength(delete, "delete", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((0, 1), Throws.TypeError);
        Case(("", 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case((EcmaArray.Of(), 1), Throws.TypeError);
        Case((Map.Prototype, 1), Throws.TypeError);
        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Global.Set.Construct(), 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1), Throws.TypeError);
      });

      It("should return true when an entry is deleted", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("a", 1),
          EcmaArray.Of("b", 2)
        ));
        Case((map, "a"), true);
        That(map["size"], Is.EqualTo(1));
      });

      It("should return false when delete is noop", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("a", 1),
          EcmaArray.Of("b", 2)
        ));
        Case((map, "not-in-the-map"), false);
        Case((map, "a"), true);
        Case((map, "a"), false);
      });

      It("should not break iterator", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("a", 1),
          EcmaArray.Of("b", 2),
          EcmaArray.Of("c", 3)
        ));
        EcmaValue iterator = map.Invoke("entries");
        iterator.Invoke("next");
        delete.Call(map, "b");
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { "c", 3 });
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Entries(RuntimeFunction entries) {
      IsUnconstructableFunctionWLength(entries, "entries", 0);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return iterator", () => {
        EcmaValue map = Map.Construct();
        EcmaValue iterator = entries.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), true);

        map.Invoke("set", "a", 1);
        map.Invoke("set", "b", 2);
        map.Invoke("set", "c", 3);
        iterator = entries.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { "a", 1 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { "b", 2 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { "c", 3 });
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ForEach(RuntimeFunction forEach) {
      IsUnconstructableFunctionWLength(forEach, "forEach", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Null, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((false, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((0, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case(("", RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((new Symbol(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case((EcmaArray.Of(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Map.Prototype, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Object.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Global.Set.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((WeakSet.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
      });

      It("should throw a TypeError when callback is not callable", () => {
        EcmaValue map = Map.Construct();
        Case((map, Undefined), Throws.TypeError);
        Case((map, Null), Throws.TypeError);
        Case((map, false), Throws.TypeError);
        Case((map, 0), Throws.TypeError);
        Case((map, ""), Throws.TypeError);
        Case((map, new Symbol()), Throws.TypeError);
      });

      It("should iterate in insertion order", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of("foo", "valid foo"),
          EcmaArray.Of("bar", false),
          EcmaArray.Of("baz", "valid baz")
        ));
        map.Invoke("set", 0, false);
        map.Invoke("set", 1, false);
        map.Invoke("set", 2, "valid 2");
        map.Invoke("delete", 1);
        map.Invoke("delete", "bar");
        map.Invoke("set", 0, "valid 0");

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, s) => list.Add(new[] { v, e, s })));
        That(list, Is.EquivalentTo(new[] {
          new[] { "valid foo", "foo", map },
          new[] { "valid baz", "baz", map },
          new[] { "valid 2", 2, map },
          new[] { "valid 0", 0, map }
        }));
      });

      It("should iterate values added after foreach begins", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", "foo", 0);
        map.Invoke("set", "bar", 1);

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, m) => {
          if (m["size"] == 2) {
            m.Invoke("set", "baz", 2);
          }
          list.Add(new[] { v, e, m });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 0, "foo", map },
          new[] { 1, "bar", map },
          new[] { 2, "baz", map }
        }));
      });

      It("should iterate values deleted then re-added", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", "foo", 0);
        map.Invoke("set", "bar", 1);

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, m) => {
          if (v == 0) {
            m.Invoke("delete", "foo");
            m.Invoke("set", "foo", 2);
          }
          list.Add(new[] { v, e, m });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 0, "foo", map },
          new[] { 1, "bar", map },
          new[] { 2, "foo", map }
        }));
      });

      It("should iterate values not deleted", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", "foo", 0);
        map.Invoke("set", "bar", 1);

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, m) => {
          if (v == 0) {
            m.Invoke("delete", "bar");
          }
          list.Add(new[] { v, e, m });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 0, "foo", map }
        }));
      });

      It("should return undefined", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(EcmaArray.Of(1, 1)));
        Case((map, RuntimeFunction.Create(v => v)), Undefined);
      });

      It("should invoke callback with thisArg", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(EcmaArray.Of(1, 1)));
        EcmaValue thisArg = Object.Construct();
        EcmaValue thisValue = Undefined;
        map.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          thisValue = This;
        }), thisArg);
        That(thisValue, Is.EqualTo(thisArg));
      });

      It("should invoke callback with undefined if thisArg is not provided", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(EcmaArray.Of(1, 1)));
        EcmaValue thisValue = Object.Construct();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          thisValue = This;
        }));
        That(thisValue, Is.EqualTo(Undefined));
      });

      It("should throw if callback throws", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of(1, 1),
          EcmaArray.Of(2, 2)
        ));
        Logs.Clear();
        Case((map, Intercept(ThrowTest262Exception)), Throws.Test262);
        That(Logs, NUnit.Framework.Has.Exactly(1).Items);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Get(RuntimeFunction get) {
      IsUnconstructableFunctionWLength(get, "get", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((0, 1), Throws.TypeError);
        Case(("", 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case((EcmaArray.Of(), 1), Throws.TypeError);
        Case((Map.Prototype, 1), Throws.TypeError);
        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Global.Set.Construct(), 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1), Throws.TypeError);
      });

      It("should return undefined if key is not on the Map object", () => {
        EcmaValue map = Map.Construct();
        Case((map, "item"), Undefined);
        map.Invoke("set", "item", 1);
        map.Invoke("delete", "item");
        Case((map, "item"), Undefined);
      });

      It("should return value from the specified key", () => {
        EcmaValue obj = Object.Construct();
        EcmaValue sym = new Symbol();
        EcmaValue map = Map.Construct();
        map.Invoke("set", "bar", 0);
        map.Invoke("set", 1, 42);
        map.Invoke("set", NaN, 1);
        map.Invoke("set", "bar", 0);
        map.Invoke("set", obj, 2);
        map.Invoke("set", sym, 4);
        map.Invoke("set", Null, 5);
        map.Invoke("set", Undefined, 6);

        Case((map, "bar"), 0);
        Case((map, 1), 42);
        Case((map, NaN), 1);
        Case((map, "bar"), 0);
        Case((map, obj), 2);
        Case((map, sym), 4);
        Case((map, Null), 5);
        Case((map, Undefined), 6);
      });

      It("should treat +0 and -0 as the same key", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", 0, 42);
        Case((map, -0d), 42);

        map.Invoke("clear");
        map.Invoke("set", -0d, 43);
        Case((map, 0), 43);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Has(RuntimeFunction has) {
      IsUnconstructableFunctionWLength(has, "has", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return true if key is on the Map object", () => {
        EcmaValue map = Map.Construct();
        EcmaValue obj = Object.Construct();
        EcmaValue arr = Array.Construct();
        EcmaValue sym = new Symbol();

        map.Invoke("set", "str", Undefined);
        map.Invoke("set", 1, Undefined);
        map.Invoke("set", NaN, Undefined);
        map.Invoke("set", true, Undefined);
        map.Invoke("set", false, Undefined);
        map.Invoke("set", obj, Undefined);
        map.Invoke("set", arr, Undefined);
        map.Invoke("set", sym, Undefined);
        map.Invoke("set", Null, Undefined);
        map.Invoke("set", Undefined, Undefined);

        Case((map, "str"), true);
        Case((map, 1), true);
        Case((map, NaN), true);
        Case((map, true), true);
        Case((map, false), true);
        Case((map, obj), true);
        Case((map, arr), true);
        Case((map, sym), true);
        Case((map, Null), true);
        Case((map, Undefined), true);
      });

      It("should return undefined if key is not object", () => {
        EcmaValue map = Map.Construct();
        Case((map, Undefined), false);
        Case((map, Null), false);
        Case((map, 1), false);
        Case((map, true), false);
        Case((map, ""), false);
        Case((map, new Symbol()), false);
      });

      It("should treat +0 and -0 as the same key", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", 0, 42);
        Case((map, -0d), true);

        map.Invoke("clear");
        map.Invoke("set", -0d, 43);
        Case((map, 0), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Keys(RuntimeFunction keys) {
      IsUnconstructableFunctionWLength(keys, "keys", 0);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return iterator", () => {
        EcmaValue map = Map.Construct();
        EcmaValue iterator = keys.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), true);

        EcmaValue obj = Object.Construct();
        map.Invoke("set", "foo", 1);
        map.Invoke("set", obj, 2);
        map.Invoke("set", map, 3);
        iterator = keys.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), false, "foo");
        VerifyIteratorResult(iterator.Invoke("next"), false, obj);
        VerifyIteratorResult(iterator.Invoke("next"), false, map);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Set(RuntimeFunction set) {
      IsUnconstructableFunctionWLength(set, "set", 2);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, 1, 1), Throws.TypeError);
        Case((Null, 1, 1), Throws.TypeError);
        Case((false, 1, 1), Throws.TypeError);
        Case((0, 1, 1), Throws.TypeError);
        Case(("", 1, 1), Throws.TypeError);
        Case((new Symbol(), 1, 1), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case((EcmaArray.Of(), 1, 1), Throws.TypeError);
        Case((Map.Prototype, 1, 1), Throws.TypeError);
        Case((Object.Construct(), 1, 1), Throws.TypeError);
        Case((Global.Set.Construct(), 1, 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1, 1), Throws.TypeError);
      });

      It("should return this", () => {
        EcmaValue map = Map.Construct();
        Case((map, 1, 1), map);
        Case((map, 1, 1), map);
      });

      It("should append value as the last element of entries", () => {
        EcmaValue sym = new Symbol();
        EcmaValue map = Map.Construct(EcmaArray.Of(
          EcmaArray.Of(4, 4),
          EcmaArray.Of("foo3", 3),
          EcmaArray.Of(sym, 2)
        ));
        map.Invoke("set", Null, 42);
        map.Invoke("set", 1, "valid");
        That(map["size"], Is.EqualTo(5));

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        map.Invoke("forEach", RuntimeFunction.Create((v, e, m) => list.Add(new[] { v, e, m })));
        That(list, Is.EquivalentTo(new[] {
          new[] { 4, 4, map },
          new[] { 3, "foo3", map },
          new[] { 2, sym, map },
          new[] { 42, Null, map },
          new[] { "valid", 1, map }
        }));
      });

      It("should replace a value in the map", () => {
        EcmaValue map = Map.Construct(EcmaArray.Of(EcmaArray.Of("item", 1)));
        set.Call(map, "item", 42);
        That(map.Invoke("get", "item"), Is.EqualTo(42));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Size(RuntimeFunction size) {
      IsUnconstructableFunctionWLength(size, "get size", 0);
      That(Map.Prototype, UnitTest.Has.OwnProperty("size", EcmaPropertyAttributes.Configurable));
      That(Map.Prototype.GetOwnProperty("size").Set, Is.Undefined);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return count of present values", () => {
        EcmaValue map = Map.Construct();
        Case(map, 0);
        map.Invoke("set", 1, 1);
        Case(map, 1);
        map.Invoke("delete", 1);
        Case(map, 0);

        map.Invoke("set", 0);
        map.Invoke("set", Undefined);
        map.Invoke("set", false);
        map.Invoke("set", NaN);
        map.Invoke("set", Null);
        map.Invoke("set", "");
        map.Invoke("set", new Symbol());
        Case(map, 7);

        Case(Map.Construct(EcmaArray.Of(
         EcmaArray.Of(0, Undefined),
         EcmaArray.Of(Undefined, Undefined),
         EcmaArray.Of(false, Undefined),
         EcmaArray.Of(NaN, Undefined),
         EcmaArray.Of(Null, Undefined),
         EcmaArray.Of("", Undefined),
         EcmaArray.Of(new Symbol(), Undefined)
        )), 7);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Values(RuntimeFunction values) {
      IsUnconstructableFunctionWLength(values, "values", 0);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[MapData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Global.Set.Construct(), Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return iterator", () => {
        EcmaValue map = Map.Construct();
        EcmaValue iterator = values.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), true);

        EcmaValue obj = Object.Construct();
        map.Invoke("set", "foo", 1);
        map.Invoke("set", obj, 2);
        map.Invoke("set", map, 3);
        iterator = values.Call(map);
        VerifyIteratorResult(iterator.Invoke("next"), false, 1);
        VerifyIteratorResult(iterator.Invoke("next"), false, 2);
        VerifyIteratorResult(iterator.Invoke("next"), false, 3);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
