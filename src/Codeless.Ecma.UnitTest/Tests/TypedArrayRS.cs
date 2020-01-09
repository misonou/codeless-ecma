using Codeless.Ecma.Runtime;
using Codeless.Ecma.UnitTest.Harness;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class TypedArrayRS : TestBase {
    RuntimeFunction TypedArray => (RuntimeFunction)WellKnownObject.TypedArray;

    [Test]
    public void DefineOwnProperty() {
      It("should produce consistent canonicalization of NaN values", () => {
        TestWithTypedArrayConstructors(FA => {
          EcmaValue samples = FA.Construct(1);
          foreach (EcmaValue value in ByteConversionValues.NaNs.ForOf()) {
            EcmaValue controls = FA.Construct(EcmaArray.Of(value, value, value));
            Object.Invoke("defineProperty", samples, "0", CreateObject(new { value }));
            That(samples[0], Is.NaN);
            That(controls[0], Is.NaN);
          }
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should convert to correct values", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(initial));
          Object.Invoke("defineProperty", sample, "0", CreateObject(new { value }));
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("should return abrupt from the evaluation of ToNumber(desc.value)", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(() => Object.Invoke("defineProperty", sample, "0", CreateObject(new { value = obj })), Throws.Test262);
        });
      });

      It("should throw a TypeError if object has valid numeric index and a detached buffer", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        EcmaValue desc = CreateObject(new {
          value = 0,
          configurable = false,
          enumerable = true,
          writable = true
        });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(42);
          DetachBuffer(sample);
          That(() => Reflect.Invoke("defineProperty", sample, "0", desc), Throws.TypeError, "Throws TypeError on valid numeric index if instance has a detached buffer");
          That(() => Reflect.Invoke("defineProperty", sample, "7", CreateObject(new { value = obj })), Throws.Test262, "Return Abrupt before Detached Buffer check from ToNumber(desc.value)");
          That(Reflect.Invoke("defineProperty", sample, "-1", desc), Is.False, "Return false before Detached Buffer check when value is a negative number");
          That(Reflect.Invoke("defineProperty", sample, "1.1", desc), Is.False, "Return false before Detached Buffer check when value is not an integer");
          That(Reflect.Invoke("defineProperty", sample, "-0", desc), Is.False, "Return false before Detached Buffer check when value is -0");
          That(Reflect.Invoke("defineProperty", sample, "2", CreateObject(new { value = obj, configurable = true, enumerable = true, writable = true })), Is.False, "Return false before Detached Buffer check when desc configurable is true");
          That(Reflect.Invoke("defineProperty", sample, "3", CreateObject(new { value = obj, configurable = false, enumerable = false, writable = true })), Is.False, "Return false before Detached Buffer check when desc enumerable is false");
          That(Reflect.Invoke("defineProperty", sample, "3", CreateObject(new { value = obj, configurable = false, enumerable = true, writable = false })), Is.False, "Return false before Detached Buffer check when desc writable is false");
          That(Reflect.Invoke("defineProperty", sample, "42", desc), Is.False, "Return false before Detached Buffer check when key == [[ArrayLength]]");
          That(Reflect.Invoke("defineProperty", sample, "43", desc), Is.False, "Return false before Detached Buffer check when key > [[ArrayLength]]");
          That(Reflect.Invoke("defineProperty", sample, "5", CreateObject(new { get = Noop })), Is.False, "Return false before Detached Buffer check with accessor descriptor");
          That(Reflect.Invoke("defineProperty", sample, "6", CreateObject(new { configurable = false, enumerable = true, writable = true })), Is.True, "Return true before Detached Buffer check when desc value is not present");
        });
      });

      It("should throw a TypeError if object has valid numeric index and a detached buffer (honoring the Realm of the current execution context)", () => {
        EcmaValue other = new RuntimeRealm().GetRuntimeObject(WellKnownObject.Global);
        EcmaValue desc = CreateObject(new {
          value = 0,
          configurable = false,
          enumerable = true,
          writable = true
        });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue OtherTA = other[TA["name"]];
          EcmaValue sample = OtherTA.Construct(1);
          DetachBuffer(sample);
          That(() => Reflect.Invoke("defineProperty", sample, "0", desc), Throws.TypeError);
        });
      });

      It("should return false if numericIndex is >= [[ArrayLength]]", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Reflect.Invoke("defineProperty", sample, "2", CreateObject(new { value = 42, configurable = false, enumerable = true, writable = true })), Is.False, "numericIndex == length");
          That(Reflect.Invoke("defineProperty", sample, "3", CreateObject(new { value = 42, configurable = false, enumerable = true, writable = true })), Is.False, "numericIndex > length");
        });
      });

      It("should return false if numericIndex is < 0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Reflect.Invoke("defineProperty", sample, "-1", CreateObject(new { value = 42, configurable = false, enumerable = true, writable = true })), Is.False);
        });
      });

      It("should return false if numericIndex is -0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          That(Reflect.Invoke("defineProperty", sample, "-0", CreateObject(new { value = 42, configurable = false, enumerable = true, writable = true })), Is.False);
          That(sample[0], Is.EqualTo(0));
          That(sample["-0"], Is.Undefined);
        });
      });

      It("should set an ordinary property value if numeric key is not a CanonicalNumericIndex", () => {
        EcmaValue keys = EcmaArray.Of(
          "1.0",
          "+1",
          "1000000000000000000000",
          "0.0000001");
        EcmaValue dataDesc = CreateObject(new {
          value = 42,
          writable = true,
          configurable = true
        });
        EcmaValue fnGet = Noop;
        EcmaValue fnSet = Noop;
        EcmaValue acDesc = CreateObject(new {
          get = fnGet,
          set = fnSet,
          enumerable = true,
          configurable = false
        });
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue key in keys.ForOf()) {
            EcmaValue sample1 = TA.Construct();
            That(Reflect.Invoke("defineProperty", sample1, key, dataDesc), Is.True, "return true after defining data property [" + key.ToString() + "]");

            That(sample1[key], Is.EqualTo(42), "value is set to [" + key.ToString() + "]");
            That(sample1, Has.OwnProperty(EcmaPropertyKey.FromValue(key), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
            That(sample1[0], Is.Undefined, "no value is set on sample1[0]");
            That(sample1["length"], Is.EqualTo(0), "length is still 0");

            EcmaValue sample2 = TA.Construct();
            That(Reflect.Invoke("defineProperty", sample2, key, acDesc), Is.True, "return true after defining accessors property [" + key.ToString() + "]");

            EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", sample2, key);
            That(sample2, Has.OwnProperty(EcmaPropertyKey.FromValue(key), EcmaPropertyAttributes.Enumerable));
            That(desc["get"], Is.EqualTo(fnGet), "accessor's get [" + key.ToString() + "]");
            That(desc["set"], Is.EqualTo(fnSet), "accessor's set [" + key.ToString() + "]");
            That(sample2[0], Is.Undefined, "no value is set on sample2[0]");
            That(sample2["length"], Is.EqualTo(0), "length is still 0");

            EcmaValue sample3 = TA.Construct();
            Object.Invoke("preventExtensions", sample3);
            That(Reflect.Invoke("defineProperty", sample3, key, dataDesc), Is.False, "return false defining property on a non-extensible sample");
            That(Object.Invoke("getOwnPropertyDescriptor", sample3, key), Is.Undefined);

            EcmaValue sample4 = TA.Construct();
            Object.Invoke("preventExtensions", sample4);
            That(Reflect.Invoke("defineProperty", sample4, key, acDesc), Is.False, "return false defining property on a non-extensible sample");
            That(Object.Invoke("getOwnPropertyDescriptor", sample4, key), Is.Undefined);
          }
        });
      });

      It("should return false if numericIndex is not an integer", () => {
        EcmaValue desc = CreateObject(new {
          value = 42,
          configurable = false,
          enumerable = true,
          writable = true
        });
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);

          That(Reflect.Invoke("defineProperty", sample, "0.1", desc), Is.False, "0.1");
          That(sample[0], Is.EqualTo(0), "'0.1' - does not change the value for [0]");
          That(sample["0.1"], Is.Undefined, "'0.1' - does not define a value for ['0.1']");

          That(Reflect.Invoke("defineProperty", sample, "0.000001", desc), Is.False, "0.000001");
          That(sample[0], Is.EqualTo(0), "'0.000001' - does not change the value for [0]");
          That(sample["0.000001"], Is.Undefined, "'0.000001' - does not define a value for ['0.000001']");

          That(Reflect.Invoke("defineProperty", sample, "1.1", desc), Is.False, "1.1");
          That(sample[1], Is.EqualTo(0), "'1.1' - does not change the value for [1]");
          That(sample["1.1"], Is.Undefined, "'1.1' - does not define a value for ['1.1']");

          That(Reflect.Invoke("defineProperty", sample, "Infinity", desc), Is.False, "Infinity");
          That(sample[0], Is.EqualTo(0), "'Infinity' - does not change the value for [0]");
          That(sample[1], Is.EqualTo(0), "'Infinity' - does not change the value for [1]");
          That(sample["Infinity"], Is.Undefined, "'Infinity' - does not define a value for ['Infinity']");

          That(Reflect.Invoke("defineProperty", sample, "-Infinity", desc), Is.False, "-Infinity");
          That(sample[0], Is.EqualTo(0), "'-Infinity' - does not change the value for [0]");
          That(sample[1], Is.EqualTo(0), "'-Infinity' - does not change the value for [1]");
          That(sample["-Infinity"], Is.Undefined, "'-Infinity' - does not define a value for ['-Infinity']");
        });
      });

      It("should return false if key is a numeric index and Descriptor is an AccessorDescriptor", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          EcmaValue fnGet = RuntimeFunction.Create(() => 42);
          EcmaValue fnSet = Noop;

          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { get = fnGet, enumerable = true })), Is.False, "get accessor");
          That(sample[0], Is.EqualTo(0));

          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { set = fnSet, enumerable = true })), Is.False, "set accessor");
          That(sample[0], Is.EqualTo(0));

          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { get = fnGet, set = fnSet, enumerable = true })), Is.False, "get and set accessor");
          That(sample[0], Is.EqualTo(0));
        });
      });

      It("should return false if key is a numeric index and Desc.[[Configurable]] is true", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { value = 42, configurable = true, enumerable = true, writable = true })), Is.False);
          That(sample[0], Is.EqualTo(0));
        });
      });

      It("should return false if key is a numeric index and Desc.[[Enumerable]] is false", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { value = 42, configurable = false, enumerable = false, writable = true })), Is.False);
          That(sample[0], Is.EqualTo(0));
        });
      });

      It("should return false if key is a numeric index and Desc.[[Writable]] is false", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(2);
          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { value = 42, configurable = false, enumerable = true, writable = false })), Is.False);
          That(sample[0], Is.EqualTo(0));
        });
      });

      It("should return true after setting a valid numeric index key", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 42));
          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { value = 8, configurable = false, enumerable = true, writable = true })), Is.True);
          That(sample[0], Is.EqualTo(8));
          That(sample, Has.OwnProperty("0", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable));
        });
      });

      It("should define an ordinary property value if key is a Symbol", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          EcmaValue s1 = new Symbol("foo");
          That(Reflect.Invoke("defineProperty", sample, s1, CreateObject(new { value = 42, configurable = true })), Is.True);
          That(sample[s1], Is.EqualTo(42));
          That(sample, Has.OwnProperty(EcmaPropertyKey.FromValue(s1), EcmaPropertyAttributes.Configurable));

          EcmaValue s2 = new Symbol("bar");
          EcmaValue fnGet = Noop;
          EcmaValue fnSet = Noop;
          That(Reflect.Invoke("defineProperty", sample, s2, CreateObject(new { get = fnGet, set = fnSet, enumerable = true })), Is.True);
          That(sample, Has.OwnProperty(EcmaPropertyKey.FromValue(s2), EcmaPropertyAttributes.Enumerable));

          EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", sample, s2);
          That(desc["get"], Is.EqualTo(fnGet));
          That(desc["set"], Is.EqualTo(fnSet));
        });
      });

      It("cannot define a new non-numerical key on a non-extensible instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Object.Invoke("preventExtensions", sample);

          That(Reflect.Invoke("defineProperty", sample, "foo", CreateObject(new { value = 42 })), Is.False);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "foo"), Is.Undefined);

          That(Reflect.Invoke("defineProperty", sample, "bar", CreateObject(new { get = Noop, set = Noop, enumerable = false, configurable = true })), Is.False);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "bar"), Is.Undefined);
        });
      });

      It("should redefine a non-numerical key on a non-extensible instance", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          sample["foo"] = true;
          sample["bar"] = true;
          Object.Invoke("preventExtensions", sample);

          That(Reflect.Invoke("defineProperty", sample, "foo", CreateObject(new { value = 42 })), Is.True);
          That(sample, Has.OwnProperty("foo", 42, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable));

          EcmaValue fnGet = Noop;
          EcmaValue fnSet = Noop;
          That(Reflect.Invoke("defineProperty", sample, "bar", CreateObject(new { get = fnGet, set = fnSet, enumerable = false, configurable = false })), Is.True);

          EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", sample, "bar");
          That(sample, Has.OwnProperty("bar", EcmaPropertyAttributes.None));
          That(desc["get"], Is.EqualTo(fnGet));
          That(desc["set"], Is.EqualTo(fnSet));
        });
      });

      It("should set the value and return true", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(0, 0));
          That(Reflect.Invoke("defineProperty", sample, "0", CreateObject(new { value = 1 })), Is.True);
          That(Reflect.Invoke("defineProperty", sample, "1", CreateObject(new { value = 2 })), Is.True);
          That(sample[0], Is.EqualTo(1));
          That(sample[1], Is.EqualTo(2));
        });
      });

      It("should return false for non-numeric index property value if `this` is not extensible", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Object.Invoke("preventExtensions", sample);
          That(Reflect.Invoke("defineProperty", sample, "foo", CreateObject(new { value = 42 })), Is.False);
          That(Reflect.Invoke("getOwnPropertyDescriptor", sample, "foo"), Is.Undefined);

          EcmaValue s = new Symbol("1");
          That(Reflect.Invoke("defineProperty", sample, s, CreateObject(new { value = 42 })), Is.False);
          That(Reflect.Invoke("getOwnPropertyDescriptor", sample, s), Is.Undefined);
        });
      });

      It("should throw a TypeError and not modify the typed array when buffer is detached during value conversion", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(17));
          EcmaValue desc = CreateObject(new {
            value = CreateObject(valueOf: () => {
              DetachBuffer(sample);
              return 42;
            })
          });
          That(() => Reflect.Invoke("defineProperty", sample, 0, desc), Throws.TypeError);
        });
      });
    }

    [Test]
    public void Get() {
      It("does not throw on an instance with a detached buffer if key is not a number", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          sample["foo"] = "test262";

          DetachBuffer(sample);
          That(sample["undef"], Is.Undefined);
          That(sample["foo"], Is.EqualTo("test262"));
        });
      });

      It("does not throw on an instance with a detached buffer if key is a Symbol", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          DetachBuffer(sample);

          EcmaValue s = new Symbol("1");
          That(sample[s], Is.Undefined);
          sample[s] = "test262";
          That(sample[s], Is.EqualTo("test262"));
        });
      });

      It("should throw a TypeError if key has a numeric index and object has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          DetachBuffer(sample);

          That(() => sample[0], Throws.TypeError, "valid numeric index");
          That(() => sample["1.1"], Throws.TypeError, "detach buffer runs before checking for 1.1");
          That(() => sample["-0"], Throws.TypeError, "detach buffer runs before checking for -0");
          That(() => sample["-1"], Throws.TypeError, "detach buffer runs before checking for -1");
          That(() => sample["1"], Throws.TypeError, "detach buffer runs before checking for key == length");
          That(() => sample["2"], Throws.TypeError, "detach buffer runs before checking for key > length");
        });
      });

      It("should throw a TypeError if key has a numeric index and object has a detached (honoring the Realm of the current execution context)", () => {
        EcmaValue other = new RuntimeRealm().GetRuntimeObject(WellKnownObject.Global);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue OtherTA = other[TA["name"]];
          EcmaValue sample = OtherTA.Construct(1);
          DetachBuffer(sample);
          That(() => sample[0], Throws.TypeError);
        });
      });

      It("should return value from valid numeric index", () => {
        using (TempProperty(TypedArray.Prototype, 0, new EcmaPropertyDescriptor(ThrowTest262Exception, Undefined)))
        using (TempProperty(TypedArray.Prototype, 1, new EcmaPropertyDescriptor(ThrowTest262Exception, Undefined))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 1));
            That(sample[0], Is.EqualTo(42));
            That(sample[1], Is.EqualTo(1));
          });
        }
      });

      It("should treat Infinity as a canonical numeric string, test with access on detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(0);
          DetachBuffer(sample);
          That(() => sample[Infinity], Throws.TypeError);
        });
      });

      It("should return undefined if key is numeric index is not an integer", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "1.1", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(sample["1.1"], Is.Undefined);
          }
        });
      });

      It("should return undefined if key is numeric index is -0", () => {
        TestWithTypedArrayConstructors(TA => {
          using (TempProperty(TypedArray.Prototype, "-0", EcmaPropertyDescriptor.FromValue(CreateObject(new { get = ThrowTest262Exception })))) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(sample["-0"], Is.Undefined);
          }
        });
      });

      It("should return undefined if key is numeric index < 0 or index ≥ [[ArrayLength]]", () => {
        EcmaValue throwDesc = CreateObject(new { get = ThrowTest262Exception });
        using (TempProperty(TypedArray.Prototype, "-1", throwDesc))
        using (TempProperty(TypedArray.Prototype, "2", throwDesc))
        using (TempProperty(TypedArray.Prototype, "3", throwDesc)) {
          TestWithTypedArrayConstructors((TA) => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(sample["-1"], Is.Undefined);
            That(sample["2"], Is.Undefined);
            That(sample["3"], Is.Undefined);
          });
        }
      });

      It("should use OrginaryGet if numeric key is not a CanonicalNumericIndex", () => {
        EcmaValue keys = EcmaArray.Of(
          "1.0",
          "+1",
          "1000000000000000000000",
          "0.0000001");
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue key in keys.ForOf()) {
            EcmaValue sample = TA.Construct();
            That(sample[key], Is.Undefined);
            using (TempProperty(TypedArray.Prototype, EcmaPropertyKey.FromValue(key), "test262")) {
              That(sample[key], Is.EqualTo("test262"), "return value from inherited key [" + key.ToString() + "]");

              sample[key] = "bar";
              That(sample[key], Is.EqualTo("bar"), "return value from own key [" + key.ToString() + "]");

              Object.Invoke("defineProperty", sample, key, CreateObject(new { get = RuntimeFunction.Create(() => "baz") }));
              That(sample[key], Is.EqualTo("baz"), "return value from get accessor  [" + key.ToString() + "]");
            }
          }
        });
      });

      It("should use OrginaryGet if key is not a CanonicalNumericIndex", () => {
        using (TempProperty(TypedArray.Prototype, "baz", "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(sample["foo"], Is.Undefined, "return undefined for inexistent properties");

            sample["foo"] = "bar";
            That(sample["foo"], Is.EqualTo("bar"), "return value");

            Object.Invoke("defineProperty", sample, "bar", CreateObject(new { get = RuntimeFunction.Create(() => "baz") }));
            That(sample["bar"], Is.EqualTo("baz"), "return value from get accessor");
            That(sample["baz"], Is.EqualTo("test262"), "return value from inherited key");
          });
        }
      });

      It("should use OrginaryGet if key is a Symbol", () => {
        EcmaValue parentKey = new Symbol("2");
        EcmaValue s1 = new Symbol("1");
        using (TempProperty(TypedArray.Prototype, EcmaPropertyKey.FromValue(parentKey), "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42));
            That(sample[s1], Is.Undefined, "return undefined for inexistent properties");

            sample[s1] = "foo";
            That(sample[s1], Is.EqualTo("foo"), "return value");

            Object.Invoke("defineProperty", sample, s1, CreateObject(new { get = RuntimeFunction.Create(() => "bar") }));
            That(sample[s1], Is.EqualTo("bar"), "return value from get accessor");
            That(sample[parentKey], Is.EqualTo("test262"), "return value from inherited key");
          });
        }
      });

      It("should return abrupt from OrginaryGet when key is not a numeric index", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Object.Invoke("defineProperty", sample, "test262", CreateObject(new { get = ThrowTest262Exception }));
          That(() => sample["test262"], Throws.Test262);
        });
      });
    }

    [Test]
    public void GetOwnProperty() {
      It("does not throw on an instance with a detached buffer if key is not a number", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          DetachBuffer(sample);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "undef"), Is.Undefined);

          Object.Invoke("defineProperty", sample, "foo", CreateObject(new { value = "bar" }));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "foo")["value"], Is.EqualTo("bar"));
        });
      });

      It("does not throw on an instance with a detached buffer if key is a Symbol", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          DetachBuffer(sample);

          EcmaValue s = new Symbol("1");
          Object.Invoke("defineProperty", sample, s, CreateObject(new { value = "baz" }));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, s)["value"], Is.EqualTo("baz"));
        });
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          DetachBuffer(sample);
          That(() => Object.Invoke("getOwnPropertyDescriptor", sample, 0), Throws.TypeError);
        });
      });

      It("should throw a TypeError if this has a detached buffer (honoring the Realm of the current execution context)", () => {
        EcmaValue other = new RuntimeRealm().GetRuntimeObject(WellKnownObject.Global);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue OtherTA = other[TA["name"]];
          EcmaValue sample = OtherTA.Construct(1);
          DetachBuffer(sample);
          That(() => Object.Invoke("getOwnPropertyDescriptor", sample, 0), Throws.TypeError);
        });
      });

      It("should throw a TypeError for-in enumeration with detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(42);
          DetachBuffer(sample);
          That(() => {
            foreach (var i in sample) {
              ThrowTest262Exception();
            }
          }, Throws.TypeError);
        });
      });

      It("should return a descriptor object from an index property", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));

          EcmaValue desc0 = Object.Invoke("getOwnPropertyDescriptor", sample, 0);
          That(desc0["value"], Is.EqualTo(42));
          That(desc0["writable"], Is.True);
          That(desc0["enumerable"], Is.True);
          That(desc0["configurable"], Is.False);

          EcmaValue desc1 = Object.Invoke("getOwnPropertyDescriptor", sample, 1);
          That(desc1["value"], Is.EqualTo(43));
          That(desc1["writable"], Is.True);
          That(desc1["enumerable"], Is.True);
          That(desc1["configurable"], Is.False);
        });
      });

      It("should return undefined when P is -0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "-0"), Is.Undefined);
        });
      });

      It("should return undefined when P is not an integer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "1.1"), Is.Undefined);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "0.1"), Is.Undefined);
        });
      });

      It("should return undefined when P is not a valid index number", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "-1"), Is.Undefined);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "-42"), Is.Undefined);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "1"), Is.Undefined);
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "42"), Is.Undefined);
        });
      });

      It("should return an ordinary property value if numeric key is not a CanonicalNumericIndex", () => {
        EcmaValue keys = EcmaArray.Of(
          "1.0",
          "+1",
          "1000000000000000000000",
          "0.0000001");
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue key in keys.ForOf()) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(Object.Invoke("getOwnPropertyDescriptor", sample, key), Is.Undefined);

            Object.Invoke("defineProperty", sample, key, CreateObject(new { value = "bar" }));
            That(Object.Invoke("getOwnPropertyDescriptor", sample, key)["value"], Is.EqualTo("bar"));
          }
        });
      });

      It("should return an ordinary property value if key is not a CanonicalNumericIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "undef"), Is.Undefined);

          Object.Invoke("defineProperty", sample, "foo", CreateObject(new { value = "bar" }));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, "foo")["value"], Is.EqualTo("bar"));
        });
      });

      It("should return an ordinary property value if key is a Symbol", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          EcmaValue s = new Symbol("foo");
          Object.Invoke("defineProperty", sample, s, CreateObject(new { value = "baz" }));
          That(Object.Invoke("getOwnPropertyDescriptor", sample, s)["value"], Is.EqualTo("baz"));
        });
      });
    }

    [Test]
    public void HasProperty() {
      It("should return abrupt from OrdinaryHasProperty parent's [[HasProperty]]", () => {
        EcmaValue handler = CreateObject(new { has = ThrowTest262Exception });
        EcmaValue proxy = Proxy.Construct(TypedArray.Prototype, handler);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Object.Invoke("setPrototypeOf", sample, proxy);

          That(Reflect.Invoke("has", sample, 0), Is.True);
          That(Reflect.Invoke("has", sample, 1), Is.False);
          That(() => Reflect.Invoke("has", sample, "foo"), Throws.Test262);

          Object.Invoke("defineProperty", sample, "foo", CreateObject(new { value = 42 }));
          That(Reflect.Invoke("has", sample, "foo"), Is.True);
        });
      });

      It("does not throw on an instance with a detached buffer if key is not a CanonicalNumericIndexString", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Object.Invoke("defineProperty", sample, "bar", CreateObject(new { value = 42 }));

          DetachBuffer(sample);
          That(Reflect.Invoke("has", sample, "foo"), Is.False);
          That(Reflect.Invoke("has", sample, "bar"), Is.True);
        });
      });

      It("does not throw on an instance with a detached buffer if key is a Symbol", () => {
        EcmaValue s1 = new Symbol("foo");
        EcmaValue s2 = new Symbol("bar");
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          Object.Invoke("defineProperty", sample, s1, CreateObject(new { value = "baz" }));

          DetachBuffer(sample);
          That(Reflect.Invoke("has", sample, s1), Is.True);
          That(Reflect.Invoke("has", sample, s2), Is.False);
        });
      });

      It("should throw a TypeError if this has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          DetachBuffer(sample);

          That(() => Reflect.Invoke("has", sample, "0"), Throws.TypeError);
          That(() => Reflect.Invoke("has", sample, "-0"), Throws.TypeError);
          That(() => Reflect.Invoke("has", sample, "1.1"), Throws.TypeError);
        });
      });

      It("should throw a TypeError if this has a detached buffer (honoring the Realm of the current execution context)", () => {
        EcmaValue other = new RuntimeRealm().GetRuntimeObject(WellKnownObject.Global);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue OtherTA = other[TA["name"]];
          EcmaValue sample = OtherTA.Construct(EcmaArray.Of(42));
          DetachBuffer(sample);

          That(() => Reflect.Invoke("has", sample, "0"), Throws.TypeError);
          That(() => Reflect.Invoke("has", sample, "-0"), Throws.TypeError);
          That(() => Reflect.Invoke("has", sample, "1.1"), Throws.TypeError);
        });
      });

      It("should return true for indexed properties", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          That(Reflect.Invoke("has", sample, 0), Is.True);
          That(Reflect.Invoke("has", sample, 1), Is.True);
        });
      });

      It("should treat Infinity as a canonical numeric string, test with access on detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(0);
          DetachBuffer(sample);
          That(() => Reflect.Invoke("has", sample, Infinity), Throws.TypeError);
        });
      });

      It("should find inherited properties if property is not a CanonicalNumericIndexString", () => {
        using (TempProperty(TypedArray.Prototype, "foo", 42))
        using (TempProperty(TypedArray.Prototype, 42, true)) {
          TestWithTypedArrayConstructors(TA => {
            using (TempProperty(TA.Prototype, "bar", 42)) {
              EcmaValue sample = TA.Construct(0);
              That(Reflect.Invoke("has", sample, "subarray"), Is.True);
              That(Reflect.Invoke("has", sample, "foo"), Is.True);
              That(Reflect.Invoke("has", sample, "bar"), Is.True);
              That(Reflect.Invoke("has", sample, "baz"), Is.False);
              That(Reflect.Invoke("has", sample, 42), Is.False);
            }
          });
        }
      });

      It("should return false if P's value is >= this's [[ArrayLength]]", () => {
        using (TempProperty(TypedArray.Prototype, 1, "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(1);
            That(Reflect.Invoke("has", sample, "1"), Is.False);
          });
        }
      });

      It("should return false if P's value is < 0", () => {
        using (TempProperty(TypedArray.Prototype, -1, "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(1);
            That(Reflect.Invoke("has", sample, "-1"), Is.False);
          });
        }
      });

      It("should return false if P's value is -0", () => {
        using (TempProperty(TypedArray.Prototype, "-0", "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(1);
            That(Reflect.Invoke("has", sample, "-0"), Is.False);
          });
        }
      });

      It("should return false if P's value is not an integer", () => {
        using (TempProperty(TypedArray.Prototype, "1.1", "test262"))
        using (TempProperty(TypedArray.Prototype, "0.000001", "test262")) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(1);
            That(Reflect.Invoke("has", sample, "1.1"), Is.False);
            That(Reflect.Invoke("has", sample, "0.000001"), Is.False);
          });
        }
      });

      It("should return boolean from numeric keys that are not a CanonicalNumericIndexString", () => {
        EcmaValue keys = EcmaArray.Of(
          "1.0",
          "+1",
          "1000000000000000000000",
          "0.0000001");
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue key in keys.ForOf()) {
            EcmaValue sample = TA.Construct(1);
            That(Reflect.Invoke("has", sample, key), Is.False);

            using (TempProperty(TypedArray.Prototype, EcmaPropertyKey.FromValue(key), 42)) {
              That(Reflect.Invoke("has", sample, key), Is.True);
            }
            Object.Invoke("defineProperty", sample, key, CreateObject(new { value = 42 }));
            That(Reflect.Invoke("has", sample, key), Is.True);
          }
        });
      });

      It("should return boolean from properties that are not a CanonicalNumericIndexString", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          That(Reflect.Invoke("has", sample, "foo"), Is.False);

          Object.Invoke("defineProperty", sample, "foo", CreateObject(new { value = 42 }));
          That(Reflect.Invoke("has", sample, "foo"), Is.True);
        });
      });

      It("should return boolean from Symbol properties", () => {
        EcmaValue s = new Symbol("foo");
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          That(Reflect.Invoke("has", sample, s), Is.False);

          Object.Invoke("defineProperty", sample, s, CreateObject(new { value = 42 }));
          That(Reflect.Invoke("has", sample, s), Is.True);
        });
      });
    }

    [Test]
    public void OwnPropertyKeys() {
      It("should return keys", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample1 = TA.Construct(EcmaArray.Of(42, 42, 42));
          That(Reflect.Invoke("ownKeys", sample1), Is.EquivalentTo(new[] { "0", "1", "2" }));

          EcmaValue sample2 = TA.Construct(4);
          That(Reflect.Invoke("ownKeys", sample2), Is.EquivalentTo(new[] { "0", "1", "2", "3" }));

          EcmaValue sample3 = TA.Construct(4).Invoke("subarray", 2);
          That(Reflect.Invoke("ownKeys", sample3), Is.EquivalentTo(new[] { "0", "1" }));

          EcmaValue sample4 = TA.Construct();
          That(Reflect.Invoke("ownKeys", sample4), Is.EquivalentTo(new EcmaValue[0]));
        });
      });

      It("should return integer index and non numeric string keys", () => {
        EcmaValue s1 = new Symbol("1");
        EcmaValue s2 = new Symbol("2");

        using (TempProperty(TypedArray.Prototype, 3, 42))
        using (TempProperty(TypedArray.Prototype, "bar", 42)) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample1 = TA.Construct(EcmaArray.Of(42, 42, 42));
            sample1[s1] = 42;
            sample1[s2] = 42;
            sample1["test262"] = 42;
            sample1["ecma262"] = 42;
            That(Reflect.Invoke("ownKeys", sample1), Is.EquivalentTo(new[] { "0", "1", "2", "test262", "ecma262", s1, s2 }));

            EcmaValue sample2 = TA.Construct(4).Invoke("subarray", 2);
            sample2[s1] = 42;
            sample2[s2] = 42;
            sample2["test262"] = 42;
            sample2["ecma262"] = 42;
            That(Reflect.Invoke("ownKeys", sample2), Is.EquivalentTo(new[] { "0", "1", "test262", "ecma262", s1, s2 }));
          });
        }
      });

      It("should list non-enumerable own keys", () => {
        EcmaValue s = new Symbol("1");
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct();
          Object.Invoke("defineProperty", sample, s, CreateObject(new { value = 42, enumerable = false }));
          Object.Invoke("defineProperty", sample, "test262", CreateObject(new { value = 42, enumerable = false }));
          That(Reflect.Invoke("ownKeys", sample), Is.EquivalentTo(new[] { "test262", s }));
        });
      });
    }

    [Test]
    public void Set() {
      It("should produce consistent canonicalization of NaN values", () => {
        TestWithTypedArrayConstructors(FA => {
          EcmaValue samples = FA.Construct(1);
          foreach (EcmaValue value in ByteConversionValues.NaNs.ForOf()) {
            EcmaValue controls = FA.Construct(EcmaArray.Of(value, value, value));
            samples[0] = value;
            That(samples[0], Is.NaN);
            That(controls[0], Is.NaN);
          }
        }, new[] { Global.Float32Array, Global.Float64Array });
      });

      It("should convert to correct values", () => {
        TestTypedArrayConversion((TA, value, expected, initial) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(initial));
          sample[0] = value;
          That(sample[0], Is.EqualTo(expected), "{0}: {1} converts to {2}", TA["name"], value, expected);
        });
      });

      It("does not throw on an instance with a detached buffer if key is not a number", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          DetachBuffer(sample);
          That(Reflect.Invoke("set", sample, "foo", "test262"), Is.True);
          That(sample["foo"], Is.EqualTo("test262"));
        });
      });

      It("does not throw on an instance with a detached buffer if key is a Symbol", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
          DetachBuffer(sample);

          EcmaValue s = new Symbol("1");
          That(Reflect.Invoke("set", sample, s, "test262"), Is.True);
          That(sample[s], Is.EqualTo("test262"));
        });
      });

      It("should throw a TypeError if key has a numeric index and object has a detached buffer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          DetachBuffer(sample);

          That(() => sample[0] = 1, Throws.TypeError, "valid numeric index");
          That(() => sample["1.1"] = 1, Throws.TypeError, "detach buffer runs before checking for 1.1");
          That(() => sample["-0"] = 1, Throws.TypeError, "detach buffer runs before checking for -0");
          That(() => sample["-1"] = 1, Throws.TypeError, "detach buffer runs before checking for -1");
          That(() => sample["1"] = 1, Throws.TypeError, "detach buffer runs before checking for key == length");
          That(() => sample["2"] = 1, Throws.TypeError, "detach buffer runs before checking for key > length");
          That(() => sample["0"] = CreateObject(valueOf: ThrowTest262Exception), Throws.Test262, "ToNumber(value) is called before detached buffer check");
        });
      });

      It("should throw a TypeError if key has a numeric index and object has a detached (honoring the Realm of the current execution context)", () => {
        EcmaValue other = new RuntimeRealm().GetRuntimeObject(WellKnownObject.Global);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue OtherTA = other[TA["name"]];
          EcmaValue sample = OtherTA.Construct(1);
          DetachBuffer(sample);
          That(() => sample[0] = 0, Throws.TypeError);
        });
      });

      It("should return true after setting value", () => {
        EcmaValue throwDesc = CreateObject(new { set = ThrowTest262Exception });
        using (TempProperty(TypedArray.Prototype, "0", EcmaPropertyDescriptor.FromValue(throwDesc)))
        using (TempProperty(TypedArray.Prototype, "1", EcmaPropertyDescriptor.FromValue(throwDesc))) {
          TestWithTypedArrayConstructors(TA => {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42, 43));
            That(Reflect.Invoke("set", sample, "0", 1), Is.True);
            That(Reflect.Invoke("set", sample, "1", 42), Is.True);
            That(sample[0], Is.EqualTo(1));
            That(sample[1], Is.EqualTo(42));
          });
        }
      });

      It("should return false if index is -0", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Reflect.Invoke("set", sample, "-0", 1), Is.False);
          That(sample.Invoke("hasOwnProperty", "-0"), Is.False);
        });
      });

      It("should return false if index is not integer", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Reflect.Invoke("set", sample, "1.1", 1), Is.False);
          That(Reflect.Invoke("set", sample, "0.0001", 1), Is.False);
          That(sample.Invoke("hasOwnProperty", "1.1"), Is.False);
          That(sample.Invoke("hasOwnProperty", "0.0001"), Is.False);
        });
      });

      It("should return false if index is out of bounds", () => {
        TestWithTypedArrayConstructors((TA) => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Reflect.Invoke("set", sample, "-1", 1), Is.False);
          That(Reflect.Invoke("set", sample, "1", 1), Is.False);
          That(Reflect.Invoke("set", sample, "2", 1), Is.False);
          That(sample.Invoke("hasOwnProperty", "-1"), Is.False);
          That(sample.Invoke("hasOwnProperty", "1"), Is.False);
          That(sample.Invoke("hasOwnProperty", "2"), Is.False);
        });
      });

      It("should use OrginarySet if numeric key is not a CanonicalNumericIndex", () => {
        EcmaValue keys = EcmaArray.Of(
          "1.0",
          "+1",
          "1000000000000000000000",
          "0.0000001");
        TestWithTypedArrayConstructors(TA => {
          foreach (EcmaValue key in keys.ForOf()) {
            EcmaValue sample = TA.Construct(EcmaArray.Of(42));
            That(Reflect.Invoke("set", sample, key, "ecma262"), Is.True, "Return true setting a new property [" + key.ToString() + "]");
            That(sample[key], Is.EqualTo("ecma262"));

            That(Reflect.Invoke("set", sample, key, "es3000"), Is.True, "Return true setting a value to a writable property [" + key.ToString() + "]");
            That(sample[key], Is.EqualTo("es3000"));

            Object.Invoke("defineProperty", sample, key, CreateObject(new { writable = false, value = Undefined }));
            That(Reflect.Invoke("set", sample, key, 42), Is.False, "Return false setting a value to a non-writable property [" + key.ToString() + "]");
            That(sample[key], Is.Undefined);
          }
        });
      });

      It("should use OrginarySet if key is not a CanonicalNumericIndex", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Reflect.Invoke("set", sample, "test262", "ecma262"), Is.True, "Return true setting a new property");
          That(sample["test262"], Is.EqualTo("ecma262"));

          That(Reflect.Invoke("set", sample, "test262", "es3000"), Is.True, "Return true setting a value to a writable property");
          That(sample["test262"], Is.EqualTo("es3000"));

          Object.Invoke("defineProperty", sample, "foo", CreateObject(new { writable = false, value = Undefined }));
          That(Reflect.Invoke("set", sample, "foo", 42), Is.False, "Return false setting a value to a non-writable property");
          That(sample["foo"], Is.Undefined);
        });
      });

      It("should use OrginarySet if key is a Symbol", () => {
        EcmaValue s1 = new Symbol("1");
        EcmaValue s2 = new Symbol("2");
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(Reflect.Invoke("set", sample, s1, "ecma262"), Is.True, "Return true setting a new property");
          That(sample[s1], Is.EqualTo("ecma262"));

          That(Reflect.Invoke("set", sample, s1, "es3000"), Is.True, "Return true setting a value to a writable property");
          That(sample[s1], Is.EqualTo("es3000"));

          Object.Invoke("defineProperty", sample, s2, CreateObject(new { writable = false, value = Undefined }));
          That(Reflect.Invoke("set", sample, s2, 42), Is.False, "Return false setting a value to a non-writable property");
          That(sample[s2], Is.Undefined);
        });
      });

      It("should return abrupt from OrginarySet when key is not a numeric index", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(1);
          Object.Invoke("defineProperty", sample, "test262", CreateObject(new { set = ThrowTest262Exception }));
          That(() => sample["test262"] = 1, Throws.Test262);
          That(sample["test262"], Is.Undefined);
        });
      });

      It("should throw a TypeError and not modify the typed array when buffer is detached during value conversion", () => {
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(17));
          That(() => Reflect.Invoke("set", sample, 0, CreateObject(valueOf: () => {
            DetachBuffer(sample);
            return 42;
          })), Throws.TypeError);
          That(() => sample[0], Throws.TypeError);
        });
      });

      It("should return abrupt from the evaluation of ToNumber(desc.value)", () => {
        EcmaValue obj = CreateObject(valueOf: ThrowTest262Exception);
        TestWithTypedArrayConstructors(TA => {
          EcmaValue sample = TA.Construct(EcmaArray.Of(42));
          That(() => sample["0"] = obj, Throws.Test262);
          That(() => sample["1.1"] = obj, Throws.Test262);
          That(() => sample["-0"] = obj, Throws.Test262);
          That(() => sample["-1"] = obj, Throws.Test262);
          That(() => sample["1"] = obj, Throws.Test262);
          That(() => sample["2"] = obj, Throws.Test262);
        });
      });
    }
  }
}
