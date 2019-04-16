using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Diagnostics.VisualStudio;
using Codeless.Ecma.Primitives;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Codeless.Ecma {
  public enum EcmaValueType {
    Undefined,
    Null,
    Object,
    Number,
    String,
    Boolean,
    Symbol
  }

  public enum EcmaNumberType {
    Invalid,
    Int32,
    Int64,
    Double
  }

  public enum EcmaValueComparison {
    Strict,
    Abstract,
    SameValue,
    SameValueZero,
    SameValueNotNumber
  }

  public enum EcmaPreferredPrimitiveType {
    Default,
    String,
    Number
  }

  /// <summary>
  /// Represents a dynamic value in pipe executions to mimic behaviors to values in ECMAScript.
  /// </summary>
  [Serializable]
  [JsonConverter(typeof(EcmaValueJsonConverter))]
  [DebuggerTypeProxy(typeof(EcmaValueDebuggerProxy))]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public partial struct EcmaValue : IEquatable<EcmaValue>, IComparable<EcmaValue>, IEnumerable<EcmaPropertyKey>, ISerializable, IConvertible {
    /// <summary>
    /// Represents an undefined value. It is similar to *undefined* in ECMAScript which could be returned when accessing an undefined property.
    /// </summary>
    public static readonly EcmaValue Undefined = new EcmaValue(EcmaValueHandle.Undefined, UndefinedBinder.Default);
    public static readonly EcmaValue Null = new EcmaValue(EcmaValueHandle.Null, NullBinder.Default);
    public static readonly EcmaValue NaN = new EcmaValue(EcmaValueHandle.NaN, DoubleBinder.Default);
    public static readonly EcmaValue Infinity = new EcmaValue(EcmaValueHandle.PositiveInfinity, DoubleBinder.Default);
    public static readonly EcmaValue NegativeInfinity = new EcmaValue(EcmaValueHandle.NegativeInfinity, DoubleBinder.Default);
    public static readonly EcmaValue NegativeZero = new EcmaValue(EcmaValueHandle.NegativeZero, DoubleBinder.Default);
    public static readonly EcmaValue[] EmptyArray = new EcmaValue[0];

    private readonly EcmaValueHandle handle;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IEcmaValueBinder binder_;

    [DebuggerStepThrough]
    public EcmaValue(bool value) {
      this.handle = BooleanBinder.Default.ToHandle(value);
      this.binder_ = BooleanBinder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(byte value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(sbyte value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(char value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(float value) {
      this.handle = DoubleBinder.Default.ToHandle(value);
      this.binder_ = DoubleBinder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(double value) {
      this.handle = DoubleBinder.Default.ToHandle(value);
      this.binder_ = DoubleBinder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(short value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(ushort value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(int value) {
      this.handle = Int32Binder.Default.ToHandle(value);
      this.binder_ = Int32Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(uint value) {
      this.handle = Int64Binder.Default.ToHandle(value);
      this.binder_ = Int64Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(long value) {
      this.handle = Int64Binder.Default.ToHandle(value);
      this.binder_ = Int64Binder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(ulong value) {
      this.handle = DoubleBinder.Default.ToHandle(value);
      this.binder_ = DoubleBinder.Default;
    }

    [DebuggerStepThrough]
    public EcmaValue(string value) {
      IEcmaValueBinder binder = WellKnownPropertyNameBinder.IsWellKnownPropertyName(value) ? WellKnownPropertyNameBinder.Default : PrimitiveBinderWrapper<string>.GetBinder(value, StringBinder.Default);
      this.handle = binder.ToHandle(value);
      this.binder_ = binder;
    }

    [DebuggerStepThrough]
    public EcmaValue(Symbol value) {
      if (value.SymbolType != 0) {
        this.handle = new EcmaValueHandle((long)value.SymbolType);
        this.binder_ = WellKnownSymbolBinder.Default;
      } else {
        IEcmaValueBinder binder = PrimitiveBinderWrapper<Symbol>.GetBinder(value, SymbolBinder.Default);
        this.handle = binder.ToHandle(value);
        this.binder_ = binder;
      }
    }

    [DebuggerStepThrough]
    public EcmaValue(RuntimeObject value) {
      IEcmaValueBinder binder = (IEcmaValueBinder)value ?? UndefinedBinder.Default;
      this.handle = binder.ToHandle(value);
      this.binder_ = binder;
    }

    [DebuggerStepThrough]
    public EcmaValue(object value) {
      EcmaValue resolved = UnboxObject(value);
      this.handle = resolved.handle;
      this.binder_ = resolved.binder;
    }

    [DebuggerStepThrough]
    public EcmaValue(SerializationInfo info, StreamingContext context) {
      Type type = (Type)info.GetValue("ut", typeof(Type));
      object value = info.GetValue("uo", type);
      EcmaValue resolved = UnboxObject(value);
      this.handle = resolved.handle;
      this.binder_ = resolved.binder;
    }

    [DebuggerStepThrough]
    internal EcmaValue(EcmaValueHandle value, IEcmaValueBinder binder) {
      this.handle = value;
      this.binder_ = binder;
    }

    private IEcmaValueBinder binder {
      get { return binder_ ?? UndefinedBinder.Default; }
    }

    /// <summary>
    /// Gets value of the specified property from the object.
    /// </summary>
    /// <param name="index">Property name.</param>
    /// <returns>Value associated with the property name, -or- <see cref="Undefined"/> if property does not exist.</returns>
    public EcmaValue this[EcmaPropertyKey index] {
      get { return binder.TryGet(handle, index, out EcmaValue value) ? value : default; }
      set { binder.TrySet(handle, index, value); }
    }

    public EcmaValue this[string key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[int key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[long key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[Symbol key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    /// <summary>
    /// Gets the type of value represented by the <see cref="EcmaValue"/> instance.
    /// </summary>
    public EcmaValueType Type {
      get { return binder.GetValueType(handle); }
    }

    [EcmaSpecification("typeof", EcmaSpecificationKind.RuntimeSemantics)]
    public string TypeOf {
      get {
        switch (this.Type) {
          case EcmaValueType.Undefined:
            return InternalString.TypeOf.Undefined;
          case EcmaValueType.Null:
            return InternalString.TypeOf.Object;
          case EcmaValueType.Boolean:
            return InternalString.TypeOf.Boolean;
          case EcmaValueType.Number:
            return InternalString.TypeOf.Number;
          case EcmaValueType.String:
            return InternalString.TypeOf.String;
          case EcmaValueType.Symbol:
            return InternalString.TypeOf.Symbol;
        }
        return this.IsCallable ? InternalString.TypeOf.Function : InternalString.TypeOf.Object;
      }
    }

    public bool IsNullOrUndefined {
      get {
        IEcmaValueBinder binder = binder_;
        return binder == default || binder == UndefinedBinder.Default || binder == NullBinder.Default;
      }
    }

    public bool IsPrimitive {
      get {
        IEcmaValueBinder binder = binder_;
        return binder == default || binder == UndefinedBinder.Default || binder == NullBinder.Default || binder.GetValueType(handle) != EcmaValueType.Object;
      }
    }

    public bool IsArrayLike {
      get {
        IEcmaValueBinder binder = binder_;
        EcmaValueHandle handle = this.handle;
        return binder != default && binder.GetValueType(handle) == EcmaValueType.Object && binder.HasOwnProperty(handle, WellKnownPropertyName.Length);
      }
    }

    public bool IsNaN {
      get { return binder_ == DoubleBinder.Default && handle == EcmaValueHandle.NaN; }
    }

    public bool IsFinite {
      get {
        IEcmaValueBinder binder = binder_;
        if (binder == Int32Binder.Default || binder == Int64Binder.Default) {
          return true;
        }
        if (binder == DoubleBinder.Default) {
          EcmaValueHandle handle = this.handle;
          return handle != EcmaValueHandle.NegativeInfinity && handle != EcmaValueHandle.PositiveInfinity && handle != EcmaValueHandle.NaN;
        }
        return false;
      }
    }

    [EcmaSpecification("IsInteger", EcmaSpecificationKind.AbstractOperations)]
    public bool IsInteger {
      get {
        switch (binder.GetNumberType(handle)) {
          case EcmaNumberType.Invalid:
            return false;
          case EcmaNumberType.Double:
            double d = Math.Abs(DoubleBinder.Default.FromHandle(handle));
            return Math.Floor(d) == d;
        }
        return true;
      }
    }

    [EcmaSpecification("IsCallable", EcmaSpecificationKind.AbstractOperations)]
    public bool IsCallable {
      get { return binder.IsCallable(handle); }
    }

    [EcmaSpecification("IsRegExp", EcmaSpecificationKind.AbstractOperations)]
    public bool IsRegExp {
      get { return GetUnderlyingObject() is EcmaRegExp; }
    }

    [EcmaSpecification("IsExtensible", EcmaSpecificationKind.AbstractOperations)]
    public bool IsExtensible {
      get { return binder.IsExtensible(handle); }
    }

    public string ToStringTag {
      get { return binder.GetToStringTag(handle); }
    }

    [DebuggerStepThrough]
    public object GetUnderlyingObject() {
      return binder.FromHandle(handle);
    }

    [DebuggerStepThrough]
    public bool HasProperty(EcmaPropertyKey name) {
      return binder.HasProperty(handle, name);
    }

    [DebuggerStepThrough]
    public bool HasOwnProperty(EcmaPropertyKey name) {
      return binder.HasOwnProperty(handle, name);
    }

    [EcmaSpecification("InstanceofOperator", EcmaSpecificationKind.RuntimeSemantics)]
    public bool InstanceOf(EcmaValue constructor) {
      if (constructor.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      RuntimeObject obj = constructor.ToObject();
      RuntimeObject instOfHandler = obj.GetMethod(Symbol.HasInstance);
      if (instOfHandler != null) {
        return instOfHandler.Call(constructor, this).ToBoolean();
      }
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      return obj.HasInstance(binder.ToRuntimeObject(handle));
    }

    public IEnumerable<EcmaPropertyKey> EnumerateKeys() {
      return binder.GetEnumerableOwnProperties(handle);
    }

    public IEnumerable<EcmaValue> EnumerateValues() {
      foreach (EcmaPropertyKey propertyKey in binder.GetEnumerableOwnProperties(handle)) {
        EcmaValue value;
        binder.TryGet(handle, propertyKey, out value);
        yield return value;
      }
    }

    public IEnumerable<EcmaPropertyEntry> EnumerateEntries() {
      foreach (EcmaPropertyKey propertyKey in binder.GetEnumerableOwnProperties(handle)) {
        EcmaValue value;
        binder.TryGet(handle, propertyKey, out value);
        yield return new EcmaPropertyEntry(propertyKey, value);
      }
    }

    [DebuggerStepThrough]
    public EcmaValue Call(EcmaValue thisArgs, params EcmaValue[] arguments) {
      return binder.Call(handle, thisArgs, arguments);
    }

    [DebuggerStepThrough]
    public EcmaValue Construct(params EcmaValue[] arguments) {
      if (this.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      return binder.ToRuntimeObject(handle).Construct(arguments);
    }

    [EcmaSpecification("Invoke", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue Invoke(EcmaPropertyKey propertyKey, params EcmaValue[] arguments) {
      if (this.IsNullOrUndefined) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      RuntimeObject obj = binder.ToRuntimeObject(handle);
      RuntimeObject method = obj.GetMethod(propertyKey);
      if (method == null) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction, propertyKey);
      }
      return method.Call(this, arguments);
    }

    public EcmaValue Or(EcmaValue other) {
      return ToBoolean() ? this : other;
    }

    public EcmaValue And(EcmaValue other) {
      return ToBoolean() ? other : this;
    }

    public EcmaIteratorEnumerator ForOf() {
      return ToObject().GetIterator();
    }

    public static bool Equals(EcmaValue x, EcmaValue y) {
      return Equals(x, y, EcmaValueComparison.Strict);
    }

    [EcmaSpecification("Abstract Equality Comparison", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.Abstract")]
    [EcmaSpecification("Strict Equality Comparison", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.Strict")]
    [EcmaSpecification("SameValue", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValue")]
    [EcmaSpecification("SameValueZero", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValueZero")]
    [EcmaSpecification("SameValueNonNumber", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValueNonNumber")]
    public static bool Equals(EcmaValue x, EcmaValue y, EcmaValueComparison mode) {
      if (mode == EcmaValueComparison.Abstract) {
        while (x.Type != y.Type) {
          if (x.IsNullOrUndefined && y.IsNullOrUndefined) {
            return true;
          }
          if (y.Type == EcmaValueType.Boolean || (x.Type == EcmaValueType.Number && y.Type == EcmaValueType.String)) {
            y = y.ToNumber();
            continue;
          }
          if (x.Type == EcmaValueType.Boolean || (x.Type == EcmaValueType.String && y.Type == EcmaValueType.Number)) {
            x = x.ToNumber();
            continue;
          }
          if (y.Type == EcmaValueType.Object && (x.Type == EcmaValueType.String || x.Type == EcmaValueType.Number || x.Type == EcmaValueType.Symbol)) {
            y = y.ToPrimitive();
            continue;
          }
          if (x.Type == EcmaValueType.Object && (y.Type == EcmaValueType.String || y.Type == EcmaValueType.Number || y.Type == EcmaValueType.Symbol)) {
            x = x.ToPrimitive();
            continue;
          }
          if (x.IsNullOrUndefined || y.IsNullOrUndefined) {
            return false;
          }
        }
      }
      if (x.binder == y.binder) {
        if (x.handle == y.handle) {
          return mode != EcmaValueComparison.Strict || x.binder != DoubleBinder.Default || x.handle != EcmaValueHandle.NaN;
        }
        if (x.binder == DoubleBinder.Default && (x.handle == EcmaValueHandle.NegativeZero || y.handle == EcmaValueHandle.NegativeZero)) {
          return mode != EcmaValueComparison.SameValue && (x.handle == EcmaValueHandle.PostiveZero || y.handle == EcmaValueHandle.PostiveZero);
        }
        return false;
      }
      if (x.Type != y.Type) {
        return false;
      }
      switch (x.Type) {
        case EcmaValueType.Undefined:
        case EcmaValueType.Null:
          return true;
        case EcmaValueType.Object:
          return ReferenceEquals(x.GetUnderlyingObject(), y.GetUnderlyingObject());
        case EcmaValueType.Number:
          switch (GetNumberCoercion(x, y)) {
            case EcmaNumberType.Double:
              double doubleX = x.ToDouble();
              double doubleY = y.ToDouble();
              if (Double.IsNaN(doubleX) && Double.IsNaN(doubleY)) {
                return mode != EcmaValueComparison.Strict && mode != EcmaValueComparison.SameValueNotNumber;
              }
              if (doubleX == 0 && doubleY == 0 && x.handle.Value != y.handle.Value) {
                return mode != EcmaValueComparison.SameValue;
              }
              return x.ToDouble().Equals(y.ToDouble());
            case EcmaNumberType.Int64:
              return x.ToInt64().Equals(y.ToInt64());
            case EcmaNumberType.Int32:
              return x.ToInt32().Equals(y.ToInt32());
          }
          break;
      }
      return x.GetUnderlyingObject().Equals(y.GetUnderlyingObject());
    }

    [EcmaSpecification("Abstract Relational Comparison", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue Compare(EcmaValue x, EcmaValue y) {
      if (x.IsNaN || y.IsNaN) {
        return Undefined;
      }
      return x.CompareTo(y) < 0 ? true : false;
    }

    public override int GetHashCode() {
      return binder.GetHashCode(handle);
    }

    public int GetHashCode(EcmaValueComparison kind) {
      if (kind == EcmaValueComparison.Abstract) {
        if (binder == NullBinder.Default || binder == UndefinedBinder.Default) {
          return 0;
        }
        if (binder == StringBinder.Default) {
          double d = ToDouble();
          if (Double.IsNaN(d)) {
            return binder.GetHashCode(handle);
          }
          return d.GetHashCode();
        }
        if (this.Type == EcmaValueType.Object) {
          return ToPrimitive().GetHashCode(kind);
        }
      }
      return GetHashCode();
    }

    public override bool Equals(object obj) {
      if (obj is EcmaValue value) {
        return Equals(this, value, EcmaValueComparison.Strict);
      }
      return base.Equals(obj);
    }

    public bool Equals(EcmaValue other) {
      return Equals(this, other, EcmaValueComparison.Strict);
    }

    public bool Equals(EcmaValue other, EcmaValueComparison mode) {
      return Equals(this, other, mode);
    }

    public int CompareTo(EcmaValue other) {
      EcmaValue x = this.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      EcmaValue y = other.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      switch (GetNumberCoercion(x, y)) {
        case EcmaNumberType.Double:
          return ToDouble().CompareTo(other.ToDouble());
        case EcmaNumberType.Int64:
          return ToInt64().CompareTo(other.ToInt64());
        case EcmaNumberType.Int32:
          return ToInt32().CompareTo(other.ToInt32());
        default:
          return ToString().CompareTo(other.ToString());
      }
    }

    [EcmaSpecification("ToObject", EcmaSpecificationKind.AbstractOperations)]
    public RuntimeObject ToObject() {
      return binder.ToRuntimeObject(handle);
    }

    public RuntimeObject ToObject(bool ignorePrimitive) {
      if (ignorePrimitive && this.Type != EcmaValueType.Object) {
        return null;
      }
      return ToObject();
    }

    [EcmaSpecification("ToString", EcmaSpecificationKind.AbstractOperations)]
    public override string ToString() {
      return binder.ToString(handle);
    }

    public EcmaValue ToPrimitive() {
      return binder.ToPrimitive(handle, EcmaPreferredPrimitiveType.Default);
    }

    [EcmaSpecification("ToPrimitive", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue ToPrimitive(EcmaPreferredPrimitiveType preferredType) {
      return binder.ToPrimitive(handle, preferredType);
    }

    [EcmaSpecification("ToNumber", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue ToNumber() {
      return binder.ToNumber(handle);
    }

    [EcmaSpecification("ToLength", EcmaSpecificationKind.AbstractOperations)]
    public long ToLength() {
      long length = ToInt64();
      if (length < 0) {
        return 0;
      }
      return Math.Min(length, NumberConstructor.MaxSafeInteger);
    }

    [EcmaSpecification("ToIndex", EcmaSpecificationKind.AbstractOperations)]
    public long ToIndex() {
      if (binder == UndefinedBinder.Default) {
        return 0;
      }
      long index = ToInt64();
      if (index < 0 || index > NumberConstructor.MaxSafeInteger) {
        throw new EcmaRangeErrorException("Number out of range");
      }
      return index;
    }

    [EcmaSpecification("ToBoolean", EcmaSpecificationKind.AbstractOperations)]
    public bool ToBoolean() {
      return binder.ToBoolean(handle);
    }

    public char ToChar() {
      return (char)ConvertToInt(0xFFFF, true);
    }

    [EcmaSpecification("ToInt8", EcmaSpecificationKind.AbstractOperations)]
    public sbyte ToInt8() {
      return (sbyte)ConvertToInt(0xFF, false);
    }

    [EcmaSpecification("ToUInt8", EcmaSpecificationKind.AbstractOperations)]
    public byte ToUInt8() {
      return (byte)ConvertToInt(0xFF, true);
    }

    [EcmaSpecification("ToInt16", EcmaSpecificationKind.AbstractOperations)]
    public short ToInt16() {
      return (short)ConvertToInt(UInt16.MaxValue, false);
    }

    [EcmaSpecification("ToUInt16", EcmaSpecificationKind.AbstractOperations)]
    public ushort ToUInt16() {
      return (ushort)ConvertToInt(UInt16.MaxValue, true);
    }

    [EcmaSpecification("ToInt32", EcmaSpecificationKind.AbstractOperations)]
    public int ToInt32() {
      return (int)ConvertToInt(UInt32.MaxValue, false);
    }

    [EcmaSpecification("ToUInt32", EcmaSpecificationKind.AbstractOperations)]
    public uint ToUInt32() {
      return (uint)ConvertToInt(UInt32.MaxValue, true);
    }

    public long ToInt64() {
      return binder.ToInt64(handle);
    }

    public double ToDouble() {
      return binder.ToDouble(handle);
    }

    public static object ChangeType(EcmaValue value, Type conversionType) {
      if (conversionType == typeof(EcmaValue)) {
        return value;
      }
      switch (System.Type.GetTypeCode(conversionType)) {
        case TypeCode.Boolean:
          return value.ToBoolean();
        case TypeCode.Byte:
          return value.ToUInt8();
        case TypeCode.Char:
          return value.ToChar();
        case TypeCode.Double:
          return value.ToDouble();
        case TypeCode.Int16:
          return value.ToInt16();
        case TypeCode.Int32:
          return value.ToInt32();
        case TypeCode.Int64:
          return value.ToInt64();
        case TypeCode.SByte:
          return value.ToInt8();
        case TypeCode.Single:
          return (float)value.ToDouble();
        case TypeCode.String:
          return value.ToString();
        case TypeCode.UInt16:
          return value.ToUInt16();
        case TypeCode.UInt32:
          return value.ToUInt32();
        case TypeCode.UInt64:
          return (ulong)value.ToInt64();
        case TypeCode.DateTime:
          if (value.GetUnderlyingObject() is EcmaDate dt) {
            return dt.Value;
          }
          break;
      }
      return EcmaValueUtility.ConvertToUnknownType(value, conversionType);
    }

    public static EcmaNumberType GetNumberCoercion(EcmaValue x) {
      return x.binder.GetNumberType(x.handle);
    }

    public static EcmaNumberType GetNumberCoercion(EcmaValue x, EcmaValue y) {
      EcmaNumberType typeX = x.binder.GetNumberType(y.handle);
      EcmaNumberType typeY = y.binder.GetNumberType(y.handle);
      if (typeX == EcmaNumberType.Invalid || typeY == EcmaNumberType.Invalid) {
        return EcmaNumberType.Invalid;
      }
      if (typeX == EcmaNumberType.Double || typeY == EcmaNumberType.Double) {
        return EcmaNumberType.Double;
      }
      if (typeX == EcmaNumberType.Int64 || typeY == EcmaNumberType.Int64) {
        return EcmaNumberType.Int64;
      }
      return EcmaNumberType.Int32;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay {
      get { return InspectorUtility.WriteValue(this); }
    }

    private long ConvertToInt(long mask, bool unsigned) {
      long longValue = 0;
      switch (binder.GetNumberType(handle)) {
        case EcmaNumberType.Invalid:
          double doubleValue = ToDouble();
          if (doubleValue == 0 || Double.IsNaN(doubleValue) || Double.IsInfinity(doubleValue)) {
            return 0;
          }
          longValue = ToInt64();
          break;
        case EcmaNumberType.Double:
          if (handle == EcmaValueHandle.PostiveZero || handle == EcmaValueHandle.NegativeZero || handle == EcmaValueHandle.NaN || handle == EcmaValueHandle.PositiveInfinity || handle == EcmaValueHandle.NegativeInfinity) {
            return 0;
          }
          longValue = ToInt64();
          break;
        case EcmaNumberType.Int64:
        case EcmaNumberType.Int32:
          longValue = ToInt64();
          break;
      }
      long mod = longValue % mask;
      return !unsigned && mod >= (mask >> 1) ? mod - mask : mod;
    }

    private static double MultiplyDouble(double x, double y) {
      try {
        return x * y;
      } catch (OverflowException) {
        return x < 0 ^ y < 0 ? Double.NegativeInfinity : Double.PositiveInfinity;
      }
    }

    private static readonly ConcurrentDictionary<Type, IEcmaValueBinder> binderTypes = new ConcurrentDictionary<Type, IEcmaValueBinder>(new Dictionary<Type, IEcmaValueBinder> {
      { typeof(WellKnownSymbol), WellKnownSymbolBinder.Default },
      { typeof(WellKnownPropertyName), WellKnownPropertyNameBinder.Default }
    });

    private static EcmaValue UnboxObject(object target) {
      switch (target) {
        case null:
          return EcmaValue.Undefined;
        case EcmaValue value:
          return value;
        case string str:
          return new EcmaValue(str);
        case Symbol sym:
          return new EcmaValue(sym);
        case RuntimeObject obj:
          return new EcmaValue(obj);
        case DateTime dt:
          return new EcmaDate(dt);
      }
      Type type = target.GetType();
      IEcmaValueBinder binder = null;
      if (type.IsEnum) {
        binder = binderTypes.GetOrAdd(type, t => (IEcmaValueBinder)Activator.CreateInstance(typeof(EnumBinder<>).MakeGenericType(t)));
      } else {
        switch (System.Type.GetTypeCode(type)) {
          case TypeCode.Boolean:
            binder = BooleanBinder.Default;
            break;
          case TypeCode.Byte:
          case TypeCode.SByte:
          case TypeCode.Char:
          case TypeCode.Int16:
          case TypeCode.Int32:
          case TypeCode.UInt16:
            binder = Int32Binder.Default;
            break;
          case TypeCode.Int64:
          case TypeCode.UInt32:
            binder = Int64Binder.Default;
            break;
          case TypeCode.UInt64:
          case TypeCode.Single:
          case TypeCode.Double:
            binder = DoubleBinder.Default;
            break;
          default:
            binder = RuntimeRealm.GetRuntimeObject(target);
            break;
        }
      }
      return new EcmaValue(binder.ToHandle(target), binder);
    }

    #region Operator overloading
    public static explicit operator string(EcmaValue value) {
      return value.ToString();
    }

    public static explicit operator bool(EcmaValue value) {
      return value.ToBoolean();
    }

    public static explicit operator double(EcmaValue value) {
      return value.ToDouble();
    }

    public static explicit operator int(EcmaValue value) {
      return value.ToInt32();
    }

    public static explicit operator long(EcmaValue value) {
      return value.ToInt64();
    }

    public static implicit operator EcmaValue(string value) {
      return new EcmaValue(value);
    }

    public static implicit operator EcmaValue(bool value) {
      return new EcmaValue(value);
    }

    public static implicit operator EcmaValue(double value) {
      return new EcmaValue(value);
    }

    public static implicit operator EcmaValue(int value) {
      return new EcmaValue(value);
    }

    public static implicit operator EcmaValue(long value) {
      return new EcmaValue(value);
    }

    public static implicit operator EcmaValue(RuntimeObject obj) {
      return new EcmaValue(obj);
    }

    public static bool operator ==(EcmaValue x, EcmaValue y) {
      return Equals(x, y);
    }

    public static bool operator !=(EcmaValue x, EcmaValue y) {
      return !Equals(x, y);
    }

    public static bool operator <(EcmaValue x, EcmaValue y) {
      return x.CompareTo(y) < 0;
    }

    public static bool operator >(EcmaValue x, EcmaValue y) {
      return x.CompareTo(y) > 0;
    }

    public static bool operator <=(EcmaValue x, EcmaValue y) {
      return x.CompareTo(y) <= 0;
    }

    public static bool operator >=(EcmaValue x, EcmaValue y) {
      return x.CompareTo(y) >= 0;
    }

    public static bool operator true(EcmaValue x) {
      return x.ToBoolean();
    }

    public static bool operator false(EcmaValue x) {
      return !x.ToBoolean();
    }

    public static EcmaValue operator +(EcmaValue x) {
      return x.Type == EcmaValueType.Number ? x : x.ToDouble();
    }

    public static EcmaValue operator -(EcmaValue x) {
      if (x.Type == EcmaValueType.Number) {
        switch (x.binder.GetNumberType(x.handle)) {
          case EcmaNumberType.Double:
            return -x.ToDouble();
          case EcmaNumberType.Int64:
            return x.ToInt64() == 0 ? EcmaValue.NegativeZero : new EcmaValue(-x.ToInt64());
          case EcmaNumberType.Int32:
            return x.ToInt32() == 0 ? EcmaValue.NegativeZero : new EcmaValue(-x.ToInt32());
        }
      }
      return -x.ToDouble();
    }

    public static EcmaValue operator +(EcmaValue x, EcmaValue y) {
      switch (GetNumberCoercion(x, y)) {
        case EcmaNumberType.Double:
          return new EcmaValue(x.ToDouble() + y.ToDouble());
        case EcmaNumberType.Int64:
          return new EcmaValue(x.ToInt64() + y.ToInt64());
        case EcmaNumberType.Int32:
          return new EcmaValue(x.ToInt32() + y.ToInt32());
        default:
          return new EcmaValue(x.ToString() + y.ToString());
      }
    }

    public static EcmaValue operator -(EcmaValue x, EcmaValue y) {
      x = +x;
      y = +y;
      switch (GetNumberCoercion(x, y)) {
        case EcmaNumberType.Double:
          return new EcmaValue(x.ToDouble() - y.ToDouble());
        case EcmaNumberType.Int64:
          return new EcmaValue(x.ToInt64() - y.ToInt64());
        case EcmaNumberType.Int32:
          return new EcmaValue(x.ToInt32() - y.ToInt32());
      }
      return NaN;
    }

    public static EcmaValue operator *(EcmaValue x, EcmaValue y) {
      x = +x;
      y = +y;
      switch (GetNumberCoercion(x, y)) {
        case EcmaNumberType.Double:
          return MultiplyDouble(x.ToDouble(), y.ToDouble());
        case EcmaNumberType.Int64:
          try {
            return new EcmaValue(x.ToInt64() * y.ToInt64());
          } catch (OverflowException) {
            return MultiplyDouble(x.ToDouble(), y.ToDouble());
          }
        case EcmaNumberType.Int32:
          try {
            return new EcmaValue(x.ToInt32() * y.ToInt32());
          } catch (OverflowException) {
            return MultiplyDouble(x.ToDouble(), y.ToDouble());
          }
      }
      return NaN;
    }

    public static EcmaValue operator /(EcmaValue x, EcmaValue y) {
      x = +x;
      y = +y;
      try {
        return new EcmaValue(x.ToDouble() / y.ToDouble());
      } catch (DivideByZeroException) {
        return new EcmaValue(x >= 0 ? Double.PositiveInfinity : Double.NegativeInfinity);
      }
    }

    public static EcmaValue operator %(EcmaValue x, EcmaValue y) {
      x = +x;
      y = +y;
      try {
        switch (GetNumberCoercion(x, y)) {
          case EcmaNumberType.Double:
            return new EcmaValue(x.ToDouble() % y.ToDouble());
          case EcmaNumberType.Int64:
            return new EcmaValue(x.ToInt64() % y.ToInt64());
          case EcmaNumberType.Int32:
            return new EcmaValue(x.ToInt32() % y.ToInt32());
        }
      } catch (DivideByZeroException) { }
      return NaN;
    }

    public static EcmaValue operator &(EcmaValue x, long y) {
      return new EcmaValue((+x).ToInt64() & y);
    }

    public static EcmaValue operator &(EcmaValue x, EcmaValue y) {
      return new EcmaValue((+x).ToInt64() & (+y).ToInt64());
    }

    public static EcmaValue operator |(EcmaValue x, long y) {
      return new EcmaValue((+x).ToInt64() | y);
    }

    public static EcmaValue operator |(EcmaValue x, EcmaValue y) {
      return new EcmaValue((+x).ToInt64() | (+y).ToInt64());
    }

    public static EcmaValue operator <<(EcmaValue x, int y) {
      return new EcmaValue(unchecked((+x).ToInt64() << y));
    }

    public static EcmaValue operator >>(EcmaValue x, int y) {
      return new EcmaValue((+x).ToInt64() >> y);
    }
    #endregion

    #region Interfaces
    IEnumerator<EcmaPropertyKey> IEnumerable<EcmaPropertyKey>.GetEnumerator() {
      return EnumerateKeys().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return EnumerateKeys().GetEnumerator();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
      object o = GetUnderlyingObject();
      info.AddValue("ut", o.GetType());
      info.AddValue("uo", o);
    }

    bool IEquatable<EcmaValue>.Equals(EcmaValue other) {
      return Equals(this, other, EcmaValueComparison.SameValue);
    }

    TypeCode IConvertible.GetTypeCode() {
      throw new NotSupportedException();
    }

    bool IConvertible.ToBoolean(IFormatProvider provider) {
      return ToBoolean();
    }

    char IConvertible.ToChar(IFormatProvider provider) {
      return ToChar();
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider) {
      return ToInt8();
    }

    byte IConvertible.ToByte(IFormatProvider provider) {
      return ToUInt8();
    }

    short IConvertible.ToInt16(IFormatProvider provider) {
      return ToInt16();
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider) {
      return ToUInt16();
    }

    int IConvertible.ToInt32(IFormatProvider provider) {
      return ToInt32();
    }

    uint IConvertible.ToUInt32(IFormatProvider provider) {
      return ToUInt32();
    }

    long IConvertible.ToInt64(IFormatProvider provider) {
      return ToInt64();
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider) {
      return (ulong)ToInt64();
    }

    float IConvertible.ToSingle(IFormatProvider provider) {
      return (float)ToDouble();
    }

    double IConvertible.ToDouble(IFormatProvider provider) {
      return ToDouble();
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider) {
      throw new NotSupportedException();
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider) {
      return (DateTime)ChangeType(this, typeof(DateTime));
    }

    string IConvertible.ToString(IFormatProvider provider) {
      return ToString();
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
      return ChangeType(this, conversionType);
    }
    #endregion
  }
}
