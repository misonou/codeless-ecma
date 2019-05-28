using NUnit.Framework.Constraints;

namespace Codeless.Ecma.UnitTest.Constraints {
  internal class TypeOfConstraint : IConstraint {
    private readonly string expected;

    public TypeOfConstraint(string typeOfStr) {
      this.expected = typeOfStr;
    }

    public string DisplayName => "TypeOfConstraint";

    public string Description => $"Expect value is of type '{expected}'";

    public object[] Arguments => new[] { expected };

    public ConstraintBuilder Builder {
      get => throw new System.NotImplementedException();
      set => throw new System.NotImplementedException();
    }

    public ConstraintResult ApplyTo<TActual>(TActual actual) {
      string typeOf = Global.TypeOf(new EcmaValue(actual));
      return new ConstraintResult(this, typeOf, typeOf == expected);
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
