using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ArrayPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Array.Prototype, Has.OwnProperty("constructor", Array, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Array.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Array.Prototype), Is.EqualTo("[object Array]"), "Array prototype object: its [[Class]] must be 'Array'");

      That(Array.Prototype, Has.OwnProperty("length", 0, EcmaPropertyAttributes.Writable));
    }

    [Test, RuntimeFunctionInjection]
    public void Concat(RuntimeFunction concat) {
      IsUnconstructableFunctionWLength(concat, "concat", 1);
      IsAbruptedFromToObject(concat);

      It("should return an array containing the elements of the object followed by the elements of each argument in order", () => {
        EcmaArray x = EcmaArray.Of(0, 1);
        Case(x, new[] { 0, 1 });
        That(EcmaArray.IsArray(concat.Call(x)), "returned value is an Array exotic object");
        That(x, Is.Not.EqualTo(concat.Call(x)), "returned value is a fresh array");

        EcmaValue y = new EcmaObject();
        Case((new EcmaArray(), new EcmaArray(0, 1), new EcmaArray(2, 3, 4)), new[] { 0, 1, 2, 3, 4 });
        Case((EcmaArray.Of(0), y, new EcmaArray(1, 2), -1, true, "string"), new[] { 0, y, 1, 2, -1, true, "string" });
      });

      It("is intentionally generic", () => {
        EcmaValue x = new EcmaObject();
        EcmaValue y = new EcmaObject();
        Case(x, new[] { x });
        Case((x, y, new EcmaArray(1, 2), -1, true, "string"), new[] { x, y, 1, 2, -1, true, "string" });
      });

      It("should access inherited properties", () => {
        using (TempProperty(Object.Prototype, 2, 2)) {
          EcmaValue arr = EcmaArray.Of(0);
          arr["length"] = 3;
          Assume.That(!arr.HasOwnProperty(1));
          Assume.That(!arr.HasOwnProperty(2));

          EcmaValue result = concat.Call(arr);
          That(result, Is.EquivalentTo(new[] { 0, Undefined, 2 }));
          That(!result.HasOwnProperty(1));
          That(result.HasOwnProperty(2));
        }
      });

      It("should ignore Symbol.isConcatSpreadable property if its value is undefined", () => {
        EcmaValue arr = EcmaArray.Of();
        arr[Symbol.IsConcatSpreadable] = Undefined;
        Case(arr, EcmaValue.EmptyArray);

        EcmaValue obj = new EcmaObject();
        obj[Symbol.IsConcatSpreadable] = Undefined;
        Case(obj, new[] { obj });
      });

      It("should concat an Object's items if Symbol.isConcatSpreadable property is defined and coercible to true", () => {
        EcmaValue obj = new EcmaObject();
        obj[Symbol.IsConcatSpreadable] = true;
        Case(obj, EcmaValue.EmptyArray);
        obj[Symbol.IsConcatSpreadable] = 86;
        Case(obj, EcmaValue.EmptyArray);
        obj[Symbol.IsConcatSpreadable] = "string";
        Case(obj, EcmaValue.EmptyArray);
        obj[Symbol.IsConcatSpreadable] = new Symbol();
        Case(obj, EcmaValue.EmptyArray);
        obj[Symbol.IsConcatSpreadable] = new EcmaObject();
        Case(obj, EcmaValue.EmptyArray);

        EcmaValue objWithHoles = CreateObject((Symbol.IsConcatSpreadable, true), ("length", 6), ("1", 1), ("3", 3), ("5", 5));
        Case(objWithHoles, new[] { Undefined, 1, Undefined, 3, Undefined, 5 });

        EcmaValue objWithStrLength = CreateObject((Symbol.IsConcatSpreadable, true), ("length", "6"), ("1", 1), ("3", 3), ("5", 5));
        Case(objWithStrLength, new[] { Undefined, 1, Undefined, 3, Undefined, 5 });

        EcmaValue objWithBoolLength = CreateObject((Symbol.IsConcatSpreadable, true), ("length", true), ("1", 1), ("3", 3), ("5", 5));
        Case(objWithBoolLength, new[] { Undefined });

        EcmaValue objWithNegLength = CreateObject((Symbol.IsConcatSpreadable, true), ("length", -4294967294), ("1", 1), ("3", 3), ("5", 5));
        Case(objWithNegLength, EcmaValue.EmptyArray);

        EcmaValue numObj = Number.Construct(84);
        numObj[Symbol.IsConcatSpreadable] = true;
        Case(numObj, EcmaValue.EmptyArray);

        EcmaValue boolObj = Boolean.Construct(true);
        boolObj[Symbol.IsConcatSpreadable] = true;
        Case(boolObj, EcmaValue.EmptyArray);

        EcmaValue strObj = String.Construct("yuck\uD83D\uDCA9");
        strObj[Symbol.IsConcatSpreadable] = true;
        Case(strObj, new[] { "y", "u", "c", "k", "\uD83D", "\uDCA9" });

        using (TempProperty(String.Prototype, Symbol.IsConcatSpreadable, true)) {
          Case(String.Construct("yuck\uD83D\uDCA9"), new[] { "y", "u", "c", "k", "\uD83D", "\uDCA9" });
          Case("yuck\uD83D\uDCA9", new[] { "yuck\uD83D\uDCA9" });
        }
      });

      It("should concat an Array as an ordinary Object if Symbol.isConcatSpreadable property is defined and coercible to false", () => {
        EcmaValue arr = EcmaArray.Of();
        arr[Symbol.IsConcatSpreadable] = Null;
        Case(arr, new[] { arr });
        arr[Symbol.IsConcatSpreadable] = false;
        Case(arr, new[] { arr });
        arr[Symbol.IsConcatSpreadable] = 0;
        Case(arr, new[] { arr });
        arr[Symbol.IsConcatSpreadable] = NaN;
        Case(arr, new[] { arr });
      });

      It("should return abrupt from accessing Symbol.isConcatSpreadable property", () => {
        Case(CreateObject((Symbol.IsConcatSpreadable, get: ThrowTest262Exception, set: null)), Throws.Test262);
      });

      It("should return abrupt from getting length of concat-spreadable objects", () => {
        EcmaValue obj = new EcmaObject();
        obj[Symbol.IsConcatSpreadable] = true;

        DefineProperty(obj, "length", get: ThrowTest262Exception, configurable: true);
        Case(obj, Throws.Test262);

        DefineProperty(obj, "length", value: CreateObject(toString: ThrowTest262Exception, valueOf: () => new EcmaObject()), configurable: true);
        Case(obj, Throws.Test262);

        DefineProperty(obj, "length", value: CreateObject(valueOf: ThrowTest262Exception), configurable: true);
        Case(obj, Throws.Test262);

        DefineProperty(obj, "length", value: new Symbol(), configurable: true);
        Case(obj, Throws.TypeError);
      });

      It("should concat a Proxy object as its [[ProxyTarget]]", () => {
        EcmaValue arrayProxy = Proxy.Construct(EcmaArray.Of(), new EcmaObject());
        Case(arrayProxy, EcmaValue.EmptyArray);
        Case(Proxy.Construct(arrayProxy, new EcmaObject()), EcmaValue.EmptyArray);

        EcmaValue spreadable = CreateObject((Symbol.IsConcatSpreadable, true));
        Case(Proxy.Construct(spreadable, new EcmaObject()), EcmaValue.EmptyArray);
      });

      It("should return abrupt if argument is a revoked proxy object", () => {
        EcmaValue handle = Proxy.Invoke("revocable", EcmaArray.Of(), new EcmaObject());
        handle.Invoke("revoke");
        Case(handle["proxy"], Throws.TypeError);

        EcmaValue handle2 = default;
        handle2 = Proxy.Invoke("revocable", EcmaArray.Of(), CreateObject(("get", RuntimeFunction.Create((target, key) => {
          // Defer proxy revocation until after property access to ensure that the
          // expected TypeError originates from the IsArray operation.
          if (key == Symbol.IsConcatSpreadable) {
            handle2.Invoke("revoke");
          }
          return target[key];
        }))));
        Case(handle2["proxy"], Throws.TypeError);
      });

      It("should use species constructor to create a new instance", () => {
        int callCount = 0;
        EcmaValue instance = EcmaArray.Of();
        EcmaValue thisValue = default, args = default;
        EcmaValue ConstructorFn = RuntimeFunction.Create(() => {
          callCount++;
          thisValue = This;
          args = Arguments;
          return instance;
        });

        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, ConstructorFn));
        Case(arr, instance);
        That(callCount, Is.EqualTo(1), "Constructor invoked exactly once");
        That(Object.Invoke("getPrototypeOf", thisValue), Is.EqualTo(ConstructorFn["prototype"]));
        That(args, Is.EquivalentTo(new[] { 0 }));
      });

      It("should create Array exotic object if species constructor is undefined or null", () => {
        EcmaValue arr = EcmaArray.Of();

        arr["constructor"] = CreateObject((Symbol.Species, Undefined));
        That(Object.Invoke("getPrototypeOf", concat.Call(arr)), Is.EqualTo(Array.Prototype));
        arr["constructor"] = CreateObject((Symbol.Species, Null));
        That(Object.Invoke("getPrototypeOf", concat.Call(arr)), Is.EqualTo(Array.Prototype));
      });

      It("should return abrupt from ArraySpeciesCreate", () => {
        EcmaValue arr = EcmaArray.Of();

        arr["constructor"] = CreateObject((Symbol.Species, get: ThrowTest262Exception, set: null));
        Case(arr, Throws.Test262, "abrupt completion from accessing @@species");
        arr["constructor"] = CreateObject((Symbol.Species, ThrowTest262Exception));
        Case(arr, Throws.Test262, "abrupt completion from species constructor");
        arr["constructor"] = CreateObject((Symbol.Species, GlobalThis["parseInt"]));
        Case(arr, Throws.TypeError, "species constructor is a non-constructor object");

        arr["constructor"] = Null;
        Case(arr, Throws.TypeError, "constructor property is neither an Object nor undefined");
        arr["constructor"] = 1;
        Case(arr, Throws.TypeError, "constructor property is neither an Object nor undefined");
        arr["constructor"] = "string";
        Case(arr, Throws.TypeError, "constructor property is neither an Object nor undefined");
        arr["constructor"] = true;
        Case(arr, Throws.TypeError, "constructor property is neither an Object nor undefined");

        DefineProperty(arr, "constructor", get: ThrowTest262Exception);
        Case(arr, Throws.Test262);
      });

      It("should ignore constructor value for non-Array", () => {
        EcmaValue obj = CreateObject(("length", 0));
        DefineProperty(obj, "constructor", get: Intercept(() => Undefined));
        Logs.Clear();

        EcmaValue result = concat.Call(obj);
        That(Logs, Has.Exactly(0).Items);
        That(result, Is.EquivalentTo(new[] { obj }));
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Array.Prototype));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void CopyWithin(RuntimeFunction copyWithin) {
      IsUnconstructableFunctionWLength(copyWithin, "copyWithin", 2);
      IsAbruptedFromToObject(copyWithin);
      IsAbruptedFromToPrimitive(copyWithin.Bind(""));
      IsAbruptedFromToPrimitive(copyWithin.Bind("", 0));
      IsAbruptedFromToPrimitive(copyWithin.Bind("", 0, 0));
      IsAbruptedFromSymbolToNumber(copyWithin.Bind(""));
      IsAbruptedFromSymbolToNumber(copyWithin.Bind("", 0));
      IsAbruptedFromSymbolToNumber(copyWithin.Bind("", 0, 0));

      It("should return this value", () => {
        EcmaValue arr = new EcmaArray();
        Case((arr, 0, 0), arr);

        EcmaValue obj = CreateObject(("length", 0));
        Case((obj, 0, 0), obj);
      });

      It("should coerce end argument to integer values", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, Null), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, NaN), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, false), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, true), new[] { 0, 0, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, "-2"), new[] { 0, 0, 1, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, -2.5), new[] { 0, 0, 1, 3 });
      });

      It("should coerce start argument to integer values", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), 1, Undefined), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, Null), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, NaN), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, false), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, true), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, "1"), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0.5), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1.5), new[] { 1, 2, 3, 3 });
      });

      It("should coerce target argument to integer values", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), Undefined, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), Null, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), NaN, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), false, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), true, 0), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), "1", 0), new[] { 0, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0.5, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1.5, 0), new[] { 0, 0, 1, 2 });
      });

      It("should copy values with non-negative target and start positions", () => {
        Case((EcmaArray.Of("a", "b", "c", "d", "e", "f"), 0, 0), new[] { "a", "b", "c", "d", "e", "f" });
        Case((EcmaArray.Of("a", "b", "c", "d", "e", "f"), 0, 2), new[] { "c", "d", "e", "f", "e", "f" });
        Case((EcmaArray.Of("a", "b", "c", "d", "e", "f"), 3, 0), new[] { "a", "b", "c", "a", "b", "c" });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 1, 4), new[] { 0, 4, 5, 3, 4, 5 });

        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, Undefined), new[] { 1, 2, 3, 3 });
      });

      It("should copy values with non-negative target, start and end positions", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 0, 0), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 0, 2), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, 2), new[] { 1, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 1, 0, 2), new[] { 0, 0, 1, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 1, 3, 5), new[] { 0, 3, 4, 3, 4, 5 });
      });

      It("should handle negative argument", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, -1), new[] { 1, 2, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 2, 0, -1), new[] { 0, 1, 0, 1, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 1, 2, -2), new[] { 0, 2, 2, 3, 4 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, -2, -1), new[] { 2, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 2, -2, -1), new[] { 0, 1, 3, 3, 4 });
        Case((EcmaArray.Of(0, 1, 2, 3), -3, -2, -1), new[] { 0, 2, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -2, -3, -1), new[] { 0, 1, 2, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -5, -2, -1), new[] { 3, 1, 2, 3, 4 });

        Case((EcmaArray.Of(0, 1, 2, 3), 0, -1), new[] { 3, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 2, -2), new[] { 0, 1, 3, 4, 4 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 1, -2), new[] { 0, 3, 4, 3, 4 });
        Case((EcmaArray.Of(0, 1, 2, 3), -1, -2), new[] { 0, 1, 2, 2 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -2, -3), new[] { 0, 1, 2, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -5, -2), new[] { 3, 4, 2, 3, 4 });

        Case((EcmaArray.Of(0, 1, 2, 3), -1, 0), new[] { 0, 1, 2, 0 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -2, 2), new[] { 0, 1, 2, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), -1, 2), new[] { 0, 1, 2, 2 });
      });

      It("should handle out-of-bound argument", () => {
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, 6), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, Infinity), new[] { 1, 2, 3, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 1, 3, 6), new[] { 0, 3, 4, 5, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 1, 3, Infinity), new[] { 0, 3, 4, 5, 4, 5 });

        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 6, 0), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 7, 0), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), Infinity, 0), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 6, 2), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 7, 2), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), Infinity, 2), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 0, 6), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 0, 7), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 0, Infinity), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 2, 6), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 1, 7), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 3, Infinity), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 6, 6), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), 10, 10), new[] { 0, 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5), Infinity, Infinity), new[] { 0, 1, 2, 3, 4, 5 });

        Case((EcmaArray.Of(0, 1, 2, 3), 0, 1, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 0, 1, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, -2, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 0, -2, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3), 0, -9, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 0, -9, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3), -3, -2, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), -3, -2, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3), -7, -8, -9), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), -7, -8, -Infinity), new[] { 1, 2, 3, 4, 5 });

        Case((EcmaArray.Of(0, 1, 2, 3), 0, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 0, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 2, -10), new[] { 0, 1, 0, 1, 2 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 2, -Infinity), new[] { 1, 2, 1, 2, 3 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), 10, -10), new[] { 0, 1, 2, 3, 4 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), 10, -Infinity), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3), -9, -10), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), -9, -Infinity), new[] { 1, 2, 3, 4, 5 });

        Case((EcmaArray.Of(0, 1, 2, 3), -10, 0), new[] { 0, 1, 2, 3 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), -Infinity, 0), new[] { 1, 2, 3, 4, 5 });
        Case((EcmaArray.Of(0, 1, 2, 3, 4), -10, 2), new[] { 2, 3, 4, 3, 4 });
        Case((EcmaArray.Of(1, 2, 3, 4, 5), -Infinity, 2), new[] { 3, 4, 5, 4, 5 });
      });

      It("should copy holes", () => {
        EcmaValue arr = new EcmaArray();
        arr[0] = 0;
        arr[1] = 1;
        arr[4] = 1;
        copyWithin.Call(arr, 0, 1, 4);
        That(arr["length"], Is.EqualTo(5));
        That(arr[0], Is.EqualTo(1));
        That(arr[4], Is.EqualTo(1));
        That(!arr.HasOwnProperty(1));
        That(!arr.HasOwnProperty(2));
        That(!arr.HasOwnProperty(3));
      });

      It("should return abrupt from ToLength", () => {
        Case(CreateObject(("length", get: ThrowTest262Exception, set: null)), Throws.Test262);
        Case(CreateObject(("length", CreateObject(valueOf: ThrowTest262Exception))), Throws.Test262);
        Case(CreateObject(("length", new Symbol())), Throws.TypeError);
      });

      It("should return abrupt from HasProperty", () => {
        EcmaValue o = CreateObject(("0", true), ("length", 1));
        Case((Proxy.Construct(o, CreateObject(("has", ThrowTest262Exception))), 0, 0), Throws.Test262);
      });

      It("should return abrupt from getting property value", () => {
        EcmaValue o = CreateObject(("length", 1));
        DefineProperty(o, "0", get: ThrowTest262Exception);
        Case((o, 0, 0), Throws.Test262);
      });

      It("should return abrupt from setting property value", () => {
        EcmaValue o = CreateObject(("0", true), ("length", 43));
        DefineProperty(o, "42", set: _ => ThrowTest262Exception());
        Case((o, 42, 0), Throws.Test262);
      });

      It("should return abrupt from deleting property value", () => {
        EcmaValue o = CreateObject(("length", 43));
        DefineProperty(o, "42", configurable: false, writable: true);
        Case((o, 42, 0), Throws.TypeError);

        EcmaValue o1 = CreateObject(("42", true), ("length", 43));
        Case((Proxy.Construct(o1, CreateObject(("deleteProperty", ThrowTest262Exception))), 42, 0), Throws.Test262);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Entries(RuntimeFunction entries) {
      IsUnconstructableFunctionWLength(entries, "entries", 0);
      IsAbruptedFromToObject(entries);

      It("should return a valid iterator with %ArrayIteratorPrototype% as the prototype", () => {
        That(Object.Invoke("getPrototypeOf", entries.Call(EcmaArray.Of())), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
        That(Object.Invoke("getPrototypeOf", entries.Call(CreateObject(new { length = 2 }))), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
      });

      It("should return a valid iterator with the array's numeric properties", () => {
        EcmaValue iterator = entries.Call(EcmaArray.Of("a", "b", "c"));
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 0, "a" });
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 1, "b" });
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 2, "c" });
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = entries.Call(arr);
        arr.Push("a");
        VerifyIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 0, "a" });
        VerifyIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection, MaxTime(2000)]
    public void Every(RuntimeFunction every) {
      IsUnconstructableFunctionWLength(every, "every", 1);
      IsAbruptedFromToObject(every);

      It("should throw TypeError if callback is not callable", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case((EcmaArray.Of(), Undefined), Throws.TypeError);
        Case((EcmaArray.Of(), Null), Throws.TypeError);
        Case((EcmaArray.Of(), NaN), Throws.TypeError);
        Case((EcmaArray.Of(), Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), -Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), true), Throws.TypeError);
        Case((EcmaArray.Of(), "string"), Throws.TypeError);
        Case((EcmaArray.Of(), Number.Construct(42)), Throws.TypeError);
        Case((EcmaArray.Of(), Boolean.Construct(false)), Throws.TypeError);
        Case((EcmaArray.Of(), String.Construct("")), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaObject()), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaArray()), Throws.TypeError);
      });

      It("should access length property before checking callback argument", () => {
        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "length", get: ThrowTest262Exception);
        Case(obj, Throws.Test262);
      });

      It("should return true when all calls to callback return true", () => {
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5, 6, 7, 8, 9), RuntimeFunction.Create(() => true)), true);
        Case((EcmaArray.Of(0, 1, 2, 3, 4, 5, 6, 7, 8, 9), RuntimeFunction.Create((v, i) => i == 9 ? false : true)), false);
      });

      It("should return true when length is zero", () => {
        RuntimeFunction cb = RuntimeFunction.Create(() => Undefined);
        Case((EcmaArray.Of(), cb), true);

        Case((CreateObject(("length", 0), (0, "1")), cb), true);
        Case((CreateObject(("length", Null), (0, "1")), cb), true);
        Case((CreateObject(("length", "0"), (0, "1")), cb), true);
        Case((CreateObject(("length", false), (0, "1")), cb), true);
        Case((CreateObject(("length", CreateObject(toString: () => "0")), (0, "1")), cb), true);
        Case((CreateObject(("length", CreateObject(valueOf: () => 0)), (0, "1")), cb), true);
      });

      It("should coerce length of array-like objects to integer between 0 and 2^53-1", () => {
        Case((CreateObject(("length", 2.5), (0, true), (1, true), (2, false)), RuntimeFunction.Create(v => v)), true);
        Case((CreateObject(("length", "2"), (0, true), (1, true), (2, false)), RuntimeFunction.Create(v => v)), true);
        Case((CreateObject(("length", -Infinity), (0, false), (1, false), (2, false)), RuntimeFunction.Create(v => v)), true);
        Case((CreateObject(("length", NaN), (0, false), (1, false), (2, false)), RuntimeFunction.Create(v => v)), true);
        Case((CreateObject(("length", CreateObject(valueOf: () => "2")), (0, true), (1, true), (2, false)), RuntimeFunction.Create(v => v)), true);
        Case(CreateObject(("length", new Symbol())), Throws.TypeError);
      });

      It("should only visit properties whose name is an array index", () => {
        EcmaValue arr = EcmaArray.Of(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        arr["i"] = 10;
        arr[true] = 11;

        Logs.Clear();
        every.Call(arr, Intercept(() => true));
        That(Logs, Has.Exactly(10).Items);
      });

      It("should coerce return value of callbackFn to boolean", () => {
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => Undefined)), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => Null)), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => NaN)), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => 0)), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => -0d)), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => Infinity)), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => -Infinity)), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => 5)), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => -5)), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => "")), false);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => "string")), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => Number.Construct(0))), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => String.Construct(""))), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => Boolean.Construct(false))), true);
        Case((EcmaArray.Of(11), RuntimeFunction.Create(() => EcmaArray.Of())), true);
      });

      It("should call predicate with correct arguments and in correct sequence", () => {
        EcmaArray args = new EcmaArray();
        EcmaValue obj = new EcmaObject();
        EcmaValue arr = EcmaArray.Of(0, 1, true, Null, obj, "five");
        arr[7] = "seven";
        every.Call(arr, RuntimeFunction.Create(() => Return(args.Push(Arguments), true)));
        That(args[0], Is.EquivalentTo(new[] { 0, 0, arr }));
        That(args[1], Is.EquivalentTo(new[] { 1, 1, arr }));
        That(args[2], Is.EquivalentTo(new[] { true, 2, arr }));
        That(args[3], Is.EquivalentTo(new[] { Null, 3, arr }));
        That(args[4], Is.EquivalentTo(new[] { obj, 4, arr }));
        That(args[5], Is.EquivalentTo(new[] { "five", 5, arr }));
        That(args[6], Is.EquivalentTo(new[] { "seven", 7, arr }));
      });

      It("should call predicate with this value", () => {
        EcmaValue thisValue = default;
        every.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)));
        That(thisValue, Is.Undefined);

        EcmaValue obj = new EcmaObject();
        every.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)), obj);
        That(thisValue, Is.EqualTo(obj));
      });

      It("should immediately return false if callback returns false", () => {
        Logs.Clear();
        Case((EcmaArray.Of(1, 2, 3), Intercept(() => 0)), false);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should return abrupt from property accessor", () => {
        Logs.Clear();
        EcmaValue obj = CreateObject(new { length = 2 });
        DefineProperty(obj, 0, get: Intercept(ThrowTest262Exception));
        DefineProperty(obj, 1, get: Intercept(ThrowTest262Exception));
        Case((obj, RuntimeFunction.Create(() => Undefined)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should return abrupt from callback", () => {
        Logs.Clear();
        Case((EcmaArray.Of(1, 2, 3), Intercept(ThrowTest262Exception)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should not see added elements whose array index is larger than original length", () => {
        Logs.Clear();
        every.Call(EcmaArray.Of(0, 1, 2), Intercept((v, i, o) => Return(o[3] = 3, true)));
        That(Logs, Has.Exactly(3).Items);

        Logs.Clear();
        every.Call(CreateObject(("length", 1), ("0", 0), ("99", 99)), Intercept((v, i, o) => Return(o["length"] = 100, o[1] = 1, true)));
        That(Logs, Has.Exactly(1).Items);
      });

      It("should not see deleted elements before visit", () => {
        Case((EcmaArray.Of(1, 2), RuntimeFunction.Create((v, i, o) => Return(o.ToObject().Delete(1), i))), false);
      });

      It("should see updated elements before visit", () => {
        Case((EcmaArray.Of(1, 2, 3), RuntimeFunction.Create((v, i, o) => Return(o.Invoke("fill", 0, 0), v > 0))), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Fill(RuntimeFunction fill) {
      IsUnconstructableFunctionWLength(fill, "fill", 1);
      IsAbruptedFromToObject(fill);
      IsAbruptedFromToPrimitive(fill.Bind(EcmaArray.Of(), 1));
      IsAbruptedFromToPrimitive(fill.Bind(EcmaArray.Of(), 1, 0));
      IsAbruptedFromSymbolToNumber(fill.Bind(EcmaArray.Of(), 1));
      IsAbruptedFromSymbolToNumber(fill.Bind(EcmaArray.Of(), 1, 0));

      It("should not alter array with length is zero", () => {
        EcmaArray arr = EcmaArray.Of();
        Object.Invoke("freeze", arr);
        Case((arr, 8), Throws.Nothing);
      });

      Case(EcmaArray.Of(0, 0), new[] { Undefined, Undefined },
        "fill with undefined if value is not specified");
      Case((EcmaArray.Of(0, 0, 0), 8), new[] { 8, 8, 8 },
        "fill all elements if start and end is not specified");

      It("should return this value", () => {
        EcmaValue arr = new EcmaArray();
        Case((arr, 1), arr);

        EcmaValue obj = CreateObject(new { length = 0 });
        Case((obj, 1), obj);
      });

      It("should return abrupt from this length", () => {
        EcmaValue o1 = new EcmaObject();
        DefineProperty(o1, "length", get: ThrowTest262Exception);
        Case(o1, Throws.Test262);

        EcmaValue o2 = new EcmaObject();
        o2["length"] = CreateObject(valueOf: ThrowTest262Exception);
        Case(o2, Throws.Test262);

        EcmaValue o3 = CreateObject(new { length = new Symbol() });
        Case(o3, Throws.TypeError);
      });

      It("should return abrupt from setting a property value", () => {
        EcmaValue o1 = CreateObject(new { length = 1 });
        DefineProperty(o1, "0", set: _ => ThrowTest262Exception());
        Case(o1, Throws.Test262);
      });

      It("should coerce start and end argument to integer", () => {
        Case((EcmaArray.Of(0, 0), 1, Undefined), Is.EquivalentTo(new[] { 1, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, Undefined), Is.EquivalentTo(new[] { 1, 1 }));
        Case((EcmaArray.Of(0, 0), 1, Null), Is.EquivalentTo(new[] { 1, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, Null), Is.EquivalentTo(new[] { 0, 0 }));
        Case((EcmaArray.Of(0, 0), 1, true), Is.EquivalentTo(new[] { 0, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, true), Is.EquivalentTo(new[] { 1, 0 }));
        Case((EcmaArray.Of(0, 0), 1, false), Is.EquivalentTo(new[] { 1, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, false), Is.EquivalentTo(new[] { 0, 0 }));
        Case((EcmaArray.Of(0, 0), 1, NaN), Is.EquivalentTo(new[] { 1, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, NaN), Is.EquivalentTo(new[] { 0, 0 }));
        Case((EcmaArray.Of(0, 0), 1, "1"), Is.EquivalentTo(new[] { 0, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, "1"), Is.EquivalentTo(new[] { 1, 0 }));
        Case((EcmaArray.Of(0, 0), 1, 1.5), Is.EquivalentTo(new[] { 0, 1 }));
        Case((EcmaArray.Of(0, 0), 1, 0, 1.5), Is.EquivalentTo(new[] { 1, 0 }));
      });

      It("should fill all the elements from start to end", () => {
        Case((EcmaArray.Of(0, 0, 0), 8, 1), Is.EquivalentTo(new[] { 0, 8, 8 }));
        Case((EcmaArray.Of(0, 0, 0), 8, 4), Is.EquivalentTo(new[] { 0, 0, 0 }));
        Case((EcmaArray.Of(0, 0, 0), 8, -1), Is.EquivalentTo(new[] { 0, 0, 8 }));

        Case((EcmaArray.Of(0, 0, 0), 8, 0, 1), Is.EquivalentTo(new[] { 8, 0, 0 }));
        Case((EcmaArray.Of(0, 0, 0), 8, 0, -1), Is.EquivalentTo(new[] { 8, 8, 0 }));
        Case((EcmaArray.Of(0, 0, 0), 8, 0, 5), Is.EquivalentTo(new[] { 8, 8, 8 }));

        Case((EcmaArray.Of(0, 0, 0), 8, 1, 2), Is.EquivalentTo(new[] { 0, 8, 0 }));
        Case((EcmaArray.Of(0, 0, 0, 0, 0), 8, -3, 4), Is.EquivalentTo(new[] { 0, 0, 8, 8, 0 }));
        Case((EcmaArray.Of(0, 0, 0, 0, 0), 8, -2, -1), Is.EquivalentTo(new[] { 0, 0, 0, 8, 0 }));
        Case((EcmaArray.Of(0, 0, 0, 0, 0), 8, -1, -3), Is.EquivalentTo(new[] { 0, 0, 0, 0, 0 }));

        EcmaValue arrWithHoles = new EcmaArray();
        arrWithHoles[4] = 0;
        Case((arrWithHoles, 8, 1, 3), Is.EquivalentTo(new[] { Undefined, 8, 8, Undefined, 0 }));
        That(!arrWithHoles.HasProperty(0));
        That(!arrWithHoles.HasProperty(3));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Find(RuntimeFunction find) {
      IsUnconstructableFunctionWLength(find, "find", 1);
      IsAbruptedFromToObject(find);

      It("should throw TypeError if callback is not callable", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case((EcmaArray.Of(), Undefined), Throws.TypeError);
        Case((EcmaArray.Of(), Null), Throws.TypeError);
        Case((EcmaArray.Of(), NaN), Throws.TypeError);
        Case((EcmaArray.Of(), Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), -Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), true), Throws.TypeError);
        Case((EcmaArray.Of(), "string"), Throws.TypeError);
        Case((EcmaArray.Of(), Number.Construct(42)), Throws.TypeError);
        Case((EcmaArray.Of(), Boolean.Construct(false)), Throws.TypeError);
        Case((EcmaArray.Of(), String.Construct("")), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaObject()), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaArray()), Throws.TypeError);
      });

      It("should access length property before checking callback argument", () => {
        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "length", get: ThrowTest262Exception);
        Case(obj, Throws.Test262);
      });

      It("should return found value if predicate return a boolean true value", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        int called = 0;
        Case((arr, RuntimeFunction.Create(() => Return(called++, true))), "Shoes");
        That(called, Is.EqualTo(1));

        called = 0;
        Case((arr, RuntimeFunction.Create(v => Return(called++, v == "Bike"))), "Bike");
        That(called, Is.EqualTo(3));
      });

      It("should return undefined if predicate always returns a boolean false value", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        int called = 0;
        Case((arr, RuntimeFunction.Create(() => Return(called++, false))), Undefined);
        That(called, Is.EqualTo(3));
      });

      It("should coerce return value of callbackFn to boolean", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        Case((arr, RuntimeFunction.Create(() => Undefined)), Undefined);
        Case((arr, RuntimeFunction.Create(() => Null)), Undefined);
        Case((arr, RuntimeFunction.Create(() => NaN)), Undefined);
        Case((arr, RuntimeFunction.Create(() => 0)), Undefined);
        Case((arr, RuntimeFunction.Create(() => -0d)), Undefined);
        Case((arr, RuntimeFunction.Create(() => Infinity)), "Shoes");
        Case((arr, RuntimeFunction.Create(() => -Infinity)), "Shoes");
        Case((arr, RuntimeFunction.Create(() => 5)), "Shoes");
        Case((arr, RuntimeFunction.Create(() => -5)), "Shoes");
        Case((arr, RuntimeFunction.Create(() => "")), Undefined);
        Case((arr, RuntimeFunction.Create(() => "string")), "Shoes");
        Case((arr, RuntimeFunction.Create(() => Number.Construct(0))), "Shoes");
        Case((arr, RuntimeFunction.Create(() => String.Construct(""))), "Shoes");
        Case((arr, RuntimeFunction.Create(() => Boolean.Construct(false))), "Shoes");
        Case((arr, RuntimeFunction.Create(() => EcmaArray.Of())), "Shoes");
      });

      It("should call predicate for each array entry and holes", () => {
        EcmaValue arr = EcmaArray.Of("Mike", "Rick", "Leo");
        EcmaValue results = new EcmaArray();
        find.Call(arr, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
        That(results, Is.EquivalentTo(new[] {
          new[] { "Mike", 0, arr },
          new[] { "Rick", 1, arr },
          new[] { "Leo", 2, arr }
        }));

        arr = EcmaArray.Of();
        arr[0] = Undefined;
        arr[3] = "foo";
        int called = 0;
        find.Call(arr, RuntimeFunction.Create(() => Void(called++)));
        That(called, Is.EqualTo(4));
      });

      It("should call predicate with this value", () => {
        EcmaValue thisValue = default;
        find.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)));
        That(thisValue, Is.Undefined);

        EcmaValue obj = new EcmaObject();
        find.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)), obj);
        That(thisValue, Is.EqualTo(obj));
      });

      It("should return abrupt from property accessor", () => {
        Logs.Clear();
        EcmaValue obj = CreateObject(new { length = 2 });
        DefineProperty(obj, 0, get: Intercept(ThrowTest262Exception));
        DefineProperty(obj, 1, get: Intercept(ThrowTest262Exception));
        Case((obj, RuntimeFunction.Create(() => Undefined)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should return abrupt from callback", () => {
        Logs.Clear();
        Case((EcmaArray.Of(1, 2, 3), Intercept(ThrowTest262Exception)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should see array altered during iteration", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        EcmaValue results = new EcmaArray();
        find.Call(arr, RuntimeFunction.Create(v => {
          if (results["length"] == 0) {
            arr.Invoke("splice", 1, 1);
          }
          results.Invoke("push", v);
        }));
        That(results, Is.EquivalentTo(new[] { "Shoes", "Bike", Undefined }));

        arr = EcmaArray.Of("Skateboard", "Barefoot");
        results = new EcmaArray();
        find.Call(arr, RuntimeFunction.Create(v => {
          if (results["length"] == 0) {
            arr.Invoke("push", "Motorcycle");
            arr[1] = "Magic Carpet";
          }
          results.Invoke("push", v);
        }));
        That(results, Is.EquivalentTo(new[] { "Skateboard", "Magic Carpet" }));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void FindIndex(RuntimeFunction findIndex) {
      IsUnconstructableFunctionWLength(findIndex, "findIndex", 1);
      IsAbruptedFromToObject(findIndex);

      It("should throw TypeError if callback is not callable", () => {
        Case(EcmaArray.Of(), Throws.TypeError);
        Case((EcmaArray.Of(), Undefined), Throws.TypeError);
        Case((EcmaArray.Of(), Null), Throws.TypeError);
        Case((EcmaArray.Of(), NaN), Throws.TypeError);
        Case((EcmaArray.Of(), Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), -Infinity), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), true), Throws.TypeError);
        Case((EcmaArray.Of(), "string"), Throws.TypeError);
        Case((EcmaArray.Of(), Number.Construct(42)), Throws.TypeError);
        Case((EcmaArray.Of(), Boolean.Construct(false)), Throws.TypeError);
        Case((EcmaArray.Of(), String.Construct("")), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaObject()), Throws.TypeError);
        Case((EcmaArray.Of(), new EcmaArray()), Throws.TypeError);
      });

      It("should access length property before checking callback argument", () => {
        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "length", get: ThrowTest262Exception);
        Case(obj, Throws.Test262);
      });

      It("should return index if predicate return a boolean true value", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        int called = 0;
        Case((arr, RuntimeFunction.Create(() => Return(called++, true))), 0);
        That(called, Is.EqualTo(1));

        called = 0;
        Case((arr, RuntimeFunction.Create(v => Return(called++, v == "Bike"))), 2);
        That(called, Is.EqualTo(3));
      });

      It("should return -1 if predicate always returns a boolean false value", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        int called = 0;
        Case((arr, RuntimeFunction.Create(() => Return(called++, false))), -1);
        That(called, Is.EqualTo(3));
      });

      It("should coerce return value of callbackFn to boolean", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        Case((arr, RuntimeFunction.Create(() => Undefined)), -1);
        Case((arr, RuntimeFunction.Create(() => Null)), -1);
        Case((arr, RuntimeFunction.Create(() => NaN)), -1);
        Case((arr, RuntimeFunction.Create(() => 0)), -1);
        Case((arr, RuntimeFunction.Create(() => -0d)), -1);
        Case((arr, RuntimeFunction.Create(() => Infinity)), 0);
        Case((arr, RuntimeFunction.Create(() => -Infinity)), 0);
        Case((arr, RuntimeFunction.Create(() => 5)), 0);
        Case((arr, RuntimeFunction.Create(() => -5)), 0);
        Case((arr, RuntimeFunction.Create(() => "")), -1);
        Case((arr, RuntimeFunction.Create(() => "string")),0);
        Case((arr, RuntimeFunction.Create(() => Number.Construct(0))), 0);
        Case((arr, RuntimeFunction.Create(() => String.Construct(""))), 0);
        Case((arr, RuntimeFunction.Create(() => Boolean.Construct(false))), 0);
        Case((arr, RuntimeFunction.Create(() => EcmaArray.Of())), 0);
      });

      It("should call predicate for each array entry and holes", () => {
        EcmaValue arr = EcmaArray.Of("Mike", "Rick", "Leo");
        EcmaValue results = new EcmaArray();
        findIndex.Call(arr, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
        That(results, Is.EquivalentTo(new[] {
          new[] { "Mike", 0, arr },
          new[] { "Rick", 1, arr },
          new[] { "Leo", 2, arr }
        }));

        arr = EcmaArray.Of();
        arr[0] = Undefined;
        arr[3] = "foo";
        int called = 0;
        findIndex.Call(arr, RuntimeFunction.Create(() => Void(called++)));
        That(called, Is.EqualTo(4));
      });

      It("should call predicate with this value", () => {
        EcmaValue thisValue = default;
        findIndex.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)));
        That(thisValue, Is.Undefined);

        EcmaValue obj = new EcmaObject();
        findIndex.Call(EcmaArray.Of(1), RuntimeFunction.Create(() => Void(thisValue = This)), obj);
        That(thisValue, Is.EqualTo(obj));
      });

      It("should return abrupt from property accessor", () => {
        Logs.Clear();
        EcmaValue obj = CreateObject(new { length = 2 });
        DefineProperty(obj, 0, get: Intercept(ThrowTest262Exception));
        DefineProperty(obj, 1, get: Intercept(ThrowTest262Exception));
        Case((obj, RuntimeFunction.Create(() => Undefined)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should return abrupt from callback", () => {
        Logs.Clear();
        Case((EcmaArray.Of(1, 2, 3), Intercept(ThrowTest262Exception)), Throws.Test262);
        That(Logs, Has.Exactly(1).Items);
      });

      It("should see array altered during iteration", () => {
        EcmaValue arr = EcmaArray.Of("Shoes", "Car", "Bike");
        EcmaValue results = new EcmaArray();
        findIndex.Call(arr, RuntimeFunction.Create(v => {
          if (results["length"] == 0) {
            arr.Invoke("splice", 1, 1);
          }
          results.Invoke("push", v);
        }));
        That(results, Is.EquivalentTo(new[] { "Shoes", "Bike", Undefined }));

        arr = EcmaArray.Of("Skateboard", "Barefoot");
        results = new EcmaArray();
        findIndex.Call(arr, RuntimeFunction.Create(v => {
          if (results["length"] == 0) {
            arr.Invoke("push", "Motorcycle");
            arr[1] = "Magic Carpet";
          }
          results.Invoke("push", v);
        }));
        That(results, Is.EquivalentTo(new[] { "Skateboard", "Magic Carpet" }));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Includes(RuntimeFunction includes) {
      IsUnconstructableFunctionWLength(includes, "includes", 1);
      IsAbruptedFromToObject(includes);

      Case(EcmaArray.Of(0), false);
      Case(EcmaArray.Of(Undefined), true);

      Case((EcmaArray.Of(7, 7, 7, 7), 7, 4), false);
      Case((EcmaArray.Of(7, 7, 7, 7), 7, 5), false);
      Case((EcmaArray.Of(7, 7, 7, 7), 7, Infinity), false);
      Case((EcmaArray.Of(7, 7, 7, 7), 7, -Infinity), true);
      Case((EcmaArray.Of(42, 43), 42, -0d), true);
      Case((EcmaArray.Of(42, 43), 43, -0d), true);
      Case((EcmaArray.Of(42, 43), 44, -0d), false);

      Logs.Clear();
      Case((Proxy.Construct(new EcmaObject(), CreateObject(new { get = Intercept((o, k) => k == "length" ? 4 : k * 10, "{1}") })), 42), false);
      CollectionAssert.AreEqual(new[] { "length", "0", "1", "2", "3" }, Logs);


    }

    [Test, RuntimeFunctionInjection]
    public void IndexOf(RuntimeFunction indexOf) {
      IsUnconstructableFunctionWLength(indexOf, "indexOf", 1);
      IsAbruptedFromToObject(indexOf);

      It("should return index of the first eligible element", () => {
        Case(EcmaArray.Of(Undefined), 0);
        Case((EcmaArray.Of(Undefined), Undefined), 0);
        Case((EcmaArray.Of(Null), Null), 0);
        Case((EcmaArray.Of(NaN), NaN), -1);
        Case((EcmaArray.Of(0), -0d), 0);
        Case((EcmaArray.Of(-0d), 0), 0);
        Case((EcmaArray.Of(-1, 0, 1), 1), 2);
        Case((EcmaArray.Of("", "ab", "bca", "abc"), "abc"), 3);
        Case((EcmaArray.Of(false, true), true), 1);

        EcmaValue obj = new EcmaObject();
        Case((EcmaArray.Of(new EcmaObject(), new EcmaObject(), obj), obj), 2);

        Case((EcmaArray.Of(1, 2, 2, 1, 2), 2), 1);
      });

      It("should return -1 for elements not present in array", () => {
        EcmaValue arr = new EcmaArray();
        arr[100] = 1;
        arr[99999] = "";
        arr[10] = new EcmaObject();
        arr[5555] = 5.5;
        arr[123456] = "str";
        arr[5] = 1E+308;

        Case((arr, 1), 100);
        Case((arr, ""), 99999);
        Case((arr, "str"), 123456);
        Case((arr, 1E+308), 5);
        Case((arr, 5.5), 5555);
        Case((arr, true), -1);
        Case((arr, 5), -1);
        Case((arr, "str1"), -1);
        Case((arr, Null), -1);
        Case((arr, new EcmaObject()), -1);

        Case((EcmaArray.Of("true"), true), -1);
        Case((EcmaArray.Of("0"), 0), -1);
        Case((EcmaArray.Of(false), 0), -1);
        Case((EcmaArray.Of(Undefined), 0), -1);
        Case((EcmaArray.Of(Null), 0), -1);
      });

      It("should coerce length to integer", () => {
        Case((CreateObject(("length", Undefined), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", true), (0, 0), (1, 1)), 0), 0);
        Case((CreateObject(("length", true), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", NaN), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", Infinity), (0, 0), (1, 1)), 0), 0);
        Case((CreateObject(("length", -Infinity), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", -1), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", 1.9), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", ""), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", "2"), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", "2.1"), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(toString: () => "2")), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(toString: () => "2", valueOf: () => new EcmaObject())), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2)), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2, toString: () => 1)), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2, toString: () => new EcmaObject())), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => new EcmaObject(), toString: () => new EcmaObject())), (0, 0), (1, 1)), 1), Throws.TypeError);
      });

      It("should coerce fromIndex to integer", () => {
        Case((EcmaArray.Of(1, 2, 1, 2, 1, 2), 2, "2"), 3);
        Case((EcmaArray.Of(1, 2, 1, 2, 1, 2), 2, "one"), 1);
        Case((EcmaArray.Of(1, 2, 3), 3, 0.49), 2);
        Case((EcmaArray.Of(1, 2, 3), 1, 0.51), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, 1.51), -1);
        Case((EcmaArray.Of(1, 2, 3), 1, true), -1);
        Case((EcmaArray.Of(1, 2, 3), 1, false), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, Undefined), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, Null), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, NaN), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, Infinity), -1);
        Case((EcmaArray.Of(1, 2, 3), 1, -Infinity), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, 0), 0);
        Case((EcmaArray.Of(1, 2, 3), 1, -0d), 0);
        Case((EcmaArray.Of(1, 2, 3), 2, -1), -1);
        Case((EcmaArray.Of(1, 2, 3), 3, -1), 2);
        Case((EcmaArray.Of(1, 2, 3), 2, -1.5), -1);
        Case((EcmaArray.Of(1, 2, 3), 3, -1.5), 2);
        Case((EcmaArray.Of(1, 2, 3), 2, 2.5), -1);
        Case((EcmaArray.Of(1, 2, 3), 3, 2.5), 2);
        Case((EcmaArray.Of(1, 2, 3), 1, "Infinity"), -1);
        Case((EcmaArray.Of(1, 2, 3), 1, "-Infinity"), 0);
        Case((EcmaArray.Of(1, 2, 3), 2, "-1"), -1);
        Case((EcmaArray.Of(1, 2, 3), 3, "-1"), 2);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => "1")), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => "1", valueOf: () => new EcmaObject())), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1)), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1, toString: () => "2")), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1, toString: () => new EcmaObject())), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject())), Throws.TypeError);
      });

      It("should not access subsequent element once search value is found", () => {
        EcmaValue arr = EcmaArray.Of(1, 2, 1, 2);
        DefineProperty(arr, 2, get: Intercept(() => 2));
        DefineProperty(arr, 3, get: Intercept(() => 2));
        Logs.Clear();
        Case((arr, 2), 1);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should not access any other properties if length is 0", () => {
        EcmaValue obj = CreateObject(("length", 0));
        DefineProperty(obj, 0, get: Intercept(() => 1));
        Logs.Clear();
        Case((obj, 1), -1);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should use [[HasProperty]] to check for existing elements", () => {
        EcmaValue arr = EcmaArray.Of(1, Null, 3);
        Object.Invoke("setPrototypeOf", arr, CreateProxyCompleteTraps(Array.Prototype, CreateObject(new { has = RuntimeFunction.Create((t, pk) => pk.In(t)) })));
        indexOf.Call(arr, 100, CreateObject(valueOf: () => Return(arr["length"] = 0, 0)));
      });

      It("should not match non-existent property if search value is undefined", () => {
        EcmaValue arr = EcmaArray.Of();
        arr[0] = 0;
        arr[2] = 2;
        Case((arr, Undefined), -1);
      });

      It("should see inherited property", () => {
        using (TempProperty(Object.Prototype, 0, true)) {
          Case((CreateObject(new { length = 3 }), true), 0);
        }
      });

      It("should see updated elements during iteration", () => {
        EcmaValue arr = EcmaArray.Of(false, false);
        DefineProperty(arr, 0, get: () => Return(arr[1] = true, false));
        Case((arr, true), 1);
      });

      It("should not see deleted elements during iteration", () => {
        EcmaValue arr = EcmaArray.Of(0, 1, 2, "last");
        DefineProperty(arr, 0, get: () => Return(arr["length"] = 3, 0));
        Case((arr, "last"), -1);

        arr = EcmaArray.Of(0, 1, 2);
        DefineProperty(arr, 0, get: () => Return(arr.ToObject().Delete(1), 0));
        Case((arr, 1), -1);
      });

      It("should not have iteration count affected by changed length of array during iteration", () => {
        EcmaValue arr = EcmaArray.Of(0, 1, 2);
        DefineProperty(arr, 1, get: () => Return(arr["length"] = 2, 1));
        using (TempProperty(Object.Prototype, 2, "prototype")) {
          Case((arr, "prototype"), 2);
        }

        arr = EcmaArray.Of(0, 1, 2, 3);
        DefineProperty(arr, 1, get: () => Return(arr[4] = 4, 1));
        Case((arr, 4), -1);
      });

      It("should return abrupt immediately without access subsequent element", () => {
        EcmaValue obj = CreateObject(("length", 2));
        DefineProperty(obj, 0, get: ThrowTest262Exception);
        DefineProperty(obj, 1, get: Intercept(() => _));
        Logs.Clear();
        Case((obj, true), Throws.Test262);
        That(Logs, Has.Exactly(0).Items);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Keys(RuntimeFunction keys) {
      IsUnconstructableFunctionWLength(keys, "keys", 0);
      IsAbruptedFromToObject(keys);

      It("should return a valid iterator with %ArrayIteratorPrototype% as the prototype", () => {
        That(Object.Invoke("getPrototypeOf", keys.Call(EcmaArray.Of())), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
        That(Object.Invoke("getPrototypeOf", keys.Call(CreateObject(new { length = 2 }))), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
      });

      It("should return a valid iterator with the array's numeric properties", () => {
        EcmaValue iterator = keys.Call(EcmaArray.Of("a", "b", "c"));
        VerifyIteratorResult(iterator.Invoke("next"), false, 0);
        VerifyIteratorResult(iterator.Invoke("next"), false, 1);
        VerifyIteratorResult(iterator.Invoke("next"), false, 2);
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = keys.Call(arr);
        arr.Push("a");
        VerifyIteratorResult(iterator.Invoke("next"), false, 0);
        VerifyIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void LastIndexOf(RuntimeFunction lastIndexOf) {
      IsUnconstructableFunctionWLength(lastIndexOf, "lastIndexOf", 1);
      IsAbruptedFromToObject(lastIndexOf);

      It("should return index of the last eligible element", () => {
        Case(EcmaArray.Of(Undefined), 0);
        Case((EcmaArray.Of(Undefined), Undefined), 0);
        Case((EcmaArray.Of(Null), Null), 0);
        Case((EcmaArray.Of(NaN), NaN), -1);
        Case((EcmaArray.Of(0), -0d), 0);
        Case((EcmaArray.Of(-0d), 0), 0);
        Case((EcmaArray.Of(-1, 0, 1), 1), 2);
        Case((EcmaArray.Of("", "ab", "bca", "abc"), "abc"), 3);
        Case((EcmaArray.Of(false, true), true), 1);

        EcmaValue obj = new EcmaObject();
        Case((EcmaArray.Of(obj, new EcmaObject()), obj), 0);
        Case((CreateObject(("length", 4294967296), (0, obj), (4294967294, obj), (4294967295, obj)), obj), 4294967295);

        Case((EcmaArray.Of(2, 1, 2, 2, 1), 2), 3);
      });

      It("should return -1 for elements not present in array", () => {
        EcmaValue arr = new EcmaArray();
        arr[100] = 1;
        arr[99999] = "";
        arr[10] = new EcmaObject();
        arr[5555] = 5.5;
        arr[123456] = "str";
        arr[5] = 1E+308;

        Case((arr, 1), 100);
        Case((arr, ""), 99999);
        Case((arr, "str"), 123456);
        Case((arr, 1E+308), 5);
        Case((arr, 5.5), 5555);
        Case((arr, true), -1);
        Case((arr, 5), -1);
        Case((arr, "str1"), -1);
        Case((arr, Null), -1);
        Case((arr, new EcmaObject()), -1);

        Case((EcmaArray.Of("true"), true), -1);
        Case((EcmaArray.Of("0"), 0), -1);
        Case((EcmaArray.Of(false), 0), -1);
        Case((EcmaArray.Of(Undefined), 0), -1);
        Case((EcmaArray.Of(Null), 0), -1);
      });

      It("should coerce length to integer", () => {
        Case((CreateObject(("length", Undefined), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", true), (0, 0), (1, 1)), 0), 0);
        Case((CreateObject(("length", true), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", NaN), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", -Infinity), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", -1), (0, 0), (1, 1)), 0), -1);
        Case((CreateObject(("length", 1.9), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", ""), (0, 0), (1, 1)), 1), -1);
        Case((CreateObject(("length", "2"), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", "2.1"), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(toString: () => "2")), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(toString: () => "2", valueOf: () => new EcmaObject())), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2)), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2, toString: () => 1)), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => 2, toString: () => new EcmaObject())), (0, 0), (1, 1)), 1), 1);
        Case((CreateObject(("length", CreateObject(valueOf: () => new EcmaObject(), toString: () => new EcmaObject())), (0, 0), (1, 1)), 1), Throws.TypeError);
      });

      It("should coerce fromIndex to integer", () => {
        Case((EcmaArray.Of(0, 1, 1), 1, "1"), 1);
        Case((EcmaArray.Of(0, 1, 1), 1, "one"), -1);
        Case((EcmaArray.Of(1, 2, 1), 2, 1.49), 1);
        Case((EcmaArray.Of(1, 2, 1), 2, 0.51), -1);
        Case((EcmaArray.Of(1, 2, 1), 1, 0.51), 0);
        Case((EcmaArray.Of(1, 2, 1), 2, true), 1);
        Case((EcmaArray.Of(1, 2, 1), 2, false), -1);
        Case((EcmaArray.Of(1, 2, 1), 2, Undefined), -1);
        Case((EcmaArray.Of(1, 2, 1), 1, Undefined), 0);
        Case((EcmaArray.Of(1, 2, 1), 2, Null), -1);
        Case((EcmaArray.Of(1, 2, 1), 1, Null), 0);
        Case((EcmaArray.Of(1, 2, 1), 2, -0d), -1);
        Case((EcmaArray.Of(1, 2, 1), 1, -0d), 0);
        Case((EcmaArray.Of(0, 1, 2), 1, 1.5), 1);
        Case((EcmaArray.Of(0, 1, 2), 2, 1.5), -1);
        Case((EcmaArray.Of(0, 1, 2), 1, -2.5), 1);
        Case((EcmaArray.Of(0, 1, 2), 2, -2.5), -1);
        Case((EcmaArray.Of(0, 1, 2), 0, -Infinity), -1);
        Case((EcmaArray.Of(0, 1, 2), 1, NaN), -1);
        Case((EcmaArray.Of(0, 1, 2), 0, NaN), 0);
        Case((EcmaArray.Of(0, 1, 2), 1, "-2"), 1);
        Case((EcmaArray.Of(0, 1, 2), 2, "-2"), -1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => "1")), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => "1", valueOf: () => new EcmaObject())), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1)), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1, toString: () => "2")), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(valueOf: () => 1, toString: () => new EcmaObject())), 1);
        Case((EcmaArray.Of(1, 2, 3), 2, CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject())), Throws.TypeError);
      });

      It("should not access subsequent element once search value is found", () => {
        EcmaValue arr = EcmaArray.Of(1, 2, 1, 2);
        DefineProperty(arr, 2, get: Intercept(() => 1));
        DefineProperty(arr, 1, get: Intercept(() => 2));
        Logs.Clear();
        Case((arr, 2), 3);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should not access any other properties if length is 0", () => {
        EcmaValue obj = CreateObject(("length", 0));
        DefineProperty(obj, 0, get: Intercept(() => 1));
        Logs.Clear();
        Case((obj, 1), -1);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should use [[HasProperty]] to check for existing elements", () => {
        EcmaValue arr = EcmaArray.Of(1, Null, 3);
        Object.Invoke("setPrototypeOf", arr, CreateProxyCompleteTraps(Array.Prototype, CreateObject(new { has = RuntimeFunction.Create((t, pk) => pk.In(t)) })));
        lastIndexOf.Call(arr, 100, CreateObject(valueOf: () => Return(arr["length"] = 0, 2)));
      });

      It("should not match non-existent property if search value is undefined", () => {
        EcmaValue arr = EcmaArray.Of();
        arr[0] = 0;
        arr[2] = 2;
        Case((arr, Undefined), -1);
      });

      It("should see inherited property", () => {
        using (TempProperty(Object.Prototype, 0, true)) {
          Case((CreateObject(new { length = 3 }), true), 0);
        }
      });

      It("should see updated elements during iteration", () => {
        EcmaValue arr = EcmaArray.Of(false, false);
        DefineProperty(arr, 1, get: () => Return(arr[0] = true, false));
        Case((arr, true), 0);
      });

      It("should not see deleted elements during iteration", () => {
        EcmaValue arr = EcmaArray.Of(0, 1, 2, "last", 4);
        DefineProperty(arr, 4, get: () => Return(arr["length"] = 3, 0));
        Case((arr, "last"), -1);

        arr = EcmaArray.Of(0, 1, 2);
        DefineProperty(arr, 2, get: () => Return(arr.ToObject().Delete(1), 0));
        Case((arr, 1), -1);
      });

      It("should not have iteration count affected by decreasing length of array during iteration", () => {
        EcmaValue arr = EcmaArray.Of(0, 1, 2, 3, 4);
        DefineProperty(arr, 3, get: () => Return(arr["length"] = 2, 1));
        using (TempProperty(Object.Prototype, 2, "prototype")) {
          Case((arr, "prototype"), 2);
        }

        arr = EcmaArray.Of(0, 1, 2, 3);
        DefineProperty(arr, 1, get: () => Return(arr[4] = 4, 1));
        Case((arr, 4), -1);
      });

      It("should return abrupt immediately without access subsequent element", () => {
        EcmaValue obj = CreateObject(("length", 2));
        DefineProperty(obj, 1, get: ThrowTest262Exception);
        DefineProperty(obj, 0, get: Intercept(() => _));
        Logs.Clear();
        Case((obj, true), Throws.Test262);
        That(Logs, Has.Exactly(0).Items);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Pop(RuntimeFunction pop) {
      IsUnconstructableFunctionWLength(pop, "pop", 0);
      IsAbruptedFromToObject(pop);

      It("should remove and return the last element from the array", () => {
        EcmaValue x = EcmaArray.Of();
        Case(x, Undefined);
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        EcmaValue y = EcmaArray.Of(0, 1, 2, 3);
        Case(y, 3);
        That(y, Is.EquivalentTo(new[] { 0, 1, 2 }));
      });

      It("should coerce length of array-like object to integer", () => {
        EcmaValue obj = new EcmaObject();
        Case(obj, Undefined);

        obj["length"] = Null;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = NaN;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = -Infinity;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = -0d;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = 0.5;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = Number.Construct(0);
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = 2.5;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(1));

        obj["length"] = Number.Construct(2);
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(1));

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: () => 0);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: () => new EcmaObject());
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: ThrowTest262Exception);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(toString: () => "1");
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(toString: () => "1", valueOf: () => new EcmaObject());
        Case(obj, -1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: ThrowTest262Exception);
        Case(obj, Throws.Test262);

        obj["length"] = CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject());
        Case(obj, Throws.TypeError);
      });

      It("should clamp length to 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        pop.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 2));

        arrayLike["length"] = (1L << 53);
        pop.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 2));

        arrayLike["length"] = (1L << 53) + 2;
        pop.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 2));

        arrayLike["length"] = Infinity;
        pop.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 2));
      });

      It("should see inherited properties", () => {
        RuntimeFunction ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn.Prototype[1] = -1;
        ConstructorFn.Prototype["length"] = 2;

        EcmaValue x = ConstructorFn.Construct();
        x[0] = 0;
        x[1] = 1;
        Case(x, 1);
        That(x, Is.EquivalentTo(new[] { 0 }));
        That(ConstructorFn.Prototype[1], Is.EqualTo(-1));
        That(ConstructorFn.Prototype["length"], Is.EqualTo(2));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Push(RuntimeFunction push) {
      IsUnconstructableFunctionWLength(push, "push", 1);
      IsAbruptedFromToObject(push);

      It("should append arguments to the end of the array", () => {
        EcmaValue x = EcmaArray.Of();
        Case((x, 1), 1);
        That(x, Is.EquivalentTo(new[] { 1 }));
        Case(x, 1);
        That(x, Is.EquivalentTo(new[] { 1 }));

        EcmaValue y = EcmaArray.Of(0);
        Case((y, true, Infinity, "NaN", "1", -1), 6);
        That(y, Is.EquivalentTo(new[] { 0, true, Infinity, "NaN", "1", -1 }));
      });

      It("should coerce length of array-like object to integer", () => {
        EcmaValue obj = new EcmaObject();
        Assume.That(obj["length"], Is.Undefined);
        Case((obj, -1), 1);
        That(obj, Is.EquivalentTo(new[] { -1 }));

        obj["length"] = Undefined;
        Case((obj, -2), 1);
        That(obj, Is.EquivalentTo(new[] { -2 }));

        obj["length"] = Null;
        Case((obj, -3), 1);
        That(obj, Is.EquivalentTo(new[] { -3 }));

        obj["length"] = NaN;
        Case((obj, -4), 1);
        That(obj, Is.EquivalentTo(new[] { -4 }));

        obj["length"] = -Infinity;
        Case((obj, -5), 1);
        That(obj, Is.EquivalentTo(new[] { -5 }));

        obj["length"] = 0.5;
        Case((obj, -6), 1);
        That(obj, Is.EquivalentTo(new[] { -6 }));

        obj["length"] = 1.5;
        Case((obj, -7), 2);
        That(obj, Is.EquivalentTo(new[] { -6, -7 }));

        obj["length"] = Number.Construct(0);
        Case((obj, -8), 1);
        That(obj, Is.EquivalentTo(new[] { -8 }));

        obj["length"] = CreateObject(valueOf: () => 3);
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: () => 1);
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: () => new EcmaObject());
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: ThrowTest262Exception);
        Case(obj, 3);

        obj["length"] = CreateObject(toString: () => "1");
        Case(obj, 1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: () => new EcmaObject());
        Case(obj, 1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: ThrowTest262Exception);
        Case(obj, Throws.Test262);

        obj["length"] = CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject());
        Case(obj, Throws.TypeError);
      });

      It("should clamp length to 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        push.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53);
        push.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53) + 2;
        push.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = Infinity;
        push.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));
      });

      It("should throw a TypeError if the new length exceeds 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53);
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53) + 2;
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = Infinity;
        Case((arrayLike, Null), Throws.TypeError);
      });

      It("should see inherited properties", () => {
        RuntimeFunction ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn.Prototype[1] = -1;
        ConstructorFn.Prototype["length"] = 1;

        EcmaValue x = ConstructorFn.Construct();
        x[0] = 0;
        Case((x, 1), 2);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));
        That(ConstructorFn.Prototype[1], Is.EqualTo(-1));
        That(ConstructorFn.Prototype["length"], Is.EqualTo(1));
      });
    }

    [Test, RuntimeFunctionInjection, MaxTime(2000)]
    public void Reverse(RuntimeFunction reverse) {
      IsUnconstructableFunctionWLength(reverse, "reverse", 0);
      IsAbruptedFromToObject(reverse);

      Case(EcmaArray.Of(), EcmaValue.EmptyArray);
      Case(EcmaArray.Of(1), new[] { 1 });
      Case(EcmaArray.Of(1, 2), new[] { 2, 1 });
      Case(EcmaArray.Of(1, 2, 3), new[] { 3, 2, 1 });
      Case(EcmaArray.Of(1, 2, 3, 4), new[] { 4, 3, 2, 1 });
      Case(EcmaArray.Of(1, 2, 3, 4, 5), new[] { 5, 4, 3, 2, 1 });

      It("should return this value", () => {
        EcmaValue arr = new EcmaArray();
        Case(arr, arr);

        EcmaValue obj = CreateObject(new { length = 0 });
        Case(obj, obj);
      });

      It("should reverse holes", () => {
        EcmaValue arr = new EcmaArray(2);
        arr[0] = "first";
        Case(arr, new[] { Undefined, "first" });
        That(!arr.HasOwnProperty(0));

        EcmaValue obj = new EcmaObject();
        obj["length"] = 10;
        obj[0] = true;
        obj[2] = Infinity;
        obj[4] = Undefined;
        obj[5] = Undefined;
        obj[8] = "NaN";
        obj[9] = "-1";
        Case(obj, new[] { "-1", "NaN", Undefined, Undefined, Undefined, Undefined, Undefined, Infinity, Undefined, true });
      });

      It("should coerce length of array-like object to integer", () => {
        EcmaValue obj = CreateObject(("length", 2.5), ("0", "first"), ("1", "second"), ("2", "third"));
        reverse.Call(obj);
        That(obj[0], Is.EqualTo("second"));
        That(obj[1], Is.EqualTo("first"));
        That(obj[2], Is.EqualTo("third"));
        That(obj["length"], Is.EqualTo(2.5));
      });

      It("should handle objects with length > 2^53-1", () => {
        EcmaValue obj = CreateObject(("length", 9007199254740994));
        DefineProperty(obj, 9007199254740990, get: ThrowTest262Exception);
        Case(obj, Throws.Test262);
      });

      It("should execute observable operation in the correct order", () => {
        Logs.Clear();
        EcmaValue arrayLike = CreateObject(("length", 9007199254740994));
        arrayLike[0] = "zero";
        arrayLike[2] = "two";
        DefineProperty(arrayLike, 4, get: ThrowTest262Exception);
        arrayLike[9007199254740987] = "9007199254740987";
        arrayLike[9007199254740990] = "9007199254740990";

        EcmaValue proxy = CreateProxyCompleteTraps(arrayLike, CreateObject(new {
          getOwnPropertyDescriptor = Intercept((a, b) => Reflect.Invoke("getOwnPropertyDescriptor", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "GetOwnPropertyDescriptor:{1}"),
          defineProperty = Intercept(() => Reflect.Invoke("defineProperty", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "DefineProperty:{1}"),
          has = Intercept(() => Reflect.Invoke("has", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "Has:{1}"),
          get = Intercept(() => Reflect.Invoke("get", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "Get:{1}"),
          set = Intercept(() => Reflect.Invoke("set", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "Set:{1}"),
          deleteProperty = Intercept(() => Reflect.Invoke("deleteProperty", EcmaValueUtility.CreateListFromArrayLike(Arguments)), "Delete:{1}")
        }));
        Case(proxy, Throws.Test262);

        CollectionAssert.AreEqual(new[] {
          // Initial get length operation.
          "Get:length",

          // Lower and upper index are both present.
          "Has:0",
          "Get:0",
          "Has:9007199254740990",
          "Get:9007199254740990",
          "Set:0",
          "GetOwnPropertyDescriptor:0",
          "DefineProperty:0",
          "Set:9007199254740990",
          "GetOwnPropertyDescriptor:9007199254740990",
          "DefineProperty:9007199254740990",

          // Lower and upper index are both absent.
          "Has:1",
          "Has:9007199254740989",

          // Lower index is present, upper index is absent.
          "Has:2",
          "Get:2",
          "Has:9007199254740988",
          "Delete:2",
          "Set:9007199254740988",
          "GetOwnPropertyDescriptor:9007199254740988",
          "DefineProperty:9007199254740988",

          // Lower index is absent, upper index is present.
          "Has:3",
          "Has:9007199254740987",
          "Get:9007199254740987",
          "Set:3",
          "GetOwnPropertyDescriptor:3",
          "DefineProperty:3",
          "Delete:9007199254740987",

          // Stop exception.
          "Has:4",
          "Get:4",
        }, Logs);

        That(arrayLike["length"], Is.EqualTo(9007199254740994), "Length property is not modified");

        That(arrayLike[0], Is.EqualTo("9007199254740990"), "Property at index 0");
        That(!arrayLike.HasProperty(1), "Property at index 1");
        That(!arrayLike.HasProperty(2), "Property at index 2");
        That(arrayLike[3], Is.EqualTo("9007199254740987"), "Property at index 3");

        That(!arrayLike.HasProperty(9007199254740987), "Property at index 2**53-5");
        That(arrayLike[9007199254740988], Is.EqualTo("two"), "Property at index 2**53-4");
        That(!arrayLike.HasProperty(9007199254740989), "Property at index 2**53-3");
        That(arrayLike[9007199254740990], Is.EqualTo("zero"), "Property at index 2**53-2");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Shift(RuntimeFunction shift) {
      IsUnconstructableFunctionWLength(shift, "shift", 0);
      IsAbruptedFromToObject(shift);

      It("should remove and return the first element from the array", () => {
        EcmaValue x = EcmaArray.Of();
        Case(x, Undefined);
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        EcmaValue y = EcmaArray.Of(0, 1, 2, 3);
        Case(y, 0);
        That(y, Is.EquivalentTo(new[] { 1, 2, 3 }));
      });

      It("should move holes along with elements", () => {
        EcmaValue z = EcmaArray.Of();
        z[0] = 0;
        z[3] = 3;
        Case(z, 0);
        That(z, Is.EquivalentTo(new[] { Undefined, Undefined, 3 }));
        That(!z.HasOwnProperty(0));
        That(!z.HasOwnProperty(1));

        EcmaValue obj = new EcmaObject();
        obj[0] = 0;
        obj[3] = 3;
        obj["length"] = 4;
        Case(obj, 0);
        That(obj, Is.EquivalentTo(new[] { Undefined, Undefined, 3 }));
        That(!obj.HasOwnProperty(0));
        That(!obj.HasOwnProperty(1));
      });

      It("should coerce length of array-like object to integer", () => {
        EcmaValue obj = new EcmaObject();
        Case(obj, Undefined);

        obj["length"] = Null;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = NaN;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = -Infinity;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = -0d;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = 0.5;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = Number.Construct(0);
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(0));

        obj["length"] = 2.5;
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(1));

        obj["length"] = Number.Construct(2);
        Case(obj, Undefined);
        That(obj["length"], Is.EqualTo(1));

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: () => 0);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: () => new EcmaObject());
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(valueOf: () => 1, toString: ThrowTest262Exception);
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(toString: () => "1");
        Case(obj, -1);

        obj[0] = -1;
        obj["length"] = CreateObject(toString: () => "1", valueOf: () => new EcmaObject());
        Case(obj, -1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: ThrowTest262Exception);
        Case(obj, Throws.Test262);

        obj["length"] = CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject());
        Case(obj, Throws.TypeError);
      });

      It("should see inherited properties", () => {
        RuntimeFunction ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn.Prototype[1] = 1;
        ConstructorFn.Prototype["length"] = 2;

        EcmaValue x = ConstructorFn.Construct();
        x[0] = 0;
        Case(x, 0);
        That(x, Is.EquivalentTo(new[] { 1 }));
        That(ConstructorFn.Prototype[0], Is.Undefined);
        That(ConstructorFn.Prototype["length"], Is.EqualTo(2));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Slice(RuntimeFunction slice) {
      IsUnconstructableFunctionWLength(slice, "slice", 2);
      IsAbruptedFromToObject(slice);

      It("should copy elements to a new array object", () => {
        EcmaArray arr = EcmaArray.Of(0, 1, 2, 3, 4);
        Case((arr, 0, 3), new[] { 0, 1, 2 });
        Case((arr, 3, 3), EcmaValue.EmptyArray);
        Case((arr, 4, 3), EcmaValue.EmptyArray);
        Case((arr, 5, 5), EcmaValue.EmptyArray);
        Case((arr, 3, 5), new[] { 3, 4 });
        Case((arr, 2, 4), new[] { 2, 3 });
        Case((arr, 3, 6), new[] { 3, 4 });
        Case((arr, -3, 3), new[] { 2 });
        Case((arr, -1, 5), new[] { 4 });
        Case((arr, -5, 1), new[] { 0 });
        Case((arr, -9, 5), new[] { 0, 1, 2, 3, 4 });
        Case((arr, 0, -2), new[] { 0, 1, 2 });
        Case((arr, 1, -4), EcmaValue.EmptyArray);
        Case((arr, 0, -5), EcmaValue.EmptyArray);
        Case((arr, 4, -9), EcmaValue.EmptyArray);
        Case((arr, -5, -2), new[] { 0, 1, 2 });
        Case((arr, -3, -1), new[] { 2, 3 });
        Case((arr, -9, -1), new[] { 0, 1, 2, 3 });
        Case((arr, -6, -6), EcmaValue.EmptyArray);
        Case((arr, 3, Undefined), new[] { 3, 4 });
        Case((arr, -2), new[] { 3, 4 });
      });

      It("is intentionally generic", () => {
        EcmaValue obj = CreateObject(("length", 5), (0, 0), (1, 1), (2, 2), (3, 3), (4, 4));
        Case((obj, 0, 3), new[] { 0, 1, 2 });
        Case((obj, -5, 3), new[] { 0, 1, 2 });
        Case((obj, 0, -2), new[] { 0, 1, 2 });
        Case((obj, -5, -2), new[] { 0, 1, 2 });
        Case((obj, 2, Undefined), new[] { 2, 3, 4 });
        Case((obj, 2), new[] { 2, 3, 4 });

        obj = CreateObject(("length", 4294967296), (0, "x"), (4294967295, "y"));
        Case((obj, 0, 4294967296), Throws.RangeError);
        obj = CreateObject(("length", 4294967297), (0, "x"), (4294967296, "y"));
        Case((obj, 0, 4294967297), Throws.RangeError);
        obj = CreateObject(("length", -1), (4294967294, "x"));
        Case((obj, 4294967294, 4294967295), EcmaValue.EmptyArray);
      });

      It("should coerce start and end to integer", () => {
        EcmaArray arr = EcmaArray.Of(0, 1, 2, 3, 4);
        Case((arr, 2.5, 4), new[] { 2, 3 });
        Case((arr, NaN, 3), new[] { 0, 1, 2 });
        Case((arr, Infinity, 3), EcmaValue.EmptyArray);
        Case((arr, -Infinity, 3), new[] { 0, 1, 2 });
        Case((arr, CreateObject(valueOf: () => 0, toString: () => 3), 3), new[] { 0, 1, 2 });

        Case((arr, 2, 4.5), new[] { 2, 3 });
        Case((arr, 0, NaN), EcmaValue.EmptyArray);
        Case((arr, 0, Infinity), new[] { 0, 1, 2, 3, 4 });
        Case((arr, 0, -Infinity), EcmaValue.EmptyArray);
        Case((arr, 0, CreateObject(valueOf: () => 3, toString: () => 0)), new[] { 0, 1, 2 });
      });

      It("should convert value -0 to 0", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue args = default;
        arr["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => Void(args = Arguments))));
        slice.Call(arr, 0, -0d);
        That(args, Is.EquivalentTo(new[] { 0 }));
      });

      It("should overwrite non-writable properties by CreateDataPropertyOrThrow", () => {
        EcmaValue a = EcmaArray.Of(1);
        a["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => {
          EcmaValue q = Array.Construct(0);
          DefineProperty(q, 0, value: 0, writable: false, enumerable: false, configurable: true);
          return q;
        })));
        EcmaValue r = slice.Call(a, 0);
        That(r, Has.OwnProperty(0, 1, EcmaPropertyAttributes.DefaultDataProperty));
      });

      It("should throw a TypeError if constructor is not an Object", () => {
        EcmaArray arr = EcmaArray.Of();
        arr["constructor"] = Null;
        Case(arr, Throws.TypeError);
        arr["constructor"] = 1;
        Case(arr, Throws.TypeError);
        arr["constructor"] = "string";
        Case(arr, Throws.TypeError);
        arr["constructor"] = true;
        Case(arr, Throws.TypeError);
      });

      It("should return abrupt from accessing constructor", () => {
        EcmaArray arr = EcmaArray.Of();
        DefineProperty(arr, "constructor", get: ThrowTest262Exception);
        Case(arr, Throws.Test262);
      });

      It("should return abrupt from creating new Array exotic object whose length exceed 2^32-1", () => {
        EcmaValue obj = CreateObject(("length", get: () => 1L << 32, set: Intercept(_ => Undefined)));
        Logs.Clear();
        Case((obj, 0), Throws.RangeError);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should ignore constructor for non-Array values", () => {
        EcmaValue obj = CreateObject(("length", get: () => 0, set: _ => Undefined), ("constructor", get: Intercept(() => Undefined), set: null));
        Logs.Clear();
        EcmaValue result = slice.Call(obj, 0);
        That(result.InstanceOf(Array));
        That(EcmaArray.IsArray(result));
        That(Logs, Has.Exactly(0).Items);
      });

      It("should prefer Array constructor of current realm record", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue OArray = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ArrayConstructor);
        arr["constructor"] = OArray;
        using (TempProperty(Array, Symbol.Species, Intercept(() => Undefined)))
        using (TempProperty(OArray, Symbol.Species, Intercept(() => Undefined))) {
          Logs.Clear();
          That(slice.Call(arr, 0).InstanceOf(Array));
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should accept non-Array constructors from other realms", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue CustomCtor = RuntimeFunction.Create(() => Undefined);
        EcmaValue OObject = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ObjectConstructor);
        arr["constructor"] = OObject;
        OObject[Symbol.Species] = CustomCtor;
        using (TempProperty(Array, Symbol.Species, Intercept(() => Undefined))) {
          Logs.Clear();
          That(Object.Invoke("getPrototypeOf", slice.Call(arr, 0)), Is.EqualTo(CustomCtor["prototype"]));
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should use species constructor of a Proxy object whose target is an array", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue proxy = Proxy.Construct(Proxy.Construct(arr, new EcmaObject()), new EcmaObject());
        EcmaValue Ctor = RuntimeFunction.Create(() => Undefined);
        arr["constructor"] = RuntimeFunction.Create(() => Undefined);
        arr["constructor"].ToObject()[Symbol.Species] = Ctor;
        That(Object.Invoke("getPrototypeOf", slice.Call(proxy)), Is.EqualTo(Ctor["prototype"]));
      });

      It("should return abrupt from revoked Proxy object", () => {
        EcmaValue o = Proxy.Invoke("revocable", EcmaArray.Of(), new EcmaObject());
        o.Invoke("revoke");
        Case(o["proxy"], Throws.TypeError);
      });

      It("should return abrupt from accessing species constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, get: ThrowTest262Exception, set: null));
        Case(arr, Throws.Test262);
      });

      It("should return abrupt from species constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, ThrowTest262Exception));
        Case(arr, Throws.Test262);
      });

      It("should create an Array exotic object if species constructor is undefined or null", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, Undefined));
        That(EcmaArray.IsArray(slice.Call(arr)));

        arr["constructor"] = CreateObject((Symbol.Species, Null));
        That(EcmaArray.IsArray(slice.Call(arr)));
      });

      It("should return abrupt if species constructor is not a constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, GlobalThis["parseInt"]));
        Case(arr, Throws.TypeError);
      });

      It("should handle length and deleteCount when they exceed the integer limit", () => {
        EcmaValue arrayLike = CreateObject(
          ("length", (1L << 53) + 2),
          ("9007199254740988", "9007199254740988"),
          ("9007199254740989", "9007199254740989"),
          ("9007199254740990", "9007199254740990"),
          ("9007199254740991", "9007199254740991"));
        Case((arrayLike, 9007199254740989), new[] { "9007199254740989", "9007199254740990" });
        Case((arrayLike, 9007199254740989, 9007199254740990), new[] { "9007199254740989" });
        Case((arrayLike, 9007199254740989, 9007199254740996), new[] { "9007199254740989", "9007199254740990" });
        Case((arrayLike, -2), new[] { "9007199254740989", "9007199254740990" });
        Case((arrayLike, -2, -1), new[] { "9007199254740989" });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Splice(RuntimeFunction splice) {
      IsUnconstructableFunctionWLength(splice, "splice", 2);
      IsAbruptedFromToObject(splice);

      It("should remove and insert elements at the start position", () => {
        // length > deleteCount > start = 0, itemCount = 0
        EcmaValue x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, 3), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));

        // length > deleteCount > start = 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, 3, 4, 5), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 4, 5, 3 }));

        // length = deleteCount > start = 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, 4), new[] { 0, 1, 2, 3 });
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        // length > deleteCount > start > 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 1, 3, 4, 5), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0, 4, 5 }));

        // deleteCount > length > start = 0, itemCount = 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, 5), new[] { 0, 1, 2, 3 });
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        // length = deleteCount > start > 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 1, 4, 4, 5), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0, 4, 5 }));

        // -length = start < deleteCount < 0, itemCount = 0
        x = EcmaArray.Of(0, 1);
        Case((x, -2, -1), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));

        // -length < start = deleteCount < 0, itemCount = 0
        x = EcmaArray.Of(0, 1);
        Case((x, -1, -1), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));

        // -length = start < deleteCount < 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, -2, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 2, 3, 0, 1 }));

        // -length < start = deleteCount < 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, -1, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 2, 3, 1 }));

        // start < -length < deleteCount < 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, -3, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 2, 3, 0, 1 }));

        // -length < deleteCount < start = 0, itemCount = 0
        x = EcmaArray.Of(0, 1);
        Case((x, 0, -1), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));

        // -length = -start < deleteCount < 0, itemCount = 0
        x = EcmaArray.Of(0, 1);
        Case((x, 2, -1), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));

        // -length < deleteCount < start = 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, 0, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 2, 3, 0, 1 }));

        // -length = -start < deleteCount < 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, 2, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        // -start < -length < deleteCount < 0, itemCount > 0
        x = EcmaArray.Of(0, 1);
        Case((x, 3, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        // length = -start > deleteCount > 0, itemCount = 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -4, 3), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));

        // length = -start > deleteCount > 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -4, 3, 4, 5), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 4, 5, 3 }));

        // -start > length = deleteCount > 0, itemCount = 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -5, 4), new[] { 0, 1, 2, 3 });
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        // length > -start = deleteCount > 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -3, 3, 4, 5), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0, 4, 5 }));

        // -start > deleteCount > length > 0, itemCount = 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -9, 5), new[] { 0, 1, 2, 3 });
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        // length = deleteCount > -start > 0, itemCount > 0
        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -3, 4, 4, 5), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0, 4, 5 }));
      });

      It("is intentionally generic", () => {
        EcmaValue x = CreateObject(("length", 4), (0, 0), (1, 1), (2, 2), (3, 3));
        Case((x, 0, 3, 4, 5), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 4, 5, 3 }));

        x = CreateObject(("length", 2), (0, 0), (1, 1));
        Case((x, -2, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 2, 3, 0, 1 }));

        x = CreateObject(("length", 2), (0, 0), (1, 1));
        Case((x, 0, -1, 2, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 2, 3, 0, 1 }));

        x = CreateObject(("length", 4), (0, 0), (1, 1), (2, 2), (3, 3));
        Case((x, -4, 3, 4, 5), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 4, 5, 3 }));

        x = CreateObject(("length", 4294967296), (0, "x"), (4294967295, "y"));
        Case((x, 4294967295, 1), new[] { "y" });
        That(x[0], Is.EqualTo("x"));
        That(x[4294967295], Is.Undefined);
        That(x["length"], Is.EqualTo(4294967295));

        x = CreateObject(("length", -1), (4294967294, "x"));
        Case((x, 4294967294, 1), EcmaValue.EmptyArray);
        That(x[4294967294], Is.EqualTo("x"));
        That(x["length"], Is.EqualTo(0));
      });

      It("should coerce start and deleteCount to integer", () => {
        EcmaValue x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, Undefined, Undefined), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 1, Undefined), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 1.5, 3), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, NaN, 3), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, Infinity, 3), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, -Infinity, 3), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, CreateObject(valueOf: () => 0, toString: () => 3), 3), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 1, 3.5), new[] { 1, 2, 3 });
        That(x, Is.EquivalentTo(new[] { 0 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, NaN), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, Infinity), new[] { 0, 1, 2, 3 });
        That(x, Is.EquivalentTo(EcmaValue.EmptyArray));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, -Infinity), EcmaValue.EmptyArray);
        That(x, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));

        x = EcmaArray.Of(0, 1, 2, 3);
        Case((x, 0, CreateObject(valueOf: () => 3, toString: () => 0)), new[] { 0, 1, 2 });
        That(x, Is.EquivalentTo(new[] { 3 }));
      });

      It("should convert deleteCount of value -0 to 0", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue args = default;
        arr["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => Void(args = Arguments))));
        splice.Call(arr, 0, -0d);
        That(args, Is.EquivalentTo(new[] { 0 }));
      });

      It("should throw a TypeError if length is not writable", () => {
        EcmaValue x = EcmaArray.Of(0, 1, 2);
        DefineProperty(x, "length", writable: false);
        Case((x, 1, 2, 4), Throws.TypeError);

        x = CreateObject(("length", get: () => 0, set: null));
        Case((x, 1, 2, 4), Throws.TypeError);
      });

      It("should throw a TypeError if the new length exceeds 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        Case((arrayLike, 0, 0, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53);
        Case((arrayLike, 0, 0, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53) + 2;
        Case((arrayLike, 0, 0, Null), Throws.TypeError);

        arrayLike["length"] = Infinity;
        Case((arrayLike, 0, 0, Null), Throws.TypeError);
      });

      It("should clamp length to 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        splice.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53);
        splice.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53) + 2;
        splice.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = Infinity;
        splice.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));
      });

      It("should overwrite non-writable properties by CreateDataPropertyOrThrow", () => {
        EcmaValue a = EcmaArray.Of(1);
        a["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => {
          EcmaValue q = Array.Construct(0);
          DefineProperty(q, 0, value: 0, writable: false, enumerable: false, configurable: true);
          return q;
        })));
        EcmaValue r = splice.Call(a, 0);
        That(r, Has.OwnProperty(0, 1, EcmaPropertyAttributes.DefaultDataProperty));
      });

      It("should get and set length property even if no elements are inserted or removed", () => {
        Logs.Clear();
        splice.Call(CreateObject(("length", get: Intercept(() => 0, "get"), set: Intercept(_ => Undefined, "set"))));
        CollectionAssert.AreEqual(new[] { "get", "set" }, Logs);
      });

      It("should call correct property traps on the new array", () => {
        EcmaValue a = EcmaArray.Of(0, 1);
        a["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(len => {
          return Proxy.Construct(Array.Construct(len), Proxy.Construct(new EcmaObject(), CreateObject(new {
            get = Intercept(() => Undefined, "{1}")
          })));
        })));
        Logs.Clear();
        splice.Call(a, 0);
        CollectionAssert.AreEqual(new[] { "defineProperty", "defineProperty", "set", "getOwnPropertyDescriptor", "defineProperty" }, Logs);
      });

      It("should throw a TypeError if constructor is not an Object", () => {
        EcmaArray arr = EcmaArray.Of();
        arr["constructor"] = Null;
        Case(arr, Throws.TypeError);
        arr["constructor"] = 1;
        Case(arr, Throws.TypeError);
        arr["constructor"] = "string";
        Case(arr, Throws.TypeError);
        arr["constructor"] = true;
        Case(arr, Throws.TypeError);
      });

      It("should return abrupt from accessing constructor", () => {
        EcmaArray arr = EcmaArray.Of();
        DefineProperty(arr, "constructor", get: ThrowTest262Exception);
        Case(arr, Throws.Test262);
      });

      It("should return abrupt from creating new Array exotic object whose length exceed 2^32-1", () => {
        EcmaValue obj = CreateObject(("length", get: () => 1L << 32, set: Intercept(_ => Undefined)));
        Logs.Clear();
        Case((obj, 0), Throws.RangeError);
        That(Logs, Has.Exactly(0).Items);
      });

      It("should ignore constructor for non-Array values", () => {
        EcmaValue obj = CreateObject(("length", get: () => 0, set: _ => Undefined), ("constructor", get: Intercept(() => Undefined), set: null));
        Logs.Clear();
        EcmaValue result = splice.Call(obj, 0);
        That(result.InstanceOf(Array));
        That(EcmaArray.IsArray(result));
        That(Logs, Has.Exactly(0).Items);
      });

      It("should prefer Array constructor of current realm record", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue OArray = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ArrayConstructor);
        arr["constructor"] = OArray;
        using (TempProperty(Array, Symbol.Species, Intercept(() => Undefined)))
        using (TempProperty(OArray, Symbol.Species, Intercept(() => Undefined))) {
          Logs.Clear();
          That(splice.Call(arr, 0).InstanceOf(Array));
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should accept non-Array constructors from other realms", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue CustomCtor = RuntimeFunction.Create(() => Undefined);
        EcmaValue OObject = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ObjectConstructor);
        arr["constructor"] = OObject;
        OObject[Symbol.Species] = CustomCtor;
        using (TempProperty(Array, Symbol.Species, Intercept(() => Undefined))) {
          Logs.Clear();
          That(Object.Invoke("getPrototypeOf", splice.Call(arr, 0)), Is.EqualTo(CustomCtor["prototype"]));
          That(Logs, Has.Exactly(0).Items);
        }
      });

      It("should use species constructor of a Proxy object whose target is an array", () => {
        EcmaValue arr = EcmaArray.Of();
        EcmaValue proxy = Proxy.Construct(Proxy.Construct(arr, new EcmaObject()), new EcmaObject());
        EcmaValue Ctor = RuntimeFunction.Create(() => Undefined);
        arr["constructor"] = RuntimeFunction.Create(() => Undefined);
        arr["constructor"].ToObject()[Symbol.Species] = Ctor;
        That(Object.Invoke("getPrototypeOf", splice.Call(proxy)), Is.EqualTo(Ctor["prototype"]));
      });

      It("should return abrupt from revoked Proxy object", () => {
        EcmaValue o = Proxy.Invoke("revocable", EcmaArray.Of(), new EcmaObject());
        o.Invoke("revoke");
        Case(o["proxy"], Throws.TypeError);
      });

      It("should return abrupt from accessing species constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, get: ThrowTest262Exception, set: null));
        Case(arr, Throws.Test262);
      });

      It("should return abrupt from species constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, ThrowTest262Exception));
        Case(arr, Throws.Test262);
      });

      It("should create an Array exotic object if species constructor is undefined or null", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, Undefined));
        That(EcmaArray.IsArray(splice.Call(arr)));

        arr["constructor"] = CreateObject((Symbol.Species, Null));
        That(EcmaArray.IsArray(splice.Call(arr)));
      });

      It("should return abrupt if species constructor is not a constructor", () => {
        EcmaValue arr = EcmaArray.Of();
        arr["constructor"] = CreateObject((Symbol.Species, GlobalThis["parseInt"]));
        Case(arr, Throws.TypeError);
      });

      It("should handle length and deleteCount when they exceed the integer limit", () => {
        EcmaValue arrayLike = CreateObject(
          ("length", (1L << 53) + 2),
          ("9007199254740988", "9007199254740988"),
          ("9007199254740989", "9007199254740989"),
          ("9007199254740990", "9007199254740990"),
          ("9007199254740991", "9007199254740991"));
        Case((arrayLike, 9007199254740989, (1L << 53) + 4), new[] { "9007199254740989", "9007199254740990" });
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 3));
        That(!arrayLike.HasOwnProperty("9007199254740989"));
        That(!arrayLike.HasOwnProperty("9007199254740990"));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleString(RuntimeFunction toLocaleString) {
      IsUnconstructableFunctionWLength(toLocaleString, "toLocaleString", 0);

      Logs.Clear();
      EcmaValue obj = CreateObject(("toLocaleString", Intercept(() => Undefined)));
      toLocaleString.Call(EcmaArray.Of(Undefined, obj, Null, obj, obj));
      That(Logs, Has.Exactly(3).Items);
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);

      It("should be identical as calling join with no arguments", () => {
        Case(EcmaArray.Of(), "");
        Case(EcmaArray.Of(0, 1, 2, 3), "0,1,2,3");
        Case(EcmaArray.Of(Undefined, 1, Null, 3), ",1,,3");
        Case(EcmaArray.Of("", "", ""), ",,");
        Case(EcmaArray.Of("\\", "\\", "\\"), "\\,\\,\\");
        Case(EcmaArray.Of("&", "&", "&"), "&,&,&");
        Case(EcmaArray.Of(true, true, true), "true,true,true");
        Case(EcmaArray.Of(Infinity, Infinity, Infinity), "Infinity,Infinity,Infinity");
        Case(EcmaArray.Of(NaN, NaN, NaN), "NaN,NaN,NaN");

        EcmaValue arr = new EcmaArray();
        arr[0] = 0;
        arr[3] = 3;
        Case(arr, "0,,,3");
      });

      It("should coerce array's items by ToPrimitive(string)", () => {
        Case(EcmaArray.Of(CreateObject(valueOf: () => "+")), "[object Object]");
        Case(EcmaArray.Of(CreateObject(toString: () => "+")), "+");
        Case(EcmaArray.Of(CreateObject(toString: () => "+", valueOf: () => "*")), "+");
        Case(EcmaArray.Of(CreateObject(toString: () => "+", valueOf: () => new EcmaObject())), "+");
        Case(EcmaArray.Of(CreateObject(toString: () => new EcmaObject(), valueOf: () => "+")), "+");
        Case(EcmaArray.Of(CreateObject(toString: () => "+", valueOf: () => ThrowTest262Exception())), "+");
        Case(EcmaArray.Of(CreateObject(toString: () => ThrowTest262Exception(), valueOf: () => "+")), Throws.Test262);
        Case(EcmaArray.Of(CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject())), Throws.TypeError);
      });

      It("should get inherited property", () => {
        using (TempProperty(Array.Prototype, 1, 1)) {
          EcmaValue arr = EcmaArray.Of(0);
          arr["length"] = 2;
          Case(arr, "0,1");
        }
        Array.Prototype["length"] = 0;
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Unshift(RuntimeFunction unshift) {
      IsUnconstructableFunctionWLength(unshift, "unshift", 1);
      IsAbruptedFromToObject(unshift);

      It("should append arguments to the start of the array", () => {
        EcmaValue x = Array.Construct();
        Case((x, 1), 1);
        That(x, Is.EquivalentTo(new[] { 1 }));
        Case(x, 1);
        That(x, Is.EquivalentTo(new[] { 1 }));
        Case((x, -1), 2);
        That(x, Is.EquivalentTo(new[] { -1, 1 }));

        EcmaValue y = Array.Construct();
        y[0] = 0;
        Case((y, true, Infinity, "NaN", "1", -1), 6);
        That(y, Is.EquivalentTo(new[] { true, Infinity, "NaN", "1", -1, 0 }));
      });

      It("should move holes along with elements", () => {
        EcmaArray arr = new EcmaArray();
        arr[1] = 1;
        arr[3] = 3;
        Case((arr, 0), 5);
        That(arr, Is.EquivalentTo(new[] { 0, Undefined, 1, Undefined, 3 }));
        That(!arr.HasProperty(1));
        That(!arr.HasProperty(3));
      });

      It("should coerce length of array-like object to integer", () => {
        EcmaValue obj = new EcmaObject();
        Assume.That(obj["length"], Is.Undefined);
        Case((obj, -1), 1);
        That(obj, Is.EquivalentTo(new[] { -1 }));

        obj["length"] = Undefined;
        Case((obj, -2), 1);
        That(obj, Is.EquivalentTo(new[] { -2 }));

        obj["length"] = Null;
        Case((obj, -3), 1);
        That(obj, Is.EquivalentTo(new[] { -3 }));

        obj["length"] = NaN;
        Case((obj, -4), 1);
        That(obj, Is.EquivalentTo(new[] { -4 }));

        obj["length"] = -Infinity;
        Case((obj, -5), 1);
        That(obj, Is.EquivalentTo(new[] { -5 }));

        obj["length"] = 0.5;
        Case((obj, -6), 1);
        That(obj, Is.EquivalentTo(new[] { -6 }));

        obj["length"] = 1.5;
        Case((obj, -7), 2);
        That(obj, Is.EquivalentTo(new[] { -7, -6 }));

        obj["length"] = Number.Construct(0);
        Case((obj, -8), 1);
        That(obj, Is.EquivalentTo(new[] { -8 }));

        obj["length"] = CreateObject(valueOf: () => 3);
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: () => 1);
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: () => new EcmaObject());
        Case(obj, 3);

        obj["length"] = CreateObject(valueOf: () => 3, toString: ThrowTest262Exception);
        Case(obj, 3);

        obj["length"] = CreateObject(toString: () => "1");
        Case(obj, 1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: () => new EcmaObject());
        Case(obj, 1);

        obj["length"] = CreateObject(toString: () => "1", valueOf: ThrowTest262Exception);
        Case(obj, Throws.Test262);

        obj["length"] = CreateObject(toString: () => new EcmaObject(), valueOf: () => new EcmaObject());
        Case(obj, Throws.TypeError);
      });

      It("should clamp length to 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        unshift.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53);
        unshift.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = (1L << 53) + 2;
        unshift.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));

        arrayLike["length"] = Infinity;
        unshift.Call(arrayLike);
        That(arrayLike["length"], Is.EqualTo((1L << 53) - 1));
      });

      It("should throw a TypeError if the new length exceeds 2^53-1", () => {
        EcmaValue arrayLike = new EcmaObject();

        arrayLike["length"] = (1L << 53) - 1;
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53);
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = (1L << 53) + 2;
        Case((arrayLike, Null), Throws.TypeError);

        arrayLike["length"] = Infinity;
        Case((arrayLike, Null), Throws.TypeError);
      });

      It("should see inherited properties", () => {
        RuntimeFunction ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn.Prototype[0] = 1;
        ConstructorFn.Prototype["length"] = 1;

        EcmaValue x = ConstructorFn.Construct();
        Case((x, 0), 2);
        That(x, Is.EquivalentTo(new[] { 0, 1 }));
        That(ConstructorFn.Prototype[0], Is.EqualTo(1));
        That(ConstructorFn.Prototype["length"], Is.EqualTo(1));
      });
    }

    [Test]
    public void Unscopables() {
      That(Array.Prototype, Has.OwnProperty(Symbol.Unscopables, EcmaPropertyAttributes.Configurable));

      EcmaValue unscopables = Array.Prototype[Symbol.Unscopables];
      That(unscopables, Is.TypeOf("object"));
      That(Object.Invoke("getPrototypeOf", unscopables), Is.EqualTo(Null));
      That(unscopables, Has.OwnProperty("copyWithin", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("entries", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("fill", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("find", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("findIndex", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("flat", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("flatMap", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("includes", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("keys", EcmaPropertyAttributes.DefaultDataProperty));
      That(unscopables, Has.OwnProperty("values", EcmaPropertyAttributes.DefaultDataProperty));
    }

    [Test, RuntimeFunctionInjection]
    public void Values(RuntimeFunction values) {
      IsUnconstructableFunctionWLength(values, "values", 0);
      IsAbruptedFromToObject(values);

      It("should return a valid iterator with %ArrayIteratorPrototype% as the prototype", () => {
        That(Object.Invoke("getPrototypeOf", values.Call(EcmaArray.Of())), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
        That(Object.Invoke("getPrototypeOf", values.Call(CreateObject(new { length = 2 }))), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
      });

      It("should return a valid iterator with the array's numeric properties", () => {
        EcmaValue iterator = values.Call(EcmaArray.Of("a", "b", "c"));
        VerifyIteratorResult(iterator.Invoke("next"), false, "a");
        VerifyIteratorResult(iterator.Invoke("next"), false, "b");
        VerifyIteratorResult(iterator.Invoke("next"), false, "c");
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = values.Call(arr);
        arr.Push("a");
        VerifyIteratorResult(iterator.Invoke("next"), false, "a");
        VerifyIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        VerifyIteratorResult(iterator.Invoke("next"), true);
      });
    }
  }
}
