using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma {
  public enum TypedArrayKind {
    Float32Array,
    Float64Array,
    Int8Array,
    Int16Array,
    Int32Array,
    Uint8Array,
    Uint8ClampedArray,
    Uint16Array,
    Uint32Array,
    BigInt64Array,
    BigUint64Array
  }

  public class TypedArrayInfo {
    private static readonly TypedArrayInfo[] info = new[] {
      new TypedArrayInfo(TypedArrayKind.Float32Array, WellKnownObject.Float32Array, WellKnownObject.Float32ArrayPrototype, sizeof(float), "Float32Array"),
      new TypedArrayInfo(TypedArrayKind.Float64Array, WellKnownObject.Float64Array, WellKnownObject.Float64ArrayPrototype, sizeof(double), "Float64Array"),
      new TypedArrayInfo(TypedArrayKind.Int8Array, WellKnownObject.Int8Array, WellKnownObject.Int8ArrayPrototype, sizeof(sbyte), "Int8Array"),
      new TypedArrayInfo(TypedArrayKind.Int16Array, WellKnownObject.Int16Array, WellKnownObject.Int16ArrayPrototype, sizeof(short), "Int16Array"),
      new TypedArrayInfo(TypedArrayKind.Int32Array, WellKnownObject.Int32Array, WellKnownObject.Int32ArrayPrototype, sizeof(int), "Int32Array"),
      new TypedArrayInfo(TypedArrayKind.Uint8Array, WellKnownObject.Uint8Array, WellKnownObject.Uint8ArrayPrototype, sizeof(byte), "Uint8Array"),
      new TypedArrayInfo(TypedArrayKind.Uint8ClampedArray, WellKnownObject.Uint8ClampedArray, WellKnownObject.Uint8ClampedArrayPrototype, sizeof(byte), "Uint8ClampedArray"),
      new TypedArrayInfo(TypedArrayKind.Uint16Array, WellKnownObject.Uint16Array, WellKnownObject.Uint16ArrayPrototype, sizeof(ushort), "Uint16Array"),
      new TypedArrayInfo(TypedArrayKind.Uint32Array, WellKnownObject.Uint32Array, WellKnownObject.Uint32ArrayPrototype, sizeof(uint), "Uint32Array"),
      new TypedArrayInfo(TypedArrayKind.BigInt64Array, WellKnownObject.BigInt64Array, WellKnownObject.BigInt64ArrayPrototype, sizeof(long), "BigInt64Array"),
      new TypedArrayInfo(TypedArrayKind.BigUint64Array, WellKnownObject.BigUint64Array, WellKnownObject.BigUint64ArrayPrototype, sizeof(ulong), "BigUint64Array"),
    };

    private TypedArrayInfo(TypedArrayKind kind, WellKnownObject defaultConstructor, WellKnownObject proto, int elementSize, string typeName) {
      this.ArrayKind = kind;
      this.DefaultConstructor = defaultConstructor;
      this.DefaultPrototype = proto;
      this.ElementSize = elementSize;
      this.TypedArrayName = typeName;
    }

    private WellKnownObject DefaultConstructor { get; }
    private WellKnownObject DefaultPrototype { get; }
    private TypedArrayKind ArrayKind { get; }
    private int ElementSize { get; }
    private string TypedArrayName { get; }

    public static WellKnownObject GetDefaultConstructor(TypedArrayKind kind) {
      return info[(int)kind].DefaultConstructor;
    }

    public static WellKnownObject GetDefaultPrototype(TypedArrayKind kind) {
      return info[(int)kind].DefaultPrototype;
    }

    public static string GetTypedArrayName(TypedArrayKind kind) {
      return info[(int)kind].TypedArrayName;
    }

    public static int GetElementSize(TypedArrayKind kind) {
      return info[(int)kind].ElementSize;
    }
  }

  public abstract class TypedArray : RuntimeObject, IArrayBufferView {
    private long byteLength;
    private long byteOffset;
    private int length;

    public TypedArray(TypedArrayKind kind)
      : base(TypedArrayInfo.GetDefaultPrototype(kind)) {
      this.ArrayKind = kind;
      this.ElementSize = TypedArrayInfo.GetElementSize(kind);
    }

    public ArrayBuffer Buffer { get; private set; }
    public TypedArrayKind ArrayKind { get; }
    public int ElementSize { get; }

    public int Length {
      get { return this.Buffer.IsDetached ? 0 : length; }
      private set { length = value; }
    }

    public long ByteLength {
      get { return this.Buffer.IsDetached ? 0 : byteLength; }
      private set { byteLength = value; }
    }

    public long ByteOffset {
      get { return this.Buffer.IsDetached ? 0 : byteOffset; }
      private set { byteOffset = value; }
    }

    public override EcmaValue this[int key] {
      get {
        return key >= 0 && key < length ? GetValueFromBuffer(key) : EcmaValue.Undefined;
      }
      set {
        if (key >= 0 && key < length) {
          SetValueInBuffer(key, value);
        }
      }
    }

    internal void Init(long length) {
      if (length > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidLength, length);
      }
      Init(new ArrayBuffer(length * this.ElementSize));
    }

    internal void Init(ArrayBuffer buffer) {
      Guard.ArgumentNotNull(buffer, "buffer");
      Init(buffer, 0, buffer.ByteLength);
    }

    internal virtual void Init(ArrayBuffer buffer, long byteOffset, long byteLength) {
      Guard.ArgumentNotNull(buffer, "buffer");
      long bytesPerElement = this.ElementSize;
      long count = byteLength / bytesPerElement;
      if ((byteOffset % bytesPerElement) != 0) {
        throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidOffset, TypedArrayInfo.GetTypedArrayName(this.ArrayKind), bytesPerElement);
      }
      if ((byteLength % bytesPerElement) != 0) {
        throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidByteLength, TypedArrayInfo.GetTypedArrayName(this.ArrayKind), bytesPerElement);
      }
      if (byteOffset < 0 || byteOffset > buffer.ByteLength) {
        throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, byteOffset);
      }
      if (byteLength + byteOffset > buffer.ByteLength || count > Int32.MaxValue) {
        throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidLength, count);
      }
      this.Buffer = buffer;
      this.ByteOffset = byteOffset;
      this.ByteLength = byteLength;
      this.Length = (int)count;
    }

    [EcmaSpecification("TypedArraySpeciesCreate", EcmaSpecificationKind.AbstractOperations)]
    public static TypedArray SpeciesCreate(TypedArray source, params EcmaValue[] args) {
      RuntimeObject constructor = RuntimeObject.GetSpeciesConstructor(source, TypedArrayInfo.GetDefaultConstructor(source.ArrayKind));
      return constructor.Construct(args).GetUnderlyingObject<TypedArray>();
    }

    public long GetByteOffset(int index) {
      return this.ElementSize * index + this.ByteOffset;
    }

    [EcmaSpecification("IntegerIndexedElementGet", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("GetValueFromBuffer", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue GetValueFromBuffer(int index) {
      Guard.BufferNotDetached(this);
      return GetValueFromBufferImpl(index);
    }

    [EcmaSpecification("IntegerIndexedElementSet", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("SetValueInBuffer", EcmaSpecificationKind.AbstractOperations)]
    public void SetValueInBuffer(int index, EcmaValue value) {
      value = CoerceValue(value);
      Guard.BufferNotDetached(this);
      SetValueInBufferImpl(index, value);
    }

    public abstract void Sort();

    public abstract void Sort(RuntimeObject callback);

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsCanonicalNumericIndex) {
        Guard.BufferNotDetached(this);
        if (IsValidIndex(propertyKey, out int index)) {
          EcmaValue value = GetValueFromBuffer(index);
          return new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable);
        }
        return null;
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsCanonicalNumericIndex) {
        Guard.BufferNotDetached(this);
        return IsValidIndex(propertyKey, out int _);
      }
      return base.HasProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyKey.IsCanonicalNumericIndex) {
        if (IsValidIndex(propertyKey, out int index)) {
          if (descriptor.IsAccessorDescriptor) {
            return false;
          }
          if ((descriptor.HasConfigurable && descriptor.Configurable) ||
              (descriptor.HasEnumerable && !descriptor.Enumerable) ||
              (descriptor.HasWritable && !descriptor.Writable)) {
            return false;
          }
          if (descriptor.HasValue) {
            SetValueInBuffer(index, descriptor.Value);
          }
          return true;
        }
        return false;
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (propertyKey.IsCanonicalNumericIndex) {
        Guard.BufferNotDetached(this);
        if (IsValidIndex(propertyKey, out int index)) {
          return GetValueFromBuffer(index);
        }
        return default;
      }
      return base.Get(propertyKey, receiver);
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      if (propertyKey.IsCanonicalNumericIndex) {
        value = CoerceValue(value);
        Guard.BufferNotDetached(this);
        if (IsValidIndex(propertyKey, out int index)) {
          SetValueInBuffer(index, value);
          return true;
        }
        return false;
      }
      return base.Set(propertyKey, value, receiver);
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      return Enumerable.Range(0, length).Select(v => new EcmaPropertyKey(v)).Concat(base.GetOwnPropertyKeys().Where(v => !v.IsArrayIndex));
    }

    protected abstract EcmaValue GetValueFromBufferImpl(int index);

    protected abstract void SetValueInBufferImpl(int index, EcmaValue value);

    protected virtual EcmaValue CoerceValue(EcmaValue value) {
      return value.ToNumber();
    }

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      this.Buffer = (ArrayBuffer)context.Clone(this.Buffer);
    }

    private bool IsValidIndex(EcmaPropertyKey propertyKey, out int index) {
      if (propertyKey.IsArrayIndex && !propertyKey.IsNegativeZero) {
        long longIndex = propertyKey.ToArrayIndex();
        if (longIndex >= 0 && longIndex < length) {
          index = (int)longIndex;
          return true;
        }
      }
      index = -1;
      return false;
    }
  }

  public abstract class TypedArray<T> : TypedArray where T : struct, IComparable<T> {
    public TypedArray(TypedArrayKind kind)
      : base(kind) { }

    public ArrayBufferView<T> BufferView { get; private set; }

    internal override void Init(ArrayBuffer buffer, long byteOffset, long byteLength) {
      base.Init(buffer, byteOffset, byteLength);
      this.BufferView = GetArrayBufferView(buffer, byteOffset, byteLength);
    }

    public override void Sort() {
      this.BufferView.Sort();
    }

    public override void Sort(RuntimeObject callback) {
      Guard.ArgumentNotNull(callback, "callback");
      this.BufferView.Sort(new ComparerWrapper<T>((x, y) => {
        EcmaValue v = CallSortCallback(callback, x, y).ToNumber();
        Guard.BufferNotDetached(this);
        return v.IsNaN || v == 0 ? 0 : v > 0 ? 1 : -1;
      }));
    }

    protected abstract ArrayBufferView<T> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength);

    protected abstract EcmaValue CallSortCallback(RuntimeObject callback, T x, T y);

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      this.BufferView = GetArrayBufferView(this.Buffer, this.ByteOffset, this.ByteLength);
    }
  }

  #region Derived classes
  [Cloneable(false)]
  public class Float32Array : TypedArray<float> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Float32Array;
    private static readonly IComparer<float> floatComparer = new ComparerWrapper<float>(CompareFloat);

    public Float32Array()
      : base(arrayKind) { }

    public Float32Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Float32Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Float32Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Float32Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Float32Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    public override void Sort() {
      this.BufferView.Sort(floatComparer);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = (float)value.ToDouble();
    }

    protected override ArrayBufferView<float> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetFloatBufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, float x, float y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }

    private static int CompareFloat(float x, float y) {
      if (Single.IsNaN(x) && Single.IsNaN(y)) {
        return 0;
      }
      if (Single.IsNaN(x)) {
        return 1;
      }
      if (Single.IsNaN(y)) {
        return -1;
      }
      if (x < y) {
        return -1;
      }
      if (x > y) {
        return 1;
      }
      if (x == -0f && y == 0) {
        return -1;
      }
      if (y == -0f && x == 0) {
        return 1;
      }
      return 0;
    }
  }

  [Cloneable(false)]
  public class Float64Array : TypedArray<double> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Float64Array;
    private static readonly IComparer<double> doubleComparer = new ComparerWrapper<double>(CompareDouble);

    public Float64Array()
      : base(arrayKind) { }

    public Float64Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Float64Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Float64Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Float64Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Float64Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    public override void Sort() {
      this.BufferView.Sort(doubleComparer);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToDouble();
    }

    protected override ArrayBufferView<double> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetDoubleBufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, double x, double y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }

    private static int CompareDouble(double x, double y) {
      if (Double.IsNaN(x) && Double.IsNaN(y)) {
        return 0;
      }
      if (Double.IsNaN(x)) {
        return 1;
      }
      if (Double.IsNaN(y)) {
        return -1;
      }
      if (x < y) {
        return -1;
      }
      if (x > y) {
        return 1;
      }
      if (x == -0d && y == 0) {
        return -1;
      }
      if (y == -0d && x == 0) {
        return 1;
      }
      return 0;
    }
  }

  [Cloneable(false)]
  public class Int8Array : TypedArray<sbyte> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Int8Array;

    public Int8Array()
      : base(arrayKind) { }

    public Int8Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Int8Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Int8Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Int8Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Int8Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToInt8();
    }

    protected override ArrayBufferView<sbyte> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetSByteBufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, sbyte x, sbyte y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Int16Array : TypedArray<short> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Int16Array;

    public Int16Array()
      : base(arrayKind) { }

    public Int16Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Int16Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Int16Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Int16Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Int16Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToInt16();
    }

    protected override ArrayBufferView<short> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetInt16BufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, short x, short y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Int32Array : TypedArray<int> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Int32Array;

    public Int32Array()
      : base(arrayKind) { }

    public Int32Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Int32Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Int32Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Int32Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Int32Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToInt32();
    }

    protected override ArrayBufferView<int> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetInt32BufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, int x, int y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Uint8Array : TypedArray<byte> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Uint8Array;

    public Uint8Array()
      : base(arrayKind) { }

    public Uint8Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Uint8Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Uint8Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Uint8Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Uint8Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToUInt8();
    }

    protected override ArrayBufferView<byte> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetByteBufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, byte x, byte y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Uint8ClampedArray : TypedArray<byte> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Uint8ClampedArray;

    public Uint8ClampedArray()
      : base(arrayKind) { }

    public Uint8ClampedArray(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Uint8ClampedArray(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Uint8ClampedArray(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Uint8ClampedArray(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Uint8ClampedArray(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToUInt8Clamp();
    }

    protected override ArrayBufferView<byte> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetByteBufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, byte x, byte y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Uint16Array : TypedArray<ushort> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Uint16Array;

    public Uint16Array()
      : base(arrayKind) { }

    public Uint16Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Uint16Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Uint16Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Uint16Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Uint16Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToUInt16();
    }

    protected override ArrayBufferView<ushort> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetUInt16BufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, ushort x, ushort y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class Uint32Array : TypedArray<uint> {
    private const TypedArrayKind arrayKind = TypedArrayKind.Uint32Array;

    public Uint32Array()
      : base(arrayKind) { }

    public Uint32Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public Uint32Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public Uint32Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public Uint32Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public Uint32Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return this.BufferView[index];
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = value.ToUInt32();
    }

    protected override ArrayBufferView<uint> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetUInt32BufferView(byteOffset, byteLength);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, uint x, uint y) {
      return callback.Call(EcmaValue.Undefined, x, y);
    }
  }

  [Cloneable(false)]
  public class BigInt64Array : TypedArray<long> {
    private const TypedArrayKind arrayKind = TypedArrayKind.BigInt64Array;

    public BigInt64Array()
      : base(arrayKind) { }

    public BigInt64Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public BigInt64Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public BigInt64Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public BigInt64Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public BigInt64Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return BigIntHelper.ToBigInt(this.BufferView[index]);
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = BigIntHelper.ToInt64(value);
    }

    protected override ArrayBufferView<long> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetInt64BufferView(byteOffset, byteLength, true);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, long x, long y) {
      return callback.Call(EcmaValue.Undefined, BigIntHelper.ToBigInt(x), BigIntHelper.ToBigInt(y));
    }

    protected override EcmaValue CoerceValue(EcmaValue value) {
      return BigIntHelper.ToBigInt(value);
    }
  }

  [Cloneable(false)]
  public class BigUint64Array : TypedArray<ulong> {
    private const TypedArrayKind arrayKind = TypedArrayKind.BigUint64Array;

    public BigUint64Array()
      : base(arrayKind) { }

    public BigUint64Array(int length)
    : base(arrayKind) {
      Init(length);
    }

    public BigUint64Array(byte[] bytes)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes));
    }

    public BigUint64Array(byte[] bytes, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(new ArrayBuffer(bytes), byteOffset, byteLength);
    }

    public BigUint64Array(ArrayBuffer buffer)
      : base(arrayKind) {
      Init(buffer);
    }

    public BigUint64Array(ArrayBuffer buffer, long byteOffset, long byteLength)
      : base(arrayKind) {
      Init(buffer, byteOffset, byteLength);
    }

    protected override EcmaValue GetValueFromBufferImpl(int index) {
      return BigIntHelper.ToBigInt(this.BufferView[index]);
    }

    protected override void SetValueInBufferImpl(int index, EcmaValue value) {
      this.BufferView[index] = BigIntHelper.ToUInt64(value);
    }

    protected override ArrayBufferView<ulong> GetArrayBufferView(ArrayBuffer buffer, long byteOffset, long byteLength) {
      return buffer.GetUInt64BufferView(byteOffset, byteLength, true);
    }

    protected override EcmaValue CallSortCallback(RuntimeObject callback, ulong x, ulong y) {
      return callback.Call(EcmaValue.Undefined, BigIntHelper.ToBigInt(x), BigIntHelper.ToBigInt(y));
    }

    protected override EcmaValue CoerceValue(EcmaValue value) {
      return BigIntHelper.ToBigInt(value);
    }
  }
  #endregion
}
