using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Harness;
using NUnit.Framework;
using System.Linq;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AtomicsObject : TestBase {
    RuntimeObject Atomics => Global.Atomics;
    RuntimeFunction Int32Array => Global.Int32Array;
    RuntimeFunction SharedArrayBuffer => Global.SharedArrayBuffer;
    RuntimeFunction[] IntArrayCtors => new[] { Global.Int8Array, Global.Int16Array, Global.Int32Array, Global.Uint8Array, Global.Uint16Array, Global.Uint32Array };
    RuntimeFunction[] BadCtors => new[] { Global.Int8Array, Global.Uint8Array, Global.Int16Array, Global.Uint16Array, Global.Uint32Array, Global.Uint8ClampedArray, Global.Float32Array, Global.Float64Array };

    [Test]
    public void Properties() {
      That(GlobalThis, Has.OwnProperty("Atomics", Atomics, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Atomics, Has.OwnProperty(Symbol.ToStringTag, "Atomics", EcmaPropertyAttributes.Configurable));
      That(Object.Invoke("getPrototypeOf", Atomics), Is.EqualTo(Object.Prototype));
      That(() => Atomics.Call(), Throws.TypeError);
      That(() => Atomics.Construct(), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Add(RuntimeFunction add) {
      IsUnconstructableFunctionWLength(add, "add", 3);
      That(Atomics, Has.OwnProperty("add", add, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue newValue = 0b00000001000000001000000010000001;
        Case((_, sample, 0, newValue), 0);
        That(sample[0], Is.EqualTo(newValue));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 0;
          Case((_, view, 8, 10), 0, "Atomics.add(view, 8, 10) returns 0");
          That(view[8], Is.EqualTo(10), "The value of view[8] is 10");

          Case((_, view, 8, -5), 10, "Atomics.add(view, 8, -5) returns 10");
          That(view[8], Is.EqualTo(5), "The value of view[8] is 5");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.add(view, 3, 0) returns the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.add(view, 3, 0) returns the value of `control[0]` (12345)");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.add(view, 3, 0) returns the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void And(RuntimeFunction and) {
      IsUnconstructableFunctionWLength(and, "and", 3);
      That(Atomics, Has.OwnProperty("and", and, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue a = 0b00000001000000001000000010000001;
        EcmaValue b = 0b00000001111111111000000011111111;
        EcmaValue c = 0b00000001000000001000000010000001;
        sample[0] = a;
        Case((_, sample, 0, b), a);
        That(sample[0], Is.EqualTo(c));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 0x33333333;
          control[0] = 0x33333333;
          Case((_, view, 8, 0x55555555), control[0], "Atomics.and(view, 8, 0x55555555) returns the value of `control[0]` (0x33333333)");

          control[0] = 0x11111111;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0x11111111)");
          Case((_, view, 8, 0xF0F0F0F0), control[0], "Atomics.and(view, 8, 0xF0F0F0F0) returns the value of `control[0]` (0x11111111)");

          control[0] = 0x10101010;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0x10101010)");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.and(view, 3, 0) returns the value of `control[0]` (-5)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.and(view, 3, 0) returns the value of `control[0]` (12345)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.and(view, 3, 0) returns the value of `control[0]` (123456789)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void CompareExchange(RuntimeFunction compareExchange) {
      IsUnconstructableFunctionWLength(compareExchange, "compareExchange", 4);
      That(Atomics, Has.OwnProperty("compareExchange", compareExchange, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return abrupt from ToNumber(replacementValue)", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          Case((_, TA.Construct(buffer), 0, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        }, IntArrayCtors);
      });

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue update = 0b00000001000000001000000010000001;
        sample[0] = update;

        Case((_, sample, 0, update, 0), update);
        That(sample[0], Is.EqualTo(0));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          // Performs the exchange
          view[8] = 0;
          Case((_, view, 8, 0, 10), 0, "Atomics.compareExchange(view, 8, 0, 10) returns 0");
          That(view[8], Is.EqualTo(10), "The value of view[8] is 10");

          view[8] = 0;
          Case((_, view, 8, 1, 10), 0, "Atomics.compareExchange(view, 8, 1, 10) returns 0");
          That(view[8], Is.EqualTo(0), "The value of view[8] is 0");

          view[8] = 0;
          Case((_, view, 8, 0, -5), 0, "Atomics.compareExchange(view, 8, 0, -5) returns 0");
          control[0] = -5;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (-5)");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, -5, 0), control[0], "Atomics.compareExchange(view, 3, -5, 0) returns the value of `control[0]` (-5)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 12345, 0), control[0], "Atomics.compareExchange(view, 3, 12345, 0) returns the value of `control[0]` (12345)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 123456789, 0), control[0], "Atomics.compareExchange(view, 3, 123456789, 0) returns the value of `control[0]` (123456789)");
          That(view[3], Is.EqualTo(0), "The value of view[3] is 0");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Exchange(RuntimeFunction exchange) {
      IsUnconstructableFunctionWLength(exchange, "exchange", 3);
      That(Atomics, Has.OwnProperty("exchange", exchange, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue update = 0b00000001000000001000000010000001;
        Case((_, sample, 0, update), 0);
        That(sample[0], Is.EqualTo(update));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 0;
          Case((_, view, 8, 10), 0, "Atomics.exchange(view, 8, 10) returns 0");
          That(view[8], Is.EqualTo(10), "The value of view[8] is 10");

          Case((_, view, 8, -5), 10, "Atomics.exchange(view, 8, -5) returns 10");
          control[0] = -5;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (-5)");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.exchange(view, 3, 0) returns the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.exchange(view, 3, 0) returns the value of `control[0]` (12345)");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.exchange(view, 3, 0) returns the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Or(RuntimeFunction or) {
      IsUnconstructableFunctionWLength(or, "or", 3);
      That(Atomics, Has.OwnProperty("or", or, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue a = 0b00000001000000001000000010000001;
        EcmaValue b = 0b00000001111111111000000011111111;
        EcmaValue c = 0b00000001111111111000000011111111;
        sample[0] = a;
        Case((_, sample, 0, b), a);
        That(sample[0], Is.EqualTo(c));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 0x33333333;
          control[0] = 0x33333333;
          Case((_, view, 8, 0x55555555), control[0], "Atomics.or(view, 8, 0x55555555) returns the value of `control[0]` (0x33333333)");

          control[0] = 0x77777777;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0x77777777)");
          Case((_, view, 8, 0xF0F0F0F0), control[0], "Atomics.or(view, 8, 0xF0F0F0F0) returns the value of `control[0]` (0x77777777)");

          control[0] = 0xF7F7F7F7;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0xF7F7F7F7)");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.or(view, 3, 0) returns the value of `control[0]` (-5)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.or(view, 3, 0) returns the value of `control[0]` (12345)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (12345)");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.or(view, 3, 0) returns the value of `control[0]` (123456789)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Sub(RuntimeFunction sub) {
      IsUnconstructableFunctionWLength(sub, "sub", 3);
      That(Atomics, Has.OwnProperty("sub", sub, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue update = 0b00000001000000001000000010000001;
        sample[0] = update;
        Case((_, sample, 0, update), update);
        That(sample[0], Is.EqualTo(0));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 100;
          Case((_, view, 8, 10), 100, "Atomics.sub(view, 8, 10) returns 100");
          That(view[8], Is.EqualTo(90), "The value of view[8] is 90");

          Case((_, view, 8, -5), 90, "Atomics.sub(view, 8, -5) returns 90");
          That(view[8], Is.EqualTo(95), "The value of view[8] is 95");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.sub(view, 3, 0) returns the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.sub(view, 3, 0) returns the value of `control[0]` (12345)");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.sub(view, 3, 0) returns the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Xor(RuntimeFunction xor) {
      IsUnconstructableFunctionWLength(xor, "xor", 3);
      That(Atomics, Has.OwnProperty("xor", xor, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the value that existed at the index prior to the operation", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue a = 0b00000001000000001000000010000001;
        EcmaValue b = 0b00000001111111111000000011111111;
        EcmaValue c = 0b00000000111111110000000001111110;
        sample[0] = a;
        Case((_, sample, 0, b), a);
        That(sample[0], Is.EqualTo(c));
      });

      It("should modify value at the index correctly", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[8] = 0x33333333;
          control[0] = 0x33333333;
          Case((_, view, 8, 0x55555555), control[0], "Atomics.xor(view, 8, 0x55555555) returns the value of `control[0]` (0x33333333)");

          control[0] = 0x66666666;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0x66666666)");
          Case((_, view, 8, 0xF0F0F0F0), control[0], "Atomics.xor(view, 8, 0xF0F0F0F0) returns the value of `control[0]` (0x66666666)");

          control[0] = 0x96969696;
          That(view[8], Is.EqualTo(control[0]), "The value of view[8] equals the value of `control[0]` (0x96969696)");

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3, 0), control[0], "Atomics.xor(view, 3, 0) returns the value of `control[0]` (-5)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3, 0), control[0], "Atomics.xor(view, 3, 0) returns the value of `control[0]` (12345)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (12345)");

          // And again
          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3, 0), control[0], "Atomics.xor(view, 3, 0) returns the value of `control[0]` (123456789)");
          That(view[3], Is.EqualTo(control[0]), "The value of view[3] equals the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsLockFree(RuntimeFunction isLockFree) {
      IsUnconstructableFunctionWLength(isLockFree, "isLockFree", 1);
      That(Atomics, Has.OwnProperty("isLockFree", isLockFree, EcmaPropertyAttributes.DefaultMethodProperty));

      Case((_, 1), Is.TypeOf("boolean"));
      Case((_, 2), Is.TypeOf("boolean"));
      Case((_, 8), Is.TypeOf("boolean"));

      Case((_, 1), isLockFree.Call(_, 1));
      Case((_, 2), isLockFree.Call(_, 2));
      Case((_, 8), isLockFree.Call(_, 8));

      Case((_, 3), false);
      Case((_, 4), true);
      Case((_, 5), false);
      Case((_, 6), false);
      Case((_, 7), false);
      Case((_, 9), false);
      Case((_, 10), false);
      Case((_, 11), false);
      Case((_, 12), false);

      Case((_, true), isLockFree.Call(_, 1));
      Case((_, "1"), isLockFree.Call(_, 1));
      Case((_, "3"), isLockFree.Call(_, 3));
      Case((_, CreateObject(valueOf: () => 1)), isLockFree.Call(_, 1));
      Case((_, CreateObject(valueOf: () => 3)), isLockFree.Call(_, 3));
      Case((_, CreateObject(toString: () => "1")), isLockFree.Call(_, 1));
      Case((_, CreateObject(toString: () => "3")), isLockFree.Call(_, 3));

      EcmaValue hide(EcmaValue k, EcmaValue x) => k ? hide(k - 3, x) + x : 0;
      Case((_, hide(3, NaN)), false);
      Case((_, hide(3, -1)), false);
      Case((_, hide(3, 3.14)), false);
      Case((_, hide(3, 0)), false);
    }

    [Test, RuntimeFunctionInjection]
    public void Load(RuntimeFunction load) {
      IsUnconstructableFunctionWLength(load, "load", 2);
      That(Atomics, Has.OwnProperty("load", load, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation(skipToNumberValue: true);

      It("should return the value that existed at the index", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue update = 0b00000001000000001000000010000001;
        Case((_, sample, 0), 0);
        sample[0] = update;
        Case((_, sample, 0), update);
      });

      It("should load value at the index", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          view[3] = -5;
          control[0] = -5;
          Case((_, view, 3), control[0], "Atomics.load(view, 3) returns the value of `control[0]` (-5)");

          control[0] = 12345;
          view[3] = 12345;
          Case((_, view, 3), control[0], "Atomics.load(view, 3) returns the value of `control[0]` (12345)");

          control[0] = 123456789;
          view[3] = 123456789;
          Case((_, view, 3), control[0], "Atomics.load(view, 3) returns the value of `control[0]` (123456789)");
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Store(RuntimeFunction store) {
      IsUnconstructableFunctionWLength(store, "store", 3);
      That(Atomics, Has.OwnProperty("store", store, EcmaPropertyAttributes.DefaultMethodProperty));
      VerifyCommonValidation();

      It("should return the newly stored value", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        EcmaValue sample = Global.Int32Array.Construct(buffer);
        EcmaValue update = 0b00000001000000001000000010000001;
        Case((_, sample, 0, update), update);
        That(sample[0], Is.EqualTo(update));
      });

      It("should store value at the index", () => {
        EcmaValue sab = new SharedArrayBuffer(1024);
        EcmaValue ab = new ArrayBuffer(16);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(sab, 32, 20);
          EcmaValue control = TA.Construct(ab, 0, 2);

          foreach (EcmaValue v in new[] { 10, -5, 12345, 123456789, Math["PI"], "33", CreateObject(valueOf: () => 33), Undefined }) {
            Case((_, view, 3, v), v.ToInteger());

            control[0] = v;
            That(view[3], Is.EqualTo(control[0]));
          }
        }, IntArrayCtors);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Wait(RuntimeFunction wait) {
      IsUnconstructableFunctionWLength(wait, "wait", 4);
      That(Atomics, Has.OwnProperty("wait", wait, EcmaPropertyAttributes.DefaultMethodProperty));

      const int RUNNING = 1;

      It("should throw a TypeError if typedArray arg is not an Int32Array", () => {
        EcmaValue sab = SharedArrayBuffer.Construct(1024);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Global.Float64Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Float32Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Int8Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Int16Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint8Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint8ClampedArray.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint16Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint32Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if typedArray.buffer is not a SharedArrayBuffer", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(1024);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Int32Array.Construct(buffer), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if the typedArray arg is not a TypedArray object", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Object.Construct(), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if typedArray arg is not an Object", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Undefined, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Null, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, true, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, false, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, "", poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, -Infinity, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, new Symbol(), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if bufferData is null", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(1024);
        EcmaValue i32a = Int32Array.Construct(buffer);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        DetachBuffer(i32a);
        Case((_, i32a, poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a RangeError for out-of-bound indices", () => {
        EcmaValue buffer = SharedArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(buffer);
          Case((_, view, -1, 0), Throws.RangeError);
          Case((_, view, view["length"], 0), Throws.RangeError);
          Case((_, view, view["length"] * 2, 0), Throws.RangeError);
          Case((_, view, Infinity, 0), Throws.RangeError);
          Case((_, view, -Infinity, 0), Throws.RangeError);
          Case((_, view, CreateObject(valueOf: () => 125), 0), Throws.RangeError);
          Case((_, view, CreateObject(toString: () => "125", valueOf: () => Object.Construct()), 0), Throws.RangeError);
        }, new[] { Int32Array });
      });

      It("should throw if agent cannot be suspended, CanBlock is false", () => {
        RuntimeExecution.Current.CanSuspend = false;
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        Case((_, i32a, 0, 0, 0), Throws.TypeError);
      });

      It("should validate TypedArray type before `index` argument is coerced", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(SharedArrayBuffer.Construct(8));
          Case((_, typedArray, CreateObject(valueOf: ThrowTest262Exception), 0, 0), Throws.TypeError);
        }, BadCtors);
      });

      It("should validate TypedArray type before `value` argument is coerced", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(SharedArrayBuffer.Construct(8));
          Case((_, typedArray, 0, CreateObject(valueOf: ThrowTest262Exception), 0), Throws.TypeError);
        }, BadCtors);
      });

      It("should validate TypedArray type before `timeout` argument is coerced", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(SharedArrayBuffer.Construct(8));
          Case((_, typedArray, 0, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.TypeError);
        }, BadCtors);
      });

      It("should result in an inifinite timeout for Undefined timeout arg", () => {
        const int WAIT_INDEX = 0;   // Index all agents are waiting on
        const int NUMAGENT = 2;     // Total number of agents started
        const int NOTIFYCOUNT = 2;  // Total number of agents to notify up
        TestWithAgent(
          workers: new Agent.WorkerStart[] {
            agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING, 1);
                agent.Report("A " + Atomics.Invoke("wait", i32a, 0, 0, Undefined));
                agent.Leaving();
              });
            },
            agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING, 1);
                agent.Report("B " + Atomics.Invoke("wait", i32a, 0, 0));
                agent.Leaving();
              });
            }
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(Atomics.Invoke("notify", i32a, WAIT_INDEX, NOTIFYCOUNT), Is.EqualTo(NOTIFYCOUNT));
            EcmaValue reports = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              reports.Invoke("push", agent.GetReport());
            }
            reports.Invoke("sort");
            That(reports, Is.EquivalentTo(new[] { "A ok", "B ok" }));
          }
        );
      });

      It("should result in an inifinite timeout for NaN timeout arg", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, NaN));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(1), "Atomics.notify(i32a, 0) returns 1");
            That(agent.GetReport(), Is.EqualTo("ok"), "agent.GetReport() returns \"ok\"");
          }
        );
      });

      It("should result in an +0 timeout for False timeout arg", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = Atomics.Invoke("wait", i32a, 0, 0, false);
              EcmaValue status2 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(valueOf: () => false));
              EcmaValue status3 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(toPrimitive: () => false));
              agent.Report(status1);
              agent.Report(status2);
              agent.Report(status3);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, false), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(valueOf: () => false)), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(toPrimitive: () => false)), "timed-out");
        }
      });

      It("should result in an +1 timeout for True timeout arg", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = Atomics.Invoke("wait", i32a, 0, 0, true);
              EcmaValue status2 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(valueOf: () => true));
              EcmaValue status3 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(toPrimitive: () => true));
              agent.Report(status1);
              agent.Report(status2);
              agent.Report(status3);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, true), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(valueOf: () => true)), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(toPrimitive: () => true)), "timed-out");
        }
      });

      It("should result in an +0 timeout for Null timeout arg", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = Atomics.Invoke("wait", i32a, 0, 0, Null);
              EcmaValue status2 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(valueOf: () => Null));
              EcmaValue status3 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(toPrimitive: () => Null));
              agent.Report(status1);
              agent.Report(status2);
              agent.Report(status3);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, Null), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(valueOf: () => Null)), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(toPrimitive: () => Null)), "timed-out");
        }
      });

      It("should coerce timeout arg to number", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(valueOf: () => 0));
              EcmaValue status2 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(toString: () => "0"));
              EcmaValue status3 = Atomics.Invoke("wait", i32a, 0, 0, CreateObject(toPrimitive: () => 0));
              agent.Report(status1);
              agent.Report(status2);
              agent.Report(status3);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, CreateObject(valueOf: () => 0)), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(toString: () => "0")), "timed-out");
          Case((_, i32a, 0, 0, CreateObject(toPrimitive: () => 0)), "timed-out");
        }
      });

      It("should return abrupt from ToNumber(timeout)", () => {
        TestWithAgent(
          worker: agent => {
            EcmaValue obj1 = CreateObject(valueOf: ThrowTest262Exception);
            EcmaValue obj2 = CreateObject(toPrimitive: ThrowTest262Exception);
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = "";
              EcmaValue status2 = "";
              try {
                Atomics.Invoke("wait", i32a, 0, 0, obj1);
              } catch {
                status1 = "poisonedValueOf";
              }
              try {
                Atomics.Invoke("wait", i32a, 0, 0, obj2);
              } catch {
                status2 = "poisonedToPrimitive";
              }
              agent.Report(status1);
              agent.Report(status2);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("poisonedValueOf"));
            That(agent.GetReport(), Is.EqualTo("poisonedToPrimitive"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          Case((_, i32a, 0, 0, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262);
        }
      });

      It("should return abrupt from ToNumber(symbol timeout)", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = "";
              EcmaValue status2 = "";
              try {
                Atomics.Invoke("wait", i32a, 0, 0, new Symbol("1"));
              } catch {
                status1 = "Symbol(\"1\")";
              }
              try {
                Atomics.Invoke("wait", i32a, 0, 0, new Symbol("2"));
              } catch {
                status2 = "Symbol(\"2\")";
              }
              agent.Report(status1);
              agent.Report(status2);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("Symbol(\"1\")"));
            That(agent.GetReport(), Is.EqualTo("Symbol(\"2\")"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, new Symbol("1")), Throws.TypeError);
          Case((_, i32a, 0, 0, new Symbol("2")), Throws.TypeError);
        }
      });

      It("should return abrupt from ToNumber(symbol index)", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = "";
              EcmaValue status2 = "";
              try {
                Atomics.Invoke("wait", i32a, new Symbol("1"), 0, 0);
              } catch {
                status1 = "Symbol(\"1\")";
              }
              try {
                Atomics.Invoke("wait", i32a, new Symbol("2"), 0, 0);
              } catch {
                status2 = "Symbol(\"2\")";
              }
              agent.Report(status1);
              agent.Report(status2);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("Symbol(\"1\")"));
            That(agent.GetReport(), Is.EqualTo("Symbol(\"2\")"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, new Symbol("1"), 0, 0), Throws.TypeError);
          Case((_, i32a, new Symbol("2"), 0, 0), Throws.TypeError);
        }
      });

      It("should return abrupt from ToNumber(symbol value)", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue status1 = "";
              EcmaValue status2 = "";
              try {
                Atomics.Invoke("wait", i32a, 0, new Symbol("1"), 0);
              } catch {
                status1 = "Symbol(\"1\")";
              }
              try {
                Atomics.Invoke("wait", i32a, 0, new Symbol("2"), 0);
              } catch {
                status2 = "Symbol(\"2\")";
              }
              agent.Report(status1);
              agent.Report(status2);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("Symbol(\"1\")"));
            That(agent.GetReport(), Is.EqualTo("Symbol(\"2\")"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0), "Atomics.notify(i32a, 0) returns 0");
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, new Symbol("1"), 0), Throws.TypeError);
          Case((_, i32a, 0, new Symbol("2"), 0), Throws.TypeError);
        }
      });

      It("should not throw with good view and indices", () => {
        TestWithAgent(
          worker: agent => {
            EcmaValue sab = SharedArrayBuffer.Construct(1024);
            EcmaValue view = Int32Array.Construct(sab, 32, 30);
            view[0] = 0;
            agent.Report("A " + Atomics.Invoke("wait", view, 0, 0, 0));
            agent.Report("B " + Atomics.Invoke("wait", view, 0, 37, 0));

            EcmaValue[] indices = { -0d, "-0", view["length"] - 1, CreateObject(valueOf: () => 0), CreateObject(toString: () => "0", valueOf: () => Object.Construct()) };
            foreach (EcmaValue idx in indices) {
              view.Invoke("fill", 0);
              // Atomics.store() computes an index from Idx in the same way as other
              // Atomics operations, not quite like view[Idx].
              Atomics.Invoke("store", view, idx, 37);
              agent.Report("C " + Atomics.Invoke("wait", view, 0, idx, 0));
            }
            agent.Report("done");
            agent.Leaving();
          },
          main: agent => {
            That(agent.GetReport(), Is.EqualTo("A timed-out"));
            That(agent.GetReport(), Is.EqualTo("B not-equal"));
            EcmaValue r;
            while ((r = agent.GetReport()) != "done") {
              That(r, Is.EqualTo("C not-equal"));
            }
          }
        );
      });

      It("should coerce Undefined index arg to 0", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, Undefined, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(1));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
            That(agent.GetReport(), Is.EqualTo("ok"));
          }
        );
      });

      It("should throw a RangeError is index < 0", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, i32a, -Infinity, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -7.999, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -1, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -300, poisoned, poisoned), Throws.RangeError);
      });

      It("should time out with a negative timeout", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, -5));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
        using (null) {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          Case((_, i32a, 0, 0, -1), "timed-out");
        }
      });

      It("should return the right result when it timed out and that the time to time out is reasonable", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("should return the right result when it was awoken before a timeout", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Huge);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("add", i32a, 0, 1);

            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(1));
            That(agent.GetReport() < Agent.Timeout.Huge, "The result of `(lapse < TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("ok"));
          }
        );
      });

      It("should return not-equal when value of index is not equal", () => {
        TestWithAgent(
         worker: agent => {
           agent.ReceiveBroadcast(sab => {
             EcmaValue i32a = Int32Array.Construct(sab);
             Atomics.Invoke("add", i32a, RUNNING, 1);

             agent.Report(Atomics.Invoke("wait", i32a, 0, 44, Agent.Timeout.Small));
             agent.Report(Atomics.Invoke("wait", i32a, 0, 251.4, Agent.Timeout.Small));
             agent.Leaving();
           });
         },
         main: agent => {
           EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
           agent.SafeBroadcast(i32a);
           agent.WaitUntil(i32a, RUNNING, 1);
           agent.TryYield();
           Atomics.Invoke("add", i32a, 0, 1);

           That(agent.GetReport(), Is.EqualTo("not-equal"));
           That(agent.GetReport(), Is.EqualTo("not-equal"));
           That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
         }
       );
      });

      It("should get the correct WaiterList", () => {
        const int NUMAGENT = 2;
        const int RUNNING_ = 4;
        TestWithAgent(
          workers: new Agent.WorkerStart[]{
            agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);
                agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Infinity));
                agent.Leaving();
              });
            },
            agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);
                agent.Report(Atomics.Invoke("wait", i32a, 2, 0, Infinity));
                agent.Leaving();
              });
            },
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 5));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING_, NUMAGENT);
            That(Atomics.Invoke("notify", i32a, 1), Is.EqualTo(0));
            That(Atomics.Invoke("notify", i32a, 3), Is.EqualTo(0));

            EcmaValue woken = 0;
            while ((woken = Atomics.Invoke("notify", i32a, 2)) == 0) ;
            That(woken, Is.EqualTo(1));
            That(agent.GetReport(), Is.EqualTo("ok"));

            woken = 0;
            while ((woken = Atomics.Invoke("notify", i32a, 0)) == 0) ;
            That(woken, Is.EqualTo(1));
            That(agent.GetReport(), Is.EqualTo("ok"));
          }
        );
      });

      It("should apply new waiters to the end of the list and woken by order they entered the list (FIFO)", () => {
        const int NUMAGENT = 3;
        const int LOCK_INDEX = 2;
        const int WAIT_INDEX = 0;
        Agent.WorkerStart[] workers = Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(agentNum => {
          return agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              // Synchronize workers before reporting the initial report.
              while (Atomics.Invoke("compareExchange", i32a, LOCK_INDEX, 0, 1) != 0) ;

              // Report the agent number before waiting.
              agent.Report(agentNum);

              // Wait until restarted by main thread.
              EcmaValue status = Atomics.Invoke("wait", i32a, WAIT_INDEX, 0);

              // Report wait status.
              agent.Report(status);

              // Report the agent number after waiting.
              agent.Report(agentNum);
              agent.Leaving();
            });
          };
        }).ToArray();

        TestWithAgent(workers, agent => {
          EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
          agent.SafeBroadcast(i32a);
          agent.WaitUntil(i32a, RUNNING, NUMAGENT);

          // Agents may be started in any order.
          EcmaValue started = EcmaArray.Of();
          for (var i = 0; i < NUMAGENT; i++) {
            // Wait until an agent entered its critical section.
            agent.WaitUntil(i32a, LOCK_INDEX, 1);

            // Record the agent number.
            started.Invoke("push", agent.GetReport());

            // The agent may have been interrupted between reporting its initial report
            // and the `Atomics.wait` call. Try to yield control to ensure the agent
            // actually started to wait.
            agent.TryYield();

            // Now continue with the next agent.
            Atomics.Invoke("store", i32a, LOCK_INDEX, 0);
          }

          // Agents must notify in the order they waited.
          for (var i = 0; i < NUMAGENT; i++) {
            EcmaValue woken = 0;
            while ((woken = Atomics.Invoke("notify", i32a, WAIT_INDEX, 1)) == 0) ;

            That(woken, Is.EqualTo(1), "Atomics.notify(i32a, WAIT_INDEX, 1) returns 1, at index = " + i);
            That(agent.GetReport(), Is.EqualTo("ok"), "agent.getReport() returns \"ok\", at index = " + i);
            That(agent.GetReport(), Is.EqualTo(started[i]), "agent.getReport() returns the value of `started[" + i + "]`");
          }
        });
      });

      It("does not spuriously notify on index which is subject to Add operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("add", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to And operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("and", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to compareExchange operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("compareExchange", i32a, 0, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to exchange operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("exchange", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to Or operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("or", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to Store operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("store", i32a, 0, 0x111111);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to Sub operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("sub", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });

      It("does not spuriously notify on index which is subject to Xor operation", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);

              EcmaValue before = agent.MonotonicNow();
              EcmaValue unpark = Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Small);
              EcmaValue duration = agent.MonotonicNow() - before;

              agent.Report(duration);
              agent.Report(unpark);
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            Atomics.Invoke("xor", i32a, 0, 1);

            That(agent.GetReport() >= Agent.Timeout.Small, "The result of `(lapse >= TIMEOUT)` is true");
            That(agent.GetReport(), Is.EqualTo("timed-out"));
            That(Atomics.Invoke("notify", i32a, 0), Is.EqualTo(0));
          }
        );
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Notify(RuntimeFunction notify) {
      IsUnconstructableFunctionWLength(notify, "notify", 3);
      That(Atomics, Has.OwnProperty("notify", notify, EcmaPropertyAttributes.DefaultMethodProperty));

      const int RUNNING = 1;

      It("should throw a TypeError if typedArray arg is not an Int32Array", () => {
        EcmaValue sab = SharedArrayBuffer.Construct(1024);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Global.Float64Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Float32Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Int8Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Int16Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint8Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint8ClampedArray.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint16Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Global.Uint32Array.Construct(sab), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if typedArray.buffer is not a SharedArrayBuffer", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(1024);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Int32Array.Construct(buffer), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if the typedArray arg is not a TypedArray object", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Object.Construct(), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if typedArray arg is not an Object", () => {
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, Undefined, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, Null, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, true, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, false, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, "", poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, -Infinity, poisoned, poisoned, poisoned), Throws.TypeError);
        Case((_, new Symbol(), poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should throw a TypeError if bufferData is null", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(1024);
        EcmaValue i32a = Int32Array.Construct(buffer);
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        DetachBuffer(i32a);
        Case((_, i32a, poisoned, poisoned, poisoned), Throws.TypeError);
      });

      It("should validate TypedArray type before `index` argument is coerced", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(SharedArrayBuffer.Construct(8));
          Case((_, typedArray, CreateObject(valueOf: ThrowTest262Exception), 0, 0), Throws.TypeError);
        }, BadCtors);
      });

      It("should validate TypedArray type before `count` argument is coerced", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue typedArray = TA.Construct(SharedArrayBuffer.Construct(8));
          Case((_, typedArray, 0, CreateObject(valueOf: ThrowTest262Exception), 0), Throws.TypeError);
        }, BadCtors);
      });

      It("should throw a RangeError for out-of-bound indices", () => {
        EcmaValue buffer = SharedArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(buffer);
          Case((_, view, -1, 0), Throws.RangeError);
          Case((_, view, view["length"], 0), Throws.RangeError);
          Case((_, view, view["length"] * 2, 0), Throws.RangeError);
          Case((_, view, Infinity, 0), Throws.RangeError);
          Case((_, view, -Infinity, 0), Throws.RangeError);
          Case((_, view, CreateObject(valueOf: () => 125), 0), Throws.RangeError);
          Case((_, view, CreateObject(toString: () => "125", valueOf: () => Object.Construct()), 0), Throws.RangeError);
        }, new[] { Int32Array });
      });

      It("should throw a RangeError is index < 0", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        EcmaValue poisoned = CreateObject(valueOf: ThrowTest262Exception);
        Case((_, i32a, -Infinity, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -7.999, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -1, poisoned, poisoned), Throws.RangeError);
        Case((_, i32a, -300, poisoned, poisoned), Throws.RangeError);
      });

      It("should coerce count to integer", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        Case((_, i32a, 0), 0);
        Case((_, i32a, 0, -3), 0);
        Case((_, i32a, 0, Infinity), 0);
        Case((_, i32a, 0, Undefined), 0);
        Case((_, i32a, 0, "33"), 0);
        Case((_, i32a, 0, CreateObject(valueOf: () => 8)), 0);
      });

      It("should result in an infinite count for missing 'count' argument", () => {
        const int RUNNING_ = 0; // Index to notify agent has started.
        const int WAIT_INDEX = 1; // Index all agents are waiting on.
        const int NUMAGENT = 4; // Total number of agents started
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);

                EcmaValue status = Atomics.Invoke("wait", i32a, WAIT_INDEX, 0);
                agent.Report(String.Invoke("fromCharCode", 0x41 + i) + " " + status);
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING_, NUMAGENT);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, WAIT_INDEX), Is.EqualTo(NUMAGENT));

            EcmaValue reports = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              reports.Invoke("push", agent.GetReport());
            }
            reports.Invoke("sort");
            That(reports, Is.EquivalentTo(new[] { "A ok", "B ok", "C ok", "D ok" }));
          }
        );
      });

      It("should result in an infinite count for Undefined count arg", () => {
        const int RUNNING_ = 0; // Index to notify agent has started.
        const int WAIT_INDEX = 1; // Index all agents are waiting on.
        const int NUMAGENT = 4; // Total number of agents started
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);

                EcmaValue status = Atomics.Invoke("wait", i32a, WAIT_INDEX, 0);
                agent.Report(String.Invoke("fromCharCode", 0x41 + i) + " " + status);
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING_, NUMAGENT);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, WAIT_INDEX, Undefined), Is.EqualTo(NUMAGENT));

            EcmaValue reports = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              reports.Invoke("push", agent.GetReport());
            }
            reports.Invoke("sort");
            That(reports, Is.EquivalentTo(new[] { "A ok", "B ok", "C ok", "D ok" }));
          }
        );
      });

      It("should convert NaN to 0 for 'count' argument", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        Case((_, i32a, 0, NaN), 0);
      });

      It("should return abrupt from ToInteger(index)", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        Case((_, i32a, new Symbol(), 0), Throws.TypeError);
        Case((_, i32a, CreateObject(valueOf: ThrowTest262Exception), 0), Throws.Test262);
        Case((_, i32a, CreateObject(toPrimitive: ThrowTest262Exception), 0), Throws.Test262);
      });

      It("should return abrupt from ToInteger(count)", () => {
        EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
        Case((_, i32a, 0, new Symbol()), Throws.TypeError);
        Case((_, i32a, 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
        Case((_, i32a, 0, CreateObject(toPrimitive: ThrowTest262Exception)), Throws.Test262);
      });

      It("should notify zero waiters if the count is negative", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, 0, -1), Is.EqualTo(0));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
          }
        );
      });

      It("should notify zero waiters if the count is NaN", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, 0, NaN), Is.EqualTo(0));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
          }
        );
      });

      It("should notify zero waiters if the count is 0", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, 0, 0), Is.EqualTo(0));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
          }
        );
      });

      It("should notify one waiter if the count is 1", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, 0, 1), Is.EqualTo(1));
            That(agent.GetReport(), Is.EqualTo("ok"));
            That(Atomics.Invoke("notify", i32a, 0, 1), Is.EqualTo(0));
          }
        );
      });

      It("should notify two waiter if the count is 2", () => {
        const int WAIT_INDEX = 0;
        const int NOTIFYCOUNT = 2;
        const int NUMAGENT = 3;
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING, 1);
                agent.Report(Atomics.Invoke("wait", i32a, WAIT_INDEX, 0, Agent.Timeout.Long));
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, NUMAGENT);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, WAIT_INDEX, NOTIFYCOUNT), Is.EqualTo(NOTIFYCOUNT));

            EcmaValue reports = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              reports.Invoke("push", agent.GetReport());
            }
            reports.Invoke("sort");
            That(reports, Is.EquivalentTo(new[] { "ok", "ok", "timed-out" }));
          }
        );
      });

      It("should notify zero waiters if there are no waiters at the index specified", () => {
        TestWithAgent(
          worker: agent => {
            agent.ReceiveBroadcast(sab => {
              EcmaValue i32a = Int32Array.Construct(sab);
              Atomics.Invoke("add", i32a, RUNNING, 1);
              agent.Report(Atomics.Invoke("wait", i32a, 0, 0, Agent.Timeout.Long));
              agent.Leaving();
            });
          },
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 4));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, 1);
            agent.TryYield();
            That(Atomics.Invoke("notify", i32a, 1, 1), Is.EqualTo(0));
            That(agent.GetReport(), Is.EqualTo("timed-out"));
          }
        );
      });

      It("should notify agents in the order they are waiting", () => {
        const int NUMAGENT = 3;
        const int WAIT_INDEX = 0;              // Waiters on this will be woken
        const int SPIN = 1;                    // Worker i (zero-based) spins on location SPIN+i
        const int RUNNING_ = SPIN + NUMAGENT;  // Accounting of live agents
        const int BUFFER_SIZE = RUNNING_ + 1;
        // Create workers and start them all spinning.  We set atomic slots to make
        // them go into a wait, thus controlling the waiting order.  Then we notify them
        // one by one and observe the notification order.
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);
                while (Atomics.Invoke("load", i32a, SPIN + i) == 0) ;

                agent.Report(i);
                Atomics.Invoke("wait", i32a, WAIT_INDEX, 0);
                agent.Report(i);
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * BUFFER_SIZE));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING_, NUMAGENT);

            EcmaValue waiterlist = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              That(Atomics.Invoke("store", i32a, SPIN + i, 1), Is.EqualTo(1));
              waiterlist.Invoke("push", agent.GetReport());
              agent.TryYield();
            }
            EcmaValue notified = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT; i++) {
              That(Atomics.Invoke("notify", i32a, WAIT_INDEX, 1), Is.EqualTo(1));
              notified.Invoke("push", agent.GetReport());
            }
            That(waiterlist.Invoke("join", ""), Is.EqualTo(notified.Invoke("join", "")));
          }
        );
      });

      It("should notify all waiters on a location, but does not notify waiters on other locations", () => {
        const int WAIT_INDEX = 0;             // Waiters on this will be woken
        const int WAIT_FAKE = 1;              // Waiters on this will not be woken
        const int RUNNING_ = 2;               // Accounting of live agents
        const int NOTIFY_INDEX = 3;           // Accounting for too early timeouts
        const int NUMAGENT = 3;
        const int TIMEOUT_AGENT_MESSAGES = 2; // Number of messages for the timeout agent
        const int BUFFER_SIZE = 4;
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT + 1).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING_, 1);
                if (i == NUMAGENT) {
                  agent.Report("B " + Atomics.Invoke("wait", i32a, WAIT_FAKE, 0, Agent.Timeout.Long));
                  // If this value is not 1, then the agent timeout before the main agent
                  // called Atomics.notify.
                  EcmaValue result = Atomics.Invoke("load", i32a, NOTIFY_INDEX) == 1 ? "timeout after Atomics.notify" : "timeout before Atomics.notify";
                  agent.Report("W " + result);
                } else {
                  agent.Report("A " + Atomics.Invoke("wait", i32a, WAIT_INDEX, 0));
                }
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * BUFFER_SIZE));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING_, NUMAGENT + 1);
            agent.TryYield();

            That(Atomics.Invoke("notify", i32a, WAIT_INDEX, NUMAGENT), Is.EqualTo(NUMAGENT));
            Atomics.Invoke("store", i32a, NOTIFY_INDEX, 1);

            EcmaValue reports = EcmaArray.Of();
            for (int i = 0; i < NUMAGENT + TIMEOUT_AGENT_MESSAGES; i++) {
              reports.Invoke("push", agent.GetReport());
            }
            reports.Invoke("sort");
            That(reports, Is.EquivalentTo(new[] { "A ok", "A ok", "A ok", "B timed-out", "W timeout after Atomics.notify" }));
          }
        );
      });

      It("should convert an undefined index to 0", () => {
        const int NUMAGENT = 2;
        TestWithAgent(
          workers: Enumerable.Range(0, NUMAGENT).Select<int, Agent.WorkerStart>(i => {
            return agent => {
              agent.ReceiveBroadcast(sab => {
                EcmaValue i32a = Int32Array.Construct(sab);
                Atomics.Invoke("add", i32a, RUNNING, 1);
                agent.Report(Atomics.Invoke("wait", i32a, 0, 0));
                agent.Leaving();
              });
            };
          }),
          main: agent => {
            EcmaValue i32a = Int32Array.Construct(SharedArrayBuffer.Construct(Int32Array["BYTES_PER_ELEMENT"] * 5));
            agent.SafeBroadcast(i32a);
            agent.WaitUntil(i32a, RUNNING, NUMAGENT);
            agent.TryYield();

            EcmaValue woken = 0;
            while ((woken = Atomics.Invoke("notify", i32a, Undefined, 1)) == 0) ;
            That(woken, Is.EqualTo(1));
            That(agent.GetReport(), Is.EqualTo("ok"));

            woken = 0;
            while ((woken = Atomics.Invoke("notify", i32a)) == 0) ;
            That(woken, Is.EqualTo(1));
            That(agent.GetReport(), Is.EqualTo("ok"));
          }
        );
      });
    }

    private void TestWithAgent(Agent.WorkerStart worker, System.Action<Agent> main) {
      RuntimeExecution.Current.CanSuspend = true;
      using (Agent agent = new Agent()) {
        agent.Start(worker);
        main(agent);
      }
    }

    private void TestWithAgent(System.Collections.Generic.IEnumerable<Agent.WorkerStart> workers, System.Action<Agent> main) {
      RuntimeExecution.Current.CanSuspend = true;
      using (Agent agent = new Agent()) {
        foreach (Agent.WorkerStart w in workers) {
          agent.Start(w);
        }
        main(agent);
      }
    }

    private void VerifyCommonValidation(bool skipToNumberValue = false) {
      It("should throw a TypeError if first argument is not a TypedArray", () => {
        Case((_, Null, 0, 0), Throws.TypeError);
        Case((_, Undefined, 0, 0), Throws.TypeError);
        Case((_, true, 0, 0), Throws.TypeError);
        Case((_, false, 0, 0), Throws.TypeError);
        Case((_, Boolean.Construct(true), 0, 0), Throws.TypeError);
        Case((_, 10, 0, 0), Throws.TypeError);
        Case((_, 3.14, 0, 0), Throws.TypeError);
        Case((_, Number.Construct(4), 0, 0), Throws.TypeError);
        Case((_, "Hi there", 0, 0), Throws.TypeError);
        Case((_, Date.Construct(), 0, 0), Throws.TypeError);
        Case((_, RegExp.Construct("a*utomaton", "g"), 0, 0), Throws.TypeError);
        Case((_, CreateObject(new { password = "qumquat" }), 0, 0), Throws.TypeError);
        Case((_, Global.DataView.Construct(Global.ArrayBuffer.Construct(10)), 0, 0), Throws.TypeError);
        Case((_, Global.ArrayBuffer.Construct(128), 0, 0), Throws.TypeError);
        Case((_, Global.SharedArrayBuffer.Construct(128), 0, 0), Throws.TypeError);
        Case((_, Error.Construct("Ouch"), 0, 0), Throws.TypeError);
        Case((_, EcmaArray.Of(1, 1, 2, 3, 5, 8), 0, 0), Throws.TypeError);
        Case((_, RuntimeFunction.Create(x => -x), 0, 0), Throws.TypeError);
        Case((_, new Symbol("halleluja"), 0, 0), Throws.TypeError);
        Case((_, Object, 0, 0), Throws.TypeError);
        Case((_, Global.Int32Array, 0, 0), Throws.TypeError);
        Case((_, Date, 0, 0), Throws.TypeError);
        Case((_, Math, 0, 0), Throws.TypeError);
        Case((_, Atomics, 0, 0), Throws.TypeError);
      });

      It("should throw a TypeError for non shared views", () => {
        EcmaValue buffer = Global.ArrayBuffer.Construct(16);
        TestWithTypedArrayConstructors(TA => {
          Case((_, TA.Construct(buffer), 0, 0), Throws.TypeError);
        }, IntArrayCtors);
      });

      It("should throw a TypeError for invalid TypedArray type", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(16);
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          Case((_, TA.Construct(buffer), obj, obj), Throws.TypeError);
        }, new[] { Global.Uint8ClampedArray, Global.Float32Array, Global.Float64Array });
      });

      It("should throw a RangeError for out-of-bound indices", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue view = TA.Construct(buffer);
          Case((_, view, -1, 0), Throws.RangeError);
          Case((_, view, view["length"], 0), Throws.RangeError);
          Case((_, view, view["length"] * 2, 0), Throws.RangeError);
          Case((_, view, Infinity, 0), Throws.RangeError);
          Case((_, view, -Infinity, 0), Throws.RangeError);
          Case((_, view, CreateObject(valueOf: () => 125), 0), Throws.RangeError);
          Case((_, view, CreateObject(toString: () => "125", valueOf: () => Object.Construct()), 0), Throws.RangeError);

          Case((_, view, ((EcmaValue)0) / -1, 0), Throws.Nothing);
          Case((_, view, "-0", 0), Throws.Nothing);
          Case((_, view, Undefined, 0), Throws.Nothing);
          Case((_, view, NaN, 0), Throws.Nothing);
          Case((_, view, 0.5, 0), Throws.Nothing);
          Case((_, view, "0.5", 0), Throws.Nothing);
          Case((_, view, -0.9, 0), Throws.Nothing);
          Case((_, view, CreateObject(new { password = "qumquat" }), 0), Throws.Nothing);
          Case((_, view, view["length"] - 1, 0), Throws.Nothing);
          Case((_, view, CreateObject(valueOf: () => 0), 0), Throws.Nothing);
          Case((_, view, CreateObject(toString: () => "0", valueOf: () => Object.Construct()), 0), Throws.Nothing);
        }, IntArrayCtors);
      });

      It("should return abrupt from ToNumber(index)", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        TestWithTypedArrayConstructors(TA => {
          Case((_, TA.Construct(buffer), CreateObject(valueOf: ThrowTest262Exception), 0), Throws.Test262);
        }, IntArrayCtors);
      });

      It("should return abrupt from ToNumber(value)", () => {
        EcmaValue buffer = Global.SharedArrayBuffer.Construct(8);
        if (!skipToNumberValue) {
          TestWithTypedArrayConstructors(TA => {
            Case((_, TA.Construct(buffer), 0, CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
          }, IntArrayCtors);
        }
      });
    }
  }
}
