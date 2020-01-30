using Codeless.Ecma.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.UnitTest.Assert;
using static Codeless.Ecma.UnitTest.StaticHelper;

namespace Codeless.Ecma.UnitTest.Tests {
  public class AsyncFromSyncIteratorPrototype : TestBase {
    [Test]
    public void Next() {
      It("should return a promise for an IteratorResult object", () => {
        EcmaValue g = new GeneratorFunction(EmptyGenerator);
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(g.Call());
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue p = g1.Call().Invoke("next");
        p.Invoke("then", Intercept(result => {
          VerifyIteratorResult(result, true);
        }));
        That(p, Is.InstanceOf(Global.Promise));
        VerifyPromiseSettled();
      });

      It("should reject promise if getter `done` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(
                ("done", get: () => Keywords.Throw(thrownError), set: null),
                ("value", get: () => 1, set: null)
              );
            })
          });
        })));

        IEnumerable<EcmaValue> g_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        g.Call().Invoke("next").Invoke("then", UnexpectedFulfill, Intercept(error => {
          That(error, Is.EqualTo(thrownError));
        }));
        VerifyPromiseSettled();
      });

      It("should reject promise if getter `value` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = CreateObject((Symbol.Iterator, FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(
                ("value", get: () => Keywords.Throw(thrownError), set: null),
                ("done", get: () => false, set: null)
              );
            })
          });
        })));

        IEnumerable<EcmaValue> g_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g = new AsyncGeneratorFunction(g_);
        g.Call().Invoke("next").Invoke("then", UnexpectedFulfill, Intercept(error => {
          That(error, Is.EqualTo(thrownError));
        }));
        VerifyPromiseSettled();
      });

      It("should reject promise if sync iterator next() function returns an aburpt completion", () => {
        EcmaValue thrownError = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          yield return Keywords.Throw(thrownError);
        }
        EcmaValue g = new GeneratorFunction(g_);
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(g.Call());
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        g1.Call().Invoke("next").Invoke("then", UnexpectedFulfill, Intercept(error => {
          That(error, Is.EqualTo(thrownError));
        }));
        VerifyPromiseSettled();
      });

      It("should unwrap a Promise value return by the sync iterator", () => {
        EcmaValue thrownError = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          yield return Promise.Resolve(1);
        }
        EcmaValue g = new GeneratorFunction(g_);
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(g.Call());
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        g1.Call().Invoke("next").Invoke("then", Intercept(result => {
          VerifyIteratorResult(result, false, 1);
        }));
        VerifyPromiseSettled();
      });
    }

    [Test]
    public void Return() {
      It("should return a iterator result object when built-in sync throw is called", () => {
        IEnumerable<EcmaValue> g_() {
          yield return 42;
          Keywords.Throw(Error.Construct());
          yield return 43;
        }
        EcmaValue g = new GeneratorFunction(g_);
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(g.Call());
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        EcmaValue val = "some specific return value";
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return", val).Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, val);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return value undefined if sync `return` is undefined", () => {
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if `return` does not return an Object", () => {
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @return = FunctionLiteral(() => 1)
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.InstanceOf(Global.TypeError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if `return` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @return = FunctionLiteral(() => Keywords.Throw(thrownError))
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter of `return` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(
            ("next", get: () => FunctionLiteral(() => CreateObject(new { value = 1, done = false })), set: null),
            ("return", get: () => Keywords.Throw(thrownError), set: null)
          );
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter `done` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @return = FunctionLiteral(() => {
              return CreateObject(
                ("done", get: () => Keywords.Throw(thrownError), set: null),
                ("value", get: () => 1, set: null)
              );
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter `value` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @return = FunctionLiteral(() => {
              return CreateObject(
                ("done", get: () => false, set: null),
                ("value", get: () => Keywords.Throw(thrownError), set: null)
              );
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should unwrap a Promise value return by the sync iterator", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @return = FunctionLiteral(() => {
              return CreateObject(new { value = Promise.Resolve(42), done = true });
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("return").Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, 42);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });
    }

    [Test]
    public void Throw() {
      It("should return a iterator result object when built-in sync throw is called", () => {
        EcmaValue thrownError = Error.Construct();
        IEnumerable<EcmaValue> g_() {
          yield return 42;
          Keywords.Throw(Error.Construct());
          yield return 43;
        }
        EcmaValue g = new GeneratorFunction(g_);
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(g.Call());
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw", thrownError).Invoke("then", UnexpectedFulfill, Intercept(r2 => {
            That(r2, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("return return rejected promise if `throw` is undefined", () => {
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", UnexpectedFulfill, Intercept(r2 => {
            That(r2, Is.Undefined);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if `throw` does not return an Object", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @throw = FunctionLiteral(() => 1)
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw", thrownError).Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.InstanceOf(Global.TypeError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if `throw` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @throw = FunctionLiteral(() => Keywords.Throw(thrownError))
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter of `throw` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(
            ("next", get: () => {
              return FunctionLiteral(() => {
                return CreateObject(new { value = 1, done = false });
              });
            }, set: null),
            ("throw", get: () => Keywords.Throw(thrownError), set: null)
          );
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter `value` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @throw = FunctionLiteral(() => {
              return CreateObject(
                ("value", get: () => Keywords.Throw(thrownError), set: null),
                ("done", get: () => false, set: null)
              );
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should return rejected promise if getter `done` abrupt completes", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @throw = FunctionLiteral(() => {
              return CreateObject(
                ("value", get: () => 1, set: null),
                ("done", get: () => Keywords.Throw(thrownError), set: null)
              );
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", UnexpectedFulfill, Intercept(err => {
            That(err, Is.EqualTo(thrownError));
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });

      It("should unwrap a Promise value return by the sync iterator", () => {
        EcmaValue thrownError = Error.Construct();
        EcmaValue obj = new EcmaObject();
        obj[Symbol.Iterator] = FunctionLiteral(() => {
          return CreateObject(new {
            next = FunctionLiteral(() => {
              return CreateObject(new { value = 1, done = false });
            }),
            @throw = FunctionLiteral(() => {
              return CreateObject(new { value = Promise.Resolve(42), done = true });
            })
          });
        });
        IEnumerable<EcmaValue> g1_() {
          yield return Yield.Many(obj);
        }
        EcmaValue g1 = new AsyncGeneratorFunction(g1_);
        EcmaValue iter = g1.Call();
        iter.Invoke("next").Invoke("then", Intercept(r1 => {
          iter.Invoke("throw").Invoke("then", Intercept(r2 => {
            VerifyIteratorResult(r2, true, 42);
            iter.Invoke("next").Invoke("then", Intercept(r3 => {
              VerifyIteratorResult(r3, true);
            }));
          }));
        }));
        VerifyPromiseSettled(count: 3);
      });
    }
  }
}
