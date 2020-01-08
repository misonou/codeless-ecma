using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class DataViewConstructor : TestBase {
    RuntimeFunction ArrayBuffer => Global.ArrayBuffer;
    RuntimeFunction SharedArrayBuffer => (RuntimeFunction)GlobalThis["SharedArrayBuffer"].ToObject();
    RuntimeFunction DataView => Global.DataView;

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "DataView", 1, DataView.Prototype);
      That(GlobalThis, Has.OwnProperty("DataView", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(ArrayBuffer.Construct(8)), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.DataViewPrototype)));
      });

      It("should throw a TypeError if ArrayBuffer is called as a function", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262WithMessage("NewTarget should be verified before byteOffset"));
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        That(() => DataView.Call(Undefined, buffer, obj), Throws.TypeError);
        That(() => DataView.Call(Undefined, sbuffer, obj), Throws.TypeError);
      });

      It("should throw a TypeError if buffer is not Object", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262WithMessage("buffer should be verified before byteOffset"));
        That(() => DataView.Construct(0, obj), Throws.TypeError);
        That(() => DataView.Construct(1, obj), Throws.TypeError);
        That(() => DataView.Construct("", obj), Throws.TypeError);
        That(() => DataView.Construct("buffer", obj), Throws.TypeError);
        That(() => DataView.Construct(true, obj), Throws.TypeError);
        That(() => DataView.Construct(false, obj), Throws.TypeError);
        That(() => DataView.Construct(NaN, obj), Throws.TypeError);
        That(() => DataView.Construct(new Symbol(), obj), Throws.TypeError);
      });

      It("should throw a TypeError if buffer does not have [[ArrayBufferData]]", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262WithMessage("buffer should be verified before byteOffset"));
        That(() => DataView.Construct(Object.Construct(), obj), Throws.TypeError, "{}");
        That(() => DataView.Construct(EcmaArray.Of(), obj), Throws.TypeError, "[]");
        That(() => DataView.Construct(Global.Int8Array.Construct(), obj), Throws.TypeError, "typedArray instance");
        That(() => DataView.Construct(DataView.Construct(ArrayBuffer.Construct(0)), obj), Throws.TypeError, "dataView instance");
        That(() => DataView.Construct(DataView.Construct(SharedArrayBuffer.Construct(0)), obj), Throws.TypeError, "dataView instance");
      });

      It("should throw a TypeError if the buffer is detached", () => {
        EcmaValue obj = CreateObject(valueOf: Intercept(() => 0));
        EcmaValue buffer = ArrayBuffer.Construct();
        DetachBuffer(buffer);

        That(() => DataView.Construct(buffer, obj), Throws.TypeError);
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should reuse buffer argument instead of making a new clone", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        That(DataView.Construct(buffer, 0)["buffer"], Is.EqualTo(buffer));
        That(DataView.Construct(buffer, 0)["buffer"], Is.EqualTo(buffer));

        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        That(DataView.Construct(sbuffer, 0)["buffer"], Is.EqualTo(sbuffer));
        That(DataView.Construct(sbuffer, 0)["buffer"], Is.EqualTo(sbuffer));
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        That(() => DataView.Construct(buffer, -1), Throws.RangeError);
        That(() => DataView.Construct(buffer, -Infinity), Throws.RangeError);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        That(() => DataView.Construct(sbuffer, -1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, -Infinity), Throws.RangeError);
      });

      It("should throw if buffer is detached during OrdinaryCreateFromConstructor", () => {
        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue called = false;
        EcmaValue byteOffset = CreateObject(valueOf: () => Return(called = true, 0));
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new {
          get = Intercept(() => {
            DetachBuffer(buffer);
            return DataView.Prototype;
          })
        }));

        That(() => Reflect.Invoke("construct", DataView, EcmaArray.Of(buffer, byteOffset), newTarget), Throws.TypeError);
        That(called, Is.EqualTo(true));
      });

      It("should return abrupt from newTarget's custom constructor prototype", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));

        EcmaValue buffer = ArrayBuffer.Construct(8);
        That(() => Reflect.Invoke("construct", DataView, EcmaArray.Of(buffer, 0), newTarget), Throws.Test262);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        That(() => Reflect.Invoke("construct", DataView, EcmaArray.Of(sbuffer, 0), newTarget), Throws.Test262);
      });

      It("should use DataView.Prototype if newTarget's prototype is not an Object", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop);
        newTarget["prototype"] = Null;

        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue result = Reflect.Invoke("construct", DataView, EcmaArray.Of(buffer, 0), newTarget);
        That(result["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(DataView.Prototype));

        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        EcmaValue sresult = Reflect.Invoke("construct", DataView, EcmaArray.Of(sbuffer, 0), newTarget);
        That(sresult["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sresult), Is.EqualTo(DataView.Prototype));
      });

      It("should use newTarget's custom constructor prototype if Object", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop);
        EcmaValue proto = Object.Construct();
        newTarget["prototype"] = proto;

        EcmaValue buffer = ArrayBuffer.Construct(8);
        EcmaValue result = Reflect.Invoke("construct", DataView, EcmaArray.Of(buffer, 0), newTarget);
        That(result["constructor"], Is.EqualTo(Object));
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(proto));

        EcmaValue sbuffer = SharedArrayBuffer.Construct(8);
        EcmaValue sresult = Reflect.Invoke("construct", DataView, EcmaArray.Of(sbuffer, 0), newTarget);
        That(sresult["constructor"], Is.EqualTo(Object));
        That(Object.Invoke("getPrototypeOf", sresult), Is.EqualTo(proto));
      });

      It("should return new instance from defined offset", () => {
        EcmaValue sample;
        EcmaValue buffer = ArrayBuffer.Construct(4);

        sample = DataView.Construct(buffer, 0);
        That(sample["byteLength"], Is.EqualTo(4), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(2), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 4);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(4), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, Undefined);
        That(sample["byteLength"], Is.EqualTo(4), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1, Undefined);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 2, Undefined);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(2), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3, Undefined);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 4, Undefined);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(4), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        buffer = SharedArrayBuffer.Construct(4);
        sample = DataView.Construct(buffer, 0);
        That(sample["byteLength"], Is.EqualTo(4), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(2), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 4);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(4), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, Undefined);
        That(sample["byteLength"], Is.EqualTo(4), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1, Undefined);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 2, Undefined);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(2), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3, Undefined);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 4, Undefined);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(4), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

      });

      It("should return new instance from defined length and offset", () => {
        EcmaValue sample;
        EcmaValue buffer = ArrayBuffer.Construct(3);

        sample = DataView.Construct(buffer, 1, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1, 0);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 3);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3, 0);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 1);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        buffer = SharedArrayBuffer.Construct(3);
        sample = DataView.Construct(buffer, 1, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 1, 0);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(1), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 3);
        That(sample["byteLength"], Is.EqualTo(3), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 3, 0);
        That(sample["byteLength"], Is.EqualTo(0), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(3), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 1);
        That(sample["byteLength"], Is.EqualTo(1), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));

        sample = DataView.Construct(buffer, 0, 2);
        That(sample["byteLength"], Is.EqualTo(2), "sample.byteLength");
        That(sample["byteOffset"], Is.EqualTo(0), "sample.byteOffset");
        That(sample["buffer"], Is.EqualTo(buffer), "sample.buffer");
        That(sample["constructor"], Is.EqualTo(DataView));
        That(Object.Invoke("getPrototypeOf", sample), Is.EqualTo(DataView.Prototype));
      });

      It("should throw a RangeError if offset > bufferByteLength", () => {
        EcmaValue buffer = ArrayBuffer.Construct(1);
        That(() => DataView.Construct(buffer, 2), Throws.RangeError);
        That(() => DataView.Construct(buffer, Infinity), Throws.RangeError);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(1);
        That(() => DataView.Construct(sbuffer, 2), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, Infinity), Throws.RangeError);
      });

      It("should throw a RangeError if offset + viewByteLength > bufferByteLength", () => {
        EcmaValue buffer = ArrayBuffer.Construct(3);
        That(() => DataView.Construct(buffer, 0, 4), Throws.RangeError);
        That(() => DataView.Construct(buffer, 1, 3), Throws.RangeError);
        That(() => DataView.Construct(buffer, 2, 2), Throws.RangeError);
        That(() => DataView.Construct(buffer, 3, 1), Throws.RangeError);
        That(() => DataView.Construct(buffer, 4, 0), Throws.RangeError);
        That(() => DataView.Construct(buffer, 4, -1), Throws.RangeError);
        That(() => DataView.Construct(buffer, 4, -Infinity), Throws.RangeError);
        That(() => DataView.Construct(buffer, 0, Infinity), Throws.RangeError);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(3);
        That(() => DataView.Construct(sbuffer, 0, 4), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 1, 3), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 2, 2), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 3, 1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 4, 0), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 4, -1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 4, -Infinity), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 0, Infinity), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteOffset) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(2);
        That(() => DataView.Construct(buffer, -1), Throws.RangeError);
        That(() => DataView.Construct(buffer, -Infinity), Throws.RangeError);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(2);
        That(() => DataView.Construct(sbuffer, -1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, -Infinity), Throws.RangeError);
      });

      It("should throw a RangeError if ToInteger(byteLength) < 0", () => {
        EcmaValue buffer = ArrayBuffer.Construct(2);
        That(() => DataView.Construct(buffer, 0, -1), Throws.RangeError);
        That(() => DataView.Construct(buffer, 0, -Infinity), Throws.RangeError);
        That(() => DataView.Construct(buffer, 1, -1), Throws.RangeError);
        That(() => DataView.Construct(buffer, 2, -Infinity), Throws.RangeError);

        EcmaValue sbuffer = SharedArrayBuffer.Construct(2);
        That(() => DataView.Construct(sbuffer, -1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, -Infinity), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 1, -1), Throws.RangeError);
        That(() => DataView.Construct(sbuffer, 2, -Infinity), Throws.RangeError);
      });

      It("should perform ToIndex conversions on byteOffset", () => {
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(valueOf: () => 4);
        EcmaValue ab = ArrayBuffer.Construct(42);
        EcmaValue sample;

        sample = DataView.Construct(ab, -0);
        That(sample["byteOffset"], Is.EqualTo(0), "-0");

        sample = DataView.Construct(ab, obj1);
        That(sample["byteOffset"], Is.EqualTo(3), "object's valueOf");

        sample = DataView.Construct(ab, obj2);
        That(sample["byteOffset"], Is.EqualTo(4), "object's toString");

        sample = DataView.Construct(ab, "");
        That(sample["byteOffset"], Is.EqualTo(0), "the Empty string");

        sample = DataView.Construct(ab, "0");
        That(sample["byteOffset"], Is.EqualTo(0), "string '0'");

        sample = DataView.Construct(ab, "1");
        That(sample["byteOffset"], Is.EqualTo(1), "string '1'");

        sample = DataView.Construct(ab, true);
        That(sample["byteOffset"], Is.EqualTo(1), "true");

        sample = DataView.Construct(ab, false);
        That(sample["byteOffset"], Is.EqualTo(0), "false");

        sample = DataView.Construct(ab, NaN);
        That(sample["byteOffset"], Is.EqualTo(0), "NaN");

        sample = DataView.Construct(ab, Null);
        That(sample["byteOffset"], Is.EqualTo(0), "null");

        sample = DataView.Construct(ab, Undefined);
        That(sample["byteOffset"], Is.EqualTo(0), "undefined");

        sample = DataView.Construct(ab, 0.1);
        That(sample["byteOffset"], Is.EqualTo(0), "0.1");

        sample = DataView.Construct(ab, 0.9);
        That(sample["byteOffset"], Is.EqualTo(0), "0.9");

        sample = DataView.Construct(ab, 1.1);
        That(sample["byteOffset"], Is.EqualTo(1), "1.1");

        sample = DataView.Construct(ab, 1.9);
        That(sample["byteOffset"], Is.EqualTo(1), "1.9");

        sample = DataView.Construct(ab, -0.1);
        That(sample["byteOffset"], Is.EqualTo(0), "-0.1");

        sample = DataView.Construct(ab, -0.99999);
        That(sample["byteOffset"], Is.EqualTo(0), "-0.99999");
      });

      It("should perform ToIndex conversions on byteLength", () => {
        EcmaValue obj1 = CreateObject(valueOf: () => 3);
        EcmaValue obj2 = CreateObject(valueOf: () => 4);
        EcmaValue ab = ArrayBuffer.Construct(42);
        EcmaValue sample;

        sample = DataView.Construct(ab, 0, -0);
        That(sample["byteLength"], Is.EqualTo(0), "-0");

        sample = DataView.Construct(ab, 0, obj1);
        That(sample["byteLength"], Is.EqualTo(3), "object's valueOf");

        sample = DataView.Construct(ab, 0, obj2);
        That(sample["byteLength"], Is.EqualTo(4), "object's toString");

        sample = DataView.Construct(ab, 0, "");
        That(sample["byteLength"], Is.EqualTo(0), "the Empty string");

        sample = DataView.Construct(ab, 0, "0");
        That(sample["byteLength"], Is.EqualTo(0), "string '0'");

        sample = DataView.Construct(ab, 0, "1");
        That(sample["byteLength"], Is.EqualTo(1), "string '1'");

        sample = DataView.Construct(ab, 0, true);
        That(sample["byteLength"], Is.EqualTo(1), "true");

        sample = DataView.Construct(ab, 0, false);
        That(sample["byteLength"], Is.EqualTo(0), "false");

        sample = DataView.Construct(ab, 0, NaN);
        That(sample["byteLength"], Is.EqualTo(0), "NaN");

        sample = DataView.Construct(ab, 0, Null);
        That(sample["byteLength"], Is.EqualTo(0), "null");

        sample = DataView.Construct(ab, 0, 0.1);
        That(sample["byteLength"], Is.EqualTo(0), "0.1");

        sample = DataView.Construct(ab, 0, 0.9);
        That(sample["byteLength"], Is.EqualTo(0), "0.9");

        sample = DataView.Construct(ab, 0, 1.1);
        That(sample["byteLength"], Is.EqualTo(1), "1.1");

        sample = DataView.Construct(ab, 0, 1.9);
        That(sample["byteLength"], Is.EqualTo(1), "1.9");

        sample = DataView.Construct(ab, 0, -0.1);
        That(sample["byteLength"], Is.EqualTo(0), "-0.1");

        sample = DataView.Construct(ab, 0, -0.99999);
        That(sample["byteLength"], Is.EqualTo(0), "-0.99999");
      });

      It("should return abrupt from ToNumber(byteOffset)", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue ab = ArrayBuffer.Construct(0);
        That(() => DataView.Construct(ab, obj), Throws.Test262);
        That(() => DataView.Construct(ab, new Symbol()), Throws.TypeError);
      });

      It("should return abrupt from ToNumber(byteLength)", () => {
        EcmaValue obj1 = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue obj2 = CreateObject(toString: ThrowTest262Exception);
        EcmaValue ab = ArrayBuffer.Construct(0);
        That(() => DataView.Construct(ab, 0, obj1), Throws.Test262);
        That(() => DataView.Construct(ab, 0, obj2), Throws.Test262);
        That(() => DataView.Construct(ab, 0, new Symbol()), Throws.TypeError);
      });
    }
  }
}
