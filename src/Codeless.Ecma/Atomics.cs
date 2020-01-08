using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Atomics, Global = true)]
  public static class Atomics {
    private enum Operation { Add, Sub, And, Or, Xor, Exchange, CompareExchange }

    private static readonly int[] byteMask = new int[] { 0x000000ff, 0x0000ff00, 0x00ff0000, unchecked((int)0xff000000) };
    private static readonly int[] shortMask = new int[] { 0x0000ffff, 0, unchecked((int)0xffff0000) };

    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.Atomics;

    [IntrinsicMember]
    public static EcmaValue Load(TypedArray array, EcmaValue index) {
      return array.GetValueFromBuffer(ValidateArrayAndIndex(array, index));
    }

    [IntrinsicMember]
    public static EcmaValue Store(TypedArray array, EcmaValue index, EcmaValue value) {
      int pos = ValidateArrayAndIndex(array, index);
      value = value.ToInteger();
      array.SetValueInBuffer(pos, value);
      return value;
    }

    [IntrinsicMember]
    public static EcmaValue Add(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.Add, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue Sub(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.Sub, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue And(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.And, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue Or(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.Or, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue Xor(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.Xor, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue Exchange(TypedArray array, EcmaValue index, EcmaValue value) {
      return DoAtomicOperation(array, index, Operation.Exchange, value, default);
    }

    [IntrinsicMember]
    public static EcmaValue CompareExchange(TypedArray array, EcmaValue index, EcmaValue expected, EcmaValue replacement) {
      return DoAtomicOperation(array, index, Operation.CompareExchange, expected, replacement);
    }

    [IntrinsicMember]
    public static EcmaValue IsLockFree(int size) {
      return size == 4;
    }

    [IntrinsicMember]
    public static EcmaValue Wait(TypedArray array, EcmaValue index, EcmaValue value, EcmaValue timeout) {
      int pos = ValidateWaitableArrayAndIndex(array, index);
      int intValue = value.ToInt32();
      timeout = timeout.ToNumber();
      if (!RuntimeExecution.Current.CanSuspend) {
        throw new EcmaTypeErrorException("Atomics.wait cannot be called in this context");
      }
      int milliseconds = timeout.IsNaN || timeout > Int32.MaxValue ? -1 : timeout < 0 ? 0 : timeout.ToInt32();
      SharedArrayBuffer buffer = (SharedArrayBuffer)array.Buffer;
      return buffer.Wait(array.GetByteOffset(pos), intValue, milliseconds, out bool comparandEquals) ? "ok" : comparandEquals ? "timed-out" : "not-equal";
    }

    [IntrinsicMember]
    public static EcmaValue Notify(TypedArray array, EcmaValue index, EcmaValue count) {
      int pos = ValidateWaitableArrayAndIndex(array, index);
      count = count == default ? Int32.MaxValue : count.ToNumber();
      int countValue = count.IsNaN || count <= 0 ? 0 : count > Int32.MaxValue ? Int32.MaxValue : count.ToInt32();
      SharedArrayBuffer buffer = (SharedArrayBuffer)array.Buffer;
      return buffer.Notify(array.GetByteOffset(pos), countValue);
    }

    private static EcmaValue DoAtomicOperation(TypedArray array, EcmaValue index, Operation operation, EcmaValue value, EcmaValue replacement) {
      int i = ValidateArrayAndIndex(array, index);
      value = value.ToNumber();
      replacement = replacement.ToNumber();

      long offset = array.GetByteOffset(i);
      int[] buffer = array.Buffer.Int32Array;
      int bufferIndex = (int)(offset >> 2);
      int bytesPerElement = array.ElementSize;
      int operand = value.ToInt32();
      int bitShift = 0;
      int mask = -1;
      if (bytesPerElement != 4) {
        int byteShift = (int)(offset % 4);
        if (byteShift != 0 && !BitConverter.IsLittleEndian) {
          byteShift = 4 - byteShift;
        }
        bitShift = byteShift * 8;
        operand <<= bitShift;
        mask = (bytesPerElement == 2 ? shortMask : byteMask)[byteShift];
      } else {
        int result;
        switch (operation) {
          case Operation.CompareExchange:
            result = Interlocked.CompareExchange(ref buffer[bufferIndex], replacement.ToInt32(), operand);
            break;
          case Operation.Exchange:
            result = Interlocked.Exchange(ref buffer[bufferIndex], operand);
            break;
          case Operation.Add:
            result = unchecked(Interlocked.Add(ref buffer[bufferIndex], operand) - operand);
            break;
          case Operation.Sub:
            result = unchecked(Interlocked.Add(ref buffer[bufferIndex], -operand) + operand);
            break;
          default:
            goto fallback;
        }
        return array.ArrayKind == TypedArrayKind.Uint32Array ? (EcmaValue)(uint)result : result;
      }
      fallback:
      while (true) {
        int curValue = buffer[bufferIndex];
        int newValue = curValue;
        switch (operation) {
          case Operation.Or:
            newValue |= operand;
            break;
          case Operation.And:
            newValue &= operand;
            break;
          case Operation.Xor:
            newValue ^= operand;
            break;
          case Operation.Add:
            newValue = unchecked(curValue + operand);
            break;
          case Operation.Sub:
            newValue = unchecked(curValue - operand);
            break;
          case Operation.CompareExchange:
            newValue = (curValue & mask) == (operand & mask) ? replacement.ToInt32() << bitShift : curValue;
            break;
          case Operation.Exchange:
            newValue = operand;
            break;
        }
        if (bytesPerElement != 4) {
          newValue = (curValue & ~mask) | (newValue & mask);
        }
        if (curValue == Interlocked.CompareExchange(ref buffer[bufferIndex], newValue, curValue)) {
          int result = (curValue & mask) >> bitShift;
          switch (array.ArrayKind) {
            case TypedArrayKind.Int8Array:
              return (sbyte)result;
            case TypedArrayKind.Uint8Array:
              return (byte)result;
            case TypedArrayKind.Int16Array:
              return (short)result;
            case TypedArrayKind.Uint16Array:
              return (ushort)result;
            case TypedArrayKind.Uint32Array:
              return (uint)result;
          }
          return result;
        }
      }
    }

    private static int ValidateArrayAndIndex(TypedArray array, EcmaValue index) {
      switch (array.ArrayKind) {
        case TypedArrayKind.Int8Array:
        case TypedArrayKind.Int16Array:
        case TypedArrayKind.Int32Array:
        case TypedArrayKind.Uint8Array:
        case TypedArrayKind.Uint16Array:
        case TypedArrayKind.Uint32Array:
          break;
        default:
          throw new EcmaTypeErrorException(InternalString.Error.TypedArrayNotAtomic);
      }
      if (!array.Buffer.IsShared) {
        throw new EcmaTypeErrorException(InternalString.Error.TypedArrayNotAtomic);
      }
      long i = index.ToIndex();
      if (i < 0 || i >= array.Length) {
        throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, i);
      }
      return (int)i;
    }

    private static int ValidateWaitableArrayAndIndex(TypedArray array, EcmaValue index) {
      if (array.ArrayKind != TypedArrayKind.Int32Array) {
        throw new EcmaTypeErrorException(InternalString.Error.TypedArrayNotWaitable);
      }
      if (!array.Buffer.IsShared) {
        throw new EcmaTypeErrorException(InternalString.Error.TypedArrayNotWaitable);
      }
      long i = index.ToIndex();
      if (i < 0 || i >= array.Length) {
        throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, i);
      }
      return (int)i;
    }
  }
}
