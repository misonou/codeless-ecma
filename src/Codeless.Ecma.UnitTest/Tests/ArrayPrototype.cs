using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
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
        CheckIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 0, "a" });
        CheckIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 1, "b" });
        CheckIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 2, "c" });
        CheckIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = entries.Call(arr);
        arr.Push("a");
        CheckIteratorResult(iterator.Invoke("next"), false, new EcmaValue[] { 0, "a" });
        CheckIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        CheckIteratorResult(iterator.Invoke("next"), true);
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

        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "length", get: ThrowTest262Exception);
        Case(obj, Throws.Test262, "length is accessed before checking callback argument");
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

      It("should invoke callback with correct arguments and sequence", () => {
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
    public void Keys(RuntimeFunction keys) {
      IsUnconstructableFunctionWLength(keys, "keys", 0);
      IsAbruptedFromToObject(keys);

      It("should return a valid iterator with %ArrayIteratorPrototype% as the prototype", () => {
        That(Object.Invoke("getPrototypeOf", keys.Call(EcmaArray.Of())), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
        That(Object.Invoke("getPrototypeOf", keys.Call(CreateObject(new { length = 2 }))), Is.EqualTo((RuntimeObject)WellKnownObject.ArrayIteratorPrototype));
      });

      It("should return a valid iterator with the array's numeric properties", () => {
        EcmaValue iterator = keys.Call(EcmaArray.Of("a", "b", "c"));
        CheckIteratorResult(iterator.Invoke("next"), false, 0);
        CheckIteratorResult(iterator.Invoke("next"), false, 1);
        CheckIteratorResult(iterator.Invoke("next"), false, 2);
        CheckIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = keys.Call(arr);
        arr.Push("a");
        CheckIteratorResult(iterator.Invoke("next"), false, 0);
        CheckIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        CheckIteratorResult(iterator.Invoke("next"), true);
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
        CheckIteratorResult(iterator.Invoke("next"), false, "a");
        CheckIteratorResult(iterator.Invoke("next"), false, "b");
        CheckIteratorResult(iterator.Invoke("next"), false, "c");
        CheckIteratorResult(iterator.Invoke("next"), true);
      });

      It("should see new items via iteration until iterator is done", () => {
        EcmaArray arr = new EcmaArray();
        EcmaValue iterator = values.Call(arr);
        arr.Push("a");
        CheckIteratorResult(iterator.Invoke("next"), false, "a");
        CheckIteratorResult(iterator.Invoke("next"), true);
        arr.Push("b");
        CheckIteratorResult(iterator.Invoke("next"), true);
      });
    }

    private static void CheckIteratorResult(EcmaValue result, bool done, object value = default) {
      That(result, Is.TypeOf("object"), "iterator return must be an Object");
      That(result["done"], Is.EqualTo(done), "expected iteration done: {0}", done);
      if (!done) {
        That(result["value"], value is System.Array arr ? Is.EquivalentTo(arr) : Is.EqualTo(value), "expected iteration result");
      }
    }
  }
}
