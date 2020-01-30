using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Codeless.Ecma {
  public class ArrayBufferView<T> : IDisposable, IList<T>, ICollection<T> where T : struct, IComparable<T> {
    private static readonly IComparer<T> defaultComparer = new ComparerWrapper<T>((x, y) => x.CompareTo(y));
    private T[] array;
    private int offset;
    private int length;

    internal ArrayBufferView(T[] array, long offset, long length) {
      Guard.ArgumentNotNull(array, "array");
      if (array.GetType() != typeof(byte[])) {
        throw new ArgumentException("Buffer must be a byte array", "array");
      }
      int bytesPerElement = GetElementSize();
      if (offset < 0 || offset * bytesPerElement > array.Length || offset > Int32.MaxValue) {
        throw new ArgumentOutOfRangeException("offset");
      }
      if (length < 0 || (offset + length) * bytesPerElement > array.Length || (offset + length) > Int32.MaxValue) {
        throw new ArgumentOutOfRangeException("length");
      }
      this.array = array;
      this.offset = (int)offset;
      this.length = (int)length;
    }

    public virtual T this[int index] {
      get => array[GetRealIndex(index)];
      set => array[GetRealIndex(index)] = value;
    }

    public int Count {
      get { return length; }
    }

    public void Sort() {
      Sort(null);
    }

    public void Sort(IComparer<T> comparer) {
      if (typeof(T) != typeof(byte) && (comparer == null || comparer == Comparer<T>.Default)) {
        // prevent Array.Sort use native TrySZArraySort when comparer is null or is the default comparer
        // because the buffer is actually a byte array which causes
        // bound check issue and unexpected behavior on ordering of bytes
        comparer = defaultComparer;
      }
      try {
        Array.Sort(array, offset, length, comparer);
      } catch (InvalidOperationException ex) {
        throw ex.InnerException;
      }
    }

    public T[] ToArray() {
      T[] array = new T[length];
      CopyTo(array, 0);
      return array;
    }

    public void CopyTo(T[] array, int arrayIndex) {
      int bytesPerElement = GetElementSize();
      Buffer.BlockCopy(this.array, offset * bytesPerElement, array, arrayIndex * bytesPerElement, length * bytesPerElement);
    }

    public virtual void Dispose() {
      array = null;
      offset = 0;
      length = 0;
    }

    protected int GetRealIndex(int index) {
      if (index < 0 || index >= length) {
        throw new ArgumentOutOfRangeException("index");
      }
      return offset + index;
    }

    private static int GetElementSize() {
      switch (Type.GetTypeCode(typeof(T))) {
        case TypeCode.Byte:
          return sizeof(byte);
        case TypeCode.SByte:
          return sizeof(sbyte);
        case TypeCode.Int16:
          return sizeof(short);
        case TypeCode.Int32:
          return sizeof(int);
        case TypeCode.Int64:
          return sizeof(long);
        case TypeCode.UInt16:
          return sizeof(ushort);
        case TypeCode.UInt32:
          return sizeof(uint);
        case TypeCode.UInt64:
          return sizeof(ulong);
        case TypeCode.Single:
          return sizeof(float);
        case TypeCode.Double:
          return sizeof(double);
      }
      throw new InvalidOperationException("Unsupported array type");
    }

    #region Interfaces
    bool ICollection<T>.IsReadOnly => false;

    int IList<T>.IndexOf(T item) {
      return Array.IndexOf(array, item, offset, length);
    }

    void IList<T>.Insert(int index, T item) {
      throw new InvalidOperationException("Size of array buffer is fixed");
    }

    void IList<T>.RemoveAt(int index) {
      throw new InvalidOperationException("Size of array buffer is fixed");
    }

    void ICollection<T>.Add(T item) {
      throw new InvalidOperationException("Size of array buffer is fixed");
    }

    void ICollection<T>.Clear() {
      throw new InvalidOperationException("Size of array buffer is fixed");
    }

    bool ICollection<T>.Contains(T item) {
      return Array.IndexOf(array, item, offset, length) != -1;
    }

    bool ICollection<T>.Remove(T item) {
      throw new InvalidOperationException("Size of array buffer is fixed");
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
      for (int i = offset, len = this.length + offset; i < len; i++) {
        yield return array[i];
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable<T>)this).GetEnumerator();
    }
    #endregion
  }

  internal class SharedInt64BufferView : ArrayBufferView<long> {
    private long[] array;

    internal SharedInt64BufferView(long[] array, long offset, long length)
      : base(array, offset, length) {
      this.array = array;
    }

    public override long this[int index] {
      get => Interlocked.Read(ref array[GetRealIndex(index)]);
      set => Interlocked.Exchange(ref array[GetRealIndex(index)], value);
    }

    public override void Dispose() {
      base.Dispose();
      array = null;
    }
  }

  internal class SharedUInt64BufferView : ArrayBufferView<ulong> {
    private long[] array;

    internal SharedUInt64BufferView(ulong[] arrayBase, long[] array, long offset, long length)
      : base(arrayBase, offset, length) {
      this.array = array;
    }

    public override ulong this[int index] {
      get => unchecked((ulong)Interlocked.Read(ref array[GetRealIndex(index)]));
      set => Interlocked.Exchange(ref array[GetRealIndex(index)], unchecked((long)value));
    }

    public override void Dispose() {
      base.Dispose();
      array = null;
    }
  }
}
