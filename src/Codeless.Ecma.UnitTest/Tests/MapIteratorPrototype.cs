using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class MapIteratorPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue iterator = Map.Construct().Invoke(Symbol.Iterator);
      EcmaValue proto = Object.Invoke("getPrototypeOf", iterator);
      That(proto, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Map Iterator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      IsUnconstructableFunctionWLength(next, "next", 0);

      It("should throw a TypeError if this value does not have all of the internal slots of a Set Iterator Instance", () => {
        EcmaValue map = Map.Construct();
        foreach (var i in new EcmaPropertyKey[] { Symbol.Iterator, "keys", "entries", "values" }) {
          EcmaValue iterator = map.Invoke(i);
          That(() => iterator["next"].Call(Undefined), Throws.TypeError);
          That(() => iterator["next"].Call(Null), Throws.TypeError);
          That(() => iterator["next"].Call(1), Throws.TypeError);
          That(() => iterator["next"].Call(false), Throws.TypeError);
          That(() => iterator["next"].Call(""), Throws.TypeError);
          That(() => iterator["next"].Call(new Symbol()), Throws.TypeError);

          That(() => iterator["next"].Call(map), Throws.TypeError);
          That(() => iterator["next"].Call(Object.Construct()), Throws.TypeError);
          That(() => iterator["next"].Call(map.Invoke(i)), Throws.Nothing);
        }
      });

      It("should return a valid iterator with the context as the IteratedObject", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", 1, 11);
        map.Invoke("set", 2, 22);
        map.Invoke("set", 3, 33);

        EcmaValue iterator = map.Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 1, 11 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 2, 22 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 3, 33 });
        VerifyIteratorResult(iterator.Invoke("next"), true);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should onlt visit new item if it is added before iterator is done", () => {
        EcmaValue map = Map.Construct();
        map.Invoke("set", 1, 11);
        map.Invoke("set", 2, 22);

        EcmaValue iterator = map.Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 1, 11 });
        map.Invoke("set", 3, 33);
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 2, 22 });
        VerifyIteratorResult(iterator.Invoke("next"), false, new[] { 3, 33 });
        VerifyIteratorResult(iterator.Invoke("next"), true);
        map.Invoke("set", 3, 44);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
