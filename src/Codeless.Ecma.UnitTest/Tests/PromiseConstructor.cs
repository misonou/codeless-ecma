using Codeless.Ecma.Runtime;
using NUnit.Framework;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class PromiseConstructor : TestBase {
    RuntimeFunction Promise => Global.Promise;

    [Test, RuntimeFunctionInjection]
    public void Constructor(RuntimeFunction ctor) {
      IsConstructorWLength(ctor, "Promise", 1, Promise.Prototype);
      That(GlobalThis, Has.OwnProperty("Promise", ctor, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Configurable));

      It("should derive [[Prototype]] value from realm of newTarget", () => {
        RuntimeRealm realm = new RuntimeRealm();
        EcmaValue fn = realm.GetRuntimeObject(WellKnownObject.FunctionConstructor).Construct();
        fn["prototype"] = Null;
        EcmaValue other = Reflect.Invoke("construct", ctor, EcmaArray.Of(Noop), fn);
        That(Object.Invoke("getPrototypeOf", other), Is.EqualTo(realm.GetRuntimeObject(WellKnownObject.PromisePrototype)));
      });

      It("should throw TypeError when 'this' is not Object", () => {
        That(() => Promise.Call("", Noop), Throws.TypeError);
      });

      It("should throw TypeError when 'this' is promise", () => {
        That(() => Promise.Call(Promise.Construct(Noop), Noop), Throws.TypeError);

        EcmaValue p1 = Promise.Construct(FunctionLiteral(resolve => resolve.Call()));
        p1.Invoke("then",
          Intercept(() => That(() => Promise.Call(p1, Noop), Throws.TypeError)),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();

        EcmaValue p2 = Promise.Construct(FunctionLiteral((_, reject) => reject.Call()));
        p2.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(() => That(() => Promise.Call(p2, Noop), Throws.TypeError)));
        VerifyPromiseSettled();
      });

      It("should throw TypeError when executor is not callable", () => {
        That(() => Promise.Construct(""), Throws.TypeError);
      });

      It("should call executor synchronously", () => {
        EcmaValue args = default;
        Promise.Construct(FunctionLiteral(() => args = Arguments));
        That(args["length"], Is.EqualTo(2));
        That(args[0], Is.TypeOf("function"));
        That(args[1], Is.TypeOf("function"));
      });

      It("should call executor with 'this' equal to global object in sloppy mode, undefined in strict mode", () => {
        Promise.Construct(Intercept((resolve) => {
          That(This, Is.Undefined);
          resolve.Call();
        }));
        VerifyPromiseSettled();
      });

      It("should catch exceptions thrown from executor and turn them into reject", () => {
        EcmaValue errorObject = new EcmaObject();
        EcmaValue p = Promise.Construct(FunctionLiteral(() => {
          Throw(errorObject);
        }));
        p.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(err => That(err, Is.EqualTo(errorObject))));
        VerifyPromiseSettled();
      });

      It("a GetCapabilitiesExecutor function function is an anonymous built-in function, not constructors", () => {
        EcmaValue executorFunction = default;
        EcmaValue NotPromise = FunctionLiteral(executor => {
          executorFunction = executor;
          executor.Call(Undefined, Noop, Noop);
        });
        Promise["resolve"].Call(NotPromise);
        IsUnconstructableFunctionWLength(executorFunction, null, 2);
        That(Object.Invoke("isExtensible", executorFunction), Is.EqualTo(true));
      });

      It("resolve function is an anonymous built-in function, not constructors", () => {
        EcmaValue resolve = default;
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        IsUnconstructableFunctionWLength(resolve, null, 1);
        That(Object.Invoke("isExtensible", resolve), Is.EqualTo(true));
      });

      It("should resolve with a thenable object value from within the executor function", () => {
        EcmaValue value = new EcmaObject();
        EcmaValue returnValue = Null;
        EcmaValue thenable = Promise.Construct(FunctionLiteral(resolve => CallAndVerifyReturn(resolve, value)));
        EcmaValue promise = Promise.Construct(FunctionLiteral(resolve => CallAndVerifyReturn(resolve, thenable)));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(value))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should resolve with a thenable object value after execution of the executor function", () => {
        EcmaValue value = new EcmaObject();
        EcmaValue resolve = default;
        EcmaValue thenable = Promise.Construct(FunctionLiteral(_resolve => _resolve.Call(Undefined, value)));
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(value))),
          Intercept(UnexpectedReject));
        CallAndVerifyReturn(resolve, thenable);
        VerifyPromiseSettled();
      });

      It("should throw TypeError when resolving with a reference to the promise itself", () => {
        EcmaValue resolve = default;
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.InstanceOf(TypeError), "The promise should be rejected with a TypeError instance")));
        CallAndVerifyReturn(resolve, promise);
        VerifyPromiseSettled();
      });

      It("should ignore resolution after immediate invocation of the provided reject function", () => {
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((resolve, reject) => {
          CallAndVerifyReturn(reject, thenable);
          CallAndVerifyReturn(resolve);
        }));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(_ => _));
        VerifyPromiseSettled();
      });

      It("should ignore resolution after deferred invocation of the provided reject function", () => {
        EcmaValue resolve = default, reject = default;
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_resolve, _reject) => {
          resolve = _resolve;
          reject = _reject;
        }));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(_ => _));
        CallAndVerifyReturn(reject, thenable);
        CallAndVerifyReturn(resolve);
        VerifyPromiseSettled();
      });

      It("should resolve with a non-object value from within the executor function", () => {
        EcmaValue promise = Promise.Construct(FunctionLiteral((resolve, reject) => {
          CallAndVerifyReturn(resolve, 45);
        }));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(45))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should resolve with a non-object value after invocation of the executor function", () => {
        EcmaValue resolve = default;
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(45))),
          Intercept(UnexpectedReject));
        CallAndVerifyReturn(resolve, 45);
        VerifyPromiseSettled();
      });

      It("should resolve with a non-thenable object value from within the executor function", () => {
        EcmaValue nonThenable = CreateObject(new { then = Null });
        EcmaValue promise = Promise.Construct(FunctionLiteral((resolve, reject) => {
          CallAndVerifyReturn(resolve, nonThenable);
        }));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(nonThenable))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should resolve with a non-thenable object value after invocation of the executor function", () => {
        EcmaValue resolve = default;
        EcmaValue nonThenable = CreateObject(new { then = Null });
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(nonThenable))),
          Intercept(UnexpectedReject));
        CallAndVerifyReturn(resolve, nonThenable);
        VerifyPromiseSettled();
      });

      It("should reject with an object with a poisoned `then` property from within the executor function", () => {
        EcmaValue value = new EcmaObject();
        EcmaValue poisonedThen = CreateObject(("then", get: () => Throw(value), set: null));
        EcmaValue promise = Promise.Construct(FunctionLiteral((resolve, reject) => {
          CallAndVerifyReturn(resolve, poisonedThen);
        }));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(value))));
        VerifyPromiseSettled();
      });

      It("should reject with an object with a poisoned `then` property after invocation of the executor function", () => {
        EcmaValue resolve = default;
        EcmaValue value = new EcmaObject();
        EcmaValue poisonedThen = CreateObject(("then", get: () => Throw(value), set: null));
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(value))));
        CallAndVerifyReturn(resolve, poisonedThen);
        VerifyPromiseSettled();
      });

      It("should resolve with a resolved Promise instance whose `then` method has been overridden from within the executor function", () => {
        EcmaValue value = new EcmaObject();
        EcmaValue lateCallCount = 0;
        EcmaValue thenable = Promise.Construct(FunctionLiteral(resolve => resolve.Call()));
        thenable["then"] = FunctionLiteral(resolve => resolve.Call(Undefined, value));
        EcmaValue promise = Promise.Construct(FunctionLiteral(resolve => CallAndVerifyReturn(resolve, thenable)));
        thenable["then"] = FunctionLiteral(() => lateCallCount += 1);
        promise.Invoke("then",
          Intercept(v => {
            That(v, Is.EqualTo(value));
            That(lateCallCount, Is.EqualTo(0), "The `then` method should be executed synchronously");
          }),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should resolve with a resolved Promise instance whose `then` method has been overridden after execution of the executor function", () => {
        EcmaValue resolve = default;
        EcmaValue value = new EcmaObject();
        EcmaValue thenable = Promise.Construct(FunctionLiteral(_resolve => _resolve.Call()));
        EcmaValue promise = Promise.Construct(FunctionLiteral(_resolve => resolve = _resolve));
        thenable["then"] = FunctionLiteral(_resolve => _resolve.Call(Undefined, value));
        promise.Invoke("then",
          Intercept(v => That(v, Is.EqualTo(value))),
          Intercept(UnexpectedReject));
        CallAndVerifyReturn(resolve, thenable);
        VerifyPromiseSettled();
      });

      It("reject function is an anonymous built-in function, not constructors", () => {
        EcmaValue reject = default;
        EcmaValue promise = Promise.Construct(FunctionLiteral((_, _reject) => reject = _reject));
        IsUnconstructableFunctionWLength(reject, null, 1);
        That(Object.Invoke("isExtensible", reject), Is.EqualTo(true));
      });

      It("should ignore rejections through immediate invocation of the provided resolving function", () => {
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((resolve, reject) => {
          resolve.Call();
          CallAndVerifyReturn(reject, thenable);
        }));
        promise.Invoke("then",
          Intercept(_ => _),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should ignore rejections through deferred invocation of the provided resolving function", () => {
        EcmaValue resolve = default;
        EcmaValue reject = default;
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_resolve, _reject) => {
          resolve = _resolve;
          reject = _reject;
        }));
        promise.Invoke("then",
          Intercept(_ => _),
          Intercept(UnexpectedReject));
        resolve.Call();
        CallAndVerifyReturn(reject, thenable);
        VerifyPromiseSettled();
      });

      It("should reject through an abrupt completion captured in a queued job", () => {
        EcmaValue thenable = Promise.Invoke("resolve");
        EcmaValue promise = Promise.Construct(FunctionLiteral(() => Throw(thenable)));
        promise.Invoke("then", FunctionLiteral(UnexpectedFulfill)).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(thenable))));
        VerifyPromiseSettled();
      });

      It("should reject through deferred invocation of the provided resolving function", () => {
        EcmaValue reject = default;
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_, _reject) => reject = _reject));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(thenable))));
        CallAndVerifyReturn(reject, thenable);
        VerifyPromiseSettled();
      });

      It("should reject through deferred invocation of the provided resolving function, captured in a queued job", () => {
        EcmaValue reject = default;
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_, _reject) => reject = _reject));
        promise.Invoke("then", FunctionLiteral(UnexpectedFulfill)).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(thenable))));
        CallAndVerifyReturn(reject, thenable);
        VerifyPromiseSettled();
      });

      It("should reject through immediate invocation of the provided resolving function", () => {
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_, reject) => CallAndVerifyReturn(reject, thenable)));
        promise.Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(thenable))));
        VerifyPromiseSettled();
      });

      It("Rejecting through immediate invocation of the provided resolving function, captured in a queued job", () => {
        EcmaValue thenable = Promise.Construct(Noop);
        EcmaValue promise = Promise.Construct(FunctionLiteral((_, reject) => CallAndVerifyReturn(reject, thenable)));
        promise.Invoke("then", FunctionLiteral(UnexpectedFulfill)).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(v => That(v, Is.EqualTo(thenable))));
        VerifyPromiseSettled();
      });
    }

    [Test, RuntimeFunctionInjection]
    public void All(RuntimeFunction all) {
      IsUnconstructableFunctionWLength(all, "all", 1);
      That(Promise, Has.OwnProperty("all", all, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a Promise", () => {
        Case((_, EcmaArray.Of()), Is.InstanceOf(Promise));
      });

      It("should resolve with a new Array", () => {
        EcmaValue arr = EcmaArray.Of();
        Promise.Invoke("all", EcmaArray.Of()).Invoke("then", Intercept(v => {
          That(v, Is.InstanceOf(Array));
          That(v["length"], Is.EqualTo(0));
          That(v, Is.Not.EqualTo(arr));
        }));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if argument is not iterable", () => {
        Promise.Invoke("all", Null).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", Undefined).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", 3).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", false).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", new Symbol()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", Error.Construct()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", "").Invoke("then",
          Intercept(v => That(v, Is.EquivalentTo(new EcmaValue[0]))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if GetIterator is not callable", () => {
        Promise.Invoke("all", CreateObject((Symbol.Iterator, Undefined))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", CreateObject((Symbol.Iterator, Null))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", CreateObject((Symbol.Iterator, false))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", CreateObject((Symbol.Iterator, 1))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", CreateObject((Symbol.Iterator, ""))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("all", CreateObject((Symbol.Iterator, new Symbol()))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from abrupt completion from iterator", () => {
        EcmaValue iterThrows = CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        EcmaValue error = Error.Construct();
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => CreateObject(new { next = FunctionLiteral(() => Throw(error)) }))));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if Symbol.iterator returns is not an Object", () => {
        EcmaValue iterThrows;
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Undefined)));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Null)));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => false)));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => 1)));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => "")));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => new Symbol())));
        Promise.Invoke("all", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from posioned next from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => Throw(error)),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("all", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned value from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = CreateObject(new { done = false });
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("all", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned done from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = new EcmaObject();
        Object.Invoke("defineProperty", poisonedVal, "done", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = ThrowTest262Exception }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("all", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        Logs.Clear();
        all.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call();
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called with no arguments");

        Logs.Clear();
        all.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call(Undefined, Undefined, Undefined);
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called  with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (undefined, function)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (function, undefined)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, "invalid value", 123);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (String, Number)");
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a" }), "executor not called at all");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call();
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with no arguments");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, function)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (function, undefined)");

        Logs.Clear();
        That(() => {
          all.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, 123, "invalid value");
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (Number, String)");
      });

      It("should invoke this constructor", () => {
        EcmaValue executor = default;
        EcmaValue SubPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((a) => {
            Super.Construct(a);
            executor = a;
          }))
        };

        EcmaValue p = all.Call(SubPromise, EcmaArray.Of());
        That(p["constructor"], Is.EqualTo(SubPromise));
        That(p, Is.InstanceOf(SubPromise));
        That(executor, Is.TypeOf("function"));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should throw a TypeError if this is not an Object", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((86, EcmaArray.Of()), Throws.TypeError);
        Case(("string", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol(), EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if this is not a constructor", () => {
        Case(all, Throws.TypeError);
      });

      It("should throw a TypeError for bad this", () => {
        Case((Noop, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return abrupt from constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("should invoke `then` with an extensible, non-constructor and anonymous Promise.all Resolve Element functions", () => {
        EcmaValue Constructor = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue resolveElementFunction = default;
        EcmaValue thenable = CreateObject(new { then = Intercept(f => resolveElementFunction = f) });
        all.Call(Constructor, EcmaArray.Of(thenable));
        VerifyPromiseSettled();
        That(resolveElementFunction.IsExtensible);
        IsUnconstructableFunctionWLength(resolveElementFunction, null, 1);
      });

      It("does not retrieve `Symbol.species` property of the `this` value", () => {
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, Noop, Noop);
        });
        Object.Invoke("defineProperty", C, Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
        Case((C, EcmaArray.Of()), Throws.Nothing);
      });

      It("should call each promise with a new resolve function", () => {
        EcmaValue resolveFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, resolveFunction, UnexpectedReject);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(p1OnFulfilled = onFulfilled, Is.Not.EqualTo(resolveFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => {
            That(onFulfilled, Is.Not.EqualTo(resolveFunction));
            That(onFulfilled, Is.Not.EqualTo(p1OnFulfilled));
          })
        });
        all.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should call each promise with the same reject function", () => {
        EcmaValue rejectFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, UnexpectedFulfill, rejectFunction);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onRejected, Is.EqualTo(rejectFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onRejected, Is.EqualTo(rejectFunction)))
        });
        all.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should accept an array with settled promise", () => {
        EcmaValue p1 = Promise.Invoke("resolve", 3);
        Promise.Invoke("all", EcmaArray.Of(p1)).Invoke("then",
          Intercept(result => That(result, Is.EquivalentTo(new[] { 3 }))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should reject when a promise in its array rejects", () => {
        EcmaValue rejectP1 = default;
        EcmaValue p1 = Promise.Construct(FunctionLiteral((_, reject) => rejectP1 = reject));
        EcmaValue p2 = Promise.Invoke("resolve", 2);
        Promise.Invoke("all", EcmaArray.Of(p1, p2)).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.EqualTo(1))));
        rejectP1.Call(Undefined, 1);
        VerifyPromiseSettled();

        EcmaValue rejectP2 = default;
        p1 = Promise.Invoke("resolve", 1);
        p2 = Promise.Construct(FunctionLiteral((_, reject) => rejectP2 = reject));
        Promise.Invoke("all", EcmaArray.Of(p1, p2)).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.EqualTo(2))));
        rejectP2.Call(Undefined, 2);
        VerifyPromiseSettled();
      });

      It("cannot change result value of resolved Promise.all element", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values, Is.EquivalentTo(new[] { "expectedValue-p1", "expectedValue-p2" }));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "expectedValue-p1");
            onFulfilled.Call(Undefined, "unexpectedValue-p1");
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "expectedValue-p2");
            onFulfilled.Call(Undefined, "unexpectedValue-p2");
          })
        });

        all.Call(Constructor, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled(count: 1);
      });

      It("cannot change result value of resolved Promise.all element after returned", () => {
        EcmaValue valuesArray = default;
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            valuesArray = values;
            That(EcmaArray.IsArray(values), "values is array");
            That(values, Is.EquivalentTo(new[] { "expectedValue" }));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);
        EcmaValue p1OnFulfilled = default;

        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
            onFulfilled.Call(Undefined, "expectedValue");
          })
        });
        all.Call(Constructor, EcmaArray.Of(p1));
        That(Logs.Count, Is.EqualTo(1), "callCount after call to all()");
        That(valuesArray, Is.EquivalentTo(new[] { "expectedValue" }));

        p1OnFulfilled.Call(Undefined, "unexpectedValue");
        That(Logs.Count, Is.EqualTo(1), "callCount after call to onFulfilled()");
        That(valuesArray, Is.EquivalentTo(new[] { "expectedValue" }));
      });

      It("cannot tamper remainingElementsCount when Promise.all resolve element function is called twice in a row", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values, Is.EquivalentTo(new[] { "p1-fulfill", "p2-fulfill", "p3-fulfill" }));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p2-fulfill");
            onFulfilled.Call(Undefined, "p2-fulfill-unexpected");
          })
        });
        EcmaValue p3 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p3-fulfill");
          })
        });
        all.Call(Constructor, EcmaArray.Of(p1, p2, p3));
        p1OnFulfilled.Call(Undefined, "p1-fulfill");
        VerifyPromiseSettled(count: 1);
      });

      It("cannot tamper remainingElementsCount when two Promise.all resolve element functions are called in succession", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values, Is.EquivalentTo(new[] { "p1-fulfill", "p2-fulfill", "p3-fulfill" }));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled.Call(Undefined, "p1-fulfill");
            onFulfilled.Call(Undefined, "p2-fulfill");
          })
        });
        EcmaValue p3 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p3-fulfill");
          })
        });
        all.Call(Constructor, EcmaArray.Of(p1, p2, p3));
        VerifyPromiseSettled(count: 1);
      });

      It("does not close iterator when the resolve capability returns an abrupt", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = true })),
            @return = Intercept(() => new EcmaObject())
          });
        })));

        EcmaValue P = FunctionLiteral(executor => {
          return Promise.Construct(FunctionLiteral((_, reject) => {
            executor.Call(Undefined, ThrowTest262Exception, reject);
          }));
        });
        all.Call(P, iter);
        That(Logs.Count, Is.EqualTo(0));
      });

      It("should not invoke array setters", () => {
        using (TempProperty(Array.Prototype, 0, new EcmaPropertyDescriptor(Undefined, ThrowTest262Exception))) {
          all.Call(_, EcmaArray.Of(42)).Invoke("then", Intercept(Noop));
          VerifyPromiseSettled();
        }
      });

      It("should invoke the constructor's `resolve` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue resolve = Promise["resolve"];
        EcmaValue current = p1;
        EcmaValue next = p2;
        EcmaValue afterNext = p3;

        EcmaValue customResolve = Intercept(nextValue => {
          That(nextValue, Is.EqualTo(current), "`resolve` invoked with next iterated value");
          That(Arguments["length"], Is.EqualTo(1), "`resolve` invoked with a single argument");
          That(This, Is.EqualTo(Promise), "`this` value is the constructor");
          current = next;
          next = afterNext;
          afterNext = Null;
          return resolve.Call(Promise, nextValue);
        });
        using (TempProperty(Promise, "resolve", customResolve)) {
          all.Call(_, EcmaArray.Of(p1, p2, p3));
          That(Logs.Count, Is.EqualTo(3));
        }
      });

      It("should use the value returned by the constructor's `resolve` method", () => {
        EcmaValue originalCallCount = 0;
        EcmaValue newCallCount = 0;
        EcmaValue originalThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            originalCallCount += 1;
          })
        });
        EcmaValue newThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            newCallCount += 1;
          })
        });

        EcmaValue P = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        P["resolve"] = FunctionLiteral(() => newThenable);
        all.Call(P, EcmaArray.Of(originalThenable));
        That(originalCallCount, Is.EqualTo(0));
        That(newCallCount, Is.EqualTo(1));
      });

      It("should return a rejected promise when error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          all.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should return a rejected promise when `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          all.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should close iterator after error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          all.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should close iterator after `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          all.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should invoke the instance's `then` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue currentThis = p1;
        EcmaValue nextThis = p2;
        EcmaValue afterNextThis = p3;
        p1["then"] = p2["then"] = p3["then"] = Intercept((a, b) => {
          IsUnconstructableFunctionWLength(a, null, 1);
          IsUnconstructableFunctionWLength(b, null, 1);
          That(Arguments["length"], Is.EqualTo(2));
          That(This, Is.EqualTo(currentThis));
          currentThis = nextThis;
          nextThis = afterNextThis;
          afterNextThis = Null;
        });

        all.Call(_, EcmaArray.Of(p1, p2, p3));
        That(Logs.Count, Is.EqualTo(3));
      });

      It("should return a rejected promise when error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        all.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise when the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        promise["then"] = FunctionLiteral(() => Throw(error));
        all.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should close iterator after error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        all.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should close iterator after the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        promise["then"] = FunctionLiteral(() => Throw(error));
        all.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void AllSettled(RuntimeFunction allSettled) {
      IsUnconstructableFunctionWLength(allSettled, "allSettled", 1);
      That(Promise, Has.OwnProperty("allSettled", allSettled, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a Promise", () => {
        Case((_, EcmaArray.Of()), Is.InstanceOf(Promise));
      });

      It("should resolve with a new Array", () => {
        EcmaValue arr = EcmaArray.Of();
        Promise.Invoke("allSettled", EcmaArray.Of()).Invoke("then", Intercept(v => {
          That(v, Is.InstanceOf(Array));
          That(v["length"], Is.EqualTo(0));
          That(v, Is.Not.EqualTo(arr));
        }));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if argument is not iterable", () => {
        Promise.Invoke("allSettled", Null).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", Undefined).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", 3).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", false).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", new Symbol()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", Error.Construct()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", "").Invoke("then",
          Intercept(v => That(v, Is.EquivalentTo(new EcmaValue[0]))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if GetIterator is not callable", () => {
        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, Undefined))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, Null))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, false))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, 1))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, ""))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("allSettled", CreateObject((Symbol.Iterator, new Symbol()))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from abrupt completion from iterator", () => {
        EcmaValue iterThrows = CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        EcmaValue error = Error.Construct();
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => CreateObject(new { next = FunctionLiteral(() => Throw(error)) }))));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if Symbol.iterator returns is not an Object", () => {
        EcmaValue iterThrows;
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Undefined)));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Null)));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => false)));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => 1)));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => "")));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => new Symbol())));
        Promise.Invoke("allSettled", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from posioned next from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => Throw(error)),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("allSettled", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned value from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = CreateObject(new { done = false });
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("allSettled", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned done from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = new EcmaObject();
        Object.Invoke("defineProperty", poisonedVal, "done", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = ThrowTest262Exception }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("allSettled", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        Logs.Clear();
        allSettled.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call();
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called with no arguments");

        Logs.Clear();
        allSettled.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call(Undefined, Undefined, Undefined);
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called  with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (undefined, function)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (function, undefined)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, "invalid value", 123);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (String, Number)");
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a" }), "executor not called at all");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call();
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with no arguments");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, function)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (function, undefined)");

        Logs.Clear();
        That(() => {
          allSettled.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, 123, "invalid value");
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (Number, String)");
      });

      It("should invoke this constructor", () => {
        EcmaValue executor = default;
        EcmaValue SubPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((a) => {
            Super.Construct(a);
            executor = a;
          }))
        };

        EcmaValue p = allSettled.Call(SubPromise, EcmaArray.Of());
        That(p["constructor"], Is.EqualTo(SubPromise));
        That(p, Is.InstanceOf(SubPromise));
        That(executor, Is.TypeOf("function"));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should throw a TypeError if this is not an Object", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((86, EcmaArray.Of()), Throws.TypeError);
        Case(("string", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol(), EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if this is not a constructor", () => {
        Case(allSettled, Throws.TypeError);
      });

      It("should throw a TypeError for bad this", () => {
        Case((Noop, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return abrupt from constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("should invoke `then` with an extensible, non-constructor and anonymous Promise.allSettled Resolve Element functions", () => {
        EcmaValue Constructor = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue resolveElementFunction = default;
        EcmaValue thenable = CreateObject(new { then = Intercept(f => resolveElementFunction = f) });
        allSettled.Call(Constructor, EcmaArray.Of(thenable));
        VerifyPromiseSettled();
        That(resolveElementFunction.IsExtensible);
        IsUnconstructableFunctionWLength(resolveElementFunction, null, 1);
      });

      It("should invoke `then` with an extensible, non-constructor and anonymous Promise.allSettled Reject Element functions", () => {
        EcmaValue Constructor = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue rejectElementFunction = default;
        EcmaValue thenable = CreateObject(new { then = Intercept((_, f) => rejectElementFunction = f) });
        allSettled.Call(Constructor, EcmaArray.Of(thenable));
        VerifyPromiseSettled();
        That(rejectElementFunction.IsExtensible);
        IsUnconstructableFunctionWLength(rejectElementFunction, null, 1);
      });

      It("should retrieve constructor's `resolve` method exactly once", () => {
        EcmaValue p1 = Promise.Invoke("resolve", 1);
        EcmaValue p2 = Promise.Invoke("resolve", 1);
        EcmaValue p3 = Promise.Invoke("reject", 1);
        EcmaValue p4 = Promise.Invoke("resolve", 1);
        EcmaValue resolve = Promise["resolve"];
        EcmaValue getCount = 0;
        EcmaValue callCount = 0;

        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Return(getCount += 1, FunctionLiteral((v) => Return(callCount += 1, resolve.Call(Promise, v))))), Undefined))) {
          allSettled.Call(_, EcmaArray.Of(p1, p2, p3, p4));
          That(getCount, Is.EqualTo(1), "Got `resolve` only once for each iterated value");
          That(callCount, Is.EqualTo(4), "`resolve` invoked once for each iterated value");

          getCount = 0;
          callCount = 0;
          allSettled.Call(_, EcmaArray.Of());
          That(getCount, Is.EqualTo(1), "Got `resolve` only once for each iterated value");
          That(callCount, Is.EqualTo(0), "`resolve` not called for empty iterator");
        }
      });

      It("does not retrieve `Symbol.species` property of the `this` value", () => {
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, Noop, Noop);
        });
        Object.Invoke("defineProperty", C, Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
        Case((C, EcmaArray.Of()), Throws.Nothing);
      });

      It("should call each promise with a new resolve function", () => {
        EcmaValue resolveFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, resolveFunction, UnexpectedReject);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(p1OnFulfilled = onFulfilled, Is.Not.EqualTo(resolveFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => {
            That(onFulfilled, Is.Not.EqualTo(resolveFunction));
            That(onFulfilled, Is.Not.EqualTo(p1OnFulfilled));
          })
        });
        allSettled.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should call each promise with a new reject function", () => {
        EcmaValue rejectFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, UnexpectedFulfill, rejectFunction);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnReject = default;
        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(p1OnReject = onRejected, Is.Not.EqualTo(rejectFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => {
            That(onRejected, Is.Not.EqualTo(rejectFunction));
            That(onRejected, Is.Not.EqualTo(p1OnReject));
          })
        });
        allSettled.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should accept rejected promise", () => {
        EcmaValue p1 = Promise.Invoke("reject", 1);
        allSettled.Call(_, EcmaArray.Of(p1)).Invoke("then", Intercept(values => {
          That(EcmaArray.IsArray(values), "values is array");
          That(values["length"], Is.EqualTo(1));
          That(values[0]["status"], Is.EqualTo("rejected"));
          That(values[0]["reason"], Is.EqualTo(1));
        }), UnexpectedReject);
      });

      It("cannot change result value of resolved Promise.allSettled element", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values["length"], Is.EqualTo(2));
            That(values[0]["status"], Is.EqualTo("fulfilled"));
            That(values[0]["value"], Is.EqualTo("expectedValue-p1"));
            That(values[1]["status"], Is.EqualTo("fulfilled"));
            That(values[1]["value"], Is.EqualTo("expectedValue-p2"));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "expectedValue-p1");
            onFulfilled.Call(Undefined, "unexpectedValue-p1");
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "expectedValue-p2");
            onFulfilled.Call(Undefined, "unexpectedValue-p2");
          })
        });

        allSettled.Call(Constructor, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled(count: 1);
      });

      It("cannot change result value of resolved Promise.allSettled element after returned", () => {
        EcmaValue valuesArray = default;
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            valuesArray = values;
            That(EcmaArray.IsArray(values), "values is array");
            That(values["length"], Is.EqualTo(1));
            That(values[0]["status"], Is.EqualTo("fulfilled"));
            That(values[0]["value"], Is.EqualTo("expectedValue"));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);
        EcmaValue p1OnFulfilled = default;

        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
            onFulfilled.Call(Undefined, "expectedValue");
          })
        });
        allSettled.Call(Constructor, EcmaArray.Of(p1));
        That(Logs.Count, Is.EqualTo(1), "callCount after call to all()");
        That(valuesArray["length"], Is.EqualTo(1));
        That(valuesArray[0]["status"], Is.EqualTo("fulfilled"));
        That(valuesArray[0]["value"], Is.EqualTo("expectedValue"));

        p1OnFulfilled.Call(Undefined, "unexpectedValue");
        That(Logs.Count, Is.EqualTo(1), "callCount after call to onFulfilled()");
        That(valuesArray["length"], Is.EqualTo(1));
        That(valuesArray[0]["status"], Is.EqualTo("fulfilled"));
        That(valuesArray[0]["value"], Is.EqualTo("expectedValue"));
      });

      It("cannot change result status of resolved Promise.allSettled element", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values["length"], Is.EqualTo(2));
            That(values[0]["status"], Is.EqualTo("fulfilled"));
            That(values[0]["value"], Is.EqualTo("expectedValue-p1"));
            That(values[1]["status"], Is.EqualTo("rejected"));
            That(values[1]["reason"], Is.EqualTo("expectedValue-p2"));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "expectedValue-p1");
            onRejected.Call(Undefined, "unexpectedValue-p1");
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onRejected.Call(Undefined, "expectedValue-p2");
            onFulfilled.Call(Undefined, "unexpectedValue-p2");
          })
        });

        allSettled.Call(Constructor, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled(count: 1);
      });

      It("cannot tamper remainingElementsCount when Promise.allSettled resolve element function is called twice in a row", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values["length"], Is.EqualTo(3));
            That(values[0]["status"], Is.EqualTo("fulfilled"));
            That(values[0]["value"], Is.EqualTo("p1-fulfill"));
            That(values[1]["status"], Is.EqualTo("fulfilled"));
            That(values[1]["value"], Is.EqualTo("p2-fulfill"));
            That(values[2]["status"], Is.EqualTo("fulfilled"));
            That(values[2]["value"], Is.EqualTo("p3-fulfill"));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p2-fulfill");
            onFulfilled.Call(Undefined, "p2-fulfill-unexpected");
          })
        });
        EcmaValue p3 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p3-fulfill");
          })
        });
        allSettled.Call(Constructor, EcmaArray.Of(p1, p2, p3));
        p1OnFulfilled.Call(Undefined, "p1-fulfill");
        VerifyPromiseSettled(count: 1);
      });

      It("cannot tamper remainingElementsCount when two Promise.allSettled resolve element functions are called in succession", () => {
        EcmaValue Constructor = FunctionLiteral(executor => {
          EcmaValue resolve = Intercept(values => {
            That(EcmaArray.IsArray(values), "values is array");
            That(values["length"], Is.EqualTo(3));
            That(values[0]["status"], Is.EqualTo("fulfilled"));
            That(values[0]["value"], Is.EqualTo("p1-fulfill"));
            That(values[1]["status"], Is.EqualTo("fulfilled"));
            That(values[1]["value"], Is.EqualTo("p2-fulfill"));
            That(values[2]["status"], Is.EqualTo("fulfilled"));
            That(values[2]["value"], Is.EqualTo("p3-fulfill"));
          });
          executor.Call(Undefined, resolve, UnexpectedReject);
        });
        Constructor["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1OnFulfilled = default;
        EcmaValue p1 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled = onFulfilled;
          })
        });
        EcmaValue p2 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            p1OnFulfilled.Call(Undefined, "p1-fulfill");
            onFulfilled.Call(Undefined, "p2-fulfill");
          })
        });
        EcmaValue p3 = CreateObject(new {
          then = FunctionLiteral((onFulfilled, onRejected) => {
            onFulfilled.Call(Undefined, "p3-fulfill");
          })
        });
        allSettled.Call(Constructor, EcmaArray.Of(p1, p2, p3));
        VerifyPromiseSettled(count: 1);
      });

      It("does not close iterator when the resolve capability returns an abrupt", () => {
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = true })),
            @return = Intercept(() => new EcmaObject())
          });
        })));

        EcmaValue P = FunctionLiteral(executor => {
          return Promise.Construct(FunctionLiteral((_, reject) => {
            executor.Call(Undefined, ThrowTest262Exception, reject);
          }));
        });
        allSettled.Call(P, iter);
        That(Logs.Count, Is.EqualTo(0));
      });

      It("should not invoke array setters", () => {
        using (TempProperty(Array.Prototype, 0, new EcmaPropertyDescriptor(Undefined, ThrowTest262Exception))) {
          allSettled.Call(_, EcmaArray.Of(42)).Invoke("then", Intercept(Noop));
          VerifyPromiseSettled();
        }
      });

      It("should invoke the constructor's `resolve` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue resolve = Promise["resolve"];
        EcmaValue current = p1;
        EcmaValue next = p2;
        EcmaValue afterNext = p3;

        EcmaValue customResolve = Intercept(nextValue => {
          That(nextValue, Is.EqualTo(current), "`resolve` invoked with next iterated value");
          That(Arguments["length"], Is.EqualTo(1), "`resolve` invoked with a single argument");
          That(This, Is.EqualTo(Promise), "`this` value is the constructor");
          current = next;
          next = afterNext;
          afterNext = Null;
          return resolve.Call(Promise, nextValue);
        });
        using (TempProperty(Promise, "resolve", customResolve)) {
          allSettled.Call(_, EcmaArray.Of(p1, p2, p3));
          That(Logs.Count, Is.EqualTo(3));
        }
      });

      It("should use the value returned by the constructor's `resolve` method", () => {
        EcmaValue originalCallCount = 0;
        EcmaValue newCallCount = 0;
        EcmaValue originalThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            originalCallCount += 1;
          })
        });
        EcmaValue newThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            newCallCount += 1;
          })
        });

        EcmaValue P = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        P["resolve"] = FunctionLiteral(() => newThenable);
        allSettled.Call(P, EcmaArray.Of(originalThenable));
        That(originalCallCount, Is.EqualTo(0));
        That(newCallCount, Is.EqualTo(1));
      });

      It("should return a rejected promise when error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          allSettled.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should return a rejected promise when `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          allSettled.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should close iterator after error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          allSettled.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should close iterator after `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          allSettled.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should invoke the instance's `then` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue currentThis = p1;
        EcmaValue nextThis = p2;
        EcmaValue afterNextThis = p3;
        p1["then"] = p2["then"] = p3["then"] = Intercept((a, b) => {
          IsUnconstructableFunctionWLength(a, null, 1);
          IsUnconstructableFunctionWLength(b, null, 1);
          That(Arguments["length"], Is.EqualTo(2));
          That(This, Is.EqualTo(currentThis));
          currentThis = nextThis;
          nextThis = afterNextThis;
          afterNextThis = Null;
        });

        allSettled.Call(_, EcmaArray.Of(p1, p2, p3));
        That(Logs.Count, Is.EqualTo(3));
      });

      It("should return a rejected promise when error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        allSettled.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise when the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        promise["then"] = FunctionLiteral(() => Throw(error));
        allSettled.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should close iterator after error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        allSettled.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should close iterator after the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        promise["then"] = FunctionLiteral(() => Throw(error));
        allSettled.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Race(RuntimeFunction race) {
      IsUnconstructableFunctionWLength(race, "race", 1);
      That(Promise, Has.OwnProperty("race", race, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a Promise", () => {
        Case((_, EcmaArray.Of()), Is.InstanceOf(Promise));
      });

      It("should return a rejected promise if argument is not iterable", () => {
        Promise.Invoke("race", Null).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", Undefined).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", 3).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", false).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", new Symbol()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", Error.Construct()).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", "a").Invoke("then",
          Intercept(v => That(v, Is.EqualTo("a"))),
          Intercept(UnexpectedReject));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if GetIterator is not callable", () => {
        Promise.Invoke("race", CreateObject((Symbol.Iterator, Undefined))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", CreateObject((Symbol.Iterator, Null))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", CreateObject((Symbol.Iterator, false))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", CreateObject((Symbol.Iterator, 1))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", CreateObject((Symbol.Iterator, ""))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();

        Promise.Invoke("race", CreateObject((Symbol.Iterator, new Symbol()))).Invoke("then",
          Intercept(UnexpectedFulfill),
          Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from abrupt completion from iterator", () => {
        EcmaValue iterThrows = CreateObject((Symbol.Iterator, get: ThrowTest262Exception, set: null));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        EcmaValue error = Error.Construct();
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => CreateObject(new { next = FunctionLiteral(() => Throw(error)) }))));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise if Symbol.iterator returns is not an Object", () => {
        EcmaValue iterThrows;
        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Undefined)));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => Null)));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => false)));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => 1)));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => "")));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();

        iterThrows = CreateObject((Symbol.Iterator, FunctionLiteral(() => new Symbol())));
        Promise.Invoke("race", iterThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.InstanceOf(Error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise from posioned next from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => Throw(error)),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("race", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned value from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = CreateObject(new { done = false });
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("race", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should return a rejected promise from posioned done from iterator", () => {
        EcmaValue returnCallCount = 0;
        EcmaValue error = Error.Construct();
        EcmaValue poisonedVal = new EcmaObject();
        Object.Invoke("defineProperty", poisonedVal, "done", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        Object.Invoke("defineProperty", poisonedVal, "value", CreateObject(new { get = ThrowTest262Exception }));

        EcmaValue iterNextValThrows = new EcmaObject();
        iterNextValThrows[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => poisonedVal),
            @return = FunctionLiteral(() => Return(returnCallCount += 1, new EcmaObject()))
          });
        });
        Promise.Invoke("race", iterNextValThrows).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
        That(returnCallCount, Is.EqualTo(0));
      });

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        Logs.Clear();
        race.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call();
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called with no arguments");

        Logs.Clear();
        race.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call(Undefined, Undefined, Undefined);
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }), EcmaArray.Of());
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called  with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (undefined, function)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (function, undefined)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, "invalid value", 123);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (String, Number)");
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a" }), "executor not called at all");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call();
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with no arguments");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, function)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (function, undefined)");

        Logs.Clear();
        That(() => {
          race.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, 123, "invalid value");
            Logs.Add("b");
          }), EcmaArray.Of());
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (Number, String)");
      });

      It("should invoke this constructor", () => {
        EcmaValue executor = default;
        EcmaValue SubPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((a) => {
            Super.Construct(a);
            executor = a;
          }))
        };

        EcmaValue p = race.Call(SubPromise, EcmaArray.Of());
        That(p["constructor"], Is.EqualTo(SubPromise));
        That(p, Is.InstanceOf(SubPromise));
        That(executor, Is.TypeOf("function"));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should throw a TypeError if this is not an Object", () => {
        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((86, EcmaArray.Of()), Throws.TypeError);
        Case(("string", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol(), EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if this is not a constructor", () => {
        Case(race, Throws.TypeError);
      });

      It("should throw a TypeError for bad this", () => {
        Case((Noop, EcmaArray.Of()), Throws.TypeError);
      });

      It("should return abrupt from constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("does not retrieve `Symbol.species` property of the `this` value", () => {
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, Noop, Noop);
        });
        Object.Invoke("defineProperty", C, Symbol.Species, CreateObject(new { get = ThrowTest262Exception }));
        Case((C, EcmaArray.Of()), Throws.Nothing);
      });

      It("should call each promise with the same resolve function", () => {
        EcmaValue resolveFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, resolveFunction, UnexpectedReject);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onFulfilled, Is.EqualTo(resolveFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onFulfilled, Is.EqualTo(resolveFunction)))
        });
        race.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should call each promise with the same reject function", () => {
        EcmaValue rejectFunction = Noop;
        EcmaValue C = FunctionLiteral(executor => {
          executor.Call(Undefined, UnexpectedFulfill, rejectFunction);
        });
        C["resolve"] = FunctionLiteral(v => v);

        EcmaValue p1 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onRejected, Is.EqualTo(rejectFunction)))
        });
        EcmaValue p2 = CreateObject(new {
          then = Intercept((onFulfilled, onRejected) => That(onRejected, Is.EqualTo(rejectFunction)))
        });
        race.Call(C, EcmaArray.Of(p1, p2));
        VerifyPromiseSettled();
      });

      It("should never settle for empty array", () => {
        EcmaValue p = race.Call(_, EcmaArray.Of());
        p.Invoke("then", UnexpectedFulfill, UnexpectedReject).Invoke("then", UnexpectedFulfill, UnexpectedReject);
        Promise.Invoke("resolve").Invoke("then").Invoke("then").Invoke("then");
        RuntimeExecution.ContinueUntilEnd();
      });

      It("should settle immediately when given settled promises", () => {
        EcmaValue p1 = Promise.Invoke("reject", 1);
        EcmaValue p = Promise.Invoke("race", EcmaArray.Of(p1));
        Logs.Add("1");
        p.Invoke("then", Intercept(UnexpectedFulfill), Intercept(Noop, "4"));

        Promise.Invoke("resolve").Invoke("then", Intercept(Noop, "3")).Invoke("then", Intercept(Noop, "5"));
        Logs.Add("2");
        VerifyPromiseSettled(new[] { "1", "2", "3", "4", "5" });
      });

      It("should settle when first settles", () => {
        EcmaValue p1, p2;
        p1 = Promise.Invoke("resolve", 1);
        p2 = Promise.Invoke("resolve", 2);
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(v => That(v, Is.EqualTo(1))), Intercept(UnexpectedReject));
        VerifyPromiseSettled();

        p1 = Promise.Invoke("resolve", 1);
        p2 = Promise.Construct(Noop);
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(v => That(v, Is.EqualTo(1))), Intercept(UnexpectedReject));
        VerifyPromiseSettled();

        p1 = Promise.Construct(Noop);
        p2 = Promise.Invoke("resolve", 2);
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(v => That(v, Is.EqualTo(2))), Intercept(UnexpectedReject));
        VerifyPromiseSettled();

        p1 = Promise.Invoke("reject", 1);
        p2 = Promise.Invoke("resolve", 2);
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(UnexpectedFulfill), Intercept(v => That(v, Is.EqualTo(1))));
        VerifyPromiseSettled();

        EcmaValue resolveP1 = default, rejectP2 = default;
        p1 = Promise.Construct(FunctionLiteral((resolve, reject) => resolveP1 = resolve));
        p2 = Promise.Construct(FunctionLiteral((resolve, reject) => rejectP2 = reject));
        rejectP2.Call(Undefined, 2);
        resolveP1.Call(Undefined, 1);
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(v => That(v, Is.EqualTo(1))), Intercept(UnexpectedReject));
        VerifyPromiseSettled();

        p1 = Promise.Construct(FunctionLiteral((resolve, reject) => resolveP1 = resolve));
        p2 = Promise.Construct(FunctionLiteral((resolve, reject) => rejectP2 = reject));
        Promise.Invoke("race", EcmaArray.Of(p1, p2)).Invoke("then", Intercept(UnexpectedFulfill), Intercept(v => That(v, Is.EqualTo(2))));
        rejectP2.Call(Undefined, 2);
        resolveP1.Call(Undefined, 1);
        VerifyPromiseSettled();
      });

      It("should invoke the constructor's `resolve` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue resolve = Promise["resolve"];
        EcmaValue current = p1;
        EcmaValue next = p2;
        EcmaValue afterNext = p3;

        EcmaValue customResolve = Intercept(nextValue => {
          That(nextValue, Is.EqualTo(current), "`resolve` invoked with next iterated value");
          That(Arguments["length"], Is.EqualTo(1), "`resolve` invoked with a single argument");
          That(This, Is.EqualTo(Promise), "`this` value is the constructor");
          current = next;
          next = afterNext;
          afterNext = Null;
          return resolve.Call(Promise, nextValue);
        });
        using (TempProperty(Promise, "resolve", customResolve)) {
          race.Call(_, EcmaArray.Of(p1, p2, p3));
          That(Logs.Count, Is.EqualTo(3));
        }
      });

      It("should use the value returned by the constructor's `resolve` method", () => {
        EcmaValue originalCallCount = 0;
        EcmaValue newCallCount = 0;
        EcmaValue originalThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            originalCallCount += 1;
          })
        });
        EcmaValue newThenable = CreateObject(new {
          then = FunctionLiteral(() => {
            newCallCount += 1;
          })
        });

        EcmaValue P = FunctionLiteral(executor => executor.Call(Undefined, Noop, Noop));
        P["resolve"] = FunctionLiteral(() => newThenable);
        race.Call(P, EcmaArray.Of(originalThenable));
        That(originalCallCount, Is.EqualTo(0));
        That(newCallCount, Is.EqualTo(1));
      });

      It("should return a rejected promise when error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          race.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should return a rejected promise when `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          race.Call(_, EcmaArray.Of(Promise.Construct(Noop))).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
          VerifyPromiseSettled();
        }
      });

      It("should close iterator after error retrieving the constructor's `resolve` method", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", new EcmaPropertyDescriptor(FunctionLiteral(() => Throw(error)), Undefined))) {
          race.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should close iterator after `resolve` method throws", () => {
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        using (TempProperty(Promise, "resolve", FunctionLiteral(() => Throw(error)))) {
          race.Call(_, iter);
          That(Logs.Count, Is.EqualTo(1));
        }
      });

      It("should invoke the instance's `then` method", () => {
        EcmaValue p1 = Promise.Construct(Noop);
        EcmaValue p2 = Promise.Construct(Noop);
        EcmaValue p3 = Promise.Construct(Noop);
        EcmaValue currentThis = p1;
        EcmaValue nextThis = p2;
        EcmaValue afterNextThis = p3;
        p1["then"] = p2["then"] = p3["then"] = Intercept((a, b) => {
          IsUnconstructableFunctionWLength(a, null, 1);
          IsUnconstructableFunctionWLength(b, null, 1);
          That(Arguments["length"], Is.EqualTo(2));
          That(This, Is.EqualTo(currentThis));
          currentThis = nextThis;
          nextThis = afterNextThis;
          afterNextThis = Null;
        });

        race.Call(_, EcmaArray.Of(p1, p2, p3));
        That(Logs.Count, Is.EqualTo(3));
      });

      It("should return a rejected promise when error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        race.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should return a rejected promise when the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        promise["then"] = FunctionLiteral(() => Throw(error));
        race.Call(_, EcmaArray.Of(promise)).Invoke("then", UnexpectedFulfill, Intercept(e => That(e, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should close iterator after error retrieving the instance's `then` method", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        Object.Invoke("defineProperty", promise, "then", CreateObject(new { get = FunctionLiteral(() => Throw(error)) }));
        race.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should close iterator after the instance's `then` method throws", () => {
        EcmaValue promise = Promise.Construct(Noop);
        EcmaValue error = Error.Construct();
        EcmaValue iter = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => CreateObject(new { value = promise, done = false })),
            @return = Intercept(() => new EcmaObject())
          });
        })));
        promise["then"] = FunctionLiteral(() => Throw(error));
        race.Call(_, iter).Invoke("then", UnexpectedFulfill, FunctionLiteral(e => That(e, Is.EqualTo(error))));
        That(Logs.Count, Is.EqualTo(1));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Reject(RuntimeFunction reject) {
      IsUnconstructableFunctionWLength(reject, "reject", 1);
      That(Promise, Has.OwnProperty("reject", reject, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        Logs.Clear();
        reject.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call();
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }));
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called with no arguments");

        Logs.Clear();
        reject.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call(Undefined, Undefined, Undefined);
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }));
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called  with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (undefined, function)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (function, undefined)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, "invalid value", 123);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (String, Number)");
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a" }), "executor not called at all");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call();
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with no arguments");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Undefined);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, function)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (function, undefined)");

        Logs.Clear();
        That(() => {
          reject.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, 123, "invalid value");
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (Number, String)");
      });

      It("should return abrupt from reject capability", () => {
        Case(FunctionLiteral((executor) => {
          return Promise.Construct(FunctionLiteral(() => {
            executor.Call(Undefined, Noop, ThrowTest262Exception);
          }));
        }), Throws.Test262);
      });

      It("should invoke this constructor", () => {
        EcmaValue executor = default;
        EcmaValue SubPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((a) => {
            Super.Construct(a);
            executor = a;
          }))
        };

        EcmaValue p = reject.Call(SubPromise);
        That(p["constructor"], Is.EqualTo(SubPromise));
        That(p, Is.InstanceOf(SubPromise));
        That(executor, Is.TypeOf("function"));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should throw a TypeError if this is not an Object", () => {
        EcmaValue promise = Promise.Construct(Noop);
        promise["constructor"] = Undefined;
        Case((Undefined, promise), Throws.TypeError);
        promise["constructor"] = Null;
        Case((Null, promise), Throws.TypeError);
        promise["constructor"] = true;
        Case((true, promise), Throws.TypeError);
        promise["constructor"] = 1;
        Case((1, promise), Throws.TypeError);
        promise["constructor"] = "";
        Case(("", promise), Throws.TypeError);

        EcmaValue symbol = new Symbol();
        promise["constructor"] = symbol;
        Case((symbol, promise), Throws.TypeError);

        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((86, EcmaArray.Of()), Throws.TypeError);
        Case(("string", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol(), EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if this is not a constructor", () => {
        Case(reject, Throws.TypeError);
      });

      It("should throw a TypeError for bad this", () => {
        Case((Noop, 4), Throws.TypeError);
      });

      It("should return abrupt from constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("should call reject capability after Promise constructor returns", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue thisValue = default, args = default;

        reject.Call(FunctionLiteral((executor) => {
          EcmaValue r = Intercept(v => {
            thisValue = This;
            args = Arguments;
          });
          executor.Call(Undefined, UnexpectedFulfill, r);
          That(Logs.Count, Is.EqualTo(0), "callCount before returning from constructor");
        }), obj);

        That(Logs.Count, Is.EqualTo(1), "callCount after call to resolve()");
        That(args, Is.EquivalentTo(new[] { obj }));
        That(thisValue, Is.Undefined);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Resolve(RuntimeFunction resolve) {
      IsUnconstructableFunctionWLength(resolve, "resolve", 1);
      That(Promise, Has.OwnProperty("resolve", resolve, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should resolve an object whose `then` property is not callable", () => {
        EcmaValue nonThenable = CreateObject(new { then = Null });
        resolve.Call(_, nonThenable).Invoke("then", Intercept(v => That(v, Is.EqualTo(nonThenable))));
        VerifyPromiseSettled();
      });

      It("should reject from getting then property", () => {
        EcmaValue err = Error.Construct();
        EcmaValue poisonedThen = CreateObject(("then", get: () => Throw(err), set: null));
        resolve.Call(_, poisonedThen).Invoke("then", Intercept(UnexpectedFulfill), Intercept(e => That(e, Is.EqualTo(err))));
      });

      It("should return promise with unique constructor", () => {
        EcmaValue promise1 = Promise.Construct(Noop);
        promise1["constructor"] = Null;
        Case((_, promise1), Is.Not.EqualTo(promise1));
      });

      It("should throw a TypeError if capabilities executor already called with non-undefined values", () => {
        Logs.Clear();
        resolve.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call();
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }));
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called with no arguments");

        Logs.Clear();
        resolve.Call(FunctionLiteral((executor) => {
          Logs.Add("a");
          executor.Call(Undefined, Undefined, Undefined);
          Logs.Add("b");
          executor.Call(Undefined, Noop, Noop);
          Logs.Add("c");
        }));
        That(Logs, Is.EquivalentTo(new[] { "a", "b", "c" }), "executor initially called  with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (undefined, function)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (function, undefined)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, "invalid value", 123);
            Logs.Add("b");
            executor.Call(Undefined, Noop, Noop);
            Logs.Add("c");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor initially called  with (String, Number)");
      });

      It("should throw a TypeError if either resolve or reject capability is not callable", () => {
        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a" }), "executor not called at all");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call();
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with no arguments");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Undefined);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, undefined)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Undefined, Noop);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (undefined, function)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, Noop, Undefined);
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (function, undefined)");

        Logs.Clear();
        That(() => {
          resolve.Call(FunctionLiteral((executor) => {
            Logs.Add("a");
            executor.Call(Undefined, 123, "invalid value");
            Logs.Add("b");
          }));
        }, Throws.TypeError);
        That(Logs, Is.EquivalentTo(new[] { "a", "b" }), "executor called with (Number, String)");
      });

      It("should return abrupt from resolve capability", () => {
        Case(FunctionLiteral((executor) => {
          return Promise.Construct(FunctionLiteral(() => {
            executor.Call(Undefined, ThrowTest262Exception, Noop);
          }));
        }), Throws.Test262);
      });

      It("should invoke this constructor", () => {
        EcmaValue executor = default;
        EcmaValue SubPromise = new ClassLiteral(Extends(Promise)) {
          ["constructor"] = FunctionLiteral(Intercept((a) => {
            Super.Construct(a);
            executor = a;
          }))
        };

        EcmaValue p = resolve.Call(SubPromise);
        That(p["constructor"], Is.EqualTo(SubPromise));
        That(p, Is.InstanceOf(SubPromise));
        That(executor, Is.TypeOf("function"));
        That(Logs.Count, Is.EqualTo(1));
      });

      It("should throw a TypeError if this is not an Object", () => {
        EcmaValue promise = Promise.Construct(Noop);
        promise["constructor"] = Undefined;
        Case((Undefined, promise), Throws.TypeError);
        promise["constructor"] = Null;
        Case((Null, promise), Throws.TypeError);
        promise["constructor"] = true;
        Case((true, promise), Throws.TypeError);
        promise["constructor"] = 1;
        Case((1, promise), Throws.TypeError);
        promise["constructor"] = "";
        Case(("", promise), Throws.TypeError);

        EcmaValue symbol = new Symbol();
        promise["constructor"] = symbol;
        Case((symbol, promise), Throws.TypeError);

        Case((Undefined, EcmaArray.Of()), Throws.TypeError);
        Case((Null, EcmaArray.Of()), Throws.TypeError);
        Case((86, EcmaArray.Of()), Throws.TypeError);
        Case(("string", EcmaArray.Of()), Throws.TypeError);
        Case((true, EcmaArray.Of()), Throws.TypeError);
        Case((new Symbol(), EcmaArray.Of()), Throws.TypeError);
      });

      It("should throw a TypeError if this is not a constructor", () => {
        Case(resolve, Throws.TypeError);
      });

      It("should throw a TypeError for bad this", () => {
        Case((Noop, 4), Throws.TypeError);
      });

      It("should return abrupt from constructor", () => {
        Case(ThrowTest262Exception, Throws.Test262);
      });

      It("should call resolve capability after Promise constructor returns", () => {
        EcmaValue obj = new EcmaObject();
        EcmaValue thisValue = default, args = default;

        resolve.Call(FunctionLiteral((executor) => {
          EcmaValue r = Intercept(v => {
            thisValue = This;
            args = Arguments;
          });
          executor.Call(Undefined, r, UnexpectedReject);
          That(Logs.Count, Is.EqualTo(0), "callCount before returning from constructor");
        }), obj);

        That(Logs.Count, Is.EqualTo(1), "callCount after call to resolve()");
        That(args, Is.EquivalentTo(new[] { obj }));
        That(thisValue, Is.Undefined);
      });

      It("should resolve with a resolved Promise instance whose `then` method has been overridden", () => {
        EcmaValue value = new EcmaObject();
        EcmaValue rejectCallCount = 0;
        EcmaValue thenable = Promise.Construct(FunctionLiteral(r => r.Call()));
        thenable["then"] = FunctionLiteral(r => r.Call(Undefined, value));
        resolve.Call(_, thenable).Invoke("then", Intercept(v => That(v, Is.EqualTo(value))), UnexpectedReject);
        VerifyPromiseSettled();
      });

      It("should pass through promise with same constructor", () => {
        EcmaValue p1, p2;

        p1 = resolve.Call(_, 1);
        Case((_, p1), Is.EqualTo(p1));

        EcmaValue resolveP1 = default;
        EcmaValue obj = new EcmaObject();
        p1 = Promise.Construct(FunctionLiteral(r => resolveP1 = r));
        p2 = resolve.Call(_, p1);
        That(p2, Is.EqualTo(p1));

        p2.Invoke("then", Intercept(v => That(v, Is.EqualTo(obj))));
        resolveP1.Call(Undefined, obj);
        VerifyPromiseSettled();

        EcmaValue rejectP1 = default;
        p1 = Promise.Construct(FunctionLiteral((_, r) => rejectP1 = r));
        p2 = resolve.Call(_, p1);
        That(p2, Is.EqualTo(p1));

        p2.Invoke("then", Intercept(UnexpectedFulfill), Intercept(v => That(v, Is.EqualTo(obj))));
        rejectP1.Call(Undefined, obj);
        VerifyPromiseSettled();
      });

      It("should throw TypeError for self-resolved Promise", () => {
        EcmaValue resolveP1 = default;
        EcmaValue p1 = Promise.Construct(FunctionLiteral(r => resolveP1 = r));
        resolveP1.Call(Undefined, p1);

        p1.Invoke("then", Intercept(UnexpectedFulfill), Intercept(e => That(e, Is.InstanceOf(TypeError))));
        VerifyPromiseSettled();
      });

      It("should delegate to foreign thenable", () => {
        EcmaValue p1, thenable = default;
        thenable = CreateObject(new {
          then = FunctionLiteral((onResolve, onReject) => {
            Logs.Add(3);
            That(This, Is.EqualTo(thenable));
            onResolve.Call(Undefined, "resolved");
            Logs.Add(4);
            Throw("interrupt flow");
            Logs.Add(4);
          })
        });

        Logs.Add(1);
        p1 = resolve.Call(_, thenable);

        Logs.Add(2);
        p1.Invoke("then", FunctionLiteral(q => {
          Logs.Add(5);
          That(q, Is.EqualTo("resolved"));
          That(Logs, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
        }));
        VerifyPromiseSettled();
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Species(RuntimeFunction species) {
      That(Promise, Has.OwnProperty(WellKnownSymbol.Species, EcmaPropertyAttributes.Configurable));
      That(Promise.GetOwnProperty(WellKnownSymbol.Species).Set, Is.Undefined);

      IsUnconstructableFunctionWLength(species, "get [Symbol.species]", 0);
      Case(Promise, Is.EqualTo(Promise));

      EcmaValue thisValue = new EcmaObject();
      Case(thisValue, Is.EqualTo(thisValue));
    }

    private void CallAndVerifyReturn(EcmaValue fn, EcmaValue value = default) {
      That(fn.Call(Undefined, value), Is.Undefined);
    }
  }
}
