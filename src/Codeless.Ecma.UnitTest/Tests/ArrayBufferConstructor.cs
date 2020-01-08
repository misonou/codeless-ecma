using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ArrayBufferConstructor : TestBase {
    RuntimeFunction ArrayBuffer => Global.ArrayBuffer;
    RuntimeFunction DataView => Global.DataView;

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "ArrayBuffer", 1, ArrayBuffer.Prototype);
      That(GlobalThis, Has.OwnProperty("ArrayBuffer", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(Noop), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ArrayBufferPrototype)));
      });

      It("should derive [[Prototype]] internal slot from NewTarget", () => {
        EcmaValue arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(8), Object);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(Object.Prototype));

        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = RuntimeFunction.Create(() => Array.Prototype) }));
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(16), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(Array.Prototype));
      });

      It("should use %ArrayBufferPrototype% if NewTarget.prototype is not an object", () => {
        EcmaValue arrayBuffer;
        EcmaValue newTarget = RuntimeFunction.Create(Noop);
        newTarget["prototype"] = Undefined;
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(1), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is undefined");

        newTarget["prototype"] = Null;
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(2), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is null");

        newTarget["prototype"] = true;
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(3), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is a Boolean");

        newTarget["prototype"] = "";
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(4), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is a String");

        newTarget["prototype"] = new Symbol();
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(5), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is a Symbol");

        newTarget["prototype"] = 1;
        arrayBuffer = Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(6), newTarget);
        That(Object.Invoke("getPrototypeOf", arrayBuffer), Is.EqualTo(ArrayBuffer.Prototype), "newTarget.prototype is a Number");
      });

      It("should throw a TypeError if ArrayBuffer is called as a function", () => {
        That(() => ArrayBuffer.Call(Undefined), Throws.TypeError);
        That(() => ArrayBuffer.Call(Undefined, 42), Throws.TypeError);
      });

      It("should convert the `length` parameter to a value numeric index value", () => {
        EcmaValue obj1 = CreateObject(valueOf: () => 42);
        EcmaValue obj2 = CreateObject(toString: () => 42);
        That(ArrayBuffer.Construct(obj1)["byteLength"], Is.EqualTo(42), "object's valueOf");
        That(ArrayBuffer.Construct(obj2)["byteLength"], Is.EqualTo(42), "object's toString");
        That(ArrayBuffer.Construct("")["byteLength"], Is.EqualTo(0), "the Empty string");
        That(ArrayBuffer.Construct("0")["byteLength"], Is.EqualTo(0), "string '0'");
        That(ArrayBuffer.Construct("1")["byteLength"], Is.EqualTo(1), "string '1'");
        That(ArrayBuffer.Construct(true)["byteLength"], Is.EqualTo(1), "true");
        That(ArrayBuffer.Construct(false)["byteLength"], Is.EqualTo(0), "false");
        That(ArrayBuffer.Construct(NaN)["byteLength"], Is.EqualTo(0), "NaN");
        That(ArrayBuffer.Construct(Null)["byteLength"], Is.EqualTo(0), "null");
        That(ArrayBuffer.Construct(Undefined)["byteLength"], Is.EqualTo(0), "undefined");
        That(ArrayBuffer.Construct(0.1)["byteLength"], Is.EqualTo(0), "0.1");
        That(ArrayBuffer.Construct(0.9)["byteLength"], Is.EqualTo(0), "0.9");
        That(ArrayBuffer.Construct(1.1)["byteLength"], Is.EqualTo(1), "1.1");
        That(ArrayBuffer.Construct(1.9)["byteLength"], Is.EqualTo(1), "1.9");
        That(ArrayBuffer.Construct(-0.1)["byteLength"], Is.EqualTo(0), "-0.1");
        That(ArrayBuffer.Construct(-0.99999)["byteLength"], Is.EqualTo(0), "-0.99999");
      });

      It("should return an empty instance if length is absent", () => {
        That(ArrayBuffer.Construct()["byteLength"], Is.EqualTo(0));
      });

      It("should accept zero as the `length` parameter", () => {
        That(ArrayBuffer.Construct(0)["byteLength"], Is.EqualTo(0));
        That(ArrayBuffer.Construct(-0d)["byteLength"], Is.EqualTo(0));
      });

      It("should throw a RangeError if length represents an integer < 0", () => {
        That(() => ArrayBuffer.Construct(-1), Throws.RangeError);
        That(() => ArrayBuffer.Construct(-1.1), Throws.RangeError);
        That(() => ArrayBuffer.Construct(-Infinity), Throws.RangeError);
      });

      It("should throw a RangeError if length >= 2 ** 53", () => {
        That(() => ArrayBuffer.Construct(9007199254740992), Throws.RangeError);
        That(() => ArrayBuffer.Construct(Infinity), Throws.RangeError);
      });

      It("should return abrupt from ToIndex(length)", () => {
        That(() => ArrayBuffer.Construct(new Symbol()), Throws.TypeError);
        That(() => ArrayBuffer.Construct(CreateObject(valueOf: ThrowTest262Exception)), Throws.Test262);
      });

      It("should create the new ArrayBuffer instance prior to allocating the Data Block", () => {
        EcmaValue newTarget = RuntimeFunction.Create(Noop).Bind(Null);
        Object.Invoke("defineProperty", newTarget, "prototype", CreateObject(new { get = ThrowTest262Exception }));
        That(() => Reflect.Invoke("construct", ArrayBuffer, EcmaArray.Of(9007199254740992), newTarget), Throws.Test262);
      });

      It("should initialize all bytes to zero", () => {
        EcmaValue view = DataView.Construct(ArrayBuffer.Construct(9));
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
    public void IsView(RuntimeFunction isView) {
      IsUnconstructableFunctionWLength(isView, "isView", 1);
      That(ArrayBuffer, Has.OwnProperty("isView", isView, EcmaPropertyAttributes.DefaultMethodProperty));

      Case(_, false);

      Case((_, Undefined), false);
      Case((_, Null), false);
      Case((_, 1), false);
      Case((_, ""), false);
      Case((_, Object.Construct()), false);
      Case((_, EcmaArray.Of()), false);
      Case((_, ArrayBuffer.Construct(1)), false);

      EcmaValue DV = RuntimeFunction.Create(DerivedCtor).AsDerivedClassConstructorOf(DataView);
      Case((_, DataView.Construct(ArrayBuffer.Construct(1), 0, 0)), true);
      Case((_, DV.Construct(ArrayBuffer.Construct(1), 0, 0)), true);
      Case((_, DataView), false);
      Case((_, DataView.Construct(ArrayBuffer.Construct(1), 0, 0)["buffer"]), false);

      TestWithTypedArrayConstructors(ctor => {
        EcmaValue TA = RuntimeFunction.Create(DerivedCtor).AsDerivedClassConstructorOf(ctor);
        Case((_, ctor.Construct()), true);
        Case((_, TA.Construct()), true);
        Case((_, ctor), false);
        Case((_, ctor.Construct()["buffer"]), false);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(ArrayBuffer, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(ArrayBuffer.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(ArrayBuffer, Is.EqualTo(ArrayBuffer));

      EcmaValue thisValue = new EcmaObject();
      Case(thisValue, Is.EqualTo(thisValue));
    }
  }
}
