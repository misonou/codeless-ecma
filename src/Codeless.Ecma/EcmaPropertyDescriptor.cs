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
    Configurable = 4
  }

  public class EcmaPropertyDescriptor {
    public EcmaPropertyDescriptor()
      : this(EcmaValue.Undefined, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable) { }

    public EcmaPropertyDescriptor(EcmaValue data)
      : this(data, EcmaPropertyAttributes.Writable | EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable) { }

    public EcmaPropertyDescriptor(EcmaValue data, EcmaPropertyAttributes state) {
      this.Value = data;
      this.IsDataDescriptor = true;
      this.Writable = (state & EcmaPropertyAttributes.Writable) != 0;
      this.Configurable = (state & EcmaPropertyAttributes.Configurable) != 0;
      this.Enumerable = (state & EcmaPropertyAttributes.Enumerable) != 0;
    }

    public EcmaPropertyDescriptor(EcmaValue getter, EcmaValue setter)
      : this(getter, setter, EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable) { }

    public EcmaPropertyDescriptor(EcmaValue getter, EcmaValue setter, EcmaPropertyAttributes state) {
      this.Get = getter;
      this.Set = setter;
      this.IsAccessorDescriptor = true;
      this.Configurable = (state & EcmaPropertyAttributes.Configurable) != 0;
      this.Enumerable = (state & EcmaPropertyAttributes.Enumerable) != 0;
    }

    public bool IsAccessorDescriptor { get; private set; }
    public bool IsDataDescriptor { get; private set; }
    public bool? Enumerable { get; set; }
    public bool? Configurable { get; set; }
    public bool? Writable { get; set; }
    public EcmaValue Value { get; set; }
    public EcmaValue Get { get; set; }
    public EcmaValue Set { get; set; }

    public EcmaPropertyDescriptor Clone() {
      return (EcmaPropertyDescriptor)MemberwiseClone();
    }

    [EcmaSpecification("FromPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public EcmaValue ToValue() {
      RuntimeObject obj = new EcmaObject();
      if (IsDataDescriptor) {
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Value, this.Value);
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Writable, this.Writable.Value);
      }
      if (IsAccessorDescriptor) {
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Get, this.Get);
        obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Set, this.Set);
      }
      obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Enumerable, this.Enumerable.Value);
      obj.CreateDataPropertyOrThrow(WellKnownPropertyName.Configurable, this.Configurable.Value);
      return obj;
    }

    [EcmaSpecification("ToPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    [EcmaSpecification("CompletePropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaPropertyDescriptor FromValue(EcmaValue value) {
      Guard.ArgumentIsObject(value);
      EcmaPropertyDescriptor result = new EcmaPropertyDescriptor();
      if (value.HasProperty(WellKnownPropertyName.Enumerable)) {
        result.Enumerable = (bool)value[WellKnownPropertyName.Enumerable];
      }
      if (value.HasProperty(WellKnownPropertyName.Configurable)) {
        result.Enumerable = (bool)value[WellKnownPropertyName.Configurable];
      }
      if (value.HasProperty(WellKnownPropertyName.Value)) {
        result.Value = value[WellKnownPropertyName.Value];
        result.IsDataDescriptor = true;
      }
      if (value.HasProperty(WellKnownPropertyName.Writable)) {
        result.Writable = (bool)value[WellKnownPropertyName.Writable];
        result.IsDataDescriptor = true;
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
      if (result.Get.IsCallable || result.Set.IsCallable) {
        if (result.IsDataDescriptor) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidDescriptor);
        }
        result.IsAccessorDescriptor = true;
      }
      if (result.IsDataDescriptor) {
        result.Writable = result.Writable.GetValueOrDefault();
      }
      result.Configurable = result.Configurable.GetValueOrDefault();
      result.Enumerable = result.Enumerable.GetValueOrDefault();
      return result;
    }

    [EcmaSpecification("ValidateAndApplyPropertyDescriptor", EcmaSpecificationKind.AbstractOperations)]
    public static bool ValidateAndApplyPropertyDescriptor(ref EcmaPropertyDescriptor descriptor, EcmaPropertyDescriptor current, bool extensionPrevented) {
      if (current == null) {
        if (extensionPrevented) {
          return false;
        }
        return true;
      }
      if (current.Configurable == false) {
        if (descriptor.Configurable == true || (descriptor.Enumerable.HasValue && descriptor.Enumerable != current.Enumerable)) {
          return false;
        }
      }
      if (current.IsDataDescriptor != descriptor.IsDataDescriptor) {
        if (!current.Configurable.Value) {
          return false;
        }
        descriptor = (EcmaPropertyDescriptor)descriptor.MemberwiseClone();
        if (current.IsDataDescriptor) {
          descriptor.Configurable = current.Configurable;
          descriptor.Enumerable = current.Enumerable;
        }
        return true;
      }
      if (current.IsDataDescriptor) {
        if (!current.Configurable.Value && !current.Writable.Value) {
          return descriptor.Writable != true && descriptor.Value.Equals(current.Value, EcmaValueComparison.SameValue);
        }
      }
      if (current.IsAccessorDescriptor) {
        if (current.Configurable == false) {
          return descriptor.Set.Equals(current.Set, EcmaValueComparison.SameValue) &&
                 descriptor.Get.Equals(current.Get, EcmaValueComparison.SameValue);
        }
      }
      EcmaPropertyDescriptor n = (EcmaPropertyDescriptor)descriptor.MemberwiseClone();
      n.Configurable = descriptor.Configurable.GetValueOrDefault(current.Configurable.GetValueOrDefault());
      n.Writable = descriptor.Writable.GetValueOrDefault(current.Writable.GetValueOrDefault());
      if (n.IsAccessorDescriptor) {
        if (n.Get == default) {
          n.Get = current.Get;
        }
        if (n.Set == default) {
          n.Set = current.Set;
        }
      }
      descriptor = n;
      return true;
    }
  }
}
