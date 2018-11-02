using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeListBinder : ArrayLikeObjectBinder {
    public NativeListBinder(Type type)
      : base(type) { }

    protected override Type RestrictedType {
      get { return typeof(IList); }
    }

    protected override int GetLength(object target) {
      return ((IList)target).Count;
    }

    protected override bool TryGetIndex(object target, long index, out EcmaValue value) {
      value = new EcmaValue(((IList)target)[(int)index]);
      return true;
    }

    protected override bool TrySetIndex(object target, long index, EcmaValue value) {
      IList list = (IList)target;
      while (index >= list.Count) {
        list.Add(null);
      }
      list[(int)index] = value.GetUnderlyingObject();
      return true;
    }
  }
}
