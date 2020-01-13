using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Codeless.Ecma {
  public interface IArrayBufferView {
    ArrayBuffer Buffer { get; }
    long ByteOffset { get; }
    long ByteLength { get; }
  }

  [Cloneable(false, Transferable = true)]
  public class ArrayBuffer : RuntimeObject {
    [StructLayout(LayoutKind.Explicit)]
    private struct BufferView {
      [FieldOffset(0)]
      public byte[] Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public sbyte[] SByteBuffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public short[] Int16Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ushort[] UInt16Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public int[] Int32Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public uint[] UInt32Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public long[] Int64Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ulong[] UInt64Buffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public float[] FloatBuffer;
      [FieldOffset(0)]
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public double[] DoubleBuffer;
    }

    private List<WeakReference> disposables = new List<WeakReference>();
    private BufferView buffer;

    public ArrayBuffer()
      : base(WellKnownObject.ArrayBufferPrototype) { }

    public ArrayBuffer(RuntimeObject constructor)
      : base(WellKnownObject.ArrayBufferPrototype, constructor) { }

    public ArrayBuffer(long size)
      : this() {
      Init(size);
    }

    public ArrayBuffer(byte[] buffer)
      : this() {
      Guard.ArgumentNotNull(buffer, "buffer");
      this.buffer.Buffer = buffer;
    }

    protected ArrayBuffer(WellKnownObject proto)
      : base(proto) { }

    protected ArrayBuffer(WellKnownObject proto, RuntimeObject constructor)
      : base(proto, constructor) { }

    protected ArrayBuffer(WellKnownObject proto, long size)
      : base(proto) {
      Init(size);
    }

    protected ArrayBuffer(WellKnownObject proto, byte[] buffer)
      : base(proto) {
      Guard.ArgumentNotNull(buffer, "buffer");
      this.buffer.Buffer = buffer;
    }

    [EcmaSpecification("IsSharedBuffer", EcmaSpecificationKind.AbstractOperations)]
    public bool IsShared {
      get { return this is SharedArrayBuffer; }
    }

    [EcmaSpecification("IsDetachedBuffer", EcmaSpecificationKind.AbstractOperations)]
    public bool IsDetached {
      get { return buffer.Buffer == null; }
    }

    public long ByteLength {
      get { return buffer.Buffer != null ? buffer.Buffer.Length : 0; }
    }

    internal int[] Int32Array {
      get { return buffer.Int32Buffer; }
    }

    [EcmaSpecification("CreateByteDataBlock", EcmaSpecificationKind.AbstractOperations)]
    internal void Init(long size) {
      if (size < 0) {
        throw new EcmaRangeErrorException("Array buffer size cannot be negative");
      }
      try {
        buffer.Buffer = new byte[size];
      } catch {
        throw new EcmaRangeErrorException("Array buffer allocation failed");
      }
    }

    [EcmaSpecification("AllocateArrayBuffer", EcmaSpecificationKind.AbstractOperations)]
    public static ArrayBuffer AllocateArrayBuffer(RuntimeObject constructor, long byteLength) {
      ArrayBuffer view = new ArrayBuffer(constructor);
      view.Init(byteLength);
      return view;
    }

    [EcmaSpecification("CopyDataBlockBytes", EcmaSpecificationKind.AbstractOperations)]
    public static void CopyBytes(ArrayBuffer src, long srcOffset, ArrayBuffer dst, long dstOffset, long count) {
      Guard.ArgumentNotNull(src, "src");
      Guard.ArgumentNotNull(dst, "dst");
      src.ThrowIfBufferDetached();
      dst.ThrowIfBufferDetached();
      if (dstOffset + dst.ByteLength < count) {
        throw new EcmaTypeErrorException(InternalString.Error.SourceTooLarge);
      }
      Buffer.BlockCopy(src.buffer.Buffer, (int)srcOffset, dst.buffer.Buffer, (int)dstOffset, (int)count);
    }

    public ArrayBuffer Slice(long start, long end) {
      ThrowIfBufferDetached();

      long thisLength = buffer.Buffer.Length;
      start = start < 0 ? Math.Max(thisLength + start, 0) : Math.Min(start, thisLength);
      end = end < 0 ? Math.Max(thisLength + end, 0) : Math.Min(end, thisLength);

      long newLength = Math.Max(end - start, 0);
      RuntimeObject constructor = GetSpeciesConstructor(this, this.IsShared ? WellKnownObject.SharedArrayBuffer : WellKnownObject.ArrayBuffer);
      ArrayBuffer other = constructor.Construct(newLength).GetUnderlyingObject<ArrayBuffer>();
      if (other == this) {
        throw new EcmaTypeErrorException("{0} subclass returned this from species constructor", this.GetType().Name);
      }
      if (other.IsShared != this.IsShared) {
        throw new EcmaTypeErrorException("Method {0}.prototype.slice called on incompatible receiver", this.GetType().Name);
      }
      CopyBytes(this, start, other, 0, newLength);
      return other;
    }

    [EcmaSpecification("CloneArrayBuffer", EcmaSpecificationKind.AbstractOperations)]
    public ArrayBuffer Clone(long start, long length, RuntimeObject constructor) {
      Guard.ArgumentNotNull(constructor, "constructor");
      ArrayBuffer other = AllocateArrayBuffer(constructor, length);
      CopyBytes(this, start, other, 0, length);
      return other;
    }

    [EcmaSpecification("DetachArrayBuffer", EcmaSpecificationKind.AbstractOperations)]
    public void Detach() {
      if (this.IsShared) {
        throw new InvalidOperationException();
      }
      if (this.buffer.Buffer != null) {
        this.buffer.Buffer = null;
        foreach (WeakReference pointer in disposables) {
          if (pointer.IsAlive) {
            IDisposable disposable = pointer.Target as IDisposable;
            if (disposable != null) {
              disposable.Dispose();
            }
          }
        }
        disposables.Clear();
      }
    }

    public float GetFloat(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(float));
      if ((index % sizeof(float)) == 0) {
        return buffer.FloatBuffer[index / sizeof(float)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(float*)(ptr + index);
        }
      }
    }

    public double GetDouble(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(double));
      if ((index % sizeof(double)) == 0) {
        return buffer.DoubleBuffer[index / sizeof(double)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(double*)(ptr + index);
        }
      }
    }

    public short GetInt16(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(short));
      if ((index % sizeof(short)) == 0) {
        return buffer.Int16Buffer[index / sizeof(short)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(short*)(ptr + index);
        }
      }
    }

    public ushort GetUInt16(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(ushort));
      if ((index % sizeof(ushort)) == 0) {
        return buffer.UInt16Buffer[index / sizeof(ushort)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(ushort*)(ptr + index);
        }
      }
    }

    public int GetInt32(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(int));
      if ((index % sizeof(int)) == 0) {
        return buffer.Int32Buffer[index / sizeof(int)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(int*)(ptr + index);
        }
      }
    }

    public uint GetUInt32(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(uint));
      if ((index % sizeof(uint)) == 0) {
        return buffer.UInt32Buffer[index / sizeof(uint)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(uint*)(ptr + index);
        }
      }
    }

    public long GetInt64(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(long));
      if ((index % sizeof(long)) == 0) {
        return buffer.Int64Buffer[index / sizeof(long)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(long*)(ptr + index);
        }
      }
    }

    public ulong GetUInt64(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(ulong));
      if ((index % sizeof(ulong)) == 0) {
        return buffer.UInt64Buffer[index / sizeof(ulong)];
      }
      unsafe {
        fixed (byte* ptr = buffer.Buffer) {
          return *(ulong*)(ptr + index);
        }
      }
    }

    public byte GetByte(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(byte));
      return buffer.Buffer[index];
    }

    public sbyte GetSByte(long index) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(sbyte));
      return buffer.SByteBuffer[index];
    }

    public void SetValue(long index, float value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(float));
      if ((index % sizeof(float)) == 0) {
        buffer.FloatBuffer[index / sizeof(float)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(float*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, double value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(double));
      if ((index % sizeof(double)) == 0) {
        buffer.DoubleBuffer[index / sizeof(double)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(double*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, short value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(short));
      if ((index % sizeof(short)) == 0) {
        buffer.Int16Buffer[index / sizeof(short)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(short*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, ushort value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(ushort));
      if ((index % sizeof(ushort)) == 0) {
        buffer.UInt16Buffer[index / sizeof(ushort)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(ushort*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, int value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(int));
      if ((index % sizeof(int)) == 0) {
        buffer.Int32Buffer[index / sizeof(int)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(int*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, uint value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(uint));
      if ((index % sizeof(uint)) == 0) {
        buffer.UInt32Buffer[index / sizeof(uint)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(uint*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, long value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(long));
      if ((index % sizeof(long)) == 0) {
        buffer.Int64Buffer[index / sizeof(long)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(long*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, ulong value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(ulong));
      if ((index % sizeof(ulong)) == 0) {
        buffer.UInt64Buffer[index / sizeof(ulong)] = value;
      } else {
        unsafe {
          fixed (byte* ptr = buffer.Buffer) {
            *(ulong*)(ptr + index) = value;
          }
        }
      }
    }

    public void SetValue(long index, byte value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(byte));
      buffer.Buffer[index] = value;
    }

    public void SetValue(long index, sbyte value) {
      ThrowIfBufferDetached();
      ThrowIfIndexOutOfBound(index, sizeof(sbyte));
      buffer.SByteBuffer[index] = value;
    }

    public ArrayBufferView<float> GetFloatBufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<float> bufferView = new ArrayBufferView<float>(buffer.FloatBuffer, byteOffset / sizeof(float), byteLength / sizeof(float));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<double> GetDoubleBufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<double> bufferView = new ArrayBufferView<double>(buffer.DoubleBuffer, byteOffset / sizeof(double), byteLength / sizeof(double));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<short> GetInt16BufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<short> bufferView = new ArrayBufferView<short>(buffer.Int16Buffer, byteOffset / sizeof(short), byteLength / sizeof(short));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<ushort> GetUInt16BufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<ushort> bufferView = new ArrayBufferView<ushort>(buffer.UInt16Buffer, byteOffset / sizeof(ushort), byteLength / sizeof(ushort));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<int> GetInt32BufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<int> bufferView = new ArrayBufferView<int>(buffer.Int32Buffer, byteOffset / sizeof(int), byteLength / sizeof(int));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<uint> GetUInt32BufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<uint> bufferView = new ArrayBufferView<uint>(buffer.UInt32Buffer, byteOffset / sizeof(uint), byteLength / sizeof(uint));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<long> GetInt64BufferView(long byteOffset, long byteLength, bool tearFree) {
      ThrowIfBufferDetached();
      ArrayBufferView<long> bufferView = tearFree ? new SharedInt64BufferView(buffer.Int64Buffer, byteOffset / sizeof(long), byteLength / sizeof(long)) : new ArrayBufferView<long>(buffer.Int64Buffer, byteOffset / sizeof(long), byteLength / sizeof(long));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<ulong> GetUInt64BufferView(long byteOffset, long byteLength, bool tearFree) {
      ThrowIfBufferDetached();
      ArrayBufferView<ulong> bufferView = tearFree ? new SharedUInt64BufferView(buffer.UInt64Buffer, buffer.Int64Buffer, byteOffset / sizeof(long), byteLength / sizeof(long)) : new ArrayBufferView<ulong>(buffer.UInt64Buffer, byteOffset / sizeof(ulong), byteLength / sizeof(ulong));
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<byte> GetByteBufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<byte> bufferView = new ArrayBufferView<byte>(buffer.Buffer, byteOffset, byteLength);
      AddDisposable(bufferView);
      return bufferView;
    }

    public ArrayBufferView<sbyte> GetSByteBufferView(long byteOffset, long byteLength) {
      ThrowIfBufferDetached();
      ArrayBufferView<sbyte> bufferView = new ArrayBufferView<sbyte>(buffer.SByteBuffer, byteOffset, byteLength);
      AddDisposable(bufferView);
      return bufferView;
    }

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      if (!this.IsShared) {
        ArrayBuffer other = (ArrayBuffer)sourceObj;
        other.ThrowIfBufferDetached();
        if (isTransfer) {
          other.Detach();
        } else {
          long count = other.buffer.Buffer.LongLength;
          buffer.Buffer = new byte[count];
          CopyBytes(other, 0, this, 0, count);
        }
      }
    }

    private void AddDisposable(IDisposable item) {
      disposables.Add(new WeakReference(item));
    }

    private void ThrowIfIndexOutOfBound(long index, long elementSize) {
      if (index < 0 && index + elementSize > this.ByteLength) {
        throw new ArgumentOutOfRangeException("Index out of bound", "index");
      }
    }

    private void ThrowIfBufferDetached() {
      if (this.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
    }
  }
}
