using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SetPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Set.Prototype, UnitTest.Has.OwnProperty("constructor", Set, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Set.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Set.Prototype), Is.EqualTo("[object Set]"), "Set prototype object: its [[Class]] must be 'Set'");

      That(Set.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.Iterator, Set.Prototype.Get("values"), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Set.Prototype, UnitTest.Has.OwnProperty(WellKnownSymbol.ToStringTag, "Set", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Add(RuntimeFunction add) {
      IsUnconstructableFunctionWLength(add, "add", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((0, 1), Throws.TypeError);
        Case(("", 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case((EcmaArray.Of(), 1), Throws.TypeError);
        Case((Map.Construct(), 1), Throws.TypeError);
        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Set.Prototype, 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1), Throws.TypeError);
      });

      It("should return this", () => {
        EcmaValue set = Set.Construct();
        Case((set, 1), set);
        Case((set, 1), set);
      });

      It("should preserve insertion order", () => {
        EcmaValue set = Set.Construct();
        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);

        List<EcmaValue> list = new List<EcmaValue>();
        set.Invoke("forEach", RuntimeFunction.Create(v => list.Add(v)));
        That(list, Is.EquivalentTo(new[] { 1, 2, 3 }));
      });

      It("should not add duplicate entry", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        That(set["size"], Is.EqualTo(1));
        set.Invoke("add", 1);
        That(set["size"], Is.EqualTo(1));

        set = Set.Construct();
        That(set["size"], Is.EqualTo(0));
        set.Invoke("add", 1);
        set.Invoke("add", 1);
        That(set["size"], Is.EqualTo(1));
      });

      It("should treat +0 and -0 as the same value", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(-0d));
        That(set["size"], Is.EqualTo(1));
        set.Invoke("add", -0d);
        That(set["size"], Is.EqualTo(1));
        set.Invoke("add", 0);
        That(set["size"], Is.EqualTo(1));
      });
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

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Set.Prototype, Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should clear all contents", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        That(set["size"], Is.EqualTo(3));
        clear.Call(set);
        That(set["size"], Is.EqualTo(0));
        That(set.Invoke("has", 1), Is.EqualTo(false));
        That(set.Invoke("has", 2), Is.EqualTo(false));
        That(set.Invoke("has", 3), Is.EqualTo(false));

        set = Set.Construct();
        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);
        That(set["size"], Is.EqualTo(3));
        clear.Call(set);
        That(set["size"], Is.EqualTo(0));
        That(set.Invoke("has", 1), Is.EqualTo(false));
        That(set.Invoke("has", 2), Is.EqualTo(false));
        That(set.Invoke("has", 3), Is.EqualTo(false));
      });

      It("should return undefined", () => {
        EcmaValue set = Set.Construct();
        Case(set, Undefined);
        Case(set, Undefined);
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

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case((EcmaArray.Of(), 1), Throws.TypeError);
        Case((Map.Construct(), 1), Throws.TypeError);
        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Set.Prototype, 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1), Throws.TypeError);
      });

      It("should return true when an entry is deleted", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        That(set["size"], Is.EqualTo(1));
        Case((set, 1), true);
        That(set["size"], Is.EqualTo(0));

        set = Set.Construct();
        That(set["size"], Is.EqualTo(0));
        set.Invoke("add", 1);
        That(set["size"], Is.EqualTo(1));
        Case((set, 1), true);
        That(set["size"], Is.EqualTo(0));
      });

      It("should treat +0 and -0 as the same value", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(-0d));
        That(set["size"], Is.EqualTo(1));
        Case((set, 0), true);
        That(set["size"], Is.EqualTo(0));
      });

      It("should return true when delete operation occurs", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        Case((set, 1), true);
      });

      It("should return false when delete is noop", () => {
        EcmaValue set = Set.Construct();
        Case((set, 1), false);
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

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Set.Prototype, Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return iterator", () => {
        EcmaValue set = Set.Construct();
        EcmaValue iterator = entries.Call(set);
        VerifyIteratorResult(iterator.Invoke("next"), true);

        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);
        iterator = entries.Call(set);
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 1, 1 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 2, 2 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 3, 3 });
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

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case((EcmaArray.Of(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Map.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Object.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((Set.Prototype, RuntimeFunction.Create(_ => _)), Throws.TypeError);
        Case((WeakSet.Construct(), RuntimeFunction.Create(_ => _)), Throws.TypeError);
      });

      It("should throw a TypeError when callback is not callable", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        Case((set, Undefined), Throws.TypeError);
        Case((set, Null), Throws.TypeError);
        Case((set, false), Throws.TypeError);
        Case((set, 0), Throws.TypeError);
        Case((set, ""), Throws.TypeError);
        Case((set, new Symbol()), Throws.TypeError);
      });

      It("should iterate in insertion order", () => {
        EcmaValue set = Set.Construct();
        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);

        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => list.Add(new[] { v, e, s })));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 2, 2, set },
          new[] { 3, 3, set }
        }));
      });

      It("should iterate in entry order", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => list.Add(new[] { v, e, s })));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 2, 2, set },
          new[] { 3, 3, set }
        }));
      });

      It("should iterate values added after foreach begins", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          if (v == 1) {
            s.Invoke("add", 2);
          }
          if (v == 2) {
            s.Invoke("add", 3);
          }
          list.Add(new[] { v, e, s });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 2, 2, set },
          new[] { 3, 3, set }
        }));
      });

      It("should iterate values deleted then re-added", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          if (v == 1) {
            s.Invoke("delete", 2);
          }
          if (v == 3) {
            s.Invoke("add", 2);
          }
          list.Add(new[] { v, e, s });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 3, 3, set },
          new[] { 2, 2, set }
        }));
      });

      It("should iterate values not deleted", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("delete", 2);
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => list.Add(new[] { v, e, s })));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 3, 3, set }
        }));
      });

      It("should iterate values revisits after delete re-add", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        List<EcmaValue[]> list = new List<EcmaValue[]>();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          if (v == 2) {
            s.Invoke("delete", 1);
          }
          if (v == 3) {
            s.Invoke("add", 1);
          }
          list.Add(new[] { v, e, s });
        }));
        That(list, Is.EquivalentTo(new[] {
          new[] { 1, 1, set },
          new[] { 3, 3, set },
          new[] { 2, 2, set },
          new[] { 1, 1, set }
        }));
      });

      It("should return undefined", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        Case((set, RuntimeFunction.Create(v => v)), Undefined);
      });

      It("should invoke callback with thisArg", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        EcmaValue thisArg = Object.Construct();
        EcmaValue thisValue = Undefined;
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          thisValue = This;
        }), thisArg);
        That(thisValue, Is.EqualTo(thisArg));
      });

      It("should invoke callback with undefined if thisArg is not provided", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1));
        EcmaValue thisValue = Object.Construct();
        set.Invoke("forEach", RuntimeFunction.Create((v, e, s) => {
          thisValue = This;
        }));
        That(thisValue, Is.EqualTo(Undefined));
      });

      It("should throw if callback throws", () => {
        EcmaValue set = Set.Construct(EcmaArray.Of(1, 2, 3));
        Logs.Clear();
        Case((set, Intercept(ThrowTest262Exception)), Throws.Test262);
        That(Logs, NUnit.Framework.Has.Exactly(1).Items);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Has(RuntimeFunction has) {
      IsUnconstructableFunctionWLength(has, "has", 1);

      It("should throw a TypeError when this is not an object", () => {
        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((false, 1), Throws.TypeError);
        Case((0, 1), Throws.TypeError);
        Case(("", 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case((EcmaArray.Of(), 1), Throws.TypeError);
        Case((Map.Construct(), 1), Throws.TypeError);
        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Set.Prototype, 1), Throws.TypeError);
        Case((WeakSet.Construct(), 1), Throws.TypeError);
      });

      It("should return true if value presents", () => {
        EcmaValue obj = new EcmaObject();
        Symbol sym = new Symbol();
        Case((Set.Construct(EcmaArray.Of(Undefined)), Undefined), true);
        Case((Set.Construct(EcmaArray.Of(Null)), Null), true);
        Case((Set.Construct(EcmaArray.Of(NaN)), NaN), true);
        Case((Set.Construct(EcmaArray.Of(0)), 0), true);
        Case((Set.Construct(EcmaArray.Of(0)), -0d), true);
        Case((Set.Construct(EcmaArray.Of(true)), true), true);
        Case((Set.Construct(EcmaArray.Of("")), ""), true);
        Case((Set.Construct(EcmaArray.Of(sym)), sym), true);
        Case((Set.Construct(EcmaArray.Of(obj)), obj), true);
      });

      It("should return false if value is not present", () => {
        EcmaValue obj = new EcmaObject();
        Symbol sym = new Symbol();
        Case((Set.Construct(), Undefined), false);
        Case((Set.Construct(), Null), false);
        Case((Set.Construct(), NaN), false);
        Case((Set.Construct(), 0), false);
        Case((Set.Construct(), -0d), false);
        Case((Set.Construct(), true), false);
        Case((Set.Construct(), ""), false);
        Case((Set.Construct(), sym), false);
        Case((Set.Construct(), obj), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Keys(RuntimeFunction keys) {
      That(keys, Is.EqualTo(Set.Prototype.Get("values")), "The value of Set.prototype.keys is Set.prototype.values");
    }

    [Test, RuntimeFunctionInjection]
    public void Size(RuntimeFunction size) {
      IsUnconstructableFunctionWLength(size, "get size", 0);
      That(Set.Prototype, UnitTest.Has.OwnProperty("size", EcmaPropertyAttributes.Configurable));
      That(Set.Prototype.GetOwnProperty("size").Set, Is.Undefined);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Set.Prototype, Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return count of present values", () => {
        EcmaValue set = Set.Construct();
        Case(set, 0);
        set.Invoke("add", 0);
        Case(set, 1);
        set.Invoke("delete", 0);
        Case(set, 0);

        set.Invoke("add", 0);
        set.Invoke("add", Undefined);
        set.Invoke("add", false);
        set.Invoke("add", NaN);
        set.Invoke("add", Null);
        set.Invoke("add", "");
        set.Invoke("add", new Symbol());
        Case(set, 7);

        Case(Set.Construct(EcmaArray.Of(0, Undefined, false, NaN, Null, "", new Symbol())), 7);
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

      It("should throw a TypeError when this object has no [[SetData]] internal slot", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Map.Construct(), Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(Set.Prototype, Throws.TypeError);
        Case(WeakSet.Construct(), Throws.TypeError);
      });

      It("should return iterator", () => {
        EcmaValue set = Set.Construct();
        EcmaValue iterator = values.Call(set);
        VerifyIteratorResult(iterator.Invoke("next"), true);

        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);
        iterator = values.Call(set);
        VerifyIteratorResult(iterator.Invoke("next"), false, 1);
        VerifyIteratorResult(iterator.Invoke("next"), false, 2);
        VerifyIteratorResult(iterator.Invoke("next"), false, 3);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
