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
      this.Value = data;
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
      get { return (attributes & EcmaPropertyAttributes.HasValue) != 0; }
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

    public EcmaValue Value {
      get {
        return this.IsDataDescriptor ? getter : default;
      }
      set {
        attributes |= EcmaPropertyAttributes.HasValue;
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
      if (IsDataDescriptor) {
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Value, this.Value);
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Writable, this.Writable);
      }
      if (IsAccessorDescriptor) {
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Get, this.Get);
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Set, this.Set);
      }
      obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Enumerable, this.Enumerable);
      obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Configurable, this.Configurable);
      return obj;
    }

    [EcmaSpecification("ToPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaPropertyDescriptor FromValue(EcmaValue value) {
      Guard.ArgumentIsObject(value);
      EcmaPropertyDescriptor result = new EcmaPropertyDescriptor();
      if (value.HasProperty(WellKnownPropertyName.Enumerable)) {
        result.Enumerable = (bool)value[WellKnownPropertyName.Enumerable];
      }
      if (value.HasProperty(WellKnownPropertyName.Configurable)) {
        result.Configurable = (bool)value[WellKnownPropertyName.Configurable];
      }
      if (value.HasProperty(WellKnownPropertyName.Value)) {
        result.Value = value[WellKnownPropertyName.Value];
      }
      if (value.HasProperty(WellKnownPropertyName.Writable)) {
        result.Writable = (bool)value[WellKnownPropertyName.Writable];
        if (!result.IsDataDescriptor) {
          result.Value = EcmaValue.Undefined;
        }
      }
      if (value.HasProperty(WellKnownPropertyName.Get)) {
        EcmaValue getter = value[WellKnownPropertyName.Get];
        if (!getter.IsCallable && getter.Type != EcmaValueType.Undefined) {
          throw new EcmaTypeErrorException(InternalString.Error.SetterMustBeFunction);
        }
        result.Get = getter;
      }
      if (value.HasProperty(WellKnownPropertyName.Set)) {
        EcmaValue setter = value[WellKnownPropertyName.Set];
        if (!setter.IsCallable && setter.Type != EcmaValueType.Undefined) {
          throw new EcmaTypeErrorException(InternalString.Error.GetterMustBeFunction);
        }
        result.Set = setter;
      }
      if (result.IsAccessorDescriptor && result.IsDataDescriptor) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidDescriptor);
      }
      result.CompleteDescriptor(false);
      return result;
    }

    [EcmaSpecification("ValidateAndApplyPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static bool ValidateAndApplyPropertyDescriptor(ref EcmaPropertyDescriptor descriptor, EcmaPropertyDescriptor current, bool extensionPrevented) {
      if (current == null) {
        if (extensionPrevented) {
          return false;
        }
        descriptor = (EcmaPropertyDescriptor)descriptor.MemberwiseClone();
        descriptor.CompleteDescriptor(true);
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
          if (descriptor.Writable == true || !descriptor.Value.Equals(current.Value, EcmaValueComparison.SameValue)) {
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

    public void CompleteDescriptor(bool defaultAttribute) {
      CompleteDescriptor(defaultAttribute ? defaultProperty : frozenProperty);
    }

    [EcmaSpecification("CompletePropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public void CompleteDescriptor(EcmaPropertyDescriptor current) {
      EcmaPropertyAttributes sourceAttrs = current.attributes;
      if (!IsDataDescriptor) {
        if (current.IsAccessorDescriptor) {
          if ((attributes & EcmaPropertyAttributes.HasGetter) == 0) {
            this.Get = current.Get;
          }
          if ((attributes & EcmaPropertyAttributes.HasSetter) == 0) {
            this.Set = current.Set;
          }
        } else if (!IsAccessorDescriptor) {
          this.Value = current.Value;
        }
      }
      if ((attributes & EcmaPropertyAttributes.HasConfigurable) == 0) {
        attributes |= EcmaPropertyAttributes.HasConfigurable | (sourceAttrs & EcmaPropertyAttributes.Configurable);
      }
      if ((attributes & EcmaPropertyAttributes.HasEnumerable) == 0) {
        attributes |= EcmaPropertyAttributes.HasEnumerable | (sourceAttrs & EcmaPropertyAttributes.Enumerable);
      }
      if ((attributes & EcmaPropertyAttributes.HasWritable) == 0) {
        attributes |= EcmaPropertyAttributes.HasWritable | (IsDataDescriptor ? (sourceAttrs & EcmaPropertyAttributes.Writable) : 0);
      }
    }
  }
}
