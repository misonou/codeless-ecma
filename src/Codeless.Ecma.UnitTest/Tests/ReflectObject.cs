using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ReflectObject : TestBase {
    [Test]
    public void Properties() {
      That(Object.Invoke("getPrototypeOf", Reflect), Is.EqualTo(Object.Prototype));
      That(GlobalThis, Has.OwnProperty("Reflect", Reflect, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(() => Reflect.Call(), Throws.TypeError);
      That(() => Reflect.Construct(), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Apply(RuntimeFunction apply) {
      IsUnconstructableFunctionWLength(apply, "apply", 3);
      That(Reflect, Has.OwnProperty("apply", apply, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt if argumentsList is not an ArrayLike object", () => {
        EcmaValue fn = Function.Construct();
        EcmaValue o = CreateObject(("length", get: ThrowTest262Exception, set: null));
        Case((_, fn, 1, o), Throws.Test262);
        Case((_, fn, 1, 1), Throws.TypeError);
      });

      It("should call target with thisArgument and argumentsList", () => {
        EcmaValue o = Object.Construct();
        EcmaValue thisArg = default;
        EcmaValue args = default;
        EcmaValue fn = Intercept(() => Return(thisArg = This, args = Arguments));

        Logs.Clear();
        apply.Call(_, fn, o, EcmaArray.Of("arg1", 2, Undefined, Null));
        That(Logs.Count, Is.EqualTo(1));
        That(thisArg, Is.EqualTo(o));
        That(args, Is.EquivalentTo(new[] { "arg1", 2, Undefined, Null }));
      });

      It("should return target result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue fn = RuntimeFunction.Create(() => o);
        Case((_, fn, 1, EcmaArray.Of()), o);
      });

      It("should throw a TypeError if `target` is not callable", () => {
        Case((_, 1, 1, EcmaArray.Of()), Throws.TypeError);
        Case((_, Null, 1, EcmaArray.Of()), Throws.TypeError);
        Case((_, Object.Construct(), 1, EcmaArray.Of()), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Construct(RuntimeFunction construct) {
      IsUnconstructableFunctionWLength(construct, "construct", 2);
      That(Reflect, Has.OwnProperty("construct", construct, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt if argumentsList is not an ArrayLike object", () => {
        EcmaValue fn = Function.Construct();
        EcmaValue o = CreateObject(("length", get: ThrowTest262Exception, set: null));
        Case((_, fn, o), Throws.Test262);
        Case((_, fn, 1), Throws.TypeError);
      });

      It("should throws a TypeError if `newTarget` is not a constructor", () => {
        Case((_, RuntimeFunction.Create(_ => _), EcmaArray.Of(), 1), Throws.TypeError);
        Case((_, RuntimeFunction.Create(_ => _), EcmaArray.Of(), Null), Throws.TypeError);
        Case((_, RuntimeFunction.Create(_ => _), EcmaArray.Of(), Object.Construct()), Throws.TypeError);
        Case((_, RuntimeFunction.Create(_ => _), EcmaArray.Of(), Date["now"]), Throws.TypeError);
      });

      It("should return target result using newTarget argument", () => {
        EcmaValue o = Object.Construct();
        EcmaValue internPrototype = default;
        EcmaValue fn = RuntimeFunction.Create(() => {
          This.ToObject()["o"] = o;
          internPrototype = Object.Invoke("getPrototypeOf", This);
        });
        EcmaValue result = Reflect.Invoke("construct", fn, EcmaArray.Of(), Array);
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Array.Prototype));
        That(internPrototype, Is.EqualTo(Array.Prototype), "prototype of this from within the constructor function is Array.prototype");
        That(result["o"], Is.EqualTo(o));
      });

      It("should return target result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue fn = RuntimeFunction.Create(() => {
          This.ToObject()["o"] = o;
        });
        EcmaValue result = Reflect.Invoke("construct", fn, EcmaArray.Of());
        That(result["o"], Is.EqualTo(o));
        That(result.InstanceOf(fn));
      });

      It("should throw a TypeError if `target` is not a constructor", () => {
        Case((_, 1, EcmaArray.Of()), Throws.TypeError);
        Case((_, Null, EcmaArray.Of()), Throws.TypeError);
        Case((_, Object.Construct(), EcmaArray.Of()), Throws.TypeError);
        Case((_, Date["now"], EcmaArray.Of()), Throws.TypeError);
      });

      It("should construct with given argumentsList", () => {
        EcmaValue o = Object.Construct();
        EcmaValue fn = RuntimeFunction.Create(() => {
          This.ToObject()["args"] = Arguments;
        });
        EcmaValue result = Reflect.Invoke("construct", fn, EcmaArray.Of(42, "Mike", "Leo"));
        That(result["args"], Is.EquivalentTo(new EcmaValue[] { 42, "Mike", "Leo" }));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void DefineProperty(RuntimeFunction defineProperty) {
      IsUnconstructableFunctionWLength(defineProperty, "defineProperty", 3);
      That(Reflect, Has.OwnProperty("defineProperty", defineProperty, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should define properties from the attributes object", () => {
        EcmaValue o = Object.Construct();
        Reflect.Invoke("defineProperty", o, "p1", CreateObject(new {
          value = 42,
          writable = true,
          enumerable = true
        }));
        That(o["p1"], Is.EqualTo(42));
        That(o, Has.OwnProperty("p1", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable));

        EcmaValue f1 = RuntimeFunction.Create(_ => _);
        EcmaValue f2 = RuntimeFunction.Create(_ => _);
        Reflect.Invoke("defineProperty", o, "p2", CreateObject(new {
          get = f1,
          set = f2
        }));
        EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", o, "p2");
        That(desc["get"], Is.EqualTo(f1));
        That(desc["set"], Is.EqualTo(f2));
      });

      It("should define symbol properties", () => {
        EcmaValue o = Object.Construct();
        EcmaValue s1 = new Symbol();
        Reflect.Invoke("defineProperty", o, s1, CreateObject(new {
          value = 42,
          writable = true,
          enumerable = true
        }));
        That(o[s1], Is.EqualTo(42));
        That(o, Has.OwnProperty(EcmaPropertyKey.FromValue(s1), EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable));

        EcmaValue s2 = new Symbol();
        EcmaValue f1 = RuntimeFunction.Create(_ => _);
        EcmaValue f2 = RuntimeFunction.Create(_ => _);
        Reflect.Invoke("defineProperty", o, s2, CreateObject(new {
          get = f1,
          set = f2
        }));
        EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", o, s2);
        That(desc["get"], Is.EqualTo(f1));
        That(desc["set"], Is.EqualTo(f2));
      });

      It("should return abrupt from ToPropertyDescriptor(attributes)", () => {
        EcmaValue attributes = CreateObject(("enumerable", get: ThrowTest262Exception, set: null));
        Case((_, Object.Construct(), "a", attributes), Throws.Test262);
      });

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result on defining a property", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("defineProperty", ThrowTest262Exception)));
        Case((_, p, "p1", Object.Construct()), Throws.Test262);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p", Object.Construct()), Throws.TypeError);
        Case((_, Null, "p", Object.Construct()), Throws.TypeError);
        Case((_, Undefined, "p", Object.Construct()), Throws.TypeError);
        Case((_, "", "p", Object.Construct()), Throws.TypeError);
        Case((_, new Symbol(), "p", Object.Construct()), Throws.TypeError);
      });

      It("should return boolean result of the property definition", () => {
        EcmaValue o = Object.Construct();
        o["p1"] = "foo";
        Case((_, o, "p1", Object.Construct()), true);
        That(o.Invoke("hasOwnProperty", "p1"), Is.EqualTo(true));

        Case((_, o, "p2", CreateObject(new { value = 42 })), true);
        That(o.Invoke("hasOwnProperty", "p2"), Is.EqualTo(true));

        Object.Invoke("freeze", o);
        Case((_, o, "p2", CreateObject(new { value = 43 })), false);
        That(o["p2"], Is.EqualTo(42));

        Case((_, o, "p3", Object.Construct()), false);
        That(o.Invoke("hasOwnProperty", "p3"), Is.EqualTo(false));

        Case((_, o, "p4", CreateObject(new { value = 1 })), false);
        That(o.Invoke("hasOwnProperty", "p4"), Is.EqualTo(false));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void DeleteProperty(RuntimeFunction deleteProperty) {
      IsUnconstructableFunctionWLength(deleteProperty, "deleteProperty", 2);
      That(Reflect, Has.OwnProperty("deleteProperty", deleteProperty, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should delete property", () => {
        EcmaValue o = CreateObject(("prop", 42));
        Reflect.Invoke("deleteProperty", o, "prop");
        That(o.Invoke("hasOwnProperty", "prop"), Is.EqualTo(false));
      });

      It("should delete a symbol property", () => {
        EcmaValue s = new Symbol();
        EcmaValue o = CreateObject((s, 42));
        Reflect.Invoke("deleteProperty", o, s);
        That(o.Invoke("hasOwnProperty", s), Is.EqualTo(false));
        That(o[s], Is.Undefined);
      });

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result from deleting a property", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("deleteProperty", ThrowTest262Exception)));
        Case((_, p, "p1"), Throws.Test262);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p"), Throws.TypeError);
        Case((_, Null, "p"), Throws.TypeError);
        Case((_, Undefined, "p"), Throws.TypeError);
        Case((_, "", "p"), Throws.TypeError);
        Case((_, new Symbol(), "p"), Throws.TypeError);
      });

      It("should return boolean resul", () => {
        EcmaValue o = Object.Construct();
        o["p1"] = "foo";
        Case((_, o, "p1"), true);
        That(o.Invoke("hasOwnProperty", "p1"), Is.EqualTo(false));

        o["p2"] = "foo";
        Object.Invoke("freeze", o);
        Case((_, o, "p2"), false);
        That(o.Invoke("hasOwnProperty", "p2"), Is.EqualTo(true));
      });
    }

    [Test]
    public void Enumerate() {
      That(Reflect.Invoke("hasOwnProperty", "enumerate"), Is.EqualTo(false));
      That(Reflect["enumerate"], Is.Undefined);
    }

    [Test, RuntimeFunctionInjection]
    public void Get(RuntimeFunction get) {
      IsUnconstructableFunctionWLength(get, "get", 2);
      That(Reflect, Has.OwnProperty("get", get, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result from get a property value", () => {
        EcmaValue o1 = Object.Construct();
        Object.Invoke("defineProperty", o1, "p1", CreateObject(new { get = ThrowTest262Exception }));
        Case((_, o1, "p1"), Throws.Test262);
        Case((_, Object.Invoke("create", o1), "p1"), Throws.Test262);
      });

      It("should return value from a receiver", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue receiver = CreateObject(("y", 42));
        Object.Invoke("defineProperty", o1, "x", CreateObject(new { get = RuntimeFunction.Create(() => This["y"]) }));
        Case((_, o1, "x", receiver), 42);
        Case((_, Object.Invoke("create", o1), "x", receiver), 42);
      });

      It("should return value", () => {
        EcmaValue o = Object.Construct();
        o["p1"] = "value 1";
        Case((_, o, "p1"), "value 1", "Return value from data descriptor");

        Object.Invoke("defineProperty", o, "p2", CreateObject(new { get = Undefined }));
        Case((_, o, "p2"), Undefined, "Return undefined if getter is undefined");

        Object.Invoke("defineProperty", o, "p3", CreateObject(new { get = RuntimeFunction.Create(() => "foo") }));
        Case((_, o, "p3"), "foo", "Return Call(getter, Receiver)");

        EcmaValue o2 = Object.Invoke("create", CreateObject(("p", 42)));
        Case((_, o2, "p"), 42, "Return value from prototype without own property.");
        Case((_, o2, "u"), Undefined, "Return undefined without property on the object and its prototype");
      });

      It("should return value where property key is a symbol", () => {
        EcmaValue o = Object.Construct();
        EcmaValue s = new Symbol();
        o[s] = 42;
        Case((_, o, s), 42);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p"), Throws.TypeError);
        Case((_, Null, "p"), Throws.TypeError);
        Case((_, Undefined, "p"), Throws.TypeError);
        Case((_, "", "p"), Throws.TypeError);
        Case((_, new Symbol(), "p"), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetOwnPropertyDescriptor(RuntimeFunction getOwnPropertyDescriptor) {
      IsUnconstructableFunctionWLength(getOwnPropertyDescriptor, "getOwnPropertyDescriptor", 2);
      That(Reflect, Has.OwnProperty("getOwnPropertyDescriptor", getOwnPropertyDescriptor, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result from getting the property descriptor", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("getOwnPropertyDescriptor", ThrowTest262Exception)));
        Case((_, p, "p1"), Throws.Test262);
      });

      It("should return a property descriptor object as an accessor descriptor", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue fn = RuntimeFunction.Create(_ => _);
        Object.Invoke("defineProperty", o1, "p", CreateObject(new { get = fn, configurable = true }));

        EcmaValue result = Reflect.Invoke("getOwnPropertyDescriptor", o1, "p");
        That(Object.Invoke("keys", result), Is.EquivalentTo(new[] { "get", "set", "enumerable", "configurable" }));
        That(result["enumerable"], Is.EqualTo(false));
        That(result["configurable"], Is.EqualTo(true));
        That(result["get"], Is.EqualTo(fn));
        That(result["set"], Is.EqualTo(Undefined));
      });

      It("should return a property descriptor object as a data descriptor", () => {
        EcmaValue o1 = CreateObject(("p", "foo"));
        EcmaValue result = Reflect.Invoke("getOwnPropertyDescriptor", o1, "p");
        That(Object.Invoke("keys", result), Is.EquivalentTo(new[] { "value", "writable", "enumerable", "configurable" }));
        That(result["value"], Is.EqualTo("foo"));
        That(result["enumerable"], Is.EqualTo(true));
        That(result["configurable"], Is.EqualTo(true));
        That(result["writable"], Is.EqualTo(true));
      });

      It("should return a property descriptor object using a symbol value on property key", () => {
        EcmaValue s = new Symbol();
        EcmaValue o = CreateObject((s, 42));
        EcmaValue result = Reflect.Invoke("getOwnPropertyDescriptor", o, s);
        That(Object.Invoke("keys", result), Is.EquivalentTo(new[] { "value", "writable", "enumerable", "configurable" }));
        That(result["value"], Is.EqualTo(42));
        That(result["enumerable"], Is.EqualTo(true));
        That(result["configurable"], Is.EqualTo(true));
        That(result["writable"], Is.EqualTo(true));
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p"), Throws.TypeError);
        Case((_, Null, "p"), Throws.TypeError);
        Case((_, Undefined, "p"), Throws.TypeError);
        Case((_, "", "p"), Throws.TypeError);
        Case((_, new Symbol(), "p"), Throws.TypeError);
      });

      It("should return undefined for an non existing own property", () => {
        EcmaValue o = Object.Invoke("create", CreateObject(("p", 1)));
        Case((_, o, "p"), Undefined);
        Case((_, Object.Construct(), Undefined), Undefined);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetPrototypeOf(RuntimeFunction getPrototypeOf) {
      IsUnconstructableFunctionWLength(getPrototypeOf, "getPrototypeOf", 1);
      That(Reflect, Has.OwnProperty("getPrototypeOf", getPrototypeOf, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return null prototype", () => {
        Case((_, Object.Invoke("create", Null)), Null);
      });

      It("should return abrupt result from getting the prototype", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("getPrototypeOf", ThrowTest262Exception)));
        Case((_, p), Throws.Test262);
      });

      It("should return the internal [[Prototype]] object", () => {
        Case((_, Object.Construct()), Object.Prototype);
      });

      It("should skip own properties to return the internal [[Prototype]] object", () => {
        EcmaValue valid = Object.Construct();
        EcmaValue o = Object.Invoke("create", valid, CreateObject(("prototype", CreateObject(new { value = "invalid", enumerable = true }))));
        Case((_, o), valid);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p"), Throws.TypeError);
        Case((_, Null, "p"), Throws.TypeError);
        Case((_, Undefined, "p"), Throws.TypeError);
        Case((_, "", "p"), Throws.TypeError);
        Case((_, new Symbol(), "p"), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection(WellKnownObject.Reflect, "has")]
    public void Has_(RuntimeFunction has) {
      IsUnconstructableFunctionWLength(has, "has", 2);
      That(Reflect, Has.OwnProperty("has", has, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("has", ThrowTest262Exception)));
        Case((_, p, "p1"), Throws.Test262);
      });

      It("should return boolean value", () => {
        EcmaValue o1 = CreateObject(("p", 42));
        Case((_, o1, "p"), true);
        Case((_, o1, "z"), false);
        Case((_, Object.Invoke("create", o1), "p"), true);
      });

      It("should return boolean value from a projectKey as a Symbol", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue s1 = new Symbol();
        EcmaValue s2 = new Symbol();
        o1[s1] = 42;
        Case((_, o1, s1), true);
        Case((_, o1, s2), false);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1, "p"), Throws.TypeError);
        Case((_, Null, "p"), Throws.TypeError);
        Case((_, Undefined, "p"), Throws.TypeError);
        Case((_, "", "p"), Throws.TypeError);
        Case((_, new Symbol(), "p"), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsExtensible(RuntimeFunction isExtensible) {
      IsUnconstructableFunctionWLength(isExtensible, "isExtensible", 1);
      That(Reflect, Has.OwnProperty("isExtensible", isExtensible, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("isExtensible", ThrowTest262Exception)));
        Case((_, p), Throws.Test262);
      });

      It("should return the boolean result", () => {
        EcmaValue o = Object.Construct();
        Case((_, o), true);
        Object.Invoke("preventExtensions", o);
        Case((_, o), false);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, Undefined), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, new Symbol()), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void OwnKeys(RuntimeFunction ownKeys) {
      IsUnconstructableFunctionWLength(ownKeys, "ownKeys", 1);
      That(Reflect, Has.OwnProperty("ownKeys", ownKeys, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("ownKeys", ThrowTest262Exception)));
        Case((_, p), Throws.Test262);
      });

      It("should return target's own property keys only, ignore prototype keys", () => {
        EcmaValue proto = CreateObject(("foo", 1));
        EcmaValue o = Object.Invoke("create", proto);
        o["p1"] = 42;
        o["p2"] = 43;
        o["p3"] = 44;
        Case((_, o), new[] { "p1", "p2", "p3" });
      });

      It("should return empty array when target has no own properties", () => {
        Case((_, Object.Construct()), EcmaValue.EmptyArray);
        EcmaValue o = CreateObject(("d", 42));
        o.ToObject().Delete("d");
        Case((_, o), EcmaValue.EmptyArray);
      });

      It("should return target's own non enumerable property keys", () => {
        Case((_, EcmaArray.Of()), new[] { "length" });

        EcmaValue arr = EcmaArray.Of();
        arr[2] = 2;
        Case((_, arr), new[] { "2", "length" });

        EcmaValue o = Object.Construct();
        Object.Invoke("defineProperty", o, "p1", CreateObject(new { value = 42, enumerable = false }));
        Object.Invoke("defineProperty", o, "p2", CreateObject(new { get = RuntimeFunction.Create(_ => _), enumerable = false }));
        Case((_, o), new[] { "p1", "p2" });
      });

      It("should return keys in their corresponding order", () => {
        EcmaValue o = Object.Construct();
        o["p2"] = 43;
        o["p1"] = 42;

        EcmaValue s1 = new Symbol("1");
        EcmaValue s2 = new Symbol("a");
        o[s1] = 44;
        o[s2] = 45;

        o[2] = 46;
        o[0] = 47;
        o[1] = 48;
        Case((_, o), new[] { "0", "1", "2", "p2", "p1", s1, s2 });

        EcmaValue o1 = CreateObject(
         (12345678900, true),
         ("b", true),
         (1, true),
         ("a", true),
         (Number["MAX_SAFE_INTEGER"], true),
         (Global.Symbol.Invoke("for", "z"), true),
         (12345678901, true),
         (4294967294, true),
         (4294967295, true)
        );
        Case((_, o1), new[] { "1", "4294967294", "12345678900", "b", "a", String.Call(_, Number["MAX_SAFE_INTEGER"]), "12345678901", "4294967295", Global.Symbol.Invoke("for", "z") });
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, Undefined), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, new Symbol()), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void PreventExtensions(RuntimeFunction preventExtensions) {
      IsUnconstructableFunctionWLength(preventExtensions, "preventExtensions", 1);
      That(Reflect, Has.OwnProperty("preventExtensions", preventExtensions, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("preventExtensions", ThrowTest262Exception)));
        Case((_, p), Throws.Test262);
      });

      It("should return boolean from Proxy object", () => {
        EcmaValue p1 = Proxy.Construct(Object.Construct(), CreateObject(("preventExtensions", RuntimeFunction.Create(() => false))));
        Case((_, p1), false);

        EcmaValue p2 = Proxy.Construct(Object.Construct(), CreateObject(("preventExtensions", RuntimeFunction.Create((t) => Return(Object.Invoke("preventExtensions", t), true)))));
        Case((_, p2), true);
      });

      It("should Always returns true when target is an ordinary object", () => {
        EcmaValue o = Object.Construct();
        Case((_, o), true);
        Case((_, o), true);
      });

      It("should prevent extentions on target", () => {
        EcmaValue o = Object.Construct();
        Reflect.Invoke("preventExtensions", o);
        That(Object.Invoke("isExtensible", o), Is.EqualTo(false));
        That(() => Object.Invoke("defineProperty", o, "y", Object.Construct()), Throws.TypeError);
        That(() => Object.Invoke("setPrototypeOf", o, Array.Prototype), Throws.TypeError);

        Reflect.Invoke("preventExtensions", o);
        That(Object.Invoke("isExtensible", o), Is.EqualTo(false), "object is still not extensible on exhausted calls");
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, Undefined), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, new Symbol()), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Set(RuntimeFunction set) {
      IsUnconstructableFunctionWLength(set, "set", 3);
      That(Reflect, Has.OwnProperty("set", set, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should call accessor's set from target's prototype", () => {
        EcmaValue proto = Object.Construct();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        Object.Invoke("defineProperty", proto, "p", CreateObject(new { set = Intercept(() => Return(thisValue = This, args = Arguments, Undefined)) }));
        Logs.Clear();

        EcmaValue target = Object.Invoke("create", proto);
        Case((_, target, "p", 42), true);
        That(args, Is.EquivalentTo(new[] { 42 }));
        That(thisValue, Is.EqualTo(target));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should create a property data descriptor", () => {
        EcmaValue o1 = Object.Construct();
        Case((_, o1, "p", 42), true, "returns true on a successful setting");

        EcmaValue desc = Object.Invoke("getOwnPropertyDescriptor", o1, "p");
        That(desc["value"], Is.EqualTo(42), "sets a data descriptor to set a new property");
        That(o1, Has.OwnProperty("p", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Enumerable));

        EcmaValue o2 = Object.Construct();
        EcmaValue receiver = Object.Construct();
        Case((_, o2, "p", 43, receiver), true, "returns true on a successful setting with a receiver");

        desc = Object.Invoke("getOwnPropertyDescriptor", o2, "p");
        That(desc, Is.Undefined, "does not set a data descriptor on target if receiver is given");

        desc = Object.Invoke("getOwnPropertyDescriptor", receiver, "p");
        That(desc["value"], Is.EqualTo(43), "sets a data descriptor on the receiver object to set a new property");
        That(receiver, Has.OwnProperty("p", EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Enumerable));
      });

      It("should return false if target property turns to a data descriptor and receiver property is an accessor descriptor", () => {
        EcmaValue receiver = Object.Construct();
        EcmaValue fn = RuntimeFunction.Create(_ => _);
        Object.Invoke("defineProperty", receiver, "p", CreateObject(new { set = fn }));

        EcmaValue o1 = Object.Construct();
        Case((_, o1, "p", 42, receiver), false, "target has no own `p` and receiver.p has an accessor descriptor");
        That(Object.Invoke("getOwnPropertyDescriptor", receiver, "p")["set"], Is.EqualTo(fn), "receiver.p is not changed");
        That(o1.Invoke("hasOwnProperty", "p"), Is.EqualTo(false), "target.p is not set");

        EcmaValue o2 = CreateObject(new { p = 43 });
        Case((_, o2, "p", 42, receiver), false, "target.p has a data descriptor and receiver.p has an accessor descriptor");
        That(Object.Invoke("getOwnPropertyDescriptor", receiver, "p")["set"], Is.EqualTo(fn), "receiver.p is not changed");
        That(o2["p"], Is.EqualTo(43), "target.p is not changed");
      });

      It("should return false if receiver is not an object", () => {
        EcmaValue o1 = CreateObject(("p", 42));
        EcmaValue receiver = "receiver is a string";
        Case((_, o1, "p", 43, receiver), false);
        That(o1["p"], Is.EqualTo(42));
        That(receiver.Invoke("hasOwnProperty", "p"), Is.EqualTo(false));
      });

      It("should return false if target is not writable", () => {
        EcmaValue o1 = Object.Construct();
        Object.Invoke("defineProperty", o1, "p", CreateObject(new { value = 42, writable = false }));
        Case((_, o1, "p", 43), false);
        That(o1["p"], Is.EqualTo(42), "does not set a new value");
      });

      It("should return false if receiver is not writable", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue receiver = Object.Construct();
        Object.Invoke("defineProperty", receiver, "p", CreateObject(new { value = 42, writable = false }));
        Case((_, o1, "p", 43, receiver), false);
        That(receiver["p"], Is.EqualTo(42), "does not set a new value on receiver");
        That(o1.Invoke("hasOwnProperty", "p"), Is.EqualTo(false), "does not set a new value on target");
      });

      It("should return abrupt from ToPropertyKey(propertyKey)", () => {
        Case((_, Object.Construct(), CreateObject(toString: ThrowTest262Exception)), Throws.Test262);
      });

      It("should return abrupt result from get a property value", () => {
        EcmaValue o1 = Object.Construct();
        Object.Invoke("defineProperty", o1, "p1", CreateObject(new { set = ThrowTest262Exception }));
        Case((_, o1, "p1", 42), Throws.Test262);
        Case((_, Object.Invoke("create", o1), "p1", 42), Throws.Test262);
      });

      It("should set value on an accessor descriptor property with receiver as `this`", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue receiver = Object.Construct();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        Object.Invoke("defineProperty", o1, "p", CreateObject(new { set = Intercept(() => Return(thisValue = This, args = Arguments, false)) }));

        Logs.Clear();
        Case((_, o1, "p", 42, receiver), true, "returns true if target.p has an accessor descriptor");
        That(args, Is.EquivalentTo(new[] { 42 }));
        That(thisValue, Is.EqualTo(receiver));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should set value on an accessor descriptor property", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue thisValue = default;
        EcmaValue args = default;
        Object.Invoke("defineProperty", o1, "p", CreateObject(new { set = Intercept(() => Return(thisValue = This, args = Arguments, false)) }));

        Logs.Clear();
        Case((_, o1, "p", 42), true, "returns true if target.p has an accessor descriptor");
        That(args, Is.EquivalentTo(new[] { 42 }));
        That(thisValue, Is.EqualTo(o1));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should set the new value", () => {
        EcmaValue o1 = CreateObject(new { p = 43 });
        Case((_, o1, "p", 42), true);
        That(o1["p"], Is.EqualTo(42));

        EcmaValue o2 = CreateObject(new { p = 43 });
        EcmaValue receiver = CreateObject(new { p = 44 });
        Case((_, o2, "p", 42, receiver), true);
        That(o2["p"], Is.EqualTo(43), "with a receiver, does not set a value on target");
        That(receiver["p"], Is.EqualTo(42), "sets the new value on the receiver");
      });

      It("should set the new value for symbol property", () => {
        EcmaValue o1 = Object.Construct();
        EcmaValue s = new Symbol();
        Case((_, o1, s, 42), true);
        That(o1[s], Is.EqualTo(42), "sets the new value");

        EcmaValue o2 = Object.Construct();
        o2[s] = 43;
        EcmaValue receiver = Object.Construct();
        receiver[s] = 44;
        Case((_, o2, s, 42, receiver), true);
        That(o2[s], Is.EqualTo(43), "with a receiver, does not set a value on target");
        That(receiver[s], Is.EqualTo(42), "sets the new value on the receiver");
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, Undefined), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, new Symbol()), Throws.TypeError);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetPrototypeOf(RuntimeFunction setPrototypeOf) {
      IsUnconstructableFunctionWLength(setPrototypeOf, "setPrototypeOf", 2);
      That(Reflect, Has.OwnProperty("setPrototypeOf", setPrototypeOf, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return abrupt result", () => {
        EcmaValue o = Object.Construct();
        EcmaValue p = Proxy.Construct(o, CreateObject(("setPrototypeOf", ThrowTest262Exception)));
        Case((_, p, Object.Construct()), Throws.Test262);
      });

      It("should return true if the new prototype is set", () => {
        EcmaValue o1 = Object.Construct();
        Case((_, o1, Null), true);
        That(Object.Invoke("getPrototypeOf", o1), Is.EqualTo(Null));

        EcmaValue o2 = Object.Invoke("create", Null);
        Case((_, o2, Object.Prototype), true);
        That(Object.Invoke("getPrototypeOf", o2), Is.EqualTo(Object.Prototype));

        EcmaValue o3 = Object.Construct();
        EcmaValue proto = Object.Construct();
        Case((_, o3, proto), true);
        That(Object.Invoke("getPrototypeOf", o3), Is.EqualTo(proto));
      });

      It("should return true if proto has the same value as current target's prototype", () => {
        EcmaValue o1 = Object.Construct();
        Object.Invoke("preventExtensions", o1);
        Case((_, o1, Object.Prototype), true);

        EcmaValue o2 = Object.Invoke("create", Null);
        Object.Invoke("preventExtensions", o2);
        Case((_, o2, Null), true);

        EcmaValue proto = Object.Construct();
        EcmaValue o3 = Object.Invoke("create", proto);
        Object.Invoke("preventExtensions", o3);
        Case((_, o3, proto), true);
      });

      It("should return false if target is not extensible, without changing the prototype", () => {
        EcmaValue o1 = Object.Construct();
        Object.Invoke("preventExtensions", o1);
        Case((_, o1, Object.Construct()), false);
        That(Object.Invoke("getPrototypeOf", o1), Is.EqualTo(Object.Prototype));

        EcmaValue o2 = Object.Construct();
        Object.Invoke("preventExtensions", o2);
        Case((_, o2, Null), false);
        That(Object.Invoke("getPrototypeOf", o2), Is.EqualTo(Object.Prototype));

        EcmaValue o3 = Object.Invoke("create", Null);
        Object.Invoke("preventExtensions", o3);
        Case((_, o3, Object.Construct()), false);
        That(Object.Invoke("getPrototypeOf", o3), Is.EqualTo(Null));
      });

      It("should return false if target is found as a prototype of proto, without setting", () => {
        EcmaValue target = Object.Construct();
        EcmaValue proto = Object.Invoke("create", target);
        Case((_, target, proto), false);
        That(Object.Invoke("getPrototypeOf", target), Is.EqualTo(Object.Prototype));
      });

      It("should throw a TypeError if proto is not Object or proto is not null", () => {
        Case((_, Object.Construct(), Undefined), Throws.TypeError);
        Case((_, Object.Construct(), 1), Throws.TypeError);
        Case((_, Object.Construct(), "string"), Throws.TypeError);
        Case((_, Object.Construct(), true), Throws.TypeError);
        Case((_, Object.Construct(), new Symbol()), Throws.TypeError);
      });

      It("should throw a TypeError if `target` is not an Object", () => {
        Case((_, 1), Throws.TypeError);
        Case((_, Null), Throws.TypeError);
        Case((_, Undefined), Throws.TypeError);
        Case((_, ""), Throws.TypeError);
        Case((_, new Symbol()), Throws.TypeError);
      });
    }
  }
}
