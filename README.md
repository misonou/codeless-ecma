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

- Primitives (except `BigInt`)
- Built-in objects (see limitations below)
- Type coercion and operators are done on the `EcmaValue` type
- Automatic wrapping on native objects and native functions
- Execution threads and realms, including event loops

### Limitations

**Intrinsic limitations**:
- 64-bit floating point numbers may produce different serialization due to CLR `double` intrinsic
- Unicode handling in regular expressions

**Built-in objects**:
- `new Function` and `eval` are not available
- Regular expressions features that are introduced lately
- Not yet implemented: `BigInt`, generators and async functions

**Features may or may not be implemented**:
- Script parsing, interpreting, compiling and code generation
- Real script executions, e.g. closures
- Module loading and export bindings

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

x = EcmaValue.Infinity          // Infinity
x = EcmaValue.NegativeInfinity  // -Infinity
x = EcmaValue.NegativeZero      // -0
x = EcmaValue.NaN               // NaN
```

**Objects**

```C#
EcmaValue o1 = new EcmaObject();        // o1 = {}
o1["a"] = 0;                            // o1.a = 2
o1[0] = 1;                              // o1[0] = 1
o1[new Symbol()] = 2;                   // o1[Symbol()] = 2

EcmaValue arr = EcmaArray.Of(1, 2, 3);  // arr = [1, 2, 3]
arr[5] = "foo";
arr["length"] == 6;
```

**Functions**

``` C#
EcmaValue fn1 = RuntimeFunction.Create(a => a + 1); // fn1 = function (a) { return a + 1 }
fn1.Call(EcmaValue.Undefined, 1);                   // fn1(1) => 2
                                                    // Note that this the internal [[Call]] operation
                                                    // that is different from fn1.call(undefined, 1)

EcmaValue obj = new EcmaObject();
obj.Invoke("hasProperty", "a");                     // obj.hasProperty('a')

// delegates are automatically wrapped
Func<EcmaValue, EcmaValue EcmaValue> add = (a, b) => a + b;
EcmaValue fn2 = add;
fn2.Call(EcmaValue.Undefined, 1, "1");              // "11" (1 is coerced to "1")

// parameters and return value are automatically coerced
Func<int, int, int> addInt = (a, b) => a + b;
EcmaValue fn3 = addInt;
fn3.Call(EcmaValue.Undefined, 1, "1");              // 2 ("1" is coerced to 1)
fn3.Call(EcmaValue.Undefined, 1, EcmaValue.NaN);    // 1 (NaN is coerced to 0, as ToInt32 in `NaN | 0`)
```

**Operations**

```C#
EcmaValue x = 0, y = "str", z;
z = x + y;                                       // x + y    => "0str"
z = x == y;                                      // x === y  => false
z = x || y;                                      // x || y   => "str"
z = x && y;                                      // x && y   => 0
z = x.Equals("0", EcmaValueComparison.Abstract); // x == '0' => true
z = +y;                                          // +y       => NaN

foreach (EcmaPropertyKey k in y);                // for (var k in y);
foreach (EcmaValue v in y.ForOf());              // for (var v of y);
```

**Conversions**

```C#
// most primitive types can be implicitly converted to `EcmaValue`
// as listed in the above example

// convert back to native types using cast
EcmaValue d = 1.1;
int i = (int)i;                    // 1
string s = (string)d;              // "1.1" (calls ToStringOrThrow(), see note below)

// convert back to native types using method
int i1 = d.ToInt32();
string s1 = d.ToStringOrThrow();   // notice that ToStringOrThrow() is used
                                   // for on-the-spec behavior
                                   // because ToString() overload does not throw
                                   // in case of conversion failure for compat
EcmaValue sym = new Symbol("s");
string s2 = sym.ToStringOrThrow(); // throws
Console.WriteLine("{0}", sym);     // calls ToString(), writes "Symbol(s)"
```

**Keywords**

```C#
// works great with using static
using static Codeless.Ecma.Keywords;

TypeOf(x);                            // typeof x
Throw(x);                             // throw x
Void(x);                              // void x
a.Instanceof(A);                      // a instanceof A
Null;                                 // null

This["foo"];                          // this.foo
Arguments["length"];                  // arguments.length
Super.Construct();                    // super()
Super.Invoke("foo");                  // super.foo();
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
d.GetDate();             // 3

EcmaDate vd = v.GetUnderlyingObject<EcmaDate>();
vd.SetDate(3);
v.Invoke("getDate");     // 3
```

**Native objects**

```C#
class A {
  // all properties are defined if none of them 
  // decorated with `InstrinsicMemberAttribute`
  public int MyProperty { get; set; } = 1;

  // methods are only defined 
  // if decorated with `InstrinsicMemberAttribute`
  [InstrinsicMember]
  public string Say() {
    return "Hello!";
  }
}

A a = new A();

// objects that is not derived from Codeless.Ecma.Runtime.RuntimeObject
// are wrapped with reflection
EcmaValue foo = RuntimeRealm.Current.GetRuntimeObject(a);
foo["MyProperty"];                   // 1
foo["MyProperty"] = "2";             // "2" is coerced to 2
foo["MyProperty"];                   // 2
a.MyProperty;                        // 2 (value is successfully set on object a)

// properties are defined as getters and setters
// improper values may throw
foo["MyProperty"] = new Symbol();    // throws

foo.Invoke("Say");                   // "Hello!"

// IList and IDictionary are handled specially
Hashtable ht = new Hashtable();
EcmaValue bar = RuntimeRealm.Current.GetRuntimeObject(ht);
bar[0] = 1;
bar["foo"] = "bar";
ht["foo"];                           // "bar" (value is successully set on hashtable)
EcmaObject.GetOwnPropertyNames(bar); // [0, "foo"] 
```
