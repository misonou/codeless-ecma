﻿using static Codeless.Ecma.Global;
using static Codeless.Ecma.Keywords;

namespace Codeless.Ecma.UnitTest.Harness {
  public static class ByteConversionValues {
    public static readonly EcmaValue NaNs = EcmaArray.Of(
      NaN,
      Number["NaN"],
      NaN * 0,
      0 / (EcmaValue)0,
      Infinity / Infinity,
      -(0 / (EcmaValue)0),
      Math.Invoke("pow", -1, 0.5),
      -Math.Invoke("pow", -1, 0.5),
      Number.Call(Undefined, "Not-a-Number")
    );

    public static readonly EcmaValue Values = EcmaArray.Of(
      127,              // 2 ** 7 - 1
      128,              // 2 ** 7
      32767,            // 2 ** 15 - 1
      32768,            // 2 ** 15
      2147483647,       // 2 ** 31 - 1
      2147483648,       // 2 ** 31
      255,              // 2 ** 8 - 1
      256,              // 2 ** 8
      65535,            // 2 ** 16 - 1
      65536,            // 2 ** 16
      4294967295,       // 2 ** 32 - 1
      4294967296,       // 2 ** 32
      9007199254740991, // 2 ** 53 - 1
      9007199254740992, // 2 ** 53
      1.1,
      0.1,
      0.5,
      0.50000001,
      0.6,
      0.7,
      Undefined,
      -1,
      -0,
      -0.1,
      -1.1,
      NaN,
      -127,             // - ( 2 ** 7 - 1 )
      -128,             // - ( 2 ** 7 )
      -32767,           // - ( 2 ** 15 - 1 )
      -32768,           // - ( 2 ** 15 )
      -2147483647,      // - ( 2 ** 31 - 1 )
      -2147483648,      // - ( 2 ** 31 )
      -255,             // - ( 2 ** 8 - 1 )
      -256,             // - ( 2 ** 8 )
      -65535,           // - ( 2 ** 16 - 1 )
      -65536,           // - ( 2 ** 16 )
      -4294967295,      // - ( 2 ** 32 - 1 )
      -4294967296,      // - ( 2 ** 32 )
      Infinity,
      -Infinity,
      0
    );

    public static readonly EcmaValue Expected = StaticHelper.CreateObject(new {
      Int8 = EcmaArray.Of(
        127,  // 127
        -128, // 128
        -1,   // 32767
        0,    // 32768
        -1,   // 2147483647
        0,    // 2147483648
        -1,   // 255
        0,    // 256
        -1,   // 65535
        0,    // 65536
        -1,   // 4294967295
        0,    // 4294967296
        -1,   // 9007199254740991
        0,    // 9007199254740992
        1,    // 1.1
        0,    // 0.1
        0,    // 0.5
        0,    // 0.50000001,
        0,    // 0.6
        0,    // 0.7
        0,    // undefined
        -1,   // -1
        0,    // -0
        0,    // -0.1
        -1,   // -1.1
        0,    // NaN
        -127, // -127
        -128, // -128
        1,    // -32767
        0,    // -32768
        1,    // -2147483647
        0,    // -2147483648
        1,    // -255
        0,    // -256
        1,    // -65535
        0,    // -65536
        1,    // -4294967295
        0,    // -4294967296
        0,    // Infinity
        0,    // -Infinity
        0
      ),
      Uint8 = EcmaArray.Of(
        127, // 127
        128, // 128
        255, // 32767
        0,   // 32768
        255, // 2147483647
        0,   // 2147483648
        255, // 255
        0,   // 256
        255, // 65535
        0,   // 65536
        255, // 4294967295
        0,   // 4294967296
        255, // 9007199254740991
        0,   // 9007199254740992
        1,   // 1.1
        0,   // 0.1
        0,   // 0.5
        0,   // 0.50000001,
        0,   // 0.6
        0,   // 0.7
        0,   // undefined
        255, // -1
        0,   // -0
        0,   // -0.1
        255, // -1.1
        0,   // NaN
        129, // -127
        128, // -128
        1,   // -32767
        0,   // -32768
        1,   // -2147483647
        0,   // -2147483648
        1,   // -255
        0,   // -256
        1,   // -65535
        0,   // -65536
        1,   // -4294967295
        0,   // -4294967296
        0,   // Infinity
        0,   // -Infinity
        0
      ),
      Uint8Clamped = EcmaArray.Of(
        127, // 127
        128, // 128
        255, // 32767
        255, // 32768
        255, // 2147483647
        255, // 2147483648
        255, // 255
        255, // 256
        255, // 65535
        255, // 65536
        255, // 4294967295
        255, // 4294967296
        255, // 9007199254740991
        255, // 9007199254740992
        1,   // 1.1,
        0,   // 0.1
        0,   // 0.5
        1,   // 0.50000001,
        1,   // 0.6
        1,   // 0.7
        0,   // undefined
        0,   // -1
        0,   // -0
        0,   // -0.1
        0,   // -1.1
        0,   // NaN
        0,   // -127
        0,   // -128
        0,   // -32767
        0,   // -32768
        0,   // -2147483647
        0,   // -2147483648
        0,   // -255
        0,   // -256
        0,   // -65535
        0,   // -65536
        0,   // -4294967295
        0,   // -4294967296
        255, // Infinity
        0,   // -Infinity
        0
      ),
      Int16 = EcmaArray.Of(
        127,    // 127
        128,    // 128
        32767,  // 32767
        -32768, // 32768
        -1,     // 2147483647
        0,      // 2147483648
        255,    // 255
        256,    // 256
        -1,     // 65535
        0,      // 65536
        -1,     // 4294967295
        0,      // 4294967296
        -1,     // 9007199254740991
        0,      // 9007199254740992
        1,      // 1.1
        0,      // 0.1
        0,      // 0.5
        0,      // 0.50000001,
        0,      // 0.6
        0,      // 0.7
        0,      // undefined
        -1,     // -1
        0,      // -0
        0,      // -0.1
        -1,     // -1.1
        0,      // NaN
        -127,   // -127
        -128,   // -128
        -32767, // -32767
        -32768, // -32768
        1,      // -2147483647
        0,      // -2147483648
        -255,   // -255
        -256,   // -256
        1,      // -65535
        0,      // -65536
        1,      // -4294967295
        0,      // -4294967296
        0,      // Infinity
        0,      // -Infinity
        0
      ),
      Uint16 = EcmaArray.Of(
        127,   // 127
        128,   // 128
        32767, // 32767
        32768, // 32768
        65535, // 2147483647
        0,     // 2147483648
        255,   // 255
        256,   // 256
        65535, // 65535
        0,     // 65536
        65535, // 4294967295
        0,     // 4294967296
        65535, // 9007199254740991
        0,     // 9007199254740992
        1,     // 1.1
        0,     // 0.1
        0,     // 0.5
        0,     // 0.50000001,
        0,     // 0.6
        0,     // 0.7
        0,     // undefined
        65535, // -1
        0,     // -0
        0,     // -0.1
        65535, // -1.1
        0,     // NaN
        65409, // -127
        65408, // -128
        32769, // -32767
        32768, // -32768
        1,     // -2147483647
        0,     // -2147483648
        65281, // -255
        65280, // -256
        1,     // -65535
        0,     // -65536
        1,     // -4294967295
        0,     // -4294967296
        0,     // Infinity
        0,     // -Infinity
        0
      ),
      Int32 = EcmaArray.Of(
        127,         // 127
        128,         // 128
        32767,       // 32767
        32768,       // 32768
        2147483647,  // 2147483647
        -2147483648, // 2147483648
        255,         // 255
        256,         // 256
        65535,       // 65535
        65536,       // 65536
        -1,          // 4294967295
        0,           // 4294967296
        -1,          // 9007199254740991
        0,           // 9007199254740992
        1,           // 1.1
        0,           // 0.1
        0,           // 0.5
        0,           // 0.50000001,
        0,           // 0.6
        0,           // 0.7
        0,           // undefined
        -1,          // -1
        0,           // -0
        0,           // -0.1
        -1,          // -1.1
        0,           // NaN
        -127,        // -127
        -128,        // -128
        -32767,      // -32767
        -32768,      // -32768
        -2147483647, // -2147483647
        -2147483648, // -2147483648
        -255,        // -255
        -256,        // -256
        -65535,      // -65535
        -65536,      // -65536
        1,           // -4294967295
        0,           // -4294967296
        0,           // Infinity
        0,           // -Infinity
        0
      ),
      Uint32 = EcmaArray.Of(
        127,        // 127
        128,        // 128
        32767,      // 32767
        32768,      // 32768
        2147483647, // 2147483647
        2147483648, // 2147483648
        255,        // 255
        256,        // 256
        65535,      // 65535
        65536,      // 65536
        4294967295, // 4294967295
        0,          // 4294967296
        4294967295, // 9007199254740991
        0,          // 9007199254740992
        1,          // 1.1
        0,          // 0.1
        0,          // 0.5
        0,          // 0.50000001,
        0,          // 0.6
        0,          // 0.7
        0,          // undefined
        4294967295, // -1
        0,          // -0
        0,          // -0.1
        4294967295, // -1.1
        0,          // NaN
        4294967169, // -127
        4294967168, // -128
        4294934529, // -32767
        4294934528, // -32768
        2147483649, // -2147483647
        2147483648, // -2147483648
        4294967041, // -255
        4294967040, // -256
        4294901761, // -65535
        4294901760, // -65536
        1,          // -4294967295
        0,          // -4294967296
        0,          // Infinity
        0,          // -Infinity
        0
      ),
      Float32 = EcmaArray.Of(
        127,                  // 127
        128,                  // 128
        32767,                // 32767
        32768,                // 32768
        2147483648,           // 2147483647
        2147483648,           // 2147483648
        255,                  // 255
        256,                  // 256
        65535,                // 65535
        65536,                // 65536
        4294967296,           // 4294967295
        4294967296,           // 4294967296
        9007199254740992,     // 9007199254740991
        9007199254740992,     // 9007199254740992
        1.100000023841858,    // 1.1
        0.10000000149011612,  // 0.1
        0.5,                  // 0.5
        0.5,                  // 0.50000001,
        0.6000000238418579,   // 0.6
        0.699999988079071,    // 0.7
        NaN,                  // undefined
        -1,                   // -1
        -0,                   // -0
        -0.10000000149011612, // -0.1
        -1.100000023841858,   // -1.1
        NaN,                  // NaN
        -127,                 // -127
        -128,                 // -128
        -32767,               // -32767
        -32768,               // -32768
        -2147483648,          // -2147483647
        -2147483648,          // -2147483648
        -255,                 // -255
        -256,                 // -256
        -65535,               // -65535
        -65536,               // -65536
        -4294967296,          // -4294967295
        -4294967296,          // -4294967296
        Infinity,             // Infinity
        -Infinity,            // -Infinity
        0
      ),
      Float64 = EcmaArray.Of(
        127,              // 127
        128,              // 128
        32767,            // 32767
        32768,            // 32768
        2147483647,       // 2147483647
        2147483648,       // 2147483648
        255,              // 255
        256,              // 256
        65535,            // 65535
        65536,            // 65536
        4294967295,       // 4294967295
        4294967296,       // 4294967296
        9007199254740991, // 9007199254740991
        9007199254740992, // 9007199254740992
        1.1,              // 1.1
        0.1,              // 0.1
        0.5,              // 0.5
        0.50000001,       // 0.50000001,
        0.6,              // 0.6
        0.7,              // 0.7
        NaN,              // undefined
        -1,               // -1
        -0,               // -0
        -0.1,             // -0.1
        -1.1,             // -1.1
        NaN,              // NaN
        -127,             // -127
        -128,             // -128
        -32767,           // -32767
        -32768,           // -32768
        -2147483647,      // -2147483647
        -2147483648,      // -2147483648
        -255,             // -255
        -256,             // -256
        -65535,           // -65535
        -65536,           // -65536
        -4294967295,      // -4294967295
        -4294967296,      // -4294967296
        Infinity,         // Infinity
        -Infinity,        // -Infinity
        0
      )
    });
  }
}
