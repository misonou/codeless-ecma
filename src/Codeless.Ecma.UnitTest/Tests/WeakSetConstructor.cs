using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class WeakSetConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "WeakSet", 0, WeakSet.Prototype);
      That(GlobalThis, Has.OwnProperty("WeakSet", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("must be called as constructor", () => {
        That(() => WeakSet.Call(This), Throws.TypeError);
        That(() => WeakSet.Call(This, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return a new WeakSet object if iterable is undefined or null", () => {
        That(Object.Invoke("getPrototypeOf", WeakSet.Construct()), Is.EqualTo(WeakSet.Prototype));
        That(Object.Invoke("getPrototypeOf", WeakSet.Construct(Undefined)), Is.EqualTo(WeakSet.Prototype));
        That(Object.Invoke("getPrototypeOf", WeakSet.Construct(Null)), Is.EqualTo(WeakSet.Prototype));
      });

      It("should throw a TypeError if object is not iterable", () => {
        That(() => WeakSet.Construct(Object.Construct()), Throws.TypeError);
      });

      It("should throw TypeError if add is not callable only when iterable is not undefined or null", () => {
        using (TempProperty(WeakSet.Prototype, "add", Null)) {
          That(() => WeakSet.Construct(EcmaArray.Of()), Throws.TypeError);
          That(() => WeakSet.Construct(), Throws.Nothing);
          That(() => WeakSet.Construct(Undefined), Throws.Nothing);
          That(() => WeakSet.Construct(Null), Throws.Nothing);
        }
      });

      It("should return abrupt from getting add mehod only when iterable is not undefined or null", () => {
        using (TempProperty(WeakSet.Prototype, "add", EcmaPropertyDescriptor.FromValue(CreateObject(("get", ThrowTest262Exception), ("configurable", true))))) {
          That(() => WeakSet.Construct(EcmaArray.Of()), Throws.Test262);
          That(() => WeakSet.Construct(), Throws.Nothing);
          That(() => WeakSet.Construct(Undefined), Throws.Nothing);
          That(() => WeakSet.Construct(Null), Throws.Nothing);
        }
      });

      It("should call add for each entry iterated from iterable", () => {
        EcmaValue add = WeakSet.Prototype.Get("add");
        EcmaValue added = EcmaArray.Of();
        using (TempProperty(WeakSet.Prototype, "add", Intercept(v => Return(added.Invoke("push", v), add.Invoke("apply", This, Arguments))))) {
          EcmaValue first = Object.Construct();
          EcmaValue second = Object.Construct();
          WeakSet.Construct(EcmaArray.Of(first, second));
          That(added, Is.EquivalentTo(new[] { first, second }));

          Logs.Clear();
          WeakSet.Construct();
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should throw if iterable next failure", () => {
        EcmaValue iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = ThrowTest262Exception }))));
        That(() => WeakSet.Construct(iterable), Throws.Test262);

        iterable = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new { next = RuntimeFunction.Create(() => CreateObject(("value", get: ThrowTest262Exception, set: null), ("done", get: () => false, set: null))) }))));
        That(() => WeakSet.Construct(iterable), Throws.Test262);
      });

      It("should close iterator after add failure", () => {
        EcmaValue iterable = CreateObject(
          (Symbol.Iterator, RuntimeFunction.Create(() => CreateObject(new {
            next = RuntimeFunction.Create(() => CreateObject(new { value = Null, done = false })),
            @return = RuntimeFunction.Create(Intercept(_ => _))
          })))
        );
        Logs.Clear();
        using (TempProperty(WeakSet.Prototype, "add", ThrowTest262Exception)) {
          That(() => WeakSet.Construct(iterable), Throws.Test262);
          That(Logs, Has.Exactly(1).Items);
        }
      });

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.WeakSetPrototype)));
      });
    }
  }
}
