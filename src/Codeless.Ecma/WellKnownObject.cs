﻿using Codeless.Ecma.Runtime;
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
    RegExpConstructor,
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
    SetConstructor,
    SetPrototype,
    WeakSetConstructor,
    WeakSetPrototype,
    MapConstructor,
    MapPrototype,
    WeakMapConstructor,
    WeakMapPrototype,
    Json,
    Math,
    Reflect,
    Proxy,
    Atomics,
    PromiseConstructor,
    PromisePrototype,
    Uint8Array,
    Uint8ArrayPrototype,
    Uint8ClampedArray,
    Uint8ClampedArrayPrototype,
    Uint16Array,
    Uint16ArrayPrototype,
    Uint32Array,
    Uint32ArrayPrototype,
    IteratorPrototype,
    MapIteratorPrototype,
    SetIteratorPrototype,
    ArrayIteratorPrototype,
    AsyncIteratorPrototype,
    GeneratorFunction,
    GeneratorPrototype,
    AsyncFunction,
    DataView,
    DataViewPrototype,
    SharedArrayBuffer,
    SharedArrayBufferPrototype,
    MaxValue
  }
}
