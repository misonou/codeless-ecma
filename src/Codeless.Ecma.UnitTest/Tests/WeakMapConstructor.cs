using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class WeakMapConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "WeakMap", 0, WeakMap.Prototype);
      That(GlobalThis, Has.OwnProperty("WeakMap", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("must be called as constructor", () => {
        That(() => WeakMap.Call(This), Throws.TypeError);
        That(() => WeakMap.Call(This, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return a new WeakMap object if iterable is undefined or null", () => {
        That(Object.Invoke("getPrototypeOf", WeakMap.Construct()), Is.EqualTo(WeakMap.Prototype));
        That(Object.Invoke("getPrototypeOf", WeakMap.Construct(Undefined)), Is.EqualTo(WeakMap.Prototype));
        That(Object.Invoke("getPrototypeOf", WeakMap.Construct(Null)), Is.EqualTo(WeakMap.Prototype));
      });

      It("should throw a TypeError if object is not iterable", () => {
        That(() => WeakMap.Construct(Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError if iterable items are not Objects", () => {
        That(() => WeakMap.Construct(EcmaArray.Of(Undefined, 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of(Null, 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of(1, 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of("", 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of(true, 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of(new Symbol(), 1)), Throws.TypeError);
        That(() => WeakMap.Construct(EcmaArray.Of(EcmaArray.Of("a", 1), 2)), Throws.TypeError);
      });

      It("should throw TypeError if set is not callable only when iterable is not undefined or null", () => {
        using (TempProperty(WeakMap.Prototype, "set", Null)) {
          That(() => WeakMap.Construct(EcmaArray.Of()), Throws.TypeError);
          That(() => WeakMap.Construct(), Throws.Nothing);
          That(() => WeakMap.Construct(Undefined), Throws.Nothing);
          That(() => WeakMap.Construct(Null), Throws.Nothing);
        }
      });

      It("should return abrupt from getting set mehod only when iterable is not undefined or null", () => {
        using (TempProperty(WeakMap.Prototype, "set", EcmaPropertyDescriptor.FromValue(CreateObject(("get", ThrowTest262Exception), ("configurable", true))))) {
          That(() => WeakMap.Construct(EcmaArray.Of()), Throws.Test262);
          That(() => WeakMap.Construct(), Throws.Nothing);
          That(() => WeakMap.Construct(Undefined), Throws.Nothing);
          That(() => WeakMap.Construct(Null), Throws.Nothing);
        }
      });

      It("should call set for each entry iterated from iterable", () => {
        EcmaValue add = WeakMap.Prototype.Get("set");
        EcmaValue added = EcmaArray.Of();
        using (TempProperty(WeakMap.Prototype, "set", Intercept((k, v) => Return(added.Invoke("push", EcmaArray.Of(k, v)), add.Invoke("apply", This, Arguments))))) {
          EcmaValue first = Object.Construct();
          EcmaValue second = Object.Construct();
          WeakMap.Construct(EcmaArray.Of(EcmaArray.Of(first, 42), EcmaArray.Of(second, 43)));
          That(added, Is.EquivalentTo(new[] { new[] { first, 42 }, new[] { second, 43 } }));

          Logs.Clear();
          WeakMap.Construct();
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should throw if iterable next failure", () => {
        EcmaValue iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = ThrowTest262Exception }))));
        That(() => WeakMap.Construct(iterable), Throws.Test262);

        iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = RuntimeFunction.Create(() => CreateObject(("value", get: ThrowTest262Exception, set: null), ("done", get: () => false, set: null))) }))));
        That(() => WeakMap.Construct(iterable), Throws.Test262);
      });

      It("should close iterator after set failure", () => {
        EcmaValue iterable = CreateObject(
          (Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new {
            next = RuntimeFunction.Create(() => CreateObject(new { value = EcmaArray.Of(), done = false })),
            @return = RuntimeFunction.Create(Intercept(_ => _))
          })))
        );
        Logs.Clear();
        using (TempProperty(WeakMap.Prototype, "set", ThrowTest262Exception)) {
          That(() => WeakMap.Construct(iterable), Throws.Test262);
          That(Logs, Has.Exactly(1).Items);
        }
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.WeakMapPrototype)));
      });
    }
  }
}
