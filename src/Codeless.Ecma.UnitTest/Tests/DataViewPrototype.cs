using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class DataViewPrototype : TestBase {
    RuntimeFunction ArrayBuffer => Global.ArrayBuffer;
    RuntimeFunction DataView => Global.DataView;

    [Test]
    public void Properties() {
      That(DataView, Has.OwnProperty("prototype", DataView.Prototype, EcmaPropertyAttributes.None));
      That(DataView.Prototype, Has.OwnProperty("constructor", DataView, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(DataView.Prototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "DataView", EcmaPropertyAttributes.Configurable));
      That(DataView.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(DataView.Prototype), Is.EqualTo("[object DataView]"));
    }

    [Test, RuntimeFunctionInjection]
    public void Buffer(RuntimeFunction buffer) {
      IsUnconstructableFunctionWLength(buffer, "get buffer", 0);
      That(DataView.Prototype, Has.OwnProperty("buffer", EcmaPropertyAttributes.Configurable));
      That(DataView.Prototype.GetOwnProperty("buffer").Set, Is.Undefined);

      It("should return buffer from [[ViewedArrayBuffer]] internal slot", () => {
        EcmaValue buf = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buf, 0);
        Case(dv, buf);
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("does not throw with a detached buffer", () => {
        EcmaValue buf = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buf, 0);
        DetachBuffer(buf);
        Case(dv, buf);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ByteLength(RuntimeFunction byteLength) {
      IsUnconstructableFunctionWLength(byteLength, "get byteLength", 0);
      That(DataView.Prototype, Has.OwnProperty("byteLength", EcmaPropertyAttributes.Configurable));
      That(DataView.Prototype.GetOwnProperty("byteLength").Set, Is.Undefined);

      It("should return value from [[ByteLength]] internal slot", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case(DataView.Construct(buffer, 0), 12);
        Case(DataView.Construct(buffer, 4), 8);
        Case(DataView.Construct(buffer, 6, 4), 4);
        Case(DataView.Construct(buffer, 12), 0);
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case(dv, Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void ByteOffset(RuntimeFunction byteOffset) {
      IsUnconstructableFunctionWLength(byteOffset, "get byteOffset", 0);
      That(DataView.Prototype, Has.OwnProperty("byteOffset", EcmaPropertyAttributes.Configurable));
      That(DataView.Prototype.GetOwnProperty("byteOffset").Set, Is.Undefined);

      It("should return value from [[ByteOffset]] internal slot", () => {
        var buffer = ArrayBuffer.Construct(12);

        var sample1 = DataView.Construct(buffer, 0);
        var sample2 = DataView.Construct(buffer, 4);
        var sample3 = DataView.Construct(buffer, 6, 4);
        var sample4 = DataView.Construct(buffer, 12);
        var sample5 = DataView.Construct(buffer, 0, 2);

        Case(sample1, 0);
        Case(sample2, 4);
        Case(sample3, 6);
        Case(sample4, 12);
        Case(sample5, 0);
      });

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case(dv, Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetFloat32(RuntimeFunction getFloat32) {
      IsUnconstructableFunctionWLength(getFloat32, "getFloat32", 1);
      That(DataView.Prototype, Has.OwnProperty("getFloat32", getFloat32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 32-bit float value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 128);
        dv.Invoke("setUint8", 1, 0);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        Case((dv, 0), -0d);

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 128);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 255);
        dv.Invoke("setUint8", 5, 128);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Infinity);
        Case((dv, 4), -Infinity);

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 192);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 255);
        dv.Invoke("setUint8", 5, 192);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), NaN);
        Case((dv, 4), NaN);

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getFloat32(0, true)");
        Case((dv, 1, true), 0, "sample.getFloat32(1, true)");
        Case((dv, 2, true), 0, "sample.getFloat32(2, true)");
        Case((dv, 3, true), 0, "sample.getFloat32(3, true)");
        Case((dv, 4, true), 0, "sample.getFloat32(4, true)");
        Case((dv, 0, false), 0, "sample.getFloat32(0, false)");
        Case((dv, 1, false), 0, "sample.getFloat32(1, false)");
        Case((dv, 2, false), 0, "sample.getFloat32(2, false)");
        Case((dv, 3, false), 0, "sample.getFloat32(3, false)");
        Case((dv, 4, false), 0, "sample.getFloat32(4, false)");

        buffer = ArrayBuffer.Construct(12);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 4, 75);
        dv.Invoke("setUint8", 5, 75);
        dv.Invoke("setUint8", 6, 75);
        dv.Invoke("setUint8", 7, 75);
        dv.Invoke("setUint8", 8, 76);
        dv.Invoke("setUint8", 9, 76);
        dv.Invoke("setUint8", 10, 77);
        dv.Invoke("setUint8", 11, 77);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), 13323083, "0, false");
        Case((dv, 1, false), 13323084, "1, false");
        Case((dv, 2, false), 13323340, "2, false");
        Case((dv, 3, false), 13388877, "3, false");
        Case((dv, 4, false), 53556532, "4, false");
        Case((dv, 0, true), 13323083, "0, true");
        Case((dv, 1, true), 53292332, "1, true");
        Case((dv, 2, true), 53554476, "2, true");
        Case((dv, 3, true), 214222000, "3, true");
        Case((dv, 4, true), 215270592, "4, true");

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 66);
        dv.Invoke("setUint8", 1, 40);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 64);
        dv.Invoke("setUint8", 5, 224);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);

        Case((dv, 0, false), 42, "0, false");
        Case((dv, 1, false), 7.105481567709626e-15, "1, false");
        Case((dv, 2, false), 2.327276489550656e-41, "2, false");
        Case((dv, 3, false), 5.95782781324968e-39, "3, false");
        Case((dv, 4, false), 7, "4, false");

        Case((dv, 0, true), 1.4441781973331565e-41, "0, true");
        Case((dv, 1, true), 2.000009536743164, "1, true");
        Case((dv, 2, true), -55340232221128655000d, "2, true");
        Case((dv, 3, true), 2.059411001342953e-38, "3, true");
        Case((dv, 4, true), 8.04457422399591e-41, "4, true");

        dv.Invoke("setUint8", 0, 75);
        dv.Invoke("setUint8", 1, 75);
        dv.Invoke("setUint8", 2, 76);
        dv.Invoke("setUint8", 3, 76);
        dv.Invoke("setUint8", 4, 75);
        dv.Invoke("setUint8", 5, 75);
        dv.Invoke("setUint8", 6, 76);
        dv.Invoke("setUint8", 7, 76);

        Case((dv, 0, false), 13323340, "0, false");
        Case((dv, 1, false), 13388875, "1, false");
        Case((dv, 2, false), 53554476, "2, false");
        Case((dv, 3, false), 53292336, "3, false");
        Case((dv, 4, false), 13323340, "4, false");
        Case((dv, 0, true), 53554476, "0, true");
        Case((dv, 1, true), 13388875, "1, true");
        Case((dv, 2, true), 13323340, "2, true");
        Case((dv, 3, true), 53292336, "3, true");
        Case((dv, 4, true), 53554476, "4, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 75);
        dv.Invoke("setUint8", 1, 75);
        dv.Invoke("setUint8", 2, 76);
        dv.Invoke("setUint8", 3, 76);

        // False
        Case((dv, 0), 13323340, "no arg");
        Case((dv, 0, Undefined), 13323340, "undefined");
        Case((dv, 0, Null), 13323340, "null");
        Case((dv, 0, 0), 13323340, "0");
        Case((dv, 0, ""), 13323340, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 53554476, "{}");
        Case((dv, 0, new Symbol("1")), 53554476, "symbol");
        Case((dv, 0, 1), 53554476, "1");
        Case((dv, 0, "string"), 53554476, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 75);
        dv.Invoke("setUint8", 1, 75);
        dv.Invoke("setUint8", 2, 76);
        dv.Invoke("setUint8", 3, 76);
        dv.Invoke("setUint8", 4, 75);
        dv.Invoke("setUint8", 5, 75);
        dv.Invoke("setUint8", 6, 76);
        dv.Invoke("setUint8", 7, 76);

        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 2);
        Case((dv, -0d), 13323340, "-0");
        Case((dv, obj1), 53292336, "object's valueOf");
        Case((dv, obj2), 53554476, "object's toString");
        Case((dv, ""), 13323340, "the Empty string");
        Case((dv, "0"), 13323340, "string '0'");
        Case((dv, "2"), 53554476, "string '2'");
        Case((dv, true), 13388875, "true");
        Case((dv, false), 13323340, "false");
        Case((dv, NaN), 13323340, "NaN");
        Case((dv, Null), 13323340, "null");
        Case((dv, 0.1), 13323340, "0.1");
        Case((dv, 0.9), 13323340, "0.9");
        Case((dv, 1.1), 13388875, "1.1");
        Case((dv, 1.9), 13388875, "1.9");
        Case((dv, -0.1), 13323340, "-0.1");
        Case((dv, -0.99999), 13323340, "-0.99999");
        Case((dv, Undefined), 13323340, "undefined");
        Case(dv, 13323340, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetFloat64(RuntimeFunction getFloat64) {
      IsUnconstructableFunctionWLength(getFloat64, "getFloat64", 1);
      That(DataView.Prototype, Has.OwnProperty("getFloat64", getFloat64, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 8), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 7), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 6), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 5), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 7), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 7), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 64-bit float value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 128);
        dv.Invoke("setUint8", 1, 0);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), -0d);

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 240);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Infinity);

        dv.Invoke("setUint8", 0, 255);
        dv.Invoke("setUint8", 1, 240);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), -Infinity);

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 248);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Is.NaN, "127, 248, 0, ...");

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 249);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Is.NaN, "127, 249, 0, ...");

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 250);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Is.NaN, "127, 250, 0, ...");

        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 251);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        Case((dv, 0), Is.NaN, "127, 251, 0, ...");

        buffer = ArrayBuffer.Construct(12);
        dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getFloat64(0, true)");
        Case((dv, 1, true), 0, "sample.getFloat64(1, true)");
        Case((dv, 2, true), 0, "sample.getFloat64(2, true)");
        Case((dv, 3, true), 0, "sample.getFloat64(3, true)");
        Case((dv, 4, true), 0, "sample.getFloat64(4, true)");
        Case((dv, 0, false), 0, "sample.getFloat64(0, false)");
        Case((dv, 1, false), 0, "sample.getFloat64(1, false)");
        Case((dv, 2, false), 0, "sample.getFloat64(2, false)");
        Case((dv, 3, false), 0, "sample.getFloat64(3, false)");
        Case((dv, 4, false), 0, "sample.getFloat64(4, false)");

        buffer = ArrayBuffer.Construct(16);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 4, 67);
        dv.Invoke("setUint8", 5, 67);
        dv.Invoke("setUint8", 6, 68);
        dv.Invoke("setUint8", 7, 68);
        dv.Invoke("setUint8", 8, 67);
        dv.Invoke("setUint8", 9, 67);
        dv.Invoke("setUint8", 10, 68);
        dv.Invoke("setUint8", 11, 68);
        dv.Invoke("setUint8", 12, 67);
        dv.Invoke("setUint8", 13, 67);
        dv.Invoke("setUint8", 14, 68);
        dv.Invoke("setUint8", 15, 68);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), 10846169068898440, "0");
        Case((dv, 1, false), 11409110432516230, "1");
        Case((dv, 2, false), 747563348316297500000d, "2");
        Case((dv, 3, false), 710670423110242000000d, "3");
        Case((dv, 4, false), 10846169068898440, "4");
        Case((dv, 0, true), 747563348316297500000d, "0, true");
        Case((dv, 1, true), 11409110432516230, "1, true");
        Case((dv, 2, true), 10846169068898440, "2, true");
        Case((dv, 3, true), 710670423110242000000d, "3, true");
        Case((dv, 4, true), 747563348316297500000d, "4, true");

        buffer = ArrayBuffer.Construct(16);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 67);
        dv.Invoke("setUint8", 1, 67);
        dv.Invoke("setUint8", 2, 68);
        dv.Invoke("setUint8", 3, 68);
        dv.Invoke("setUint8", 4, 67);
        dv.Invoke("setUint8", 5, 67);
        dv.Invoke("setUint8", 6, 68);
        dv.Invoke("setUint8", 7, 68);
        dv.Invoke("setUint8", 8, 67);
        dv.Invoke("setUint8", 9, 67);
        dv.Invoke("setUint8", 10, 68);
        dv.Invoke("setUint8", 11, 68);
        dv.Invoke("setUint8", 12, 0);
        dv.Invoke("setUint8", 13, 0);
        dv.Invoke("setUint8", 14, 0);
        dv.Invoke("setUint8", 15, 0);

        Case((dv, 0, false), 10846169068898440, "0, false");
        Case((dv, 1, false), 11409110432516230, "1, false");
        Case((dv, 2, false), 747563348316297500000d, "2, false");
        Case((dv, 3, false), 710670423110242000000d, "3, false");
        Case((dv, 4, false), 10846169068898440, "4, false");
        Case((dv, 5, false), 11409110432516096, "5, false");
        Case((dv, 6, false), 747563348314040600000d, "6, false");
        Case((dv, 7, false), 710670422532459300000d, "7, false");
        Case((dv, 8, false), 10846166811934720, "8, false");

        Case((dv, 0, true), 747563348316297500000d, "0, true");
        Case((dv, 1, true), 11409110432516230, "1, true");
        Case((dv, 2, true), 10846169068898440, "2, true");
        Case((dv, 3, true), 710670423110242000000d, "3, true");
        Case((dv, 4, true), 747563348316297500000d, "4, true");
        Case((dv, 5, true), 2.254739805726094e-307, "5, true");
        Case((dv, 6, true), 3.7084555987028e-310, "6, true");
        Case((dv, 7, true), 1.44861546824e-312, "7, true");
        Case((dv, 8, true), 5.65865417e-315, "8, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 67);
        dv.Invoke("setUint8", 1, 17);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 0);
        dv.Invoke("setUint8", 4, 0);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 20);
        dv.Invoke("setUint8", 7, 68);

        // False
        Case((dv, 0), 1196268651021585, "no arg");
        Case((dv, 0, Undefined), 1196268651021585, "undefined");
        Case((dv, 0, Null), 1196268651021585, "null");
        Case((dv, 0, 0), 1196268651021585, "0");
        Case((dv, 0, ""), 1196268651021585, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 92233720368620160000d, "{}");
        Case((dv, 0, new Symbol("1")), 92233720368620160000d, "symbol");
        Case((dv, 0, 1), 92233720368620160000d, "1");
        Case((dv, 0, "string"), 92233720368620160000d, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 67);
        dv.Invoke("setUint8", 1, 67);
        dv.Invoke("setUint8", 2, 68);
        dv.Invoke("setUint8", 3, 68);
        dv.Invoke("setUint8", 4, 67);
        dv.Invoke("setUint8", 5, 67);
        dv.Invoke("setUint8", 6, 68);
        dv.Invoke("setUint8", 7, 68);
        dv.Invoke("setUint8", 8, 67);
        dv.Invoke("setUint8", 9, 68);
        dv.Invoke("setUint8", 10, 68);
        dv.Invoke("setUint8", 11, 68);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0d), 10846169068898440, "-0");
        Case((dv, obj1), 747563348316297600000d, "{}.valueOf");
        Case((dv, obj2), 710670423110275600000d, "{}.toString");
        Case((dv, ""), 10846169068898440, "the Empty string");
        Case((dv, "0"), 10846169068898440, "string '0'");
        Case((dv, "2"), 747563348316297600000d, "string '2'");
        Case((dv, true), 11409110432516230, "true");
        Case((dv, false), 10846169068898440, "false");
        Case((dv, NaN), 10846169068898440, "NaN");
        Case((dv, Null), 10846169068898440, "null");
        Case((dv, 0.1), 10846169068898440, "0.1");
        Case((dv, 0.9), 10846169068898440, "0.9");
        Case((dv, 1.1), 11409110432516230, "1.1");
        Case((dv, 1.9), 11409110432516230, "1.9");
        Case((dv, -0.1), 10846169068898440, "-0.1");
        Case((dv, -0.99999), 10846169068898440, "-0.99999");
        Case((dv, Undefined), 10846169068898440, "undefined");
        Case(dv, 10846169068898440, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetInt16(RuntimeFunction getInt16) {
      IsUnconstructableFunctionWLength(getInt16, "getInt16", 1);
      That(DataView.Prototype, Has.OwnProperty("getInt16", getInt16, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 10), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 2), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 2), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 16-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getInt16(0, true)");
        Case((dv, 1, true), 0, "sample.getInt16(1, true)");
        Case((dv, 2, true), 0, "sample.getInt16(2, true)");
        Case((dv, 3, true), 0, "sample.getInt16(3, true)");
        Case((dv, 4, true), 0, "sample.getInt16(4, true)");
        Case((dv, 0, false), 0, "sample.getInt16(0, false)");
        Case((dv, 1, false), 0, "sample.getInt16(1, false)");
        Case((dv, 2, false), 0, "sample.getInt16(2, false)");
        Case((dv, 3, false), 0, "sample.getInt16(3, false)");
        Case((dv, 4, false), 0, "sample.getInt16(4, false)");

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 2);
        dv.Invoke("setUint8", 2, 6);
        dv.Invoke("setUint8", 3, 2);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 42);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 39);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), -32726, "0, false");
        Case((dv, 1, false), 10880, "1, false");
        Case((dv, 2, false), -32729, "2, false");
        Case((dv, 0, true), 10880, "0, true");
        Case((dv, 1, true), -32726, "1, true");
        Case((dv, 2, true), 10112, "2, true");

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 1);
        dv.Invoke("setUint8", 7, 0);

        Case((dv, 0, false), 32767, "0, false");
        Case((dv, 1, false), -1, "1, false");
        Case((dv, 2, false), -1, "2, false");
        Case((dv, 3, false), -128, "3, false");
        Case((dv, 4, false), -32768, "4, false");
        Case((dv, 5, false), 1, "5, false");
        Case((dv, 6, false), 256, "8, false");

        Case((dv, 0, true), -129, "0, true");
        Case((dv, 1, true), -1, "1, true");
        Case((dv, 2, true), -1, "2, true");
        Case((dv, 3, true), -32513, "3, true");
        Case((dv, 4, true), 128, "4, true");
        Case((dv, 5, true), 256, "5, true");
        Case((dv, 6, true), 1, "6, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(2);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 42);

        // False
        Case((dv, 0), 42, "no arg");
        Case((dv, 0, Undefined), 42, "undefined");
        Case((dv, 0, Null), 42, "null");
        Case((dv, 0, 0), 42, "0");
        Case((dv, 0, ""), 42, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 10752, "{}");
        Case((dv, 0, new Symbol("1")), 10752, "symbol");
        Case((dv, 0, 1), 10752, "1");
        Case((dv, 0, "string"), 10752, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 1);
        dv.Invoke("setUint8", 3, 127);
        dv.Invoke("setUint8", 4, 255);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0d), 32767, "-0");
        Case((dv, obj1), 383, "object's valueOf");
        Case((dv, obj2), 32767, "object's toString");
        Case((dv, ""), 32767, "the Empty string");
        Case((dv, "0"), 32767, "string '0'");
        Case((dv, "2"), 383, "string '2'");
        Case((dv, true), -255, "true");
        Case((dv, false), 32767, "false");
        Case((dv, NaN), 32767, "NaN");
        Case((dv, Null), 32767, "null");
        Case((dv, 0.1), 32767, "0.1");
        Case((dv, 0.9), 32767, "0.9");
        Case((dv, 1.1), -255, "1.1");
        Case((dv, 1.9), -255, "1.9");
        Case((dv, -0.1), 32767, "-0.1");
        Case((dv, -0.99999), 32767, "-0.99999");
        Case((dv, Undefined), 32767, "undefined");
        Case(dv, 32767, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetInt32(RuntimeFunction getInt32) {
      IsUnconstructableFunctionWLength(getInt32, "getInt32", 1);
      That(DataView.Prototype, Has.OwnProperty("getInt32", getInt32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 32-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getInt32(0, true)");
        Case((dv, 1, true), 0, "sample.getInt32(1, true)");
        Case((dv, 2, true), 0, "sample.getInt32(2, true)");
        Case((dv, 3, true), 0, "sample.getInt32(3, true)");
        Case((dv, 4, true), 0, "sample.getInt32(4, true)");
        Case((dv, 0, false), 0, "sample.getInt32(0, false)");
        Case((dv, 1, false), 0, "sample.getInt32(1, false)");
        Case((dv, 2, false), 0, "sample.getInt32(2, false)");
        Case((dv, 3, false), 0, "sample.getInt32(3, false)");
        Case((dv, 4, false), 0, "sample.getInt32(4, false)");

        buffer = ArrayBuffer.Construct(12);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 39);
        dv.Invoke("setUint8", 1, 2);
        dv.Invoke("setUint8", 2, 6);
        dv.Invoke("setUint8", 3, 2);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 1);
        dv.Invoke("setUint8", 8, 127);
        dv.Invoke("setUint8", 9, 0);
        dv.Invoke("setUint8", 10, 127);
        dv.Invoke("setUint8", 11, 1);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), -2147450879, "0, false");
        Case((dv, 1, false), 8388991, "1, false");
        Case((dv, 2, false), -2147385600, "2, false");
        Case((dv, 3, false), 25100415, "3, false");
        Case((dv, 0, true), 25165952, "0, true");
        Case((dv, 1, true), 2130804736, "1, true");
        Case((dv, 2, true), 8323456, "2, true");
        Case((dv, 3, true), 2130738945, "3, true");

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        dv.Invoke("setUint8", 8, 1);
        dv.Invoke("setUint8", 9, 0);
        dv.Invoke("setUint8", 10, 0);
        dv.Invoke("setUint8", 11, 0);

        Case((dv, 0, false), 2147483647, "0, false"); // 2**32-1
        Case((dv, 1, false), -128, "1, false");
        Case((dv, 2, false), -32768, "2, false");
        Case((dv, 3, false), -8388608, "3, false");
        Case((dv, 4, false), -2147483648, "4, false");
        Case((dv, 5, false), 1, "5, false");
        Case((dv, 6, false), 256, "6, false");
        Case((dv, 7, false), 65536, "7, false");
        Case((dv, 8, false), 16777216, "8, false");

        Case((dv, 0, true), -129, "0, true");
        Case((dv, 1, true), -2130706433, "1, true");
        Case((dv, 2, true), 8454143, "2, true");
        Case((dv, 3, true), 33023, "3, true");
        Case((dv, 4, true), 128, "4, true");
        Case((dv, 5, true), 16777216, "5, true");
        Case((dv, 6, true), 65536, "6, true");
        Case((dv, 7, true), 256, "7, true");
        Case((dv, 8, true), 1, "8, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 17);
        dv.Invoke("setUint8", 2, 4);
        dv.Invoke("setUint8", 3, 0);

        // False
        Case((dv, 0), 1115136, "no arg");
        Case((dv, 0, Undefined), 1115136, "undefined");
        Case((dv, 0, Null), 1115136, "null");
        Case((dv, 0, 0), 1115136, "0");
        Case((dv, 0, ""), 1115136, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 266496, "{}");
        Case((dv, 0, new Symbol("1")), 266496, "symbol");
        Case((dv, 0, 1), 266496, "1");
        Case((dv, 0, "string"), 266496, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 255);
        dv.Invoke("setUint8", 6, 255);
        dv.Invoke("setUint8", 7, 255);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0d), 2147483647, "-0");
        Case((dv, obj1), -32513, "object's valueOf");
        Case((dv, obj2), -8323073, "object's toString");
        Case((dv, ""), 2147483647, "the Empty string");
        Case((dv, "0"), 2147483647, "string '0'");
        Case((dv, "2"), -32513, "string '2'");
        Case((dv, true), -128, "true");
        Case((dv, false), 2147483647, "false");
        Case((dv, NaN), 2147483647, "NaN");
        Case((dv, Null), 2147483647, "null");
        Case((dv, 0.1), 2147483647, "0.1");
        Case((dv, 0.9), 2147483647, "0.9");
        Case((dv, 1.1), -128, "1.1");
        Case((dv, 1.9), -128, "1.9");
        Case((dv, -0.1), 2147483647, "-0.1");
        Case((dv, -0.99999), 2147483647, "-0.99999");
        Case((dv, Undefined), 2147483647, "undefined");
        Case((dv), 2147483647, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetInt8(RuntimeFunction getInt8) {
      IsUnconstructableFunctionWLength(getInt8, "getInt8", 1);
      That(DataView.Prototype, Has.OwnProperty("getInt8", getInt8, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 0), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 8-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), 0, "sample.getInt8(0)");
        Case((dv, 1), 0, "sample.getInt8(1)");
        Case((dv, 2), 0, "sample.getInt8(2)");
        Case((dv, 3), 0, "sample.getInt8(3)");

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 7);
        dv.Invoke("setUint8", 1, 7);
        dv.Invoke("setUint8", 2, 7);
        dv.Invoke("setUint8", 3, 7);
        dv.Invoke("setUint8", 4, 1);
        dv.Invoke("setUint8", 5, 127);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 255);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0), 1);
        Case((dv, 1), 127);
        Case((dv, 2), -128);
        Case((dv, 3), -1);

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 1);

        Case((dv, 0), 127);
        Case((dv, 1), -1);
        Case((dv, 2), 0);
        Case((dv, 3), 1);
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 39);
        dv.Invoke("setUint8", 1, 42);
        dv.Invoke("setUint8", 2, 7);
        dv.Invoke("setUint8", 3, 77);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0d), 39, "-0");
        Case((dv, obj1), 7, "object's valueOf");
        Case((dv, obj2), 77, "object's toString");
        Case((dv, ""), 39, "the Empty string");
        Case((dv, "0"), 39, "string '0'");
        Case((dv, "2"), 7, "string '2'");
        Case((dv, true), 42, "true");
        Case((dv, false), 39, "false");
        Case((dv, NaN), 39, "NaN");
        Case((dv, Null), 39, "null");
        Case((dv, 0.1), 39, "0.1");
        Case((dv, 0.9), 39, "0.9");
        Case((dv, 1.1), 42, "1.1");
        Case((dv, 1.9), 42, "1.9");
        Case((dv, -0.1), 39, "-0.1");
        Case((dv, -0.99999), 39, "-0.99999");
        Case((dv, Undefined), 39, "undefined");
        Case((dv), 39, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetUint16(RuntimeFunction getUint16) {
      IsUnconstructableFunctionWLength(getUint16, "getUint16", 1);
      That(DataView.Prototype, Has.OwnProperty("getUint16", getUint16, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 10), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 2), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 2), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 16-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getUint16(0, true)");
        Case((dv, 1, true), 0, "sample.getUint16(1, true)");
        Case((dv, 2, true), 0, "sample.getUint16(2, true)");
        Case((dv, 3, true), 0, "sample.getUint16(3, true)");
        Case((dv, 4, true), 0, "sample.getUint16(4, true)");
        Case((dv, 0, false), 0, "sample.getUint16(0, false)");
        Case((dv, 1, false), 0, "sample.getUint16(1, false)");
        Case((dv, 2, false), 0, "sample.getUint16(2, false)");
        Case((dv, 3, false), 0, "sample.getUint16(3, false)");
        Case((dv, 4, false), 0, "sample.getUint16(4, false)");

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 2);
        dv.Invoke("setUint8", 2, 6);
        dv.Invoke("setUint8", 3, 2);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 42);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 39);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), 32810, "0, false");
        Case((dv, 1, false), 10880, "1, false");
        Case((dv, 2, false), 32807, "2, false");
        Case((dv, 0, true), 10880, "0, true");
        Case((dv, 1, true), 32810, "1, true");
        Case((dv, 2, true), 10112, "2, true");

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 1);
        dv.Invoke("setUint8", 7, 0);

        Case((dv, 0, false), 32767, "0, false");
        Case((dv, 1, false), 65535, "1, false");
        Case((dv, 2, false), 65535, "2, false");
        Case((dv, 3, false), 65408, "3, false");
        Case((dv, 4, false), 32768, "4, false");
        Case((dv, 5, false), 1, "5, false");
        Case((dv, 6, false), 256, "8, false");

        Case((dv, 0, true), 65407, "0, true");
        Case((dv, 1, true), 65535, "1, true");
        Case((dv, 2, true), 65535, "2, true");
        Case((dv, 3, true), 33023, "3, true");
        Case((dv, 4, true), 128, "4, true");
        Case((dv, 5, true), 256, "5, true");
        Case((dv, 6, true), 1, "6, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(2);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 42);

        // False
        Case((dv, 0), 42, "no arg");
        Case((dv, 0, Undefined), 42, "undefined");
        Case((dv, 0, Null), 42, "null");
        Case((dv, 0, 0), 42, "0");
        Case((dv, 0, ""), 42, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 10752, "{}");
        Case((dv, 0, new Symbol("1")), 10752, "symbol");
        Case((dv, 0, 1), 10752, "1");
        Case((dv, 0, "string"), 10752, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 1);
        dv.Invoke("setUint8", 3, 127);
        dv.Invoke("setUint8", 4, 255);
        dv.Invoke("setUint8", 5, 1);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0d), 32767, "-0");
        Case((dv, obj1), 383, "object's valueOf");
        Case((dv, obj2), 32767, "object's toString");
        Case((dv, ""), 32767, "the Empty string");
        Case((dv, "0"), 32767, "string '0'");
        Case((dv, "2"), 383, "string '2'");
        Case((dv, true), 65281, "true");
        Case((dv, false), 32767, "false");
        Case((dv, NaN), 32767, "NaN");
        Case((dv, Null), 32767, "null");
        Case((dv, 0.1), 32767, "0.1");
        Case((dv, 0.9), 32767, "0.9");
        Case((dv, 1.1), 65281, "1.1");
        Case((dv, 1.9), 65281, "1.9");
        Case((dv, -0.1), 32767, "-0.1");
        Case((dv, -0.99999), 32767, "-0.99999");
        Case((dv, Undefined), 32767, "undefined");
        Case((dv), 32767, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetUint32(RuntimeFunction getUint32) {
      IsUnconstructableFunctionWLength(getUint32, "getUint32", 1);
      That(DataView.Prototype, Has.OwnProperty("getUint32", getUint32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 32-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, true), 0, "sample.getUint32(0, true)");
        Case((dv, 1, true), 0, "sample.getUint32(1, true)");
        Case((dv, 2, true), 0, "sample.getUint32(2, true)");
        Case((dv, 3, true), 0, "sample.getUint32(3, true)");
        Case((dv, 4, true), 0, "sample.getUint32(4, true)");
        Case((dv, 0, false), 0, "sample.getUint32(0, false)");
        Case((dv, 1, false), 0, "sample.getUint32(1, false)");
        Case((dv, 2, false), 0, "sample.getUint32(2, false)");
        Case((dv, 3, false), 0, "sample.getUint32(3, false)");
        Case((dv, 4, false), 0, "sample.getUint32(4, false)");


        buffer = ArrayBuffer.Construct(12);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 39);
        dv.Invoke("setUint8", 1, 2);
        dv.Invoke("setUint8", 2, 6);
        dv.Invoke("setUint8", 3, 2);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 1);
        dv.Invoke("setUint8", 8, 127);
        dv.Invoke("setUint8", 9, 0);
        dv.Invoke("setUint8", 10, 127);
        dv.Invoke("setUint8", 11, 1);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0, false), 2147516417, "0, false");
        Case((dv, 1, false), 8388991, "1, false");
        Case((dv, 2, false), 2147581696, "2, false");
        Case((dv, 3, false), 25100415, "3, false");
        Case((dv, 0, true), 25165952, "0, true");
        Case((dv, 1, true), 2130804736, "1, true");
        Case((dv, 2, true), 8323456, "2, true");
        Case((dv, 3, true), 2130738945, "3, true");

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 0);
        dv.Invoke("setUint8", 6, 0);
        dv.Invoke("setUint8", 7, 0);
        dv.Invoke("setUint8", 8, 1);
        dv.Invoke("setUint8", 9, 0);
        dv.Invoke("setUint8", 10, 0);
        dv.Invoke("setUint8", 11, 0);

        Case((dv, 0, false), 2147483647, "0, false");
        Case((dv, 1, false), 4294967168, "1, false");
        Case((dv, 2, false), 4294934528, "2, false");
        Case((dv, 3, false), 4286578688, "3, false");
        Case((dv, 4, false), 2147483648, "4, false");
        Case((dv, 5, false), 1, "5, false");
        Case((dv, 6, false), 256, "6, false");
        Case((dv, 7, false), 65536, "7, false");
        Case((dv, 8, false), 16777216, "8, false");

        Case((dv, 0, true), 4294967167, "0, true");
        Case((dv, 1, true), 2164260863, "1, true");
        Case((dv, 2, true), 8454143, "2, true");
        Case((dv, 3, true), 33023, "3, true");
        Case((dv, 4, true), 128, "4, true");
        Case((dv, 5, true), 16777216, "5, true");
        Case((dv, 6, true), 65536, "6, true");
        Case((dv, 7, true), 256, "7, true");
        Case((dv, 8, true), 1, "8, true");
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 0);
        dv.Invoke("setUint8", 1, 17);
        dv.Invoke("setUint8", 2, 4);
        dv.Invoke("setUint8", 3, 0);

        // False
        Case((dv, 0), 1115136, "no arg");
        Case((dv, 0, Undefined), 1115136, "undefined");
        Case((dv, 0, Null), 1115136, "null");
        Case((dv, 0, 0), 1115136, "0");
        Case((dv, 0, ""), 1115136, "the empty string");

        // True
        Case((dv, 0, Object.Construct()), 266496, "{}");
        Case((dv, 0, new Symbol("1")), 266496, "symbol");
        Case((dv, 0, 1), 266496, "1");
        Case((dv, 0, "string"), 266496, "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 255);
        dv.Invoke("setUint8", 3, 255);
        dv.Invoke("setUint8", 4, 128);
        dv.Invoke("setUint8", 5, 255);
        dv.Invoke("setUint8", 6, 128);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0), 2147483647, "-0");
        Case((dv, obj1), 4294934783, "object's valueOf");
        Case((dv, obj2), 4286644096, "object's toString");
        Case((dv, ""), 2147483647, "the Empty string");
        Case((dv, "0"), 2147483647, "string '0'");
        Case((dv, "2"), 4294934783, "string '2'");
        Case((dv, true), 4294967168, "true");
        Case((dv, false), 2147483647, "false");
        Case((dv, NaN), 2147483647, "NaN");
        Case((dv, Null), 2147483647, "null");
        Case((dv, 0.1), 2147483647, "0.1");
        Case((dv, 0.9), 2147483647, "0.9");
        Case((dv, 1.1), 4294967168, "1.1");
        Case((dv, 1.9), 4294967168, "1.9");
        Case((dv, -0.1), 2147483647, "-0.1");
        Case((dv, -0.99999), 2147483647, "-0.99999");
        Case((dv, Undefined), 2147483647, "undefined");
        Case((dv), 2147483647, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetUint8(RuntimeFunction getUint8) {
      IsUnconstructableFunctionWLength(getUint8, "getUint8", 1);
      That(DataView.Prototype, Has.OwnProperty("getUint8", getUint8, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1), Throws.RangeError);
        Case((dv, Infinity), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 1), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 0), 0), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1), Throws.RangeError);
        Case((dv, -Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol()), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return correct 8-bit integer value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), 0, "sample.getUint8(0)");
        Case((dv, 1), 0, "sample.getUint8(1)");
        Case((dv, 2), 0, "sample.getUint8(2)");
        Case((dv, 3), 0, "sample.getUint8(3)");

        buffer = ArrayBuffer.Construct(8);
        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 7);
        dv.Invoke("setUint8", 1, 7);
        dv.Invoke("setUint8", 2, 7);
        dv.Invoke("setUint8", 3, 7);
        dv.Invoke("setUint8", 4, 1);
        dv.Invoke("setUint8", 5, 127);
        dv.Invoke("setUint8", 6, 128);
        dv.Invoke("setUint8", 7, 255);

        dv = DataView.Construct(buffer, 4);
        Case((dv, 0), 1);
        Case((dv, 1), 127);
        Case((dv, 2), 128);
        Case((dv, 3), 255);

        dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 127);
        dv.Invoke("setUint8", 1, 255);
        dv.Invoke("setUint8", 2, 0);
        dv.Invoke("setUint8", 3, 1);

        Case((dv, 0), 127);
        Case((dv, 1), 255);
        Case((dv, 2), 0);
        Case((dv, 3), 1);
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        dv.Invoke("setUint8", 0, 39);
        dv.Invoke("setUint8", 1, 42);
        dv.Invoke("setUint8", 2, 7);
        dv.Invoke("setUint8", 3, 77);

        EcmaValue obj1 = CreateObject(valueOf: () => 2);
        EcmaValue obj2 = CreateObject(toString: () => 3);
        Case((dv, -0), 39, "-0");
        Case((dv, obj1), 7, "object's valueOf");
        Case((dv, obj2), 77, "object's toString");
        Case((dv, ""), 39, "the Empty string");
        Case((dv, "0"), 39, "string '0'");
        Case((dv, "2"), 7, "string '1'");
        Case((dv, true), 42, "true");
        Case((dv, false), 39, "false");
        Case((dv, NaN), 39, "NaN");
        Case((dv, Null), 39, "null");
        Case((dv, 0.1), 39, "0.1");
        Case((dv, 0.9), 39, "0.9");
        Case((dv, 1.1), 42, "1.1");
        Case((dv, 1.9), 42, "1.9");
        Case((dv, -0.1), 39, "-0.1");
        Case((dv, -0.99999), 39, "-0.99999");
        Case((dv, Undefined), 39, "undefined");
        Case((dv), 39, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetFloat32(RuntimeFunction setFloat32) {
      IsUnconstructableFunctionWLength(setFloat32, "setFloat32", 2);
      That(DataView.Prototype, Has.OwnProperty("setFloat32", setFloat32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getFloat32", 4), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getFloat32", 8), Is.EqualTo(0), "[2] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to NaN) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getFloat32", 0), Is.NaN);
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, 42, true), Undefined);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(1.4441781973331565e-41));
        Case((dv, 0, 1.4441781973331565e-41, true), Undefined);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setFloat32.Call(dv, 0, 1);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(1), "no arg");
        setFloat32.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(2), "undefined");
        setFloat32.Call(dv, 0, 3, Null);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(3), "null");
        setFloat32.Call(dv, 0, 4, 0);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(4), "0");
        setFloat32.Call(dv, 0, 5, "");
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(5), "the empty string");

        // True
        setFloat32.Call(dv, 0, 6, Object.Construct());
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(6.89663052202102e-41), "{}");
        setFloat32.Call(dv, 0, 7, new Symbol("1"));
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(8.04457422399591e-41), "symbol");
        setFloat32.Call(dv, 0, 8, 1);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(9.10844001811131e-44), "1");
        setFloat32.Call(dv, 0, 9, "string");
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(5.830802910055564e-42), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, -0d, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "-0");

        setFloat32.Call(dv, 3, 0);
        setFloat32.Call(dv, obj1, 42);
        That(dv.Invoke("getFloat32", 3), Is.EqualTo(42), "object's valueOf");

        setFloat32.Call(dv, 4, 0);
        setFloat32.Call(dv, obj2, 42);
        That(dv.Invoke("getFloat32", 4), Is.EqualTo(42), "object's toString");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, "", 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "the Empty string");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, "0", 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "string '0'");

        setFloat32.Call(dv, 2, 0);
        setFloat32.Call(dv, "2", 42);
        That(dv.Invoke("getFloat32", 2), Is.EqualTo(42), "string '2'");

        setFloat32.Call(dv, 1, 0);
        setFloat32.Call(dv, true, 42);
        That(dv.Invoke("getFloat32", 1), Is.EqualTo(42), "true");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, false, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "false");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, NaN, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "NaN");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, Null, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "null");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, 0.1, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "0.1");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, 0.9, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "0.9");

        setFloat32.Call(dv, 1, 0);
        setFloat32.Call(dv, 1.1, 42);
        That(dv.Invoke("getFloat32", 1), Is.EqualTo(42), "1.1");

        setFloat32.Call(dv, 1, 0);
        setFloat32.Call(dv, 1.9, 42);
        That(dv.Invoke("getFloat32", 1), Is.EqualTo(42), "1.9");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, -0.1, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "-0.1");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, -0.99999, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "-0.99999");

        setFloat32.Call(dv, 0, 0);
        setFloat32.Call(dv, Undefined, 42);
        That(dv.Invoke("getFloat32", 0), Is.EqualTo(42), "undefined");

        setFloat32.Call(dv, 0, 7);
        setFloat32.Call(dv);
        That(dv.Invoke("getFloat32", 0), Is.NaN, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetFloat64(RuntimeFunction setFloat64) {
      IsUnconstructableFunctionWLength(setFloat64, "setFloat64", 2);
      That(DataView.Prototype, Has.OwnProperty("setFloat64", setFloat64, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 8, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 7, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 6, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 5, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 7), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 7), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getFloat64", 4), Is.EqualTo(0), "[1] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to NaN) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getFloat64", 0), Is.NaN);
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, 42, true), Undefined);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(8.759e-320));
        Case((dv, 0, 8.759e-320, true), Undefined);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setFloat64.Call(dv, 0, 1);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(1), "no arg");
        setFloat64.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(2), "undefined");
        setFloat64.Call(dv, 0, 3, Null);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(3), "null");
        setFloat64.Call(dv, 0, 4, 0);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(4), "0");
        setFloat64.Call(dv, 0, 5, "");
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(5), "the empty string");

        // True
        setFloat64.Call(dv, 0, 3.067e-320, Object.Construct());
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(6), "{}");
        setFloat64.Call(dv, 0, 3.573e-320, new Symbol("1"));
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(7), "symbol");
        setFloat64.Call(dv, 0, 4.079e-320, 1);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(8), "1");
        setFloat64.Call(dv, 0, 4.332e-320, "string");
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(9), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, -0d, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "-0");

        setFloat64.Call(dv, 3, 0);
        setFloat64.Call(dv, obj1, 42);
        That(dv.Invoke("getFloat64", 3), Is.EqualTo(42), "object's valueOf");

        setFloat64.Call(dv, 4, 0);
        setFloat64.Call(dv, obj2, 42);
        That(dv.Invoke("getFloat64", 4), Is.EqualTo(42), "object's toString");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, "", 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "the Empty string");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, "0", 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "string '0'");

        setFloat64.Call(dv, 2, 0);
        setFloat64.Call(dv, "2", 42);
        That(dv.Invoke("getFloat64", 2), Is.EqualTo(42), "string '2'");

        setFloat64.Call(dv, 1, 0);
        setFloat64.Call(dv, true, 42);
        That(dv.Invoke("getFloat64", 1), Is.EqualTo(42), "true");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, false, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "false");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, NaN, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "NaN");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, Null, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "null");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, 0.1, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "0.1");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, 0.9, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "0.9");

        setFloat64.Call(dv, 1, 0);
        setFloat64.Call(dv, 1.1, 42);
        That(dv.Invoke("getFloat64", 1), Is.EqualTo(42), "1.1");

        setFloat64.Call(dv, 1, 0);
        setFloat64.Call(dv, 1.9, 42);
        That(dv.Invoke("getFloat64", 1), Is.EqualTo(42), "1.9");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, -0.1, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "-0.1");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, -0.99999, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "-0.99999");

        setFloat64.Call(dv, 0, 0);
        setFloat64.Call(dv, Undefined, 42);
        That(dv.Invoke("getFloat64", 0), Is.EqualTo(42), "undefined");

        setFloat64.Call(dv, 0, 7);
        setFloat64.Call(dv);
        That(dv.Invoke("getFloat64", 0), Is.NaN, "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetInt16(RuntimeFunction setInt16) {
      IsUnconstructableFunctionWLength(setInt16, "setInt16", 2);
      That(DataView.Prototype, Has.OwnProperty("setInt16", setInt16, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 10), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 2), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 2), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getInt16", 2), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getInt16", 4), Is.EqualTo(0), "[2] no value was set");
        That(dv.Invoke("getInt16", 6), Is.EqualTo(0), "[3] no value was set");
        That(dv.Invoke("getInt16", 8), Is.EqualTo(0), "[4] no value was set");
        That(dv.Invoke("getInt16", 10), Is.EqualTo(0), "[5] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(0));
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        setInt16.Call(dv, 0, -1870724872, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(-2048));

        setInt16.Call(dv, 0, -134185072, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(-28545));

        setInt16.Call(dv, 0, 1870724872, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(2303));

        setInt16.Call(dv, 0, 150962287, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(28544));

        setInt16.Call(dv, 0, 4160782224, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(-28545));

        setInt16.Call(dv, 0, 2424242424, true);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(-2048));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setInt16.Call(dv, 0, 1);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(1), "no arg");
        setInt16.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(2), "undefined");
        setInt16.Call(dv, 0, 3, Null);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(3), "null");
        setInt16.Call(dv, 0, 4, 0);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(4), "0");
        setInt16.Call(dv, 0, 5, "");
        That(dv.Invoke("getInt16", 0), Is.EqualTo(5), "the empty string");

        // True
        setInt16.Call(dv, 0, 1536, Object.Construct());
        That(dv.Invoke("getInt16", 0), Is.EqualTo(6), "{}");
        setInt16.Call(dv, 0, 1792, new Symbol("1"));
        That(dv.Invoke("getInt16", 0), Is.EqualTo(7), "symbol");
        setInt16.Call(dv, 0, 2048, 1);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(8), "1");
        setInt16.Call(dv, 0, 2304, "string");
        That(dv.Invoke("getInt16", 0), Is.EqualTo(9), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, -0d, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "-0");

        setInt16.Call(dv, 3, 0);
        setInt16.Call(dv, obj1, 42);
        That(dv.Invoke("getInt16", 3), Is.EqualTo(42), "object's valueOf");

        setInt16.Call(dv, 4, 0);
        setInt16.Call(dv, obj2, 42);
        That(dv.Invoke("getInt16", 4), Is.EqualTo(42), "object's toString");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, "", 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "the Empty string");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, "0", 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "string '0'");

        setInt16.Call(dv, 2, 0);
        setInt16.Call(dv, "2", 42);
        That(dv.Invoke("getInt16", 2), Is.EqualTo(42), "string '2'");

        setInt16.Call(dv, 1, 0);
        setInt16.Call(dv, true, 42);
        That(dv.Invoke("getInt16", 1), Is.EqualTo(42), "true");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, false, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "false");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, NaN, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "NaN");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, Null, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "null");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, 0.1, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "0.1");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, 0.9, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "0.9");

        setInt16.Call(dv, 1, 0);
        setInt16.Call(dv, 1.1, 42);
        That(dv.Invoke("getInt16", 1), Is.EqualTo(42), "1.1");

        setInt16.Call(dv, 1, 0);
        setInt16.Call(dv, 1.9, 42);
        That(dv.Invoke("getInt16", 1), Is.EqualTo(42), "1.9");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, -0.1, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "-0.1");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, -0.99999, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "-0.99999");

        setInt16.Call(dv, 0, 0);
        setInt16.Call(dv, Undefined, 42);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(42), "undefined");

        setInt16.Call(dv, 0, 7);
        setInt16.Call(dv);
        That(dv.Invoke("getInt16", 0), Is.EqualTo(0), "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetInt32(RuntimeFunction setInt32) {
      IsUnconstructableFunctionWLength(setInt32, "setInt32", 2);
      That(DataView.Prototype, Has.OwnProperty("setInt32", setInt32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getInt32", 4), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getInt32", 8), Is.EqualTo(0), "[2] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(0));
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        setInt32.Call(dv, 0, -1870724872, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(-134185072));

        setInt32.Call(dv, 0, -134185072, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(-1870724872));

        setInt32.Call(dv, 0, 1870724872, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(150962287));

        setInt32.Call(dv, 0, 150962287, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(1870724872));

        setInt32.Call(dv, 0, 4160782224, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(-1870724872));

        setInt32.Call(dv, 0, 2424242424, true);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(-134185072));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setInt32.Call(dv, 0, 1);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(1), "no arg");
        setInt32.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(2), "undefined");
        setInt32.Call(dv, 0, 3, Null);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(3), "null");
        setInt32.Call(dv, 0, 4, 0);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(4), "0");
        setInt32.Call(dv, 0, 5, "");
        That(dv.Invoke("getInt32", 0), Is.EqualTo(5), "the empty string");

        // True
        setInt32.Call(dv, 0, 6, Object.Construct());
        That(dv.Invoke("getInt32", 0), Is.EqualTo(100663296), "{}");
        setInt32.Call(dv, 0, 7, new Symbol("1"));
        That(dv.Invoke("getInt32", 0), Is.EqualTo(117440512), "symbol");
        setInt32.Call(dv, 0, 8, 1);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(134217728), "1");
        setInt32.Call(dv, 0, 9, "string");
        That(dv.Invoke("getInt32", 0), Is.EqualTo(150994944), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, -0d, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "-0");

        setInt32.Call(dv, 3, 0);
        setInt32.Call(dv, obj1, 42);
        That(dv.Invoke("getInt32", 3), Is.EqualTo(42), "object's valueOf");

        setInt32.Call(dv, 4, 0);
        setInt32.Call(dv, obj2, 42);
        That(dv.Invoke("getInt32", 4), Is.EqualTo(42), "object's toString");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, "", 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "the Empty string");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, "0", 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "string '0'");

        setInt32.Call(dv, 2, 0);
        setInt32.Call(dv, "2", 42);
        That(dv.Invoke("getInt32", 2), Is.EqualTo(42), "string '2'");

        setInt32.Call(dv, 1, 0);
        setInt32.Call(dv, true, 42);
        That(dv.Invoke("getInt32", 1), Is.EqualTo(42), "true");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, false, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "false");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, NaN, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "NaN");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, Null, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "null");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, 0.1, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "0.1");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, 0.9, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "0.9");

        setInt32.Call(dv, 1, 0);
        setInt32.Call(dv, 1.1, 42);
        That(dv.Invoke("getInt32", 1), Is.EqualTo(42), "1.1");

        setInt32.Call(dv, 1, 0);
        setInt32.Call(dv, 1.9, 42);
        That(dv.Invoke("getInt32", 1), Is.EqualTo(42), "1.9");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, -0.1, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "-0.1");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, -0.99999, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "-0.99999");

        setInt32.Call(dv, 0, 0);
        setInt32.Call(dv, Undefined, 42);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(42), "undefined");

        setInt32.Call(dv, 0, 7);
        setInt32.Call(dv);
        That(dv.Invoke("getInt32", 0), Is.EqualTo(0), "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetInt8(RuntimeFunction setInt8) {
      IsUnconstructableFunctionWLength(setInt8, "setInt8", 2);
      That(DataView.Prototype, Has.OwnProperty("setInt8", setInt8, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 5, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 4, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 3), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 2, 1), 1, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getInt8", 1), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getInt8", 2), Is.EqualTo(0), "[2] no value was set");
        That(dv.Invoke("getInt8", 3), Is.EqualTo(0), "[2] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(0));
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, -0d, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "-0");

        setInt8.Call(dv, 3, 0);
        setInt8.Call(dv, obj1, 42);
        That(dv.Invoke("getInt8", 3), Is.EqualTo(42), "object's valueOf");

        setInt8.Call(dv, 4, 0);
        setInt8.Call(dv, obj2, 42);
        That(dv.Invoke("getInt8", 4), Is.EqualTo(42), "object's toString");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, "", 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "the Empty string");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, "0", 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "string '0'");

        setInt8.Call(dv, 2, 0);
        setInt8.Call(dv, "2", 42);
        That(dv.Invoke("getInt8", 2), Is.EqualTo(42), "string '2'");

        setInt8.Call(dv, 1, 0);
        setInt8.Call(dv, true, 42);
        That(dv.Invoke("getInt8", 1), Is.EqualTo(42), "true");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, false, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "false");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, NaN, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "NaN");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, Null, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "null");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, 0.1, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "0.1");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, 0.9, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "0.9");

        setInt8.Call(dv, 1, 0);
        setInt8.Call(dv, 1.1, 42);
        That(dv.Invoke("getInt8", 1), Is.EqualTo(42), "1.1");

        setInt8.Call(dv, 1, 0);
        setInt8.Call(dv, 1.9, 42);
        That(dv.Invoke("getInt8", 1), Is.EqualTo(42), "1.9");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, -0.1, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "-0.1");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, -0.99999, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "-0.99999");

        setInt8.Call(dv, 0, 0);
        setInt8.Call(dv, Undefined, 42);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(42), "undefined");

        setInt8.Call(dv, 0, 7);
        setInt8.Call(dv);
        That(dv.Invoke("getInt8", 0), Is.EqualTo(0), "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetUint16(RuntimeFunction setUint16) {
      IsUnconstructableFunctionWLength(setUint16, "setUint16", 2);
      That(DataView.Prototype, Has.OwnProperty("setUint16", setUint16, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 10), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 11), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 2), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 2), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 1), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getUint16", 2), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getUint16", 4), Is.EqualTo(0), "[2] no value was set");
        That(dv.Invoke("getUint16", 6), Is.EqualTo(0), "[3] no value was set");
        That(dv.Invoke("getUint16", 8), Is.EqualTo(0), "[4] no value was set");
        That(dv.Invoke("getUint16", 10), Is.EqualTo(0), "[5] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(0));
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        setUint16.Call(dv, 0, -1870724872, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(63488));

        setUint16.Call(dv, 0, -134185072, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(36991));

        setUint16.Call(dv, 0, 1870724872, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(2303));

        setUint16.Call(dv, 0, 150962287, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(28544));

        setUint16.Call(dv, 0, 4160782224, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(36991));

        setUint16.Call(dv, 0, 2424242424, true);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(63488));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setUint16.Call(dv, 0, 1);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(1), "no arg");
        setUint16.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(2), "undefined");
        setUint16.Call(dv, 0, 3, Null);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(3), "null");
        setUint16.Call(dv, 0, 4, 0);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(4), "0");
        setUint16.Call(dv, 0, 5, "");
        That(dv.Invoke("getUint16", 0), Is.EqualTo(5), "the empty string");

        // True
        setUint16.Call(dv, 0, 1536, Object.Construct());
        That(dv.Invoke("getUint16", 0), Is.EqualTo(6), "{}");
        setUint16.Call(dv, 0, 1792, new Symbol("1"));
        That(dv.Invoke("getUint16", 0), Is.EqualTo(7), "symbol");
        setUint16.Call(dv, 0, 2048, 1);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(8), "1");
        setUint16.Call(dv, 0, 2304, "string");
        That(dv.Invoke("getUint16", 0), Is.EqualTo(9), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, -0d, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "-0");

        setUint16.Call(dv, 3, 0);
        setUint16.Call(dv, obj1, 42);
        That(dv.Invoke("getUint16", 3), Is.EqualTo(42), "object's valueOf");

        setUint16.Call(dv, 4, 0);
        setUint16.Call(dv, obj2, 42);
        That(dv.Invoke("getUint16", 4), Is.EqualTo(42), "object's toString");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, "", 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "the Empty string");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, "0", 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "string '0'");

        setUint16.Call(dv, 2, 0);
        setUint16.Call(dv, "2", 42);
        That(dv.Invoke("getUint16", 2), Is.EqualTo(42), "string '2'");

        setUint16.Call(dv, 1, 0);
        setUint16.Call(dv, true, 42);
        That(dv.Invoke("getUint16", 1), Is.EqualTo(42), "true");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, false, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "false");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, NaN, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "NaN");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, Null, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "null");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, 0.1, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "0.1");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, 0.9, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "0.9");

        setUint16.Call(dv, 1, 0);
        setUint16.Call(dv, 1.1, 42);
        That(dv.Invoke("getUint16", 1), Is.EqualTo(42), "1.1");

        setUint16.Call(dv, 1, 0);
        setUint16.Call(dv, 1.9, 42);
        That(dv.Invoke("getUint16", 1), Is.EqualTo(42), "1.9");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, -0.1, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "-0.1");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, -0.99999, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "-0.99999");

        setUint16.Call(dv, 0, 0);
        setUint16.Call(dv, Undefined, 42);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(42), "undefined");

        setUint16.Call(dv, 0, 7);
        setUint16.Call(dv);
        That(dv.Invoke("getUint16", 0), Is.EqualTo(0), "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetUint32(RuntimeFunction setUint32) {
      IsUnconstructableFunctionWLength(setUint32, "setUint32", 2);
      That(DataView.Prototype, Has.OwnProperty("setUint32", setUint32, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 13, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 12, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 11, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 10, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 9, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 8), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 9), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 3), 0, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 4), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 4, 3), 0, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getUint32", 4), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getUint32", 8), Is.EqualTo(0), "[2] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(0));
      });

      It("should set values with little endian order when littleEndian is true", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        setUint32.Call(dv, 0, -1870724872, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(4160782224));

        setUint32.Call(dv, 0, -134185072, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(2424242424));

        setUint32.Call(dv, 0, 1870724872, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(150962287));

        setUint32.Call(dv, 0, 150962287, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(1870724872));

        setUint32.Call(dv, 0, 4160782224, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(2424242424));

        setUint32.Call(dv, 0, 2424242424, true);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(4160782224));
      });

      It("should coerce littleEndian argument to boolean", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);

        // False
        setUint32.Call(dv, 0, 1);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(1), "no arg");
        setUint32.Call(dv, 0, 2, Undefined);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(2), "undefined");
        setUint32.Call(dv, 0, 3, Null);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(3), "null");
        setUint32.Call(dv, 0, 4, 0);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(4), "0");
        setUint32.Call(dv, 0, 5, "");
        That(dv.Invoke("getUint32", 0), Is.EqualTo(5), "the empty string");

        // True
        setUint32.Call(dv, 0, 6, Object.Construct());
        That(dv.Invoke("getUint32", 0), Is.EqualTo(100663296), "{}");
        setUint32.Call(dv, 0, 7, new Symbol("1"));
        That(dv.Invoke("getUint32", 0), Is.EqualTo(117440512), "symbol");
        setUint32.Call(dv, 0, 8, 1);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(134217728), "1");
        setUint32.Call(dv, 0, 9, "string");
        That(dv.Invoke("getUint32", 0), Is.EqualTo(150994944), "string");
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, -0d, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "-0");

        setUint32.Call(dv, 3, 0);
        setUint32.Call(dv, obj1, 42);
        That(dv.Invoke("getUint32", 3), Is.EqualTo(42), "object's valueOf");

        setUint32.Call(dv, 4, 0);
        setUint32.Call(dv, obj2, 42);
        That(dv.Invoke("getUint32", 4), Is.EqualTo(42), "object's toString");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, "", 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "the Empty string");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, "0", 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "string '0'");

        setUint32.Call(dv, 2, 0);
        setUint32.Call(dv, "2", 42);
        That(dv.Invoke("getUint32", 2), Is.EqualTo(42), "string '2'");

        setUint32.Call(dv, 1, 0);
        setUint32.Call(dv, true, 42);
        That(dv.Invoke("getUint32", 1), Is.EqualTo(42), "true");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, false, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "false");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, NaN, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "NaN");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, Null, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "null");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, 0.1, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "0.1");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, 0.9, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "0.9");

        setUint32.Call(dv, 1, 0);
        setUint32.Call(dv, 1.1, 42);
        That(dv.Invoke("getUint32", 1), Is.EqualTo(42), "1.1");

        setUint32.Call(dv, 1, 0);
        setUint32.Call(dv, 1.9, 42);
        That(dv.Invoke("getUint32", 1), Is.EqualTo(42), "1.9");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, -0.1, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "-0.1");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, -0.99999, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "-0.99999");

        setUint32.Call(dv, 0, 0);
        setUint32.Call(dv, Undefined, 42);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(42), "undefined");

        setUint32.Call(dv, 0, 7);
        setUint32.Call(dv);
        That(dv.Invoke("getUint32", 0), Is.EqualTo(0), "no arg");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetUint8(RuntimeFunction setUint8) {
      IsUnconstructableFunctionWLength(setUint8, "setUint8", 2);
      That(DataView.Prototype, Has.OwnProperty("setUint8", setUint8, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[DataView]]", () => {
        Case(DataView.Prototype, Throws.TypeError);
        Case(Object.Prototype, Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(ArrayBuffer.Construct(8), Throws.TypeError);
        Case(Global.Int8Array.Construct(), Throws.TypeError);
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToIndex(requestIndex)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(6);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, -1, 0), Throws.RangeError);
        Case((dv, Infinity, 0), Throws.RangeError);
      });

      It("should checked detached buffer before out of range byteOffset's value", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 13, 0), Throws.TypeError);
      });

      It("should checked detached buffer after ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        DetachBuffer(buffer);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
      });

      It("should throw a RangeError if getIndex + elementSize > viewSize", () => {
        EcmaValue buffer = ArrayBuffer.Construct(4);
        Case((DataView.Construct(buffer, 0), Infinity, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 5, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0), 4, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 3), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 0, 1), 1, 39), Throws.RangeError);
        Case((DataView.Construct(buffer, 2, 1), 1, 39), Throws.RangeError);

        EcmaValue dv = DataView.Construct(buffer, 0);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(0), "[0] no value was set");
        That(dv.Invoke("getUint8", 1), Is.EqualTo(0), "[1] no value was set");
        That(dv.Invoke("getUint8", 2), Is.EqualTo(0), "[2] no value was set");
        That(dv.Invoke("getUint8", 3), Is.EqualTo(0), "[2] no value was set");
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, -1, poisoned), Throws.RangeError);
        Case((dv, -1.5, poisoned), Throws.RangeError);
        Case((dv, -Infinity, poisoned), Throws.RangeError);
        Case((dv, Infinity, poisoned), Throws.RangeError);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(0), "[0] no value was set");
      });

      It("should checked index bounds after value conversion", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 100, poisoned), Throws.Test262);
        Case((dv, "100", poisoned), Throws.Test262);
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, new Symbol(), 1), Throws.TypeError);
        Case((dv, CreateObject(valueOf: ThrowTest262Exception), 1), Throws.Test262);
        Case((dv, CreateObject(toString: ThrowTest262Exception), 1), Throws.Test262);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0, new Symbol()), Throws.TypeError);
        Case((dv, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((dv, 0, CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should set value as undefined (cast to 0) when value argument is not present", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue dv = DataView.Construct(buffer, 0);
        Case((dv, 0), Undefined);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(0));
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue buffer = ArrayBuffer.Construct(12);
        EcmaValue dv = DataView.Construct(buffer, 0);
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(toString: () => 4);

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, -0d, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "-0");

        setUint8.Call(dv, 3, 0);
        setUint8.Call(dv, obj1, 42);
        That(dv.Invoke("getUint8", 3), Is.EqualTo(42), "object's valueOf");

        setUint8.Call(dv, 4, 0);
        setUint8.Call(dv, obj2, 42);
        That(dv.Invoke("getUint8", 4), Is.EqualTo(42), "object's toString");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, "", 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "the Empty string");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, "0", 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "string '0'");

        setUint8.Call(dv, 2, 0);
        setUint8.Call(dv, "2", 42);
        That(dv.Invoke("getUint8", 2), Is.EqualTo(42), "string '2'");

        setUint8.Call(dv, 1, 0);
        setUint8.Call(dv, true, 42);
        That(dv.Invoke("getUint8", 1), Is.EqualTo(42), "true");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, false, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "false");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, NaN, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "NaN");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, Null, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "null");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, 0.1, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "0.1");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, 0.9, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "0.9");

        setUint8.Call(dv, 1, 0);
        setUint8.Call(dv, 1.1, 42);
        That(dv.Invoke("getUint8", 1), Is.EqualTo(42), "1.1");

        setUint8.Call(dv, 1, 0);
        setUint8.Call(dv, 1.9, 42);
        That(dv.Invoke("getUint8", 1), Is.EqualTo(42), "1.9");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, -0.1, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "-0.1");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, -0.99999, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "-0.99999");

        setUint8.Call(dv, 0, 0);
        setUint8.Call(dv, Undefined, 42);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(42), "undefined");

        setUint8.Call(dv, 0, 7);
        setUint8.Call(dv);
        That(dv.Invoke("getUint8", 0), Is.EqualTo(0), "no arg");
      });
    }
  }
}
