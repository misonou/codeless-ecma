using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Diagnostics.VisualStudio;
using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

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
    Default,
    NoCoercion,
    SameValue,
    SameValueZero,
    SameValueNotNumber
  }

  public enum EcmaPreferredPrimitiveType {
    Default,
    String,
    Number
  }

  public enum EcmaPropertyEnumerateKind {
    Key,
    Value,
    Entry
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
    public static readonly EcmaValue Undefined = default(EcmaValue);
    public static readonly EcmaValue Null = new EcmaValue(EcmaValueHandle.Null, NullBinder.Default);
    public static readonly EcmaValue NaN = new EcmaValue(EcmaValueHandle.NaN, NativeDoubleBinder.Default);
    public static readonly EcmaValue Infinity = new EcmaValue(EcmaValueHandle.PositiveInfinity, NativeDoubleBinder.Default);
    public static readonly EcmaValue NegativeZero = new EcmaValue(EcmaValueHandle.NegativeZero, NativeDoubleBinder.Default);
    public static readonly EcmaValue[] EmptyArray = new EcmaValue[0];

    private readonly EcmaValueHandle handle;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IEcmaValueBinder binder_;

    public EcmaValue(bool value) {
      this.handle = NativeBooleanBinder.Default.ToHandle(value);
      this.binder_ = NativeBooleanBinder.Default;
    }

    public EcmaValue(byte value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(sbyte value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(char value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(float value) {
      this.handle = NativeDoubleBinder.Default.ToHandle(value);
      this.binder_ = NativeDoubleBinder.Default;
    }

    public EcmaValue(double value) {
      this.handle = NativeDoubleBinder.Default.ToHandle(value);
      this.binder_ = NativeDoubleBinder.Default;
    }

    public EcmaValue(short value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(ushort value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(int value) {
      this.handle = NativeInt32Binder.Default.ToHandle(value);
      this.binder_ = NativeInt32Binder.Default;
    }

    public EcmaValue(uint value) {
      this.handle = NativeInt64Binder.Default.ToHandle(value);
      this.binder_ = NativeInt64Binder.Default;
    }

    public EcmaValue(long value) {
      this.handle = NativeInt64Binder.Default.ToHandle(value);
      this.binder_ = NativeInt64Binder.Default;
    }

    public EcmaValue(ulong value) {
      this.handle = NativeDoubleBinder.Default.ToHandle(value);
      this.binder_ = NativeDoubleBinder.Default;
    }

    public EcmaValue(object value) {
      EcmaValue resolved = UnboxObject(value);
      this.handle = resolved.handle;
      this.binder_ = resolved.binder;
    }

    public EcmaValue(SerializationInfo info, StreamingContext context) {
      Type type = (Type)info.GetValue("ut", typeof(Type));
      object value = info.GetValue("uo", type);
      EcmaValue resolved = UnboxObject(value);
      this.handle = resolved.handle;
      this.binder_ = resolved.binder;
    }

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
      get {
        EcmaValue value;
        binder.TryGet(handle, index, out value);
        return value;
      }
      set {
        if (!binder.TrySet(handle, index, value)) {
          throw new EcmaTypeErrorException(InternalString.Error.SetProperty);
        }
      }
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
      get { return binder == UndefinedBinder.Default || binder == NullBinder.Default; }
    }

    public bool IsPrimitive {
      get { return binder == UndefinedBinder.Default || binder == NullBinder.Default || binder.GetValueType(handle) != EcmaValueType.Object; }
    }

    public bool IsArrayLike {
      get { return binder.GetValueType(handle) == EcmaValueType.Object && binder.HasOwnProperty(handle, WellKnownPropertyName.Length); }
    }

    public bool IsNaN {
      get { return handle == EcmaValueHandle.NaN && binder.GetNumberType(handle) == EcmaNumberType.Double; }
    }

    public bool IsFinite {
      get {
        EcmaValue value = ToNumber();
        return !value.IsNaN && value != EcmaValue.Infinity && value != -EcmaValue.Infinity;
      }
    }

    [EcmaSpecification("IsInteger", EcmaSpecificationKind.AbstractOperations)]
    public bool IsInteger {
      get {
        if (binder.GetNumberType(handle) == EcmaNumberType.Invalid) {
          return false;
        }
        if (binder.GetNumberType(handle) == EcmaNumberType.Double) {
          double d = Math.Abs(NativeDoubleBinder.Default.FromHandle(handle));
          return Math.Floor(d) == d;
        }
        return true;
      }
    }

    [EcmaSpecification("IsCallable", EcmaSpecificationKind.AbstractOperations)]
    public bool IsCallable {
      get { return this.Type == EcmaValueType.Object && binder.IsCallable(handle); }
    }

    [EcmaSpecification("IsRegExp", EcmaSpecificationKind.AbstractOperations)]
    public bool IsRegExp {
      get { return GetUnderlyingObject() is EcmaRegExp; }
    }

    [EcmaSpecification("IsExtensible", EcmaSpecificationKind.AbstractOperations)]
    public bool IsExtensible {
      get { return this.Type == EcmaValueType.Object && binder.IsExtensible(handle); }
    }

    public string ToStringTag {
      get { return binder.GetToStringTag(handle); }
    }

    public object GetUnderlyingObject() {
      object obj = binder.FromHandle(handle);
      //ITransientRuntimeObject tObj = obj as ITransientRuntimeObject;
      //if (tObj != null) {
      //  return tObj.PrimitiveData.GetUnderlyingObject();
      //}
      return obj;
    }

    public bool HasProperty(EcmaPropertyKey name) {
      return binder.HasProperty(handle, name);
    }

    public bool HasOwnProperty(EcmaPropertyKey name) {
      return binder.HasOwnProperty(handle, name);
    }

    [EcmaSpecification("InstanceofOperator", EcmaSpecificationKind.RuntimeSemantics)]
    public bool InstanceOf(EcmaValue constructor) {
      if (constructor.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException("");
      }
      RuntimeObject obj = constructor.ToRuntimeObject();
      EcmaValue instOfHandler = obj.GetMethod(Symbol.HasInstance);
      if (!instOfHandler.IsNullOrUndefined) {
        return instOfHandler.Call(constructor, this).ToBoolean();
      }
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException("");
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

    public EcmaValue Call(EcmaValue thisArgs, params EcmaValue[] arguments) {
      return binder.Call(handle, thisArgs, arguments);
    }

    [EcmaSpecification("Invoke", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue Invoke(EcmaPropertyKey propertyKey, params EcmaValue[] arguments) {
      RuntimeObject obj = binder.ToRuntimeObject(handle);
      EcmaValue method = obj.GetMethod(propertyKey);
      if (method.IsNullOrUndefined) {
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

    public static bool Equals(EcmaValue x, EcmaValue y) {
      return Equals(x, y, EcmaValueComparison.Default);
    }

    [EcmaSpecification("Abstract Equality Comparison", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.Default")]
    [EcmaSpecification("Strict Equality Comparison", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.NoCoercion")]
    [EcmaSpecification("SameValue", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValue")]
    [EcmaSpecification("SameValueZero", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValueZero")]
    [EcmaSpecification("SameValueNonNumber", EcmaSpecificationKind.AbstractOperations,
      Description = "Equivalent to calling Equals with EcmaValueComparison.SameValueNonNumber")]
    public static bool Equals(EcmaValue x, EcmaValue y, EcmaValueComparison mode) {
      if (mode == EcmaValueComparison.Default) {
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
          return mode != EcmaValueComparison.NoCoercion || x.binder != NativeDoubleBinder.Default || x.handle != EcmaValueHandle.NaN;
        }
        if (x.binder == NativeDoubleBinder.Default && (x.handle == EcmaValueHandle.NegativeZero || y.handle == EcmaValueHandle.NegativeZero)) {
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
                return mode != EcmaValueComparison.NoCoercion && mode != EcmaValueComparison.SameValueNotNumber;
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

    public bool Equals(EcmaValue other) {
      return Equals(this, other);
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
    public RuntimeObject ToRuntimeObject() {
      return binder.ToRuntimeObject(handle);
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
        throw new EcmaRangeErrorException("");
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

    [EcmaSpecification("ToInteger", EcmaSpecificationKind.AbstractOperations)]
    public long ToInt64() {
      return binder.ToInt64(handle);
    }

    public double ToDouble() {
      return binder.ToDouble(handle);
    }

    [EcmaSpecification("RequireObjectCoercible", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue RequireObjectCoercible() {
      if (this.IsNullOrUndefined) {
        throw new EcmaTypeErrorException(InternalString.Error.NotCoercibleAsObject);
      }
      return this;
    }

    public static object ChangeType(EcmaValue value, Type conversionType) {
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
          if (value.binder == NativeDateTimeBinder.Default) {
            return value.binder.FromHandle(value.handle);
          }
          break;
      }
      if (conversionType == typeof(EcmaValue)) {
        return value;
      }
      if (conversionType == typeof(RuntimeObject)) {
        return value.ToRuntimeObject();
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

    private static EcmaValue UnboxObject(object target) {
      if (target == null) {
        return Undefined;
      }
      if (target.GetType() == typeof(EcmaValue)) {
        return (EcmaValue)target;
      }
      IEcmaValueBinder binder = EcmaValueUtility.GetBinder(target);
      return new EcmaValue(binder.ToHandle(target), binder);
    }

    #region Object Operations
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

    public override bool Equals(object obj) {
      if (obj is EcmaValue) {
        return Equals(this, (EcmaValue)obj, EcmaValueComparison.SameValue);
      }
      return base.Equals(obj);
    }

    public override int GetHashCode() {
      return binder.GetHashCode(handle);
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
      throw new NotImplementedException();
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
      throw new NotSupportedException();
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
