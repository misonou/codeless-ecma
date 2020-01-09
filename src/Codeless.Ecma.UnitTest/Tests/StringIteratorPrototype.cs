using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class StringIteratorPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue iterator = ((EcmaValue)"").Invoke(Symbol.Iterator);
      EcmaValue proto = Object.Invoke("getPrototypeOf", iterator);
      That(proto, Has.OwnProperty(WellKnownSymbol.ToStringTag, "String Iterator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      IsUnconstructableFunctionWLength(next, "next", 0);

      It("should throw a TypeError if this value does not have all of the internal slots", () => {
        EcmaValue iterator = ((EcmaValue)"").Invoke(Symbol.Iterator);
        That(() => iterator["next"].Call(Undefined), Throws.TypeError);
        That(() => iterator["next"].Call(Null), Throws.TypeError);
        That(() => iterator["next"].Call(1), Throws.TypeError);
        That(() => iterator["next"].Call(false), Throws.TypeError);
        That(() => iterator["next"].Call(""), Throws.TypeError);
        That(() => iterator["next"].Call(new Symbol()), Throws.TypeError);
        That(() => Object.Invoke("create", iterator).Invoke("next"), Throws.TypeError);
      });

      It("should visit each UTF-8 code point exactly once", () => {
        EcmaValue iterator = ((EcmaValue)"abc").Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, "a");
        VerifyIteratorResult(iterator.Invoke("next"), false, "b");
        VerifyIteratorResult(iterator.Invoke("next"), false, "c");
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should respect UTF-16-encoded Unicode code points specified via surrogate pairs", () => {
        var lo = "\uD834";
        var hi = "\uDF06";
        var pair = lo + hi;
        EcmaValue str = "a" + pair + "b" + lo + pair + hi + lo;
        EcmaValue iterator = str.Invoke(Symbol.Iterator);
        VerifyIteratorResult(iterator.Invoke("next"), false, "a");
        VerifyIteratorResult(iterator.Invoke("next"), false, pair);
        VerifyIteratorResult(iterator.Invoke("next"), false, "b");
        VerifyIteratorResult(iterator.Invoke("next"), false, lo);
        VerifyIteratorResult(iterator.Invoke("next"), false, pair);
        VerifyIteratorResult(iterator.Invoke("next"), false, hi);
        VerifyIteratorResult(iterator.Invoke("next"), false, lo);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
