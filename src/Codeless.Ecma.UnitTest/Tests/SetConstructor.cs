using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SetConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Set", 0, Set.Prototype);
      That(GlobalThis, Has.OwnProperty("Set", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("must be called as constructor", () => {
        That(() => Set.Call(This), Throws.TypeError);
        That(() => Set.Call(This, EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if object is not iterable", () => {
        That(() => Set.Construct(Object.Construct()), Throws.TypeError);
      });

      It("should construct an empty set if iterable is undefined or null", () => {
        That(Set.Construct()["size"], Is.EqualTo(0));
        That(Set.Construct(Undefined)["size"], Is.EqualTo(0));
        That(Set.Construct(Null)["size"], Is.EqualTo(0));
      });

      It("should throw TypeError if add is not callable only when iterable is not undefined or null", () => {
        using (TempProperty(Set.Prototype, "add", Null)) {
          That(() => Set.Construct(EcmaArray.Of()), Throws.TypeError);
          That(() => Set.Construct(), Throws.Nothing);
          That(() => Set.Construct(Undefined), Throws.Nothing);
          That(() => Set.Construct(Null), Throws.Nothing);
        }
      });

      It("should return abrupt from getting add mehod only when iterable is not undefined or null", () => {
        using (TempProperty(Set.Prototype, "add", EcmaPropertyDescriptor.FromValue(CreateObject(("get", ThrowTest262Exception), ("configurable", true))))) {
          That(() => Set.Construct(EcmaArray.Of()), Throws.Test262);
          That(() => Set.Construct(), Throws.Nothing);
          That(() => Set.Construct(Undefined), Throws.Nothing);
          That(() => Set.Construct(Null), Throws.Nothing);
        }
      });

      It("should call add for each entry iterated from iterable", () => {
        EcmaValue add = Set.Prototype.Get("add");
        using (TempProperty(Set.Prototype, "add", Intercept(() => add.Invoke("apply", This, Arguments)))) {
          Logs.Clear();
          That(Set.Construct(EcmaArray.Of(1, 2))["size"], Is.EqualTo(2));
          That(Logs, Has.Exactly(2).Items);

          Logs.Clear();
          Set.Construct();
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should throw if iterable next failure", () => {
        EcmaValue iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = ThrowTest262Exception }))));
        That(() => Set.Construct(iterable), Throws.Test262);

        iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = RuntimeFunction.Create(() => CreateObject(("value", get: ThrowTest262Exception, set: null), ("done", get: () => false, set: null))) }))));
        That(() => Set.Construct(iterable), Throws.Test262);
      });

      It("should close iterator after add failure", () => {
        EcmaValue iterable = CreateObject(
          (Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new {
            next = RuntimeFunction.Create(() => CreateObject(new { value = Null, done = false })),
            @return = RuntimeFunction.Create(Intercept(_ => _))
          })))
        );
        Logs.Clear();
        using (TempProperty(Set.Prototype, "add", ThrowTest262Exception)) {
          That(() => Set.Construct(iterable), Throws.Test262);
          That(Logs, Has.Exactly(1).Items);
        }
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.SetPrototype)));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(Set, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(Set.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(Set, Is.EqualTo(Set));

      EcmaValue obj = new EcmaObject();
      Case(obj, Is.EqualTo(obj));
    }
  }
}
