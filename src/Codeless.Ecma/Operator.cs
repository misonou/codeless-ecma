using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public static class Operator {
    public static EcmaValue BitwiseAnd(this EcmaValue x, EcmaValue y) {
      return (+x).ToInt32() & (+y).ToInt32();
    }

    public static EcmaValue BitwiseOr(this EcmaValue x, EcmaValue y) {
      return (+x).ToInt32() | (+y).ToInt32();
    }

    public static EcmaValue LeftShift(this EcmaValue x, EcmaValue y) {
      return (+x).ToInt32() << (+y).ToInt32();
    }

    public static EcmaValue RightShift(this EcmaValue x, EcmaValue y) {
      return (+x).ToInt32() >> (+y).ToInt32();
    }

    public static EcmaValue LogicalRightShift(this EcmaValue x, EcmaValue y) {
      return (+x).ToUInt32() >> (+y).ToInt32();
    }

    public static EcmaValue Pow(this EcmaValue x, EcmaValue y) {
      return EcmaMath.Pow(x, y);
    }
  }
}
