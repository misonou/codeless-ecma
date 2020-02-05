using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ObjectConstructor : TestBase {
    [Test, RuntimeFunctionInjection]
    public void Assign(RuntimeFunction assign) {
      IsUnconstructableFunctionWLength(assign, "assign", 2);
      IsAbruptedFromToObject(assign.Bind(_));
      EcmaValue result, source;

      // first argument is converted to object
      result = assign.Call(_, "a");
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo("a"));

      result = assign.Call(_, 12);
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo(12));

      result = assign.Call(_, true);
      That(result, Is.TypeOf("object"));
      That(result.Invoke("valueOf"), Is.EqualTo(true));

      // a later assignment to the same property overrides an earlier assignment
      result = assign.Call(_, CreateObject(("a", 1)), CreateObject(("a", 2)), CreateObject(("a", 3)));
      That(result["a"], Is.EqualTo(3));

      // string have own enumerable properties, so it can be wrapped to objects.
      result = assign.Call(_, 12, "aaa", "bb2b", "1c");
      That(result[0], Is.EqualTo("1"));
      That(result[1], Is.EqualTo("c"));
      That(result[2], Is.EqualTo("2"));
      That(result[3], Is.EqualTo("b"));

      // Errors thrown during retrieval of source object attributes
      source = new EcmaObject();
      DefineProperty(source, "attr", enumerable: true, get: ThrowTest262Exception);
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Does not assign non-enumerable source properties
      source = new EcmaObject();
      DefineProperty(source, "attr", get: ThrowTest262Exception);
      result = assign.Call(_, new EcmaObject(), source);
      That(result.Invoke("hasOwnProperty", "attr"), Is.EqualTo(false));

      // null and undefined source should be ignored
      // Number, Boolean, Symbol cannot have own enumerable properties
      result = assign.Call(_, new EcmaObject(), Undefined, Null, 12, false, new Symbol());
      That(Object.Invoke("keys", result)["length"], Is.EqualTo(0));

      // Invoked with a source which does not have a descriptor for an own property
      Logs.Clear();
      source = Proxy.Construct(
        new EcmaObject(),
        CreateObject(("ownKeys", Intercept(() => EcmaArray.Of("missing")))));
      result = assign.Call(_, new EcmaObject(), source);
      That(Logs, Has.Exactly(1).Items, "Proxy trap was invoked exactly once");
      That(result.Invoke("hasOwnProperty", "missing"), Is.EqualTo(false));

      // Invoked with a source whose own property descriptor cannot be retrieved
      source = Proxy.Construct(
        CreateObject(("attr", Null)),
        CreateObject(("getOwnPropertyDescriptor", ThrowTest262Exception)));
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Invoked with a source whose own property keys cannot be retrieved
      source = Proxy.Construct(
        CreateObject(("attr", Null)),
        CreateObject(("ownKeys", ThrowTest262Exception)));
      Case((_, new EcmaObject(), source), Throws.Test262);

      // Symbol-valued properties are copied after String-valued properties.
      Symbol sym1 = new Symbol();
      Symbol sym2 = new Symbol();
      Logs.Clear();
      source = CreateObject(
        (sym1, get: Intercept(() => Undefined, "get sym1"), set: null),
        ("a", get: Intercept(() => Undefined, "get a"), set: null),
        (sym2, get: Intercept(() => Undefined, "get sym2"), set: null),
        ("b", get: Intercept(() => Undefined, "get b"), set: null)
      );
      assign.Call(_, new EcmaObject(), source);
      CollectionAssert.AreEqual(new[] { "get a", "get b", "get sym1", "get sym2" }, Logs);

      // Errors thrown during definition of target object attributes
      result = new EcmaObject();
      DefineProperty(result, "attr", value: 2);
      Case((_, result, CreateObject(("attr", 2))), Throws.TypeError);
    }

    [Test, RuntimeFunctionInjection]
    public void Create(RuntimeFunction create) {
      IsUnconstructableFunctionWLength(create, "create", 2);

      Case((_, true), Throws.TypeError);
      Case((_, ""), Throws.TypeError);
      Case((_, 2), Throws.TypeError);

      Case((_, new EcmaObject(), Undefined), Throws.Nothing);
      Case((_, new EcmaObject(), Null), Throws.TypeError);

      // Object.create creates new Object
      Case((_, RuntimeFunction.Create(() => Undefined).Construct()), Is.TypeOf("object"));
      That(create.Call(_, new EcmaObject()).InstanceOf(Object));

      It("should set the prototype to the passed-in object", () => {
        EcmaValue proto = RuntimeFunction.Create(() => Undefined).Construct();
        EcmaValue result = create.Call(_, proto);
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(proto));
        That(proto.Invoke("isPrototypeOf", result), Is.EqualTo(true));
      });

      It("should add new properties", () => {
        EcmaValue proto = RuntimeFunction.Create(() => Undefined).Construct();
        EcmaValue result = create.Call(_, proto, CreateObject(
          ("x", CreateObject(("value", true), ("writable", false))),
          ("y", CreateObject(("value", "str"), ("writable", false)))
        ));
        That(result["x"], Is.EqualTo(true));
        That(result["y"], Is.EqualTo("str"));
        That(proto["x"], Is.Undefined);
        That(proto["y"], Is.Undefined);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Entries(RuntimeFunction entries) {
      IsUnconstructableFunctionWLength(entries, "entries", 1);
      IsAbruptedFromToObject(entries.Bind(_));

      Case((_, CreateObject(
        ("a", get: () => throw new EcmaRangeErrorException(""), set: null),
        ("b", get: ThrowTest262Exception, set: null))),
        Throws.RangeError,
        "It should terminate if getting a value throws an exception");

      Case((_, CreateObject(
        ("a", get: () => Return(This.ToObject()["c"] = "C", "A"), set: null),
        ("b", get: () => "B", set: null))),
        new[] { new[] { "a", "A" }, new[] { "b", "B" } },
        "It does not see a new element added by a getter that is hit during iteration");

      Case((_, CreateObject(
        ("a", get: () => "A", set: null),
        ("b", get: () => Return(Object.Invoke("defineProperty", This, "c", CreateObject(("enumerable", false))), "B"), set: null),
        ("c", get: () => "C", set: null))),
        new[] { new[] { "a", "A" }, new[] { "b", "B" } },
        "It does not see an element made non-enumerable by a getter that is hit during iteration");

      Case((_, CreateObject(
        ("a", get: () => "A", set: null),
        ("b", get: () => Return(This.ToObject().Delete("c"), "B"), set: null),
        ("c", get: () => "C", set: null))),
        new[] { new[] { "a", "A" }, new[] { "b", "B" } },
        "It does not see an element removed by a getter that is hit during iteration");

      It("does not see inherited properties", () => {
        RuntimeFunction F = RuntimeFunction.Create(() => Undefined);
        F.Prototype["a"] = "A";
        F.Prototype["b"] = "B";
        EcmaValue f = F.Construct();
        f["b"] = "b";
        f["c"] = "c";
        Case((_, f), new[] { new[] { "b", "b" }, new[] { "c", "c" } });
      });

      It("should accept primitives", () => {
        Case((_, true), EcmaValue.EmptyArray);
        Case((_, false), EcmaValue.EmptyArray);
        Case((_, 0), EcmaValue.EmptyArray);
        Case((_, -0d), EcmaValue.EmptyArray);
        Case((_, Infinity), EcmaValue.EmptyArray);
        Case((_, -Infinity), EcmaValue.EmptyArray);
        Case((_, NaN), EcmaValue.EmptyArray);
        Case((_, new Symbol()), EcmaValue.EmptyArray);
        Case((_, "abc"), new[] { new[] { "0", "a" }, new[] { "1", "b" }, new[] { "2", "c" } });
      });

      It("should perform observable operations in the correct order", () => {
        Logs.Clear();
        EcmaValue proxy = CreateProxyCompleteTraps(
          CreateObject(("a", 0), ("b", 0), ("c", 0)),
          CreateObject(("get", Intercept((a, b) => a[b], "get:{1}")),
                       ("getOwnPropertyDescriptor", Intercept((a, b) => Object.Invoke("getOwnPropertyDescriptor", a, b), "getOwnPropertyDescriptor:{1}")),
                       ("ownKeys", Intercept(a => Object.Invoke("getOwnPropertyNames", a), "ownKeys"))));
        entries.Call(_, proxy);
        CollectionAssert.AreEqual(new[] { "ownKeys", "getOwnPropertyDescriptor:a", "get:a", "getOwnPropertyDescriptor:b", "get:b", "getOwnPropertyDescriptor:c", "get:c" }, Logs);
      });

      It("should not have its behavior impacted by modifications to the global property Object", () => {
        using (TempProperty(GlobalThis, "Object", CreateObject(("values", Object["values"])))) {
          Case((_, CreateObject(("a", "A"))), new[] { new[] { "a", "A" } });
        }
      });

      It("should not have its behavior impacted by modifications to Object.keys", () => {
        EcmaValue originalKeys = Object["keys"];
        using (TempProperty(Object, "keys", ThrowTest262Exception)) {
          Case((_, CreateObject(("a", "A"))), new[] { new[] { "a", "A" } });
        }
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Freeze(RuntimeFunction freeze) {
      IsUnconstructableFunctionWLength(freeze, "freeze", 1);

      It("should not throw error when argument is undefined or null", () => {
        Case((_, Undefined), Undefined);
        Case((_, Null), Null);
      });

      It("should accept primitives", () => {
        Symbol sym = new Symbol();
        Case((_, true), true);
        Case((_, false), false);
        Case((_, 0), 0);
        Case((_, -0d), -0d);
        Case((_, Infinity), Infinity);
        Case((_, -Infinity), -Infinity);
        Case((_, NaN), NaN);
        Case((_, sym), sym);
        Case((_, "abc"), "abc");
      });

      It("should set object [[IsExtensible]] to false", () => {
        EcmaValue obj = new EcmaObject();
        Assume.That(Object.Invoke("isExtensible", obj), Is.EqualTo(true));
        freeze.Call(_, obj);
        That(Object.Invoke("isExtensible", obj), Is.EqualTo(false));
        That(() => DefineProperty(obj, "c", value: 10), Throws.TypeError, "properties cannot be added into sealed object");
      });

      It("should not freeze inherited properties", () => {
        EcmaValue proto = new EcmaObject();
        DefineProperty(proto, "prop1", value: 10, configurable: true);
        DefineProperty(proto, "prop2", get: () => 10, configurable: true);

        EcmaValue ConstructFn = RuntimeFunction.Create(() => Undefined);
        ConstructFn["prototype"] = proto;

        EcmaValue child = ConstructFn.Construct();
        freeze.Call(_, child);
        proto.ToObject().Delete("prop1");
        proto.ToObject().Delete("prop2");
        That(proto.HasOwnProperty("prop1"), Is.EqualTo(false));
        That(proto.HasOwnProperty("prop2"), Is.EqualTo(false));
      });

      It("should set [[Configurable]] and [[Writable]] attribute of own property to false", () => {
        EcmaValue obj;

        obj = new EcmaObject();
        obj["foo"] = 10;
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.Enumerable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true);
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.None));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true, writable: true);
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.None));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true, enumerable: true);
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.Enumerable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, configurable: true);
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", EcmaPropertyAttributes.None));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, configurable: true, enumerable: true);
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty("foo", EcmaPropertyAttributes.Enumerable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));
      });

      It("should freeze symbol properties", () => {
        Symbol sym = new Symbol();
        EcmaValue obj = CreateObject((sym, 1));
        freeze.Call(_, obj);
        That(obj, Has.OwnProperty(sym, EcmaPropertyAttributes.Enumerable));
      });

      It("should not throw errors on repeated calls", () => {
        That(() => {
          EcmaValue obj = new EcmaObject();
          freeze.Call(_, obj);
          freeze.Call(_, obj);
        }, Throws.Nothing);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void FromEntries(RuntimeFunction fromEntries) {
      IsUnconstructableFunctionWLength(fromEntries, "fromEntries", 1);

      That(Object.Invoke("keys", fromEntries.Call(_, new EcmaArray())), Is.Empty,
        "makes an empty object when given an empty list");

      That(Object.Invoke("getPrototypeOf", fromEntries.Call(_, new EcmaArray())), Is.EqualTo(Object.Prototype),
        "create objects that inherits from Object.prototype");

      It("should throw when invalid argument is given", () => {
        Case(_, Throws.TypeError);
        Case((_, EcmaArray.Of("ab")), Throws.TypeError);
        Case((_, EcmaArray.Of(String.Construct("ab"))), Throws.Nothing);
      });

      It("should create data properties which are enumerable, writable, and configurable", () => {
        EcmaValue result = fromEntries.Call(_, EcmaArray.Of(EcmaArray.Of("key", "value")));
        That(result, Has.OwnProperty("key", "value", EcmaPropertyAttributes.DefaultDataProperty));
      });

      It("should allow symbol keys", () => {
        Symbol sym = new Symbol();
        EcmaValue result = fromEntries.Call(_, EcmaArray.Of(EcmaArray.Of(sym, "value")));
        That(result[sym], Is.EqualTo("value"));
      });

      It("should coerce keys to strings using ToPropertyKey", () => {
        Logs.Clear();
        EcmaValue key = CreateObject(toPrimitive: Intercept(() => "key", "hint:{0}"));
        EcmaValue result = fromEntries.Call(_, EcmaArray.Of(EcmaArray.Of(key, "value")));
        That(result["key"], Is.EqualTo("value"));
        CollectionAssert.AreEqual(new[] { "hint:string" }, Logs);
      });

      It("should create properties in the order of entries in the iterable", () => {
        var result = fromEntries.Call(_, EcmaArray.Of(
          EcmaArray.Of("z", 1),
          EcmaArray.Of("y", 2),
          EcmaArray.Of("x", 3),
          EcmaArray.Of("y", 4)
        ));
        That(result["z"], Is.EqualTo(1));
        That(result["y"], Is.EqualTo(4));
        That(result["x"], Is.EqualTo(3));
        That(Object.Invoke("keys", result), Is.EquivalentTo(new[] { "z", "y", "x" }));
      });

      It("should cause observable events in the correct order", () => {
        EcmaValue makeEntry = RuntimeFunction.Create(label => CreateObject(
          ("0", get: Intercept(() => CreateObject(toString: Intercept(() => label + " key", (string)label + "[0].toString")), (string)label + "[0]"), set: null),
          ("1", get: Intercept(() => label + " value", (string)label + "[1]"), set: null)
        ));
        EcmaValue iterable = new EcmaObject();
        iterable[Symbol.Iterator] = Intercept(() => {
          var count = 0;
          return CreateObject(("next", RuntimeFunction.Create(() => {
            Logs.Add("next " + count);
            switch (count) {
              case 0:
                count++;
                return CreateObject(new { done = false, value = makeEntry.Call(_, "first") });
              case 1:
                count++;
                return CreateObject(new { done = false, value = makeEntry.Call(_, "second") });
              default:
                return CreateObject(new { done = true });
            }
          })));
        }, "get iterable[@@iterator]");

        Logs.Clear();
        EcmaValue result = fromEntries.Call(_, iterable);
        That(result["first key"], Is.EqualTo("first value"));
        That(result["second key"], Is.EqualTo("second value"));
        CollectionAssert.AreEqual(new[] { "get iterable[@@iterator]", "next 0", "first[0]", "first[1]", "first[0].toString", "next 1", "second[0]", "second[1]", "second[0].toString", "next 2" }, Logs);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void GetPrototypeOf(RuntimeFunction getPrototypeOf) {
      IsUnconstructableFunctionWLength(getPrototypeOf, "getPrototypeOf", 1);
      IsAbruptedFromToObject(getPrototypeOf.Bind(_));

      Case((_, true), Boolean.Prototype);
      Case((_, "abc"), String.Prototype);
      Case((_, 0), Number.Prototype);
      Case((_, Boolean), Function.Prototype);
      Case((_, String), Function.Prototype);
      Case((_, Number), Function.Prototype);
      Case((_, new EcmaObject()), Object.Prototype);
      Case((_, new EcmaDate()), Date.Prototype);
      Case((_, new EcmaArray()), Array.Prototype);
      Case((_, RegExp.Construct(".")), RegExp.Prototype);
      Case((_, Object.Prototype), Null);

      EcmaValue Base = RuntimeFunction.Create(() => Undefined);
      Case((_, Base.Construct()), Base["prototype"]);
    }

    [Test, RuntimeFunctionInjection(WellKnownObject.ObjectConstructor, "is")]
    public void Is_(RuntimeFunction @is) {
      IsUnconstructableFunctionWLength(@is, "is", 2);

      It("should return true for same value", () => {
        Case(_, true, "`Object.is()` returns `true`");
        Case((_, Undefined, Undefined), true, "`Object.is(undefined, undefined)` returns `true`");
        Case((_, Undefined), true, "`Object.is(undefined)` returns `true`");
        Case((_, Null, Null), true, "`Object.is(null, null)` returns `true`");
        Case((_, true, true), true, "`Object.is(true, true)` returns `true`");
        Case((_, false, false), true, "`Object.is(false, false)` returns `true`");
        Case((_, NaN, NaN), true, "`Object.is(NaN, NaN)` returns `true`");
        Case((_, -0d, -0d), true, "`Object.is(-0, -0)` returns `true`");
        Case((_, +0d, +0d), true, "`Object.is(+0, +0)` returns `true`");
        Case((_, 0, 0), true, "`Object.is(0, 0)` returns `true`");

        EcmaValue a = new EcmaObject();
        EcmaValue b = Object.Call(_, 0);
        EcmaValue c = Object.Construct("");
        EcmaValue d = new EcmaArray();
        EcmaValue e = Array.Call();
        EcmaValue f = Array.Construct();

        Case((_, a, a), true, "`Object.is(a, a)` returns `true`");
        Case((_, b, b), true, "`Object.is(b, b)` returns `true`");
        Case((_, c, c), true, "`Object.is(c, c)` returns `true`");
        Case((_, d, d), true, "`Object.is(d, d)` returns `true`");
        Case((_, e, e), true, "`Object.is(e, e)` returns `true`");
        Case((_, f, f), true, "`Object.is(f, f)` returns `true`");


        Case((_, "", ""), true, "`Object.is('', '')` returns `true`");
        Case((_, "foo", "foo"), true, "`Object.is('foo', 'foo')` returns `true`");
        Case((_, String.Call(_, "foo"), String.Call(_, "foo")), true, "`Object.is(String('foo'), String('foo'))` returns `true`");

        Symbol sym1 = new Symbol();
        Symbol sym2 = new Symbol("description");
        Case((_, sym1, sym1), true, "`Object.is(a, a)` returns `true`");
        Case((_, sym2, sym2), true, "`Object.is(b, b)` returns `true`");
      });

      It("should return false for different value of the same type", () => {
        Case((_, true, false), false, "`Object.is(true, false)` returns `false`");
        Case((_, false, true), false, "`Object.is(false, true)` returns `false`");
        Case((_, true, 1), false, "`Object.is(true, 1)` returns `false`");
        Case((_, false, 0), false, "`Object.is(false, 0)` returns `false`");
        Case((_, true, new EcmaObject()), false, "`Object.is(true, {})` returns `false`");
        Case((_, true, Undefined), false, "`Object.is(true, undefined)` returns `false`");
        Case((_, false, Undefined), false, "`Object.is(false, undefined)` returns `false`");
        Case((_, true, Null), false, "`Object.is(true, null)` returns `false`");
        Case((_, false, Null), false, "`Object.is(false, null)` returns `false`");
        Case((_, true, NaN), false, "`Object.is(true, NaN)` returns `false`");
        Case((_, false, NaN), false, "`Object.is(false, NaN)` returns `false`");
        Case((_, true, ""), false, "`Object.is(true, '')` returns `false`");
        Case((_, false, ""), false, "`Object.is(false, '')` returns `false`");
        Case((_, true, new EcmaArray()), false, "`Object.is(true, [])` returns `false`");
        Case((_, false, new EcmaArray()), false, "`Object.is(false, [])` returns `false`");

        Case((_, Null), false, "`Object.is(null)` returns `false`");
        Case((_, Null, Undefined), false, "`Object.is(null, undefined)` returns `false`");
        Case((_, Undefined, Null), false, "`Object.is(undefined, null)` returns `false`");

        Case((_, +0d, -0d), false, "`Object.is(+0, -0)` returns `false`");
        Case((_, -0d, +0d), false, "`Object.is(-0, +0)` returns `false`");
        Case((_, 0), false, "`Object.is(0)` returns `false`");
        Case((_, Infinity, -Infinity), false, "`Object.is(Infinity, -Infinity)` returns `false`");

        Case((_, new EcmaObject(), new EcmaObject()), false, "`Object.is({}, {})` returns `false`");
        Case((_, Object.Call(), Object.Call()), false, "`Object.is(Object(), Object())` returns `false`");
        Case((_, Object.Construct(), Object.Construct()), false, "`Object.is(new Object(), new Object())` returns `false`");
        Case((_, Object.Call(_, 0), Object.Call(_, 0)), false, "`Object.is(Object(0), Object(0))` returns `false`");
        Case((_, Object.Construct(""), Object.Construct("")), false, "`Object.is(new Object(''), new Object(''))` returns `false`");

        Case((_, "", true), false, "`Object.is('', true)` returns `false`");
        Case((_, "", 0), false, "`Object.is('', 0)` returns `false`");
        Case((_, "", new EcmaObject()), false, "`Object.is('', {})` returns `false`");
        Case((_, "", Undefined), false, "`Object.is('', undefined)` returns `false`");
        Case((_, "", Null), false, "`Object.is('', null)` returns `false`");
        Case((_, "", NaN), false, "`Object.is('', NaN)` returns `false`");

        Symbol sym1 = new Symbol();
        Symbol sym2 = new Symbol("description");
        Case((_, sym1, sym2), false, "`Object.is(a, b)` returns `false`");
      });

      It("should return false for values of different type", () => {
        EcmaValue a = new EcmaObject();

        Case((_, a, true), false, "`Object.is(a, true)` returns `false`");
        Case((_, a, ""), false, "`Object.is(a, '')` returns `false`");
        Case((_, a, 0), false, "`Object.is(a, 0)` returns `false`");
        Case((_, a, Undefined), false, "`Object.is(a, undefined)` returns `false`");

        Case((_, NaN, true), false, "`Object.is(NaN, true)` returns `false`");
        Case((_, NaN, ""), false, "`Object.is(NaN, '')` returns `false`");
        Case((_, NaN, a), false, "`Object.is(NaN, a)` returns `false`");
        Case((_, NaN, Undefined), false, "`Object.is(NaN, undefined)` returns `false`");
        Case((_, NaN, Null), false, "`Object.is(NaN, null)` returns `false`");

        Case((_, true, 0), false, "`Object.is(true, 0)` returns `false`");
        Case((_, true, a), false, "`Object.is(true, a)` returns `false`");
        Case((_, true, Undefined), false, "`Object.is(true, undefined)` returns `false`");
        Case((_, true, Null), false, "`Object.is(true, null)` returns `false`");
        Case((_, true, NaN), false, "`Object.is(true, NaN)` returns `false`");
        Case((_, true, ""), false, "`Object.is(true, '')` returns `false`");

        Case((_, false, 0), false, "`Object.is(false, 0)` returns `false`");
        Case((_, false, a), false, "`Object.is(false, a)` returns `false`");
        Case((_, false, Undefined), false, "`Object.is(false, undefined)` returns `false`");
        Case((_, false, Null), false, "`Object.is(false, null)` returns `false`");
        Case((_, false, NaN), false, "`Object.is(false, NaN)` returns `false`");
        Case((_, false, ""), false, "`Object.is(false, '')` returns `false`");

        Case((_, 0, true), false, "`Object.is(0, true)` returns `false`");
        Case((_, 0, a), false, "`Object.is(0, a)` returns `false`");
        Case((_, 0, Undefined), false, "`Object.is(0, undefined)` returns `false`");
        Case((_, 0, Null), false, "`Object.is(0, null)` returns `false`");
        Case((_, 0, NaN), false, "`Object.is(0, NaN)` returns `false`");
        Case((_, 0, ""), false, "`Object.is(0, '')` returns `false`");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsExtensible(RuntimeFunction isExtensible) {
      IsUnconstructableFunctionWLength(isExtensible, "isExtensible", 1);

      It("should return false if argument is undefined or null", () => {
        Case((_, Undefined), false);
        Case((_, Null), false);
      });

      It("should return false if argument is primitive value", () => {
        Symbol sym = new Symbol();
        Case((_, true), false);
        Case((_, false), false);
        Case((_, 0), false);
        Case((_, -0d), false);
        Case((_, Infinity), false);
        Case((_, -Infinity), false);
        Case((_, NaN), false);
        Case((_, sym), false);
        Case((_, "abc"), false);
      });

      It("should return false if Object is extensible", () => {
        Case((_, new EcmaObject()), true);

        // all built-in objects are not sealed except %ThrowTypeError%
        foreach (WellKnownObject o in System.Enum.GetValues(typeof(WellKnownObject))) {
          if (o != WellKnownObject.MaxValue && o != WellKnownObject.ThrowTypeError) {
            Case((_, (RuntimeObject)o), true);
          }
        }
      });

      It("should only check the Object argument but not its prototype", () => {
        EcmaValue proto, ConstructorFn, child;

        // extensible-prevented proto, extensible child
        proto = new EcmaObject();
        Object.Invoke("preventExtensions", proto);

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        Case((_, child), true, "extensible-prevented proto, extensible child");

        // extensible proto, extensible-prevented child
        proto = new EcmaObject();

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        Object.Invoke("preventExtensions", child);
        Case((_, child), false, "extensible proto, extensible-prevented child");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsFrozen(RuntimeFunction isFrozen) {
      IsUnconstructableFunctionWLength(isFrozen, "isFrozen", 1);

      It("should return true if argument is undefined or null", () => {
        Case((_, Undefined), true);
        Case((_, Null), true);
      });

      It("should return true if argument is primitive value", () => {
        Symbol sym = new Symbol();
        Case((_, true), true);
        Case((_, false), true);
        Case((_, 0), true);
        Case((_, -0d), true);
        Case((_, Infinity), true);
        Case((_, -Infinity), true);
        Case((_, NaN), true);
        Case((_, sym), true);
        Case((_, "abc"), true);
      });

      It("should return true when all own properties of 'O' are not writable and not configurable, and 'O' is not extensible", () => {
        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 20, writable: false, enumerable: false, configurable: false);
        DefineProperty(obj, "bar", get: () => 10, set: _ => _, configurable: false);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), true);
      });

      It("should return false if Object is extensible or contains own configurable or writable properties", () => {
        EcmaValue obj;
        Case((_, new EcmaObject()), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, set: _ => _, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: null, set: _ => _, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, set: null, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 20, writable: false, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 20, writable: true, configurable: false);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        // all built-in objects are not sealed except %ThrowTypeError%
        foreach (WellKnownObject o in System.Enum.GetValues(typeof(WellKnownObject))) {
          if (o != WellKnownObject.MaxValue && o != WellKnownObject.ThrowTypeError) {
            Case((_, (RuntimeObject)o), false);
          }
        }
      });

      It("should only check own properties", () => {
        EcmaValue proto, ConstructorFn, child;

        // freezed proto, extensible child
        proto = new EcmaObject();
        DefineProperty(proto, "foo", get: () => 20, set: null, configurable: false);
        Object.Invoke("preventExtensions", proto);

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        DefineProperty(child, "foo", get: null, set: _ => _, configurable: true);
        Object.Invoke("preventExtensions", child);
        Case((_, child), false, "freezed proto, extensible child");

        // extensible proto, freezed child
        proto = new EcmaObject();
        DefineProperty(proto, "foo", get: () => 20, set: null, configurable: true);
        Object.Invoke("preventExtensions", proto);

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        DefineProperty(child, "foo", get: null, set: _ => _, configurable: false);
        Object.Invoke("preventExtensions", child);
        Case((_, child), true, "extensible proto, freezed child");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void IsSealed(RuntimeFunction isSealed) {
      IsUnconstructableFunctionWLength(isSealed, "isSealed", 1);

      It("should return true if argument is undefined or null", () => {
        Case((_, Undefined), true);
        Case((_, Null), true);
      });

      It("should return true if argument is primitive value", () => {
        Symbol sym = new Symbol();
        Case((_, true), true);
        Case((_, false), true);
        Case((_, 0), true);
        Case((_, -0d), true);
        Case((_, Infinity), true);
        Case((_, -Infinity), true);
        Case((_, NaN), true);
        Case((_, sym), true);
        Case((_, "abc"), true);
      });

      It("should return true when all own properties of 'O' are not configurable, and 'O' is not extensible", () => {
        EcmaValue obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 20, writable: true, enumerable: false, configurable: false);
        DefineProperty(obj, "bar", get: () => 10, set: _ => _, configurable: false);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), true);
      });

      It("should return false if Object is extensible or contains own configurable properties", () => {
        EcmaValue obj;
        Case((_, new EcmaObject()), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, set: _ => _, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: null, set: _ => _, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, set: null, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 20, writable: false, configurable: true);
        Object.Invoke("preventExtensions", obj);
        Case((_, obj), false);

        // all built-in objects are not sealed except %ThrowTypeError%
        foreach (WellKnownObject o in System.Enum.GetValues(typeof(WellKnownObject))) {
          if (o != WellKnownObject.MaxValue && o != WellKnownObject.ThrowTypeError) {
            Case((_, (RuntimeObject)o), false);
          }
        }
      });

      It("should only check own properties", () => {
        EcmaValue proto, ConstructorFn, child;

        // sealed proto, extensible child
        proto = new EcmaObject();
        DefineProperty(proto, "foo", value: 20, writable: false, configurable: false);
        Object.Invoke("preventExtensions", proto);

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        DefineProperty(child, "foo", value: 20, writable: true, configurable: true);
        Object.Invoke("preventExtensions", child);
        Case((_, child), false, "sealed proto, extensible child");

        // extensible proto, sealed child
        proto = new EcmaObject();
        DefineProperty(proto, "foo", value: 20, writable: true, configurable: true);
        Object.Invoke("preventExtensions", proto);

        ConstructorFn = RuntimeFunction.Create(() => Undefined);
        ConstructorFn["prototype"] = proto;

        child = ConstructorFn.Construct();
        DefineProperty(child, "foo", value: 20, writable: false, configurable: false);
        Object.Invoke("preventExtensions", child);
        Case((_, child), true, "extensible proto, sealed child");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Keys(RuntimeFunction keys) {
      IsUnconstructableFunctionWLength(keys, "keys", 1);
      IsAbruptedFromToObject(keys.Bind(_));

      It("should return the standard built-in Array", () => {
        EcmaValue returnedValue = keys.Call(_, new EcmaObject());
        That(EcmaArray.IsArray(returnedValue));
        That(Object.Invoke("isExtensible", returnedValue), Is.EqualTo(true));
      });

      It("should return a freah array on each invocation", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue arr1 = keys.Call(_, obj);
        EcmaValue arr2 = keys.Call(_, obj);
        That(arr1, Is.Not.EqualTo(arr2));
      });

      It("should return an Array containing own enumerable properties", () => {
        EcmaValue obj;

        obj = new EcmaObject();
        DefineProperty(obj, "a", value: 10);
        DefineProperty(obj, "b", value: 10, enumerable: true);
        Case((_, obj), new[] { "b" });

        obj = new EcmaObject();
        obj["a"] = 10;
        obj["b"] = 10;
        Case((_, obj), new[] { "a", "b" });

        obj = EcmaArray.Of(1, 2);
        Case((_, obj), new[] { "0", "1" });

        RuntimeFunction.Create(() => obj = Arguments).Call(_, "a", "b", "c", "d");
        Case((_, obj), new[] { "0", "1", "2", "3" });
      });

      It("should not include inherited enumerable property that is overridden by non-enumerable own property", () => {
        EcmaValue proto = new EcmaObject();
        DefineProperty(proto, "prop", value: 10, enumerable: true, configurable: true);

        EcmaValue ConstructFn = RuntimeFunction.Create(() => Undefined);
        ConstructFn["prototype"] = proto;

        EcmaValue child = ConstructFn.Construct();
        DefineProperty(child, "prop", value: 20, configurable: true);

        Case((_, child), Has.Exactly(0).Items);
      });

      It("should return elements in the same order with the order of properties", () => {
        EcmaValue obj = new EcmaObject();
        obj["prop2"] = 1;
        obj["prop1"] = 2;
        EcmaValue result = keys.Call(_, obj);
        int i = 0;
        foreach (EcmaPropertyKey k in obj) {
          That(result[i++], Is.EqualTo(k.ToValue()));
        }
      });

      It("should perform [[GetOwnProperty]] observably in the correct order", () => {
        Logs.Clear();
        Symbol sym = new Symbol();
        EcmaValue target = CreateObject(("x", true));
        EcmaValue ownKeys = CreateObject(
          ("length", get: Intercept(() => 3, "ownKeys.length"), set: null),
          ("0", get: Intercept(() => "a", "ownKeys[0]"), set: null),
          ("1", get: Intercept(() => sym, "ownKeys[1]"), set: null),
          ("2", get: Intercept(() => "b", "ownKeys[2]"), set: null)
        );
        EcmaValue ownKeysDescriptors = CreateObject(
          ("a", CreateObject(new { enumerable = true, configurable = true, value = 1 })),
          ("b", CreateObject(new { enumerable = false, configurable = true, value = 2 })),
          (sym, CreateObject(new { enumerable = true, configurable = true, value = 3 }))
        );
        EcmaValue handler = CreateObject(
          ("ownKeys", Intercept(() => ownKeys, "ownKeys")),
          ("getOwnPropertyDescriptor", Intercept((a, b) => ownKeysDescriptors[b], "getOwnPropertyDescriptor:{1}"))
        );
        Case((_, Proxy.Construct(target, handler)), new[] { "a" });
        CollectionAssert.AreEqual(new[] { "ownKeys", "ownKeys.length", "ownKeys[0]", "ownKeys[1]", "ownKeys[2]", "getOwnPropertyDescriptor:a", "getOwnPropertyDescriptor:b" }, Logs);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void PreventExtensions(RuntimeFunction preventExtensions) {
      IsUnconstructableFunctionWLength(preventExtensions, "preventExtensions", 1);

      It("should not throw error when argument is undefined or null", () => {
        Case((_, Undefined), Undefined);
        Case((_, Null), Null);
      });

      It("should accept primitives", () => {
        Symbol sym = new Symbol();
        Case((_, true), true);
        Case((_, false), false);
        Case((_, 0), 0);
        Case((_, -0d), -0d);
        Case((_, Infinity), Infinity);
        Case((_, -Infinity), -Infinity);
        Case((_, NaN), NaN);
        Case((_, sym), sym);
        Case((_, "abc"), "abc");
      });

      It("should set object [[IsExtensible]] to false", () => {
        EcmaValue obj = new EcmaObject();
        Assume.That(Object.Invoke("isExtensible", obj), Is.EqualTo(true));
        preventExtensions.Call(_, obj);
        That(Object.Invoke("isExtensible", obj), Is.EqualTo(false));
        That(() => DefineProperty(obj, "c", value: 10), Throws.TypeError, "properties cannot be added into object that has [[IsExtensible]] set to false");
      });

      It("should not prevent properties from reassignment or deletion", () => {
        EcmaValue obj = CreateObject(("prop", 12));
        preventExtensions.Call(_, obj);
        obj["prop"] = 42;
        That(obj["prop"], Is.EqualTo(42));
        obj.ToObject().Delete("prop");
        That(obj.HasOwnProperty("prop"), Is.EqualTo(false));
      });

      It("should not prevent adding properties to an instance that inherits from that prototype", () => {
        EcmaValue proto = new EcmaObject();
        Assume.That(Object.Invoke("isExtensible", proto), Is.EqualTo(true));
        preventExtensions.Call(_, proto);

        EcmaValue ConstructFn = RuntimeFunction.Create(() => Undefined);
        ConstructFn["prototype"] = proto;

        EcmaValue child = ConstructFn.Construct();
        child["prop"] = 10;
        That(child.HasOwnProperty("prop"), Is.EqualTo(true));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Seal(RuntimeFunction seal) {
      IsUnconstructableFunctionWLength(seal, "seal", 1);

      It("should not throw error when argument is undefined or null", () => {
        Case((_, Undefined), Undefined);
        Case((_, Null), Null);
      });

      It("should accept primitives", () => {
        Symbol sym = new Symbol();
        Case((_, true), true);
        Case((_, false), false);
        Case((_, 0), 0);
        Case((_, -0d), -0d);
        Case((_, Infinity), Infinity);
        Case((_, -Infinity), -Infinity);
        Case((_, NaN), NaN);
        Case((_, sym), sym);
        Case((_, "abc"), "abc");
      });

      It("should set object [[IsExtensible]] to false", () => {
        EcmaValue obj = new EcmaObject();
        Assume.That(Object.Invoke("isExtensible", obj), Is.EqualTo(true));
        seal.Call(_, obj);
        That(Object.Invoke("isExtensible", obj), Is.EqualTo(false));
        That(() => DefineProperty(obj, "c", value: 10), Throws.TypeError, "properties cannot be added into sealed object");
      });

      It("should ignore inherited data properties", () => {
        EcmaValue proto = new EcmaObject();
        DefineProperty(proto, "prop1", value: 10, configurable: true);
        DefineProperty(proto, "prop2", get: () => 10, configurable: true);

        EcmaValue ConstructFn = RuntimeFunction.Create(() => Undefined);
        ConstructFn["prototype"] = proto;

        EcmaValue child = ConstructFn.Construct();
        Assume.That(Object.Invoke("isExtensible", child), Is.EqualTo(true));
        seal.Call(_, child);
        proto.ToObject().Delete("prop1");
        proto.ToObject().Delete("prop2");
        That(proto.HasOwnProperty("prop1"), Is.EqualTo(false));
        That(proto.HasOwnProperty("prop2"), Is.EqualTo(false));
      });

      It("should ignore inherited accessor properties", () => {
        EcmaValue proto = new EcmaObject();
        DefineProperty(proto, "father", get: () => 10, configurable: true);

        EcmaValue ConstructFn = RuntimeFunction.Create(() => Undefined);
        ConstructFn["prototype"] = proto;

        EcmaValue child = ConstructFn.Construct();
        Assume.That(Object.Invoke("isExtensible", child), Is.EqualTo(true));
        Assume.That(proto.HasOwnProperty("father"));
        seal.Call(_, child);
        proto.ToObject().Delete("father");
        That(proto.HasOwnProperty("father"), Is.EqualTo(false));
      });

      It("should set [[Configurable]] attribute of own property to false", () => {
        EcmaValue obj;

        obj = new EcmaObject();
        obj["foo"] = 10;
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Writable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true);
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.None));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true, writable: true);
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.Writable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", value: 10, configurable: true, enumerable: true);
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", 10, EcmaPropertyAttributes.Enumerable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, configurable: true);
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", EcmaPropertyAttributes.None));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));

        obj = new EcmaObject();
        DefineProperty(obj, "foo", get: () => 10, configurable: true, enumerable: true);
        seal.Call(_, obj);
        That(obj, Has.OwnProperty("foo", EcmaPropertyAttributes.Enumerable));
        That(obj.ToObject().Delete("foo"), Is.EqualTo(false));
      });

      It("should seal symbol properties", () => {
        Symbol sym = new Symbol();
        EcmaValue obj = CreateObject((sym, 1));
        seal.Call(_, obj);
        That(obj, Has.OwnProperty(sym, EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Writable));
      });

      It("should not throw errors on repeated calls", () => {
        That(() => {
          EcmaValue obj = new EcmaObject();
          seal.Call(_, obj);
          seal.Call(_, obj);
        }, Throws.Nothing);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void SetPrototypeOf(RuntimeFunction setPrototypeOf) {
      IsUnconstructableFunctionWLength(setPrototypeOf, "setPrototypeOf", 2);
      IsAbruptedFromToObject(setPrototypeOf.Bind(_));

      // invoked with a non-object value
      Case((_, true, Null), true);
      Case((_, 3, Null), 3);
      Case((_, "string", Null), "string");
      Case((_, Symbol.ToPrimitive, Null), Symbol.ToPrimitive);

      // invoked with an invalid prototype value
      Case((_, new EcmaObject()), Throws.TypeError);
      Case((_, new EcmaObject(), Undefined), Throws.TypeError);
      Case((_, new EcmaObject(), true), Throws.TypeError);
      Case((_, new EcmaObject(), 1), Throws.TypeError);
      Case((_, new EcmaObject(), "string"), Throws.TypeError);
      Case((_, new EcmaObject(), new Symbol()), Throws.TypeError);

      // invoked with an object whose prototype cannot be set
      Case((_, Proxy.Construct(new EcmaObject(), CreateObject(new { setPrototypeOf = ThrowTest262Exception })), Null), Throws.Test262);

      // invoked with a value that would create a cycle
      Case((_, Object.Prototype, Array.Prototype), Throws.TypeError);

      // invoked with a non-extensible object
      EcmaValue obj = new EcmaObject();
      Object.Invoke("preventExtensions", obj);
      Case((_, obj, Null), Throws.TypeError);

      EcmaValue propValue = new EcmaObject();
      EcmaValue newProto = CreateObject(new { test262Prop = propValue });
      EcmaValue testObj = new EcmaObject();
      Case((_, testObj, newProto), testObj);
      That(testObj.Invoke("hasOwnProperty", "test262Prop"), Is.EqualTo(false));
      That(testObj["test262Prop"], Is.EqualTo(propValue));
    }

    [Test, RuntimeFunctionInjection]
    public void Values(RuntimeFunction values) {
      IsUnconstructableFunctionWLength(values, "values", 1);
      IsAbruptedFromToObject(values.Bind(_));

      It("should return the standard built-in Array", () => {
        EcmaValue returnedValue = values.Call(_, new EcmaObject());
        That(EcmaArray.IsArray(returnedValue));
        That(Object.Invoke("isExtensible", returnedValue), Is.EqualTo(true));
      });

      It("should return a freah array on each invocation", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue arr1 = values.Call(_, obj);
        EcmaValue arr2 = values.Call(_, obj);
        That(arr1, Is.Not.EqualTo(arr2));
      });

      Case((_, CreateObject(
        ("a", get: () => throw new EcmaRangeErrorException(""), set: null),
        ("b", get: ThrowTest262Exception, set: null))),
        Throws.RangeError,
        "It should terminate if getting a value throws an exception");

      Case((_, CreateObject(
        ("a", get: () => Return(This.ToObject()["c"] = "C", "A"), set: null),
        ("b", get: () => "B", set: null))),
        new[] { "A", "B" },
        "It does not see a new element added by a getter that is hit during iteration");

      Case((_, CreateObject(
        ("a", get: () => "A", set: null),
        ("b", get: () => Return(Object.Invoke("defineProperty", This, "c", CreateObject(("enumerable", false))), "B"), set: null),
        ("c", get: () => "C", set: null))),
        new[] { "A", "B" },
        "It does not see an element made non-enumerable by a getter that is hit during iteration");

      Case((_, CreateObject(
        ("a", get: () => "A", set: null),
        ("b", get: () => Return(This.ToObject().Delete("c"), "B"), set: null),
        ("c", get: () => "C", set: null))),
        new[] { "A", "B" },
        "It does not see an element removed by a getter that is hit during iteration");

      It("does not see inherited properties", () => {
        RuntimeFunction F = RuntimeFunction.Create(() => Undefined);
        F.Prototype["a"] = "A";
        F.Prototype["b"] = "B";
        EcmaValue f = F.Construct();
        f["b"] = "b";
        f["c"] = "c";
        Case((_, f), new[] { "b", "c" });
      });

      It("should accept primitives", () => {
        Case((_, true), EcmaValue.EmptyArray);
        Case((_, false), EcmaValue.EmptyArray);
        Case((_, 0), EcmaValue.EmptyArray);
        Case((_, -0d), EcmaValue.EmptyArray);
        Case((_, Infinity), EcmaValue.EmptyArray);
        Case((_, -Infinity), EcmaValue.EmptyArray);
        Case((_, NaN), EcmaValue.EmptyArray);
        Case((_, new Symbol()), EcmaValue.EmptyArray);
        Case((_, "abc"), new[] { "a", "b", "c" });
      });

      It("does not include Symbol keys", () => {
        Symbol symValue = new Symbol();
        Case((_, CreateObject(("key", symValue), (new Symbol(), "value"))), new[] { symValue });
      });

      It("should perform observable operations in the correct order", () => {
        Logs.Clear();
        EcmaValue proxy = CreateProxyCompleteTraps(
          CreateObject(("a", 0), ("b", 0), ("c", 0)),
          CreateObject(("get", Intercept((a, b) => a[b], "get:{1}")),
                       ("getOwnPropertyDescriptor", Intercept((a, b) => Object.Invoke("getOwnPropertyDescriptor", a, b), "getOwnPropertyDescriptor:{1}")),
                       ("ownKeys", Intercept(a => Object.Invoke("getOwnPropertyNames", a), "ownKeys"))));
        values.Call(_, proxy);
        CollectionAssert.AreEqual(new[] { "ownKeys", "getOwnPropertyDescriptor:a", "get:a", "getOwnPropertyDescriptor:b", "get:b", "getOwnPropertyDescriptor:c", "get:c" }, Logs);
      });

      It("should not have its behavior impacted by modifications to the global property Object", () => {
        EcmaValue fakeObject = CreateObject(("values", Object["values"]));
        using (TempProperty(GlobalThis, "Object", fakeObject)) {
          Case((_, CreateObject(("a", "A"))), new[] { "A" });
        }
      });

      It("should not have its behavior impacted by modifications to Object.keys", () => {
        EcmaValue originalKeys = Object["keys"];
        using (TempProperty(Object, "keys", ThrowTest262Exception)) {
          Case((_, CreateObject(("a", "A"))), new[] { "A" });
        }
      });
    }
  }
}
