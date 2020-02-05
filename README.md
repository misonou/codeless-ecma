## Installation

To install this library to your project, uun the following command in the Package Manager Console:

`PM> Install-Package Codeless.Ecma`

or follow instructions on the [page of this nuget package](https://www.nuget.org/packages/Codeless.Ecma).

## About this library

This library aims to provide APIs under the ECMAScript specification without a full JavaScript VM.

> As this library is in prelimiary stages, there are limitations and some features are still under 
  implementation or will be implemented in future releases.

It is suitable for producing consistent results on servers which are originated from 
simple JavaScript functions on such environments such as web browsers.

### Supported features

- Primitives (`BigInt` not supported in .NET 3.5)
- Built-in objects (see limitations below)
- Type coercion and operators are done on the `EcmaValue` type
- Automatic wrapping on native objects and native functions
- Execution threads and realms, including event loops

### Limitations

**Unavailable features**:
- String interpolation
- Script evaluation using `new Function` and `eval`
- Script parsing, interpreting, compiling and code generation
- Real script executions, e.g. closures
- Module loading and export bindings

**Limitations yet to be improved**:
- Floating point numbers may produce different serialization due to CLR `double` intrinsic
- Unicode handling in regular expressions
- Regular expressions features that are introduced lately

**Built-in objects yet or may to be implemented**:
- I18n features: `Intl` and `toLocaleString` implementations
- Proposed objects: `FinalizationGroup` and `WeakRef`
- Other common features such as `Crypto`, `File`

### Unit testing

Since the Test262 suite cannot be directly run against the library, tests written in JavaScript are 
ported from to C#. Most already-implemented APIs has gone through most of the defined tests in the
test suite.

## Examples

**Primitives**

```C#
EcmaValue x;
x = default;                    // undefined
x = EcmaValue.Undefined;        // undefined
x = EcmaValue.Null;             // null
x = true;
x = "string";
x = 0;
x = -0d;                        // -0
x = new Symbol("s");            // Symbol('s')
x = Literal.BigIntLiteral("1n") // 1n

x = EcmaValue.Infinity          // Infinity
x = EcmaValue.NegativeInfinity  // -Infinity
x = EcmaValue.NegativeZero      // -0
x = EcmaValue.NaN               // NaN
```

**Conversions on primitives**

Most primitive types can be implicitly converted to `EcmaValue`
as listed in the above example.

Converting back to native types can be done by
explicit cast or calling the corresponding methods.

```C#
// using cast
EcmaValue d = 1.1;
int i = (int)i;        // 1
string s = (string)d;  // "1.1"

// using method
int i1 = d.ToInt32();
```

***`ToString()` and `ToStringOrThrow()`***

-  `ToStringOrThrow()` is used whenever a value is coerced to string in JavaScript
-  `ToString()` overload does not throw in case of conversion failure for compatibility reason,
   since many diagnostics agent simply do `ToString()` to serialize 

```C#
EcmaValue sym = new Symbol("s");
string s2 = sym.ToStringOrThrow(); // throws
Console.WriteLine("{0}", sym);     // calls ToString(), writes "Symbol(s)"
```

**Operators**

```C#
EcmaValue x = 0, y = "str", z;
z = x + y;                                       // x + y    => "0str"
z = x.Pow(2)                                     // x ** 2
z = +y;                                          // +y       => NaN
z = !x;

// comparison and relational
z = x == y;                                      // x === y  => false
z = x.Equals("0", EcmaValueComparison.Abstract); // x == '0' => true
z = x < y;                                       // x < y
z = x.InstanceOf(Global.Number)                  // x instanceOf Number
z = y.In(x)                                      // y in x

// logical short-circuit
z = x || y;                                      // x || y   => "str"
z = x && y;                                      // x && y   => 0

// the following are bitwise operation between two EcmaValue
// since overload of bitwise operator in C# only allow
// second operand to be int or long
// these methods must be used for BigInt 
// since BigInt cannot be coerce to Int32
z = x.BitwiseAnd(y)                              // x & y
z = x.BitwiseOr(y)                               // x | y
z = x.LeftShift(y)                               // x << y
z = x.RightShift(y)                              // x >> y
z = x.LogicalRightShift(y)                       // x >>> y

// for..in and for..of
foreach (EcmaPropertyKey k in y);                // for (var k in y);
foreach (EcmaValue v in y.ForOf());              // for (var v of y);

// deconstruct arrays with rest
// [r0, r1, r2, ...r3] = [1, 2, 3, 4, 5];
var (r0, r1, r2, r3) = EcmaArray.Of(1, 2, 3, 4, 5);

// argument spreading
// arr.push(...r3);
arr.Invoke("push", new ArgumentList { Operator.Spread(r3) });
```

**Keywords**

```C#
// works great with using static
using static Codeless.Ecma.Keywords;

TypeOf(x);               // typeof x
Throw(x);                // throw x
Void(x);                 // void x
Null;                    // null
This["foo"];             // this.foo
Arguments["length"];     // arguments.length
Super.Construct();       // super()
Super.Invoke("foo");     // super.foo();
```

**Objects**

```C#
// {}
EcmaValue o1 = new EcmaObject();
o1["a"] = 0;
o1[0] = 1;
o1[new Symbol()] = 2;

// new Object();
Gloabl.Object.Construct();

using static Codeless.Ecma.Literal;
using static Codeless.Ecma.InteropServices.PropertyDefinitionType;

// { ... }
EcmaValue obj = new ObjectLiteral {
  // foo: true
  ["foo"] = true,
  // [Symbol.iterator] = function () { ... }
  [Symbol.Iterator] = FunctionLiteral(() => { /* ... */ }),
  // get bar() { ... }
  [Getter, "bar"] = FunctionLiteral(() => { /* ... */ }),
  // set bar(v) { ... }
  [Setter, "bar"] = FunctionLiteral((v) => { /* ... */ }),
  // baz() { ... }
  [Method, "baz"] = FunctionLiteral(() => { /* ... */ }),
  // ...obj
  [Spread] = new ObjectLiteral {
    ["ecma262"] = 1
  }
};
```

**Arrays**

```C#
// [1, 2, 3]
EcmaValue arr = EcmaArray.Of(1, 2, 3);
arr[5] = "foo";
arr["length"] == 6;

// new Array(5)
Global.Array.Construct(5);

// [1, , ...[1, 2, 3]]
EcmaValue arr = new ArrayLiteral {
  1,
  ArrayLiteral.Empty,
  Operator.Spread(EcmaArray.Of(1, 2, 3))
};
```

**Functions**

``` C# 
EcmaValue fn1 = Literal.FunctionLiteral(a => a + 1);

// Note that this the internal [[Call]] operation
// that is different from fn1.call(undefined, 1)
fn1.Call(default, 1);                   // 2

// invoking a function property
EcmaValue obj = new EcmaObject();
obj.Invoke("hasProperty", "a");         // obj.hasProperty('a')

// delegates are automatically wrapped
Func<EcmaValue, EcmaValue EcmaValue> add = (a, b) => a + b;
EcmaValue fn2 = add;
fn2.Call(default, 1, "1");              // "11" (1 is coerced to "1")

// parameters and return value are automatically coerced
Func<int, int, int> addInt = (a, b) => a + b;
EcmaValue fn3 = addInt;
fn3.Call(default, 1, "1");              // 2 ("1" is coerced to 1)
fn3.Call(default, 1, EcmaValue.NaN);    // 1 (NaN is coerced to 0)
```

**Classes**

```C#
using static Codeless.Ecma.Literal;
using static Codeless.Ecma.Keywords;
using static Codeless.Ecma.InteropServices.PropertyDefinitionType;

// class F { ... }
EcmaValue F = new ClassLiteral("F") {
  // constructor() { ... }
  ["constructor"] = FunctionLiteral(() => { }),
  // foo() { ... }
  [Method, "foo"] = FunctionLiteral(() => { }),
  // static bar = 1;
  [Static, "bar"] = 1,
  // static baz() { ... }
  [Static, Method, "baz"] = FunctionLiteral(() => { })
};
EcmaValue f = F.Construct();

// class G extends F { }
EcmaValue G = new ClassLiteral("G", Extends(F)) { };

// class P extends Promise { }
EcmaValue P = new ClassLiteral("P", Extends(Global.Promise)) { };
```

**Async functions**

Async function are supported through native async C# delegates.
See more in the section of [Event loops](#Event-loops).

> Before .Net Framework 4.5 in which the `await` keyword is introduced,
> delegates which has the return type of `Task` can also be used to 
> produce async function, which the result is wrapped by a `Promise` object.

```C#
EcmaValue incrementAsync = FunctionLiteral(async v => await v + 1);
EcmaValue promise = incrementAsync.Call(1);
promise.Invoke("then", FunctionLiteral(v => { /* v == 2 */ }));
```

**Generators**

Since C# does not support `yield` in anonymous lambda function, 
generators need to be declared a bit awkwardly.

```C#
IEnumerable<EcmaValue> f1() {
  yield return 100;
  yield return 200;
}
GeneratorFunction g1 = new GeneratorFunction(f1);
Generator iter = g1.Call().GetUnderlyingObject<Generator>();
iter.Next(); // { value: 100, done: false }
iter.Next(); // { value: 200, done: false }
iter.Next(); // { value: undefined, done: true }
```

***Nesting generators (`yield*`)***

```C#
IEnumerable<EcmaValue> f2() {
  yield return 10;
  yield return Yield.Many(g1.Call()); // yield* g1();
  yield return 20;
}
GeneratorFunction g2 = new GeneratorFunction(f2);
iter = g2.Call().GetUnderlyingObject<Generator>();
iter.Next(); // { value: 10, done: false }
iter.Next(); // { value: 100, done: false }
iter.Next(); // { value: 200, done: false }
iter.Next(); // { value: 20, done: false }
iter.Next(); // { value: undefined, done: true }
```

***Resumption value***

Since `yield return` is a statement in C#, resumption value 
(value returned by `yield` expression) can be accessed by a static property.

```C#
IEnumerable<EcmaValue> f() {
  // following two lines is equvalent to
  // yield (yield 100);
  yield return 100;
  yield return Yield.ResumeValue;
}
GeneratorFunction g = new GeneratorFunction(f);
Generator iter = g.Call().GetUnderlyingObject<Generator>();
iter.Next(2); // { value: 100, done: false }
iter.Next();  // { value: 2, done: false }
```

***Try-catch block***

C# does not support `yield return` statement in try-catch.
This is simulated by wrapping codes in `try`, `catch` and `finally`
blocks in separate inline function and then by `Yield.TryCatch` or
`Yield.TryFinally`.

```C#
IEnumerable<EcmaValue> f() {
  IEnumerable<EcmaValue> try_() {
    Keywords.Throw(Error.Construct());
    yield break;
  }
  IEnumerable<EcmaValue> catch_(EcmaValue ex) {
    yield return ex;
  }
  IEnumerable<EcmaValue> finally_() {
    yield return default;
  }
  yield return Yield.TryCatch(try_, catch_, finally_);
}
```

***Early return***

```C#
IEnumerable<EcmaValue> f() {
  yield return 100;
  yield return 200;
  yield return Yield.Done(300); // return 300;
  // unreachable even there is no yield break statement
}
GeneratorFunction g = new GeneratorFunction(f);
Generator iter = g.Call().GetUnderlyingObject<Generator>();
iter.Next(); // { value: 100, done: false }
iter.Next(); // { value: 200, done: false }
iter.Next(); // { value: 300, done: true }
```

***Async generators***

```C#
Promise promise = new Promise();
IEnumerable<EcmaValue> f() {
  yield return promise;
  yield return 10;
  yield return 20;
}
AsyncGeneratorFunction g = new AsyncGeneratorFunction(f);
AsyncGenerator iter = g.Call().GetUnderlyingObject<AsyncGenerator>();
Promise.All(new[] { iter.Next(), iter.Next(), iter.Next(), iter.Next() });
```

**Built-in objects**

Built-in objects can be consumed directly using derived classes or via `Global` class.

For example, to create a `Date` object equivalent, you can either use `EcmaDate` class direcly,
or to construct it dynamically with `Global.Date`.

> There is a slight different between the two methods that native API may 
> restrict argument count and type. Also the operation is non-observable, 
> and no stack frame created.

```C#
// construct directly, arguments must be long
EcmaDate d = new EcmaDate(2020, 0, 1);
d.SetDate(2);

// construct dynamically
EcmaValue v = Global.Date.Construct(2020, 0, 1);
v.Invoke("setDate", 2);

// concrete instance and wrapped value are interchangeable
EcmaValue dv = d;
dv.Invoke("setDate", 3);
d.GetDate();  // 3

EcmaDate vd = v.GetUnderlyingObject<EcmaDate>();
vd.SetDate(3);
v.Invoke("getDate");  // 3
```

**Auto-wrapping of native objects**

Any object that is not inherited from `RuntimeObject` is considered a native object.
Conversion happens when assigning these native objects to `EcmaValue` variable,
for example using `new EcmaValue(obj)`.

Some of them will be converted to built-in objects:

| C# type | ES type  | Underlying type |
| - | - | - |
| `DateTime` | `Date` | `EcmaDate` |
| `Task` | `Promise` | `Promise` |
| `Exception` | `Error` | `EcmaError` |
| `Delegate` | `Function` or `AsyncFunction` | `RuntimeFunction` |

Otherwise they will be wrapped.

```C#
class A {
  // all properties are defined if none of them 
  // decorated with `IntrinsicMemberAttribute`
  public int MyProperty { get; set; } = 1;

  // methods are only defined 
  // if decorated with `IntrinsicMemberAttribute`
  [IntrinsicMember]
  public string Say() {
    return "Hello!";
  }
}

A a = new A();

EcmaValue foo = new EcmaValue(a);
Console.WriteLine(foo["MyProperty"]); // 1

foo["MyProperty"] = "2";
Console.WriteLine(foo["MyProperty"]); // 2
Console.WriteLine(a.MyProperty);      // 2 (value is successfully set on object a)

foo["MyProperty"] = new Symbol();     // throws

// calling native method
foo.Invoke("Say");

// constructor is also supplied in prototype for completeness
// however it will throw on construct
foo["constructor"].Construct();
```

***`IList` and `IDctionary`***

```C#
Hashtable ht = new Hashtable();
EcmaValue bar = new EcmaValue(ht);
bar[0] = 1;
bar["foo"] = "bar";

// value is successully set on hashtable
Console.WriteLine(ht["foo"]); // bar

// returns key saved in hashtable
EcmaObject.GetOwnPropertyNames(bar); // [0, "foo"] 
```

**Event loops**

Async events such as fulfillment or rejection of a Promise, 
or scheduled timeout event are handled like in real JavaScript environment
by an event loop.

All scheduled event will be executed in order, and in the same thread,
 through the call of 
`RuntimeExecution.ContinueUntilEnd()`.

```C#
Promise p1 = new Promise((resolve, reject) => {
  Global.SetTimeout(FunctionLiteral(() => {
    /* ... */
    resolve(1);
  }), 1000);
});

p1.Invoke("then", FunctionLiteral(value => {
  /* ... */
}));

FunctionLiteral(async () => {
  var result = await p1;
  /* ... */
}).Call();

RuntimeExecution.ContinueUntilEnd();
```

Since the awaiter of `EcmaValue` use the same mechanism of Promise object,
the `await` keyword must not be in the same method where `ContinueUntilEnd()`
resides in. Otherwise the `await` statement will block the
execution of Promise resolving handlers.

```C#
Promise p1 = new Promise((resolve, reject) => {
  Global.SetTimeout(FunctionLiteral(() => {
    /* this will never run! */
    resolve(1);
  }), 1000);
});

await p1; // execution stop here waiting promise

// the following line will never run to resume
RuntimeExecution.ContinueUntilEnd();
```

**Realms**

Multiple realms within single-thread is commonly found in web browers
where each frame/iframe has its global and its own set of built-in objects,
belonging to the **Realm** of that frame.

```C#
RuntimeRealm other = new RuntimeRealm();
```

**Workers and multi-threading**

```C#
Worker worker = new Worker(() => {
  GlobalThis["onmessage"] = FunctionLiteral(msg => { /* ... */ });
  GlobalThis["postMessage"].Call("some text to parent");
});
worker["onmessage"] = FunctionLiteral(msg => { /* ... */ });
worker.PostMessage("some text to worker");
```

Underlying is the `CreateWorkerThread()` method.

```C#
RuntimeExecution.CreateWorkerThread(hostRealm => {
  // starter function that is running in another thread
  // hostRealm is the realm that created this thread
  RuntimeObject obj = new EcmaObject();

  // since all runtime object is intended to be single-thread
  // to send object and call function between thread 
  // use RuntimeObject.Clone() and RuntimeExecution.Enqueue()
  RuntimeObject clone = obj.Clone(hostRealm);
  hostRealm.ExecutionContext.Enqueue(function () {
    // call some function in host thread
  });
});
// main thread resume once the worker thread starter function
// has finish execution
```
