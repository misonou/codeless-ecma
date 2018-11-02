using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public enum WellKnownObject {
    Global = 1,
    ObjectConstructor,
    ObjectPrototype,
    SymbolConstructor,
    SymbolPrototype,
    StringConstructor,
    StringPrototype,
    BooleanConstructor,
    BooleanPrototype,
    NumberConstructor,
    NumberPrototype,
    FunctionConstructor,
    FunctionPrototype,
    DateConstructor,
    DatePrototype,
    ArrayConstructor,
    ArrayPrototype,
    ErrorConstructor,
    ErrorPrototype,
    RegExp,
    RegExpPrototype,
    TypeError,
    TypeErrorPrototype,
    RangeError,
    RangeErrorPrototype,
    SyntaxError,
    SyntaxErrorPrototype,
    ReferenceError,
    ReferenceErrorPrototype,
    UriError,
    UriErrorPrototype,
    EvalError,
    EvalErrorPrototype,
    Set,
    SetPrototype,
    WeakSet,
    WeakSetPrototype,
    Map,
    MapPrototype,
    WeakMap,
    WeakMapPrototype,
    Json,
    Math,
    Reflect,
    Proxy,
    Atomics,
    MaxValue
  }
}
