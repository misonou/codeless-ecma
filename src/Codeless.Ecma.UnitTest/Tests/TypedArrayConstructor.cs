using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Harness;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class TypedArrayConstructor : TestBase {
    RuntimeFunction ArrayBuffer => Global.ArrayBuffer;
    RuntimeFunction TypedArray => (RuntimeFunction)WellKnownObject.TypedArray;

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "TypedArray", 0, TypedArray.Prototype);

      It("should throw a TypeError exception if directly invoked", () => {
        That(() => ctor.Call(), Throws.TypeError);
        That(() => ctor.Call(Undefined, 1), Throws.TypeError);
        That(() => ctor.Call(Undefined, 1.1), Throws.TypeError);
        That(() => ctor.Call(Undefined, Object.Construct()), Throws.TypeError);
        That(() => ctor.Call(Undefined, Global.Int8Array.Construct(4)), Throws.TypeError);
        That(() => ctor.Call(Undefined, Global.ArrayBuffer.Construct(4)), Throws.TypeError);
        That(() => ctor.Construct(), Throws.TypeError);
        That(() => ctor.Construct(1), Throws.TypeError);
        That(() => ctor.Construct(1.1), Throws.TypeError);
        That(() => ctor.Construct(Object.Construct()), Throws.TypeError);
        That(() => ctor.Construct(Global.Int8Array.Construct(4)), Throws.TypeError);
        That(() => ctor.Construct(Global.ArrayBuffer.Construct(4)), Throws.TypeError);
      });
    }

    [Test]
    public void Constructor_Derived() {
      void VerifyConstructor(RuntimeFunction constructor, string name, int bytesPerElement) {
        IsConstructorWLength(constructor, name, 3, constructor.Prototype, TypedArray);
        That(constructor, Has.OwnProperty("BYTES_PER_ELEMENT", bytesPerElement, EcmaPropertyAttributes.None));
        That(Object.Invoke("getPrototypeOf", constructor), Is.EqualTo(TypedArray));

        That(Object.Invoke("getPrototypeOf", constructor.Prototype), Is.EqualTo(TypedArray.Prototype));
        That(() => constructor.Prototype["buffer"], Throws.TypeError);
        That(constructor.Prototype, Has.OwnProperty("BYTES_PER_ELEMENT", bytesPerElement, EcmaPropertyAttributes.None));
        That(constructor.Prototype, Has.OwnProperty("constructor", constructor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      };
      VerifyConstructor(Global.Int8Array, "Int8Array", bytesPerElement: 1);
      VerifyConstructor(Global.Uint8Array, "Uint8Array", bytesPerElement: 1);
      VerifyConstructor(Global.Uint8ClampedArray, "Uint8ClampedArray", bytesPerElement: 1);
      VerifyConstructor(Global.Int16Array, "Int16Array", bytesPerElement: 2);
      VerifyConstructor(Global.Int32Array, "Int32Array", bytesPerElement: 4);
      VerifyConstructor(Global.Uint16Array, "Uint16Array", bytesPerElement: 2);
      VerifyConstructor(Global.Uint32Array, "Uint32Array", bytesPerElement: 4);
      VerifyConstructor(Global.Float32Array, "Float32Array", bytesPerElement: 4);
      VerifyConstructor(Global.Float64Array, "Float64Array", bytesPerElement: 8);
    }

    [Test]
    public void Constructor_Buffer() {
      It("should throw a RangeError if bufferByteLength modulo elementSize ≠ 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer), Throws.RangeError);
          That(() => TA.Construct(buffer, 0, Undefined), Throws.RangeError);
        }, new[] { Global.Float64Array, Global.Float32Array, Global.Int32Array, Global.Int16Array, Global.Uint32Array, Global.Uint16Array });
      });

      It("should throw a RangeError if ToInteger(byteOffset) is < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, -1), Throws.RangeError);
          That(() => TA.Construct(buffer, -Infinity), Throws.RangeError);
        });
      });

      It("TypedArray's [[ByteOffset]] internal slot is always a positive number, test with negative zero.", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(ArrayBuffer.Construct(8), -0d);
          That(typedArray["byteOffset"].Equals(0, EcmaValueComparison.SameValue));
        });
      });

      It("return abrupt from parsing integer value from byteOffset as a symbol", () => {
        EcmaValue byteOffset = new Symbol("1");
        EcmaValue buffer = ArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, byteOffset), Throws.TypeError);
        });
      });

      It("should throw a RangeError if ToInteger(byteOffset) modulo elementSize is not 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, 7), Throws.RangeError);
        }, new[] { Global.Float64Array, Global.Float32Array, Global.Int32Array, Global.Int16Array, Global.Uint32Array, Global.Uint16Array });
      });

      It("should throw if TypedArray() is passed a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buffer = ArrayBuffer.Construct(3 * TA["BYTES_PER_ELEMENT"]);
          DetachBuffer(buffer);
          That(() => TA.Construct(buffer), Throws.TypeError);
        });
      });

      It("should throw if TypedArray() is passed a detached buffer during ToIndex(length)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buffer = ArrayBuffer.Construct(3 * TA["BYTES_PER_ELEMENT"]);
          EcmaValue length = CreateObject(valueOf: () => {
            DetachBuffer(buffer);
            return 1;
          });
          That(() => TA.Construct(buffer, 0, length), Throws.TypeError);
        });
      });

      It("should throw if TypedArray() is passed a detached buffer during ToIndex(byteOffset)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buffer = ArrayBuffer.Construct(3 * TA["BYTES_PER_ELEMENT"]);
          EcmaValue byteOffset = CreateObject(valueOf: () => {
            DetachBuffer(buffer);
            return TA["BYTES_PER_ELEMENT"];
          });
          That(() => TA.Construct(buffer, byteOffset), Throws.TypeError);
        });
      });

      It("should return abrupt from parsing integer value from byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue byteOffset = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, byteOffset), Throws.Test262);
        });
      });

      It("should return abrupt completion getting newTarget's prototype", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => Reflect.Invoke("construct", TA, EcmaArray.Of(buffer), newTarget), Throws.Test262);
        });
      });

      It("should return new typedArray from defined length and offset", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue offset = TA["BYTES_PER_ELEMENT"];
          EcmaValue buffer = ArrayBuffer.Construct(3 * offset);

          EcmaValue ta1 = TA.Construct(buffer, offset, 2);
          That(ta1["length"], Is.EqualTo(2), "ta1.length");
          That(ta1["buffer"], Is.EqualTo(buffer), "ta1.buffer");
          That(ta1["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta1), Is.EqualTo(TA["prototype"]));

          EcmaValue ta2 = TA.Construct(buffer, offset, 0);
          That(ta2["length"], Is.EqualTo(0), "ta2.length");
          That(ta2["buffer"], Is.EqualTo(buffer), "ta2.buffer");
          That(ta2["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta2), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should return new typedArray from defined length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];
          EcmaValue length = 4;
          EcmaValue buffer = ArrayBuffer.Construct(bpe * length * 4);

          EcmaValue ta1 = TA.Construct(buffer, 0, length);
          That(ta1["length"], Is.EqualTo(length), "ta1.length");
          That(ta1["buffer"], Is.EqualTo(buffer), "ta1.buffer");
          That(ta1["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta1), Is.EqualTo(TA["prototype"]));

          EcmaValue ta2 = TA.Construct(buffer, 0, 0);
          That(ta2["length"], Is.EqualTo(0), "ta2.length");
          That(ta2["buffer"], Is.EqualTo(buffer), "ta2.buffer");
          That(ta2["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta2), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should throw a RangeError exception for negative ToInteger(length)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(16);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, 0, -1), Throws.RangeError);
          That(() => TA.Construct(buffer, 0, -Infinity), Throws.RangeError);
        });
      });

      It("should return new typedArray from defined offset", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];
          EcmaValue buffer = ArrayBuffer.Construct(bpe * 4);

          EcmaValue ta1 = TA.Construct(buffer, bpe * 2);
          That(ta1["length"], Is.EqualTo(2), "ta1.length");
          That(ta1["buffer"], Is.EqualTo(buffer), "ta1.buffer");
          That(ta1["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta1), Is.EqualTo(TA["prototype"]));

          EcmaValue ta2 = TA.Construct(buffer, 0);
          That(ta2["length"], Is.EqualTo(4), "ta2.length");
          That(ta2["buffer"], Is.EqualTo(buffer), "ta2.buffer");
          That(ta2["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta2), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should throw a RangeError exception if offset + newByteLength > bufferByteLength", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];
          EcmaValue buffer = ArrayBuffer.Construct(bpe);
          That(() => TA.Construct(buffer, 0, bpe * 2), Throws.RangeError);
        });
      });

      It("should throw a RangeError exception if bufferByteLength - ToInteger(byteOffset) < 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];
          EcmaValue buffer = ArrayBuffer.Construct(bpe);
          That(() => TA.Construct(buffer, bpe * 2), Throws.RangeError);
          That(() => TA.Construct(buffer, bpe * 2, Undefined), Throws.RangeError);
        });
      });

      It("should throw a TypeError if NewTarget is undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buffer = ArrayBuffer.Construct(4);
          That(() => TA.Call(Undefined, buffer), Throws.TypeError);
        });
      });

      It("should reuse buffer argument instead of making a new clone", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];
          EcmaValue buffer = ArrayBuffer.Construct(bpe);
          EcmaValue ta1 = TA.Construct(buffer);
          EcmaValue ta2 = TA.Construct(buffer);
          That(ta1["buffer"], Is.EqualTo(buffer));
          That(ta2["buffer"], Is.EqualTo(buffer));
          That(ta2["buffer"], Is.EqualTo(ta2["buffer"]));
        });
      });

      It("should return abrupt from ToLength(length)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue len = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, 0, len), Throws.Test262);
        });
      });

      It("should throw a TypeError if length is a Symbol", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(buffer, 0, new Symbol("1")), Throws.TypeError);
        });
      });

      It("should return an extensible new typedArray instance from a buffer argument", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue buffer = ArrayBuffer.Construct(8);
          That(Object.Invoke("isExtensible", TA.Construct(buffer)), Is.True);
        });
      });

      It("should derive default [[Prototype]] value from realm of the newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = Reflect.Invoke("construct", TA, EcmaArray.Of(ArrayBuffer.Construct(8)), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.Global)[TA["name"]]["prototype"]));
        });
      });

      It("should return new typedArray from undefined offset and length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue bpe = TA["BYTES_PER_ELEMENT"];

          EcmaValue buffer1 = ArrayBuffer.Construct(bpe * 4);
          EcmaValue ta1 = TA.Construct(buffer1);
          That(ta1["length"], Is.EqualTo(4), "ta1.length");
          That(ta1["buffer"], Is.EqualTo(buffer1), "ta1.buffer");
          That(ta1["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta1), Is.EqualTo(TA["prototype"]));

          EcmaValue buffer2 = ArrayBuffer.Construct(0);
          EcmaValue ta2 = TA.Construct(buffer2);
          That(ta2["length"], Is.EqualTo(0), "ta2.length");
          That(ta2["buffer"], Is.EqualTo(buffer2), "ta2.buffer");
          That(ta2["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta2), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should perform ToIndex(length) operations", () => {
        EcmaValue buffer = ArrayBuffer.Construct(16);
        EcmaValue obj1 = CreateObject(valueOf: () => 1);
        EcmaValue obj2 = CreateObject(toString: () => 1);
        TestWithTypedArrayConstructors(TA => {
          That(TA.Construct(buffer, 0, -0d)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, obj1)["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, obj2)["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, "")["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, "0")["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, "1")["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, false)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, true)["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, NaN)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, Null)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, 0.1)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, 0.9)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, 1.1)["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, 1.9)["length"], Is.EqualTo(1));
          That(TA.Construct(buffer, 0, -0.1)["length"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0, -0.99999)["length"], Is.EqualTo(0));
        });
      });

      It("should perform ToIndex(byteOffset) operations", () => {
        EcmaValue buffer = ArrayBuffer.Construct(16);
        EcmaValue obj1 = CreateObject(valueOf: () => 8);
        EcmaValue obj2 = CreateObject(toString: () => 8);
        TestWithTypedArrayConstructors(TA => {
          That(TA.Construct(buffer, -0d)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, obj1)["byteOffset"], Is.EqualTo(8));
          That(TA.Construct(buffer, obj2)["byteOffset"], Is.EqualTo(8));
          That(TA.Construct(buffer, "")["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, "0")["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, "8")["byteOffset"], Is.EqualTo(8));
          That(TA.Construct(buffer, false)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, NaN)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, Null)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0.1)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, 0.9)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, 8.1)["byteOffset"], Is.EqualTo(8));
          That(TA.Construct(buffer, 8.9)["byteOffset"], Is.EqualTo(8));
          That(TA.Construct(buffer, -0.1)["byteOffset"], Is.EqualTo(0));
          That(TA.Construct(buffer, -0.99999)["byteOffset"], Is.EqualTo(0));
          if (TA["BYTES_PER_ELEMENT"] == 1) {
            That(TA.Construct(buffer, true)["byteOffset"], Is.EqualTo(1));
          } else {
            That(() => TA.Construct(buffer, true), Throws.RangeError);
          }
        });
      });

      It("should produce an ArrayBuffer-backed TypedArray when a SharedArrayBuffer-backed TypedArray is passed", () => {
        EcmaValue sab = GlobalThis["SharedArrayBuffer"].Construct(4);
        RuntimeFunction[] intView = new[] { Global.Int8Array, Global.Uint8Array, Global.Int16Array, Global.Uint16Array, Global.Int32Array, Global.Uint32Array };
        TestWithTypedArrayConstructors(TA1 => {
          EcmaValue ta1 = TA1.Construct(sab);
          TestWithTypedArrayConstructors(TA2 => {
            EcmaValue ta2 = TA2.Construct(ta1);
            That(ta2["buffer"]["constructor"], Is.EqualTo(ArrayBuffer));
          }, intView);
        }, intView);
      });

      It("should use prototype from new target if it's an Object", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue newTarget = Noop;
        EcmaValue proto = Object.Construct();
        newTarget["prototype"] = proto;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(buffer), newTarget);
          That(ta["constructor"], Is.EqualTo(Object));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(proto));
        });
      });

      It("should use prototype from %TypedArray% if newTarget's prototype is not an Object", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue newTarget = Noop;
        newTarget["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(buffer), newTarget);
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });
    }

    [Test]
    public void Constructor_Length() {
      It("should return abrupt completion getting newTarget's prototype", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => Reflect.Invoke("construct", TA, EcmaArray.Of(1), newTarget), Throws.Test262);
        });
      });

      It("should initialize all bytes to zero", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue subject = TA.Construct(9);
          That(subject[0], Is.EqualTo(0), "index 0");
          That(subject[1], Is.EqualTo(0), "index 1");
          That(subject[2], Is.EqualTo(0), "index 2");
          That(subject[3], Is.EqualTo(0), "index 3");
          That(subject[4], Is.EqualTo(0), "index 4");
          That(subject[5], Is.EqualTo(0), "index 5");
          That(subject[6], Is.EqualTo(0), "index 6");
          That(subject[7], Is.EqualTo(0), "index 7");
          That(subject[8], Is.EqualTo(0), "index 8");
        });
      });

      It("should throw a RangeError if length is a Infinity value", () => {
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(Infinity), Throws.RangeError);
        });
      });

      It("should throw a RangeError if ToInteger(length) is a negative value", () => {
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(-1), Throws.RangeError);
          That(() => TA.Construct(-Infinity), Throws.RangeError);
        });
      });

      It("should throw a TypeError exception if length is a Symbol", () => {
        EcmaValue s = new Symbol("1");
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(s), Throws.TypeError);
        });
      });

      It("should return an extensible new typedArray instance", () => {
        TestWithTypedArrayConstructors(TA => {
          That(Object.Invoke("isExtensible", TA.Construct(4)), Is.True);
        });
      });

      It("should derive default [[Prototype]] value from realm of the newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = Reflect.Invoke("construct", TA, EcmaArray.Of(0), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.Global)[TA["name"]]["prototype"]));
        });
      });

      It("should return a TypedArray object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = TA.Construct(4);
          That(ta["length"], Is.EqualTo(4), "ta1.length");
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should perform ToIndex(length) operations", () => {
        TestWithTypedArrayConstructors(TA => {
          That(TA.Construct(-0d)["length"], Is.EqualTo(0));
          That(TA.Construct("")["length"], Is.EqualTo(0));
          That(TA.Construct("0")["length"], Is.EqualTo(0));
          That(TA.Construct("1")["length"], Is.EqualTo(1));
          That(TA.Construct(false)["length"], Is.EqualTo(0));
          That(TA.Construct(true)["length"], Is.EqualTo(1));
          That(TA.Construct(NaN)["length"], Is.EqualTo(0));
          That(TA.Construct(Null)["length"], Is.EqualTo(0));
          That(TA.Construct(0.1)["length"], Is.EqualTo(0));
          That(TA.Construct(0.9)["length"], Is.EqualTo(0));
          That(TA.Construct(1.1)["length"], Is.EqualTo(1));
          That(TA.Construct(1.9)["length"], Is.EqualTo(1));
          That(TA.Construct(-0.1)["length"], Is.EqualTo(0));
          That(TA.Construct(-0.99999)["length"], Is.EqualTo(0));
        });
      });

      It("should throw a TypeError if NewTarget is undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Call(Undefined, 0), Throws.TypeError);
          That(() => TA.Call(Undefined, Infinity), Throws.TypeError);
        });
      });

      It("should use prototype from new target if it's an Object", () => {
        EcmaValue newTarget = Noop;
        EcmaValue proto = Object.Construct();
        newTarget["prototype"] = proto;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(1), newTarget);
          That(ta["constructor"], Is.EqualTo(Object));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(proto));
        });
      });

      It("should use prototype from %TypedArray% if newTarget's prototype is not an Object", () => {
        EcmaValue newTarget = Noop;
        newTarget["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(1), newTarget);
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });
    }

    [Test]
    public void Constructor_NoArgs() {
      It("should return abrupt completion getting newTarget's prototype", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => Reflect.Invoke("construct", TA, EcmaArray.Of(), newTarget), Throws.Test262);
        });
      });

      It("should return an extensible new typedArray instance", () => {
        TestWithTypedArrayConstructors(TA => {
          That(Object.Invoke("isExtensible", TA.Construct()), Is.True);
        });
      });

      It("should derive default [[Prototype]] value from realm of the newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = Reflect.Invoke("construct", TA, EcmaArray.Of(), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.Global)[TA["name"]]["prototype"]));
        });
      });

      It("should return a TypedArray object", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = TA.Construct();
          That(ta["length"], Is.EqualTo(0), "ta1.length");
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA["prototype"]));
        });
      });

      It("should throw a TypeError if NewTarget is undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Call(), Throws.TypeError);
        });
      });

      It("should use prototype from new target if it's an Object", () => {
        EcmaValue newTarget = Noop;
        EcmaValue proto = Object.Construct();
        newTarget["prototype"] = proto;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(), newTarget);
          That(ta["constructor"], Is.EqualTo(Object));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(proto));
        });
      });

      It("should use prototype from %TypedArray% if newTarget's prototype is not an Object", () => {
        EcmaValue newTarget = Noop;
        newTarget["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(), newTarget);
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });
    }

    [Test]
    public void Constructor_Object() {
      It("should return typedArray from array argument", () => {
        EcmaValue obj = EcmaArray.Of(7, 42);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(obj);
          That(typedArray, Is.EquivalentTo(new[] { 7, 42 }));
          That(typedArray["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", typedArray), Is.EqualTo(TA.Prototype));
        });
      });

      It("should produce consistent canonicalization of NaN values", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue first = TA.Construct(ByteConversionValues.NaNs);
          EcmaValue second = TA.Construct(ByteConversionValues.NaNs);
          EcmaValue firstBytes = Global.Uint8Array.Construct(first["buffer"]);
          EcmaValue secondBytes = Global.Uint8Array.Construct(second["buffer"]);
          for (int i = 0; i < firstBytes["length"].ToLength(); i++) {
            That(firstBytes[i], Is.EqualTo(secondBytes[i]));
          }
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should convert to correct values", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(value));
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("should return abrupt completion getting newTarget's prototype", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => Reflect.Invoke("construct", TA, EcmaArray.Of(Object.Construct()), newTarget), Throws.Test262);
        });
      });

      It("should return abrupt when object @@iterator is not callable", () => {
        EcmaValue obj = Noop;
        TestWithTypedArrayConstructors(TA => {
          obj[Symbol.Iterator] = Object.Construct();
          That(() => TA.Construct(obj), Throws.TypeError);
          obj[Symbol.Iterator] = true;
          That(() => TA.Construct(obj), Throws.TypeError);
          obj[Symbol.Iterator] = 42;
          That(() => TA.Construct(obj), Throws.TypeError);
        });
      });

      It("should return abrupt from getting object @@iterator", () => {
        EcmaValue obj = Noop;
        Object.Invoke("defineProperty", obj, Symbol.Iterator, CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj), Throws.Test262);
        });
      });

      It("should return abrupt from allocating array buffer with excessive length", () => {
        EcmaValue obj = CreateObject(new { length = Math.Invoke("pow", 2, 53) });
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj), Throws.RangeError);
        });
      });

      It("should return abrupt from length property as a Symbol on the object argument", () => {
        EcmaValue obj = CreateObject(new { length = new Symbol("1") });
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj), Throws.TypeError);
        });
      });

      It("should return abrupt from getting length property on the object argument", () => {
        EcmaValue obj = CreateObject(("length", get: ThrowTest262Exception, set: null));
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj), Throws.Test262);
        });
      });

      It("should return an extensible new typedArray instance", () => {
        TestWithTypedArrayConstructors(TA => {
          That(Object.Invoke("isExtensible", TA.Construct(Object.Construct())), Is.True);
        });
      });

      It("should derive default [[Prototype]] value from realm of the newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = Reflect.Invoke("construct", TA, EcmaArray.Of(Object.Construct()), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.Global)[TA["name"]]["prototype"]));
        });
      });

      It("should return typedArray from object argument", () => {
        EcmaValue obj = CreateObject(
          ("0", Null),
          ("2", 42),
          ("3", "7"),
          ("4", NaN),
          ("5", new Symbol("1")),
          ("length", 5)
        );
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(obj);
          That(typedArray["length"], Is.EqualTo(5));
          That(typedArray[0], Is.EqualTo(0));
          That(typedArray[2], Is.EqualTo(42));
          That(typedArray[3], Is.EqualTo(7));
          That(typedArray[5], Is.Undefined);
          That(typedArray["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", typedArray), Is.EqualTo(TA.Prototype));

          if (TA == Global.Float32Array || TA == Global.Float64Array) {
            That(typedArray[1], Is.NaN);
            That(typedArray[4], Is.NaN);
          } else {
            That(typedArray[1], Is.EqualTo(0));
            That(typedArray[4], Is.EqualTo(0));
          }
        });
      });

      It("should return abrupt from getting object property", () => {
        EcmaValue obj = CreateObject(new { length = 4 });
        Object.Invoke("defineProperty", obj, "2", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj), Throws.Test262);
        });
      });

      It("should return abrupt from setting property", () => {
        EcmaValue obj1 = CreateObject(
          ("2", new Symbol("1")),
          ("length", 4)
        );
        EcmaValue obj2 = CreateObject(
          ("2", CreateObject(valueOf: ThrowTest262Exception)),
          ("length", 4)
        );
        EcmaValue obj3 = CreateObject((Symbol.ToPrimitive, ThrowTest262Exception));
        EcmaValue obj4 = CreateObject((Symbol.ToPrimitive, RuntimeFunction.Create(() => Object.Construct())));
        EcmaValue obj5 = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue obj6 = CreateObject(toString: ThrowTest262Exception, valueOf: () => Object.Construct());
        EcmaValue obj7 = CreateObject(toString: () => Object.Construct(), valueOf: () => Object.Construct());
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Construct(obj1), Throws.TypeError);
          That(() => TA.Construct(obj2), Throws.Test262);
          That(() => TA.Construct(EcmaArray.Of(obj3)), Throws.Test262);
          That(() => TA.Construct(EcmaArray.Of(obj4)), Throws.TypeError);
          That(() => TA.Construct(EcmaArray.Of(obj5)), Throws.Test262);
          That(() => TA.Construct(EcmaArray.Of(obj6)), Throws.Test262);
          That(() => TA.Construct(EcmaArray.Of(obj7)), Throws.TypeError);
        });
      });

      It("should throw a TypeError if NewTarget is undefined", () => {
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Call(Undefined, EcmaArray.Of()), Throws.TypeError);
        });
      });

      It("should use prototype from new target if it's an Object", () => {
        EcmaValue newTarget = Noop;
        EcmaValue proto = Object.Construct();
        newTarget["prototype"] = proto;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(EcmaArray.Of()), newTarget);
          That(ta["constructor"], Is.EqualTo(Object));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(proto));
        });
      });

      It("should use prototype from %TypedArray% if newTarget's prototype is not an Object", () => {
        EcmaValue newTarget = Noop;
        newTarget["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = Reflect.Invoke("construct", TA, EcmaArray.Of(EcmaArray.Of()), newTarget);
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });
    }

    [Test]
    public void Constructor_TypedArray() {
      It("should return abrupt completion getting newTarget's prototype", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        EcmaValue sample = Global.Int8Array.Construct();
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        TestWithTypedArrayConstructors(TA => {
          That(() => Reflect.Invoke("construct", TA, EcmaArray.Of(sample), newTarget), Throws.Test262);
        });
      });

      It("should throw if TypedArray() is passed a detached buffer during GetSpeciesConstructor (other ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = TA.Construct(0);
          EcmaValue speciesConstructor = RuntimeFunction.Create(Noop).Bind(Null);
          int speciesCallCount = 0;
          int prototypeCallCount = 0;

          EcmaValue bufferConstructor = CreateObject((Symbol.Species, get: () => {
            speciesCallCount++;
            DetachBuffer(ta);
            return speciesConstructor;
          }, set: null));
          Object.Invoke("defineProperty", speciesConstructor, "prototype", CreateObject(new {
            get = RuntimeFunction.Create(() => Return(prototypeCallCount++, Null))
          }));
          ta["buffer"].ToObject()["constructor"] = bufferConstructor;

          That(() => (TA != Global.Int32Array ? Global.Int32Array : Global.Uint32Array).Construct(ta), Throws.TypeError);
          That(speciesCallCount, Is.EqualTo(1), "speciesCallCount should be 1 (other ctor)");
          That(prototypeCallCount, Is.EqualTo(1), "prototypeCallCount should be 1 (other ctor)");
        });
      });

      It("should throw if TypedArray() is passed a detached buffer during GetSpeciesConstructor (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ta = TA.Construct(0);
          EcmaValue speciesConstructor = RuntimeFunction.Create(Noop).Bind(Null);
          int speciesCallCount = 0;
          int prototypeCallCount = 0;

          EcmaValue bufferConstructor = CreateObject((Symbol.Species, get: () => {
            speciesCallCount++;
            DetachBuffer(ta);
            return speciesConstructor;
          }, set: null));
          Object.Invoke("defineProperty", speciesConstructor, "prototype", CreateObject(new {
            get = RuntimeFunction.Create(() => Return(prototypeCallCount++, Null))
          }));
          ta["buffer"].ToObject()["constructor"] = bufferConstructor;

          That(() => TA.Construct(ta), Throws.TypeError);
          That(speciesCallCount, Is.EqualTo(1), "speciesCallCount should be 1 (same ctor)");
          That(prototypeCallCount, Is.EqualTo(1), "prototypeCallCount should be 1 (same ctor)");
        });
      });

      It("should return an extensible new typedArray instance", () => {
        EcmaValue typedArraySample1 = Global.Int8Array.Construct();
        EcmaValue typedArraySample2 = Global.Int8Array.Construct();
        Object.Invoke("preventExtensions", typedArraySample2);
        TestWithTypedArrayConstructors(TA => {
          That(Object.Invoke("isExtensible", TA.Construct(typedArraySample1)), Is.True);
          That(Object.Invoke("isExtensible", TA.Construct(typedArraySample2)), Is.True);
        });
      });

      It("should return abrupt completion from getting typedArray argument's buffer.constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = (TA != Global.Int32Array ? Global.Int32Array : Global.Uint32Array).Construct();
          Object.Invoke("defineProperty", sample["buffer"], "constructor", CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
        });
      });

      It("should derive the ArrayBuffer prototype from realm of the newTarget (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = fn;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ArrayBufferPrototype)));
        });
      });

      It("should derive the ArrayBuffer prototype from realm of the newTarget (same ctor)", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = fn;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ArrayBufferPrototype)));
        });
      });

      It("should use default ArrayBuffer constructor on undefined buffer.constructor.@@species (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();
          EcmaValue custom = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;

          ctor[Symbol.Species] = Intercept(() => Undefined);
          ctor[Symbol.Species].ToObject()["prototype"] = custom;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(custom));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should use default ArrayBuffer constructor on undefined buffer.constructor.@@species (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();
          EcmaValue custom = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;

          ctor[Symbol.Species] = Intercept(() => Undefined);
          ctor[Symbol.Species].ToObject()["prototype"] = custom;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(custom));
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should return abrupt completion from typedArray argument's buffer.constructor's value (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;

          sample["buffer"].ToObject()["constructor"] = 1;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = true;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = "";
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = Null;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = new Symbol("1");
          That(() => TA.Construct(sample), Throws.TypeError);
        });
      });

      It("should return abrupt completion from typedArray argument's buffer.constructor's value (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();

          sample["buffer"].ToObject()["constructor"] = 1;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = true;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = "";
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = Null;
          That(() => TA.Construct(sample), Throws.TypeError);

          sample["buffer"].ToObject()["constructor"] = new Symbol("1");
          That(() => TA.Construct(sample), Throws.TypeError);
        });
      });

      It("should return abrupt from getting typedArray argument's buffer.constructor.@@species (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          Object.Invoke("defineProperty", ctor, Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
        });
      });

      It("should return abrupt from getting typedArray argument's buffer.constructor.@@species (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          Object.Invoke("defineProperty", ctor, Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
        });
      });

      It("should return abrupt from buffer.constructor.@@species not being constructor (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();
        EcmaValue ctor = ThrowTest262Exception;
        ctor[Symbol.Species] = Math["max"];

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          sample["buffer"].ToObject()["constructor"] = ctor;
          That(() => TA.Construct(sample), Throws.TypeError);
        });
      });

      It("should return abrupt from buffer.constructor.@@species not being constructor (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = Math["max"];
          That(() => TA.Construct(sample), Throws.TypeError);
        });
      });

      It("should use default ArrayBuffer constructor on null buffer.constructor.@@species (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = Null;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(ArrayBuffer.Prototype));
        });
      });

      It("should use default ArrayBuffer constructor on null buffer.constructor.@@species (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = Null;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(ArrayBuffer.Prototype));
        });
      });

      It("should use default ArrayBuffer constructor on undefined buffer.constructor.@@species (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = Undefined;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(ArrayBuffer.Prototype));
        });
      });

      It("should use default ArrayBuffer constructor on undefined buffer.constructor.@@species (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();

          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = Undefined;

          EcmaValue ta = TA.Construct(sample);
          That(Object.Invoke("getPrototypeOf", ta["buffer"]), Is.EqualTo(ArrayBuffer.Prototype));
        });
      });

      It("should return abrupt from buffer.constructor.@@species.prototype (other ctor)", () => {
        EcmaValue sample1 = Global.Int8Array.Construct();
        EcmaValue sample2 = Global.Int16Array.Construct();

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ctor = Object.Construct();

          EcmaValue species = RuntimeFunction.Create(Intercept(() => Undefined)).Bind(Null);
          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = species;

          Object.Invoke("defineProperty", species, "prototype", CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
          That(Logs.Count, Is.EqualTo(0));
        });
      });

      It("should return abrupt from buffer.constructor.@@species.prototype (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          EcmaValue ctor = Object.Construct();
          sample["buffer"].ToObject()["constructor"] = ctor;
          ctor[Symbol.Species] = RuntimeFunction.Create(Noop).Bind(Null);
          Object.Invoke("defineProperty", ctor[Symbol.Species], "prototype", CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
        });
      });

      It("should return new typedArray (other ctor)", () => {
        EcmaValue sample1 = new Int8Array(7);
        EcmaValue sample2 = new Int16Array(7);

        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA == Global.Int8Array ? sample2 : sample1;
          EcmaValue ta = TA.Construct(sample);

          That(ta["length"], Is.EqualTo(7));
          That(ta, Is.Not.EqualTo(sample));
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return new typedArray (same ctor)", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(7);
          EcmaValue ta = TA.Construct(sample);

          That(ta["length"], Is.EqualTo(7));
          That(ta, Is.Not.EqualTo(sample));
          That(ta["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", ta), Is.EqualTo(TA.Prototype));
        });
      });

      It("should derive default [[Prototype]] value from realm of the newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue other = Reflect.Invoke("construct", TA, EcmaArray.Of(TA.Construct()), fn);
          That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.Global)[TA["name"]]["prototype"]));
        });
      });

      It("should return abrupt completion from getting typedArray argument's buffer.constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Object.Invoke("defineProperty", sample["buffer"], "constructor", CreateObject(new { get = ThrowTest262Exception }));
          That(() => TA.Construct(sample), Throws.Test262);
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void From(RuntimeFunction from) {
      IsUnconstructableFunctionWLength(from, "from", 1);
      That(TypedArray, Has.OwnProperty("from", from, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
      });

      It("cannot be invoked as a method of %TypedArray%", () => {
        Case((_, EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError exception if this is not a constructor", () => {
        Case((from, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return error produced by accessing array-like's length (as a method of %TypedArray%)", () => {
        Case((_, CreateObject(("length", get: ThrowTest262Exception, set: null))), Throws.Test262);
        Case((_, CreateObject(new { length = CreateObject(valueOf: ThrowTest262Exception) })), Throws.Test262);
      });

      It("should return error produced by accessing array-like's length", () => {
        EcmaValue arrayLike1 = CreateObject(("length", get: ThrowTest262Exception, set: null));
        EcmaValue arrayLike2 = CreateObject(new { length = CreateObject(valueOf: ThrowTest262Exception) });
        TestWithTypedArrayConstructors(TA => {
          That(() => TA.Invoke("from", arrayLike1), Throws.Test262);
          That(() => TA.Invoke("from", arrayLike2), Throws.Test262);
        });
      });

      It("should return error produced by accessing @@iterator (as a method of %TypedArray%)", () => {
        Case((_, CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null))), Throws.Test262);
        Case((_, CreateObject((Symbol.Iterator, ThrowTest262Exception))), Throws.Test262);
      });

      It("should return error produced by accessing @@iterator", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA, CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null))), Throws.Test262);
          Case((TA, CreateObject((Symbol.Iterator, ThrowTest262Exception))), Throws.Test262);
        });
      });

      It("should return error produced by advancing the iterator (as a method of %TypedArray%)", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => {
          return CreateObject(new { next = ThrowTest262Exception });
        })));
        Case((_, iter), Throws.Test262);
      });

      It("should return error produced by advancing the iterator", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => {
          return CreateObject(new { next = ThrowTest262Exception });
        })));
        TestWithTypedArrayConstructors(TA => {
          Case((TA, iter), Throws.Test262);
        });
      });

      It("should return error produced by accessing iterated value (as a method of %TypedArray%)", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => {
          return CreateObject(new {
            next = RuntimeFunction.Create(() => {
              return CreateObject(("value", get: ThrowTest262Exception, set: null));
            })
          });
        })));
        Case((_, iter), Throws.Test262);
      });

      It("should return error produced by accessing iterated value", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, RuntimeFunction.Create(() => {
          return CreateObject(new {
            next = RuntimeFunction.Create(() => {
              return CreateObject(("value", get: ThrowTest262Exception, set: null));
            })
          });
        })));
        TestWithTypedArrayConstructors(TA => {
          Case((TA, iter), Throws.Test262);
        });
      });

      It("should throw a TypeError exception is mapfn is not callable (as a method of %TypedArray%)", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, get: Intercept(Noop), set: null));
        Case((_, iter, Null), Throws.TypeError);
        Case((_, iter, 42), Throws.TypeError);
        Case((_, iter, ""), Throws.TypeError);
        Case((_, iter, true), Throws.TypeError);
        Case((_, iter, Object.Construct()), Throws.TypeError);
        Case((_, iter, EcmaArray.Of()), Throws.TypeError);
        Case((_, iter, new Symbol("1")), Throws.TypeError);
        That(Logs.Count, Is.EqualTo(0), "IsCallable(mapfn) check occurs before getting source[@@iterator]");
      });

      It("should throw a TypeError exception is mapfn is not callable", () => {
        EcmaValue arrayLike = Object.Construct();
        Object.Invoke("defineProperty", arrayLike, Symbol.Iterator, CreateObject(new { get = Intercept(() => Undefined) }));
        TestWithTypedArrayConstructors(TA => {
          Case((TA, arrayLike, Null), Throws.TypeError);
          Case((TA, arrayLike, 42), Throws.TypeError);
          Case((TA, arrayLike, ""), Throws.TypeError);
          Case((TA, arrayLike, true), Throws.TypeError);
          Case((TA, arrayLike, Object.Construct()), Throws.TypeError);
          Case((TA, arrayLike, EcmaArray.Of()), Throws.TypeError);
          Case((TA, arrayLike, new Symbol("1")), Throws.TypeError);
        });
        That(Logs.Count, Is.EqualTo(0));
      });

      It("should return abrupt from mapfn", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA, CreateObject((0, 42), ("length", 2)), ThrowTest262Exception), Throws.Test262);
        });
      });

      It("should call mapfn with correct arguments", () => {
        EcmaValue source = EcmaArray.Of(42, 43, 44);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = EcmaArray.Of();
          EcmaValue mapfn = RuntimeFunction.Create((v, i) => {
            result.Invoke("push", EcmaArray.Of(v, i, Arguments["length"]));
          });
          TA.Invoke("from", source, mapfn);
          That(result, Is.EquivalentTo(new[] {
             new[] { 42, 0, 2 },
             new[] { 43, 1, 2 },
             new[] { 44, 2, 2 }
          }));
        });
      });

      It("should call mapfn with thisArg as the this value", () => {
        EcmaValue source = EcmaArray.Of(42, 43);
        EcmaValue thisArg = Object.Construct();
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = EcmaArray.Of();
          EcmaValue mapfn = RuntimeFunction.Create((v, i) => {
            result.Invoke("push", This);
          });
          TA.Invoke("from", source, mapfn, thisArg);
          That(result, Is.EquivalentTo(new[] { thisArg, thisArg }));
        });
      });

      It("should call mapfn with undefined as the this value", () => {
        EcmaValue source = EcmaArray.Of(42, 43);
        EcmaValue thisArg = Object.Construct();
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = EcmaArray.Of();
          EcmaValue mapfn = RuntimeFunction.Create((v, i) => {
            result.Invoke("push", This);
          });
          TA.Invoke("from", source, mapfn);
          That(result, Is.EquivalentTo(new[] { Undefined, Undefined }));
        });
      });

      It("should throw a TypeError if custom constructor did not return a TypedArray", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((RuntimeFunction.Create(Noop), EcmaArray.Of()), Throws.TypeError);
        });
      });

      It("does not throw if custom constructor returns TypedArray instance with higher or same length", () => {
        EcmaValue sourceItor = EcmaArray.Of(1, 2);
        EcmaValue sourceObj = CreateObject(new { length = 2 });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue custom = TA.Construct(2);
          EcmaValue ctor = RuntimeFunction.Create(() => custom);
          Case((ctor, sourceItor), Is.EqualTo(custom));
          Case((ctor, sourceObj), Is.EqualTo(custom));

          custom = TA.Construct(3);
          Case((ctor, sourceItor), Is.EqualTo(custom));
          Case((ctor, sourceObj), Is.EqualTo(custom));
        });
      });

      It("should throw a TypeError if a custom `this` returns a smaller instance", () => {
        EcmaValue sourceItor = EcmaArray.Of(1, 2);
        EcmaValue sourceObj = CreateObject(new { length = 2 });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue custom = TA.Construct(1);
          EcmaValue ctor = RuntimeFunction.Create(() => custom);
          Case((ctor, sourceItor), Throws.TypeError);
          Case((ctor, sourceObj), Throws.TypeError);
        });
      });

      It("should call and return abrupt completion from custom constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((ThrowTest262Exception, EcmaArray.Of()), Throws.Test262);
        });
      });

      It("is %TypedArray%.from", () => {
        TestWithTypedArrayConstructors(TA => {
          That(TA["from"], Is.EqualTo(from));
          That(TA.Invoke("hasOwnProperty", "from"), Is.False);
        });
      });

      It("should convert NaN and undefined correctly", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", EcmaArray.Of(NaN, Undefined));
          That(result["length"], Is.EqualTo(2));
          That(result[0], Is.NaN);
          That(result[1], Is.NaN);
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", EcmaArray.Of(NaN, Undefined));
          That(result["length"], Is.EqualTo(2));
          That(result[0], Is.EqualTo(0));
          That(result[1], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should set -0 and +0 correctly", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", EcmaArray.Of(0, -0d));
          That(result["length"], Is.EqualTo(2));
          That(result[0], Is.EqualTo(0));
          That(result[1], Is.EqualTo(-0d));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", EcmaArray.Of(0, -0d));
          That(result["length"], Is.EqualTo(2));
          That(result[0], Is.EqualTo(0));
          That(result[1], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should return a new empty TypedArray", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", EcmaArray.Of());
          That(result["length"], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return a new TypedArray from an ordinary object", () => {
        EcmaValue source = CreateObject((0, 42), (2, 44), ("length", 4));
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source);
          That(result["length"], Is.EqualTo(4));
          That(result[0], Is.EqualTo(42));
          That(result[1], Is.NaN);
          That(result[2], Is.EqualTo(44));
          That(result[3], Is.NaN);
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source);
          That(result["length"], Is.EqualTo(4));
          That(result[0], Is.EqualTo(42));
          That(result[1], Is.EqualTo(0));
          That(result[2], Is.EqualTo(44));
          That(result[3], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should return a new TypedArray from a sparse array", () => {
        EcmaValue source = EcmaArray.Of();
        source[2] = 42;
        source[4] = 44;
        source["length"] = 6;
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source);
          That(result["length"], Is.EqualTo(6));
          That(result[0], Is.NaN);
          That(result[1], Is.NaN);
          That(result[2], Is.EqualTo(42));
          That(result[3], Is.NaN);
          That(result[4], Is.EqualTo(44));
          That(result[5], Is.NaN);
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source);
          That(result["length"], Is.EqualTo(6));
          That(result[0], Is.EqualTo(0));
          That(result[1], Is.EqualTo(0));
          That(result[2], Is.EqualTo(42));
          That(result[3], Is.EqualTo(0));
          That(result[4], Is.EqualTo(44));
          That(result[5], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should return a new TypedArray", () => {
        EcmaValue source = EcmaArray.Of(42, 43, 42);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source);
          That(result["length"], Is.EqualTo(3));
          That(result[0], Is.EqualTo(42));
          That(result[1], Is.EqualTo(43));
          That(result[2], Is.EqualTo(42));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return a new TypedArray using mapfn", () => {
        EcmaValue source = EcmaArray.Of(42, 43, 42);
        EcmaValue mapFn = RuntimeFunction.Create(v => v * 2);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("from", source, mapFn);
          That(result["length"], Is.EqualTo(3));
          That(result[0], Is.EqualTo(84));
          That(result[1], Is.EqualTo(86));
          That(result[2], Is.EqualTo(84));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return a new TypedArray using a custom Constructor", () => {
        EcmaValue source = EcmaArray.Of(42, 43, 42);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ctor = Intercept(len => {
            That(Arguments["length"], Is.EqualTo(1));
            return TA.Construct(len);
          });
          Logs.Clear();
          EcmaValue result = from.Call(ctor, source);
          That(result["length"], Is.EqualTo(3));
          That(result[0], Is.EqualTo(42));
          That(result[1], Is.EqualTo(43));
          That(result[2], Is.EqualTo(42));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from ToNumber", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA, EcmaArray.Of(new Symbol())), Throws.TypeError);

          EcmaValue source = CreateObject(new { length = 2 });
          Object.Invoke("defineProperty", source, "0", CreateObject(new { get = ThrowTest262Exception }));
          Case((TA, source), Throws.Test262);

          EcmaValue lastValue = default;
          EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
          EcmaValue mapfn = RuntimeFunction.Create(v => Return(lastValue = v, v));
          Case((TA, EcmaArray.Of(42, obj, 1), mapfn), Throws.Test262);
          That(lastValue, Is.EqualTo(obj));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Of(RuntimeFunction of) {
      IsUnconstructableFunctionWLength(of, "of", 0);
      That(TypedArray, Has.OwnProperty("of", of, EcmaPropertyAttributes.DefaultMethodProperty));

      It("cannot be invoked as a function", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
      });

      It("cannot be invoked as a method of %TypedArray%", () => {
        Case((_, EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError exception if this is not a constructor", () => {
        Case((of, EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if custom constructor did not return a TypedArray", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((RuntimeFunction.Create(Noop), 42), Throws.TypeError);
        });
      });

      It("does not throw if custom constructor returns TypedArray instance with higher or same length", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue custom = TA.Construct(3);
          EcmaValue ctor = RuntimeFunction.Create(() => custom);
          Case((ctor, 1, 2, 3), Is.EqualTo(custom));
          Case((ctor, 1, 2), Is.EqualTo(custom));
        });
      });

      It("should throw a TypeError if a custom `this` returns a smaller instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue custom = TA.Construct(1);
          EcmaValue ctor = RuntimeFunction.Create(() => custom);
          Case((ctor, 1, 2), Throws.TypeError);
        });
      });


      It("should call and return abrupt completion from custom constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((ThrowTest262Exception, 42), Throws.Test262);
        });
      });

      It("is %TypedArray%.of", () => {
        TestWithTypedArrayConstructors(TA => {
          That(TA["of"], Is.EqualTo(of));
          That(TA.Invoke("hasOwnProperty", "of"), Is.False);
        });
      });

      It("should convert NaN and undefined correctly", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("of", NaN, Undefined);
          That(result, Is.EquivalentTo(new[] { NaN, NaN }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("of", NaN, Undefined);
          That(result, Is.EquivalentTo(new[] { 0, 0 }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should set -0 and +0 correctly", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("of", 0, -0d);
          That(result, Is.EquivalentTo(new[] { 0, -0d }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Float32Array, Global.Float64Array });

        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("of", 0, -0d);
          That(result, Is.EquivalentTo(new[] { 0, 0 }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        }, new[] { Global.Int8Array, Global.Int32Array, Global.Int16Array, Global.Int8Array, Global.Uint32Array, Global.Uint16Array, Global.Uint8Array, Global.Uint8ClampedArray });
      });

      It("should return a new empty TypedArray", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = TA.Invoke("of");
          That(result["length"], Is.EqualTo(0));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return a new TypedArray", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue result = of.Call(TA, 42, 43, Null);
          That(result, Is.EquivalentTo(new[] { 42, 43, 0 }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
        });
      });

      It("should return a new TypedArray using a custom Constructor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue ctor = Intercept(len => {
            That(Arguments["length"], Is.EqualTo(1));
            return TA.Construct(len);
          });
          EcmaValue result = of.Call(ctor, 42, 43, 42);
          That(result, Is.EquivalentTo(new[] { 42, 43, 42 }));
          That(result["constructor"], Is.EqualTo(TA));
          That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(TA.Prototype));
          That(Logs.Count, Is.EqualTo(1));
        });
      });

      It("should return abrupt from ToNumber", () => {
        TestWithTypedArrayConstructors(TA => {
          Case((TA, new Symbol("1")), Throws.TypeError);

          EcmaValue obj1 = CreateObject(valueOf: Intercept(() => 42, "obj1"));
          EcmaValue obj2 = CreateObject(valueOf: Intercept(ThrowTest262Exception, "obj2"));
          Case((TA, obj1, obj2, obj1), Throws.Test262);
          That(Logs, Is.EquivalentTo(new[] { "obj1", "obj2" }));
        });
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(TypedArray, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(TypedArray.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(TypedArray, Is.EqualTo(TypedArray));

      EcmaValue thisValue = new EcmaObject();
      Case(thisValue, Is.EqualTo(thisValue));
    }
  }
}
