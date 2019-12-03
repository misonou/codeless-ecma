using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class MapConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Map", 0, Map.Prototype);
      That(GlobalThis, Has.OwnProperty("Map", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("must be called as constructor", () => {
        That(() => Map.Call(This), Throws.TypeError);
        That(() => Map.Call(This, EcmaArray.Of()), Throws.TypeError);
      });

      It("should construct an empty map if iterable is undefined or null", () => {
        That(Map.Construct()["size"], Is.EqualTo(0));
        That(Map.Construct(Undefined)["size"], Is.EqualTo(0));
        That(Map.Construct(Null)["size"], Is.EqualTo(0));
      });

      It("should throw a TypeError if object is not iterable", () => {
        That(() => Map.Construct(Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError if iterable items are not Objects", () => {
        That(() => Map.Construct(EcmaArray.Of(Undefined, 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of(Null, 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of(1, 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of("", 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of(true, 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of(new Symbol(), 1)), Throws.TypeError);
        That(() => Map.Construct(EcmaArray.Of(EcmaArray.Of("a", 1), 2)), Throws.TypeError);
      });

      It("should throw TypeError if set is not callable only when iterable is not undefined or null", () => {
        using (TempProperty(Map.Prototype, "set", Null)) {
          That(() => Map.Construct(EcmaArray.Of()), Throws.TypeError);
          That(() => Map.Construct(), Throws.Nothing);
          That(() => Map.Construct(Undefined), Throws.Nothing);
          That(() => Map.Construct(Null), Throws.Nothing);
        }
      });

      It("should return abrupt from getting set mehod only when iterable is not undefined or null", () => {
        using (TempProperty(Map.Prototype, "set", EcmaPropertyDescriptor.FromValue(CreateObject(("get", ThrowTest262Exception), ("configurable", true))))) {
          That(() => Map.Construct(EcmaArray.Of()), Throws.Test262);
          That(() => Map.Construct(), Throws.Nothing);
          That(() => Map.Construct(Undefined), Throws.Nothing);
          That(() => Map.Construct(Null), Throws.Nothing);
        }
      });

      It("should call set for each entry iterated from iterable", () => {
        EcmaValue add = Map.Prototype.Get("set");
        EcmaValue added = EcmaArray.Of();
        using (TempProperty(Map.Prototype, "set", Intercept((k, v) => Return(added.Invoke("push", EcmaArray.Of(k, v)), add.Invoke("apply", This, Arguments))))) {
          Map.Construct(EcmaArray.Of(EcmaArray.Of("foo", 42), EcmaArray.Of("bar", 43)));
          That(added, Is.EquivalentTo(new[] { new EcmaValue[] { "foo", 42 }, new EcmaValue[] { "bar", 43 } }));

          Logs.Clear();
          Map.Construct();
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should throw if iterable next failure", () => {
        EcmaValue iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = ThrowTest262Exception }))));
        That(() => Map.Construct(iterable), Throws.Test262);

        iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = RuntimeFunction.Create(() => CreateObject(("value", get: ThrowTest262Exception, set: null), ("done", get: () => false, set: null))) }))));
        That(() => Map.Construct(iterable), Throws.Test262);
      });

      It("should close iterator after set failure", () => {
        EcmaValue iterable = CreateObject(
          (Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new {
            next = RuntimeFunction.Create(() => CreateObject(new { value = EcmaArray.Of(), done = false })),
            @return = RuntimeFunction.Create(Intercept(_ => _))
          })))
        );
        Logs.Clear();
        using (TempProperty(Map.Prototype, "set", ThrowTest262Exception)) {
          That(() => Map.Construct(iterable), Throws.Test262);
          That(Logs, Has.Exactly(1).Items);
        }
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.MapPrototype)));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(Map, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(Map.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(Map, Is.EqualTo(Map));

      EcmaValue obj = new EcmaObject();
      Case(obj, Is.EqualTo(obj));
    }
  }
}
