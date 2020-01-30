using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AsyncGeneratorPrototype : TestBase {
    [Test]
    public void Properties() {
      (EcmaValue AsyncGenerator, EcmaValue AsyncGeneratorPrototype) = GetGeneratorAndPrototype();
      That(AsyncGeneratorPrototype, Has.OwnProperty("constructor", AsyncGenerator, EcmaPropertyAttributes.Configurable));
      That(AsyncGeneratorPrototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "AsyncGenerator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      (EcmaValue _, EcmaValue AsyncGeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(next, "next", 1);
      That(AsyncGeneratorPrototype, Has.OwnProperty("next", next, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a promise for an IteratorResult object", () => {
        EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        EcmaValue result = iter.Invoke("next");
        result.Invoke("then", Intercept(v => {
          VerifyIteratorResult(v, true);
        }));
        That(result, Is.InstanceOf(Global.Promise));
        VerifyPromiseSettled();
      });

      It("should not throw exception while iterator is in state executing", () => {
        EcmaValue iter = default;
        EcmaValue executionorder = 0;
        EcmaValue valueisset = false;

        IEnumerable<EcmaValue> g_() {
          iter.Invoke("next").Invoke("then", Intercept(result => {
            That(valueisset, Is.True, "variable valueisset should be set to true");
            That(++executionorder, Is.EqualTo(2));
            VerifyIteratorResult(result, false, 2);
          }), UnexpectedReject);

          valueisset = true;

          yield return 1;
          yield return 2;
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        iter = g.Call();

        iter.Invoke("next").Invoke("then", Intercept(result => {
          That(++executionorder, Is.EqualTo(1));
          VerifyIteratorResult(result, false, 1);

          iter.Invoke("next").Invoke("then", Intercept(result_ => {
            That(++executionorder, Is.EqualTo(3));
            VerifyIteratorResult(result_, true);
          }));
        }), UnexpectedReject);

        VerifyPromiseSettled();
      });

      It("should request from iterator processed in order using then", () => {
        EcmaValue yieldOrder = 0;
        EcmaValue resolveLatePromise = default;
        EcmaValue resolveLater = FunctionLiteral(() => {
          return Global.Promise.Construct(FunctionLiteral(resolve => {
            resolveLatePromise = resolve;
          }));
        });

        IEnumerable<EcmaValue> g_() {
          yield return resolveLater.Call();
          yield return ++yieldOrder;
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        That(yieldOrder, Is.Zero);

        EcmaValue item1 = iter.Invoke("next");
        EcmaValue item2 = iter.Invoke("next");
        EcmaValue item3 = iter.Invoke("next");

        EcmaValue resolvedorder = 0;
        item1.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(1));
          VerifyIteratorResult(result, false, 1);
        }));
        item2.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(2));
          VerifyIteratorResult(result, false, 2);
        }));
        item3.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(3));
          VerifyIteratorResult(result, true);
        }));

        resolveLatePromise.Call(default, ++yieldOrder);
        VerifyPromiseSettled();
      });

      It("should request from iterator processed in order using await", () => {
        EcmaValue yieldOrder = 0;
        EcmaValue resolveLatePromise = default;
        EcmaValue resolveLater = FunctionLiteral(() => {
          return Global.Promise.Construct(FunctionLiteral(resolve => {
            resolveLatePromise = resolve;
          }));
        });

        IEnumerable<EcmaValue> g_() {
          yield return resolveLater.Call();
          yield return ++yieldOrder;
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        That(yieldOrder, Is.Zero);

        EcmaValue item1 = iter.Invoke("next");
        EcmaValue item2 = iter.Invoke("next");
        EcmaValue item3 = iter.Invoke("next");

        EcmaValue awaitNexts = FunctionLiteral(async () => {
          That((await item3)["value"], Is.Undefined);
          That(yieldOrder, Is.EqualTo(2));
          That((await item2)["value"], Is.EqualTo(2));
          That((await item1)["value"], Is.EqualTo(1));
        });

        resolveLatePromise.Call(default, ++yieldOrder);
        awaitNexts.Call().Invoke("then", Intercept(Noop));
        VerifyPromiseSettled();
      });

      It("should request from iterator processed in order using then in reverse", () => {
        IEnumerable<EcmaValue> g_() {
          yield return "first";
          yield return "second";
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();

        EcmaValue item1 = iter.Invoke("next");
        EcmaValue item2 = iter.Invoke("next");
        EcmaValue item3 = iter.Invoke("next");

        EcmaValue resolvedorder = 0;
        item3.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(3));
          VerifyIteratorResult(result, true);
        }));
        item2.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(2));
          VerifyIteratorResult(result, false, "second");
        }));
        item1.Invoke("then", Intercept(result => {
          resolvedorder++;
          That(resolvedorder, Is.EqualTo(1));
          VerifyIteratorResult(result, false, "first");
        }));
        VerifyPromiseSettled();
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Return(RuntimeFunction @return) {
      (EcmaValue _, EcmaValue AsyncGeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(@return, "return", 1);
      That(AsyncGeneratorPrototype, Has.OwnProperty("return", @return, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a promise for an IteratorResult object", () => {
        EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        EcmaValue result = iter.Invoke("return");
        result.Invoke("then", Intercept(v => {
          VerifyIteratorResult(v, true);
        }));
        That(result, Is.InstanceOf(Global.Promise));
        VerifyPromiseSettled();
      });

      It("should not throw exception while iterator is in state executing", () => {
        EcmaValue iter = default;
        EcmaValue executionorder = 0;
        EcmaValue valueisset = false;

        IEnumerable<EcmaValue> g_() {
          iter.Invoke("return", 42).Invoke("then", Intercept(result => {
            That(valueisset, Is.True, "variable valueisset should be set to true");
            That(++executionorder, Is.EqualTo(2));
            VerifyIteratorResult(result, true, 42);
          }), UnexpectedReject);

          valueisset = true;

          yield return 1;
          ThrowTest262Exception();
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        iter = g.Call();

        iter.Invoke("next").Invoke("then", Intercept(result => {
          That(++executionorder, Is.EqualTo(1));
          VerifyIteratorResult(result, false, 1);

          iter.Invoke("next").Invoke("then", Intercept(result_ => {
            That(++executionorder, Is.EqualTo(3));
            VerifyIteratorResult(result_, true);
          }));
        }), UnexpectedReject);

        VerifyPromiseSettled();
      });

      It("should result in fullfilled promise when called on completed iterator", () => {
        EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(result => {
          VerifyIteratorResult(result, true);
          iter.Invoke("return", 42).Invoke("then", Intercept(result_ => {
            VerifyIteratorResult(result_, true, 42);
          }));
        }));
        VerifyPromiseSettled(count: 2);
      });

      It("should cause generator not to resume after a return type completion, returning promise before start", () => {
        EcmaValue g = new AsyncGeneratorFunction(() => throw new System.Exception());
        EcmaValue iter = g.Call();
        EcmaValue resolve = default;
        EcmaValue promise = new Promise((resolver, _) => {
          resolve = resolver;
        });
        iter.Invoke("return", promise).Invoke("then", Intercept(result_ => {
          VerifyIteratorResult(result_, true, "unwrapped-value");
          iter.Invoke("next").Invoke("then", Intercept(result => {
            VerifyIteratorResult(result, true);
          }));
        }));
        resolve.Call(default, "unwrapped-value");
        VerifyPromiseSettled(count: 2);
      });

      It("should cause generator not to resume after a return type completion, returning non-promise before start", () => {
        EcmaValue g = new AsyncGeneratorFunction(() => throw new System.Exception());
        EcmaValue iter = g.Call();
        iter.Invoke("return", "sent-value").Invoke("then", Intercept(result_ => {
          VerifyIteratorResult(result_, true, "sent-value");
          iter.Invoke("next").Invoke("then", Intercept(result => {
            VerifyIteratorResult(result, true);
          }));
        }));
        VerifyPromiseSettled(count: 2);
      });

      It("should cause generator not to resume after a return type completion, returning promise", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          throw new System.Exception();
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        EcmaValue resolve = default;
        EcmaValue promise = new Promise((resolver, _) => {
          resolve = resolver;
        });
        iter.Invoke("next").Invoke("then", Intercept(result2 => {
          VerifyIteratorResult(result2, false, 1);
          iter.Invoke("return", promise).Invoke("then", Intercept(result_ => {
            VerifyIteratorResult(result_, true, "unwrapped-value");
            iter.Invoke("next").Invoke("then", Intercept(result => {
              VerifyIteratorResult(result, true);
            }));
          }));
        }));
        resolve.Call(default, "unwrapped-value");
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator not to resume after a return type completion, returning non-promise", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          throw new System.Exception("Generator must not be resumed");
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(result2 => {
          VerifyIteratorResult(result2, false, 1);
          iter.Invoke("return", "sent-value").Invoke("then", Intercept(result_ => {
            VerifyIteratorResult(result_, true, "sent-value");
            iter.Invoke("next").Invoke("then", Intercept(result => {
              VerifyIteratorResult(result, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block", () => {
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 2;
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("return", "sent-value").Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, false, 2);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true, "sent-value");
              iter.Invoke("next").Invoke("then", Intercept(r4 => {
                VerifyIteratorResult(r4, true);
              }));
            }));
          }));
        }));
        VerifyPromiseSettled(count: 4);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block, capturing a new abrupt completion and does not resume again within that finally block", () => {
        EcmaValue error = Error.Construct("boop");
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            Keywords.Throw(error);
            throw new System.Exception("Generator must not be resumed.");
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("return", "sent-value").Invoke("then", UnexpectedFulfill, Intercept(r2 => {
            That(r2, Is.EqualTo(error));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block", () => {
        EcmaValue error = Error.Construct("boop");
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            yield return Yield.Done("done");
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("return", "sent-value").Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, "done");
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Throw(RuntimeFunction @throw) {
      (EcmaValue _, EcmaValue AsyncGeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(@throw, "throw", 1);
      That(AsyncGeneratorPrototype, Has.OwnProperty("throw", @throw, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should return a rejected promise", () => {
        EcmaValue error = Error.Construct();
        EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
        EcmaValue result = g.Call().Invoke("throw", error);

        That(result, Is.InstanceOf(Global.Promise));
        result.Invoke("then", UnexpectedFulfill, Intercept(err => That(err, Is.EqualTo(error))));
        VerifyPromiseSettled();
      });

      It("should rejected promise when called on completed iterator", () => {
        EcmaValue error = Error.Construct();
        EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, true);
          iter.Invoke("throw", error).Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(error));
          }));
        }));
        VerifyPromiseSettled();
      });

      It("should not throw exception while iterator is in state executing", () => {
        EcmaValue iter = default;
        EcmaValue order = 0;
        EcmaValue thrownErr = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          iter.Invoke("throw", thrownErr).Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(++order, Is.EqualTo(2));
            That(err, Is.EqualTo(thrownErr));
          }));
          yield return 1;
          yield return 2;
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          That(++order, Is.EqualTo(1));
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("next").Invoke("then", Intercept(r2 => {
            That(++order, Is.EqualTo(3));
            VerifyIteratorResult(r2, true);
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator not to resume after a throw completion with a promise arg before start", () => {
        EcmaValue error = Error.Construct();
        EcmaValue g = new AsyncGeneratorFunction(() => throw new System.Exception());
        EcmaValue iter = g.Call();
        EcmaValue promise = new Promise();
        iter.Invoke("throw", error).Invoke("then", UnexpectedFulfill, Intercept(err => {
          That(err, Is.EqualTo(error));
          iter.Invoke("next").Invoke("then", Intercept(result => {
            VerifyIteratorResult(result, true);
          }));
        }));
        VerifyPromiseSettled(count: 2);
      });

      It("should cause generator not to resume after a throw completion with a non-promise arg before start", () => {
        EcmaValue error = Error.Construct();
        EcmaValue g = new AsyncGeneratorFunction(() => throw new System.Exception());
        EcmaValue iter = g.Call();
        iter.Invoke("throw", error).Invoke("then", UnexpectedFulfill, Intercept(err => {
          That(err, Is.EqualTo(error));
          iter.Invoke("next").Invoke("then", Intercept(result => {
            VerifyIteratorResult(result, true);
          }));
        }));
        VerifyPromiseSettled(count: 2);
      });

      It("should cause generator not to resume after a return type completion, returning promise", () => {
        EcmaValue error = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          throw new System.Exception("Generator must not be resumed");
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        EcmaValue promise = new Promise();
        iter.Invoke("next").Invoke("then", Intercept(result2 => {
          VerifyIteratorResult(result2, false, 1);
          iter.Invoke("throw", error).Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(error));
            iter.Invoke("next").Invoke("then", Intercept(result => {
              VerifyIteratorResult(result, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator not to resume after a return type completion, returning non-promise", () => {
        EcmaValue error = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          throw new System.Exception("Generator must not be resumed");
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(result2 => {
          VerifyIteratorResult(result2, false, 1);
          iter.Invoke("throw", error).Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(error));
            iter.Invoke("next").Invoke("then", Intercept(result => {
              VerifyIteratorResult(result, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated catch-block", () => {
        EcmaValue error = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in catch block.");
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            That(ex, Is.EqualTo(error));
            yield return Yield.Done("done");
          }
          yield return Yield.TryCatch(try_, catch_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("throw", error).Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, "done");
            iter.Invoke("next").Invoke("then", Intercept(r4 => {
              VerifyIteratorResult(r4, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block", () => {
        EcmaValue error = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 2;
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("throw", error).Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, false, 2);
            iter.Invoke("next").Invoke("then", UnexpectedFulfill, Intercept(err => {
              That(err, Is.EqualTo(error));
              iter.Invoke("next").Invoke("then", Intercept(r4 => {
                VerifyIteratorResult(r4, true);
              }));
            }));
          }));
        }));
        VerifyPromiseSettled(count: 4);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block, capturing a new abrupt completion and does not resume again within that finally block", () => {
        EcmaValue error = Error.Construct("boop");
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            Keywords.Throw(error);
            throw new System.Exception("Generator must not be resumed.");
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("throw", Error.Construct("superceded")).Invoke("then", UnexpectedFulfill, Intercept(r2 => {
            That(r2, Is.EqualTo(error));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should cause generator suspended in a yield position resumes execution within an associated finally block", () => {
        EcmaValue error = Error.Construct("boop");
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            throw new System.Exception("Generator must be resumed in finally block.");
          }
          IEnumerable<EcmaValue> finally_() {
            yield return Yield.Done("done");
          }
          yield return Yield.TryFinally(try_, finally_);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          VerifyIteratorResult(r1, false, 1);
          iter.Invoke("throw", error).Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, "done");
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });
    }

    private (EcmaValue, EcmaValue) GetGeneratorAndPrototype() {
      EcmaValue g = new AsyncGeneratorFunction(EmptyGenerator);
      EcmaValue AsyncGenerator = Object.Invoke("getPrototypeOf", g);
      EcmaValue AsyncGeneratorPrototype = AsyncGenerator["prototype"];
      return (AsyncGenerator, AsyncGeneratorPrototype);
    }
  }
}
