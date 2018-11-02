using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeCollectionBinder : ArrayLikeObjectBinder {
    public NativeCollectionBinder(Type type)
      : base(type) { }

    protected override Type RestrictedType {
      get { return typeof(ICollection); }
    }

    protected override int GetLength(object target) {
      return ((ICollection)target).Count;
    }

    protected override bool TryGetIndex(object target, long index, out EcmaValue value) {
      value = new EcmaValue(((ICollection)target).OfType<object>().ElementAt((int)index));
      return true;
    }

    protected override bool TrySetIndex(object target, long index, EcmaValue value) {
      return false;
    }
  }
}
