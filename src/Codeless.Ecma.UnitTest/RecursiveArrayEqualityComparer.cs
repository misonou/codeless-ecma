using System;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.UnitTest {
  public class RecursiveArrayEqualityComparer : IEqualityComparer<object> {
    public bool Equals(object x1, object y1) {
      EcmaValue x = new EcmaValue(x1);
      EcmaValue y = new EcmaValue(y1);
      if (EcmaValue.Equals(x, y, EcmaValueComparison.SameValue)) {
        return true;
      }
      if (!x.IsArrayLike || !y.IsArrayLike) {
        return false;
      }
      if (x["length"] != y["length"]) {
        return false;
      }
      for (long i = 0, len = x["length"].ToLength(); i < len; i++) {
        if (x[i].IsArrayLike && y[i].IsArrayLike) {
          if (!Equals(x[i], y[i])) {
            return false;
          }
        } else if (!EcmaValue.Equals(x[i], y[i], EcmaValueComparison.SameValue)) {
          return false;
        }
      }
      return true;
    }

    public int GetHashCode(object obj) {
      throw new NotImplementedException();
    }
  }
}
