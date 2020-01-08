using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public enum DataViewEndianness {
    BigEndian,
    LittleEndian
  }

  public class DataView : RuntimeObject, IArrayBufferView {
    public DataView()
      : base(WellKnownObject.DataViewPrototype) { }

    public DataView(ArrayBuffer buffer, long offset, long length)
      : this() {
      Init(buffer, offset, length);
    }

    public ArrayBuffer Buffer { get; private set; }
    public long ByteOffset { get; private set; }
    public long ByteLength { get; private set; }

    internal void Init(ArrayBuffer buffer, long offset, long length) {
      Guard.ArgumentNotNull(buffer, "buffer");
      if (offset < 0 || offset > buffer.ByteLength) {
        throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, offset);
      }
      if (length < 0 || offset + length > buffer.ByteLength) {
        throw new EcmaRangeErrorException(InternalString.Error.DataViewInvalidLength, length);
      }
      if (buffer.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
      this.Buffer = buffer;
      this.ByteOffset = offset;
      this.ByteLength = length;
    }

    public EcmaValue GetFloat32(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(float));
      return CheckAndSwap(this.Buffer.GetFloat(index + ByteOffset), isLittleEndian);
    }

    public EcmaValue GetFloat64(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(double));
      return CheckAndSwap(this.Buffer.GetDouble(index + ByteOffset), isLittleEndian);
    }

    public EcmaValue GetInt8(long index) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(sbyte));
      return this.Buffer.GetSByte(index + ByteOffset);
    }

    public EcmaValue GetInt16(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(short));
      return CheckAndSwap(this.Buffer.GetInt16(index + ByteOffset), isLittleEndian);
    }

    public EcmaValue GetInt32(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(int));
      return CheckAndSwap(this.Buffer.GetInt32(index + ByteOffset), isLittleEndian);
    }

    public EcmaValue GetUInt8(long index) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(byte));
      return this.Buffer.GetByte(index + ByteOffset);
    }

    public EcmaValue GetUInt16(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(ushort));
      return CheckAndSwap(this.Buffer.GetUInt16(index + ByteOffset), isLittleEndian);
    }

    public EcmaValue GetUInt32(long index, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ThrowIfOutOfBound(index, sizeof(uint));
      return CheckAndSwap(this.Buffer.GetUInt32(index + ByteOffset), isLittleEndian);
    }

    public void SetFloat32(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      float floatValue = (float)value.ToDouble();
      ThrowIfOutOfBound(index, sizeof(float));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(floatValue, isLittleEndian));
    }

    public void SetFloat64(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      double doubleValue = value.ToDouble();
      ThrowIfOutOfBound(index, sizeof(double));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(doubleValue, isLittleEndian));
    }

    public void SetInt8(long index, EcmaValue value) {
      Guard.BufferNotDetached(this);
      sbyte sbyteValue = value.ToInt8();
      ThrowIfOutOfBound(index, sizeof(sbyte));
      this.Buffer.SetValue(index + ByteOffset, sbyteValue);
    }

    public void SetInt16(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      short shortValue = value.ToInt16();
      ThrowIfOutOfBound(index, sizeof(short));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(shortValue, isLittleEndian));
    }

    public void SetInt32(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      int intValue = value.ToInt32();
      ThrowIfOutOfBound(index, sizeof(int));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(intValue, isLittleEndian));
    }

    public void SetUInt8(long index, EcmaValue value) {
      Guard.BufferNotDetached(this);
      byte byteValue = value.ToUInt8();
      ThrowIfOutOfBound(index, sizeof(byte));
      this.Buffer.SetValue(index + ByteOffset, byteValue);
    }

    public void SetUInt16(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      ushort ushortValue = value.ToUInt16();
      ThrowIfOutOfBound(index, sizeof(ushort));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(ushortValue, isLittleEndian));
    }

    public void SetUInt32(long index, EcmaValue value, DataViewEndianness isLittleEndian) {
      Guard.BufferNotDetached(this);
      uint uintValue = value.ToUInt32();
      ThrowIfOutOfBound(index, sizeof(uint));
      this.Buffer.SetValue(index + ByteOffset, CheckAndSwap(uintValue, isLittleEndian));
    }

    private void ThrowIfOutOfBound(long index, int elementSize) {
      if (index + elementSize > this.ByteLength) {
        throw new EcmaRangeErrorException(InternalString.Error.DataViewInvalidOffset);
      }
    }

    #region Bits conversion helpers
    private static bool IsByteSwapRequired(DataViewEndianness endian) {
      switch (endian) {
        case DataViewEndianness.BigEndian:
          return BitConverter.IsLittleEndian;
        case DataViewEndianness.LittleEndian:
          return !BitConverter.IsLittleEndian;
      }
      return false;
    }

    private static unsafe int SingleToInt32Bits(float value) {
      return *(int*)&value;
    }

    private static unsafe float Int32BitsToSingle(int value) {
      return *(float*)&value;
    }

    private static short CheckAndSwap(short v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? SwapBytes(v) : v;
    }

    private static ushort CheckAndSwap(ushort v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? SwapBytes(v) : v;
    }

    private static int CheckAndSwap(int v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? SwapBytes(v) : v;
    }

    private static uint CheckAndSwap(uint v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? SwapBytes(v) : v;
    }

    private static double CheckAndSwap(double v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? BitConverter.Int64BitsToDouble(SwapBytes(BitConverter.DoubleToInt64Bits(v))) : v;
    }

    private static float CheckAndSwap(float v, DataViewEndianness isLittleEndian) {
      return IsByteSwapRequired(isLittleEndian) ? Int32BitsToSingle(SwapBytes(SingleToInt32Bits(v))) : v;
    }

    private static short SwapBytes(short v) {
      return (short)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
    }

    private static ushort SwapBytes(ushort v) {
      return (ushort)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
    }

    private static int SwapBytes(int v) {
      return ((SwapBytes((short)v) & 0xffff) << 0x10) | (SwapBytes((short)(v >> 0x10)) & 0xffff);
    }

    private static uint SwapBytes(uint v) {
      return (uint)(((SwapBytes((ushort)v) & 0xffff) << 0x10) | (SwapBytes((ushort)(v >> 0x10)) & 0xffff));
    }

    private static long SwapBytes(long v) {
      return ((SwapBytes((int)v) & 0xffffffffL) << 0x20) | (SwapBytes((int)(v >> 0x20)) & 0xffffffffL);
    }
    #endregion
  }
}
