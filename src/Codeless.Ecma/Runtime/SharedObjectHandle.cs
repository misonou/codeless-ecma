using System;

namespace Codeless.Ecma.Runtime {
  public readonly struct SharedObjectHandle : IEquatable<SharedObjectHandle> {
    public static readonly SharedObjectHandle Null = new SharedObjectHandle(0);
    public static readonly SharedObjectHandle ObjectPrototype = new SharedObjectHandle(WellKnownObject.ObjectPrototype);

    internal SharedObjectHandle(WellKnownObject knownObject) {
      this.HandleValue = (int)knownObject;
    }

    internal SharedObjectHandle(int high, int low) {
      this.HandleValue = (high << 16) | low;
    }

    private SharedObjectHandle(int value) {
      this.HandleValue = value;
    }

    internal int HandleValue { get; }

    public EcmaValue ToValue() {
      return new EcmaValue(new EcmaValueHandle(this.HandleValue), SharedIntrinsicObjectBinder.Default);
    }

    public bool Equals(SharedObjectHandle other) {
      return this.HandleValue == other.HandleValue;
    }

    public override bool Equals(object obj) {
      return obj is SharedObjectHandle handle && Equals(handle);
    }

    public override int GetHashCode() {
      return this.HandleValue.GetHashCode();
    }

    public static SharedObjectHandle FromValue(EcmaValue sharedValue) {
      if (sharedValue.Type != SharedIntrinsicObjectBinder.SharedValue) {
        throw new ArgumentException("Value must be created from SharedObjectHandle.ToValue()", "sharedValue");
      }
      return new SharedObjectHandle(sharedValue.ToInt32());
    }

    public static implicit operator SharedObjectHandle(WellKnownObject knownObject) {
      return new SharedObjectHandle(knownObject);
    }

    public static bool operator ==(SharedObjectHandle x, SharedObjectHandle y) {
      return x.Equals(y);
    }

    public static bool operator !=(SharedObjectHandle x, SharedObjectHandle y) {
      return !x.Equals(y);
    }
  }
}
