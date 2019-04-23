using Codeless.Ecma.Runtime;
using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest {
  internal class DataPropertyConstraint : IConstraint {
    private readonly string name;
    private readonly EcmaPropertyAttributes attributes;

    public DataPropertyConstraint(string name, EcmaPropertyAttributes attributes) {
      this.name = name;
      this.attributes = attributes;
    }

    public string DisplayName => "DataPropertyConstraint";

    public string Description => $"{name} {{ writable: {HasAttribute(EcmaPropertyAttributes.Writable)}, enumerable: {HasAttribute(EcmaPropertyAttributes.Enumerable)}, configurable: {HasAttribute(EcmaPropertyAttributes.Configurable)} }}";

    public object[] Arguments => new object[] { name, attributes };

    public ConstraintBuilder Builder {
      get => throw new System.NotImplementedException();
      set => throw new System.NotImplementedException();
    }

    public ConstraintResult ApplyTo<TActual>(TActual actual) {
      RuntimeObject obj;
      if (actual is EcmaValue value) {
        if (value.IsNullOrUndefined) {
          return new ConstraintResult(this, null, ConstraintStatus.Failure);
        }
        obj = value.ToObject();
      } else if (actual is RuntimeObject o) {
        obj = o;
      } else {
        return new ConstraintResult(this, null, ConstraintStatus.Error);
      }
      EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(name);
      if (descriptor == null) {
        return new ConstraintResult(this, null, ConstraintStatus.Failure);
      }
      if (descriptor.IsAccessorDescriptor) {
        return new ConstraintResult(this, $"{name} <AccessorProperty>", ConstraintStatus.Failure);
      }
      if (((descriptor.Configurable.Value ? 1 : 0) ^ (HasAttribute(EcmaPropertyAttributes.Configurable) ? 1 : 0)) != 0 ||
          ((descriptor.Enumerable.Value ? 1 : 0) ^ (HasAttribute(EcmaPropertyAttributes.Enumerable) ? 1 : 0)) != 0 ||
          ((descriptor.Writable.Value ? 1 : 0) ^ (HasAttribute(EcmaPropertyAttributes.Writable) ? 1 : 0)) != 0) {
        return new ConstraintResult(this, $"{name} {{ writable: {descriptor.Writable.Value}, enumerable: {descriptor.Enumerable.Value}, configurable: {descriptor.Configurable.Value} }}", ConstraintStatus.Failure);
      }
      return new ConstraintResult(this, true, true);
    }

    public ConstraintResult ApplyTo<TActual>(ActualValueDelegate<TActual> del) {
      return ApplyTo(del());
    }

    public ConstraintResult ApplyTo<TActual>(ref TActual actual) {
      return ApplyTo(actual);
    }

    public IConstraint Resolve() {
      return this;
    }

    private bool HasAttribute(EcmaPropertyAttributes flag) {
      return (attributes & flag) != 0;
    }
  }
}
