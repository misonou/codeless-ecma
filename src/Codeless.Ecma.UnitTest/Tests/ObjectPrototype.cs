using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ObjectPrototype : TestBase {
    [Test]
    public void Properties() {
      That(Object, Has.OwnProperty("prototype", Object.Prototype, EcmaPropertyAttributes.None));

      That(Object.Prototype, Has.OwnProperty("constructor", Object, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Object.Invoke("getPrototypeOf", Object.Prototype), Is.EqualTo(Null));
      That(Object.Prototype.Get("toString").Call(Object.Prototype), Is.EqualTo("[object Object]"), "Boolean prototype object: its [[Class]] must be 'Object'");

      That(() => Object.Prototype.Call(), Throws.TypeError);
      That(() => Object.Prototype.Construct(), Throws.TypeError);

      That(Object.Construct()["constructor"], Is.EqualTo(Object));
      That(Object.Prototype.Invoke("isPrototypeOf", Object.Construct()), Is.EqualTo(true));
      That(Object.Invoke("isExtensible", Object.Prototype), Is.EqualTo(true));
    }

    [Test, RuntimeFunctionInjection]
    public void HasOwnProperty(RuntimeFunction hasOwnProperty) {
      IsUnconstructableFunctionWLength(hasOwnProperty, "hasOwnProperty", 1);
      IsAbruptedFromToObject(hasOwnProperty);

      Case((new EcmaObject(), "foo"), false);
      Case((CreateObject(("foo", 42)), "foo"), true);
      Case((Object.Invoke("create", CreateObject(("foo", 42))), "foo"), false);

      Symbol sym = new Symbol();
      EcmaValue returnSym = RuntimeFunction.Create(() => sym);
      Case((CreateObject((sym, 42)), sym), true);
      Case((CreateObject((sym, 42)), CreateObject((Symbol.ToPrimitive, returnSym))), true);
      Case((CreateObject((sym, 42)), CreateObject(("toString", returnSym), ("valueOf", Null))), true);
      Case((CreateObject((sym, 42)), CreateObject(("valueOf", returnSym), ("toString", Null))), true);

      EcmaValue o;
      o = RuntimeFunction.Create(() => Void(This.ToObject()["foo"] = 42)).Construct();
      Case((o, "foo"), true);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42);
      Case((o, "foo"), true);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, configurable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, writable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, enumerable: true, configurable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, enumerable: true, writable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, configurable: true, writable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", value: 42, configurable: true, enumerable: true, writable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", set: _ => 42);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, set: _ => 42);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", set: _ => 42, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, set: _ => 42, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, configurable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", set: _ => 42, configurable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, set: _ => 42, configurable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, configurable: true, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", set: _ => 42, configurable: true, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);

      o = new EcmaObject();
      DefineProperty(o, "foo", get: () => 42, set: _ => 42, configurable: true, enumerable: true);
      Case((o, "foo"), true);
      Case((Object.Invoke("create", o), "foo"), false);
    }

    [Test, RuntimeFunctionInjection]
    public void IsPrototypeOf(RuntimeFunction isPrototypeOf) {
      IsUnconstructableFunctionWLength(isPrototypeOf, "isPrototypeOf", 1);
      IsAbruptedFromToObject(isPrototypeOf);

      EcmaValue userFactory = RuntimeFunction.Create(() => Undefined);
      EcmaValue proto = userFactory.Construct();

      EcmaValue forceUserFactory = RuntimeFunction.Create(() => Undefined);
      forceUserFactory["prototype"] = proto;

      Case((proto, forceUserFactory.Construct()), true);
      Case((userFactory["prototype"], forceUserFactory.Construct()), true);
    }

    [Test, RuntimeFunctionInjection]
    public void PropertyIsEnumerable(RuntimeFunction propertyIsEnumerable) {
      IsUnconstructableFunctionWLength(propertyIsEnumerable, "propertyIsEnumerable", 1);
      IsAbruptedFromToObject(propertyIsEnumerable);

      RuntimeFunction factory = RuntimeFunction.Create(v => Void(This.ToObject()["name"] = v));
      factory.Prototype.Set("rootprop", true);
      EcmaValue obj = factory.Construct("name");
      Case((obj, "name"), true);
      Case((obj, "rootprop"), false, "propertyIsEnumerable method does not consider objects in the prototype chain");

      Symbol sym = new Symbol();
      EcmaValue returnSym = RuntimeFunction.Create(() => sym);
      Case((CreateObject((sym, 42)), sym), true);
      Case((CreateObject((sym, 42)), CreateObject((Symbol.ToPrimitive, returnSym))), true);
      Case((CreateObject((sym, 42)), CreateObject(("toString", returnSym), ("valueOf", Null))), true);
      Case((CreateObject((sym, 42)), CreateObject(("valueOf", returnSym), ("toString", Null))), true);
    }

    [Test, RuntimeFunctionInjection]
    public void ToLocaleString(RuntimeFunction toLocaleString) {
      IsUnconstructableFunctionWLength(toLocaleString, "toLocaleString", 0);
      IsAbruptedFromToObject(toLocaleString);

      Case(CreateObject(toString: () => "foo"), "foo");
    }

    [Test, RuntimeFunctionInjection]
    public void ToString(RuntimeFunction toString) {
      IsUnconstructableFunctionWLength(toString, "toString", 0);

      EcmaValue obj = new EcmaObject();
      DefineProperty(obj, Symbol.ToStringTag, get: ThrowTest262Exception);
      Case(obj, Throws.Test262);

      EcmaValue args = default;
      RuntimeFunction.Create(() => args = Arguments).Call();
      Case(args, "[object Arguments]");

      Case(new EcmaArray(), "[object Array]");

      Case(true, "[object Boolean]");
      Case(Boolean.Call(_, true), "[object Boolean]");
      Case(Boolean.Construct(true), "[object Boolean]");
      Case(Object.Call(_, true), "[object Boolean]");
      Case(Object.Call(_, Boolean.Call(true)), "[object Boolean]");
      Case(Object.Call(_, Boolean.Construct(true)), "[object Boolean]");

      Case(new EcmaDate(), "[object Date]");
      Case(Object.Call(_, new EcmaDate()), "[object Date]");

      Case(RuntimeFunction.Create(() => Undefined), "[object Function]");

      Case(Null, "[object Null]");

      Case(42, "[object Number]");
      Case(Number.Call(_, 42), "[object Number]");
      Case(Number.Construct(42), "[object Number]");
      Case(Object.Call(_, 42), "[object Number]");
      Case(Object.Call(_, Number.Call(42)), "[object Number]");
      Case(Object.Call(_, Number.Construct(42)), "[object Number]");

      Case(Object.Prototype, "[object Object]");
      Case(new EcmaObject(), "[object Object]");

      Case(RegExp.Construct("."), "[object RegExp]");

      Case("", "[object String]");
      Case(String.Call(_, ""), "[object String]");
      Case(String.Construct(""), "[object String]");
      Case(Object.Call(_, ""), "[object String]");
      Case(Object.Call(_, String.Call("")), "[object String]");
      Case(Object.Call(_, String.Construct("")), "[object String]");

      Case(Undefined, "[object Undefined]");

      Case(Proxy.Construct(Array.Construct(), Object.Construct()), "[object Array]");

      // Non-string values of `@@toStringTag` property are ignored
      Case(CreateObject((Symbol.ToStringTag, Undefined)), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, Null)), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, Symbol.ToStringTag)), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, 42)), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, String.Construct(""))), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, new EcmaObject())), "[object Object]");
      Case(CreateObject((Symbol.ToStringTag, RuntimeFunction.Create(() => ""))), "[object Object]");
    }

    [Test, RuntimeFunctionInjection]
    public void ValueOf(RuntimeFunction valueOf) {
      IsUnconstructableFunctionWLength(valueOf, "valueOf", 0);
      IsAbruptedFromToObject(valueOf);

      Case(true, Is.TypeOf("object"));
    }
  }
}
