using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ArrayConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Array", 1, Array.Prototype);

      That(Array.Construct(0)["length"], Is.EqualTo(0));
      That(Array.Construct(1)["length"], Is.EqualTo(1));
      That(Array.Construct(4294967295)["length"], Is.EqualTo(4294967295));

      That(() => Array.Construct(-1), Throws.RangeError);
      That(() => Array.Construct(4294967296), Throws.RangeError);
      That(() => Array.Construct(4294967297), Throws.RangeError);
      That(() => Array.Construct(NaN), Throws.RangeError);
      That(() => Array.Construct(Infinity), Throws.RangeError);
      That(() => Array.Construct(-Infinity), Throws.RangeError);
      That(() => Array.Construct(1.5), Throws.RangeError);
      That(() => Array.Construct(Number["MAX_VALUE"]), Throws.RangeError);
      That(() => Array.Construct(Number["MIN_VALUE"]), Throws.RangeError);

      That(Array.Construct(Null), Is.EquivalentTo(new[] { Null }));
      That(Array.Construct(Undefined), Is.EquivalentTo(new[] { Undefined }));
      That(Array.Construct(true), Is.EquivalentTo(new[] { true }));
      That(Array.Construct("1"), Is.EquivalentTo(new[] { "1" }));

      EcmaValue falseObj = Boolean.Construct(false);
      That(Array.Construct(falseObj), Is.EquivalentTo(new[] { falseObj }));
      EcmaValue strObj = String.Construct("0");
      That(Array.Construct(strObj), Is.EquivalentTo(new[] { strObj }));
      EcmaValue numObj = Number.Construct(0);
      That(Array.Construct(numObj), Is.EquivalentTo(new[] { numObj }));
      numObj = Number.Construct(4294967295);
      That(Array.Construct(numObj), Is.EquivalentTo(new[] { numObj }));
      numObj = Number.Construct(-1);
      That(Array.Construct(numObj), Is.EquivalentTo(new[] { numObj }));
      numObj = Number.Construct(4294967296);
      That(Array.Construct(numObj), Is.EquivalentTo(new[] { numObj }));
      numObj = Number.Construct(4294967297);
      That(Array.Construct(numObj), Is.EquivalentTo(new[] { numObj }));

      That(Array.Construct(), Is.EquivalentTo(EcmaValue.EmptyArray));
      That(Array.Construct(0, 1, 0, 1), Is.EquivalentTo(new[] { 0, 1, 0, 1 }));
      That(Array.Construct(Undefined, Undefined), Is.EquivalentTo(new[] { Undefined, Undefined }));

      That(Array.Call().InstanceOf(Array));
      That(Array.Call(), Is.EquivalentTo(EcmaValue.EmptyArray));
      That(Array.Call(_, 0, 1, 0, 1), Is.EquivalentTo(new[] { 0, 1, 0, 1 }));
      That(Array.Call(_, Undefined, Undefined), Is.EquivalentTo(new[] { Undefined, Undefined }));
    }

    [Test, RuntimeFunctionInjection]
    public void From(RuntimeFunction from) {
      IsUnconstructableFunctionWLength(from, "from", 1);

      Case((_, Null), Throws.TypeError,
        "Throw if source object is null");
      Case((Null, EcmaArray.Of()), Is.EquivalentTo(EcmaValue.EmptyArray),
        "Does not throw if this is null");

      It("should throw TypeError if mapFn is not undefined and callable", () => {
        Case((_, EcmaArray.Of(), Null), Throws.TypeError);
        Case((_, EcmaArray.Of(), new EcmaObject()), Throws.TypeError);
        Case((_, EcmaArray.Of(), "string"), Throws.TypeError);
        Case((_, EcmaArray.Of(), true), Throws.TypeError);
        Case((_, EcmaArray.Of(), 42), Throws.TypeError);
        Case((_, EcmaArray.Of(), new Symbol()), Throws.TypeError);
      });

      It("should create new array from the passed array", () => {
        EcmaArray arr = new EcmaArray();
        arr[0] = 0;
        arr[1] = "foo";
        arr[3] = Infinity;
        Case((_, arr), new[] { 0, "foo", Undefined, Infinity });
      });

      It("should create new array from string", () => {
        Case((_, "test"), new[] { "t", "e", "s", "t" });
      });

      It("should create an empty Array if source object does not have length property", () => {
        Case((_, CreateObject(("0", 2), ("1", 4))), Is.EquivalentTo(EcmaValue.EmptyArray));
      });

      It("should pass the number of arguments to the constructor it calls if the first argument is not iterable", () => {
        EcmaValue args = default;
        EcmaValue ConstructorFn = FunctionLiteral(() => Void(args = Arguments));
        EcmaValue result = from.Call(ConstructorFn, CreateObject(new { length = 42 }));
        That(args, Is.EquivalentTo(new[] { 42 }));
        That(result.InstanceOf(ConstructorFn));
      });

      It("should pass no arguments to the constructor it calls if the first argument is iterable", () => {
        EcmaValue args = default;
        EcmaValue ConstructorFn = FunctionLiteral(() => Void(args = Arguments));
        EcmaValue result = from.Call(ConstructorFn, EcmaArray.Of());
        That(args, Is.EquivalentTo(EcmaValue.EmptyArray));
        That(result.InstanceOf(ConstructorFn));
      });

      It("should return abrupt from custom constructor", () => {
        Case((ThrowTest262Exception, EcmaArray.Of()), Throws.Test262);
      });

      It("should return abrupt from mapping function", () => {
        Case((_, EcmaArray.Of(1, 2, 4, 8), ThrowTest262Exception), Throws.Test262);
      });

      It("should return abrupt from setting length or elements", () => {
        EcmaValue ConstructorFn = FunctionLiteral(() => Undefined);
        DefineProperty(ConstructorFn["prototype"], "length", set: _ => ThrowTest262Exception());
        Case((ConstructorFn, EcmaArray.Of(1, 2, 4, 8)), Throws.Test262);

        ConstructorFn = FunctionLiteral(() => Object.Invoke("defineProperty", This, "0", CreateObject(new { configurable = false })));
        Case((ConstructorFn, EcmaArray.Of(1, 2, 4, 8)), Throws.TypeError);
      });

      It("should call mapFn with this value is undefined if thisArg is not given in strict mode", () => {
        EcmaArray calls = new EcmaArray();
        EcmaValue mapFn = FunctionLiteral(value => Return(calls.Push(CreateObject(new { args = Arguments, thisArg = This })), value * 2));
        Case((_, CreateObject(("length", 3), ("0", 41), ("1", 42), ("2", 43)), mapFn), new[] { 82, 84, 86 });
        That(calls[0]["args"], Is.EquivalentTo(new[] { 41, 0 }), "calls[0].args");
        That(calls[1]["args"], Is.EquivalentTo(new[] { 42, 1 }), "calls[1].args");
        That(calls[2]["args"], Is.EquivalentTo(new[] { 43, 2 }), "calls[2].args");
        That(calls[0]["thisArg"], Is.Undefined, "calls[0].thisArg");
        That(calls[1]["thisArg"], Is.Undefined, "calls[1].thisArg");
        That(calls[2]["thisArg"], Is.Undefined, "calls[2].thisArg");
      });

      It("should call mapFn with given this value", () => {
        EcmaValue thisArg = new EcmaObject();
        EcmaArray calls = new EcmaArray();
        EcmaValue mapFn = FunctionLiteral(value => Return(calls.Push(CreateObject(new { args = Arguments, thisArg = This })), value * 2));
        Case((_, CreateObject(("length", 3), ("0", 41), ("1", 42), ("2", 43)), mapFn, thisArg), new[] { 82, 84, 86 });
        That(calls[0]["args"], Is.EquivalentTo(new[] { 41, 0 }), "calls[0].args");
        That(calls[1]["args"], Is.EquivalentTo(new[] { 42, 1 }), "calls[1].args");
        That(calls[2]["args"], Is.EquivalentTo(new[] { 43, 2 }), "calls[2].args");
        That(calls[0]["thisArg"], Is.EqualTo(thisArg), "calls[0].thisArg");
        That(calls[1]["thisArg"], Is.EqualTo(thisArg), "calls[1].thisArg");
        That(calls[2]["thisArg"], Is.EqualTo(thisArg), "calls[2].thisArg");
      });

      It("should not see elements added to array-like object during mapFn hit", () => {
        EcmaValue obj = CreateObject(("length", 7), ("0", 2), ("1", 4), ("2", 8), ("3", 16), ("4", 32), ("5", 64), ("6", 128));
        EcmaValue mapFn = FunctionLiteral((v, i) => {
          obj[i + 7] = 4 << (i.ToInt32() + 7);
          obj["length"] = 14;
          return v;
        });
        Case((_, obj, mapFn), new[] { 2, 4, 8, 16, 32, 64, 128 });
      });

      It("should not see elements deleted from iterable during mapFn hit", () => {
        EcmaArray arr = EcmaArray.Of(0, 1, -2, 4, -8, 16);
        Case((_, arr, FunctionLiteral(v => Return(arr.Length--, v))), new[] { 0, 1, -2 });
      });

      It("should see updated elements during mapFn hit", () => {
        EcmaArray arr = EcmaArray.Of(127, 1, -2, 4, -8, 16);
        Case((_, arr, FunctionLiteral((v, i) => Return(arr[EcmaMath.Min(5, i + 1)] = 127, v))), new[] { 127, 127, 127, 127, 127, 127 });
      });

      It("should return abrupt from accessing Symbol.iterator or calling the next method", () => {
        Case((_, CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null))), Throws.Test262);
        Case((_, CreateObject((Symbol.Iterator, ThrowTest262Exception))), Throws.Test262);

        EcmaValue obj = CreateObject((Symbol.Iterator, FunctionLiteral(() =>
          CreateObject(("next", ThrowTest262Exception)))));
        Case((_, obj), Throws.Test262);

        EcmaValue obj2 = CreateObject((Symbol.Iterator, FunctionLiteral(() =>
          CreateObject(("next", FunctionLiteral(() =>
            CreateObject(("value", get: ThrowTest262Exception, set: null))))))));
        Case((_, obj2), Throws.Test262);
      });

      It("should pass value from iterator to mapping function", () => {
        EcmaArray args = new EcmaArray();
        EcmaValue firstResult = CreateObject(new { done = false, value = new EcmaObject() });
        EcmaValue secondResult = CreateObject(new { done = false, value = new EcmaObject() });
        EcmaValue mapFn = FunctionLiteral(v => Return(args.Push(Arguments), v));
        EcmaValue nextResult = firstResult;
        EcmaValue nextNextResult = secondResult;

        EcmaValue result = from.Call(_, CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(("next", FunctionLiteral(() => {
            EcmaValue r = nextResult;
            nextResult = nextNextResult;
            nextNextResult = CreateObject(new { done = true });
            return r;
          })));
        }))), mapFn);

        That(result, Is.EquivalentTo(new[] { firstResult["value"], secondResult["value"] }));
        That(args.ToValue(), Is.EquivalentTo(new[] { new[] { firstResult["value"], 0 }, new[] { secondResult["value"], 1 } }));
      });

      It("should close the iterator if mapFn return abrupt", () => {
        Logs.Clear();
        Case((_, CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(("next", FunctionLiteral(() => CreateObject(new { done = false }))),
                              ("return", Intercept(() => Undefined)));
        }))), ThrowTest262Exception), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsArray(RuntimeFunction isArray) {
      IsUnconstructableFunctionWLength(isArray, "isArray", 1);

      Case((_, EcmaArray.Of()), true);
      Case((_, Array.Prototype), true);
      Case((_, Array.Construct(10)), true);

      Case((_, Undefined), false);
      Case((_, Null), false);
      Case((_, true), false);
      Case((_, 42), false);
      Case((_, "abc"), false);
      Case((_, new EcmaObject()), false);
      Case((_, Boolean.Construct(false)), false);
      Case((_, Number.Construct(42)), false);
      Case((_, String.Construct("abc")), false);
      Case((_, CreateObject((0, "1"), ("length", 1))), false);
      Case((_, FunctionLiteral(() => Undefined)), false);

      EcmaValue ConstructorFn = FunctionLiteral(() => Undefined);
      ConstructorFn["prototype"] = EcmaArray.Of();
      Case((_, ConstructorFn.Construct()), false);
      ConstructorFn["prototype"] = Array.Prototype;
      Case((_, ConstructorFn.Construct()), false);

      EcmaValue arrayProxy = Proxy.Construct(EcmaArray.Of(), new EcmaObject());
      Case((_, arrayProxy), true);
      Case((_, Proxy.Construct(arrayProxy, new EcmaObject())), true);
      Case((_, Proxy.Construct(new EcmaObject(), new EcmaObject())), false);

      EcmaValue handler = Proxy.Invoke("revocable", EcmaArray.Of(), new EcmaObject());
      handler.Invoke("revoke");
      Case((_, handler["proxy"]), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Of(RuntimeFunction of) {
      IsUnconstructableFunctionWLength(of, "of", 0);

      It("should create a new Array with a variable number of arguments", () => {
        Case(_, EcmaValue.EmptyArray);
        Case((_, "Mike", "Rick", "Leo"), new[] { "Mike", "Rick", "Leo" });
        Case((_, Undefined, false, Null, Undefined), new[] { Undefined, false, Null, Undefined });
      });

      It("should pass the number of arguments to the constructor it calls", () => {
        EcmaValue len = default;
        EcmaValue C = Intercept(v => len = v);
        Logs.Clear();

        of.Call(C);
        That(len, Is.EqualTo(0));
        That(Logs, Has.Exactly(1).Items);

        of.Call(C, "a", "b");
        That(len, Is.EqualTo(2));
        That(Logs, Has.Exactly(2).Items);

        of.Call(C, false, Null, Undefined);
        That(len, Is.EqualTo(3));
        That(Logs, Has.Exactly(3).Items);
      });

      It("should create own property", () => {
        using (TempProperty(Array.Prototype, 0, new EcmaPropertyDescriptor(ThrowTest262Exception, ThrowTest262Exception))) {
          Case((_, true), Throws.Nothing);
        }
        Array.Prototype["length"] = 0;
      });

      It("should return instance from a custom constructor", () => {
        EcmaValue C = FunctionLiteral(() => Undefined);
        EcmaValue result = of.Call(C, "Mike", "Rick", "Leo");
        That(result, Is.EquivalentTo(new[] { "Mike", "Rick", "Leo" }));
        That(result.InstanceOf(C));
      });

      It("should return a new Array object if this value is not a constructor", () => {
        That(of.Call(Undefined).InstanceOf(Array));
        That(of.Call(GlobalThis["parseInt"]).InstanceOf(Array));
      });

      It("should return abrupt from this constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("should return abrupt from data property creation", () => {
        EcmaValue C1 = FunctionLiteral(() => Object.Invoke("preventExtensions", This));
        Case((C1, "Bob"), Throws.TypeError);

        EcmaValue C2 = FunctionLiteral(() => Object.Invoke("defineProperty", This, 0, CreateObject(new { configurable = false, writable = true, enumerable = true })));
        Case((C2, "Bob"), Throws.TypeError);

        EcmaValue C3 = FunctionLiteral(() => Proxy.Construct(new EcmaObject(), CreateObject(new { defineProperty = ThrowTest262Exception })));
        Case((C3, "Bob"), Throws.Test262);
      });

      It("should return abrupt from setting the length property", () => {
        EcmaValue C = FunctionLiteral(() => Object.Invoke("defineProperty", This, "length", CreateObject(new { set = ThrowTest262Exception })));
        Case((C, "Bob"), Throws.Test262);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(Array, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(Array.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(Array, Is.EqualTo(Array));
    }
  }
}
