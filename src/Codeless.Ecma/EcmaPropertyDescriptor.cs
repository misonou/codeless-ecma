using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [Flags]
  public enum EcmaPropertyAttributes {
    None = 0,
    Writable = 1,
    Enumerable = 2,
    Configurable = 4,
    HasValue = 8,
    HasGetter = 16,
    HasSetter = 32,
    HasConfigurable = 64,
    HasEnumerable = 128,
    HasWritable = 256,
    LazyInitialize = 512,
    DefaultDataProperty = Writable | Enumerable | Configurable,
    DefaultMethodProperty = Writable | Configurable
  }

  public class EcmaPropertyDescriptor {
    private static readonly EcmaPropertyDescriptor defaultProperty = new EcmaPropertyDescriptor(EcmaPropertyAttributes.DefaultDataProperty);
    private static readonly EcmaPropertyDescriptor frozenProperty = new EcmaPropertyDescriptor(EcmaPropertyAttributes.None);

    private EcmaPropertyAttributes attributes;
    private EcmaValue getter;
    private EcmaValue setter;

    public EcmaPropertyDescriptor() { }

    public EcmaPropertyDescriptor(EcmaPropertyAttributes attributes) {
      this.attributes = attributes | EcmaPropertyAttributes.HasWritable | EcmaPropertyAttributes.HasEnumerable | EcmaPropertyAttributes.HasConfigurable;
    }

    public EcmaPropertyDescriptor(EcmaValue data) {
      this.Value = data;
    }

    public EcmaPropertyDescriptor(EcmaValue data, EcmaPropertyAttributes attributes)
      : this(attributes) {
      if ((attributes & EcmaPropertyAttributes.LazyInitialize) != 0) {
        getter = data;
      } else {
        this.Value = data;
      }
    }

    public EcmaPropertyDescriptor(EcmaValue getter, EcmaValue setter) {
      this.Get = getter;
      this.Set = setter;
    }

    public EcmaPropertyDescriptor(EcmaValue getter, EcmaValue setter, EcmaPropertyAttributes attributes)
      : this(attributes) {
      this.Get = getter;
      this.Set = setter;
    }

    public bool IsAccessorDescriptor {
      get { return (attributes & (EcmaPropertyAttributes.HasGetter | EcmaPropertyAttributes.HasSetter)) != 0; }
    }

    public bool IsDataDescriptor {
      get { return (attributes & (EcmaPropertyAttributes.HasValue | EcmaPropertyAttributes.HasWritable | EcmaPropertyAttributes.LazyInitialize)) != 0; }
    }

    public bool Enumerable {
      get { return (attributes & EcmaPropertyAttributes.Enumerable) != 0; }
      set { attributes = (attributes & ~EcmaPropertyAttributes.Enumerable) | EcmaPropertyAttributes.HasEnumerable | (value ? EcmaPropertyAttributes.Enumerable : 0); }
    }

    public bool Configurable {
      get { return (attributes & EcmaPropertyAttributes.Configurable) != 0; }
      set { attributes = (attributes & ~EcmaPropertyAttributes.Configurable) | EcmaPropertyAttributes.HasConfigurable | (value ? EcmaPropertyAttributes.Configurable : 0); }
    }

    public bool Writable {
      get { return (attributes & EcmaPropertyAttributes.Writable) != 0; }
      set { attributes = (attributes & ~EcmaPropertyAttributes.Writable) | EcmaPropertyAttributes.HasWritable | (value ? EcmaPropertyAttributes.Writable : 0); }
    }

    public bool HasValue {
      get { return (attributes & EcmaPropertyAttributes.HasValue) != 0; }
    }

    public EcmaValue Value {
      get {
        if ((attributes & EcmaPropertyAttributes.LazyInitialize) != 0) {
          getter = getter.Call(EcmaValue.Undefined);
          attributes = attributes | EcmaPropertyAttributes.HasValue & ~EcmaPropertyAttributes.LazyInitialize;
        }
        return this.IsDataDescriptor ? getter : default;
      }
      set {
        attributes = attributes | EcmaPropertyAttributes.HasValue & ~EcmaPropertyAttributes.LazyInitialize;
        getter = value;
      }
    }

    public EcmaValue Get {
      get {
        return this.IsAccessorDescriptor ? getter : default;
      }
      set {
        attributes = attributes & ~EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.HasGetter;
        getter = value;
      }
    }

    public EcmaValue Set {
      get {
        return this.IsAccessorDescriptor ? setter : default;
      }
      set {
        attributes = attributes & ~EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.HasSetter;
        setter = value;
      }
    }

    public EcmaPropertyDescriptor Clone() {
      return (EcmaPropertyDescriptor)MemberwiseClone();
    }

    [EcmaSpecification("FromPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue ToValue() {
      RuntimeObject obj = new EcmaObject();
      if (IsAccessorDescriptor) {
        obj.CreateDataPropertyOrThrow(WellKnownProperty.Get, this.Get);
        obj.CreateDataPropertyOrThrow(WellKnownProperty.Set, this.Set);
      } else {
        obj.CreateDataPropertyOrThrow(WellKnownProperty.Value, this.Value);
        obj.CreateDataPropertyOrThrow(WellKnownProperty.Writable, this.Writable);
      }
      obj.CreateDataPropertyOrThrow(WellKnownProperty.Enumerable, this.Enumerable);
      obj.CreateDataPropertyOrThrow(WellKnownProperty.Configurable, this.Configurable);
      return obj;
    }

    [EcmaSpecification("ToPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaPropertyDescriptor FromValue(EcmaValue value) {
      Guard.ArgumentIsObject(value);
      EcmaPropertyDescriptor result = new EcmaPropertyDescriptor();
      if (value.HasProperty(WellKnownProperty.Enumerable)) {
        result.Enumerable = (bool)value[WellKnownProperty.Enumerable];
      }
      if (value.HasProperty(WellKnownProperty.Configurable)) {
        result.Configurable = (bool)value[WellKnownProperty.Configurable];
      }
      if (value.HasProperty(WellKnownProperty.Value)) {
        result.Value = value[WellKnownProperty.Value];
      }
      if (value.HasProperty(WellKnownProperty.Writable)) {
        result.Writable = (bool)value[WellKnownProperty.Writable];
      }
      if (value.HasProperty(WellKnownProperty.Get)) {
        EcmaValue getter = value[WellKnownProperty.Get];
        if (!getter.IsCallable && getter.Type != EcmaValueType.Undefined) {
          throw new EcmaTypeErrorException(InternalString.Error.SetterMustBeFunction);
        }
        result.Get = getter;
      }
      if (value.HasProperty(WellKnownProperty.Set)) {
        EcmaValue setter = value[WellKnownProperty.Set];
        if (!setter.IsCallable && setter.Type != EcmaValueType.Undefined) {
          throw new EcmaTypeErrorException(InternalString.Error.GetterMustBeFunction);
        }
        result.Set = setter;
      }
      if (result.IsAccessorDescriptor && result.IsDataDescriptor) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidDescriptor);
      }
      result.CompleteDescriptorAttributes(frozenProperty.attributes);
      return result;
    }

    [EcmaSpecification("ValidateAndApplyPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static bool ValidateAndApplyPropertyDescriptor(ref EcmaPropertyDescriptor descriptor, EcmaPropertyDescriptor current, bool extensionPrevented) {
      if (current == null) {
        if (extensionPrevented) {
          return false;
        }
        descriptor = (EcmaPropertyDescriptor)descriptor.MemberwiseClone();
        descriptor.CompleteDescriptorAttributes(defaultProperty.attributes);
        return true;
      }
      if (!current.Configurable) {
        if (descriptor.Configurable || ((descriptor.attributes & EcmaPropertyAttributes.HasEnumerable) != 0 && descriptor.Enumerable != current.Enumerable)) {
          return false;
        }
      }
      if ((descriptor.IsDataDescriptor || descriptor.IsAccessorDescriptor) && current.IsDataDescriptor != descriptor.IsDataDescriptor) {
        if (!current.Configurable) {
          return false;
        }
      } else if (current.IsDataDescriptor) {
        if (!current.Configurable && !current.Writable) {
          if (descriptor.Writable == true || ((descriptor.attributes & EcmaPropertyAttributes.HasValue) != 0 && !descriptor.Value.Equals(current.Value, EcmaValueComparison.SameValue))) {
            return false;
          }
        }
      } else if (current.IsAccessorDescriptor) {
        if (current.Configurable == false) {
          if (((descriptor.attributes & EcmaPropertyAttributes.HasSetter) != 0 && !descriptor.Set.Equals(current.Set, EcmaValueComparison.SameValue)) ||
              ((descriptor.attributes & EcmaPropertyAttributes.HasGetter) != 0 && !descriptor.Get.Equals(current.Get, EcmaValueComparison.SameValue))) {
            return false;
          }
        }
      }
      descriptor = (EcmaPropertyDescriptor)descriptor.MemberwiseClone();
      descriptor.CompleteDescriptor(current);
      return true;
    }

    internal EcmaPropertyDescriptor EnsureSharedValue(RuntimeRealm realm) {
      bool isGetterShared = getter.Type == SharedIntrinsicObjectBinder.SharedValue;
      bool isSetterShared = setter.Type == SharedIntrinsicObjectBinder.SharedValue;
      if (isGetterShared || isSetterShared) {
        EcmaPropertyDescriptor clone = Clone();
        if (isGetterShared) {
          clone.getter = realm.GetRuntimeObject((WellKnownObject)getter.ToInt32());
        }
        if (isSetterShared) {
          clone.setter = realm.GetRuntimeObject((WellKnownObject)setter.ToInt32());
        }
        return clone;
      }
      return this;
    }

    [EcmaSpecification("CompletePropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    private void CompleteDescriptor(EcmaPropertyDescriptor current) {
      EcmaPropertyAttributes sourceAttrs = current.attributes;
      if (!IsDataDescriptor && current.IsAccessorDescriptor) {
        if ((attributes & EcmaPropertyAttributes.HasGetter) == 0) {
          this.Get = current.Get;
        }
        if ((attributes & EcmaPropertyAttributes.HasSetter) == 0) {
          this.Set = current.Set;
        }
      }
      if (!IsAccessorDescriptor && (attributes & EcmaPropertyAttributes.HasValue) == 0) {
        this.Value = current.Value;
      }
      CompleteDescriptorAttributes(sourceAttrs);
    }

    private void CompleteDescriptorAttributes(EcmaPropertyAttributes sourceAttrs) {
      if ((attributes & EcmaPropertyAttributes.HasConfigurable) == 0) {
        attributes |= EcmaPropertyAttributes.HasConfigurable | (sourceAttrs & EcmaPropertyAttributes.Configurable);
      }
      if ((attributes & EcmaPropertyAttributes.HasEnumerable) == 0) {
        attributes |= EcmaPropertyAttributes.HasEnumerable | (sourceAttrs & EcmaPropertyAttributes.Enumerable);
      }
      if ((attributes & EcmaPropertyAttributes.HasWritable) == 0 && IsDataDescriptor) {
        attributes |= EcmaPropertyAttributes.HasWritable | (sourceAttrs & EcmaPropertyAttributes.Writable);
      }
    }
  }
}
