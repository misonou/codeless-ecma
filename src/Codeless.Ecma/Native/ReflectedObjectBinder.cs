using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  public class ReflectedObjectBinder : ObjectBinder {
    private readonly Dictionary<EcmaPropertyKey, PropertyDescriptor> visibleMembers = new Dictionary<EcmaPropertyKey, PropertyDescriptor>();

    public ReflectedObjectBinder(Type type)
      : this(type, false) { }

    public ReflectedObjectBinder(Type type, bool defaultHideMembers) {
      if (!this.RestrictedType.IsAssignableFrom(type)) {
        throw new ArgumentException("type");
      }
      PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
      bool showAttributedOnly = defaultHideMembers || properties.OfType<PropertyDescriptor>().Any(v => v.Attributes[typeof(IntrinsicMemberAttribute)] != null);
      foreach (PropertyDescriptor property in properties) {
        IntrinsicMemberAttribute attribute = (IntrinsicMemberAttribute)property.Attributes[typeof(IntrinsicMemberAttribute)];
        if (!showAttributedOnly || attribute != null) {
          string visibleName = attribute != null ? attribute.Name : property.Name;
          visibleMembers[visibleName] = property;
        }
      }
    }

    protected virtual Type RestrictedType {
      get { return typeof(object); }
    }

    protected override RuntimeObject ToRuntimeObject(object target) {
      return new IntrinsicObject(new EcmaValue(target), WellKnownObject.ObjectPrototype);
    }

    protected override EcmaValue ToPrimitive(object target, EcmaPreferredPrimitiveType kind) {
      return new EcmaValue(ToRuntimeObject(target)).ToPrimitive(kind);
    }

    protected override IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target) {
      return visibleMembers.Keys;
    }

    protected override bool HasProperty(object target, EcmaPropertyKey name) {
      return HasOwnProperty(target, name);
    }

    protected override bool HasOwnProperty(object target, EcmaPropertyKey name) {
      return visibleMembers.ContainsKey(name.Name);
    }

    protected override bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value) {
      if (HasOwnProperty(target, name)) {
        PropertyDescriptor property = visibleMembers[name.Name];
        value = new EcmaValue(property.GetValue(target));
        return true;
      }
      value = default(EcmaValue);
      return false;
    }

    protected override bool TrySet(object target, EcmaPropertyKey name, EcmaValue value) {
      if (HasOwnProperty(target, name)) {
        PropertyDescriptor property = visibleMembers[name.Name];
        if (!property.IsReadOnly) {
          property.SetValue(target, EcmaValue.ChangeType(value, property.PropertyType));
          return true;
        }
      }
      return false;
    }
  }
}
