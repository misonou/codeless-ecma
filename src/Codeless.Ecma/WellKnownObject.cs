﻿using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [IntrinsicObjectEnum((int)MaxValue)]
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
    ProxyConstructor,
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
    RegExpStringIteratorPrototype,
    AsyncIteratorPrototype,
    GeneratorFunction,
    GeneratorPrototype,
    AsyncFunction,
    DataView,
    DataViewPrototype,
    SharedArrayBuffer,
    SharedArrayBufferPrototype,
    StringIteratorPrototype,
    ArrayBuffer,
    ArrayBufferPrototype,
    TypedArray,
    TypedArrayPrototype,
    Float32Array,
    Float32ArrayPrototype,
    Float64Array,
    Float64ArrayPrototype,
    Int8Array,
    Int8ArrayPrototype,
    Int16Array,
    Int16ArrayPrototype,
    Int32Array,
    Int32ArrayPrototype,
    Generator,
    AsyncFunctionPrototype,
    AsyncGeneratorFunction,
    AsyncGenerator,
    AsyncGeneratorPrototype,
    AsyncFromSyncIteratorPrototype,
    BigIntConstructor,
    BigIntPrototype,
    BigInt64Array,
    BigInt64ArrayPrototype,
    BigUint64Array,
    BigUint64ArrayPrototype,
    ThrowTypeError,
    MaxValue
  }
}
