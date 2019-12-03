using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SetIteratorPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue iterator = Set.Construct().Invoke(Symbol.Iterator);
      EcmaValue proto = Object.Invoke("getPrototypeOf", iterator);
      That(proto, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Set Iterator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      IsUnconstructableFunctionWLength(next, "next", 0);

      It("should throw a TypeError if this value does not have all of the internal slots of a Set Iterator Instance", () => {
        EcmaValue set = Set.Construct();
        foreach (var i in new EcmaPropertyKey[] { Symbol.Iterator, "keys", "entries", "values" }) {
          EcmaValue iterator = set.Invoke(i);
          That(() => iterator["next"].Call(Undefined), Throws.TypeError);
          That(() => iterator["next"].Call(Null), Throws.TypeError);
          That(() => iterator["next"].Call(1), Throws.TypeError);
          That(() => iterator["next"].Call(false), Throws.TypeError);
          That(() => iterator["next"].Call(""), Throws.TypeError);
          That(() => iterator["next"].Call(new Symbol()), Throws.TypeError);

          That(() => iterator["next"].Call(set), Throws.TypeError);
          That(() => iterator["next"].Call(Object.Construct()), Throws.TypeError);
          That(() => iterator["next"].Call(set.Invoke(i)), Throws.Nothing);
        }
      });

      It("should return a valid iterator with the context as the IteratedObject", () => {
        EcmaValue set = Set.Construct();
        set.Invoke("add", 1);
        set.Invoke("add", 2);
        set.Invoke("add", 3);

        EcmaValue iterator = set.Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, 1);
        VerifyIteratorResult(iterator.Invoke("next"), false, 2);
        VerifyIteratorResult(iterator.Invoke("next"), false, 3);
        VerifyIteratorResult(iterator.Invoke("next"), true);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should onlt visit new item if it is added before iterator is done", () => {
        EcmaValue set = Set.Construct();
        set.Invoke("add", 1);
        set.Invoke("add", 2);

        EcmaValue iterator = set.Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, 1);
        set.Invoke("add", 3);
        VerifyIteratorResult(iterator.Invoke("next"), false, 2);
        VerifyIteratorResult(iterator.Invoke("next"), false, 3);
        VerifyIteratorResult(iterator.Invoke("next"), true);
        set.Invoke("add", 4);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
