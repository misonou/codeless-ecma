using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Harness;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class TypedArrayPrototype : TestBase {
    RuntimeFunction ArrayBuffer => Global.ArrayBuffer;
    RuntimeFunction TypedArray => (RuntimeFunction)WellKnownObject.TypedArray;

    [Test]
    public void Properties() {
      That(TypedArray, Has.OwnProperty("prototype", TypedArray.Prototype, EcmaPropertyAttributes.None));
      That(TypedArray.Prototype, Has.OwnProperty("constructor", TypedArray, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));

      That(TypedArray.Prototype, Has.OwnProperty("toString", Array.Prototype["toString"], EcmaPropertyAttributes.DefaultMethodProperty));
      That(TypedArray.Prototype, Has.OwnProperty(Symbol.Iterator, TypedArray.Prototype["values"], EcmaPropertyAttributes.DefaultMethodProperty));

      EcmaValue[] keys = { "buffer", "byteLength", "byteOffset", "length", "entries", "keys", "values", "copyWithin", "every", "fill", "filter", "find", "findIndex", "forEach", "includes", "indexOf", "join", "lastIndexOf", "map", "reverse", "reduce", "reduceRight", "set", "slice", "some", "sort", "subarray", "toLocaleString", "toString", Symbol.ToStringTag, Symbol.Iterator };
      TestWithTypedArrayConstructors(TA => {
        foreach (EcmaValue key in keys) {
          if (System.Array.IndexOf(new EcmaValue[] { "buffer", "byteLength", "byteOffset", "length", Symbol.ToStringTag }, key) < 0) {
            That(TA.Prototype[key], Is.Not.EqualTo(Undefined));
          }
          That(TA.Prototype.Invoke("hasOwnProperty", key), Is.False);
        }
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Buffer(RuntimeFunction buffer) {
      IsUnconstructableFunctionWLength(buffer, "get buffer", 0);
      That(TypedArray.Prototype, Has.OwnProperty("buffer", EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetOwnProperty("buffer").Set, Is.Undefined);

      It("should return buffer from [[ViewedArrayBuffer]] internal slot", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buf = ArrayBuffer.Construct(TA["BYTES_PER_ELEMENT"]);
          EcmaValue arr = TA.Construct(buf);
          Case(arr, buf);
        });
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[TypedArrayName]] internal slot", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8)), Throws.TypeError);
      });

      It("does not throw with a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buf = ArrayBuffer.Construct(8);
          EcmaValue arr = TA.Construct(buf, 0, 1);
          DetachBuffer(buf);
          Case(arr, buf);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ByteLength(RuntimeFunction byteLength) {
      IsUnconstructableFunctionWLength(byteLength, "get byteLength", 0);
      That(TypedArray.Prototype, Has.OwnProperty("byteLength", EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetOwnProperty("byteLength").Set, Is.Undefined);

      It("should return value from [[ByteLength]] internal slot", () => {
        TestWithTypedArrayConstructors(TA => {
          Case(TA.Construct(), 0);
          Case(TA.Construct(42), 42 * TA["BYTES_PER_ELEMENT"]);
        });
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[TypedArrayName]] internal slot", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8)), Throws.TypeError);
      });

      It("should return 0 if the instance has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = TA.Construct(1);
          DetachBuffer(arr);
          Case(arr, 0);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ByteOffset(RuntimeFunction byteOffset) {
      IsUnconstructableFunctionWLength(byteOffset, "get byteOffset", 0);
      That(TypedArray.Prototype, Has.OwnProperty("byteOffset", EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetOwnProperty("byteOffset").Set, Is.Undefined);

      It("should return value from [[ByteOffset]] internal slot", () => {
        TestWithTypedArrayConstructors(TA => {
          Case(TA.Construct(), 0);
          Case(TA.Construct(ArrayBuffer.Construct(8 * TA["BYTES_PER_ELEMENT"]), 4 * TA["BYTES_PER_ELEMENT"]), 4 * TA["BYTES_PER_ELEMENT"]);
          Case(TA.Construct(TA.Construct(ArrayBuffer.Construct(8 * TA["BYTES_PER_ELEMENT"]), 4 * TA["BYTES_PER_ELEMENT"])), 0);
        });
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[TypedArrayName]] internal slot", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8)), Throws.TypeError);
      });

      It("should return 0 if the instance has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = TA.Construct(ArrayBuffer.Construct(128), 8, 1);
          DetachBuffer(arr);
          Case(arr, 0);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Length(RuntimeFunction length) {
      IsUnconstructableFunctionWLength(length, "get length", 0);
      That(TypedArray.Prototype, Has.OwnProperty("length", EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetOwnProperty("length").Set, Is.Undefined);

      It("should return value from [[ArrayLength]] internal slot", () => {
        TestWithTypedArrayConstructors(TA => {
          Case(TA.Construct(), 0);
          Case(TA.Construct(42), 42);
        });
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[TypedArrayName]] internal slot", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8)), Throws.TypeError);
      });

      It("should return 0 if the instance has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = TA.Construct(42);
          DetachBuffer(arr);
          Case(arr, 0);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToStringTag(RuntimeFunction toStringTag) {
      IsUnconstructableFunctionWLength(toStringTag, "get [Symbol.toStringTag]", 0);
      That(TypedArray.Prototype, Has.OwnProperty(Symbol.ToStringTag, EcmaPropertyAttributes.Configurable));
      That(TypedArray.Prototype.GetOwnProperty(Symbol.ToStringTag).Set, Is.Undefined);

      It("should return value from the [[TypedArrayName]] internal slot", () => {
        TestWithTypedArrayConstructors(TA => {
          Case(TA.Construct(1), TA["name"]);
        });
      });

      It("does not throw with a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = TA.Construct(1);
          DetachBuffer(arr);
          Case(arr, TA["name"]);
        });
      });

      It("should return undefined if this value is not Object", () => {
        Case(Undefined, Is.Undefined);
        Case(Null, Is.Undefined);
        Case(false, Is.Undefined);
        Case(0, Is.Undefined);
        Case("", Is.Undefined);
        Case(new Symbol(), Is.Undefined);
      });

      It("should return undefined if this value does not have a [[TypedArrayName]] internal slot", () => {
        Case(TypedArray.Prototype, Is.Undefined);
        Case(Object.Prototype, Is.Undefined);
        Case(EcmaArray.Of(), Is.Undefined);
        Case(ArrayBuffer.Construct(8), Is.Undefined);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8)), Is.Undefined);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void CopyWithin(RuntimeFunction copyWithin) {
      IsUnconstructableFunctionWLength(copyWithin, "copyWithin", 2);
      That(TypedArray.Prototype, Has.OwnProperty("copyWithin", copyWithin, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 0, 0), Throws.TypeError);
        Case((Null, 0, 0), Throws.TypeError);
        Case((42, 0, 0), Throws.TypeError);
        Case(("1", 0, 0), Throws.TypeError);
        Case((true, 0, 0), Throws.TypeError);
        Case((new Symbol("s"), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 0, 0), Throws.TypeError);
        Case((Object.Construct(), 0, 0), Throws.TypeError);
        Case((EcmaArray.Of(), 0, 0), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 0, 0), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, obj, obj), Throws.TypeError);
        });
      });

      It("should return `this`", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample1 = TA.Construct();
          EcmaValue sample2 = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample1, 0, 0), sample1);
          Case((sample2, 1, 0), sample2);
        });
      });

      It("should not return abrupt from Get(O, length) as [[ArrayLength]] is returned", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct();
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, 0, 0), sample);
          }
        });
      });

      It("should preserve bit-level encoding", () => {
        EcmaValue NaNs = ByteConversionValues.NaNs;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue subject = TA.Construct(NaNs["length"] * 2);
          for (int i = 0; i < NaNs["length"].ToInt32(); i++) {
            subject[i] = NaNs[i];
          }
          EcmaValue length = NaNs["length"] * TA["BYTES_PER_ELEMENT"];
          EcmaValue originalBytes = Global.Uint8Array.Construct(subject["buffer"], 0, length);
          copyWithin.Call(subject, NaNs["length"], 0);

          EcmaValue copyBytes = Global.Uint8Array.Construct(subject["buffer"], length);
          for (EcmaValue i = 0, len = originalBytes["length"]; i < len; i += 1) {
            That(copyBytes[i], Is.EqualTo(originalBytes[i]), "Byte at {0} should be {1}", i, originalBytes[i]);
          }
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should coerce end argument to an integer values", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, Null), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, NaN), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, false), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, true), new[] { 0, 0, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, "-2"), new[] { 0, 0, 1, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, -2.5), new[] { 0, 0, 1, 3 });
        });
      });

      It("should coerce start argument to an integer value", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, Undefined), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, false), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, NaN), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, Null), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, true), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, "1"), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0.5), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1.5), new[] { 1, 2, 3, 3 });
        });
      });

      It("should coerce target argument to an integer value", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), Undefined, 1), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), false, 1), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), NaN, 1), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), Null, 1), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), true, 0), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), "1", 0), new[] { 0, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0.5, 1), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1.5, 0), new[] { 0, 0, 1, 2 });
        });
      });

      It("should set max value of end position as this.length", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1, 6), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, 1, Infinity), new[] { 2, 3, 4, 5, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 1, 3, 6), new[] { 0, 3, 4, 5, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 1, 3, Infinity), new[] { 1, 4, 5, 4, 5 });
        });
      });

      It("should set max values of target and start positions as this.length", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 6, 0), new[] { 0, 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 0, 6), new[] { 0, 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 6, 6), new[] { 0, 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 10, 10), new[] { 0, 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), Infinity, 0), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), Infinity, Infinity), new[] { 1, 2, 3, 4, 5 });
        });
      });

      It("should copy values with non-negative target and start positions", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5, 6)), 0, 0), new[] { 1, 2, 3, 4, 5, 6 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5, 6)), 0, 2), new[] { 3, 4, 5, 6, 5, 6 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5, 6)), 3, 0), new[] { 1, 2, 3, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 1, 4), new[] { 0, 4, 5, 3, 4, 5 });
        });
      });

      It("should copy values with non-negative target, start and end positions", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 0, 0), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 0, 2), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1, 2), new[] { 1, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 1, 0, 2), new[] { 0, 0, 1, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4, 5)), 1, 3, 5), new[] { 0, 3, 4, 3, 4, 5 });
        });
      });

      It("should set values with negative end argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1, -1), new[] { 1, 2, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 2, 0, -1), new[] { 0, 1, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 1, 2, -2), new[] { 0, 2, 2, 3, 4 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, -2, -1), new[] { 2, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 2, -2, -1), new[] { 0, 1, 3, 3, 4 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -3, -2, -1), new[] { 0, 2, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -2, -3, -1), new[] { 0, 1, 2, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -5, -2, -1), new[] { 3, 1, 2, 3, 4 });
        });
      });

      It("should set values with negative out of bounds end argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1, -10), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, 1, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, -2, -10), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, -2, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, -9, -10), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, -9, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), -3, -2, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -7, -8, -9), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), -7, -8, -Infinity), new[] { 1, 2, 3, 4, 5 });
        });
      });

      It("should set values with out of bounds negative start argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, -10), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 0, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 2, -10), new[] { 0, 1, 0, 1, 2 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 2, -Infinity), new[] { 1, 2, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 10, -10), new[] { 0, 1, 2, 3, 4 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), 10, -Infinity), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -9, -10), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), -9, -Infinity), new[] { 1, 2, 3, 4, 5 });
        });
      });

      It("should set values with out of bounds negative target argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -10, 0), new[] { 0, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), -Infinity, 0), new[] { 1, 2, 3, 4, 5 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -10, 2), new[] { 2, 3, 4, 3, 4 });
          Case((TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5)), -Infinity, 2), new[] { 3, 4, 5, 4, 5 });
        });
      });

      It("should set values with negative start argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, -1), new[] { 3, 1, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 2, -2), new[] { 0, 1, 3, 4, 4 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), 1, -2), new[] { 0, 3, 4, 3, 4 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -1, -2), new[] { 0, 1, 2, 2 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -2, -3), new[] { 0, 1, 2, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -5, -2), new[] { 3, 4, 2, 3, 4 });
        });
      });

      It("should set values with negative target argument", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -1, 0), new[] { 0, 1, 2, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3, 4)), -2, 2), new[] { 0, 1, 2, 2, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), -1, 2), new[] { 0, 1, 2, 2 });
        });
      });

      It("should set final position to `this.length` if `end` is undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1, Undefined), new[] { 1, 2, 3, 3 });
          Case((TA.Construct(EcmaArray.Of(0, 1, 2, 3)), 0, 1), new[] { 1, 2, 3, 3 });
        });
      });

      It("should return abrupt from ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), 0, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), 0, 0, new Symbol("1")), Throws.TypeError);
        });
      });

      It("should return abrupt from ToInteger(start)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), 0, new Symbol("1")), Throws.TypeError);
        });
      });

      It("should return abrupt from ToInteger(target)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), new Symbol("1")), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Entries(RuntimeFunction entries) {
      IsUnconstructableFunctionWLength(entries, "entries", 0);
      That(TypedArray.Prototype, Has.OwnProperty("entries", entries, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should return an iterator which the prototype is ArrayIteratorPrototype", () => {
        EcmaValue ArrayIteratorProto = Object.Invoke("getPrototypeOf", EcmaArray.Of().Invoke(Symbol.Iterator));
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 42, 64));
          EcmaValue iter = sample.Invoke("entries");
          That(Object.Invoke("getPrototypeOf", iter), Is.EqualTo(ArrayIteratorProto));
        });
      });

      It("should return an iterator for the entries", () => {
        EcmaValue sample = EcmaArray.Of(0, 42, 64);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(sample);
          EcmaValue itor = typedArray.Invoke("entries");
          VerifyIteratorResult(itor.Invoke("next"), false, new[] { 0, 0 });
          VerifyIteratorResult(itor.Invoke("next"), false, new[] { 1, 42 });
          VerifyIteratorResult(itor.Invoke("next"), false, new[] { 2, 64 });
          VerifyIteratorResult(itor.Invoke("next"), true);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Every(RuntimeFunction every) {
      IsUnconstructableFunctionWLength(every, "every", 1);
      That(TypedArray.Prototype, Has.OwnProperty("every", every, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
            return true;
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, Intercept(() => true)), Throws.Nothing);
            That(Logs.Count, Is.EqualTo(2));
          }
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          every.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
            return true;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 0, sample },
            new[] { 8, 1, sample }
          }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          every.Call(TA.Construct(), Intercept(Noop));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          every.Call(sample, Intercept(() => 43));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          every.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
            return true;
          }));
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should call callbackfn with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          every.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
            return true;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }));

          EcmaValue thisArg = EcmaArray.Of("test262", 0, "ecma262", 0);
          results = EcmaArray.Of();
          every.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
            return true;
          }), thisArg);
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }), "thisArg does not affect callbackfn arguments");
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          every.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
            return true;
          }));
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));

          EcmaValue thisArg = Object.Construct();
          results = EcmaArray.Of();
          every.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
            return true;
          }), thisArg);
          That(results, Is.EquivalentTo(new[] { thisArg, thisArg, thisArg }));
        });
      });

      It("should return true if every callbackfn returns a coerced true", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { true, 1, "test262", new Symbol("1"), Object.Construct(), EcmaArray.Of(), -1, Infinity, -Infinity, 0.1, -0.1 };
          Case((TA.Construct(values.Length), Intercept((v, i) => values[(int)i])), true);
          That(Logs.Count, Is.EqualTo(values.Length));
        });
      });

      It("should return false if any callbackfn call returns a coerced false", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { false, "", 0, -0, NaN, Undefined, Null };
          foreach (EcmaValue val in values) {
            int called = 0;
            Case((TA.Construct(42), Intercept(() => Return(++called == 1 ? true : val))), false);
            That(called, Is.EqualTo(2));
          }
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          Case((sample, Intercept(ThrowTest262Exception)), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Fill(RuntimeFunction fill) {
      IsUnconstructableFunctionWLength(fill, "fill", 1);
      That(TypedArray.Prototype, Has.OwnProperty("fill", fill, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, 0), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 0), Throws.TypeError);
        Case((Null, 0), Throws.TypeError);
        Case((42, 0), Throws.TypeError);
        Case(("1", 0), Throws.TypeError);
        Case((true, 0), Throws.TypeError);
        Case((new Symbol("s"), 0), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 0), Throws.TypeError);
        Case((Object.Construct(), 0), Throws.TypeError);
        Case((EcmaArray.Of(), 0), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 0), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 0), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(1);
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, 1, 0), sample);
          }
        });
      });

      It("should return `this`", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample1 = TA.Construct();
          Case((sample1, 1), sample1);
          EcmaValue sample2 = TA.Construct(42);
          Case((sample2, 7), sample2);
        });
      });

      It("should fill all the elements with `value` from a default start and index", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), 8), new EcmaValue[0]);
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8), new[] { 8, 8, 8 });
        });
      });

      It("should fill all the elements from a with a custom start index", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 1), new[] { 0, 8, 8 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 4), new[] { 0, 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, -1), new[] { 0, 0, 8 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, -5), new[] { 8, 8, 8 });
        });
      });

      It("should fill all the elements from a with a custom end index", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 0, 1), new[] { 8, 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 0, -1), new[] { 8, 8, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 0, 5), new[] { 8, 8, 8 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 0, -4), new[] { 0, 0, 0 });
        });
      });

      It("should fill all the elements from a with a custom start and end indexes", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 0, 0)), 8, 1, 2), new[] { 0, 8, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0, 0, 0)), 8, -3, 4), new[] { 0, 0, 8, 8, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0, 0, 0)), 8, -2, -1), new[] { 0, 0, 0, 8, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0, 0, 0)), 8, -1, -3), new[] { 0, 0, 0, 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0, 0, 0, 0)), 8, 1, 3), new[] { 0, 8, 8, 0, 0 });
        });
      });

      It("should fill elements from coerced to Integer `start` and `end` values", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, Undefined), new[] { 1, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, Undefined), new[] { 1, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, Null), new[] { 1, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, Null), new[] { 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, true), new[] { 0, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, true), new[] { 1, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, false), new[] { 1, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, false), new[] { 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, NaN), new[] { 1, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, NaN), new[] { 0, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, "1"), new[] { 0, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, "1"), new[] { 1, 0 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 1.5), new[] { 0, 1 });
          Case((TA.Construct(EcmaArray.Of(0, 0)), 1, 0, 1.5), new[] { 1, 0 });
        });
      });

      It("should fill all the elements with non numeric values", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(42)), Null), new[] { 0 });
          Case((TA.Construct(EcmaArray.Of(42)), false), new[] { 0 });
          Case((TA.Construct(EcmaArray.Of(42)), true), new[] { 1 });
          Case((TA.Construct(EcmaArray.Of(42)), "7"), new[] { 7 });
          Case((TA.Construct(EcmaArray.Of(42)), CreateObject(toString: () => "1", valueOf: () => 7)), new[] { 7 });
          Case((TA.Construct(EcmaArray.Of(42)), CreateObject(toString: () => "7")), new[] { 7 });

          EcmaValue n = 1;
          Case((TA.Construct(2), CreateObject(valueOf: () => n += 1)), new[] { 2, 2 });
        });
      });

      It("should produce consistent canonicalization of NaN values", () => {
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue aNaN in ByteConversionValues.NaNs.ForOf()) {
            EcmaValue samples = TA.Construct(3);
            EcmaValue controls = TA.Construct(EcmaArray.Of(aNaN, aNaN, aNaN));
            fill.Call(samples, aNaN);
            That(samples[0], Is.NaN);
            That(controls[0], Is.NaN);
          }
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should convert to correct values", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(initial));
          fill.Call(sample, value);
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("should return abrupt from ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), 1, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), 1, 0, new Symbol("1")), Throws.TypeError);
        });
      });

      It("should return abrupt from ToInteger(value)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(42)), CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(EcmaArray.Of(42)), new Symbol("1")), Throws.TypeError);
        });
      });

      It("should return abrupt from ToInteger(start)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), 1, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), 1, new Symbol("1")), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Filter(RuntimeFunction filter) {
      IsUnconstructableFunctionWLength(filter, "filter", 1);
      That(TypedArray.Prototype, Has.OwnProperty("filter", filter, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(4);
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            filter.Call(sample, Intercept(() => Undefined));
            That(Logs.Count, Is.EqualTo(4));
          }
        });
      });

      It("should call callbackfn with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          filter.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }));

          EcmaValue thisArg = EcmaArray.Of("test262", 0, "ecma262", 0);
          results = EcmaArray.Of();
          filter.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))), thisArg);
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }), "thisArg does not affect callbackfn arguments");
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          filter.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }));
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));

          EcmaValue thisArg = Object.Construct();
          results = EcmaArray.Of();
          filter.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] { thisArg, thisArg, thisArg }));
        });
      });

      It("should call callbackfn for each item before TypedArraySpeciesCreate", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue length = 42;
          EcmaValue sample = TA.Construct(length);
          bool before = false;

          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = RuntimeFunction.Create(() => Void(before = Logs.Count == 42)) }));
          filter.Call(sample, Intercept(Noop));
          That(Logs.Count, Is.EqualTo(42));
          That(before, Is.True);
        });
      });

      It("should call callbackfn for each item before TypedArraySpeciesCreate", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue length = 42;
          EcmaValue sample = TA.Construct(length);
          bool before = false;

          sample["constructor"] = Object.Construct();
          Object.Invoke("defineProperty", sample["constructor"], Symbol.Species, CreateObject(new { get = RuntimeFunction.Create(() => Void(before = Logs.Count == 42)) }));
          filter.Call(sample, Intercept(Noop));
          That(Logs.Count, Is.EqualTo(42));
          That(before, Is.True);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          filter.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
            return true;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 0, sample },
            new[] { 8, 1, sample }
          }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          filter.Call(TA.Construct(), Intercept(Noop));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          filter.Call(sample, Intercept(() => 43));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          filter.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
          }));
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          Case((sample, Intercept(ThrowTest262Exception)), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return a TypedArray instance with a different buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          That(filter.Call(sample, RuntimeFunction.Create(() => true))["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(filter.Call(sample, RuntimeFunction.Create(() => false))["buffer"], Is.Not.EqualTo(sample["buffer"]));
        });
      });

      It("should return full length result if every callbackfn returns boolean true", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { true, 1, "test262", new Symbol("1"), Object.Construct(), EcmaArray.Of(), -1, Infinity, -Infinity, 0.1, -0.1 };
          foreach (EcmaValue val in values) {
            Case((TA.Construct(EcmaArray.Of(40, 41, 42)), Intercept(() => val)), new[] { 40, 41, 42 });
          }
        });
      });

      It("should return empty result if every callbackfn returns boolean false", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { false, "", 0, -0, NaN, Undefined, Null };
          foreach (EcmaValue val in values) {
            Case((TA.Construct(EcmaArray.Of(40, 41, 42)), Intercept(() => val)), new EcmaValue[0]);
          }
        });
      });

      It("should return instance with filtered values set on it", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(41, 1, 42, 7));
          Case((sample, RuntimeFunction.Create(v => v > 40)), new[] { 41, 42 });
        });
      });

      It("should get constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = Intercept(Noop) }));

          EcmaValue result = filter.Call(sample, Noop);
          That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
          That(result["constructor"], Is.EqualTo(TA), "use defaultCtor on an undefined return - constructor check");
        });
      });

      It("should get inherited constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          using (TempProperty(TA.Prototype, "constructor", new EcmaPropertyDescriptor(Intercept(Noop), Undefined))) {
            EcmaValue result = filter.Call(sample, Noop);
            That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

            That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
            That(result["constructor"], Is.Undefined, "used defaultCtor but still checks the inherited .constructor");
            That(Logs.Count, Is.EqualTo(2), "result.constructor triggers the inherited accessor property");
          }
        });
      });

      It("should return abrupt from SpeciesConstructor's get Constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, Noop), Throws.Test262);
        });
      });

      It("should throw if O.constructor returns a non-Object and non-undefined value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));

          sample["constructor"] = 42;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = "1";
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = Null;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = NaN;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = false;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = new Symbol();
          Case((sample, Noop), Throws.TypeError);
        });
      });

      It("should get @@species from found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, get: Intercept(() => Undefined), set: null));
          filter.Call(sample, Noop);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from get @@species on found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();
          Object.Invoke("defineProperty", sample["constructor"], Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, Noop), Throws.Test262);
        });
      });

      It("should use custom species constructor if available", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => TA.Construct(count))));
          Case((sample, RuntimeFunction.Create(() => true)), new[] { 40, 41, 42 });
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should call custom species constructor with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 42, 42));
          EcmaValue result = default;
          EcmaValue ctorThis = default;
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => {
            result = Arguments;
            ctorThis = This;
            return TA.Construct(count);
          })));
          filter.Call(sample, RuntimeFunction.Create(v => v == 42));
          That(result, Is.EquivalentTo(new[] { 2 }));
          That(ctorThis, Is.InstanceOf(sample["constructor"][Symbol.Species]));
        });
      });

      It("may return a totally different TypedArray from custom species constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40));
          EcmaValue other = Global.Int8Array.Construct(EcmaArray.Of(1, 0, 1));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(() => other)));
          Case((sample, Noop), new[] { 1, 0, 1 });
        });
      });

      It("does not throw a TypeError if new typedArray's length >= count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue customCount = default;
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct(customCount))));

          customCount = 2;
          That(filter.Call(sample, RuntimeFunction.Create(() => true))["length"], Is.EqualTo(customCount));
          customCount = 5;
          That(filter.Call(sample, RuntimeFunction.Create(() => true))["length"], Is.EqualTo(customCount));
        });
      });

      It("should throw a TypeError if new typedArray's length < count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct())));
          Case((sample, RuntimeFunction.Create(() => true)), Throws.TypeError);
        });
      });

      It("should use defaultConstructor if @@species is either undefined or null", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          EcmaValue result = filter.Call(sample, Noop);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));

          sample["constructor"].ToObject()[Symbol.Species] = Null;
          result = filter.Call(sample, Noop);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));
        });
      });

      It("should throw if returned @@species is not a constructor, null or undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          sample["constructor"].ToObject()[Symbol.Species] = 0;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = "string";
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = Object.Construct();
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = NaN;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = false;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = true;
          Case((sample, Noop), Throws.TypeError);
        });
      });

      It("should throw if custom species constructor does not return a compatible object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, Noop));
          Case((sample, Noop), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Find(RuntimeFunction find) {
      IsUnconstructableFunctionWLength(find, "find", 1);
      That(TypedArray.Prototype, Has.OwnProperty("find", find, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, RuntimeFunction.Create(() => true)), 42);
          }
        });
      });

      It("should call predicate called as F.call( thisArg, kValue, k, O ) for each array entry", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(39, 2, 62));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = "bar";

          find.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
          That(results, Is.EquivalentTo(new[] {
            new[] { 39, 0, sample },
            new[] { 2, 1, sample },
            new[] { 62, 2, sample }
          }));
        });
      });

      It("should call predicate with correct this value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          EcmaValue result = Null;
          find.Call(sample, RuntimeFunction.Create(() => result = This));
          That(result, Is.Undefined);

          EcmaValue o = Object.Construct();
          find.Call(sample, RuntimeFunction.Create(() => result = This), o);
          That(result, Is.EqualTo(o));
        });
      });

      It("should throw a TypeError exception if predicate is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, Null), Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, ""), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, EcmaArray.Of()), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
          Case((sample, RegExp.Construct(".")), Throws.TypeError);
        });
      });

      It("should throw a TypeError exception if predicate detaches the buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue completion = false;
          Case((sample, Intercept(() => {
            DetachBuffer(sample);
            completion = true;
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
          That(completion, Is.True);
        });
      });

      It("should handle changed values during predicate call", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = EcmaArray.Of(1, 2, 3);
          EcmaValue sample, result;

          sample = TA.Construct(3);
          find.Call(sample, RuntimeFunction.Create((v, i) => {
            sample[i] = arr[i];
            That(v, Is.EqualTo(0), "value is not mapped to instance");
          }));
          That(sample, Is.EquivalentTo(new[] { 1, 2, 3 }), "values set during each predicate call");

          sample = TA.Construct(arr);
          result = find.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i == 0) {
              sample[2] = 7;
            }
            return val == 7;
          }));
          That(result, Is.EqualTo(7), "value found");

          sample = TA.Construct(arr);
          result = find.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i == 0) {
              sample[2] = 7;
            }
            return val == 3;
          }));
          That(result, Is.Undefined, "value not found");

          sample = TA.Construct(arr);
          result = find.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              sample[0] = 7;
            }
            return val == 7;
          }));
          That(result, Is.Undefined, "value not found - changed after call");

          sample = TA.Construct(arr);
          result = find.Call(sample, RuntimeFunction.Create(() => {
            sample[0] = 7;
            return true;
          }));
          That(result, Is.EqualTo(1), "find() returns previous found value");
        });
      });

      It("should not call predicate on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => true)), Undefined);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return found value if predicate return a boolean true value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(39, 2, 62));
          EcmaValue called, result;

          called = 0;
          result = find.Call(sample, RuntimeFunction.Create(() => {
            called += 1;
            return true;
          }));
          That(result, Is.EqualTo(39), "returned true on sample[0]");
          That(called, Is.EqualTo(1), "predicate was called once");

          called = 0;
          result = find.Call(sample, RuntimeFunction.Create((val) => {
            called += 1;
            return val == 62;
          }));
          That(called, Is.EqualTo(3), "predicate was called three times");
          That(result, Is.EqualTo(62), "returned true on sample[3]");

          result = find.Call(sample, RuntimeFunction.Create(() => "string"));
          That(result, Is.EqualTo(39), "ToBoolean(string)");

          result = find.Call(sample, RuntimeFunction.Create(() => Object.Construct()));
          That(result, Is.EqualTo(39), "ToBoolean(object)");

          result = find.Call(sample, RuntimeFunction.Create(() => new Symbol("")));
          That(result, Is.EqualTo(39), "ToBoolean(symbol)");

          result = find.Call(sample, RuntimeFunction.Create(() => 1));
          That(result, Is.EqualTo(39), "ToBoolean(number)");

          result = find.Call(sample, RuntimeFunction.Create(() => -1));
          That(result, Is.EqualTo(39), "ToBoolean(negative number)");
        });
      });

      It("should return undefined if predicate always returns a boolean false value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue called = 0;

          EcmaValue result = find.Call(sample, RuntimeFunction.Create(() => {
            called += 1;
            return false;
          }));

          That(called, Is.EqualTo(3), "predicate was called three times");
          That(result, Is.Undefined);

          result = find.Call(sample, RuntimeFunction.Create(() => ""));
          That(result, Is.Undefined, "ToBoolean(empty string)");

          result = find.Call(sample, RuntimeFunction.Create(() => Undefined));
          That(result, Is.Undefined, "ToBoolean(undefined)");

          result = find.Call(sample, RuntimeFunction.Create(() => Null));
          That(result, Is.Undefined, "ToBoolean(null)");

          result = find.Call(sample, RuntimeFunction.Create(() => 0));
          That(result, Is.Undefined, "ToBoolean(0)");

          result = find.Call(sample, RuntimeFunction.Create(() => -0));
          That(result, Is.Undefined, "ToBoolean(-0)");

          result = find.Call(sample, RuntimeFunction.Create(() => NaN));
          That(result, Is.Undefined, "ToBoolean(NaN)");
        });
      });

      It("should return abrupt from predicate call", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(1), ThrowTest262Exception), Throws.Test262);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void FindIndex(RuntimeFunction findIndex) {
      IsUnconstructableFunctionWLength(findIndex, "findIndex", 1);
      That(TypedArray.Prototype, Has.OwnProperty("findIndex", findIndex, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, RuntimeFunction.Create(() => true)), 0);
          }
        });
      });

      It("should call predicate called as F.call( thisArg, kValue, k, O ) for each array entry", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(39, 2, 62));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = "bar";

          findIndex.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
          That(results, Is.EquivalentTo(new[] {
            new[] { 39, 0, sample },
            new[] { 2, 1, sample },
            new[] { 62, 2, sample }
          }));
        });
      });

      It("should call predicate with correct this value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          EcmaValue result = Null;
          findIndex.Call(sample, RuntimeFunction.Create(() => result = This));
          That(result, Is.Undefined);

          EcmaValue o = Object.Construct();
          findIndex.Call(sample, RuntimeFunction.Create(() => result = This), o);
          That(result, Is.EqualTo(o));
        });
      });

      It("should throw a TypeError exception if predicate is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, Null), Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, ""), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, EcmaArray.Of()), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
          Case((sample, RegExp.Construct(".")), Throws.TypeError);
        });
      });

      It("should throw a TypeError exception if predicate detaches the buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue completion = false;
          Case((sample, Intercept(() => {
            DetachBuffer(sample);
            completion = true;
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
          That(completion, Is.True);
        });
      });

      It("should handle changed values during predicate call", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue arr = EcmaArray.Of(10, 20, 30);
          EcmaValue sample, result;

          sample = TA.Construct(3);
          findIndex.Call(sample, RuntimeFunction.Create((v, i) => {
            sample[i] = arr[i];
            That(v, Is.EqualTo(0), "value is not mapped to instance");
          }));
          That(sample, Is.EquivalentTo(new[] { 10, 20, 30 }), "values set during each predicate call");

          sample = TA.Construct(arr);
          result = findIndex.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i == 0) {
              sample[2] = 7;
            }
            return val == 7;
          }));
          That(result, Is.EqualTo(2), "value found");

          sample = TA.Construct(arr);
          result = findIndex.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i == 0) {
              sample[2] = 7;
            }
            return val == 3;
          }));
          That(result, Is.EqualTo(-1), "value not found");

          sample = TA.Construct(arr);
          result = findIndex.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              sample[0] = 7;
            }
            return val == 7;
          }));
          That(result, Is.EqualTo(-1), "value not found - changed after call");
        });
      });

      It("should not call predicate on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => true)), -1);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return index if predicate return a boolean true value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(39, 2, 62));
          EcmaValue called, result;

          called = 0;
          result = findIndex.Call(sample, RuntimeFunction.Create(() => {
            called += 1;
            return true;
          }));
          That(result, Is.EqualTo(0), "returned true on sample[0]");
          That(called, Is.EqualTo(1), "predicate was called once");

          called = 0;
          result = findIndex.Call(sample, RuntimeFunction.Create((val) => {
            called += 1;
            return val == 62;
          }));
          That(called, Is.EqualTo(3), "predicate was called three times");
          That(result, Is.EqualTo(2), "returned true on sample[3]");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => "string"));
          That(result, Is.EqualTo(0), "ToBoolean(string)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => Object.Construct()));
          That(result, Is.EqualTo(0), "ToBoolean(object)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => new Symbol("")));
          That(result, Is.EqualTo(0), "ToBoolean(symbol)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => 1));
          That(result, Is.EqualTo(0), "ToBoolean(number)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => -1));
          That(result, Is.EqualTo(0), "ToBoolean(negative number)");
        });
      });

      It("should return -1 if predicate always returns a boolean false value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue called = 0;

          EcmaValue result = findIndex.Call(sample, RuntimeFunction.Create(() => {
            called += 1;
            return false;
          }));

          That(called, Is.EqualTo(3), "predicate was called three times");
          That(result, Is.EqualTo(-1));

          result = findIndex.Call(sample, RuntimeFunction.Create(() => ""));
          That(result, Is.EqualTo(-1), "ToBoolean(empty string)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => Undefined));
          That(result, Is.EqualTo(-1), "ToBoolean(undefined)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => Null));
          That(result, Is.EqualTo(-1), "ToBoolean(null)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => 0));
          That(result, Is.EqualTo(-1), "ToBoolean(0)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => -0));
          That(result, Is.EqualTo(-1), "ToBoolean(-0)");

          result = findIndex.Call(sample, RuntimeFunction.Create(() => NaN));
          That(result, Is.EqualTo(-1), "ToBoolean(NaN)");
        });
      });

      It("should return abrupt from predicate call", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(1), ThrowTest262Exception), Throws.Test262);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ForEach(RuntimeFunction forEach) {
      IsUnconstructableFunctionWLength(forEach, "forEach", 1);
      That(TypedArray.Prototype, Has.OwnProperty("forEach", forEach, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, Intercept(() => false)), Throws.Nothing);
            That(Logs.Count, Is.EqualTo(2));
          }
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          forEach.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 0, sample },
            new[] { 8, 1, sample }
          }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          forEach.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
          }));
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          forEach.Call(TA.Construct(), Intercept(Noop));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          forEach.Call(sample, Intercept(() => 43));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42 }));
        });
      });

      It("should call callbackfn with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          forEach.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }));

          EcmaValue thisArg = EcmaArray.Of("test262", 0, "ecma262", 0);
          results = EcmaArray.Of();
          forEach.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }), "thisArg does not affect callbackfn arguments");
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          forEach.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }));
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));

          EcmaValue thisArg = Object.Construct();
          results = EcmaArray.Of();
          forEach.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] { thisArg, thisArg, thisArg }));
        });
      });

      It("should return undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Case((sample, RuntimeFunction.Create(() => 42)), Is.Undefined);
          Case((sample, RuntimeFunction.Create(() => Null)), Is.Undefined);
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          Case((sample, Intercept(ThrowTest262Exception)), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Includes(RuntimeFunction includes) {
      IsUnconstructableFunctionWLength(includes, "includes", 1);
      That(TypedArray.Prototype, Has.OwnProperty("includes", includes, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 42), Throws.TypeError);
        Case((Null, 42), Throws.TypeError);
        Case((42, 42), Throws.TypeError);
        Case(("1", 42), Throws.TypeError);
        Case((true, 42), Throws.TypeError);
        Case((new Symbol("s"), 42), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 42), Throws.TypeError);
        Case((Object.Construct(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 42), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 42), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(7));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, 7), true);
          }
        });
      });

      It("should return false if fromIndex >= ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(42);
          Case((sample, 0, 42), false);
          Case((sample, 0, 43), false);
        });
      });

      It("should handle Infinity values for fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 43, Infinity), false);
          Case((sample, 43, -Infinity), true);
        });
      });

      It("should treat -0 fromIndex as 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, -0d), true);
          Case((sample, 43, -0d), true);
          Case((sample, 44, -0d), false);
        });
      });

      It("should return false if length is 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case(sample, false);
          Case((sample, 0), false);
          Case((sample, 0, CreateObject(valueOf: ThrowTest262Exception)), false);
        });
      });

      It("should return abrupt from ToInteger(fromIndex)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7));
          Case((sample, 7, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((sample, 7, new Symbol()), Throws.TypeError);
        });
      });

      It("should compare search element using SameValueZero", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 0, 1, Undefined));
          Case(sample, false, "no arg");
          Case((sample, Undefined), false, "undefined");
          Case((sample, "42"), false, "'42'");
          Case((sample, EcmaArray.Of(42)), false, "[42]");
          Case((sample, 42.0), true, "42.0");
          Case((sample, -0d), true, "-0");
          Case((sample, true), false, "true");
          Case((sample, false), false, "false");
          Case((sample, Null), false, "null");
          Case((sample, ""), false, "empty string");
        });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 0, 1, Undefined, NaN));
          Case((sample, NaN), true, "NaN");
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should return true for found index", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 42), true, "includes(42)");
          Case((sample, 43), true, "includes(43)");
          Case((sample, 43, 1), true, "includes(43, 1)");
          Case((sample, 42, 1), true, "includes(42, 1)");
          Case((sample, 42, 2), true, "includes(42, 2)");
          Case((sample, 42, -4), true, "includes(42, -4)");
          Case((sample, 42, -3), true, "includes(42, -3)");
          Case((sample, 42, -2), true, "includes(42, -2)");
          Case((sample, 42, -5), true, "includes(42, -5)");
        });
      });

      It("should return false if the element is not found", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 44), false, "includes(44)");
          Case((sample, 43, 2), false, "includes(43, 2)");
          Case((sample, 42, 3), false, "includes(42, 3)");
          Case((sample, 44, -4), false, "includes(44, -4)");
          Case((sample, 44, -5), false, "includes(44, -5)");
          Case((sample, 42, -1), false, "includes(42, -1)");
        });
      });

      It("should get the integer value from fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, "1"), false, "string [0]");
          Case((sample, 43, "1"), true, "string [1]");
          Case((sample, 42, true), false, "true [0]");
          Case((sample, 43, true), true, "true [1]");
          Case((sample, 42, false), true, "false [0]");
          Case((sample, 43, false), true, "false [1]");
          Case((sample, 42, NaN), true, "NaN [0]");
          Case((sample, 43, NaN), true, "NaN [1]");
          Case((sample, 42, Null), true, "null [0]");
          Case((sample, 43, Null), true, "null [1]");
          Case((sample, 42, Undefined), true, "undefined [0]");
          Case((sample, 43, Undefined), true, "undefined [1]");
          Case((sample, 42, CreateObject(valueOf: () => 1)), false, "object [0]");
          Case((sample, 43, CreateObject(valueOf: () => 1)), true, "object [1]");
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IndexOf(RuntimeFunction indexOf) {
      IsUnconstructableFunctionWLength(indexOf, "indexOf", 1);
      That(TypedArray.Prototype, Has.OwnProperty("indexOf", indexOf, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 42), Throws.TypeError);
        Case((Null, 42), Throws.TypeError);
        Case((42, 42), Throws.TypeError);
        Case(("1", 42), Throws.TypeError);
        Case((true, 42), Throws.TypeError);
        Case((new Symbol("s"), 42), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 42), Throws.TypeError);
        Case((Object.Construct(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 42), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 42), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(7));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, 7), 0);
          }
        });
      });

      It("should return -1 if fromIndex >= ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(42);
          Case((sample, 0, 42), -1);
          Case((sample, 0, 43), -1);
        });
      });

      It("should handle Infinity values for fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 43, Infinity), -1);
          Case((sample, 43, -Infinity), 1);
        });
      });

      It("should treat -0 fromIndex as 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, -0d), 0);
          Case((sample, 43, -0d), 1);
        });
      });

      It("should return -1 if length is 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case(sample, -1);
          Case((sample, 0), -1);
          Case((sample, 0, CreateObject(valueOf: ThrowTest262Exception)), -1);
        });
      });

      It("should get the integer value from fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, "1"), -1, "string [0]");
          Case((sample, 43, "1"), 1, "string [1]");
          Case((sample, 42, true), -1, "true [0]");
          Case((sample, 43, true), 1, "true [1]");
          Case((sample, 42, false), 0, "false [0]");
          Case((sample, 43, false), 1, "false [1]");
          Case((sample, 42, NaN), 0, "NaN [0]");
          Case((sample, 43, NaN), 1, "NaN [1]");
          Case((sample, 42, Null), 0, "null [0]");
          Case((sample, 43, Null), 1, "null [1]");
          Case((sample, 42, Undefined), 0, "undefined [0]");
          Case((sample, 43, Undefined), 1, "undefined [1]");
          Case((sample, 42, CreateObject(valueOf: () => 1)), -1, "object [0]");
          Case((sample, 43, CreateObject(valueOf: () => 1)), 1, "object [1]");
        });
      });

      It("should return abrupt from ToInteger(fromIndex)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7));
          Case((sample, 7, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((sample, 7, new Symbol()), Throws.TypeError);
        });
      });

      It("should return index for the first found element", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 42), 0, "indexOf(42)");
          Case((sample, 43), 1, "indexOf(43)");
          Case((sample, 43, 1), 1, "indexOf(43, 1)");
          Case((sample, 42, 1), 2, "indexOf(42, 1)");
          Case((sample, 42, 2), 2, "indexOf(42, 2)");
          Case((sample, 42, -4), 0, "indexOf(42, -4)");
          Case((sample, 42, -3), 2, "indexOf(42, -3)");
          Case((sample, 42, -2), 2, "indexOf(42, -2)");
          Case((sample, 42, -5), 0, "indexOf(42, -5)");
        });
      });

      It("should return -1 if the element is not found", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 44), -1, "indexOf(44)");
          Case((sample, 43, 2), -1, "indexOf(43, 2)");
          Case((sample, 42, 3), -1, "indexOf(42, 3)");
          Case((sample, 44, -4), -1, "indexOf(44, -4)");
          Case((sample, 44, -5), -1, "indexOf(44, -5)");
          Case((sample, 42, -1), -1, "indexOf(42, -1)");
        });
      });

      It("should compare search element using strict comparing", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 0, 1, Undefined, NaN));
          Case((sample, "42"), -1, "'42'");
          Case((sample, EcmaArray.Of(42)), -1, "[42]");
          Case((sample, 42.0), 0, "42.0");
          Case((sample, -0d), 1, "-0");
          Case((sample, true), -1, "true");
          Case((sample, false), -1, "false");
          Case((sample, NaN), -1, "null");
          Case((sample, Null), -1, "null");
          Case((sample, Undefined), -1, "undefined");
          Case((sample, ""), -1, "empty string");
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Join(RuntimeFunction join) {
      IsUnconstructableFunctionWLength(join, "join", 1);
      That(TypedArray.Prototype, Has.OwnProperty("join", join, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, ""), Throws.TypeError);
        Case((Null, ""), Throws.TypeError);
        Case((42, ""), Throws.TypeError);
        Case(("1", ""), Throws.TypeError);
        Case((true, ""), Throws.TypeError);
        Case((new Symbol("s"), ""), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, ""), Throws.TypeError);
        Case((Object.Construct(), ""), Throws.TypeError);
        Case((EcmaArray.Of(), ""), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), ""), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), ""), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, CreateObject(toString: ThrowTest262Exception)), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case(sample, Is.Not.EqualTo(""));
          }
        });
      });

      It("should return the empty String if length is 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case(sample, "");
          Case((sample, "test262"), "");
        });
      });

      It("should concatenate the result of toString for each simple value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 0, 2, 3, 42, 127));
          Case(sample, "1,0,2,3,42,127");

          EcmaValue arr = EcmaArray.Of(-2, Infinity, NaN, -Infinity, 0.6, 9007199254740992d);
          sample = TA.Construct(arr);
          Case(sample, arr.Invoke("map", RuntimeFunction.Create((_, i) => sample[i].Invoke("toString"))).Invoke("join"));
        });
      });

      It("should concatenate the result of toString for each value with custom separator", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 0, 2, 3, 42, 127));
          Case((sample, ","), "1,0,2,3,42,127");
          Case((sample, Undefined), "1,0,2,3,42,127");
          Case((sample, Null), "1null0null2null3null42null127");
          Case((sample, ",,"), "1,,0,,2,,3,,42,,127");
          Case((sample, "0"), "10002030420127");
          Case((sample, ""), "102342127");
          Case((sample, " a b c "), "1 a b c 0 a b c 2 a b c 3 a b c 42 a b c 127");
          Case((sample, Object.Construct()), "1[object Object]0[object Object]2[object Object]3[object Object]42[object Object]127");
          Case((sample, true), "1true0true2true3true42true127");
          Case((sample, CreateObject(toString: () => "foo")), "1foo0foo2foo3foo42foo127");
          Case((sample, CreateObject(new { toString = Undefined, valueOf = RuntimeFunction.Create(() => "bar") })), "1bar0bar2bar3bar42bar127");
          Case((sample, false), "1false0false2false3false42false127");
          Case((sample, -1), "1-10-12-13-142-1127");
          Case((sample, -0d), "10002030420127");

          EcmaValue arr = EcmaArray.Of(-2, Infinity, NaN, -Infinity, 0.6, 9007199254740992d);
          sample = TA.Construct(arr);
          void CheckSeparater(EcmaValue sep) {
            Case((sample, sep), arr.Invoke("map", RuntimeFunction.Create((_, i) => sample[i].Invoke("toString"))).Invoke("join", sep));
          }
          CheckSeparater(",");
          CheckSeparater(Undefined);
          CheckSeparater(",,");
          CheckSeparater("0");
          CheckSeparater("");
          CheckSeparater(" a b c ");
          CheckSeparater(Object.Construct());
          CheckSeparater(true);
          CheckSeparater(CreateObject(toString: () => "foo"));
          CheckSeparater(CreateObject(new { toString = Undefined, valueOf = RuntimeFunction.Create(() => "bar") }));
          CheckSeparater(false);
          CheckSeparater(-1);
          CheckSeparater(-0d);
        });
      });

      It("should return abrupt from ToString(Symbol separator)", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((TA.Construct(), new Symbol()), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Keys(RuntimeFunction keys) {
      IsUnconstructableFunctionWLength(keys, "keys", 0);
      That(TypedArray.Prototype, Has.OwnProperty("keys", keys, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should return an iterator which the prototype is ArrayIteratorPrototype", () => {
        EcmaValue ArrayIteratorProto = Object.Invoke("getPrototypeOf", EcmaArray.Of().Invoke(Symbol.Iterator));
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 42, 64));
          EcmaValue iter = sample.Invoke("keys");
          That(Object.Invoke("getPrototypeOf", iter), Is.EqualTo(ArrayIteratorProto));
        });
      });

      It("should return an iterator for the keys", () => {
        EcmaValue sample = EcmaArray.Of(0, 42, 64);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(sample);
          EcmaValue itor = typedArray.Invoke("keys");
          VerifyIteratorResult(itor.Invoke("next"), false, 0);
          VerifyIteratorResult(itor.Invoke("next"), false, 1);
          VerifyIteratorResult(itor.Invoke("next"), false, 2);
          VerifyIteratorResult(itor.Invoke("next"), true);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void LastIndexOf(RuntimeFunction lastIndexOf) {
      IsUnconstructableFunctionWLength(lastIndexOf, "lastIndexOf", 1);
      That(TypedArray.Prototype, Has.OwnProperty("lastIndexOf", lastIndexOf, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 42), Throws.TypeError);
        Case((Null, 42), Throws.TypeError);
        Case((42, 42), Throws.TypeError);
        Case(("1", 42), Throws.TypeError);
        Case((true, 42), Throws.TypeError);
        Case((new Symbol("s"), 42), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 42), Throws.TypeError);
        Case((Object.Construct(), 42), Throws.TypeError);
        Case((EcmaArray.Of(), 42), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 42), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 42), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(7));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, 7), 0);
          }
        });
      });

      It("should handle Infinity values for fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 43, 41));
          Case((sample, 43, Infinity), 2);
          Case((sample, 43, -Infinity), -1);
        });
      });

      It("should treat -0 fromIndex as 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, -0d), 0);
          Case((sample, 43, -0d), -1);
        });
      });

      It("should return -1 if length is 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case(sample, -1);
          Case((sample, 0), -1);
          Case((sample, 0, CreateObject(valueOf: ThrowTest262Exception)), -1);
        });
      });

      It("should get the integer value from fromIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Case((sample, 42, "1"), 0, "string [0]");
          Case((sample, 43, "1"), 1, "string [1]");
          Case((sample, 42, true), 0, "true [0]");
          Case((sample, 43, true), 1, "true [1]");
          Case((sample, 42, false), 0, "false [0]");
          Case((sample, 43, false), -1, "false [1]");
          Case((sample, 42, NaN), 0, "NaN [0]");
          Case((sample, 43, NaN), -1, "NaN [1]");
          Case((sample, 42, Null), 0, "null [0]");
          Case((sample, 43, Null), -1, "null [1]");
          Case((sample, 42, Undefined), 0, "undefined [0]");
          Case((sample, 43, Undefined), -1, "undefined [1]");
          Case((sample, 42, CreateObject(valueOf: () => 1)), 0, "object [0]");
          Case((sample, 43, CreateObject(valueOf: () => 1)), 1, "object [1]");
        });
      });

      It("should return abrupt from ToInteger(fromIndex)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7));
          Case((sample, 7, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((sample, 7, new Symbol()), Throws.TypeError);
        });
      });

      It("should return index for the first found element", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 42), 2, "lastIndexOf(42)");
          Case((sample, 43), 1, "lastIndexOf(43)");
          Case((sample, 41), 3, "lastIndexOf(41)");
          Case((sample, 41, 3), 3, "lastIndexOf(41, 3)");
          Case((sample, 41, 4), 3, "lastIndexOf(41, 4)");
          Case((sample, 43, 1), 1, "lastIndexOf(43, 1)");
          Case((sample, 43, 2), 1, "lastIndexOf(43, 2)");
          Case((sample, 43, 3), 1, "lastIndexOf(43, 3)");
          Case((sample, 43, 4), 1, "lastIndexOf(43, 4)");
          Case((sample, 42, 0), 0, "lastIndexOf(42, 0)");
          Case((sample, 42, 1), 0, "lastIndexOf(42, 1)");
          Case((sample, 42, 2), 2, "lastIndexOf(42, 2)");
          Case((sample, 42, 3), 2, "lastIndexOf(42, 3)");
          Case((sample, 42, 4), 2, "lastIndexOf(42, 4)");
          Case((sample, 42, -4), 0, "lastIndexOf(42, -4)");
          Case((sample, 42, -3), 0, "lastIndexOf(42, -3)");
          Case((sample, 42, -2), 2, "lastIndexOf(42, -2)");
          Case((sample, 42, -1), 2, "lastIndexOf(42, -1)");
          Case((sample, 43, -3), 1, "lastIndexOf(43, -3)");
          Case((sample, 43, -2), 1, "lastIndexOf(43, -2)");
          Case((sample, 43, -1), 1, "lastIndexOf(43, -1)");
          Case((sample, 41, -1), 3, "lastIndexOf(41, -1)");
        });
      });

      It("should return -1 if the element if not found", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 42, 41));
          Case((sample, 44), -1, "lastIndexOf(44)");
          Case((sample, 44, -4), -1, "lastIndexOf(44, -4)");
          Case((sample, 44, -5), -1, "lastIndexOf(44, -5)");
          Case((sample, 42, -5), -1, "lastIndexOf(42, -5)");
          Case((sample, 43, -4), -1, "lastIndexOf(43, -4)");
          Case((sample, 43, -5), -1, "lastIndexOf(43, -5)");
          Case((sample, 41, 0), -1, "lastIndexOf(41, 0)");
          Case((sample, 41, 1), -1, "lastIndexOf(41, 1)");
          Case((sample, 41, 2), -1, "lastIndexOf(41, 2)");
          Case((sample, 43, 0), -1, "lastIndexOf(43, 0)");
        });
      });

      It("should compare search element using strict comparing", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, Undefined, NaN, 0, 1));
          Case((sample, "42"), -1, "'42'");
          Case((sample, EcmaArray.Of(42)), -1, "[42]");
          Case((sample, 42.0), 0, "42.0");
          Case((sample, -0d), 3, "-0");
          Case((sample, true), -1, "true");
          Case((sample, false), -1, "false");
          Case((sample, NaN), -1, "null");
          Case((sample, Null), -1, "null");
          Case((sample, Undefined), -1, "undefined");
          Case((sample, ""), -1, "empty string");
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Map(RuntimeFunction map) {
      IsUnconstructableFunctionWLength(map, "map", 1);
      That(TypedArray.Prototype, Has.OwnProperty("map", map, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(4);
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            map.Call(sample, Intercept(() => Undefined));
            That(Logs.Count, Is.EqualTo(4));
          }
        });
      });

      It("should call callbackfn with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          map.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }));

          EcmaValue thisArg = EcmaArray.Of("test262", 0, "ecma262", 0);
          results = EcmaArray.Of();
          map.Call(sample, RuntimeFunction.Create(() => Void(results.Invoke("push", Arguments))), thisArg);
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }), "thisArg does not affect callbackfn arguments");
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          map.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }));
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));

          EcmaValue thisArg = Object.Construct();
          results = EcmaArray.Of();
          map.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] { thisArg, thisArg, thisArg }));
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
            return 0;
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          map.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
            return 0;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 0, sample },
            new[] { 8, 1, sample }
          }));
        });
      });

      It("should not import own properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bar = new Symbol("1");
          EcmaValue sample = TA.Construct(EcmaArray.Of(41, 42, 43, 44));
          sample["foo"] = 42;
          sample[bar] = 1;

          EcmaValue result = map.Call(sample, RuntimeFunction.Create(() => 0));
          That(result.Invoke("hasOwnProperty", "foo"), Is.False);
          That(result.Invoke("hasOwnProperty", bar), Is.False);
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          map.Call(TA.Construct(), Intercept(Noop));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          map.Call(sample, Intercept(() => 43));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          map.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
            return 0;
          }));
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          Case((sample, Intercept(ThrowTest262Exception)), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return a TypedArray instance with a different buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample1 = TA.Construct(EcmaArray.Of(40, 41, 42));
          That(map.Call(sample1, RuntimeFunction.Create(v => v))["buffer"], Is.Not.EqualTo(sample1["buffer"]));

          EcmaValue sample2 = TA.Construct();
          That(map.Call(sample2, RuntimeFunction.Create(v => v))["buffer"], Is.Not.EqualTo(sample2["buffer"]));
        });
      });

      It("should preserve of bit-level encoding", () => {
        EcmaValue NaNs = ByteConversionValues.NaNs;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue source = TA.Construct(NaNs);
          EcmaValue target = map.Call(source, RuntimeFunction.Create((v, i) => NaNs[i]));
          EcmaValue sourceBytes = Global.Uint8Array.Construct(source["buffer"]);
          EcmaValue targetBytes = Global.Uint8Array.Construct(target["buffer"]);
          That(sourceBytes, Is.EquivalentTo(EcmaValueUtility.CreateListFromArrayLike(targetBytes)));
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should apply the returned values from callbackfn to the new instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 4));
          Case((sample, RuntimeFunction.Create(v => v * 3)), new[] { 3, 6, 12 });
        });
      });

      It("should convert to correct values", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(initial));
          EcmaValue result = map.Call(sample, RuntimeFunction.Create(() => value));
          That(result[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("should get constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = Intercept(Noop) }));

          EcmaValue result = map.Call(sample, RuntimeFunction.Create(() => 0));
          That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
          That(result["constructor"], Is.EqualTo(TA), "use defaultCtor on an undefined return - constructor check");
        });
      });

      It("should get inherited constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          using (TempProperty(TA.Prototype, "constructor", new EcmaPropertyDescriptor(Intercept(Noop), Undefined))) {
            EcmaValue result = map.Call(sample, Noop);
            That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

            That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
            That(result["constructor"], Is.Undefined, "used defaultCtor but still checks the inherited .constructor");
            That(Logs.Count, Is.EqualTo(2), "result.constructor triggers the inherited accessor property");
          }
        });
      });

      It("should return abrupt from SpeciesConstructor's get Constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, Noop), Throws.Test262);
        });
      });

      It("should throw if O.constructor returns a non-Object and non-undefined value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));

          sample["constructor"] = 42;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = "1";
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = Null;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = NaN;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = false;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"] = new Symbol();
          Case((sample, Noop), Throws.TypeError);
        });
      });

      It("should get @@species from found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, get: Intercept(() => Undefined), set: null));
          map.Call(sample, Noop);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from get @@species on found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();
          Object.Invoke("defineProperty", sample["constructor"], Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, Noop), Throws.Test262);
        });
      });

      It("should use custom species constructor if available", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => TA.Construct(count))));
          Case((sample, RuntimeFunction.Create(v => v + 7)), new[] { 47, 48, 49 });
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should call custom species constructor with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 42, 42));
          EcmaValue result = default;
          EcmaValue ctorThis = default;
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => {
            result = Arguments;
            ctorThis = This;
            return TA.Construct(count);
          })));
          map.Call(sample, RuntimeFunction.Create(v => v == 42));
          That(result, Is.EquivalentTo(new[] { 3 }));
          That(ctorThis, Is.InstanceOf(sample["constructor"][Symbol.Species]));
        });
      });

      It("may return a totally different TypedArray from custom species constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40));
          EcmaValue other = (TA == Global.Int8Array ? Global.Int16Array : Global.Int8Array).Construct(EcmaArray.Of(1, 0, 1));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(() => other)));

          EcmaValue result = map.Call(sample, RuntimeFunction.Create(v => v + 7));
          That(result, Is.EquivalentTo(new[] { 47, 0, 1 }));
          That(result, Is.EqualTo(other));
        });
      });

      It("does not throw a TypeError if new typedArray's length >= count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue customCount = default;
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct(customCount))));

          customCount = 2;
          That(map.Call(sample, RuntimeFunction.Create(() => 0))["length"], Is.EqualTo(customCount));
          customCount = 5;
          That(map.Call(sample, RuntimeFunction.Create(() => 0))["length"], Is.EqualTo(customCount));
        });
      });

      It("should throw a TypeError if new typedArray's length < count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct())));
          Case((sample, RuntimeFunction.Create(() => 0)), Throws.TypeError);
        });
      });

      It("should use defaultConstructor if @@species is either undefined or null", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          EcmaValue result = map.Call(sample, Noop);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));

          sample["constructor"].ToObject()[Symbol.Species] = Null;
          result = map.Call(sample, Noop);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));
        });
      });

      It("should throw if returned @@species is not a constructor, null or undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          sample["constructor"].ToObject()[Symbol.Species] = 0;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = "string";
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = Object.Construct();
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = NaN;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = false;
          Case((sample, Noop), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = true;
          Case((sample, Noop), Throws.TypeError);
        });
      });

      It("should throw if custom species constructor does not return a compatible object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, Array));
          Case((sample, Noop), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Reduce(RuntimeFunction reduce) {
      IsUnconstructableFunctionWLength(reduce, "reduce", 1);
      That(TypedArray.Prototype, Has.OwnProperty("reduce", reduce, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((a, v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
            return Undefined;
          }), 0), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            reduce.Call(sample, Intercept(() => Undefined), 0);
            That(Logs.Count, Is.EqualTo(2));
          }
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          reduce.Call(sample, RuntimeFunction.Create((a, v) => {
            results.Invoke("push", v);
          }), 0);
          That(results, Is.EquivalentTo(new[] { 7, 8 }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          reduce.Call(TA.Construct(), Intercept(Noop), Undefined);
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 1, 0));
          reduce.Call(TA.Construct(), Intercept(() => 42), 7);
          That(sample, Is.EquivalentTo(new[] { 0, 1, 0 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          reduce.Call(sample, RuntimeFunction.Create((acc, val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
          }), 0);
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should call callbackfn with correct arguments using default accumulator", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          reduce.Call(sample, RuntimeFunction.Create(accumulator => {
            results.Invoke("push", Arguments);
            return accumulator - 1;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 43, 1, sample },
            new[] { 41, 44, 2, sample }
          }));
        });
      });

      It("should call callbackfn with correct arguments using custom accumulator", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          reduce.Call(sample, RuntimeFunction.Create(accumulator => {
            results.Invoke("push", Arguments);
            return accumulator + 1;
          }), 7);
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 42, 0, sample },
            new[] { 8, 43, 1, sample },
            new[] { 9, 44, 2, sample }
          }));
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          reduce.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), 0);
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));
        });
      });

      It("should return given initialValue on empty instances without calling callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => Undefined), 42), 42);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return [0] without calling callbackfn if length is 1 and initialValue is not present", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(42)), Intercept(() => Undefined)), 42);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return last accumulator value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample, Intercept(() => Logs.Count == 2 ? 42 : default)), 42, "using default accumulator");

          Logs.Clear();
          Case((sample, Intercept(() => Logs.Count == 3 ? 7 : default), 0), 7, "using custom accumulator");
        });
      });

      It("should return result of any type without any number conversions", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue symbol = new Symbol();
          EcmaValue obj = Object.Construct();
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));

          Case((sample, RuntimeFunction.Create(() => "test262")), "test262");
          Case((sample, RuntimeFunction.Create(() => "")), "");
          Case((sample, RuntimeFunction.Create(() => Undefined)), Undefined);
          Case((sample, RuntimeFunction.Create(() => Null)), Null);
          Case((sample, RuntimeFunction.Create(() => -0d)), -0d);
          Case((sample, RuntimeFunction.Create(() => 42)), 42);
          Case((sample, RuntimeFunction.Create(() => NaN)), NaN);
          Case((sample, RuntimeFunction.Create(() => Infinity)), Infinity);
          Case((sample, RuntimeFunction.Create(() => 0.6)), 0.6);
          Case((sample, RuntimeFunction.Create(() => true)), true);
          Case((sample, RuntimeFunction.Create(() => false)), false);
          Case((sample, RuntimeFunction.Create(() => symbol)), symbol);
          Case((sample, RuntimeFunction.Create(() => obj)), obj);

          Case((sample, RuntimeFunction.Create(() => "test262"), 0), "test262");
          Case((sample, RuntimeFunction.Create(() => ""), 0), "");
          Case((sample, RuntimeFunction.Create(() => Undefined), 0), Undefined);
          Case((sample, RuntimeFunction.Create(() => Null), 0), Null);
          Case((sample, RuntimeFunction.Create(() => -0d), 0), -0d);
          Case((sample, RuntimeFunction.Create(() => 42), 0), 42);
          Case((sample, RuntimeFunction.Create(() => NaN), 0), NaN);
          Case((sample, RuntimeFunction.Create(() => Infinity), 0), Infinity);
          Case((sample, RuntimeFunction.Create(() => 0.6), 0), 0.6);
          Case((sample, RuntimeFunction.Create(() => true), 0), true);
          Case((sample, RuntimeFunction.Create(() => false), 0), false);
          Case((sample, RuntimeFunction.Create(() => symbol), 0), symbol);
          Case((sample, RuntimeFunction.Create(() => obj), 0), obj);
        });
      });

      It("should throw a TypeError exception if len is 0 and initialValue is not present", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => Undefined)), Throws.TypeError);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, ThrowTest262Exception), Throws.Test262);
          Case((sample, ThrowTest262Exception, 0), Throws.Test262);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ReduceRight(RuntimeFunction reduceRight) {
      IsUnconstructableFunctionWLength(reduceRight, "reduceRight", 1);
      That(TypedArray.Prototype, Has.OwnProperty("reduceRight", reduceRight, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((a, v, i) => {
            if (i == 0) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
            return Undefined;
          }), 0), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            reduceRight.Call(sample, Intercept(() => Undefined), 0);
            That(Logs.Count, Is.EqualTo(2));
          }
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          reduceRight.Call(sample, RuntimeFunction.Create((a, v) => {
            results.Invoke("push", v);
          }), 0);
          That(results, Is.EquivalentTo(new[] { 8, 7 }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          reduceRight.Call(TA.Construct(), Intercept(Noop), Undefined);
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 1, 0));
          reduceRight.Call(TA.Construct(), Intercept(() => 42), 7);
          That(sample, Is.EquivalentTo(new[] { 0, 1, 0 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          reduceRight.Call(sample, RuntimeFunction.Create((acc, val, i) => {
            if (i < sample["length"] - 1) {
              That(sample[i + 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 2, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
          }), 0);
          That(sample, Is.EquivalentTo(new[] { 2, 1, 7 }));
        });
      });

      It("should call callbackfn with correct arguments using default accumulator", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          reduceRight.Call(sample, RuntimeFunction.Create(accumulator => {
            results.Invoke("push", Arguments);
            return accumulator + 1;
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 44, 43, 1, sample },
            new[] { 45, 42, 0, sample }
          }));
        });
      });

      It("should call callbackfn with correct arguments using custom accumulator", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          reduceRight.Call(sample, RuntimeFunction.Create(accumulator => {
            results.Invoke("push", Arguments);
            return accumulator + 1;
          }), 7);
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 44, 2, sample },
            new[] { 8, 43, 1, sample },
            new[] { 9, 42, 0, sample }
          }));
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          reduceRight.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), 0);
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));
        });
      });

      It("should return given initialValue on empty instances without calling callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => Undefined), 42), 42);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return [0] without calling callbackfn if length is 1 and initialValue is not present", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(EcmaArray.Of(42)), Intercept(() => Undefined)), 42);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return last accumulator value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample, Intercept(() => Logs.Count == 2 ? 42 : default)), 42, "using default accumulator");

          Logs.Clear();
          Case((sample, Intercept(() => Logs.Count == 3 ? 7 : default), 0), 7, "using custom accumulator");
        });
      });

      It("should return result of any type without any number conversions", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue symbol = new Symbol();
          EcmaValue obj = Object.Construct();
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));

          Case((sample, RuntimeFunction.Create(() => "test262")), "test262");
          Case((sample, RuntimeFunction.Create(() => "")), "");
          Case((sample, RuntimeFunction.Create(() => Undefined)), Undefined);
          Case((sample, RuntimeFunction.Create(() => Null)), Null);
          Case((sample, RuntimeFunction.Create(() => -0d)), -0d);
          Case((sample, RuntimeFunction.Create(() => 42)), 42);
          Case((sample, RuntimeFunction.Create(() => NaN)), NaN);
          Case((sample, RuntimeFunction.Create(() => Infinity)), Infinity);
          Case((sample, RuntimeFunction.Create(() => 0.6)), 0.6);
          Case((sample, RuntimeFunction.Create(() => true)), true);
          Case((sample, RuntimeFunction.Create(() => false)), false);
          Case((sample, RuntimeFunction.Create(() => symbol)), symbol);
          Case((sample, RuntimeFunction.Create(() => obj)), obj);

          Case((sample, RuntimeFunction.Create(() => "test262"), 0), "test262");
          Case((sample, RuntimeFunction.Create(() => ""), 0), "");
          Case((sample, RuntimeFunction.Create(() => Undefined), 0), Undefined);
          Case((sample, RuntimeFunction.Create(() => Null), 0), Null);
          Case((sample, RuntimeFunction.Create(() => -0d), 0), -0d);
          Case((sample, RuntimeFunction.Create(() => 42), 0), 42);
          Case((sample, RuntimeFunction.Create(() => NaN), 0), NaN);
          Case((sample, RuntimeFunction.Create(() => Infinity), 0), Infinity);
          Case((sample, RuntimeFunction.Create(() => 0.6), 0), 0.6);
          Case((sample, RuntimeFunction.Create(() => true), 0), true);
          Case((sample, RuntimeFunction.Create(() => false), 0), false);
          Case((sample, RuntimeFunction.Create(() => symbol), 0), symbol);
          Case((sample, RuntimeFunction.Create(() => obj), 0), obj);
        });
      });

      It("should throw a TypeError exception if len is 0 and initialValue is not present", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(), Intercept(() => Undefined)), Throws.TypeError);
          That(Logs.Count, Is.Zero);
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, ThrowTest262Exception), Throws.Test262);
          Case((sample, ThrowTest262Exception, 0), Throws.Test262);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Reverse(RuntimeFunction reverse) {
      IsUnconstructableFunctionWLength(reverse, "reverse", 0);
      That(TypedArray.Prototype, Has.OwnProperty("reverse", reverse, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case(sample, Throws.Nothing);
          }
        });
      });

      It("should preserve non numeric properties", () => {
        var s = new Symbol("1");
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["foo"] = 42;
          sample["bar"] = "bar";
          sample[s] = 1;
          reverse.Call(sample);
          That(sample["foo"], Is.EqualTo(42), "sample.foo === 42");
          That(sample["bar"], Is.EqualTo("bar"), "sample.bar === 'bar'");
          That(sample[s], Is.EqualTo(1), "sample[s] === 1");
        });
      });

      It("should return `this`", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample1 = TA.Construct();
          Case(sample1, sample1);
          EcmaValue sample2 = TA.Construct(42);
          Case(sample2, sample2);
        });
      });

      It("should revert values", () => {
        EcmaValue buffer = ArrayBuffer.Construct(64);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(buffer, 0, 4);
          EcmaValue other = TA.Construct(buffer, 0, 5);
          sample[0] = 42;
          sample[1] = 43;
          sample[2] = 2;
          sample[3] = 1;
          other[4] = 7;
          reverse.Call(sample);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 43, 42 }));
          That(other, Is.EquivalentTo(new[] { 1, 2, 43, 42, 7 }));

          sample[0] = 7;
          sample[1] = 17;
          sample[2] = 1;
          sample[3] = 0;
          other[4] = 42;
          reverse.Call(other);
          That(other, Is.EquivalentTo(new[] { 42, 0, 1, 17, 7 }));
          That(sample, Is.EquivalentTo(new[] { 42, 0, 1, 17 }));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Set(RuntimeFunction set) {
      IsUnconstructableFunctionWLength(set, "set", 1);
      That(TypedArray.Prototype, Has.OwnProperty("set", set, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((42, EcmaArray.Of()), Throws.TypeError);
        Case(("1", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol("s"), EcmaArray.Of()), Throws.TypeError);

        Case((Undefined, Global.Int8Array.Construct()), Throws.TypeError);
        Case((Null, Global.Int8Array.Construct()), Throws.TypeError);
        Case((42, Global.Int8Array.Construct()), Throws.TypeError);
        Case(("1", Global.Int8Array.Construct()), Throws.TypeError);
        Case((true, Global.Int8Array.Construct()), Throws.TypeError);
        Case((new Symbol("s"), Global.Int8Array.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, EcmaArray.Of()), Throws.TypeError);
        Case((Object.Construct(), EcmaArray.Of()), Throws.TypeError);
        Case((EcmaArray.Of(), EcmaArray.Of()), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), EcmaArray.Of()), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), EcmaArray.Of()), Throws.TypeError);

        Case((TypedArray.Prototype, Global.Int8Array.Construct()), Throws.TypeError);
        Case((Object.Construct(), Global.Int8Array.Construct()), Throws.TypeError);
        Case((EcmaArray.Of(), Global.Int8Array.Construct()), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Global.Int8Array.Construct()), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Global.Int8Array.Construct()), Throws.TypeError);
      });

      It("should throw a RangeError exception if targetOffset < 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(4);
          Case((sample, EcmaArray.Of(1), -1), Throws.RangeError);
          Case((sample, EcmaArray.Of(1), -1.00001), Throws.RangeError);
          Case((sample, EcmaArray.Of(1), -Infinity), Throws.RangeError);
          Case((sample, sample, -1), Throws.RangeError);
          Case((sample, sample, -1.00001), Throws.RangeError);
          Case((sample, sample, -Infinity), Throws.RangeError);
        });
      });

      It("should perform ToInteger(offset) operations", () => {
        EcmaValue sample;
        TestWithTypedArrayConstructors(TA => {
          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), "");
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "the empty string");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), "0");
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "'0'");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), false);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "false");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), 0.1);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "0.1");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), 0.9);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "0.9");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), -0.5);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "-0.5");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), 1.1);
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "1.1");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), NaN);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "NaN");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), Null);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "null");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), Undefined);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), Object.Construct());
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "{}");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), EcmaArray.Of());
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "[]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), EcmaArray.Of(0));
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "[0]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), true);
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "true");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), "1");
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "'1'");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), EcmaArray.Of(1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "[1]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), CreateObject(valueOf: () => 1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "valueOf");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, EcmaArray.Of(42), CreateObject(toString: () => 1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "toString");

          EcmaValue src = TA.Construct(EcmaArray.Of(42));
          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, "");
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "the empty string");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, "0");
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "'0'");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, false);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "false");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, 0.1);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "0.1");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, 0.9);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "0.9");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, -0.5);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "-0.5");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, 1.1);
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "1.1");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, NaN);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "NaN");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, Null);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "null");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, Undefined);
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, Object.Construct());
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "{}");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, EcmaArray.Of());
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "[]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, EcmaArray.Of(0));
          That(sample, Is.EquivalentTo(new[] { 42, 2 }), "[0]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, true);
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "true");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, "1");
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "'1'");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, EcmaArray.Of(1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "[1]");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, CreateObject(valueOf: () => 1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "valueOf");

          sample = TA.Construct(EcmaArray.Of(1, 2));
          set.Call(sample, src, CreateObject(toString: () => 1));
          That(sample, Is.EquivalentTo(new[] { 1, 42 }), "toString");
        });
      });

      It("should return abrupt getting src length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample, CreateObject(("length", get: ThrowTest262Exception, set: null))), Throws.Test262);
        });
      });

      It("should return abrupt from ToLength(src length)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample, CreateObject(new { length = CreateObject(valueOf: ThrowTest262Exception) })), Throws.Test262);
          Case((sample, CreateObject(new { length = CreateObject(toString: ThrowTest262Exception) })), Throws.Test262);
        });
      });

      It("should return abrupt from getting src property value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          EcmaValue obj = CreateObject(("length", 4), (0, 42), (1, 43), (3, 44));
          Object.Invoke("defineProperty", obj, "2", CreateObject(new { get = ThrowTest262Exception }));

          Case((sample, obj), Throws.Test262);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }), "values are set until exception");
        });
      });

      It("should return abrupt from ToNumber(src property value)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          EcmaValue obj = CreateObject(("length", 4), (0, 42), (1, 43), (3, 44));

          obj[2] = CreateObject(valueOf: ThrowTest262Exception);
          Case((sample, obj), Throws.Test262);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }), "values are set until exception");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          obj[2] = new Symbol("1");
          Case((sample, obj), Throws.TypeError);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }), "values are set until exception");
        });
      });

      It("should return abrupt from ToInteger(offset)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, EcmaArray.Of(1), new Symbol("1")), Throws.TypeError);
          Case((sample, EcmaArray.Of(1), CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((sample, EcmaArray.Of(1), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((sample, sample, new Symbol("1")), Throws.TypeError);
          Case((sample, sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((sample, sample, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
        });
      });

      It("should return abrupt from ToObject(array)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
        });
      });

      It("should set values to target and return undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue src = EcmaArray.Of(42, 43);
          EcmaValue srcObj = CreateObject(("length", 2), (0, 7), (1, 17));
          EcmaValue sample, result;

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, src, 0);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }), "offset: 0, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, src, 1);
          That(sample, Is.EquivalentTo(new[] { 1, 42, 43, 4 }), "offset: 1, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, src, 2);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 42, 43 }), "offset: 2, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, srcObj, 0);
          That(sample, Is.EquivalentTo(new[] { 7, 17, 3, 4 }), "offset: 0, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, srcObj, 1);
          That(sample, Is.EquivalentTo(new[] { 1, 7, 17, 4 }), "offset: 1, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          result = set.Call(sample, srcObj, 2);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 7, 17 }), "offset: 2, result: " + sample.ToString());
          That(result, Is.Undefined, "returns undefined");
        });
      });

      It("should get and set each value in order", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(5);
          EcmaValue obj = CreateObject(new { length = 3 });
          Object.Invoke("defineProperty", obj, 0, CreateObject(new { get = Intercept(() => Return(Logs.Add(sample.Invoke("join")), 42), "0") }));
          Object.Invoke("defineProperty", obj, 1, CreateObject(new { get = Intercept(() => Return(Logs.Add(sample.Invoke("join")), 43), "1") }));
          Object.Invoke("defineProperty", obj, 2, CreateObject(new { get = Intercept(() => Return(Logs.Add(sample.Invoke("join")), 44), "2") }));
          Object.Invoke("defineProperty", obj, 3, CreateObject(new { get = ThrowTest262Exception }));
          set.Call(sample, obj, 1);

          That(sample, Is.EquivalentTo(new[] { 0, 42, 43, 44, 0 }));
          That(Logs, Is.EquivalentTo(new[] { "0", "0,0,0,0,0", "1", "0,42,0,0,0", "2", "0,42,43,0,0" }));
        });
      });

      It("should preserve of bit-level encoding", () => {
        EcmaValue NaNs = ByteConversionValues.NaNs;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue source = TA.Construct(NaNs);
          EcmaValue target = TA.Construct(NaNs["length"]);
          set.Call(target, source);

          EcmaValue sourceBytes = Global.Uint8Array.Construct(source["buffer"]);
          EcmaValue targetBytes = Global.Uint8Array.Construct(target["buffer"]);
          That(sourceBytes, Is.EquivalentTo(EcmaValueUtility.CreateListFromArrayLike(targetBytes)));
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should perform value conversions on ToNumber(src property value)", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(initial));
          set.Call(sample, EcmaArray.Of(value));
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("should perform type conversions on ToNumber(src property value)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue obj1 = CreateObject(valueOf: () => 42);
          EcmaValue obj2 = CreateObject(toString: () => "42");
          EcmaValue arr = EcmaArray.Of("1", "", false, true, Null, obj1, obj2, EcmaArray.Of(), EcmaArray.Of(1));

          EcmaValue sample = TA.Construct(arr["length"]);
          set.Call(sample, arr);
          That(sample, Is.EquivalentTo(new[] { 1, 0, 0, 1, 0, 42, 42, 0, 1 }));
        });
      });

      It("should not cache values from src array", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(5);
          EcmaValue obj = CreateObject(("length", 5), (1, 7), (2, 7), (3, 7), (4, 7));
          Object.Invoke("defineProperty", obj, 0, CreateObject(new {
            get = RuntimeFunction.Create(() => {
              obj[1] = 43;
              obj[2] = 44;
              obj[3] = 45;
              obj[4] = 46;
              return 42;
            })
          }));
          set.Call(sample, obj);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 44, 45, 46 }));
        });
      });

      It("should use target's internal [[ArrayLength]]", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(2);
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, EcmaArray.Of(42, 43)), Undefined);
            Case((sample, TA.Construct(EcmaArray.Of(42, 43))), Undefined);
          }
        });
      });

      It("should use target's internal [[ByteOffset]]", () => {
        using (TempProperty(TypedArray.Prototype, "byteOffset", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
          TestWithTypedArrayConstructors(TA => {
            using (TempProperty(TA.Prototype, "byteOffset", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
              EcmaValue sample = TA.Construct(2);
              EcmaValue diffTA = TA == Global.Uint8Array ? Global.Int8Array : Global.Uint8Array;
              Object.Invoke("defineProperty", sample, "byteOffset", CreateObject(new { get = ThrowTest262Exception }));
              Case((sample, TA.Construct(EcmaArray.Of(42, 43))), Undefined);
              Case((sample, diffTA.Construct(EcmaArray.Of(42, 43))), Undefined);
              Case((sample, diffTA.Construct(sample["buffer"], 0, 2)), Undefined);
            }
          });
        }
      });

      It("should use source's internal [[ArrayLength]]", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(2);
            EcmaValue source = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", source, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, source), Undefined);
          }
        });
      });

      It("should use source's internal [[ByteOffset]]", () => {
        using (TempProperty(TypedArray.Prototype, "byteOffset", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
          TestWithTypedArrayConstructors(TA => {
            using (TempProperty(TA.Prototype, "byteOffset", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
              EcmaValue sample = TA.Construct(2);
              EcmaValue diffTA = TA == Global.Uint8Array ? Global.Int8Array : Global.Uint8Array;
              EcmaValue src1 = TA.Construct(EcmaArray.Of(42, 43));
              EcmaValue src2 = diffTA.Construct(EcmaArray.Of(42, 43));
              EcmaValue src3 = diffTA.Construct(sample["buffer"], 0, 2);
              Object.Invoke("defineProperty", src1, "byteOffset", CreateObject(new { get = ThrowTest262Exception }));
              Object.Invoke("defineProperty", src2, "byteOffset", CreateObject(new { get = ThrowTest262Exception }));
              Object.Invoke("defineProperty", src3, "byteOffset", CreateObject(new { get = ThrowTest262Exception }));
              Case((sample, src1), Undefined);
              Case((sample, src2), Undefined);
              Case((sample, src3), Undefined);
            }
          });
        }
      });

      It("should throw an error if buffer is detached before setting a value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3));
          EcmaValue obj = CreateObject(("length", 3), (0, 42));
          Object.Invoke("defineProperty", obj, 1, CreateObject(new { get = RuntimeFunction.Create(() => DetachBuffer(sample)) }));
          Object.Invoke("defineProperty", obj, 2, CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, obj), Throws.TypeError);
        });
      });

      It("should throw a TypeError if targetBuffer is detached on ToInteger(offset)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue obj = CreateObject(valueOf: Intercept(() => DetachBuffer(sample)));
          Case((sample, EcmaArray.Of(1), obj), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should throw a TypeError if targetBuffer is detached on ToInteger(offset) with TypedArray as first argument", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue obj = CreateObject(valueOf: Intercept(() => DetachBuffer(sample)));
          Case((sample, TA.Construct(1), obj), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should throw a TypeError if srcBuffer is detached on ToInteger(offset)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue target = TA.Construct();
          EcmaValue obj = CreateObject(valueOf: Intercept(() => DetachBuffer(target)));
          Case((sample, target, obj), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should throw a TypeError if targetBuffer is detached", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          DetachBuffer(sample);
          Case((sample, EcmaArray.Of(1)), Throws.TypeError);
          Case((sample, CreateObject(("length", get: ThrowTest262Exception, set: null))), Throws.TypeError);
        });
      });

      It("should throw a RangeError exception if srcLength + targetOffset > targetLength", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA.Construct(2), TA.Construct(2), 1), Throws.RangeError, "2 + 1 > 2");
          Case((TA.Construct(1), TA.Construct(2), 0), Throws.RangeError, "2 + 0 > 1");
          Case((TA.Construct(1), TA.Construct(0), 2), Throws.RangeError, "0 + 2 > 1");
          Case((TA.Construct(2), TA.Construct(2), Infinity), Throws.RangeError, "2 + Infinity > 2");
        });
      });

      It("should set values from different instances using the same buffer and same constructor, srcBuffer values are cached", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample, src;

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          src = TA.Construct(sample["buffer"], 0, 2);
          set.Call(sample, src, 0);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          src = TA.Construct(sample["buffer"], 0, 2);
          set.Call(sample, src, 1);
          That(sample, Is.EquivalentTo(new[] { 1, 1, 2, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          src = TA.Construct(sample["buffer"], 0, 2);
          set.Call(sample, src, 2);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 1, 2 }));
        });
      });

      It("should set values from different instances using the same buffer and different constructor", () => {
        EcmaValue expected = CreateObject(new {
          Float64Array = EcmaArray.Of(1.0000002464512363, 42, 1.875, 4, 5, 6, 7, 8),
          Float32Array = EcmaArray.Of(0, 42, 512.0001220703125, 4, 5, 6, 7, 8),
          Int32Array = EcmaArray.Of(1109917696, 42, 0, 4, 5, 6, 7, 8),
          Int16Array = EcmaArray.Of(0, 42, 0, 4, 5, 6, 7, 8),
          Int8Array = EcmaArray.Of(0, 42, 0, 66, 5, 6, 7, 8),
          Uint32Array = EcmaArray.Of(1109917696, 42, 0, 4, 5, 6, 7, 8),
          Uint16Array = EcmaArray.Of(0, 42, 0, 4, 5, 6, 7, 8),
          Uint8Array = EcmaArray.Of(0, 42, 0, 66, 5, 6, 7, 8),
          Uint8ClampedArray = EcmaArray.Of(0, 42, 0, 66, 5, 6, 7, 8)
        });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = TA == Global.Float32Array ? Global.Float64Array : Global.Float32Array;
          EcmaValue sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4, 5, 6, 7, 8));
          EcmaValue src = other.Construct(sample["buffer"], 0, 2);

          // Reflect changes on sample object
          src[0] = 42;
          set.Call(sample, src, 1);
          That(sample, Is.EquivalentTo(EcmaValueUtility.CreateListFromArrayLike(expected[TA["name"]])));
        });
      });

      It("should set values from different instances using the different buffer and same constructor, srcBuffer values are cached", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample, src;
          src = TA.Construct(EcmaArray.Of(42, 43));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 1);
          That(sample, Is.EquivalentTo(new[] { 1, 42, 43, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 0);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 2);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 42, 43 }));
        });
      });

      It("should set values from different instances using the different buffer and different type", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = TA == Global.Float32Array ? Global.Float64Array : Global.Float32Array;
          EcmaValue sample, src;
          src = other.Construct(EcmaArray.Of(42, 43));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 0);
          That(sample, Is.EquivalentTo(new[] { 42, 43, 3, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 1);
          That(sample, Is.EquivalentTo(new[] { 1, 42, 43, 4 }));

          sample = TA.Construct(EcmaArray.Of(1, 2, 3, 4));
          set.Call(sample, src, 2);
          That(sample, Is.EquivalentTo(new[] { 1, 2, 42, 43 }));
        });
      });

      It("should set converted values from different buffer and different type instances", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0));
          set.Call(sample, Global.Float64Array.Construct(EcmaArray.Of(value)));
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Slice(RuntimeFunction slice) {
      IsUnconstructableFunctionWLength(slice, "slice", 2);
      That(TypedArray.Prototype, Has.OwnProperty("slice", slice, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 0, 0), Throws.TypeError);
        Case((Null, 0, 0), Throws.TypeError);
        Case((42, 0, 0), Throws.TypeError);
        Case(("1", 0, 0), Throws.TypeError);
        Case((true, 0, 0), Throws.TypeError);
        Case((new Symbol("s"), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 0, 0), Throws.TypeError);
        Case((Object.Construct(), 0, 0), Throws.TypeError);
        Case((EcmaArray.Of(), 0, 0), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 0, 0), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, obj, obj), Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result;
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            result = slice.Call(sample);
          }
          That(result, Is.EquivalentTo(new[] { 42, 43 }));
        });
      });

      It("should preserve of bit-level encoding", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue source = TA.Construct(ByteConversionValues.NaNs);
          EcmaValue target = slice.Call(source);
          EcmaValue sourceBytes = Global.Uint8Array.Construct(source["buffer"]);
          EcmaValue targetBytes = Global.Uint8Array.Construct(target["buffer"]);
          That(sourceBytes, Is.EquivalentTo(EcmaValueUtility.CreateListFromArrayLike(targetBytes)));
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should handle Infinity values on start and end", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, -Infinity), new[] { 40, 41, 42, 43 });
          Case((sample, Infinity), new EcmaValue[0]);
          Case((sample, 0, -Infinity), new EcmaValue[0]);
          Case((sample, 0, Infinity), new[] { 40, 41, 42, 43 });
        });
      });

      It("should handle -0 values on start and end", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, -0d), new[] { 40, 41, 42, 43 });
          Case((sample, -0d, 4), new[] { 40, 41, 42, 43 });
          Case((sample, 0, -0d), new EcmaValue[0]);
          Case((sample, -0d, -0d), new EcmaValue[0]);
        });
      });

      It("should throw a TypeError buffer is detached on Get constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = RuntimeFunction.Create(() => DetachBuffer(sample)) }));
          Case(sample, Throws.TypeError);
        });
      });

      It("should throw a TypeError buffer is detached on Get custom constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create((count) => {
            DetachBuffer(sample);
            return TA.Construct(count);
          })));
          Case(sample, Throws.TypeError);
        });
      });

      It("should throw a TypeError buffer is detached on Get custom constructor using other targetType", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create((count) => {
            EcmaValue other = TA == Global.Int8Array ? Global.Int16Array : Global.Int8Array;
            DetachBuffer(sample);
            return other.Construct(count);
          })));
          Case(sample, Throws.TypeError);
        });
      });

      It("should throw if custom species constructor returns an instance with a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create((count) => {
            EcmaValue other = TA.Construct(count);
            DetachBuffer(other);
            return other;
          })));
          Case(sample, Throws.TypeError);
        });
      });

      It("does not throw a TypeError if buffer is detached on custom constructor and `k >= final` using same targetType", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = default, result;
          EcmaValue ctor = CreateObject((Symbol.Species, RuntimeFunction.Create((count) => {
            DetachBuffer(sample);
            return TA.Construct(count);
          })));
          sample = TA.Construct(0);
          sample["constructor"] = ctor;
          result = slice.Call(sample);
          That(result["length"], Is.Zero);
          That(result["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(result["constructor"], Is.EqualTo(TA));

          sample = TA.Construct(4);
          sample["constructor"] = ctor;
          result = slice.Call(sample, 1, 1);
          That(result["length"], Is.Zero);
          That(result["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(result["constructor"], Is.EqualTo(TA));
        });
      });

      It("does not throw a TypeError if buffer is detached on custom constructor and `k >= final` using other targetType", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = default, other = default, result;
          EcmaValue ctor = CreateObject((Symbol.Species, RuntimeFunction.Create((count) => {
            other = TA == Global.Int8Array ? Global.Int16Array : Global.Int8Array;
            DetachBuffer(sample);
            return other.Construct(count);
          })));
          sample = TA.Construct(0);
          sample["constructor"] = ctor;
          result = slice.Call(sample);
          That(result["length"], Is.Zero);
          That(result["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(result["constructor"], Is.EqualTo(other));

          sample = TA.Construct(4);
          sample["constructor"] = ctor;
          result = slice.Call(sample, 1, 1);
          That(result["length"], Is.Zero);
          That(result["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(result["constructor"], Is.EqualTo(other));
        });
      });

      It("should not import own properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bar = new Symbol("1");
          EcmaValue sample = TA.Construct(EcmaArray.Of(41, 42, 43, 44));
          sample["foo"] = 42;
          sample[bar] = 1;

          EcmaValue result = slice.Call(sample);
          That(result.Invoke("hasOwnProperty", "foo"), Is.False);
          That(result.Invoke("hasOwnProperty", bar), Is.False);
        });
      });

      It("may return a new instance with a smaller length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 1), new[] { 41, 42, 43 }, "begin == 1");
          Case((sample, 2), new[] { 42, 43 }, "begin == 2");
          Case((sample, 3), new[] { 43 }, "begin == 3");

          Case((sample, 1, 4), new[] { 41, 42, 43 }, "begin == 1, end == length");
          Case((sample, 2, 4), new[] { 42, 43 }, "begin == 2, end == length");
          Case((sample, 3, 4), new[] { 43 }, "begin == 3, end == length");

          Case((sample, 0, 1), new[] { 40 }, "begin == 0, end == 1");
          Case((sample, 0, 2), new[] { 40, 41 }, "begin == 0, end == 2");
          Case((sample, 0, 3), new[] { 40, 41, 42 }, "begin == 0, end == 3");

          Case((sample, -1), new[] { 43 }, "begin == -1");
          Case((sample, -2), new[] { 42, 43 }, "begin == -2");
          Case((sample, -3), new[] { 41, 42, 43 }, "begin == -3");

          Case((sample, -1, 4), new[] { 43 }, "begin == -1, end == length");
          Case((sample, -2, 4), new[] { 42, 43 }, "begin == -2, end == length");
          Case((sample, -3, 4), new[] { 41, 42, 43 }, "begin == -3, end == length");

          Case((sample, 0, -1), new[] { 40, 41, 42 }, "begin == 0, end == -1");
          Case((sample, 0, -2), new[] { 40, 41 }, "begin == 0, end == -2");
          Case((sample, 0, -3), new[] { 40 }, "begin == 0, end == -3");

          Case((sample, -0, -1), new[] { 40, 41, 42 }, "begin == -0, end == -1");
          Case((sample, -0, -2), new[] { 40, 41 }, "begin == -0, end == -2");
          Case((sample, -0, -3), new[] { 40 }, "begin == -0, end == -3");

          Case((sample, -2, -1), new[] { 42 }, "length == 4, begin == -2, end == -1");
          Case((sample, 1, -1), new[] { 41, 42 }, "length == 4, begin == 1, end == -1");
          Case((sample, 1, -2), new[] { 41 }, "length == 4, begin == 1, end == -2");
          Case((sample, 2, -1), new[] { 42 }, "length == 4, begin == 2, end == -1");

          Case((sample, -1, 5), new[] { 43 }, "begin == -1, end > length");
          Case((sample, -2, 4), new[] { 42, 43 }, "begin == -2, end > length");
          Case((sample, -3, 4), new[] { 41, 42, 43 }, "begin == -3, end > length");
        });
      });

      It("may return a new empty instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 4), new EcmaValue[0], "begin == length");
          Case((sample, 5), new EcmaValue[0], "begin > length");

          Case((sample, 4, 4), new EcmaValue[0], "begin == length, end == length");
          Case((sample, 5, 4), new EcmaValue[0], "begin > length, end == length");

          Case((sample, 4, 5), new EcmaValue[0], "begin == length, end > length");
          Case((sample, 5, 5), new EcmaValue[0], "begin > length, end > length");

          Case((sample, 0, 0), new EcmaValue[0], "begin == 0, end == 0");
          Case((sample, -0, -0), new EcmaValue[0], "begin == -0, end == -0");
          Case((sample, 1, 0), new EcmaValue[0], "begin > 0, end == 0");
          Case((sample, -1, 0), new EcmaValue[0], "being < 0, end == 0");

          Case((sample, 2, 1), new EcmaValue[0], "begin > 0, begin < length, begin > end, end > 0");
          Case((sample, 2, 2), new EcmaValue[0], "begin > 0, begin < length, begin == end");

          Case((sample, 2, -2), new EcmaValue[0], "begin > 0, begin < length, end == -2");

          Case((sample, -1, -1), new EcmaValue[0], "length = 4, begin == -1, end == -1");
          Case((sample, -1, -2), new EcmaValue[0], "length = 4, begin == -1, end == -2");
          Case((sample, -2, -2), new EcmaValue[0], "length = 4, begin == -2, end == -2");

          Case((sample, 0, -4), new EcmaValue[0], "begin == 0, end == -length");
          Case((sample, -4, -4), new EcmaValue[0], "begin == -length, end == -length");
          Case((sample, -5, -4), new EcmaValue[0], "begin < -length, end == -length");

          Case((sample, 0, -5), new EcmaValue[0], "begin == 0, end < -length");
          Case((sample, -4, -5), new EcmaValue[0], "begin == -length, end < -length");
          Case((sample, -5, -5), new EcmaValue[0], "begin < -length, end < -length");
        });
      });

      It("may return a new instance with the same length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 0), new[] { 40, 41, 42, 43 }, "begin == 0");
          Case((sample, -4), new[] { 40, 41, 42, 43 }, "begin == -srcLength");
          Case((sample, -5), new[] { 40, 41, 42, 43 }, "begin < -srcLength");

          Case((sample, 0, 4), new[] { 40, 41, 42, 43 }, "begin == 0, end == srcLength");
          Case((sample, -4, 4), new[] { 40, 41, 42, 43 }, "begin == -srcLength, end == srcLength");
          Case((sample, -5, 4), new[] { 40, 41, 42, 43 }, "begin < -srcLength, end == srcLength");

          Case((sample, 0, 5), new[] { 40, 41, 42, 43 }, "begin == 0, end > srcLength");
          Case((sample, -4, 5), new[] { 40, 41, 42, 43 }, "begin == -srcLength, end > srcLength");
          Case((sample, -5, 5), new[] { 40, 41, 42, 43 }, "begin < -srcLength, end > srcLength");
        });
      });

      It("should perform ToInteger(begin)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, false), new[] { 40, 41, 42, 43 }, "false");
          Case((sample, true), new[] { 41, 42, 43 }, "true");

          Case((sample, NaN), new[] { 40, 41, 42, 43 }, "NaN");
          Case((sample, Null), new[] { 40, 41, 42, 43 }, "null");
          Case((sample, Undefined), new[] { 40, 41, 42, 43 }, "undefined");

          Case((sample, 1.1), new[] { 41, 42, 43 }, "1.1");
          Case((sample, 1.5), new[] { 41, 42, 43 }, "1.5");
          Case((sample, 0.6), new[] { 40, 41, 42, 43 }, "0.6");

          Case((sample, -1.5), new[] { 43 }, "-1.5");
          Case((sample, -1.1), new[] { 43 }, "-1.1");
          Case((sample, -0.6), new[] { 40, 41, 42, 43 }, "-0.6");

          Case((sample, "3"), new[] { 43 }, "string");
          Case((sample, CreateObject(valueOf: () => 2)), new[] { 42, 43 });
        });
      });

      It("should perform ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 0, false), new EcmaValue[] { }, "false");
          Case((sample, 0, true), new[] { 40 }, "true");

          Case((sample, 0, NaN), new EcmaValue[] { }, "NaN");
          Case((sample, 0, Null), new EcmaValue[] { }, "null");
          Case((sample, 0, Undefined), new[] { 40, 41, 42, 43 }, "undefined");

          Case((sample, 0, 0.6), new EcmaValue[] { }, "0.6");
          Case((sample, 0, 1.1), new[] { 40 }, "1.1");
          Case((sample, 0, 1.5), new[] { 40 }, "1.5");
          Case((sample, 0, -0.6), new EcmaValue[] { }, "-0.6");
          Case((sample, 0, -1.1), new[] { 40, 41, 42 }, "-1.1");
          Case((sample, 0, -1.5), new[] { 40, 41, 42 }, "-1.5");

          Case((sample, 0, "3"), new[] { 40, 41, 42 }, "string");
          Case((sample, 0, CreateObject(valueOf: () => 2)), new[] { 40, 41 });
        });
      });

      It("should return abrupt from ToInteger(begin)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, new Symbol()), Throws.TypeError);
          Case((sample, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        });
      });

      It("should return abrupt from ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, 0, new Symbol()), Throws.TypeError);
          Case((sample, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((sample, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        });
      });

      It("should get constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = Intercept(Noop) }));

          EcmaValue result = slice.Call(sample);
          That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
          That(result["constructor"], Is.EqualTo(TA), "use defaultCtor on an undefined return - constructor check");
        });
      });

      It("should get inherited constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          using (TempProperty(TA.Prototype, "constructor", new EcmaPropertyDescriptor(Intercept(Noop), Undefined))) {
            EcmaValue result = slice.Call(sample);
            That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

            That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
            That(result["constructor"], Is.Undefined, "used defaultCtor but still checks the inherited .constructor");
            That(Logs.Count, Is.EqualTo(2), "result.constructor triggers the inherited accessor property");
          }
        });
      });

      It("should return abrupt from SpeciesConstructor's get Constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = ThrowTest262Exception }));
          Case(sample, Throws.Test262);
        });
      });

      It("should throw if O.constructor returns a non-Object and non-undefined value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));

          sample["constructor"] = 42;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = "1";
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = Null;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = NaN;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = false;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = new Symbol();
          Case((sample, 0), Throws.TypeError);
        });
      });

      It("should get @@species from found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, get: Intercept(() => Undefined), set: null));
          slice.Call(sample, 0);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from get @@species on found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();
          Object.Invoke("defineProperty", sample["constructor"], Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          Case(sample, Throws.Test262);
        });
      });

      It("should use custom species constructor if available", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => TA.Construct(count))));
          Case((sample, 1), new[] { 41, 42 });
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should call custom species constructor with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          EcmaValue result = default;
          EcmaValue ctorThis = default;
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(count => {
            result = Arguments;
            ctorThis = This;
            return TA.Construct(count);
          })));
          slice.Call(sample, 1);
          That(result, Is.EquivalentTo(new[] { 2 }));
          That(ctorThis, Is.InstanceOf(sample["constructor"][Symbol.Species]));
        });
      });

      It("may return a totally different TypedArray from custom species constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40));
          EcmaValue other = Global.Int8Array.Construct(EcmaArray.Of(1, 0, 1));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(() => other)));
          Case((sample, 0, 0), new[] { 1, 0, 1 });
        });
      });

      It("does not throw a TypeError if new typedArray's length >= count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue customCount = default;
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct(customCount))));

          customCount = 2;
          That(slice.Call(sample)["length"], Is.EqualTo(customCount));
          customCount = 5;
          That(slice.Call(sample)["length"], Is.EqualTo(customCount));
        });
      });

      It("should throw a TypeError if new typedArray's length < count", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, RuntimeFunction.Create(() => TA.Construct())));
          Case(sample, Throws.TypeError);
        });
      });

      It("should use defaultConstructor if @@species is either undefined or null", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          EcmaValue result = slice.Call(sample, 0);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));

          sample["constructor"].ToObject()[Symbol.Species] = Null;
          result = slice.Call(sample, 0);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));
        });
      });

      It("should throw if returned @@species is not a constructor, null or undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          sample["constructor"].ToObject()[Symbol.Species] = 0;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = "string";
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = Object.Construct();
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = NaN;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = false;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = true;
          Case((sample, 0), Throws.TypeError);
        });
      });

      It("should throw if custom species constructor does not return a compatible object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, Noop));
          Case(sample, Throws.TypeError);
        });
      });

      It("should perform regular set if target's uses a different element type", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue other = TA == Global.Int8Array ? Global.Uint8Array : Global.Int8Array;
          sample["constructor"] = CreateObject((Symbol.Species, other));

          EcmaValue result = slice.Call(sample);
          That(result, Is.EquivalentTo(new[] { 42, 43, 44 }));
          That(result["buffer"], Is.Not.EqualTo(sample["buffer"]));
          That(result["constructor"], Is.EqualTo(other));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Some(RuntimeFunction some) {
      IsUnconstructableFunctionWLength(some, "some", 1);
      That(TypedArray.Prototype, Has.OwnProperty("some", some, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, Noop), Throws.TypeError);
        Case((Null, Noop), Throws.TypeError);
        Case((42, Noop), Throws.TypeError);
        Case(("1", Noop), Throws.TypeError);
        Case((true, Noop), Throws.TypeError);
        Case((new Symbol("s"), Noop), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, Noop), Throws.TypeError);
        Case((Object.Construct(), Noop), Throws.TypeError);
        Case((EcmaArray.Of(), Noop), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), Noop), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Noop), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case((sample, ThrowTest262Exception), Throws.TypeError);
        });
      });

      It("should throw a TypeError if callbackfn is not callable", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case(sample, Throws.TypeError);
          Case((sample, Undefined), Throws.TypeError);
          Case((sample, Null), Throws.TypeError);
          Case((sample, "abc"), Throws.TypeError);
          Case((sample, 1), Throws.TypeError);
          Case((sample, NaN), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, sample), Throws.TypeError);
          Case((sample, new Symbol()), Throws.TypeError);
        });
      });

      It("should throw if instance buffer is detached during loop", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          Case((sample, Intercept((v, i) => {
            if (i == 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
          })), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case((sample, Intercept(() => false)), Throws.Nothing);
            That(Logs.Count, Is.EqualTo(2));
          }
        });
      });

      It("should not visit non-integer properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(7, 8));
          EcmaValue results = EcmaArray.Of();
          sample["foo"] = 42;
          sample[new Symbol()] = 43;
          some.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 7, 0, sample },
            new[] { 8, 1, sample }
          }));
        });
      });

      It("should not call callbackfn on empty instances", () => {
        TestWithTypedArrayConstructors(TA => {
          some.Call(TA.Construct(), Intercept(Noop));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should not change the instance with the return value of callback", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          some.Call(TA.Construct(), Intercept(() => 0));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42 }));
        });
      });

      It("should not cache integer indexed values before iteration", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue newVal = 0;
          some.Call(sample, RuntimeFunction.Create((val, i) => {
            if (i > 0) {
              That(sample[i - 1], Is.EqualTo(newVal - 1), "get the changed value during the loop");
              That(Reflect.Invoke("set", sample, 0, 7), Is.True, "re-set a value for sample[0]");
            }
            That(Reflect.Invoke("set", sample, i, newVal), Is.True, "set value during iteration");
            newVal += 1;
          }));
          That(sample, Is.EquivalentTo(new[] { 7, 1, 2 }));
        });
      });

      It("should call callbackfn with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44));
          EcmaValue results = EcmaArray.Of();
          some.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }));
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }));

          EcmaValue thisArg = EcmaArray.Of("test262", 0, "ecma262", 0);
          results = EcmaArray.Of();
          some.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", Arguments);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] {
            new[] { 42, 0, sample },
            new[] { 43, 1, sample },
            new[] { 44, 2, sample }
          }), "thisArg does not affect callbackfn arguments");
        });
      });

      It("should call callbackfn with correct `this` value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          EcmaValue results = EcmaArray.Of();
          some.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }));
          That(results, Is.EquivalentTo(new[] { Undefined, Undefined, Undefined }));

          EcmaValue thisArg = Object.Construct();
          results = EcmaArray.Of();
          some.Call(sample, RuntimeFunction.Create(() => {
            results.Invoke("push", This);
          }), thisArg);
          That(results, Is.EquivalentTo(new[] { thisArg, thisArg, thisArg }));
        });
      });

      It("should return true if any callbackfn returns a coerced true", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { true, 1, "test262", new Symbol("1"), Object.Construct(), EcmaArray.Of(), -1, Infinity, -Infinity, 0.1, -0.1 };
          foreach (EcmaValue val in values) {
            int called = 0;
            Case((TA.Construct(42), Intercept(() => Return(++called == 1 ? false : val))), true);
            That(called, Is.EqualTo(2));
          }
        });
      });

      It("should return false if every callbackfn call returns a coerced false", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue[] values = { false, "", 0, -0, NaN, Undefined, Null };
          Case((TA.Construct(values.Length), Intercept((v, i) => values[(int)i])), false);
          That(Logs.Count, Is.EqualTo(values.Length));
        });
      });

      It("should return abrupt from callbackfn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(3);
          Case((sample, Intercept(ThrowTest262Exception)), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Sort(RuntimeFunction sort) {
      IsUnconstructableFunctionWLength(sort, "sort", 1);
      That(TypedArray.Prototype, Has.OwnProperty("sort", sort, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case(sample, Throws.Nothing);
          }
        });
      });

      It("should call comparefn if not undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = EcmaArray.Of();
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 42, 42, 42, 42));
          EcmaValue comparefn = Intercept(() => {
            result.Invoke("push", EcmaArray.Of(This, Arguments));
          });
          sort.Call(sample, comparefn);
          That(Logs.Count, Is.GreaterThan(0));
          foreach (EcmaValue value in result.ForOf()) {
            That(value, Is.EquivalentTo(new object[] { Undefined, new[] { 42, 42 } }));
          }
        });
      });

      It("should throw on a non-undefined non-function", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44, 45, 46));
          Case((sample, Null), Throws.TypeError);
          Case((sample, true), Throws.TypeError);
          Case((sample, false), Throws.TypeError);
          Case((sample, ""), Throws.TypeError);
          Case((sample, 42), Throws.TypeError);
          Case((sample, Object.Construct()), Throws.TypeError);
          Case((sample, EcmaArray.Of()), Throws.TypeError);
          Case((sample, RegExp.Construct("a", "g")), Throws.TypeError);
        });
      });

      It("should return abrupt from comparefn", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43, 44, 45, 46));
          EcmaValue comparefn = Intercept(ThrowTest262Exception);
          Case((sample, comparefn), Throws.Test262);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should throw a TypeError if comparefn detaches the object buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(4);
          EcmaValue comparefn = Intercept(() => {
            if (Logs.Count > 1) {
              ThrowTest262Exception();
            }
            DetachBuffer(sample);
          });
          Case((sample, comparefn), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should pass the result of compareFn immediately through ToNumber", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = TA.Construct(4);
          EcmaValue comparefn = RuntimeFunction.Create((a, b) => {
            DetachBuffer(ta);
            return CreateObject(toPrimitive: Intercept(() => Undefined));
          });
          Case((ta, comparefn), Throws.TypeError);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should not cast values to String", () => {
        using (TempProperty(Number.Prototype, "toString", Intercept(() => Undefined))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(20, 100, 3));
            Case(sample, Is.EquivalentTo(new[] { 3, 20, 100 }));
            That(Logs.Count, Is.Zero);
          });
        }
      });

      It("should sort values to numeric ascending order", () => {
        EcmaValue sample;

        TestWithTypedArrayConstructors(TA => {
          sample = TA.Construct(EcmaArray.Of(2, NaN, NaN, 0, 1));
          Case(sample, Is.EquivalentTo(new[] { 0, 1, 2, NaN, NaN }));

          sample = TA.Construct(EcmaArray.Of(3, NaN, NaN, Infinity, 0, -Infinity, 2));
          Case(sample, Is.EquivalentTo(new[] { -Infinity, 0, 2, 3, Infinity, NaN, NaN }));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          sample = TA.Construct(EcmaArray.Of(4, 3, 2, 1));
          Case(sample, Is.EquivalentTo(new[] { 1, 2, 3, 4 }), "descending values");

          sample = TA.Construct(EcmaArray.Of(3, 4, 1, 2));
          Case(sample, Is.EquivalentTo(new[] { 1, 2, 3, 4 }), "mixed numbers");

          sample = TA.Construct(EcmaArray.Of(3, 4, 3, 1, 0, 1, 2));
          Case(sample, Is.EquivalentTo(new[] { 0, 1, 1, 2, 3, 3, 4 }), "repeating numbers");

          sample = TA.Construct(EcmaArray.Of(1, 0, -0, 2));
          Case(sample, Is.EquivalentTo(new[] { 0, 0, 1, 2 }), "0s");
        });

        TestWithTypedArrayConstructors(TA => {
          sample = TA.Construct(EcmaArray.Of(-4, 3, 4, -3, 2, -2, 1, 0));
          Case(sample, Is.EquivalentTo(new[] { -4, -3, -2, 0, 1, 2, 3, 4 }), "negative values");
        }, new[] { Global.Float64Array, Global.Float32Array, Global.Int8Array, Global.Int16Array, Global.Int32Array });

        TestWithTypedArrayConstructors(TA => {
          sample = TA.Construct(EcmaArray.Of(0.5, 0, 1.5, 1));
          Case(sample, Is.EquivalentTo(new[] { 0, 0.5, 1, 1.5 }), "non integers");

          sample = TA.Construct(EcmaArray.Of(0.5, 0, 1.5, -0.5, -1, -1.5, 1));
          Case(sample, Is.EquivalentTo(new[] { -1.5, -1, -0.5, 0, 0.5, 1, 1.5 }), "non integers + negatives");

          sample = TA.Construct(EcmaArray.Of(1, 0, -0, 2));
          Case(sample, Is.EquivalentTo(new[] { 0, 0, 1, 2 }), "0 and -0");

          sample = TA.Construct(EcmaArray.Of(3, 4, Infinity, -Infinity, 1, 2));
          Case(sample, Is.EquivalentTo(new[] { -Infinity, 1, 2, 3, 4, Infinity }), "infinities");
        }, new[] { Global.Float64Array, Global.Float32Array });
      });

      It("should return the same instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(2, 1));
          Case(sample, sample);
          Case((sample, RuntimeFunction.Create(() => 0)), sample);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Subarray(RuntimeFunction subarray) {
      IsUnconstructableFunctionWLength(subarray, "subarray", 2);
      That(TypedArray.Prototype, Has.OwnProperty("subarray", subarray, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case((Undefined, 0, 0), Throws.TypeError);
        Case((Null, 0, 0), Throws.TypeError);
        Case((42, 0, 0), Throws.TypeError);
        Case(("1", 0, 0), Throws.TypeError);
        Case((true, 0, 0), Throws.TypeError);
        Case((new Symbol("s"), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case((TypedArray.Prototype, 0, 0), Throws.TypeError);
        Case((Object.Construct(), 0, 0), Throws.TypeError);
        Case((EcmaArray.Of(), 0, 0), Throws.TypeError);
        Case((ArrayBuffer.Construct(8), 0, 0), Throws.TypeError);
        Case((Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError creating a new instance with a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue beginCalled = false;
          EcmaValue endCalled = false;
          EcmaValue o1 = CreateObject(valueOf: () => Return(beginCalled = true, 0));
          EcmaValue o2 = CreateObject(valueOf: () => Return(endCalled = true, 2));
          DetachBuffer(sample);
          Case((sample, o1, o2), Throws.TypeError);
          That(beginCalled, Is.True, "observable ToInteger(begin)");
          That(endCalled, Is.True, "observable ToInteger(end)");
        });
      });

      It("should handle Infinity values on begin and end", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, -Infinity), new[] { 40, 41, 42, 43 });
          Case((sample, Infinity), new EcmaValue[0]);
          Case((sample, 0, -Infinity), new EcmaValue[0]);
          Case((sample, 0, Infinity), new[] { 40, 41, 42, 43 });
        });
      });

      It("should handle -0 values on begin and end", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, -0d), new[] { 40, 41, 42, 43 });
          Case((sample, -0d, 4), new[] { 40, 41, 42, 43 });
          Case((sample, 0, -0d), new EcmaValue[0]);
          Case((sample, -0d, -0d), new EcmaValue[0]);
        });
      });

      It("should not import own property", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(41, 42, 43, 44));
          sample["foo"] = 42;

          EcmaValue result = subarray.Call(sample, 0);
          That(result.Invoke("hasOwnProperty", "foo"), Is.False);
        });
      });

      It("should return a new instance from the same constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          EcmaValue result = subarray.Call(sample, 1);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(sample["constructor"]));
          That(result, Is.InstanceOf(TA));
          That(sample, Is.EquivalentTo(new[] { 40, 41, 42, 43 }), "original sample remains the same");
        });
      });

      It("should return a new instance sharing the same buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          EcmaValue buffer = sample["buffer"];
          EcmaValue result = subarray.Call(sample, 1);

          That(result, Is.Not.EqualTo(sample), "returns a new instance");
          That(result["buffer"], Is.EqualTo(sample["buffer"]), "shared buffer");
          That(sample["buffer"], Is.EqualTo(buffer), "original buffer is preserved");

          sample[1] = 100;
          That(result, Is.EquivalentTo(new[] { 100, 42, 43 }), "changes on the original sample values affect the new instance");

          result[1] = 111;
          That(sample, Is.EquivalentTo(new[] { 40, 100, 111, 43 }), "changes on the new instance values affect the original sample");
        });
      });

      It("may return a new instance with a smaller length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 1), new[] { 41, 42, 43 }, "begin == 1");
          Case((sample, 2), new[] { 42, 43 }, "begin == 2");
          Case((sample, 3), new[] { 43 }, "begin == 3");

          Case((sample, 1, 4), new[] { 41, 42, 43 }, "begin == 1, end == length");
          Case((sample, 2, 4), new[] { 42, 43 }, "begin == 2, end == length");
          Case((sample, 3, 4), new[] { 43 }, "begin == 3, end == length");

          Case((sample, 0, 1), new[] { 40 }, "begin == 0, end == 1");
          Case((sample, 0, 2), new[] { 40, 41 }, "begin == 0, end == 2");
          Case((sample, 0, 3), new[] { 40, 41, 42 }, "begin == 0, end == 3");

          Case((sample, -1), new[] { 43 }, "begin == -1");
          Case((sample, -2), new[] { 42, 43 }, "begin == -2");
          Case((sample, -3), new[] { 41, 42, 43 }, "begin == -3");

          Case((sample, -1, 4), new[] { 43 }, "begin == -1, end == length");
          Case((sample, -2, 4), new[] { 42, 43 }, "begin == -2, end == length");
          Case((sample, -3, 4), new[] { 41, 42, 43 }, "begin == -3, end == length");

          Case((sample, 0, -1), new[] { 40, 41, 42 }, "begin == 0, end == -1");
          Case((sample, 0, -2), new[] { 40, 41 }, "begin == 0, end == -2");
          Case((sample, 0, -3), new[] { 40 }, "begin == 0, end == -3");

          Case((sample, -0, -1), new[] { 40, 41, 42 }, "begin == -0, end == -1");
          Case((sample, -0, -2), new[] { 40, 41 }, "begin == -0, end == -2");
          Case((sample, -0, -3), new[] { 40 }, "begin == -0, end == -3");

          Case((sample, -2, -1), new[] { 42 }, "length == 4, begin == -2, end == -1");
          Case((sample, 1, -1), new[] { 41, 42 }, "length == 4, begin == 1, end == -1");
          Case((sample, 1, -2), new[] { 41 }, "length == 4, begin == 1, end == -2");
          Case((sample, 2, -1), new[] { 42 }, "length == 4, begin == 2, end == -1");

          Case((sample, -1, 5), new[] { 43 }, "begin == -1, end > length");
          Case((sample, -2, 4), new[] { 42, 43 }, "begin == -2, end > length");
          Case((sample, -3, 4), new[] { 41, 42, 43 }, "begin == -3, end > length");
        });
      });

      It("may return a new empty instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 4), new EcmaValue[0], "begin == length");
          Case((sample, 5), new EcmaValue[0], "begin > length");

          Case((sample, 4, 4), new EcmaValue[0], "begin == length, end == length");
          Case((sample, 5, 4), new EcmaValue[0], "begin > length, end == length");

          Case((sample, 4, 4), new EcmaValue[0], "begin == length, end > length");
          Case((sample, 5, 4), new EcmaValue[0], "begin > length, end > length");

          Case((sample, 0, 0), new EcmaValue[0], "begin == 0, end == 0");
          Case((sample, -0, -0), new EcmaValue[0], "begin == -0, end == -0");
          Case((sample, 1, 0), new EcmaValue[0], "begin > 0, end == 0");
          Case((sample, -1, 0), new EcmaValue[0], "being < 0, end == 0");

          Case((sample, 2, 1), new EcmaValue[0], "begin > 0, begin < length, begin > end, end > 0");
          Case((sample, 2, 2), new EcmaValue[0], "begin > 0, begin < length, begin == end");

          Case((sample, 2, -2), new EcmaValue[0], "begin > 0, begin < length, end == -2");

          Case((sample, -1, -1), new EcmaValue[0], "length = 4, begin == -1, end == -1");
          Case((sample, -1, -2), new EcmaValue[0], "length = 4, begin == -1, end == -2");
          Case((sample, -2, -2), new EcmaValue[0], "length = 4, begin == -2, end == -2");

          Case((sample, 0, -4), new EcmaValue[0], "begin == 0, end == -length");
          Case((sample, -4, -4), new EcmaValue[0], "begin == -length, end == -length");
          Case((sample, -5, -4), new EcmaValue[0], "begin < -length, end == -length");

          Case((sample, 0, -5), new EcmaValue[0], "begin == 0, end < -length");
          Case((sample, -4, -5), new EcmaValue[0], "begin == -length, end < -length");
          Case((sample, -5, -5), new EcmaValue[0], "begin < -length, end < -length");
        });
      });

      It("may return a new instance with the same length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 0), new[] { 40, 41, 42, 43 }, "begin == 0");
          Case((sample, -4), new[] { 40, 41, 42, 43 }, "begin == -srcLength");
          Case((sample, -5), new[] { 40, 41, 42, 43 }, "begin < -srcLength");

          Case((sample, 0, 4), new[] { 40, 41, 42, 43 }, "begin == 0, end == srcLength");
          Case((sample, -4, 4), new[] { 40, 41, 42, 43 }, "begin == -srcLength, end == srcLength");
          Case((sample, -5, 4), new[] { 40, 41, 42, 43 }, "begin < -srcLength, end == srcLength");

          Case((sample, 0, 5), new[] { 40, 41, 42, 43 }, "begin == 0, end > srcLength");
          Case((sample, -4, 5), new[] { 40, 41, 42, 43 }, "begin == -srcLength, end > srcLength");
          Case((sample, -5, 5), new[] { 40, 41, 42, 43 }, "begin < -srcLength, end > srcLength");
        });
      });

      It("should perform ToInteger(begin)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, false), new[] { 40, 41, 42, 43 }, "false");
          Case((sample, true), new[] { 41, 42, 43 }, "true");

          Case((sample, NaN), new[] { 40, 41, 42, 43 }, "NaN");
          Case((sample, Null), new[] { 40, 41, 42, 43 }, "null");
          Case((sample, Undefined), new[] { 40, 41, 42, 43 }, "undefined");

          Case((sample, 1.1), new[] { 41, 42, 43 }, "1.1");
          Case((sample, 1.5), new[] { 41, 42, 43 }, "1.5");
          Case((sample, 0.6), new[] { 40, 41, 42, 43 }, "0.6");

          Case((sample, -1.5), new[] { 43 }, "-1.5");
          Case((sample, -1.1), new[] { 43 }, "-1.1");
          Case((sample, -0.6), new[] { 40, 41, 42, 43 }, "-0.6");

          Case((sample, "3"), new[] { 43 }, "string");
          Case((sample, CreateObject(valueOf: () => 2)), new[] { 42, 43 });
        });
      });

      It("should perform ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Case((sample, 0, false), new EcmaValue[] { }, "false");
          Case((sample, 0, true), new[] { 40 }, "true");

          Case((sample, 0, NaN), new EcmaValue[] { }, "NaN");
          Case((sample, 0, Null), new EcmaValue[] { }, "null");
          Case((sample, 0, Undefined), new[] { 40, 41, 42, 43 }, "undefined");

          Case((sample, 0, 0.6), new EcmaValue[] { }, "0.6");
          Case((sample, 0, 1.1), new[] { 40 }, "1.1");
          Case((sample, 0, 1.5), new[] { 40 }, "1.5");
          Case((sample, 0, -0.6), new EcmaValue[] { }, "-0.6");
          Case((sample, 0, -1.1), new[] { 40, 41, 42 }, "-1.1");
          Case((sample, 0, -1.5), new[] { 40, 41, 42 }, "-1.5");

          Case((sample, 0, "3"), new[] { 40, 41, 42 }, "string");
          Case((sample, 0, CreateObject(valueOf: () => 2)), new[] { 40, 41 });
        });
      });

      It("should return abrupt from ToInteger(begin)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, new Symbol()), Throws.TypeError);
          Case((sample, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((sample, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        });
      });

      It("should return abrupt from ToInteger(end)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Case((sample, 0, new Symbol()), Throws.TypeError);
          Case((sample, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
          Case((sample, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        });
      });

      It("should get constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = Intercept(Noop) }));

          EcmaValue result = subarray.Call(sample, 0);
          That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
          That(result["constructor"], Is.EqualTo(TA), "use defaultCtor on an undefined return - constructor check");
        });
      });

      It("should get inherited constructor on SpeciesConstructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          using (TempProperty(TA.Prototype, "constructor", new EcmaPropertyDescriptor(Intercept(Noop), Undefined))) {
            EcmaValue result = subarray.Call(sample, 0);
            That(Logs.Count, Is.EqualTo(1), "called custom ctor get accessor once");

            That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)), "use defaultCtor on an undefined return - getPrototypeOf check");
            That(result["constructor"], Is.Undefined, "used defaultCtor but still checks the inherited .constructor");
            That(Logs.Count, Is.EqualTo(2), "result.constructor triggers the inherited accessor property");
          }
        });
      });

      It("should return abrupt from SpeciesConstructor's get Constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          Object.Invoke("defineProperty", sample, "constructor", CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, 0), Throws.Test262);
        });
      });

      It("should throw if O.constructor returns a non-Object and non-undefined value", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));

          sample["constructor"] = 42;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = "1";
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = Null;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = NaN;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = false;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"] = new Symbol();
          Case((sample, 0), Throws.TypeError);
        });
      });

      It("should get @@species from found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, get: Intercept(() => Undefined), set: null));
          subarray.Call(sample, 0);
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from get @@species on found constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42, 43));
          sample["constructor"] = Object.Construct();
          Object.Invoke("defineProperty", sample["constructor"], Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          Case((sample, 0), Throws.Test262);
        });
      });

      It("should use custom species constructor if available", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept((buffer, offset, length) => TA.Construct(buffer, offset, length))));
          Case((sample, 1), new[] { 41, 42 });
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should call custom species constructor with correct arguments", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40, 41, 42));
          EcmaValue result = default;
          EcmaValue ctorThis = default;
          sample["constructor"] = CreateObject((Symbol.Species, Intercept((buffer, offset, length) => {
            result = Arguments;
            ctorThis = This;
            return TA.Construct(buffer, offset, length);
          })));
          subarray.Call(sample, 1);
          That(result, Is.EquivalentTo(new[] { sample["buffer"], TA["BYTES_PER_ELEMENT"], 2 }));
          That(ctorThis, Is.InstanceOf(sample["constructor"][Symbol.Species]));
        });
      });

      It("may return a totally different TypedArray from custom species constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(40));
          EcmaValue other = Global.Int8Array.Construct(EcmaArray.Of(1, 0, 1));
          sample["constructor"] = CreateObject((Symbol.Species, Intercept(() => other)));
          Case((sample, 0, 0), new[] { 1, 0, 1 });
        });
      });

      It("should use defaultConstructor if @@species is either undefined or null", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          EcmaValue result = subarray.Call(sample, 0);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));

          sample["constructor"].ToObject()[Symbol.Species] = Null;
          result = subarray.Call(sample, 0);
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Invoke("getPrototypeOf", sample)));
          That(result["constructor"], Is.EqualTo(TA));
        });
      });

      It("should throw if returned @@species is not a constructor, null or undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = Object.Construct();

          sample["constructor"].ToObject()[Symbol.Species] = 0;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = "string";
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = Object.Construct();
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = NaN;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = false;
          Case((sample, 0), Throws.TypeError);
          sample["constructor"].ToObject()[Symbol.Species] = true;
          Case((sample, 0), Throws.TypeError);
        });
      });

      It("should throw if custom species constructor does not return a compatible object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          sample["constructor"] = CreateObject((Symbol.Species, Noop));
          Case((sample, 0), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleString(RuntimeFunction toLocaleString) {
      IsUnconstructableFunctionWLength(toLocaleString, "toLocaleString", 0);
      That(TypedArray.Prototype, Has.OwnProperty("toLocaleString", toLocaleString, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should call toLocaleString from each property's value", () => {
        EcmaValue separator = EcmaArray.Of("", "").Invoke("toLocaleString");
        EcmaValue expected = EcmaArray.Of("hacks1", "hacks2").Invoke("join", separator);
        EcmaValue arr = EcmaArray.Of(42, 0);
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => Return(Logs.Add(This), "hacks" + Logs.Count)))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(arr);
            Case(sample, expected, "returns expected value");
            That(Logs, Is.EquivalentTo(new[] { 42, 0 }));
          });
        }
      });

      It("should call toString from each property's value return from toLocaleString", () => {
        EcmaValue separator = EcmaArray.Of("", "").Invoke("toLocaleString");
        EcmaValue expected = EcmaArray.Of("hacks1", "hacks2").Invoke("join", separator);
        EcmaValue arr = EcmaArray.Of(42, 0);
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(toString: Intercept(() => "hacks" + Logs.Count), valueOf: ThrowTest262Exception)))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(arr);
            Case(sample, expected, "returns expected value");
            That(Logs.Count, Is.EqualTo(2));
          });
        }
      });

      It("should call valueOf from each property's value return from toLocaleString", () => {
        EcmaValue separator = EcmaArray.Of("", "").Invoke("toLocaleString");
        EcmaValue expected = EcmaArray.Of("hacks1", "hacks2").Invoke("join", separator);
        EcmaValue arr = EcmaArray.Of(42, 0);
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(new { valueOf = Intercept(() => "hacks" + Logs.Count), toString = Undefined })))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(arr);
            Case(sample, expected, "returns expected value");
            That(Logs.Count, Is.EqualTo(2));
          });
        }
      });

      It("should return an empty string if called on an empty instance", () => {
        TestWithTypedArrayConstructors(TA => {
          Case(TA.Construct(), "");
        });
      });

      It("should get length from internal ArrayLength", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception }))))
          using (TempProperty(TA.Prototype, "length", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Object.Invoke("defineProperty", sample, "length", CreateObject(new { get = ThrowTest262Exception }));
            Case(sample, Throws.Nothing);
          }
        });
      });

      It("should return abrupt from firstElement's toLocaleString", () => {
        using (TempProperty(Number.Prototype, "toLocaleString", Intercept(ThrowTest262Exception))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(1));
          });
        }
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(toString: Intercept(ThrowTest262Exception))))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(1));
          });
        }
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(toString: () => Object.Construct(), valueOf: Intercept(ThrowTest262Exception))))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(1));
          });
        }
      });

      It("should return abrupt from nextElement's toLocaleString", () => {
        System.Func<EcmaValue> thrower = Intercept(() => Logs.Count > 1 ? ThrowTest262Exception() : Undefined);
        using (TempProperty(Number.Prototype, "toLocaleString", thrower)) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(2));
          });
        }
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(toString: thrower)))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(2));
          });
        }
        using (TempProperty(Number.Prototype, "toLocaleString", RuntimeFunction.Create(() => CreateObject(toString: () => Object.Construct(), valueOf: thrower)))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            Case(sample, Throws.Test262);
            That(Logs.Count, Is.EqualTo(2));
          });
        }
      });

      It("should return a string", () => {
        EcmaValue separator = EcmaArray.Of("", "").Invoke("toLocaleString");
        EcmaValue arr = EcmaArray.Of(42, 0, 43);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(arr);
          EcmaValue expected =
            sample[0].Invoke("toLocaleString").Invoke("toString") +
            separator +
            sample[1].Invoke("toLocaleString").Invoke("toString") +
            separator +
            sample[2].Invoke("toLocaleString").Invoke("toString");
          Case(sample, expected);
        });
      });
    }

    [Test]
    public void ToString() {
      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          That(() => sample.Invoke("toString"), Throws.TypeError);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Values(RuntimeFunction values) {
      IsUnconstructableFunctionWLength(values, "values", 0);
      That(TypedArray.Prototype, Has.OwnProperty("values", values, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case(Undefined, Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not Object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(42, Throws.TypeError);
        Case("1", Throws.TypeError);
        Case(true, Throws.TypeError);
        Case(new Symbol("s"), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` is not a TypedArray instance", () => {
        Case(TypedArray.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(ArrayBuffer.Construct(8), 0, 1), Throws.TypeError);
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          DetachBuffer(sample);
          Case(sample, Throws.TypeError);
        });
      });

      It("should return an iterator which the prototype is ArrayIteratorPrototype", () => {
        EcmaValue ArrayIteratorProto = Object.Invoke("getPrototypeOf", EcmaArray.Of().Invoke(Symbol.Iterator));
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 42, 64));
          EcmaValue iter = sample.Invoke("values");
          That(Object.Invoke("getPrototypeOf", iter), Is.EqualTo(ArrayIteratorProto));
        });
      });

      It("should return an iterator for the values", () => {
        EcmaValue sample = EcmaArray.Of(0, 42, 64);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(sample);
          EcmaValue itor = typedArray.Invoke("values");
          VerifyIteratorResult(itor.Invoke("next"), false, 0);
          VerifyIteratorResult(itor.Invoke("next"), false, 42);
          VerifyIteratorResult(itor.Invoke("next"), false, 64);
          VerifyIteratorResult(itor.Invoke("next"), true);
        });
      });
    }
  }
}
