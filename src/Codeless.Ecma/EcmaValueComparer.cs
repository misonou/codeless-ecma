using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaValueComparer : IComparer<EcmaValue> {
    public static EcmaValueComparer Default = new EcmaValueComparer();

    public int Compare(EcmaValue x, EcmaValue y) {
      return x.CompareTo(y);
    }
  }
}
