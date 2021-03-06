using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Global)]
  public static class Global {
    [IntrinsicMember("Infinity", EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Infinity = EcmaValue.Infinity;

    [IntrinsicMember("NaN", EcmaPropertyAttributes.None)]
    public static readonly EcmaValue NaN = EcmaValue.NaN;

    [IntrinsicMember("undefined", EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Undefined = EcmaValue.Undefined;

    public static RuntimeObject GlobalThis => (RuntimeObject)WellKnownObject.Global;

    public static RuntimeObject Atomics => (RuntimeObject)WellKnownObject.Atomics;

    public static RuntimeObject Json => (RuntimeObject)WellKnownObject.Json;

    public static RuntimeObject Math => (RuntimeObject)WellKnownObject.Math;

    public static RuntimeObject Reflect => (RuntimeObject)WellKnownObject.Reflect;

    public static RuntimeFunction Array => (RuntimeFunction)WellKnownObject.ArrayConstructor;

    public static RuntimeFunction Boolean => (RuntimeFunction)WellKnownObject.BooleanConstructor;

    public static RuntimeFunction Date => (RuntimeFunction)WellKnownObject.DateConstructor;

    public static RuntimeFunction Error => (RuntimeFunction)WellKnownObject.ErrorConstructor;

    public static RuntimeFunction Function => (RuntimeFunction)WellKnownObject.FunctionConstructor;

    public static RuntimeFunction Map => (RuntimeFunction)WellKnownObject.MapConstructor;

    public static RuntimeFunction Number => (RuntimeFunction)WellKnownObject.NumberConstructor;

    public static RuntimeFunction Object => (RuntimeFunction)WellKnownObject.ObjectConstructor;

    public static RuntimeFunction Promise => (RuntimeFunction)WellKnownObject.PromiseConstructor;

    public static RuntimeFunction Proxy => (RuntimeFunction)WellKnownObject.ProxyConstructor;

    public static RuntimeFunction RegExp => (RuntimeFunction)WellKnownObject.RegExpConstructor;

    public static RuntimeFunction Set => (RuntimeFunction)WellKnownObject.SetConstructor;

    public static RuntimeFunction String => (RuntimeFunction)WellKnownObject.StringConstructor;

    public static RuntimeFunction Symbol => (RuntimeFunction)WellKnownObject.SymbolConstructor;

    public static RuntimeFunction WeakMap => (RuntimeFunction)WellKnownObject.WeakMapConstructor;

    public static RuntimeFunction WeakSet => (RuntimeFunction)WellKnownObject.WeakSetConstructor;

    public static RuntimeFunction EvalError => (RuntimeFunction)WellKnownObject.EvalError;

    public static RuntimeFunction RangeError => (RuntimeFunction)WellKnownObject.RangeError;

    public static RuntimeFunction ReferenceError => (RuntimeFunction)WellKnownObject.ReferenceError;

    public static RuntimeFunction SyntaxError => (RuntimeFunction)WellKnownObject.SyntaxError;

    public static RuntimeFunction TypeError => (RuntimeFunction)WellKnownObject.TypeError;

    public static RuntimeFunction UriError => (RuntimeFunction)WellKnownObject.UriError;

    public static RuntimeFunction ArrayBuffer => (RuntimeFunction)WellKnownObject.ArrayBuffer;

    public static RuntimeFunction SharedArrayBuffer => (RuntimeFunction)WellKnownObject.SharedArrayBuffer;

    public static RuntimeFunction DataView => (RuntimeFunction)WellKnownObject.DataView;

    public static RuntimeFunction Float32Array => (RuntimeFunction)WellKnownObject.Float32Array;

    public static RuntimeFunction Float64Array => (RuntimeFunction)WellKnownObject.Float64Array;

    public static RuntimeFunction Int8Array => (RuntimeFunction)WellKnownObject.Int8Array;

    public static RuntimeFunction Int16Array => (RuntimeFunction)WellKnownObject.Int16Array;

    public static RuntimeFunction Int32Array => (RuntimeFunction)WellKnownObject.Int32Array;

    public static RuntimeFunction Uint8Array => (RuntimeFunction)WellKnownObject.Uint8Array;

    public static RuntimeFunction Uint8ClampedArray => (RuntimeFunction)WellKnownObject.Uint8ClampedArray;

    public static RuntimeFunction Uint16Array => (RuntimeFunction)WellKnownObject.Uint16Array;

    public static RuntimeFunction Uint32Array => (RuntimeFunction)WellKnownObject.Uint32Array;

#if BIGINTEGER
    public static RuntimeFunction BigInt => (RuntimeFunction)WellKnownObject.BigIntConstructor;

    public static RuntimeFunction BigInt64Array => (RuntimeFunction)WellKnownObject.BigInt64Array;

    public static RuntimeFunction BigUint64Array => (RuntimeFunction)WellKnownObject.BigUint64Array;
#endif

    [IntrinsicMember]
    public static EcmaValue IsFinite(EcmaValue value) {
      return value.ToNumber().IsFinite;
    }

    [IntrinsicMember]
    public static EcmaValue IsNaN(EcmaValue value) {
      return value.ToNumber().IsNaN;
    }

    public static EcmaValue ParseInt(EcmaValue str, EcmaValue radix) {
      string inputString = str.ToStringOrThrow();
      return EcmaValueUtility.ParseIntInternal(inputString, radix.ToInt32(), true);
    }

    public static EcmaValue ParseFloat(EcmaValue str) {
      string inputString = str.ToStringOrThrow();
      return EcmaValueUtility.ParseFloatInternal(inputString, true);
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURI(EcmaValue str) {
      string inputString = str.ToStringOrThrow();
      CheckLoneSurrogate(inputString);
      return Uri.EscapeUriString(inputString);
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURI(EcmaValue str) {
      return Decode(str.ToStringOrThrow(), ";/?:@&=+$,#", true, true);
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURIComponent(EcmaValue str) {
      string inputString = str.ToStringOrThrow();
      CheckLoneSurrogate(inputString);
      return Uri.EscapeDataString(inputString);
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURIComponent(EcmaValue str) {
      return Decode(str.ToStringOrThrow(), "", true, true);
    }

    [IntrinsicMember]
    public static EcmaValue Escape(EcmaValue str) {
      StringBuilder sb = new StringBuilder();
      foreach (char ch in str.ToStringOrThrow()) {
        if (ch == '*' || ch == '+' || ch == '-' || ch == '.' || ch == '/' || ch == '@' || ch == '_' || IsAlphaNumericCharPoint(ch)) {
          sb.Append(ch);
        } else if (ch < 256) {
          sb.AppendFormat("%{0:X2}", (int)ch);
        } else {
          sb.AppendFormat("%u{0:X4}", (int)ch);
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Unescape(EcmaValue str) {
      return Decode(str.ToStringOrThrow(), "", false, false);
    }

    [IntrinsicMember]
    public static EcmaValue SetTimeout(EcmaValue fn, EcmaValue milliseconds, params EcmaValue[] args) {
      int timeout = milliseconds == default ? 0 : milliseconds.ToNumber().ToInt32();
      if (timeout > 0) {
        timeout = 0;
      }
      if (timeout >= 0) {
        timeout = System.Math.Max(4, timeout);
      }
      RuntimeExecutionHandle handle = RuntimeExecution.Enqueue(() => fn.Call(Undefined, args), timeout, RuntimeExecutionFlags.Cancellable);
      return handle.Id;
    }

    [IntrinsicMember]
    public static EcmaValue SetInterval(EcmaValue fn, EcmaValue milliseconds, params EcmaValue[] args) {
      int timeout = milliseconds == default ? 0 : milliseconds.ToNumber().ToInt32();
      if (timeout > 0) {
        timeout = 0;
      }
      if (timeout >= 0) {
        timeout = System.Math.Max(4, timeout);
      }
      RuntimeExecutionHandle handle = RuntimeExecution.Enqueue(() => fn.Call(Undefined, args), timeout, RuntimeExecutionFlags.Cancellable | RuntimeExecutionFlags.Recurring);
      return handle.Id;
    }

    [IntrinsicMember]
    public static EcmaValue ClearTimeout(EcmaValue id) {
      RuntimeExecutionHandle handle = new RuntimeExecutionHandle(id == default ? 0 : id.ToNumber().ToInt32());
      RuntimeExecution.Cancel(handle);
      return EcmaValue.Undefined;
    }

    [IntrinsicMember]
    public static EcmaValue ClearInterval(EcmaValue id) {
      RuntimeExecutionHandle handle = new RuntimeExecutionHandle(id == default ? 0 : id.ToNumber().ToInt32());
      RuntimeExecution.Cancel(handle);
      return EcmaValue.Undefined;
    }

    private static void CheckLoneSurrogate(string inputString) {
      for (int i = 0, len = inputString.Length; i < len; i++) {
        if (Char.IsSurrogate(inputString, i)) {
          if (!Char.IsSurrogatePair(inputString, i)) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          i++;
        }
      }
    }

    [EcmaSpecification("Decode", EcmaSpecificationKind.RuntimeSemantics)]
    private static unsafe string Decode(string encoded, string undecoded, bool utf8, bool throwOnInvalid) {
      StringBuilder sb = new StringBuilder();
      byte* bytes = stackalloc byte[4];
      int i = -1;
      int octetLen = 0;
      int octet = 0;
      int ch;

      while (true) {
        nextChar:
        if (!ReadChar(encoded, &i, &ch, sb)) {
          if (octet != 0 && throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          break;
        }
        if (ch != '%') {
          if (octet != 0 && throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          continue;
        }
        if (!ReadChar(encoded, &i, &ch, sb)) {
          if (throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          break;
        }

        if (!utf8 && ch == 'u') {
          int ch2 = 0;
          for (int j = 0; j < 4; j++) {
            if (!ReadChar(encoded, &i, &ch, sb)) {
              if (throwOnInvalid) {
                throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
              }
              goto end;
            }
            ch = HexDigitToInt(ch);
            if (ch < 0) {
              goto nextChar;
            }
            ch2 = (ch2 << 4) + ch;
          }
          sb.Length -= 6;
          sb.Append((char)ch2);
          continue;
        }
        ch = HexDigitToInt(ch);
        if (ch < 0) {
          if (throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          continue;
        }

        int b = ch << 4;
        if (!ReadChar(encoded, &i, &ch, sb)) {
          if (throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          break;
        }
        ch = HexDigitToInt(ch);
        if (ch < 0) {
          if (throwOnInvalid) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          continue;
        }

        b += ch;
        if (utf8) {
          if (b >= 0xF0) {
            if (octet != 0) {
              throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
            }
            octet = octetLen = 3;
            bytes[0] = (byte)b;
            continue;
          }
          if (b >= 0xE0) {
            if (octet != 0) {
              throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
            }
            octet = octetLen = 2;
            bytes[0] = (byte)b;
            continue;
          }
          if (b >= 0xC0) {
            if (octet != 0 || b < 0xC2) {
              throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
            }
            octet = octetLen = 1;
            bytes[0] = (byte)b;
            continue;
          }
          if (b >= 0x80) {
            if (octet == 0 || (octet == 2 && bytes[0] == 0xE0 && b < 0xA0)) {
              throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
            }
            bytes[octetLen - (--octet)] = (byte)b;
            if (octet == 0) {
              switch (octetLen) {
                case 3:
                  sb.Length -= 12;
                  sb.Append((char)(0xd800 | (((bytes[0] & 0x07) << 8) + ((bytes[1] & 0x3F) << 2) + ((bytes[2] & 0x3F) >> 4) - 0x40)));
                  sb.Append((char)(0xdc00 | (((bytes[2] & 0x0F) << 6) + (bytes[3] & 0x3F))));
                  break;
                case 2:
                  char c = (char)(((bytes[0] & 0x0F) << 12) + ((bytes[1] & 0x3F) << 6) + (bytes[2] & 0x3F));
                  if (Char.IsSurrogate(c)) {
                    throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
                  }
                  sb.Length -= 9;
                  sb.Append(c);
                  break;
                case 1:
                  sb.Length -= 6;
                  sb.Append((char)(((bytes[0] & 0x1F) << 6) + (bytes[1] & 0x3F)));
                  break;
              }
            }
            continue;
          }
          if (octet != 0) {
            throw new EcmaUriErrorException(InternalString.Error.MalformedURI);
          }
          bytes[0] = (byte)b;
        }

        if (undecoded.IndexOf((char)b) < 0) {
          sb.Length -= 3;
          sb.Append((char)b);
        }
      }
      end:
      return sb.ToString();
    }

    private static unsafe bool ReadChar(string encoded, int* index, int* ch, StringBuilder sb) {
      int i = ++*index;
      if (i >= encoded.Length) {
        return false;
      }
      char c = encoded[i];
      *ch = c;
      sb.Append(c);
      return true;
    }

    private static bool IsAlphaNumericCharPoint(int ch) {
      return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
    }

    private static int HexDigitToInt(int ch) {
      if (ch >= '0' && ch <= '9') {
        return ch - '0';
      }
      if (ch >= 'A' && ch <= 'F') {
        return ch - 'A' + 10;
      }
      if (ch >= 'a' && ch <= 'f') {
        return ch - 'a' + 10;
      }
      return -1;
    }
  }
}
