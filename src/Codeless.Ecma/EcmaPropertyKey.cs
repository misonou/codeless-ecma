using Codeless.Ecma.Native;
using Codeless.Ecma.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [DebuggerStepThrough]
  [DebuggerDisplay("{Name,nq}")]
  public struct EcmaPropertyKey : IEquatable<EcmaPropertyKey> {
    private readonly EcmaValue value;

    public EcmaPropertyKey(WellKnownSymbol value)
      : this(new EcmaValue(new EcmaValueHandle((long)value), WellKnownSymbolBinder.Default)) { }

    public EcmaPropertyKey(Symbol value)
      : this(new EcmaValue(value)) { }

    public EcmaPropertyKey(string value)
      : this() {
      if (UInt32.TryParse(value, out uint number) && number < UInt32.MaxValue) {
        this.value = number;
      } else {
        this.value = value;
      }
    }

    public EcmaPropertyKey(int value)
      : this(value >= 0 ? new EcmaValue(value) : new EcmaValue(value.ToString())) { }

    public EcmaPropertyKey(long value)
      : this(value >= 0 && value < UInt32.MaxValue ? new EcmaValue(value) : new EcmaValue(value.ToString())) { }

    private EcmaPropertyKey(EcmaValue value)
      : this() {
      this.value = value;
    }

    public bool IsSymbol {
      get { return value.Type == EcmaValueType.Symbol; }
    }

    public bool IsArrayIndex {
      get { return value.Type == EcmaValueType.Number; }
    }

    public Symbol Symbol {
      get {
        if (!IsSymbol) {
          throw new InvalidOperationException();
        }
        return (Symbol)value.GetUnderlyingObject();
      }
    }

    public string Name {
      get { return value.ToString(); }
    }

    public long ToArrayIndex() {
      if (!IsArrayIndex) {
        throw new InvalidOperationException();
      }
      return value.ToInt64();
    }

    [EcmaSpecification("CanonicalNumericIndexString", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue ToCanonicalNumericIndex() {
      if (value.Type == EcmaValueType.Number) {
        return value.ToInt64();
      }
      if (value.Type == EcmaValueType.Symbol) {
        return default;
      }
      if (Double.TryParse(value.ToString(), out double doubleValue) && DoubleBinder.Default.ToString(doubleValue) == value.ToString()) {
        return doubleValue;
      }
      return default;
    }

    public EcmaValue ToValue() {
      return value.Type == EcmaValueType.Number ? value.ToString() : value;
    }

    [EcmaSpecification("ToPropertyKey", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaPropertyKey FromValue(EcmaValue v) {
      switch (v.Type) {
        case EcmaValueType.Object:
          return FromValue(v.ToPrimitive(EcmaPreferredPrimitiveType.String));
        case EcmaValueType.Symbol:
          return new EcmaPropertyKey(v);
        case EcmaValueType.Number:
          if (v.IsInteger && v >= 0 && v < UInt32.MaxValue) {
            return new EcmaPropertyKey(v);
          }
          break;
      }
      return new EcmaPropertyKey(v.ToString());
    }

    [EcmaSpecification("IsPropertyKey", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsPropertyKey(EcmaValue v) {
      return v.Type == EcmaValueType.Symbol || v.Type == EcmaValueType.String;
    }

    public bool Equals(EcmaPropertyKey other) {
      return EcmaValue.Equals(other.value, value, EcmaValueComparison.SameValue);
    }

    public override bool Equals(object obj) {
      return obj is EcmaPropertyKey ? Equals((EcmaPropertyKey)obj) : base.Equals(obj);
    }

    public override int GetHashCode() {
      return value.GetHashCode();
    }

    public override string ToString() {
      return value.ToString();
    }

    public static bool operator ==(EcmaPropertyKey x, EcmaPropertyKey y) {
      return x.Equals(y);
    }

    public static bool operator !=(EcmaPropertyKey x, EcmaPropertyKey y) {
      return !x.Equals(y);
    }

    public static implicit operator EcmaPropertyKey(WellKnownSymbol value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(Symbol value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(string value) {
      return new EcmaPropertyKey(value);
    }

    public static implicit operator EcmaPropertyKey(int index) {
      return new EcmaPropertyKey(index);
    }

    public static implicit operator EcmaPropertyKey(long index) {
      return new EcmaPropertyKey(index);
    }
  }
}
