using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class SharedArrayBufferPrototype : TestBase {
    RuntimeFunction SharedArrayBuffer => Global.SharedArrayBuffer;

    [Test]
    public void Properties() {
      That(SharedArrayBuffer, Has.OwnProperty("prototype", SharedArrayBuffer.Prototype, EcmaPropertyAttributes.None));
      That(SharedArrayBuffer.Prototype, Has.OwnProperty("constructor", SharedArrayBuffer, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(SharedArrayBuffer.Prototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "SharedArrayBuffer", EcmaPropertyAttributes.Configurable));
      That(SharedArrayBuffer.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(SharedArrayBuffer.Prototype), Is.EqualTo("[object SharedArrayBuffer]"));
    }

    [Test, RuntimeFunctionInjection]
    public void ByteLength(RuntimeFunction byteLength) {
      IsUnconstructableFunctionWLength(byteLength, "get byteLength", 0);
      That(SharedArrayBuffer.Prototype, Has.OwnProperty("byteLength", EcmaPropertyAttributes.Configurable));
      That(SharedArrayBuffer.Prototype.GetOwnProperty("byteLength").Set, Is.Undefined);

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[ArrayBufferData]]", () => {
        Case(SharedArrayBuffer.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Global.Int8Array.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(SharedArrayBuffer.Construct(8), 0), Throws.TypeError);
      });

      It("should throw a TypeError if `this` is a ArrayBuffer", () => {
        Case(Global.ArrayBuffer.Construct(0), Throws.TypeError);
      });

      It("should return value from [[ByteLength]] internal slot", () => {
        Case(SharedArrayBuffer.Construct(0), 0);
        Case(SharedArrayBuffer.Construct(42), 42);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Slice(RuntimeFunction slice) {
      IsUnconstructableFunctionWLength(slice, "slice", 2);
      That(SharedArrayBuffer.Prototype, Has.OwnProperty("slice", slice, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError when this is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(false, Throws.TypeError);
        Case(0, Throws.TypeError);
        Case("", Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);
      });

      It("should throw a TypeError exception when `this` does not have a [[ArrayBufferData]]", () => {
        Case(SharedArrayBuffer.Prototype, Throws.TypeError);
        Case(Object.Construct(), Throws.TypeError);
        Case(EcmaArray.Of(), Throws.TypeError);
        Case(Global.Int8Array.Construct(8), Throws.TypeError);
        Case(Global.DataView.Construct(SharedArrayBuffer.Construct(8), 0), Throws.TypeError);
      });

      It("should throw a TypeError if `this` is a ArrayBuffer", () => {
        Case(Global.ArrayBuffer.Construct(0), Throws.TypeError);
      });

      It("should default the `start` index to 0 if absent", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer)["byteLength"], Is.EqualTo(8));
        That(slice.Call(arrayBuffer, Undefined, 6)["byteLength"], Is.EqualTo(6));
      });

      It("should return zero-length buffer if `start` index exceeds `end` index", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 5, 4)["byteLength"], Is.EqualTo(0));
      });

      It("should clamp large `start` index  to [[ArrayBufferByteLength]]", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 10, 8)["byteLength"], Is.EqualTo(0));
        That(slice.Call(arrayBuffer, 0x100000000, 7)["byteLength"], Is.EqualTo(0));
        That(slice.Call(arrayBuffer, Infinity, 6)["byteLength"], Is.EqualTo(0));
      });

      It("should normalize negative `start` index relative to [[ArrayBufferByteLength]]", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, -5, 6)["byteLength"], Is.EqualTo(3));
        That(slice.Call(arrayBuffer, -12, 6)["byteLength"], Is.EqualTo(6));
        That(slice.Call(arrayBuffer, -Infinity, 6)["byteLength"], Is.EqualTo(6));
      });

      It("should convert the `start` index parameter to an integral numeric value", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 4.5, 8)["byteLength"], Is.EqualTo(4));
        That(slice.Call(arrayBuffer, NaN, 8)["byteLength"], Is.EqualTo(8));
      });

      It("should default the `end` index to [[ArrayBufferByteLength]] if absent", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 6)["byteLength"], Is.EqualTo(2));
        That(slice.Call(arrayBuffer, 6, Undefined)["byteLength"], Is.EqualTo(2));
      });

      It("should clamp large `end` index  to [[ArrayBufferByteLength]]", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 1, 12)["byteLength"], Is.EqualTo(7));
        That(slice.Call(arrayBuffer, 2, 0x100000000)["byteLength"], Is.EqualTo(6));
        That(slice.Call(arrayBuffer, 3, Infinity)["byteLength"], Is.EqualTo(5));
      });

      It("should normalize negative `end` index relative to [[ArrayBufferByteLength]]", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 2, -4)["byteLength"], Is.EqualTo(2));
        That(slice.Call(arrayBuffer, 2, -10)["byteLength"], Is.EqualTo(0));
        That(slice.Call(arrayBuffer, 2, -Infinity)["byteLength"], Is.EqualTo(0));
      });

      It("should convert the `end` index parameter to an integral numeric value", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        That(slice.Call(arrayBuffer, 0, 4.5)["byteLength"], Is.EqualTo(4));
        That(slice.Call(arrayBuffer, 0, NaN)["byteLength"], Is.EqualTo(0));
      });

      It("should convert start and end index in correct order", () => {
        slice.Call(SharedArrayBuffer.Construct(8),
          CreateObject(new { valueOf = Intercept(() => 0, "start") }),
          CreateObject(new { valueOf = Intercept(() => 0, "end") }));
        That(Logs, Is.EquivalentTo(new[] { "start", "end" }));
      });

      It("should throw a TypeError if `constructor` property is not an object", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        Case(Object.Invoke("assign", arrayBuffer, CreateObject(new { constructor = Null })), Throws.TypeError);
        Case(Object.Invoke("assign", arrayBuffer, CreateObject(new { constructor = true })), Throws.TypeError);
        Case(Object.Invoke("assign", arrayBuffer, CreateObject(new { constructor = "" })), Throws.TypeError);
        Case(Object.Invoke("assign", arrayBuffer, CreateObject(new { constructor = 1 })), Throws.TypeError);
        Case(Object.Invoke("assign", arrayBuffer, CreateObject(new { constructor = new Symbol() })), Throws.TypeError);
      });

      It("should throw a TypeError if species constructor is not a constructor", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        EcmaValue speciesConstructor = Object.Construct();
        arrayBuffer["constructor"] = speciesConstructor;

        speciesConstructor[Symbol.Species] = Object.Construct();
        Case(arrayBuffer, Throws.TypeError);
        speciesConstructor[Symbol.Species] = Function.Prototype;
        Case(arrayBuffer, Throws.TypeError);
      });

      It("should throw a TypeError if species constructor is not an object", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        EcmaValue speciesConstructor = Object.Construct();
        arrayBuffer["constructor"] = speciesConstructor;

        speciesConstructor[Symbol.Species] = true;
        Case(arrayBuffer, Throws.TypeError);
        speciesConstructor[Symbol.Species] = "";
        Case(arrayBuffer, Throws.TypeError);
        speciesConstructor[Symbol.Species] = 1;
        Case(arrayBuffer, Throws.TypeError);
        speciesConstructor[Symbol.Species] = new Symbol();
        Case(arrayBuffer, Throws.TypeError);
      });

      It("should use default constructor is `constructor` property is undefined", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        arrayBuffer["constructor"] = Undefined;
        That(Object.Invoke("getPrototypeOf", slice.Call(arrayBuffer)), Is.EqualTo(SharedArrayBuffer.Prototype));
      });

      It("should use default constructor if species constructor is null or undefined", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        EcmaValue speciesConstructor = Object.Construct();
        arrayBuffer["constructor"] = speciesConstructor;

        speciesConstructor[Symbol.Species] = Null;
        That(Object.Invoke("getPrototypeOf", slice.Call(arrayBuffer)), Is.EqualTo(SharedArrayBuffer.Prototype));
        speciesConstructor[Symbol.Species] = Undefined;
        That(Object.Invoke("getPrototypeOf", slice.Call(arrayBuffer)), Is.EqualTo(SharedArrayBuffer.Prototype));
      });

      It("does not throw TypeError if new SharedArrayBuffer is too large", () => {
        EcmaValue speciesConstructor = Object.Construct();
        speciesConstructor[Symbol.Species] = RuntimeFunction.Create((length) => {
          return SharedArrayBuffer.Construct(10);
        });
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        arrayBuffer["constructor"] = speciesConstructor;
        That(slice.Call(arrayBuffer)["byteLength"], Is.EqualTo(10));
      });

      It("should throw a TypeError if new SharedArrayBuffer is too small", () => {
        EcmaValue speciesConstructor = Object.Construct();
        speciesConstructor[Symbol.Species] = RuntimeFunction.Create((length) => {
          return SharedArrayBuffer.Construct(4);
        });
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        arrayBuffer["constructor"] = speciesConstructor;
        Case(arrayBuffer, Throws.TypeError);
      });

      It("should throw a TypeError if new object is not an SharedArrayBuffer instance", () => {
        EcmaValue speciesConstructor = Object.Construct();
        speciesConstructor[Symbol.Species] = RuntimeFunction.Create((length) => {
          return Object.Construct();
        });
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        arrayBuffer["constructor"] = speciesConstructor;
        Case(arrayBuffer, Throws.TypeError);
      });

      It("should throw a TypeError if species constructor returns `this` value", () => {
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        EcmaValue speciesConstructor = Object.Construct();
        speciesConstructor[Symbol.Species] = RuntimeFunction.Create((length) => {
          return arrayBuffer;
        });
        arrayBuffer["constructor"] = speciesConstructor;
        Case(arrayBuffer, Throws.TypeError);
      });

      It("should create new SharedArrayBuffer instance from SpeciesConstructor", () => {
        EcmaValue resultBuffer = default;
        EcmaValue speciesConstructor = Object.Construct();
        speciesConstructor[Symbol.Species] = RuntimeFunction.Create((length) => {
          return resultBuffer = SharedArrayBuffer.Construct(length);
        });
        EcmaValue arrayBuffer = SharedArrayBuffer.Construct(8);
        arrayBuffer["constructor"] = speciesConstructor;
        EcmaValue result = slice.Call(arrayBuffer);
        That(result, Is.EqualTo(resultBuffer));
      });
    }
  }
}
