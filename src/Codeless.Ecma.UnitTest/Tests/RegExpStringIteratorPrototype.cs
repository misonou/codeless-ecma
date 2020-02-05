using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class RegExpStringIteratorPrototype : TestBase {
    [Test]
    public void Properties() {
      EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
      EcmaValue proto = Object.Invoke("getPrototypeOf", iterator);
      That(proto, Has.OwnProperty(WellKnownSymbol.ToStringTag, "RegExp String Iterator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      IsUnconstructableFunctionWLength(next, "next", 0);

      It("should throw a TypeError if this value does not have all of the internal slots", () => {
        EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
        That(() => iterator["next"].Call(Undefined), Throws.TypeError);
        That(() => iterator["next"].Call(Null), Throws.TypeError);
        That(() => iterator["next"].Call(1), Throws.TypeError);
        That(() => iterator["next"].Call(false), Throws.TypeError);
        That(() => iterator["next"].Call(""), Throws.TypeError);
        That(() => iterator["next"].Call(new Symbol()), Throws.TypeError);
        That(() => Object.Invoke("create", iterator).Invoke("next"), Throws.TypeError);
      });

      It("should iterate over matches", () => {
        EcmaValue iterator = RegExp.Construct("\\w").Invoke(Symbol.MatchAll, "*a*b*");
        VerifyIteratorResult(iterator.Invoke("next"), false, v => VerifyMatchObject(v, new[] { "a" }, 1, "*a*b*"));
        VerifyIteratorResult(iterator.Invoke("next"), true);

        iterator = RegExp.Construct("\\w", "g").Invoke(Symbol.MatchAll, "*a*b*");
        VerifyIteratorResult(iterator.Invoke("next"), false, v => VerifyMatchObject(v, new[] { "a" }, 1, "*a*b*"));
        VerifyIteratorResult(iterator.Invoke("next"), false, v => VerifyMatchObject(v, new[] { "b" }, 3, "*a*b*"));
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should not throw if exec is not callable", () => {
        foreach (EcmaValue value in new[] { Undefined, Null, 4, true, new Symbol() }) {
          using (TempProperty(RegExp.Prototype, "exec", value)) {
            EcmaValue iterator = RegExp.Construct("\\w", "g").Invoke(Symbol.MatchAll, "*a*b*");
            VerifyIteratorResult(iterator.Invoke("next"), false, v => VerifyMatchObject(v, new[] { "a" }, 1, "*a*b*"));
            VerifyIteratorResult(iterator.Invoke("next"), false, v => VerifyMatchObject(v, new[] { "b" }, 3, "*a*b*"));
            VerifyIteratorResult(iterator.Invoke("next"), true);
          }
        }
      });

      It("should re-throw errors thrown coercing RegExp's lastIndex to a length", () => {
        EcmaValue iterator = RegExp.Construct(".", "g").Invoke(Symbol.MatchAll, "");
        using (TempProperty(RegExp.Prototype, "exec", FunctionLiteral(() =>
          Return(This.ToObject()["lastIndex"] = CreateObject(valueOf: ThrowTest262Exception), EcmaArray.Of(""))))) {
          Case(iterator, Throws.Test262);
        }
      });

      It("should return abrupt from calling exec", () => {
        EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
        using (TempProperty(RegExp.Prototype, "exec", ThrowTest262Exception)) {
          Case(iterator, Throws.Test262);
        }
      });

      It("should return abrupt from getting exec", () => {
        EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
        using (TempProperty(RegExp.Prototype, "exec", new EcmaPropertyDescriptor(ThrowTest262Exception, Null))) {
          Case(iterator, Throws.Test262);
        }
      });

      It("should return abrupt from accessing the first match", () => {
        EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
        using (TempProperty(RegExp.Prototype, "exec", FunctionLiteral(() => CreateObject((0, get: ThrowTest262Exception, set: null))))) {
          Case(iterator, Throws.Test262);
        }
      });

      It("should return abrupt from coercing first match to a string", () => {
        EcmaValue iterator = RegExp.Construct(".").Invoke(Symbol.MatchAll, "");
        using (TempProperty(RegExp.Prototype, "exec", FunctionLiteral(() => EcmaArray.Of(CreateObject(toString: ThrowTest262Exception))))) {
          Case(iterator, Throws.Test262);
        }
      });

      It("should continue if first match is coerced to a empty string", () => {
        EcmaValue iterator = RegExp.Construct(".", "g").Invoke(Symbol.MatchAll, "");
        EcmaValue execResult = CreateObject((0, get: () => CreateObject(toString: () => ""), set: null));
        EcmaValue internalRegExp = default;
        using (TempProperty(RegExp.Prototype, "exec", FunctionLiteral(() => Return(internalRegExp = This, execResult)))) {
          EcmaValue result = iterator.Invoke("next");
          That(internalRegExp["lastIndex"], Is.EqualTo(1));
          That(result["value"], Is.EqualTo(execResult));
          That(result["done"], Is.EqualTo(false));

          result = iterator.Invoke("next");
          That(internalRegExp["lastIndex"], Is.EqualTo(2));
          That(result["value"], Is.EqualTo(execResult));
          That(result["done"], Is.EqualTo(false));
        }
      });

      It("should work with custom RegExp exec", () => {
        EcmaValue regexp = RegExp.Construct(".", "g");
        EcmaValue str = "abc";
        EcmaValue iter = regexp.Invoke(Symbol.MatchAll, str);
        EcmaValue callArgs = default;
        EcmaValue callCount = default;

        EcmaValue callNextWithExecReturnValue(EcmaValue returnValue) {
          callArgs = Undefined;
          callCount = 0;
          using (TempProperty(RegExp.Prototype, "exec", FunctionLiteral(() => Return(callArgs = Arguments, callCount += 1, returnValue)))) {
            return iter.Invoke("next");
          }
        }

        EcmaValue firstExecReturnValue = EcmaArray.Of("ab");
        EcmaValue result = callNextWithExecReturnValue(firstExecReturnValue);
        That(result["value"], Is.EqualTo(firstExecReturnValue));
        That(result["done"], Is.EqualTo(false));

        That(callArgs["length"], Is.EqualTo(1));
        That(callArgs[0], Is.EqualTo(str));
        That(callCount, Is.EqualTo(1));

        result = callNextWithExecReturnValue(Null);
        That(result["value"], Is.Undefined);
        That(result["done"], Is.EqualTo(true));

        That(callArgs["length"], Is.EqualTo(1));
        That(callArgs[0], Is.EqualTo(str));
        That(callCount, Is.EqualTo(1));
      });
    }

    private void VerifyMatchObject(EcmaValue value, object[] arr, int index, string input) {
      That(value, Is.EquivalentTo(arr));
      That(value["index"], Is.EqualTo(index));
      That(value["input"], Is.EqualTo(input));
    }
  }
}
