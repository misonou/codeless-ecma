using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Codeless.Ecma {
  [Serializable]
  [DebuggerStepThrough]
  [DebuggerDisplay("{Value}")]
  public struct EcmaValueHandle : IEquatable<EcmaValueHandle> {
    public readonly long Value;

    public static readonly EcmaValueHandle Undefined = new EcmaValueHandle(0);
    public static readonly EcmaValueHandle Null = new EcmaValueHandle(1);
    public static readonly EcmaValueHandle NaN = new EcmaValueHandle(unchecked((long)0xFFF8000000000000));
    public static readonly EcmaValueHandle NegativeInfinity = new EcmaValueHandle(unchecked((long)0xFFF0000000000000));
    public static readonly EcmaValueHandle PositiveInfinity = new EcmaValueHandle(unchecked((long)0x7FF0000000000000));
    public static readonly EcmaValueHandle PostiveZero = new EcmaValueHandle(BitConverter.DoubleToInt64Bits(0.0));
    public static readonly EcmaValueHandle NegativeZero = new EcmaValueHandle(BitConverter.DoubleToInt64Bits(-0.0));

    public EcmaValueHandle(long value) {
      Value = value;
    }
    
    public bool Equals(EcmaValueHandle other) {
      return Value == other.Value;
    }

    public override bool Equals(object obj) {
      return obj is EcmaValueHandle && Value == ((EcmaValueHandle)obj).Value;
    }

    public override int GetHashCode() {
      return Value.GetHashCode();
    }

    [DebuggerStepThrough]
    public object GetTargetAsGCHandle() {
      if (this.Value != Undefined.Value && this.Value != Null.Value) {
        GCHandle h = GCHandle.FromIntPtr(new IntPtr(this.Value));
        if (h.IsAllocated && h.Target != null) {
          return h.Target;
        }
      }
      throw new InvalidOperationException();
    }

    public static bool operator ==(EcmaValueHandle a, EcmaValueHandle b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(EcmaValueHandle a, EcmaValueHandle b) {
      return a.Value != b.Value;
    }
  }
}
