using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SharedArrayBufferConstructor : TestBase {
    RuntimeFunction SharedArrayBuffer => Global.SharedArrayBuffer;
    RuntimeFunction DataView => Global.DataView;

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "SharedArrayBuffer", 1, SharedArrayBuffer.Prototype);
      That(GlobalThis, Has.OwnProperty("SharedArrayBuffer", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(Noop), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.SharedArrayBufferPrototype)));
      });

      It("should derive [[Prototype]] internal slot from NewTarget", () => {
        EcmaValue arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(8), Object);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(Object.Prototype));

        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = RuntimeFunction.Create(() => Array.Prototype) }));
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(16), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(Array.Prototype));
      });

      It("should use %SharedArrayBufferPrototype% if NewTarget.prototype is not an object", () => {
        EcmaValue arrayBuffer;
        EcmaValue newTarget = RuntimeFunction.Create(Noop);
        newTarget["prototype"] = Undefined;
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(1), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is undefined");

        newTarget["prototype"] = Null;
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(2), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is null");

        newTarget["prototype"] = true;
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(3), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is a Boolean");

        newTarget["prototype"] = "";
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(4), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is a String");

        newTarget["prototype"] = new Symbol();
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(5), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is a Symbol");

        newTarget["prototype"] = 1;
        arrayBuffer = Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(6), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(SharedArrayBuffer.Prototype), "newTarget.prototype is a Number");
      });

      It("should throw a TypeError if ArrayBuffer is called as a function", () => {
        That(() => SharedArrayBuffer.Call(Undefined), Throws.TypeError);
        That(() => SharedArrayBuffer.Call(Undefined, 42), Throws.TypeError);
      });

      It("should convert the `length` parameter to a value numeric index value", () => {
        EcmaValue obj1 = CreateObject(valueOf: () => 42);
        EcmaValue obj2 = CreateObject(toString: () => 42);
        That(SharedArrayBuffer.Construct(obj1)["byteLength"], Is.EqualTo(42), "object's valueOf");
        That(SharedArrayBuffer.Construct(obj2)["byteLength"], Is.EqualTo(42), "object's toString");
        That(SharedArrayBuffer.Construct("")["byteLength"], Is.EqualTo(0), "the Empty string");
        That(SharedArrayBuffer.Construct("0")["byteLength"], Is.EqualTo(0), "string '0'");
        That(SharedArrayBuffer.Construct("1")["byteLength"], Is.EqualTo(1), "string '1'");
        That(SharedArrayBuffer.Construct(true)["byteLength"], Is.EqualTo(1), "true");
        That(SharedArrayBuffer.Construct(false)["byteLength"], Is.EqualTo(0), "false");
        That(SharedArrayBuffer.Construct(NaN)["byteLength"], Is.EqualTo(0), "NaN");
        That(SharedArrayBuffer.Construct(Null)["byteLength"], Is.EqualTo(0), "null");
        That(SharedArrayBuffer.Construct(Undefined)["byteLength"], Is.EqualTo(0), "undefined");
        That(SharedArrayBuffer.Construct(0.1)["byteLength"], Is.EqualTo(0), "0.1");
        That(SharedArrayBuffer.Construct(0.9)["byteLength"], Is.EqualTo(0), "0.9");
        That(SharedArrayBuffer.Construct(1.1)["byteLength"], Is.EqualTo(1), "1.1");
        That(SharedArrayBuffer.Construct(1.9)["byteLength"], Is.EqualTo(1), "1.9");
        That(SharedArrayBuffer.Construct(-0.1)["byteLength"], Is.EqualTo(0), "-0.1");
        That(SharedArrayBuffer.Construct(-0.99999)["byteLength"], Is.EqualTo(0), "-0.99999");
      });

      It("should return an empty instance if length is absent", () => {
        That(SharedArrayBuffer.Construct()["byteLength"], Is.EqualTo(0));
      });

      It("should accept zero as the `length` parameter", () => {
        That(SharedArrayBuffer.Construct(0)["byteLength"], Is.EqualTo(0));
        That(SharedArrayBuffer.Construct(-0d)["byteLength"], Is.EqualTo(0));
      });

      It("should throw a RangeError if length represents an integer < 0", () => {
        That(() => SharedArrayBuffer.Construct(-1), Throws.RangeError);
        That(() => SharedArrayBuffer.Construct(-1.1), Throws.RangeError);
        That(() => SharedArrayBuffer.Construct(-Infinity), Throws.RangeError);
      });

      It("should throw a RangeError if requested Data Block is too large", () => {
        That(() => SharedArrayBuffer.Construct(9007199254740992), Throws.RangeError);
        That(() => SharedArrayBuffer.Construct(Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToIndex(length)", () => {
        That(() => SharedArrayBuffer.Construct(new Symbol()), Throws.TypeError);
        That(() => SharedArrayBuffer.Construct(CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
      });

      It("should create the new ArrayBuffer instance prior to allocating the Data Block", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        That(() => Reflect.Invoke("construct", SharedArrayBuffer, EcmaArray.Of(9007199254740992), newTarget), Throws.Test262);
      });

      It("should initialize all bytes to zero", () => {
        EcmaValue view = DataView.Construct(SharedArrayBuffer.Construct(9));
        That(view.Invoke("getUint8", 0), Is.EqualTo(0));
        That(view.Invoke("getUint8", 1), Is.EqualTo(0));
        That(view.Invoke("getUint8", 2), Is.EqualTo(0));
        That(view.Invoke("getUint8", 3), Is.EqualTo(0));
        That(view.Invoke("getUint8", 4), Is.EqualTo(0));
        That(view.Invoke("getUint8", 5), Is.EqualTo(0));
        That(view.Invoke("getUint8", 6), Is.EqualTo(0));
        That(view.Invoke("getUint8", 7), Is.EqualTo(0));
        That(view.Invoke("getUint8", 8), Is.EqualTo(0));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(SharedArrayBuffer, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(SharedArrayBuffer.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(SharedArrayBuffer, Is.EqualTo(SharedArrayBuffer));

      EcmaValue thisValue = new EcmaObject();
      Case(thisValue, Is.EqualTo(thisValue));
    }
  }
}
