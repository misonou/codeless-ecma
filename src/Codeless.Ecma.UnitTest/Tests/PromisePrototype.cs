using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Linq;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class PromisePrototype : TestBase {
    RuntimeFunction Promise => Global.Promise;

    [Test]
    public void Properties() {
      That(Promise, Has.OwnProperty("prototype", Promise.Prototype, EcmaPropertyAttributes.None));
      That(Promise.Prototype, Has.OwnProperty("constructor", Promise, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));
      That(Promise.Prototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Promise", EcmaPropertyAttributes.Configurable));
      That(Promise.Prototype.GetPrototypeOf(), Is.EqualTo(Object.Prototype));
      That(Object.Prototype.Get("toString").Call(Promise.Prototype), Is.EqualTo("[object Promise]"));

      That(() => Promise.Prototype["then"].Call(Promise.Prototype, Noop, Noop), Throws.TypeError, "Promise.prototype does not have a [[PromiseState]] internal slot");
    }

    [Test, RuntimeFunctionInjection]
    public void Catch(RuntimeFunction @catch) {
      IsUnconstructableFunctionWLength(@catch, "catch", 1);
      IsAbruptedFromToObject(@catch);
      That(Promise.Prototype, Has.OwnProperty("catch", @catch, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should invoke then method", () => {
        EcmaValue target = new EcmaObject();
        EcmaValue returnValue = new EcmaObject();
        EcmaValue thisValue = Null;
        EcmaValue args = Null;
        target["then"] = Intercept((a, b) => {
          thisValue = This;
          args = Arguments;
          return returnValue;
        });

        Case((target, 1, 2, 3), returnValue, "Returns the result of the invocation of `then`");
        That(Logs.Count, Is.EqualTo(1), "Invokes `then` method exactly once");
        That(thisValue, Is.EqualTo(target), "Invokes `then` method with the instance as the `this` value");
        That(args, Is.EquivalentTo(new[] { Undefined, 1 }));

        using (TempProperty(Boolean.Prototype, "then", Intercept(_ => _))) {
          Logs.Clear();
          @catch.Call(true);
          That(Logs.Count, Is.EqualTo(1));
        }
        using (TempProperty(Number.Prototype, "then", Intercept(_ => _))) {
          Logs.Clear();
          @catch.Call(34);
          That(Logs.Count, Is.EqualTo(1));
        }
        using (TempProperty(String.Prototype, "then", Intercept(_ => _))) {
          Logs.Clear();
          @catch.Call("");
          That(Logs.Count, Is.EqualTo(1));
        }
        using (TempProperty(Global.Symbol.Prototype, "then", Intercept(_ => _))) {
          Logs.Clear();
          @catch.Call(new Symbol());
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("is equivalent to then(undefined, arg)", () => {
        EcmaValue obj = new EcmaObject();
        Promise.Invoke("resolve", obj).Invoke("catch", Intercept(UnexpectedReject)).Invoke("then", Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();

        Logs.Clear();
        Promise.Invoke("reject", obj).Invoke("then", Intercept(UnexpectedFulfill)).Invoke("catch", Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();
      });

      It("should throw TypeError when then is not callable", () => {
        Case(CreateObject(new { }), Throws.TypeError);
        Case(CreateObject(new { then = Null }), Throws.TypeError);
        Case(CreateObject(new { then = 1 }), Throws.TypeError);
        Case(CreateObject(new { then = "" }), Throws.TypeError);
        Case(CreateObject(new { then = true }), Throws.TypeError);
        Case(CreateObject(new { then = new Symbol() }), Throws.TypeError);
        Case(CreateObject(new { then = new EcmaObject() }), Throws.TypeError);
      });

      It("should return abrupt from getting then method", () => {
        Case(CreateObject(("then", get: ThrowTest262Exception, set: null)), Throws.Test262);
      });

      It("should return abrupt from then method", () => {
        Case(CreateObject(new { then = ThrowTest262Exception }), Throws.Test262);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Finally(RuntimeFunction @finally) {
      IsUnconstructableFunctionWLength(@finally, "finally", 1);
      IsAbruptedFromToObject(@finally);
      That(Promise.Prototype, Has.OwnProperty("finally", @finally, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should invoke then method", () => {
        EcmaValue target = new EcmaObject();
        EcmaValue returnValue = new EcmaObject();
        EcmaValue thisValue = Null;
        EcmaValue args = Null;
        target["then"] = Intercept((a, b) => {
          thisValue = This;
          args = Arguments;
          return returnValue;
        });

        Case((target, 1, 2, 3), returnValue, "Returns the result of the invocation of `then`");
        That(Logs.Count, Is.EqualTo(1), "Invokes `then` method exactly once");
        That(thisValue, Is.EqualTo(target), "Invokes `then` method with the instance as the `this` value");
        That(args, Is.EquivalentTo(new[] { 1, 1 }));

        Logs.Clear();
        EcmaValue originalFinallyHandler = Noop;
        Case((target, originalFinallyHandler, 2, 3), returnValue, "Returns the result of the invocation of `then`");
        That(Logs.Count, Is.EqualTo(1), "Invokes `then` method exactly once");
        That(thisValue, Is.EqualTo(target), "Invokes `then` method with the instance as the `this` value");
        That(args["length"], Is.EqualTo(2));
        That(args[0], Is.Not.EqualTo(originalFinallyHandler), "Invokes `then` method with a different fulfillment handler");
        That(args[1], Is.Not.EqualTo(originalFinallyHandler), "Invokes `then` method with a different rejection handler");

        IsUnconstructableFunctionWLength(args[0], null, 1);
        IsUnconstructableFunctionWLength(args[1], null, 1);
      });

      It("should throw TypeError when then is not callable", () => {
        Case(CreateObject(new { }), Throws.TypeError);
        Case(CreateObject(new { then = Null }), Throws.TypeError);
        Case(CreateObject(new { then = 1 }), Throws.TypeError);
        Case(CreateObject(new { then = "" }), Throws.TypeError);
        Case(CreateObject(new { then = true }), Throws.TypeError);
        Case(CreateObject(new { then = new Symbol() }), Throws.TypeError);
        Case(CreateObject(new { then = new EcmaObject() }), Throws.TypeError);
      });

      It("should return abrupt from getting then method", () => {
        Case(CreateObject(("then", get: ThrowTest262Exception, set: null)), Throws.Test262);
      });

      It("should return abrupt from then method", () => {
        Case(CreateObject(new { then = ThrowTest262Exception }), Throws.Test262);
      });

      It("can override the rejection reason", () => {
        EcmaValue original = new EcmaObject();
        EcmaValue thrown = new EcmaObject();
        EcmaValue p = Promise.Invoke("reject", original);
        p.Invoke("finally", Intercept(() => {
          That(Arguments["length"], Is.EqualTo(0), "onFinally receives zero args");
          Throw(thrown);
        }, "1")).Invoke("catch", Intercept(reason => {
          That(reason, Is.EqualTo(thrown), "onFinally can override the rejection reason by throwing");
        }, "2"));
        VerifyPromiseSettled(new[] { "1", "2" });
      });

      It("can convert resolved promise to rejected by returning another promise", () => {
        EcmaValue yesValue = new EcmaObject();
        EcmaValue yes = Promise.Invoke("resolve", yesValue);
        yes["then"] = Intercept(() => Promise.Prototype["then"].Call(This, Arguments.ToArray()), "1");

        EcmaValue noReason = new EcmaObject();
        EcmaValue no = Promise.Invoke("reject", noReason);
        no["then"] = Intercept(() => Promise.Prototype["then"].Call(This, Arguments.ToArray()), "4");

        yes.Invoke("then", Intercept(x => {
          That(x, Is.EqualTo(yesValue));
          return x;
        }, "2")).Invoke("finally", Intercept(() => {
          return no;
        }, "3")).Invoke("catch", Intercept(e => {
          That(e, Is.EqualTo(noReason));
        }, "5"));
        VerifyPromiseSettled(new[] { "1", "2", "3", "4", "5" });
      });

      It("cannot override the resolution value", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue p = Promise.Invoke("resolve", obj);
        p.Invoke("finally", Intercept(() => {
          That(Arguments["length"], Is.EqualTo(0), "onFinally receives zero args");
          return new EcmaObject();
        }, "1")).Invoke("then", Intercept(v => {
          That(v, Is.EqualTo(obj), "onFinally can not override the resolution value");
        }, "2"));
        VerifyPromiseSettled(new[] { "1", "2" });
      });

      It("cannot convert rejected promise to a fulfillment", () => {
        EcmaValue original = new EcmaObject();
        EcmaValue replacement = new EcmaObject();
        EcmaValue p = Promise.Invoke("reject", original);
        p.Invoke("finally", Intercept(() => {
          That(Arguments["length"], Is.EqualTo(0), "onFinally receives zero args");
          return replacement;
        }, "1")).Invoke("catch", Intercept(reason => {
          That(reason, Is.EqualTo(original), "onFinally can override the rejection reason by throwing");
        }, "2"));
        VerifyPromiseSettled(new[] { "1", "2" });
      });

      It("cannot convert rejected promise to a fulfillment by returning another promise", () => {
        EcmaValue noReason = new EcmaObject();
        EcmaValue no = Promise.Invoke("reject", noReason);
        no["then"] = Intercept(() => Promise.Prototype["then"].Call(This, Arguments.ToArray()), "1");

        EcmaValue yesValue = new EcmaObject();
        EcmaValue yes = Promise.Invoke("resolve", yesValue);
        yes["then"] = Intercept(() => Promise.Prototype["then"].Call(This, Arguments.ToArray()), "4");

        no.Invoke("catch", Intercept(e => {
          That(e, Is.EqualTo(noReason));
          Throw(e);
        }, "2")).Invoke("finally", Intercept(() => {
          return yes;
        }, "3")).Invoke("catch", Intercept(e => {
          That(e, Is.EqualTo(noReason));
        }, "5"));
        VerifyPromiseSettled(new[] { "1", "2", "3", "4", "5" });
      });

      It("calls the SpeciesConstructor and creates the right amount of promises", () => {
        EcmaValue FooPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };

        FooPromise.Construct(FunctionLiteral(resolve => resolve.Call())).Invoke("finally", Noop).Invoke("then", FunctionLiteral(() => That(Logs.Count, Is.EqualTo(6))), UnexpectedReject);
        VerifyPromiseSettled();
      });

      It("creates the proper number of subclassed promises on rejected", () => {
        EcmaValue FooPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };

        FooPromise.Invoke("reject").Invoke("finally", Noop).Invoke("then", UnexpectedFulfill).Invoke("catch", FunctionLiteral(() => That(Logs.Count, Is.EqualTo(7))));
        VerifyPromiseSettled();
      });

      It("creates the proper number of subclassed promises on resolved", () => {
        EcmaValue FooPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };

        FooPromise.Invoke("resolve").Invoke("finally", Noop).Invoke("then", FunctionLiteral(() => That(Logs.Count, Is.EqualTo(6))));
        VerifyPromiseSettled();
      });

      It("calls the SpeciesConstructor", () => {
        EcmaValue MyPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };
        Object.Invoke("defineProperty", MyPromise, Symbol.Species, CreateObject(new { get = FunctionLiteral(() => Promise) }));

        EcmaValue p = Promise.Invoke("resolve").Invoke("finally", FunctionLiteral(() => MyPromise.Invoke("resolve")));
        That(p.InstanceOf(Promise), Is.True);
        That(p.InstanceOf(MyPromise), Is.False);
      });

      It("calls the SpeciesConstructor on rejected", () => {
        EcmaValue MyPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };
        Object.Invoke("defineProperty", MyPromise, Symbol.Species, CreateObject(new { get = FunctionLiteral(() => Promise) }));

        EcmaValue p = Promise.Invoke("reject").Invoke("finally", FunctionLiteral(() => MyPromise.Invoke("reject")));
        That(p.InstanceOf(Promise), Is.True);
        That(p.InstanceOf(MyPromise), Is.False);
      });

      It("calls the SpeciesConstructor on resolved", () => {
        EcmaValue MyPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((resolve, reject) => {
            return Super.Construct(resolve, reject);
          }))
        };
        Object.Invoke("defineProperty", MyPromise, Symbol.Species, CreateObject(new { get = FunctionLiteral(() => Promise) }));

        EcmaValue p = Promise.Invoke("resolve").Invoke("finally", FunctionLiteral(() => MyPromise.Invoke("resolve")));
        That(p.InstanceOf(Promise), Is.True);
        That(p.InstanceOf(MyPromise), Is.False);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Then(RuntimeFunction then) {
      IsUnconstructableFunctionWLength(then, "then", 2);
      That(Promise.Prototype, Has.OwnProperty("then", then, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw TypeError if this is not Promise object", () => {
        EcmaValue p = new EcmaObject();
        Object.Invoke("defineProperty", p, "constructor", CreateObject(new { get = ThrowTest262Exception }));
        Case(p, Throws.TypeError);
      });

      It("should expect a constructor conforming to Promise as this", () => {
        Case((3, Noop, Noop), Throws.TypeError);
        Case((Function.Construct().Construct(), Noop, Noop), Throws.TypeError);
      });

      It("should accept undefined arguments", () => {
        EcmaValue obj = new EcmaObject();
        Promise.Invoke("resolve", obj).Invoke("then", Undefined, Undefined).Invoke("then", Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();
        Promise.Invoke("reject", obj).Invoke("then", Undefined, Undefined).Invoke("then", Intercept(UnexpectedFulfill), Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();
      });

      It("should treat non-callable arguments as undefined", () => {
        EcmaValue obj = new EcmaObject();
        Promise.Invoke("resolve", obj).Invoke("then", 3, 5).Invoke("then", Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();
        Promise.Invoke("reject", obj).Invoke("then", 3, 5).Invoke("then", Intercept(UnexpectedFulfill), Intercept(v => That(v, Is.EqualTo(obj))));
        VerifyPromiseSettled();
      });

      It("should immediately queue handler if rejected", () => {
        EcmaValue pReject = default;
        EcmaValue p = Promise.Construct(FunctionLiteral((resolve, reject) => pReject = reject));

        Logs.Add("1");
        pReject.Call();
        p.Invoke("then", Intercept(UnexpectedFulfill), Intercept(() => Undefined, "3"));

        Promise.Invoke("resolve").Invoke("then", Intercept(() => {
          // enqueue another then-handler
          p.Invoke("then", Intercept(UnexpectedFulfill), Intercept(() => Undefined, "5"));
        }, "4"));

        Logs.Add("2");
        VerifyPromiseSettled(new[] { "1", "2", "3", "4", "5" });
      });

      It("does not check for cyclic resolutions in promise reaction jobs", () => {
        EcmaValue createBadPromise = false;
        EcmaValue obj = new EcmaObject();
        EcmaValue P = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral((executor) => {
            if (createBadPromise) {
              executor.Call(Undefined, FunctionLiteral(v => That(v, Is.EqualTo(obj))), Noop);
              return obj;
            }
            Super.Construct(executor);
            return Undefined;
          })
        };
        EcmaValue p = P.Invoke("resolve", obj);
        createBadPromise = true;
        p.Invoke("then");
        Case(p, obj);
      });

      It("should use built-in Promise when the `this` value has no `constructor` property", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        p1["constructor"] = Undefined;
        That(p1.Invoke("then"), Is.InstanceOf(Promise));
      });

      It("should throw TypeError when constructor property is non-object", () => {
        EcmaValue p = Promise.Construct(Noop);
        p["constructor"] = Null;
        Case(p, Throws.TypeError);
      });

      It("should return abrupt from getting constructor", () => {
        EcmaValue p = Promise.Invoke("resolve", "foo");
        Object.Invoke("defineProperty", p, "constructor", CreateObject(new { get = ThrowTest262Exception }));
        Case(p, Throws.Test262);
      });

      It("should return abrupt from custom constructor", () => {
        EcmaValue BadCtor = ThrowTest262Exception;
        using (TempProperty(Promise, Symbol.Species, BadCtor)) {
          EcmaValue p = Promise.Construct(FunctionLiteral(resolve => resolve.Call()));
          Case(p, Throws.Test262);
        }
      });

      It("should access constructor property exactly once", () => {
        EcmaValue p = Promise.Invoke("resolve", "foo");
        Object.Invoke("defineProperty", p, "constructor", CreateObject(new { get = Intercept(() => Promise) }));
        p.Invoke("then");
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should use species constructor", () => {
        EcmaValue thisValue = default, callCount = 0, argLength = default, getCapabilitiesExecutor = default;
        EcmaValue executor = Noop;
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue SpeciesConstructor = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral((a) => {
            Super.Construct(a);
            callCount += 1;
            thisValue = This;
            getCapabilitiesExecutor = a;
            argLength = Arguments["length"];
          })
        };

        EcmaValue constructor = Noop;
        p1["constructor"] = constructor;
        constructor[Symbol.Species] = SpeciesConstructor;

        EcmaValue p2 = p1.Invoke("then");
        That(callCount, Is.EqualTo(1));
        That(thisValue, Is.InstanceOf(SpeciesConstructor));
        That(argLength, Is.EqualTo(1));
        That(getCapabilitiesExecutor, Is.TypeOf("function"));
        That(getCapabilitiesExecutor["length"], Is.EqualTo(2));
        That(p2, Is.InstanceOf(SpeciesConstructor));
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        EcmaValue constructorFunction = default;
        EcmaValue promise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(executor => {
            if (constructorFunction) {
              constructorFunction.Call(Undefined, executor);
              return new EcmaObject();
            }
            return Super.Construct(executor);
          })
        }.ToValue().Construct(Noop);

        EcmaValue checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor not called at all");
        That(checkPoint, Is.EqualTo("a"), "executor not called at all");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined);
            checkPoint += "b";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor called with no arguments");
        That(checkPoint, Is.EqualTo("ab"), "executor called with no arguments");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, Undefined, Undefined);
            checkPoint += "b";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor called with (undefined, undefined)");
        That(checkPoint, Is.EqualTo("ab"), "executor called with (undefined, undefined)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, Undefined, Noop);
            checkPoint += "b";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor called with (undefined, function)");
        That(checkPoint, Is.EqualTo("ab"), "executor called with (undefined, function)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, Noop, Undefined);
            checkPoint += "b";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor called with (function, undefined)");
        That(checkPoint, Is.EqualTo("ab"), "executor called with (function, undefined)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, 123, "invalid value");
            checkPoint += "b";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor called with (Number, String)");
        That(checkPoint, Is.EqualTo("ab"), "executor called with (Number, String)");
      });

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        EcmaValue constructorFunction = default;
        EcmaValue promise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(executor => {
            if (constructorFunction) {
              constructorFunction.Call(Undefined, executor);
              return new EcmaObject();
            }
            return Super.Construct(executor);
          })
        }.ToValue().Construct(Noop);

        EcmaValue checkPoint = "";
        constructorFunction = FunctionLiteral((executor) => {
          checkPoint += "a";
          executor.Call(Undefined);
          checkPoint += "b";
          executor.Call(Undefined, Noop, Noop);
          checkPoint += "c";
        });
        promise.Invoke("then");
        That(checkPoint, Is.EqualTo("abc"), "executor initially called with no arguments");

        checkPoint = "";
        constructorFunction = FunctionLiteral((executor) => {
          checkPoint += "a";
          executor.Call(Undefined, Undefined, Undefined);
          checkPoint += "b";
          executor.Call(Undefined, Noop, Noop);
          checkPoint += "c";
        });
        promise.Invoke("then");
        That(checkPoint, Is.EqualTo("abc"), "executor initially called with (undefined, undefined)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, Undefined, Noop);
            checkPoint += "b";
            executor.Call(Undefined, Noop, Noop);
            checkPoint += "c";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor initially called with (undefined, function)");
        That(checkPoint, Is.EqualTo("ab"), "executor initially called with (undefined, function)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, Noop, Undefined);
            checkPoint += "b";
            executor.Call(Undefined, Noop, Noop);
            checkPoint += "c";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor initially called with (function, undefined)");
        That(checkPoint, Is.EqualTo("ab"), "executor initially called with (function, undefined)");

        checkPoint = "";
        That(() => {
          constructorFunction = FunctionLiteral((executor) => {
            checkPoint += "a";
            executor.Call(Undefined, "invalid value", 123);
            checkPoint += "b";
            executor.Call(Undefined, Noop, Noop);
            checkPoint += "c";
          });
          promise.Invoke("then");
        }, Throws.TypeError, "executor initially called with (String, Number)");
        That(checkPoint, Is.EqualTo("ab"), "executor initially called with (String, Number)");
      });
    }
  }
}
