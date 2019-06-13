using System;
using Codeless.Ecma.Runtime;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest.Constraints {
  internal class DataPropertyConstraint : IConstraint {
    private readonly EcmaPropertyKey name;
    private readonly EcmaPropertyAttributes attributes;
    private readonly EcmaValue? expected;

    static DataPropertyConstraint() {
      TestContext.AddFormatter<DataPropertyConstraint>(val => Format((DataPropertyConstraint)val));
    }

    public DataPropertyConstraint(EcmaPropertyKey name, EcmaValue? value, EcmaPropertyAttributes attributes) {
      this.name = name;
      this.attributes = attributes;
      this.expected = value;
    }

    public string DisplayName => "DataPropertyConstraint";

    public string Description => Format(this);

    public object[] Arguments => new object[] { name, expected, attributes };

    public ConstraintBuilder Builder {
      get => throw new System.NotImplementedException();
      set => throw new System.NotImplementedException();
    }

    public ConstraintResult ApplyTo<TActual>(TActual actual) {
      RuntimeObject obj = RuntimeRealm.Current.GetRuntimeObject(actual);
      EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(name);
      if (descriptor == null) {
        return new ConstraintResult(this, null, false);
      }
      if (descriptor.IsAccessorDescriptor && expected.HasValue) {
        return new ConstraintResult(this, descriptor, false);
      }
      DataPropertyConstraint comparand = new DataPropertyConstraint(name, descriptor.Value,
        (descriptor.Configurable ? EcmaPropertyAttributes.Configurable : 0) |
        (descriptor.Enumerable ? EcmaPropertyAttributes.Enumerable : 0) |
        (descriptor.Writable ? EcmaPropertyAttributes.Writable : 0));
      return new ConstraintResult(this, comparand, comparand.attributes == attributes && (!expected.HasValue || expected.Value.Equals(descriptor.Value, EcmaValueComparison.SameValue)));
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

    private static string Format(DataPropertyConstraint val) {
      return val.expected.HasValue ?
        $"Property {val.name} => {val.expected.Value} ({val.attributes})" :
        $"Property {val.name} => <any> ({val.attributes})";
    }
  }
}
