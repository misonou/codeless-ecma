using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest.Constraints {
  internal class InstanceOfConstraint : IConstraint {
    private readonly EcmaValue expected;

    public InstanceOfConstraint (EcmaValue expected) {
      this.expected = expected;
    }

    public string DisplayName => "InstanceOfConstraint";

    public string Description => $"Expect value is an instance of '{expected}'";

    public object[] Arguments => new[] { (object)expected };

    public ConstraintBuilder Builder {
      get => throw new System.NotImplementedException();
      set => throw new System.NotImplementedException();
    }

    public ConstraintResult ApplyTo<TActual>(TActual actual) {
      EcmaValue value = new EcmaValue(actual);
      return new ConstraintResult(this, value.IsNullOrUndefined ? value : value["constructor"], value.InstanceOf(expected));
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
  }
}
