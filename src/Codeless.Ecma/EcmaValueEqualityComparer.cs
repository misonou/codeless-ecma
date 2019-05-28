using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaValueEqualityComparer : IEqualityComparer<EcmaValue> {
    public static EcmaValueEqualityComparer SameValue = new EcmaValueEqualityComparer(EcmaValueComparison.SameValue);
    public static EcmaValueEqualityComparer SameValueZero = new EcmaValueEqualityComparer(EcmaValueComparison.SameValueZero);
    public static EcmaValueEqualityComparer Strict = new EcmaValueEqualityComparer(EcmaValueComparison.Strict);
    public static EcmaValueEqualityComparer Abstract = new EcmaValueEqualityComparer(EcmaValueComparison.Abstract);

    private readonly EcmaValueComparison comparison;

    public EcmaValueEqualityComparer(EcmaValueComparison comparison) {
      this.comparison = comparison;
    }

    public bool Equals(EcmaValue x, EcmaValue y) {
      return EcmaValue.Equals(x, y, comparison);
    }

    public int GetHashCode(EcmaValue obj) {
      return obj.GetHashCode(comparison);
    }
  }
}
