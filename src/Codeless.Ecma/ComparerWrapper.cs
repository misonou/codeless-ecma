using System;
using System.Collections.Generic;

namespace Codeless.Ecma {
  internal class ComparerWrapper<T> : IComparer<T> {
    private readonly Comparison<T> compare;

    public ComparerWrapper(Comparison<T> compare) {
      Guard.ArgumentNotNull(compare, "compare");
      this.compare = compare;
    }

    public int Compare(T x, T y) {
      return compare(x, y);
    }
  }
}
