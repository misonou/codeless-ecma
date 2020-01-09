using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class ProxyRS : TestBase {
    [Test]
    public void Apply() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _target = default, _args = default, _handler = default, _context = default;
        EcmaValue target = ThrowTest262Exception;
        EcmaValue handler = CreateObject(new {
          apply = RuntimeFunction.Create((t, c, args) => {
            _handler = This;
            _target = t;
            _context = c;
            _args = args;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        EcmaValue context = Object.Construct();
        p.Invoke("call", context, 1, 2);

        That(_handler, Is.EqualTo(handler), "trap context is the handler object");
        That(_target, Is.EqualTo(target), "first parameter is the target object");
        That(_context, Is.EqualTo(context), "second parameter is the call context");
        That(_args, Is.EquivalentTo(new[] { 1, 2 }), "arguments list contains all call arguments");
      });

      It("should return the result from the trap method", () => {
        EcmaValue result = Object.Construct();
        EcmaValue p = Proxy.Construct(ThrowTest262Exception, CreateObject(new {
          apply = RuntimeFunction.Create(_ => result)
        }));
        That(p.Invoke("call"), Is.EqualTo(result));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", RuntimeFunction.Create(_ => _), Object.Construct());
        p.Invoke("revoke");
        That(() => p.Invoke("proxy"), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(RuntimeFunction.Create(_ => throw new System.Exception()), CreateObject(new { apply = ThrowTest262Exception }));
        That(() => p.Call(Undefined), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new { apply = Object.Construct() }));
        That(() => p.Call(Undefined), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new { apply = Object.Construct() }));
        That(() => p.Call(Undefined), Throws.TypeError);
      });

      It("should propagate the call to the target object if the apply trap value is undefined", () => {
        EcmaValue context = default;
        EcmaValue target = Proxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new {
          apply = Intercept((_target, _context, args) => {
            context = _context;
            return args[0] + args[1];
          })
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { apply = Undefined }));
        EcmaValue o = Object.Construct();
        EcmaValue res = p.Invoke("call", o, 1, 2);
        That(Logs.Count, Is.EqualTo(1));
        That(context, Is.EqualTo(o));
        That(res, Is.EqualTo(3));
      });

      It("should propagate the call to the target object if the apply trap value is null", () => {
        EcmaValue context = default;
        EcmaValue target = Proxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new {
          apply = Intercept((_target, _context, args) => {
            context = _context;
            return args[0] + args[1];
          })
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { apply = Null }));
        EcmaValue o = Object.Construct();
        EcmaValue res = p.Invoke("call", o, 1, 2);
        That(Logs.Count, Is.EqualTo(1));
        That(context, Is.EqualTo(o));
        That(res, Is.EqualTo(3));
      });

      It("should propagate the call to the target object if trap is not set", () => {
        EcmaValue context = default;
        EcmaValue target = Proxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new {
          apply = Intercept((_target, _context, args) => {
            context = _context;
            return args[0] + args[1];
          })
        }));
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        EcmaValue o = Object.Construct();
        EcmaValue res = p.Invoke("call", o, 1, 2);
        That(Logs.Count, Is.EqualTo(1));
        That(context, Is.EqualTo(o));
        That(res, Is.EqualTo(3));
      });
    }

    [Test]
    public void Construct() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _P = default, _args = default, _handler = default, _target = default;
        EcmaValue Target = RuntimeFunction.Create(_ => _);
        EcmaValue handler = CreateObject(new {
          construct = RuntimeFunction.Create((target, args, newTarget) => {
            _handler = This;
            _target = target;
            _args = args;
            _P = newTarget;
            return target.Construct(args[0], args[1]);
          })
        });

        EcmaValue P = Proxy.Construct(Target, handler);
        P.Construct(1, 2);

        That(_handler, Is.EqualTo(handler), "trap context is the handler object");
        That(_target, Is.EqualTo(Target), "first parameter is the target object");
        That(_args, Is.EquivalentTo(new[] { 1, 2 }), "arguments list contains all call arguments");
        That(_P, Is.EqualTo(P), "constructor is sent as the third parameter");
      });

      It("should pass correct parameters to the trap method with new target", () => {
        EcmaValue Target = RuntimeFunction.Create(_ => _);
        EcmaValue NewTarget = RuntimeFunction.Create(_ => _);
        EcmaValue handler = default;
        handler = CreateObject(new {
          construct = RuntimeFunction.Create((target, args, newTarget) => {
            That(This, Is.EqualTo(handler));
            That(target, Is.EqualTo(Target));
            That(newTarget, Is.EqualTo(NewTarget));
            That(args, Is.EquivalentTo(new[] { 1, 2 }));
            return CreateObject(new { sum = args[0] + args[1] });
          })
        });

        EcmaValue P = Proxy.Construct(Target, handler);
        That(Reflect.Invoke("construct", P, EcmaArray.Of(1, 2), NewTarget)["sum"], Is.EqualTo(3));
      });

      It("should return the result from the trap method", () => {
        EcmaValue Target = RuntimeFunction.Create((a, b) => This.ToObject()["sum"] = a + b);
        EcmaValue handler = CreateObject(new {
          construct = RuntimeFunction.Create((t, c, args) => CreateObject(new { sum = 42 }))
        });

        EcmaValue P = Proxy.Construct(Target, handler);
        That(P.Construct(1, 2)["sum"], Is.EqualTo(42));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", RuntimeFunction.Create(_ => _), Object.Construct());
        p.Invoke("revoke");
        That(() => p["proxy"].Construct(), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(RuntimeFunction.Create(_ => throw new System.Exception()), CreateObject(new { construct = ThrowTest262Exception }));
        That(() => p.Construct(), Throws.Test262);
      });

      It("should throw a TypeError if trap result is not an Object", () => {
        EcmaValue Target = RuntimeFunction.Create(_ => _);
        EcmaValue P;

        P = Proxy.Construct(Target, CreateObject(new { construct = RuntimeFunction.Create(() => true) }));
        That(() => P.Construct(), Throws.TypeError);

        P = Proxy.Construct(Target, CreateObject(new { construct = RuntimeFunction.Create(() => 0) }));
        That(() => P.Construct(), Throws.TypeError);

        P = Proxy.Construct(Target, CreateObject(new { construct = RuntimeFunction.Create(() => "") }));
        That(() => P.Construct(), Throws.TypeError);

        P = Proxy.Construct(Target, CreateObject(new { construct = RuntimeFunction.Create(() => new Symbol()) }));
        That(() => P.Construct(), Throws.TypeError);

        P = Proxy.Construct(Target, CreateObject(new { construct = RuntimeFunction.Create(() => Undefined) }));
        That(() => P.Construct(), Throws.TypeError);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new { construct = Object.Construct() }));
        That(() => p.Construct(), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(RuntimeFunction.Create(_ => _), CreateObject(new { construct = Object.Construct() }));
        That(() => p.Construct(), Throws.TypeError);
      });

      It("should propagate the construct to the target object if the apply trap value is undefined", () => {
        EcmaValue NewTarget = RuntimeFunction.Create(_ => _);
        EcmaValue Target = Intercept((a, b) => {
          That(New.Target, Is.EqualTo(NewTarget));
          return CreateObject(new { sum = a + b });
        });

        EcmaValue P = Proxy.Construct(Target, CreateObject(new { constructor = Undefined }));
        EcmaValue obj = Reflect.Invoke("construct", P, EcmaArray.Of(3, 4), NewTarget);
        That(obj["sum"], Is.EqualTo(7));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should propagate the construct to the target object if the apply trap value is null", () => {
        EcmaValue NewTarget = RuntimeFunction.Create(_ => _);
        EcmaValue Target = Intercept((a, b) => {
          That(New.Target, Is.EqualTo(NewTarget));
          return CreateObject(new { sum = a + b });
        });

        EcmaValue P = Proxy.Construct(Target, CreateObject(new { constructor = Null }));
        EcmaValue obj = Reflect.Invoke("construct", P, EcmaArray.Of(3, 4), NewTarget);
        That(obj["sum"], Is.EqualTo(7));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should propagate the construct to the target object if the apply trap value is not set", () => {
        EcmaValue NewTarget = RuntimeFunction.Create(_ => _);
        EcmaValue Target = Intercept((a, b) => {
          That(New.Target, Is.EqualTo(NewTarget));
          return CreateObject(new { sum = a + b });
        });

        EcmaValue P = Proxy.Construct(Target, Object.Construct());
        EcmaValue obj = Reflect.Invoke("construct", P, EcmaArray.Of(1, 2), NewTarget);
        That(obj["sum"], Is.EqualTo(3));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should propagate the construct to the target object if the apply trap value is not set (honoring the Realm of the newTarget value)", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue C = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        EcmaValue P = Proxy.Construct(RuntimeFunction.Create(() => Undefined), Object.Construct());
        EcmaValue p = Reflect.Invoke("construct", P, EcmaArray.Of(), C);
        That(Object.Invoke("getPrototypeOf", Object.Invoke("getPrototypeOf", p)), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.ObjectPrototype)));
      });
    }

    [Test]
    public void DefineProperty() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _prop = default, _desc = default;
        EcmaValue target = Object.Construct();
        EcmaValue descriptor = CreateObject(new {
          configurable = true,
          enumerable = true,
          writable = true,
          value = 1
        });
        EcmaValue handler = CreateObject(new {
          defineProperty = RuntimeFunction.Create((t, prop, desc) => {
            _handler = This;
            _target = t;
            _prop = prop;
            _desc = desc;
            return true;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("defineProperty", p, "attr", descriptor);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));

        That(Object.Invoke("keys", _desc)["length"], Is.EqualTo(4), "descriptor arg has the same amount of keys as given descriptor");
        That(_desc["configurable"], Is.EqualTo(true));
        That(_desc["writable"], Is.EqualTo(true));
        That(_desc["enumerable"], Is.EqualTo(true));
        That(_desc["value"], Is.EqualTo(1));
      });

      It("should create property descriptor object in the Realm of the current execution context", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue desc = default;
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new {
          defineProperty = RuntimeFunction.Create((_, __, _desc) => {
            desc = _desc;
            return desc;
          })
        }));
        p["a"] = 0;
        That(Object.Invoke("getPrototypeOf", desc), Is.EqualTo(Object.Prototype));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("defineProperty", p["proxy"], "foo", CreateObject(new { configurable = true, enumerable = true })), Throws.TypeError);
      });

      It("should throw a TypeError exception if handler is null (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Invoke("revocable", Object.Invoke("create", Null), Object.Construct());
        p.Invoke("revoke");
        That(() => p["proxy"].ToObject()["prop"] = Null, Throws.TypeError);
      });

      It("will not throw an exception if a property has a corresponding target object property", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new {
          defineProperty = RuntimeFunction.Create((t, prop, desc) => {
            return Object.Invoke("defineProperty", t, prop, desc);
          })
        }));

        EcmaValue result = Reflect.Invoke("defineProperty", p, "attr", CreateObject(new {
          configurable = true,
          enumerable = true,
          writable = true,
          value = 1
        }));
        That(result, Is.EqualTo(true));
        That(p, Has.OwnProperty("attr", 1, EcmaPropertyAttributes.DefaultDataProperty));

        result = Reflect.Invoke("defineProperty", p, "attr", CreateObject(new {
          configurable = false,
          enumerable = false,
          writable = false,
          value = 2
        }));
        That(result, Is.EqualTo(true));
        That(p, Has.OwnProperty("attr", 2, EcmaPropertyAttributes.None));
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { defineProperty = ThrowTest262Exception }));
        That(() => Object.Invoke("defineProperty", p, "foo", Object.Construct()), Throws.Test262);
      });

      It("should throw a TypeError exception if Desc is not configurable and target property descriptor is configurable and trap result is true", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { defineProperty = RuntimeFunction.Create(() => true) }));
        Object.Invoke("defineProperty", target, "foo", CreateObject(new { value = 1, configurable = true }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { value = 1, configurable = false })), Throws.TypeError);
      });

      It("should Throw a TypeError exception if Desc is not configurable and target property descriptor is undefined, and trap result is true", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { defineProperty = RuntimeFunction.Create(() => true) }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { configurable = false })), Throws.TypeError);
      });

      It("should throw a TypeError exception if Desc is not configurable and target is not extensible, and trap result is true", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { defineProperty = RuntimeFunction.Create(() => true) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("defineProperty", p, "foo", Object.Construct()), Throws.TypeError);
      });

      It("should throw a TypeError exception if Desc and target property descriptor are not compatible and trap result is true", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { defineProperty = RuntimeFunction.Create(() => true) }));
        Object.Invoke("defineProperty", target, "foo", CreateObject(new { value = 1, configurable = false }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { value = 1, configurable = true })), Throws.TypeError);

        target = Object.Construct();
        p = Proxy.Construct(target, CreateObject(new {
          defineProperty = RuntimeFunction.Create(() => true)
        }));
        Object.Invoke("defineProperty", target, "foo", CreateObject(new { value = 1 }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { value = 2 })), Throws.TypeError);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { defineProperty = Object.Construct() }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { value = 1 })), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { defineProperty = Object.Construct() }));
        That(() => Object.Invoke("defineProperty", p, "foo", CreateObject(new { value = 1 })), Throws.TypeError);
      });

      It("should return target.[[DefineOwnProperty]](P, Desc) if trap is undefined", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, Object.Construct());

        Object.Invoke("defineProperty", p, "attr", CreateObject(new {
          configurable = true,
          enumerable = true,
          writable = true,
          value = 1
        }));
        That(p, Has.OwnProperty("attr", 1, EcmaPropertyAttributes.DefaultDataProperty));

        Object.Invoke("defineProperty", p, "attr", CreateObject(new {
          configurable = false,
          enumerable = false,
          writable = false,
          value = 2
        }));
        That(p, Has.OwnProperty("attr", 2, EcmaPropertyAttributes.None));
      });

      It("should check on false trap result", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { defineProperty = RuntimeFunction.Create(() => 0) }));
        That(Reflect.Invoke("defineProperty", p, "attr", Object.Construct()), Is.EqualTo(false));
        That(Object.Invoke("getOwnPropertyDescriptor", target, "attr"), Is.Undefined);
      });
    }

    [Test]
    public void DeleteProperty() {
      It("should return a boolean value", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { deleteProperty = RuntimeFunction.Create(() => 0) }));

        Object.Invoke("defineProperties", target, CreateObject(new {
          isConfigurable = CreateObject(new {
            value = 1,
            configurable = true
          }),
          notConfigurable = CreateObject(new {
            value = 1,
            configurable = false
          })
        }));
        That(Reflect.Invoke("deleteProperty", p, "attr"), Is.EqualTo(false));
        That(Reflect.Invoke("deleteProperty", p, "isConfigurable"), Is.EqualTo(false));
        That(Reflect.Invoke("deleteProperty", p, "notConfigurable"), Is.EqualTo(false));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { deleteProperty = RuntimeFunction.Create(() => 1) }));
        That(Reflect.Invoke("deleteProperty", p, "attr"), Is.EqualTo(true));
      });

      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _prop = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          deleteProperty = RuntimeFunction.Create((t, prop) => {
            _handler = This;
            _target = t;
            _prop = prop;
            return t.ToObject().Delete(EcmaPropertyKey.FromValue(prop));
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        p.ToObject().Delete("attr");

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", CreateObject(new { attr = 1 }), Object.Construct());
        p.Invoke("revoke");
        That(() => p["proxy"].ToObject().Delete("attr"), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { deleteProperty = ThrowTest262Exception }));
        That(() => p.ToObject().Delete("attr"), Throws.Test262);
      });

      It("should throw a TypeError exception if a property cannot be reported as deleted", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { deleteProperty = RuntimeFunction.Create(() => true) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          value = 1
        }));
        That(() => p.ToObject().Delete("attr"), Throws.TypeError);
      });

      It("should return true if targetDesc is undefined", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { deleteProperty = RuntimeFunction.Create(() => true) }));
        That(p.ToObject().Delete("attr"), Is.EqualTo(true));
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { deleteProperty = Object.Construct() }));
        That(() => p.ToObject().Delete("attr"), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { deleteProperty = Object.Construct() }));
        That(() => p.ToObject().Delete("attr"), Throws.TypeError);
      });

      It("should return target.[[Delete]](P) if trap is undefined", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(p.ToObject().Delete("attr"), Is.EqualTo(true));
        That(p.ToObject().Delete("notThere"), Is.EqualTo(true));
        That(Object.Invoke("getOwnPropertyDescriptor", target, "attr"), Is.Undefined);

        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          enumerable = true,
          value = 1
        }));
        That(p.ToObject().Delete("attr"), Is.EqualTo(false));
        That(Reflect.Invoke("deleteProperty", p, "attr"), Is.EqualTo(false));
      });
    }

    [Test]
    public void Enumerate() {
      // Enumerate trap was removed and it should not be triggered anymore
      EcmaValue target = EcmaArray.Of(1, 2, 3);
      EcmaValue p = Proxy.Construct(target, CreateObject(new { enumerate = ThrowTest262Exception }));

      EcmaValue forInResults = EcmaArray.Of();
      foreach (EcmaPropertyKey x in p) {
        forInResults.Invoke("push", x.ToValue());
      }
      That(forInResults, Is.EquivalentTo(new[] { "0", "1", "2" }));

      forInResults = EcmaArray.Of();
      foreach (EcmaValue x in p.ForOf()) {
        forInResults.Invoke("push", x);
      }
      That(forInResults, Is.EquivalentTo(new[] { 1, 2, 3 }));

      EcmaValue itor = p.Invoke(Symbol.Iterator);
      VerifyIteratorResult(itor.Invoke("next"), false, 1);
      VerifyIteratorResult(itor.Invoke("next"), false, 2);
      VerifyIteratorResult(itor.Invoke("next"), false, 3);
      VerifyIteratorResult(itor.Invoke("next"), true);
    }

    [Test]
    public void Get() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _prop = default, _receiver = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          get = RuntimeFunction.Create((t, prop, receiver) => {
            _handler = This;
            _target = t;
            _prop = prop;
            _receiver = receiver;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        EcmaValue _ = p["attr"];

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));
        That(_receiver, Is.EqualTo(p));
      });

      It("should throw if proxy return has not the same value for a non-writable, non-configurable property", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = RuntimeFunction.Create(() => 2) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          writable = false,
          value = 1
        }));
        That(() => p["attr"], Throws.TypeError);
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", CreateObject(new { attr = 1 }), Object.Construct());
        p.Invoke("revoke");
        That(() => p["proxy"]["attr"], Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { get = ThrowTest262Exception }));
        That(() => p["attr"], Throws.Test262);
      });

      It("should return trap result", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = RuntimeFunction.Create(() => 2) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          get = RuntimeFunction.Create(() => 1)
        }));
        That(p["attr"], Is.EqualTo(2));

        Object.Invoke("defineProperty", target, "attr1", CreateObject(new {
          configurable = false,
          writable = true,
          value = 1
        }));
        That(p["attr1"], Is.EqualTo(2));

        Object.Invoke("defineProperty", target, "attr2", CreateObject(new {
          configurable = true,
          get = Undefined
        }));
        That(p["attr2"], Is.EqualTo(2));

        Object.Invoke("defineProperty", target, "attr3", CreateObject(new {
          configurable = true,
          writable = false,
          value = 1
        }));
        That(p["attr3"], Is.EqualTo(2));

        target["attr4"] = 1;
        That(p["attr4"], Is.EqualTo(2));
        That(p["foo"], Is.EqualTo(2));
      });

      It("must report the same value for a non-configurable accessor property with an undefined get if trap result is not undefined", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = RuntimeFunction.Create(() => 2) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          get = Undefined
        }));
        That(() => p["attr"], Throws.TypeError);
      });

      It("must report the same value for a non-writable, non-configurable property", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = RuntimeFunction.Create(() => 1) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          writable = false,
          value = 1
        }));
        That(p["attr3"], Is.EqualTo(1));
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { get = Object.Construct() }));
        That(() => p["attr"], Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { get = Object.Construct() }));
        That(() => p["attr"], Throws.TypeError);
      });

      It("should return target.[[Get]](P, Receiver) if trap is not set", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(p["attr"], Is.EqualTo(1));
        That(p["foo"], Is.Undefined);
      });

      It("should return target.[[Get]](P, Receiver) if trap is undefined", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = Undefined }));
        That(p["attr"], Is.EqualTo(1));
        That(p["foo"], Is.Undefined);
      });

      It("should pass to target's [[Get]] correct receiver if trap is missing", () => {
        EcmaValue target = CreateObject(("attr", get: () => This, set: null));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { get = Null }));
        That(p["attr"], Is.EqualTo(p));

        EcmaValue q = Object.Invoke("create", Proxy.Construct(target, Object.Construct()));
        That(q["attr"], Is.EqualTo(q));
      });
    }

    [Test]
    public void GetOwnPropertyDescriptor() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _prop = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          getOwnPropertyDescriptor = RuntimeFunction.Create((t, prop) => {
            _handler = This;
            _target = t;
            _prop = prop;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("getOwnPropertyDescriptor", p, "attr");

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", CreateObject(new { attr = 1 }), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("getOwnPropertyDescriptor", p["proxy"], "attr"), Throws.TypeError);
      });

      It("should throws a TypeError exception if trap result is undefined and target is not extensible", () => {
        EcmaValue target = CreateObject(new { foo = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Undefined) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "foo"), Throws.TypeError);
      });

      It("should throws a TypeError exception if trap result is undefined and target property descriptor is not configurable", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "foo", CreateObject(new {
          configurable = false,
          writable = false,
          value = 1
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Undefined) }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "foo"), Throws.TypeError);
      });

      It("should return undefined if trap result is undefined and target property descriptor is undefined", () => {
        EcmaValue target = Object.Construct();
        EcmaValue trapped = default;
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Void(trapped = true)) }));
        That(Object.Invoke("getOwnPropertyDescriptor", p, "foo"), Is.Undefined);
        That(trapped, Is.EqualTo(true));
      });

      It("should return undefined if trap result is undefined and target is extensible and the target property descriptor is configurable", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Undefined) }));
        That(Object.Invoke("getOwnPropertyDescriptor", p, "attr"), Is.Undefined);
      });

      It("should throws a TypeError exception when trap result is neither Object nor undefined", () => {
        EcmaValue target = CreateObject(new {
          number = 1,
          symbol = new Symbol(),
          @string = "",
          boolean = true
        });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create((t, prop) => t[prop]) }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "number"), Throws.TypeError);
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "symbol"), Throws.TypeError);
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "string"), Throws.TypeError);
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "boolean"), Throws.TypeError);
      });

      It("should throw a TypeError exception when trap result is neither Object nor undefined (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Null) }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "foo"), Throws.TypeError);
      });

      It("should throw a TypeError exception if trap result and target property descriptors are not compatible", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => Object.Invoke("getOwnPropertyDescriptor", CreateObject(new { bar = 1 }), "bar")) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "bar"), Throws.TypeError);
      });

      It("should throw a TypeError exception if trap result is not configurable but target property descriptor is configurable", () => {
        EcmaValue target = CreateObject(new { bar = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => CreateObject(new { configurable = false, enumerable = true, value = 1 })) }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "bar"), Throws.TypeError);
      });

      It("should throw a TypeError exception if trap result is not configurable but target property descriptor is undefined", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create(() => CreateObject(new { configurable = false, enumerable = true, value = 1 })) }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "bar"), Throws.TypeError);
      });

      It("should return descriptor from trap result if it has the same value as the target property descriptor", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getOwnPropertyDescriptor = RuntimeFunction.Create((t, prop) => Object.Invoke("getOwnPropertyDescriptor", t, prop)) }));

        Object.Invoke("defineProperty", target, "bar", CreateObject(new {
          configurable = true,
          enumerable = true,
          value = 1
        }));
        EcmaValue result = Object.Invoke("getOwnPropertyDescriptor", p, "bar");
        That(result["configurable"], Is.EqualTo(true));
        That(result["enumerable"], Is.EqualTo(true));
        That(result["writable"], Is.EqualTo(false));
        That(result["value"], Is.EqualTo(1));

        Object.Invoke("defineProperty", target, "foo", CreateObject(new {
          configurable = false,
          enumerable = true,
          value = 1
        }));
        result = Object.Invoke("getOwnPropertyDescriptor", p, "foo");
        That(result["configurable"], Is.EqualTo(false));
        That(result["enumerable"], Is.EqualTo(true));
        That(result["writable"], Is.EqualTo(false));
        That(result["value"], Is.EqualTo(1));
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getOwnPropertyDescriptor = ThrowTest262Exception }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "attr"), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getOwnPropertyDescriptor = Object.Construct() }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "attr"), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { getOwnPropertyDescriptor = Object.Construct() }));
        That(() => Object.Invoke("getOwnPropertyDescriptor", p, "attr"), Throws.TypeError);
      });

      It("should return target.[[GetOwnProperty]](P) if trap is not set", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        EcmaValue result = Object.Invoke("getOwnPropertyDescriptor", p, "attr");
        That(result["configurable"], Is.EqualTo(true));
        That(result["enumerable"], Is.EqualTo(true));
        That(result["writable"], Is.EqualTo(true));
        That(result["value"], Is.EqualTo(1));
      });
    }

    [Test]
    public void GetPrototypeOf() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          getPrototypeOf = RuntimeFunction.Create((t) => {
            _handler = This;
            _target = t;
            return Object.Construct();
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("getPrototypeOf", p);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
      });

      It("should return trap result if it's an Object and target is extensible", () => {
        EcmaValue proto = CreateObject(new { foo = 1 });
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => proto) }));
        That(Object.Invoke("getPrototypeOf", p), Is.EqualTo(proto));
      });

      It("should work with instanceof operator", () => {
        EcmaValue CustomClass = RuntimeFunction.Create(_ => _);
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => CustomClass["prototype"]) }));
        That(p.InstanceOf(CustomClass));
      });

      It("should throw a TypeError if the target is not extensible and the trap result is not the same as the target.[[GetPrototypeOf]] result", () => {
        EcmaValue target = CreateObject(new { foo = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => Object.Construct()) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);
      });

      It("should return trap result is target is not extensible, but trap result has the same value as target.[[GetPrototypeOf]] result", () => {
        EcmaValue target = Object.Invoke("create", Array.Prototype);
        EcmaValue p = Proxy.Construct(target, CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => Array.Prototype) }));
        Object.Invoke("preventExtensions", target);
        That(Object.Invoke("getPrototypeOf", p), Is.EqualTo(Array.Prototype));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("getPrototypeOf", p["proxy"]), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = ThrowTest262Exception }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = Object.Construct() }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = Object.Construct() }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);
      });

      It("should return target.[[GetPrototypeOf]]() if trap is undefined", () => {
        EcmaValue target = Object.Invoke("create", Array.Prototype);
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(Object.Invoke("getPrototypeOf", p), Is.EqualTo(Array.Prototype));
      });

      It("should throw a TypeError exception if trap result is neither Object nor null", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => false) }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => 0) }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => "") }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => new Symbol()) }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = RuntimeFunction.Create(() => Undefined) }));
        That(() => Object.Invoke("getPrototypeOf", p), Throws.TypeError);
      });
    }

    [Test]
    public void Has_() {
      It("should be triggered by a `in` check", () => {
        EcmaValue _handler = default, _target = default, _prop = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          has = RuntimeFunction.Create((t, prop) => {
            _handler = This;
            _target = t;
            _prop = prop;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        ((EcmaValue)"attr").In(p);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));

        Void(_handler = default, _target = default, _prop = default);
        ((EcmaValue)"attr").In(Object.Invoke("create", p));

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));
      });

      It("must not report a property as non-existent, if it exists as an own property of the target object and the target object is not extensible", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { has = RuntimeFunction.Create(() => 0) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = true,
          value = 1
        }));
        Object.Invoke("preventExtensions", target);
        That(() => ((EcmaValue)"attr").In(p), Throws.TypeError);
      });

      It("must not reported a property as non-existent, if it exists as a non-configurable own property of the target object", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { has = RuntimeFunction.Create(() => 0) }));
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          value = 1
        }));
        That(() => ((EcmaValue)"attr").In(p), Throws.TypeError);
      });

      It("should return boolean trap result", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { has = RuntimeFunction.Create(() => false) }));
        That(((EcmaValue)"attr").In(p), Is.EqualTo(false));

        p = Proxy.Construct(target, CreateObject(new { has = RuntimeFunction.Create(() => 1) }));
        That(((EcmaValue)"attr").In(p), Is.EqualTo(true));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { has = RuntimeFunction.Create(() => 1) }));
        That(((EcmaValue)"attr").In(p), Is.EqualTo(true));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => ((EcmaValue)"attr").In(p["proxy"]), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { has = ThrowTest262Exception }));
        That(() => ((EcmaValue)"attr").In(p), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { has = Object.Construct() }));
        That(() => ((EcmaValue)"attr").In(p), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { has = Object.Construct() }));
        That(() => ((EcmaValue)"attr").In(p), Throws.TypeError);
      });

      It("should Return target.[[HasProperty]](P) if trap is undefined", () => {
        EcmaValue target = Object.Invoke("create", Array.Prototype);
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(((EcmaValue)"attr").In(p), Is.EqualTo(false));
        That(((EcmaValue)"length").In(p), Is.EqualTo(true));
      });
    }

    [Test]
    public void IsExtensible() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default;
        EcmaValue target = Object.Construct();
        EcmaValue handler = CreateObject(new {
          isExtensible = RuntimeFunction.Create((t) => {
            _handler = This;
            _target = t;
            return Object.Invoke("isExtensible", t);
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("isExtensible", p);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("isExtensible", p["proxy"]), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { isExtensible = ThrowTest262Exception }));
        That(() => Object.Invoke("isExtensible", p), Throws.Test262);
      });

      It("should return boolean result", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { isExtensible = RuntimeFunction.Create(t => Object.Invoke("isExtensible", t) ? 1 : 0) }));
        That(Object.Invoke("isExtensible", p), Is.EqualTo(true));

        Object.Invoke("preventExtensions", target);
        That(Object.Invoke("isExtensible", p), Is.EqualTo(false));
      });

      It("should throw a TypeError exception if boolean trap result is not the same as target.[[IsExtensible]]() result", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { isExtensible = RuntimeFunction.Create(() => false) }));
        That(() => Object.Invoke("isExtensible", p), Throws.TypeError);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { isExtensible = Object.Construct() }));
        That(() => Object.Invoke("isExtensible", p), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { isExtensible = Object.Construct() }));
        That(() => Object.Invoke("isExtensible", p), Throws.TypeError);
      });

      It("should return target.[[IsExtensible]]() if trap is undefined", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(Object.Invoke("isExtensible", p), Is.EqualTo(true));

        Object.Invoke("preventExtensions", target);
        That(Object.Invoke("isExtensible", p), Is.EqualTo(false));
      });
    }

    [Test]
    public void OwnKeys() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default;
        EcmaValue target = CreateObject(new { foo = 1, bar = 2 });
        EcmaValue handler = CreateObject(new {
          ownKeys = RuntimeFunction.Create((t) => {
            _handler = This;
            _target = t;
            return Object.Invoke("getOwnPropertyNames", t);
          })
        });

        EcmaValue p = Proxy.Construct(target, handler);
        That(Object.Invoke("getOwnPropertyNames", p), Is.EquivalentTo(new[] { "foo", "bar" }));
        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));

        Void(_handler = default, _target = default);
        That(Object.Invoke("keys", p), Is.EquivalentTo(new[] { "foo", "bar" }));
        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));

        EcmaValue a = new Symbol();
        EcmaValue b = new Symbol();
        Void(_handler = default, _target = default);
        target = Object.Construct();
        target[a] = 1;
        target[b] = 2;
        handler = CreateObject(new {
          ownKeys = RuntimeFunction.Create((t) => {
            _handler = This;
            _target = t;
            return Object.Invoke("getOwnPropertySymbols", t);
          })
        });

        p = Proxy.Construct(target, handler);
        That(Object.Invoke("getOwnPropertySymbols", p), Is.EquivalentTo(new[] { a, b }));
        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
      });

      It("should return the non-falsy trap result if target doesn't contain any non-configurable keys and is extensible", () => {
        EcmaValue target = CreateObject(new { attr = 42 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo", "bar")) }));
        That(Object.Invoke("getOwnPropertyNames", p), Is.EquivalentTo(new[] { "foo", "bar" }));
      });

      It("should return the non-falsy trap result if it contains all of target's non-configurable keys and target is extensible", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "foo", CreateObject(new {
          configurable = false,
          enumerable = true,
          value = true
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo", "bar")) }));
        That(Object.Invoke("getOwnPropertyNames", p), Is.EquivalentTo(new[] { "foo", "bar" }));
      });

      It("should throw a TypeError exception if the result list does not contain all the keys of the own properties of the target object and target is not extensible", () => {
        EcmaValue target = CreateObject(new { foo = 1, bar = 2 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo")) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        target = CreateObject(new { foo = 1 });
        p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo", "bar")) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        target = CreateObject(new { foo = 1, bar = 2 });
        p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo", "bar")) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("keys", p), Throws.Nothing);
      });

      It("should throw a TypeError exception if the result list does not contain the keys of all non-configurable own properties of the target object", () => {
        EcmaValue target = CreateObject(new { foo = 1 });
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          enumerable = true,
          value = true
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("foo")) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should throw a TypeError exception if return is not a list object", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => Undefined) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should throw a TypeError exception if the returned list have entries whose type does not match", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(EcmaArray.Of())) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(true)) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(Null)) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(1)) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(Object.Construct())) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(Undefined)) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should throw a TypeError exception if the returned list contain any duplicate entries", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of("a", "a")) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);

        EcmaValue s = new Symbol();
        p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = RuntimeFunction.Create(() => EcmaArray.Of(s, s)) }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("keys", p["proxy"]), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = ThrowTest262Exception }));
        That(() => Object.Invoke("keys", p), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { ownKeys = Object.Construct() }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { ownKeys = Object.Construct() }));
        That(() => Object.Invoke("keys", p), Throws.TypeError);
      });

      It("should return target.[[OwnPropertyKeys]]() if trap is undefined", () => {
        EcmaValue target = CreateObject(new { foo = 1, bar = 2 });
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        That(Object.Invoke("keys", p), Is.EquivalentTo(new[] { "foo", "bar" }));
      });
    }

    [Test]
    public void PreventExtensions() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default;
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue handler = CreateObject(new {
          preventExtensions = RuntimeFunction.Create((t) => {
            _handler = This;
            _target = t;
            return Object.Invoke("preventExtensions", t);
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("preventExtensions", p);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
      });

      It("should return false if boolean trap result is false", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { preventExtensions = RuntimeFunction.Create(() => 0) }));
        That(Reflect.Invoke("preventExtensions", p), Is.EqualTo(false));

        Object.Invoke("preventExtensions", target);
        That(Reflect.Invoke("preventExtensions", p), Is.EqualTo(false));
      });

      It("should throw a TypeError exception if boolean trap result is true and target is extensible", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { preventExtensions = RuntimeFunction.Create(() => true) }));
        That(() => Object.Invoke("preventExtensions", p), Throws.TypeError);
      });

      It("should return boolean trap result if its true and target is not extensible", () => {
        EcmaValue target = Object.Construct();
        EcmaValue p = Proxy.Construct(target, CreateObject(new { preventExtensions = RuntimeFunction.Create(() => 1) }));
        Object.Invoke("preventExtensions", target);
        That(Reflect.Invoke("preventExtensions", p), Is.EqualTo(true));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("preventExtensions", p["proxy"]), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { preventExtensions = ThrowTest262Exception }));
        That(() => Object.Invoke("preventExtensions", p), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { preventExtensions = Object.Construct() }));
        That(() => Object.Invoke("preventExtensions", p), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { preventExtensions = Object.Construct() }));
        That(() => Object.Invoke("preventExtensions", p), Throws.TypeError);
      });

      It("should return target.[[PreventExtensions]]() if trap is undefined", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), Object.Construct());
        That(Reflect.Invoke("preventExtensions", p), Is.EqualTo(true));
      });
    }

    [Test]
    public void Set() {
      It("should return boolean result", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { set = RuntimeFunction.Create(() => false) }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(false));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { set = RuntimeFunction.Create(() => Null) }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(false));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { set = RuntimeFunction.Create(() => 0) }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(false));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { set = RuntimeFunction.Create(() => "") }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(false));

        p = Proxy.Construct(Object.Construct(), CreateObject(new { set = RuntimeFunction.Create(() => Undefined) }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(false));
      });

      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _prop = default, _value = default, _receiver = default;
        EcmaValue target = Object.Construct();
        EcmaValue handler = CreateObject(new {
          set = RuntimeFunction.Create((t, prop, value, receiver) => {
            _handler = This;
            _target = t;
            _prop = prop;
            _value = value;
            _receiver = receiver;
            return t[prop] = value;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        p["attr"] = "foo";

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_prop, Is.EqualTo("attr"));
        That(_value, Is.EqualTo("foo"));
        That(_receiver, Is.EqualTo(p));
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => p["proxy"].ToObject()["attr"] = 1, Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { set = ThrowTest262Exception }));
        That(() => p["attr"] = "bar", Throws.Test262);
      });

      It("should returns true if trap returns true and target property accessor is configurable and set is undefined", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = true,
          set = Undefined
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(Reflect.Invoke("set", p, "attr", "bar"), Is.EqualTo(true));
      });

      It("should returns true if trap returns true and target property accessor is not configurable and set is not undefined", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          set = RuntimeFunction.Create(_ => _)
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(Reflect.Invoke("set", p, "attr", 1), Is.EqualTo(true));
      });

      It("should returns true if trap returns true and target property is not configurable but writable", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          writable = true,
          value = "foo"
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(Reflect.Invoke("set", p, "attr", 1), Is.EqualTo(true));
      });

      It("should returns true if trap returns true and target property is configurable but not writable", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = true,
          writable = false,
          value = "foo"
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(Reflect.Invoke("set", p, "attr", "foo"), Is.EqualTo(true));
      });

      It("should throw a TypeError when target property is an accessor not configurable and and set is undefined", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          set = Undefined
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(() => Reflect.Invoke("set", p, "attr", "bar"), Throws.TypeError);
      });

      It("should throw a TypeError when target property is not configurable neither writable and its value is not strictly equal to V", () => {
        EcmaValue target = Object.Construct();
        Object.Invoke("defineProperty", target, "attr", CreateObject(new {
          configurable = false,
          writable = false,
          value = "foo"
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = RuntimeFunction.Create(() => true) }));
        That(() => p["attr"] = "bar", Throws.TypeError);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { set = Object.Construct() }));
        That(() => p["attr"] = "bar", Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { set = Object.Construct() }));
        That(() => p["attr"] = "bar", Throws.TypeError);
      });

      It("should return target.[[Set]](P, V, Receiver) if trap is not set", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, Object.Construct());
        p["attr"] = 2;
        That(target["attr"], Is.EqualTo(2));
      });

      It("should return target.[[Set]](P, V, Receiver) if trap is undefined", () => {
        EcmaValue target = CreateObject(new { attr = 1 });
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = Undefined }));
        p["attr"] = 2;
        That(target["attr"], Is.EqualTo(2));
      });

      It("should pass to target's [[Set]] correct receiver if trap is missing", () => {
        EcmaValue context = default;
        EcmaValue target = CreateObject(("attr", get: null, set: _ => context = This));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { set = Null }));
        p["attr"] = 1;
        That(context, Is.EqualTo(p));

        p = Object.Invoke("create", Proxy.Construct(target, Object.Construct()));
        p["attr"] = 3;
        That(context, Is.EqualTo(p));
      });
    }

    [Test]
    public void SetPrototypeOf() {
      It("should pass correct parameters to the trap method", () => {
        EcmaValue _handler = default, _target = default, _value = default;
        EcmaValue target = Object.Construct();
        EcmaValue proto = CreateObject(new { foo = 1 });
        EcmaValue handler = CreateObject(new {
          setPrototypeOf = RuntimeFunction.Create((t, value) => {
            _handler = This;
            _target = t;
            _value = value;
            Object.Invoke("setPrototypeOf", t, value);
            return true;
          })
        });
        EcmaValue p = Proxy.Construct(target, handler);
        Object.Invoke("setPrototypeOf", p, proto);

        That(_handler, Is.EqualTo(handler));
        That(_target, Is.EqualTo(target));
        That(_value, Is.EqualTo(proto));
      });

      It("should call target.[[GetPrototypeOf]] after trap result as false and not extensible target", () => {
        EcmaValue calls = EcmaArray.Of();
        EcmaValue proto = Object.Construct();
        EcmaValue target = Proxy.Construct(Object.Invoke("create", proto), CreateObject(new {
          isExtensible = Intercept(() => false, "target.[[IsExtensible]]"),
          getPrototypeOf = Intercept(() => proto, "target.[[GetPrototypeOf]]")
        }));

        // Proxy must report same extensiblitity as target
        Object.Invoke("preventExtensions", target);

        EcmaValue proxy = Proxy.Construct(target, CreateObject(new {
          setPrototypeOf = Intercept(() => true, "proxy.[[setPrototypeOf]]")
        }));
        That(Reflect.Invoke("setPrototypeOf", proxy, proto), Is.EqualTo(true));
        That(Logs, Is.EquivalentTo(new[] { "proxy.[[setPrototypeOf]]", "target.[[IsExtensible]]", "target.[[GetPrototypeOf]]" }));
      });

      It("should throw a TypeError exception if boolean trap result is true, target is not extensible, and the given parameter is not the same object as the target prototype", () => {
        EcmaValue target = Object.Construct();
        EcmaValue proxy = Proxy.Construct(target, CreateObject(new { setPrototypeOf = RuntimeFunction.Create(() => true) }));
        Object.Invoke("preventExtensions", target);
        That(() => Reflect.Invoke("setPrototypeOf", proxy, Object.Construct()), Throws.TypeError, "target prototype is different");

        EcmaValue proto = Object.Construct();
        target = Object.Invoke("setPrototypeOf", Object.Construct(), proto);
        proxy = Proxy.Construct(target, CreateObject(new {
          setPrototypeOf = RuntimeFunction.Create(() => {
            Object.Invoke("setPrototypeOf", target, Object.Construct());
            Object.Invoke("preventExtensions", target);
            return true;
          })
        }));
        That(() => Reflect.Invoke("setPrototypeOf", proxy, proto), Throws.TypeError, "target prototype is changed inside trap handler");
      });

      It("should only allow trap result to be true for non-extensible targets if the given prototype is the same as target's prototype", () => {
        EcmaValue proto = Object.Construct();
        EcmaValue target = Object.Invoke("setPrototypeOf", Object.Construct(), proto);
        Object.Invoke("preventExtensions", target);

        EcmaValue proxy = Proxy.Construct(target, CreateObject(new { setPrototypeOf = RuntimeFunction.Create(() => true) }));
        That(Reflect.Invoke("setPrototypeOf", proxy, proto), Is.EqualTo(true), "prototype arg is the same in target");

        EcmaValue outro = Object.Construct();
        proxy = Proxy.Construct(outro, CreateObject(new {
          setPrototypeOf = RuntimeFunction.Create((t, p) => {
            Object.Invoke("setPrototypeOf", t, p);
            Object.Invoke("preventExtensions", t);
            return true;
          })
        }));
        That(Reflect.Invoke("setPrototypeOf", proxy, proto), Is.EqualTo(true), "prototype is set to target inside handler trap");
        That(Object.Invoke("getPrototypeOf", outro), Is.EqualTo(proto), "target has the custom set prototype");
      });

      It("should throw a TypeError exception if handler is null", () => {
        EcmaValue p = Proxy.Invoke("revocable", Object.Construct(), Object.Construct());
        p.Invoke("revoke");
        That(() => Object.Invoke("setPrototypeOf", p["proxy"], Object.Construct()), Throws.TypeError);
      });

      It("should return abrupt completion", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { setPrototypeOf = ThrowTest262Exception }));
        That(() => Object.Invoke("setPrototypeOf", p, Object.Construct()), Throws.Test262);
      });

      It("should return abrupt completion from IsExtensible(target)", () => {
        EcmaValue target = Proxy.Construct(Object.Construct(), CreateObject(new { isExtensible = ThrowTest262Exception }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { setPrototypeOf = RuntimeFunction.Create(() => true) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("setPrototypeOf", p, Object.Construct()), Throws.Test262);
      });

      It("should return abrupt completion from target.[[GetPrototypeOf]]()", () => {
        EcmaValue target = Proxy.Construct(Object.Construct(), CreateObject(new { getPrototypeOf = ThrowTest262Exception }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { setPrototypeOf = RuntimeFunction.Create(() => true) }));
        Object.Invoke("preventExtensions", target);
        That(() => Object.Invoke("setPrototypeOf", p, Object.Construct()), Throws.Test262);
      });

      It("should throw if trap is not callable", () => {
        EcmaValue p = Proxy.Construct(Object.Construct(), CreateObject(new { setPrototypeOf = Object.Construct() }));
        That(() => Object.Invoke("setPrototypeOf", p, Object.Construct()), Throws.TypeError);
      });

      It("should throw if trap is not callable (honoring the Realm of the current execution context)", () => {
        EcmaValue OProxy = new RuntimeRealm().GetRuntimeObject(WellKnownObject.ProxyConstructor);
        EcmaValue p = OProxy.Construct(Object.Construct(), CreateObject(new { setPrototypeOf = Object.Construct() }));
        That(() => Object.Invoke("setPrototypeOf", p, Object.Construct()), Throws.TypeError);
      });

      It("should return false if trap method return false, without checking target.[[IsExtensible]]", () => {
        EcmaValue called = 0;
        EcmaValue target = Proxy.Construct(Object.Construct(), CreateObject(new {
          isExtensible = RuntimeFunction.Create(() => {
            called += 1;
          })
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new { setPrototypeOf = RuntimeFunction.Create((t, v) => v["attr"]) }));

        EcmaValue result;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = false }));
        That(result, Is.EqualTo(false), "false");
        That(called, Is.EqualTo(0), "false - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = "" }));
        That(result, Is.EqualTo(false), "the empty string");
        That(called, Is.EqualTo(0), "the empty string - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = 0 }));
        That(result, Is.EqualTo(false), "0");
        That(called, Is.EqualTo(0), "0 - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = -0d }));
        That(result, Is.EqualTo(false), "-0");
        That(called, Is.EqualTo(0), "-0 - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = Null }));
        That(result, Is.EqualTo(false), "null");
        That(called, Is.EqualTo(0), "null - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = Undefined }));
        That(result, Is.EqualTo(false), "undefined");
        That(called, Is.EqualTo(0), "undefined - isExtensible is not called");

        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = NaN }));
        That(result, Is.EqualTo(false), "NaN");
        That(called, Is.EqualTo(0), "NaN - isExtensible is not called");
      });

      It("should return true if trap method return true and target is extensible", () => {
        EcmaValue called = 0;
        EcmaValue target = Proxy.Construct(Object.Construct(), CreateObject(new {
          isExtensible = RuntimeFunction.Create(() => {
            called += 1;
            return true;
          }),
          getPrototypeOf = ThrowTest262Exception
        }));
        EcmaValue p = Proxy.Construct(target, CreateObject(new {
          setPrototypeOf = RuntimeFunction.Create((t, v) => v["attr"])
        }));

        EcmaValue result;
        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = true }));
        That(result, Is.EqualTo(true), "true");
        That(called, Is.EqualTo(1), "true - isExtensible is called");

        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = "false" }));
        That(result, Is.EqualTo(true), "string");
        That(called, Is.EqualTo(1), "string - isExtensible is called");

        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = 42 }));
        That(result, Is.EqualTo(true), "42");
        That(called, Is.EqualTo(1), "number - isExtensible is called");

        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = p }));
        That(result, Is.EqualTo(true), "p");
        That(called, Is.EqualTo(1), "object - isExtensible is called");

        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = EcmaArray.Of() }));
        That(result, Is.EqualTo(true), "[]");
        That(called, Is.EqualTo(1), "[] - isExtensible is called");

        called = 0;
        result = Reflect.Invoke("setPrototypeOf", p, CreateObject(new { attr = new Symbol() }));
        That(result, Is.EqualTo(true), "symbol");
        That(called, Is.EqualTo(1), "symbol - isExtensible is called");
      });

      It("should return target.[[SetPrototypeOf]] (V) if trap is undefined or null", () => {
        EcmaValue proxy, value, called = 0;
        EcmaValue target = Proxy.Construct(Object.Construct(), CreateObject(new {
          setPrototypeOf = RuntimeFunction.Create((t, v) => {
            called += 1;
            value = v;
            return true;
          })
        }));
        EcmaValue proto = Object.Construct();

        proxy = Proxy.Construct(target, Object.Construct());
        called = 0;
        value = false;
        Object.Invoke("setPrototypeOf", proxy, proto);
        That(called, Is.EqualTo(1), "undefined, target.[[SetPrototypeOf]] is called");
        That(value, Is.EqualTo(proto), "undefined, called with V");

        proxy = Proxy.Construct(target, CreateObject(new { setPrototypeOf = Null }));
        called = 0;
        value = false;
        Object.Invoke("setPrototypeOf", proxy, proto);
        That(called, Is.EqualTo(1), "null, target.[[SetPrototypeOf]] is called");
        That(value, Is.EqualTo(proto), "null, called with V");
      });
    }
  }
}
