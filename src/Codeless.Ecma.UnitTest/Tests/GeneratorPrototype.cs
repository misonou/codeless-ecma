using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class GeneratorPrototype : TestBase {
    [Test]
    public void Properties() {
      (EcmaValue Generator, EcmaValue GeneratorPrototype) = GetGeneratorAndPrototype();
      That(GeneratorPrototype, Has.OwnProperty("constructor", Generator, EcmaPropertyAttributes.Configurable));
      That(GeneratorPrototype, Has.OwnProperty(WellKnownSymbol.ToStringTag, "Generator", EcmaPropertyAttributes.Configurable));
    }

    [Test, RuntimeFunctionInjection]
    public void Next(RuntimeFunction next) {
      (EcmaValue _, EcmaValue GeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(next, "next", 1);
      That(GeneratorPrototype, Has.OwnProperty("next", next, EcmaPropertyAttributes.DefaultMethodProperty));

      It("should throw a TypeError if the this value of `next` is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case("s", Throws.TypeError);
        Case(1, Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);

        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((true, 1), Throws.TypeError);
        Case(("s", 1), Throws.TypeError);
        Case((1, 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError if the context of `next` does not define the [[GeneratorState]] internal slot", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);

        Case(Object.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(g, Throws.TypeError);
        Case(g["prototype"], Throws.TypeError);

        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((g, 1), Throws.TypeError);
        Case((g["prototype"], 1), Throws.TypeError);
      });

      It("should throw a TypeError if the generator is resumed while in the \"executing\" state and the generator should be marked as \"completed\"", () => {
        EcmaValue iter = default;
        IEnumerable<EcmaValue> withoutVal_() {
          iter.Invoke("next");
          yield break;
        }
        IEnumerable<EcmaValue> withVal_() {
          iter.Invoke("next", 42);
          yield break;
        }
        EcmaValue withoutVal = new GeneratorFunction(withoutVal_);
        EcmaValue withVal = new GeneratorFunction(withVal_);

        iter = withoutVal.Call();
        Case(iter, Throws.TypeError);
        VerifyIteratorResult(iter.Invoke("next"), true);

        iter = withVal.Call();
        Case(iter, Throws.TypeError);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should visit the yielded value and then complete When a generator body contains a lone yield statement", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should visit each yielded value and then complete when a generator body contains two consecutive yield statements", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          yield return 2;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should initially complete with `undefined` as its value When a generator body contains no control flow statements", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), true);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should immediately complete with the returned value When a generator body contains a lone return statement", () => {
        IEnumerable<EcmaValue> g_() {
          yield return Yield.Done(23);
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), true, 23);
      });

      It("should yield from yield expressions", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          yield return Yield.Done(Yield.ResumeValue);
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next", 3), true, 3);
      });

      It("should return an object that has own properties `value` and `done` and that inherits directly from the Object prototype", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);
        EcmaValue result = next.Call(g.Call());
        That(result, Has.OwnProperty("value", EcmaPropertyAttributes.DefaultDataProperty));
        That(result, Has.OwnProperty("done", EcmaPropertyAttributes.DefaultDataProperty));
        That(Object.Invoke("getPrototypeOf", result), Is.EqualTo(Object.Prototype));
      });

      It("should have context of that object when invoked as a method of an object", () => {
        EcmaValue context = default;
        IEnumerable<EcmaValue> g_() {
          context = This;
          yield break;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue obj = CreateObject(new { g });
        EcmaValue iter = obj.Invoke("g");
        iter.Invoke("next");
        That(context, Is.EqualTo(obj));
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Return(RuntimeFunction @return) {
      (EcmaValue _, EcmaValue GeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(@return, "return", 1);
      That(GeneratorPrototype, Has.OwnProperty("return", @return, EcmaPropertyAttributes.DefaultMethodProperty));

      const string interrupt = "interrupt control flow as if a `return` statement had appeared at that location in the function body";

      It("should throw a TypeError if the this value of `next` is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case("s", Throws.TypeError);
        Case(1, Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);

        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((true, 1), Throws.TypeError);
        Case(("s", 1), Throws.TypeError);
        Case((1, 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError if the context of `next` does not define the [[GeneratorState]] internal slot", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);

        Case(Object.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(g, Throws.TypeError);
        Case(g["prototype"], Throws.TypeError);

        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((g, 1), Throws.TypeError);
        Case((g["prototype"], 1), Throws.TypeError);
      });

      It("should throw a TypeError if the generator is in the \"executing\" state and the generator should be marked as \"completed\"", () => {
        EcmaValue iter = default;
        IEnumerable<EcmaValue> g_() {
          iter.Invoke("return", 42);
          yield break;
        }
        EcmaValue g = new GeneratorFunction(g_);
        iter = g.Call();
        That(() => iter.Invoke("next"), Throws.TypeError);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should honor the abrupt completion and remain in the 'completed' state", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        VerifyIteratorResult(iter.Invoke("return", 33), true, 33);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should honor the abrupt completion and trigger a transition into the 'completed' from a generator in the 'suspendedStart' state", () => {
        int bodyCount = 0;
        IEnumerable<EcmaValue> g_() {
          bodyCount++;
          yield break;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("return", 56), true, 56);
        That(bodyCount, Is.Zero, "body not evaluated during processing of `return` method");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(bodyCount, Is.Zero, "body not evaluated when \"completed\" generator is advanced");
      });

      It($"should {interrupt} when a generator is paused before a `try..catch` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return default;
          try {
            unreachable++;
          } catch {
            throw;
          }
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused after a `try..catch` statement", () => {
        int afterCatch = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            Keywords.Throw(Error.Construct());
            yield break;
          }
          yield return Yield.TryCatch(try_, EmptyGenerator);
          afterCatch++;
          yield return default;
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(afterCatch, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within the `try` block of a `try..catch` statement", () => {
        int inTry = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            inTry++;
            yield return default;
            unreachable++;
          }
          yield return Yield.TryCatch(try_);
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inTry, Is.EqualTo(1));

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within the `catch` block of a `try..catch` statement", () => {
        int inCatch = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            Keywords.Throw(Error.Construct());
            yield break;
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            inCatch++;
            yield return default;
            unreachable++;
          }
          yield return Yield.TryCatch(try_, catch_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inCatch, Is.EqualTo(1));

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused before a `try..finally` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return default;
          unreachable++;
          try { } finally { }
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused after a `try..finally` statement", () => {
        int afterFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          try { } finally { }
          afterFinally++;
          yield return default;
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(afterFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `catch` block that is declared within a `try` block of a `try..catch` statement", () => {
        int inCatch = 0;
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try1() {
            IEnumerable<EcmaValue> try_() {
              Keywords.Throw(Error.Construct());
              yield break;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              inCatch++;
              yield return default;
              unreachable++;
            }
            yield return Yield.TryCatch(try_, catch_);
          }
          IEnumerable<EcmaValue> finally1() {
            inFinally++;
            yield break;
          }
          yield return Yield.TryFinally(try1, finally1);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inCatch, Is.EqualTo(1));
        That(inFinally, Is.EqualTo(0));

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `finally` block of a `try..catch` statement", () => {
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            Keywords.Throw(Error.Construct());
            try { } catch { }
            yield break;
          }
          IEnumerable<EcmaValue> finally_() {
            inFinally++;
            yield return default;
            unreachable++;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block that is declared within a `try` block of a `try..catch` statement", () => {
        int inTry = 0;
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try1() {
            IEnumerable<EcmaValue> try_() {
              inTry++;
              yield return default;
              unreachable++;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              Keywords.Throw(ex);
              yield break;
            }
            yield return Yield.TryCatch(try_, catch_);
          }
          IEnumerable<EcmaValue> finally1() {
            inFinally++;
            yield break;
          }
          yield return Yield.TryFinally(try1, finally1);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inTry, Is.EqualTo(1));
        That(inFinally, Is.EqualTo(0));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block of a `try..catch` statement and following a nested `try..catch` statment", () => {
        int inCatch = 0;
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            IEnumerable<EcmaValue> try1_() {
              Keywords.Throw(Error.Construct());
              yield break;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue _) {
              inCatch++;
              yield break;
            }
            yield return Yield.TryCatch(try1_, catch_);
          }
          IEnumerable<EcmaValue> finally_() {
            inFinally++;
            yield return default;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inCatch, Is.EqualTo(1));
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block of a `try..catch` statement and before a nested `try..catch` statement", () => {
        int inTry = 0;
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            inTry++;
            yield return default;
            try {
              unreachable++;
            } catch {
              throw;
            }
            unreachable++;
          }
          IEnumerable<EcmaValue> finally_() {
            inFinally++;
            yield break;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inTry, Is.EqualTo(1));
        That(inFinally, Is.EqualTo(0));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `finally` block of a `try..finally` statement", () => {
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield break;
          }
          IEnumerable<EcmaValue> finally_() {
            inFinally++;
            yield return default;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block block of a `try..finally` statement", () => {
        int inTry = 0;
        int inFinally = 0;
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            inTry++;
            yield return default;
            unreachable++;
          }
          IEnumerable<EcmaValue> finally_() {
            inFinally++;
            yield break;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
        }
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        That(inTry, Is.EqualTo(1));
        That(inFinally, Is.EqualTo(0));
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        VerifyIteratorResult(iter.Invoke("return", 45), true, 45);
        That(inFinally, Is.EqualTo(1));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `return`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });
    }

    [Test, RuntimeFunctionInjection]
    public void Throw(RuntimeFunction @throw) {
      (EcmaValue _, EcmaValue GeneratorPrototype) = GetGeneratorAndPrototype();
      IsUnconstructableFunctionWLength(@throw, "throw", 1);
      That(GeneratorPrototype, Has.OwnProperty("throw", @throw, EcmaPropertyAttributes.DefaultMethodProperty));

      const string interrupt = "interrupt control flow as if a `throw` statement had appeared at that location in the function body";

      It("should throw a TypeError if the this value of `next` is not an object", () => {
        Case(Undefined, Throws.TypeError);
        Case(Null, Throws.TypeError);
        Case(true, Throws.TypeError);
        Case("s", Throws.TypeError);
        Case(1, Throws.TypeError);
        Case(new Symbol(), Throws.TypeError);

        Case((Undefined, 1), Throws.TypeError);
        Case((Null, 1), Throws.TypeError);
        Case((true, 1), Throws.TypeError);
        Case(("s", 1), Throws.TypeError);
        Case((1, 1), Throws.TypeError);
        Case((new Symbol(), 1), Throws.TypeError);
      });

      It("should throw a TypeError if the context of `next` does not define the [[GeneratorState]] internal slot", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);

        Case(Object.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(Function.Construct(), Throws.TypeError);
        Case(g, Throws.TypeError);
        Case(g["prototype"], Throws.TypeError);

        Case((Object.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((Function.Construct(), 1), Throws.TypeError);
        Case((g, 1), Throws.TypeError);
        Case((g["prototype"], 1), Throws.TypeError);
      });

      It("should throw a TypeError if the generator is in the \"executing\" state and the generator should be marked as \"completed\"", () => {
        EcmaValue iter = default;
        IEnumerable<EcmaValue> g_() {
          iter.Invoke("throw", 42);
          yield break;
        }
        EcmaValue g = new GeneratorFunction(g_);
        iter = g.Call();
        That(() => iter.Invoke("next"), Throws.TypeError);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should honor the abrupt completion and remain in the 'completed' state", () => {
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(EmptyGenerator);
        EcmaValue iter = g.Call();
        iter.Invoke("next");
        Case((iter, E.Construct()), Throws.InstanceOf(E));
        VerifyIteratorResult(iter.Invoke("return", 33), true, 33);
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It("should honor the abrupt completion and trigger a transition into the 'completed' state from a generator in the 'suspendedStart' state", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          yield return 2;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        Case((iter, E.Construct()), Throws.InstanceOf(E));
        VerifyIteratorResult(iter.Invoke("next"), true);
      });

      It($"should {interrupt} when a generator is paused before a `try..catch` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          unreachable++;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            yield return ex;
          }
          yield return Yield.TryCatch(try_, catch_);
          yield return 3;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused after a `try..catch` statement", () => {
        EcmaValue obj = Object.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
            Keywords.Throw(obj);
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            yield return ex;
          }
          yield return Yield.TryCatch(try_, catch_);
          yield return 3;
          unreachable++;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, obj);
        VerifyIteratorResult(iter.Invoke("next"), false, 3);

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within the `catch` block of a `try..catch` statement", () => {
        EcmaValue obj = Object.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
            Keywords.Throw(obj);
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            yield return ex;
            unreachable++;
          }
          yield return Yield.TryCatch(try_, catch_);
          yield return 3;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, obj);

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within the `try` block of a `try..catch` statement", () => {
        EcmaValue obj = Object.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
            unreachable++;
          }
          IEnumerable<EcmaValue> catch_(EcmaValue ex) {
            yield return ex;
          }
          yield return Yield.TryCatch(try_, catch_);
          yield return 3;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("throw", obj), false, obj);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), false, 3);
        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused before a `try..finally` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          unreachable++;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 3;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 4;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        That(unreachable, Is.Zero, "statement following `yield` not executed (paused at yield)");

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused after a `try..finally` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 3;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 4;
          unreachable++;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, 3);
        VerifyIteratorResult(iter.Invoke("next"), false, 4);

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `catch` block that is declared within a `try` block of a `try..catch` statement", () => {
        EcmaValue exception = Object.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            IEnumerable<EcmaValue> try_1() {
              yield return 2;
              Keywords.Throw(exception);
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              yield return ex;
              unreachable++;
            }
            yield return Yield.TryCatch(try_1, catch_);
            yield return 3;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 4;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 5;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, exception);
        VerifyIteratorResult(iter.Invoke("throw", E.Construct()), false, 4);

        That(() => iter.Invoke("next", E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `finally` block of a `try..catch` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            Keywords.Throw(Error.Construct());
            IEnumerable<EcmaValue> try_1() {
              yield return 2;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              yield return ex;
            }
            yield return Yield.TryCatch(try_1, catch_);
            yield return 3;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 4;
            unreachable++;
          }
          yield return Yield.TryFinally(try_, finally_);
          unreachable++;
          yield return 5;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 4);

        That(() => iter.Invoke("throw", E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block that is declared within a `try` block of a `try..catch` statement", () => {
        EcmaValue exception = Error.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            IEnumerable<EcmaValue> try_1() {
              yield return 2;
              unreachable++;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              yield return ex;
            }
            yield return Yield.TryCatch(try_1, catch_);
            yield return 3;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 4;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 5;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("throw", exception), false, exception);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), false, 3);
        VerifyIteratorResult(iter.Invoke("next"), false, 4);
        VerifyIteratorResult(iter.Invoke("next"), false, 5);
        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block of a `try..catch` statement and following a nested `try..catch` statment", () => {
        EcmaValue exception = Error.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            IEnumerable<EcmaValue> try_1() {
              yield return 2;
              Keywords.Throw(exception);
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              yield return ex;
            }
            yield return Yield.TryCatch(try_1, catch_);
            yield return 3;
            unreachable++;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 4;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 5;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, exception);
        VerifyIteratorResult(iter.Invoke("next"), false, 3);
        VerifyIteratorResult(iter.Invoke("throw", E.Construct()), false, 4);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        That(() => iter.Invoke("next"), Throws.InstanceOf(E));
        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block of a `try..catch` statement and before a nested `try..catch` statement", () => {
        EcmaValue exception = Error.Construct();
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          IEnumerable<EcmaValue> try_() {
            yield return 1;
            unreachable++;
            IEnumerable<EcmaValue> try_1() {
              yield return 2;
            }
            IEnumerable<EcmaValue> catch_(EcmaValue ex) {
              yield return ex;
            }
            yield return Yield.TryCatch(try_1, catch_);
            yield return 3;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 4;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 5;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("throw", E.Construct()), false, 4);
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        That(() => iter.Invoke("next"), Throws.InstanceOf(E));
        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within the `finally` block of a `try..finally` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 3;
            unreachable++;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 4;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("next"), false, 3);

        Case((iter, E.Construct()), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });

      It($"should {interrupt} when a generator is paused within a `try` block of a `try..finally` statement", () => {
        int unreachable = 0;
        IEnumerable<EcmaValue> g_() {
          yield return 1;
          IEnumerable<EcmaValue> try_() {
            yield return 2;
            unreachable++;
          }
          IEnumerable<EcmaValue> finally_() {
            yield return 3;
          }
          yield return Yield.TryFinally(try_, finally_);
          yield return 4;
        }
        EcmaValue E = Function.Construct();
        EcmaValue g = new GeneratorFunction(g_);
        EcmaValue iter = g.Call();
        VerifyIteratorResult(iter.Invoke("next"), false, 1);
        VerifyIteratorResult(iter.Invoke("next"), false, 2);
        VerifyIteratorResult(iter.Invoke("throw", E.Construct()), false, 3);

        That(() => iter.Invoke("next"), Throws.InstanceOf(E));
        That(unreachable, Is.Zero, "statement following `yield` not executed (following `throw`)");

        VerifyIteratorResult(iter.Invoke("next"), true);
        That(unreachable, Is.Zero, "statement following `yield` not executed (once \"completed\")");
      });
    }

    private (EcmaValue, EcmaValue) GetGeneratorAndPrototype() {
      EcmaValue g = new GeneratorFunction(EmptyGenerator);
      EcmaValue Generator = Object.Invoke("getPrototypeOf", g);
      EcmaValue GeneratorPrototype = Generator["prototype"];
      return (Generator, GeneratorPrototype);
    }
  }
}
