using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  public abstract class ArrayLikeObjectBinder : ReflectedObjectBinder {
    public ArrayLikeObjectBinder(Type type)
      : base(type, true) { }

    protected override IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target) {
      for (int i = 0, length = GetLength(target); i < length; i++) {
        yield return i;
      }
    }

    protected override bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value) {
      if (name.IsArrayIndex) {
        if (!TryGetIndex(target, name.ArrayIndex, out value)) {
          value = EcmaValue.Undefined;
        }
        return true;
      }
      if (name.Name == "length") {
        value = GetLength(target);
        return true;
      }
      return base.TryGet(target, name, out value);
    }

    protected override bool TrySet(object target, EcmaPropertyKey name, EcmaValue value) {
      if (name.IsArrayIndex) {
        TrySetIndex(target, name.ArrayIndex, value);
        return true;
      }
      return base.TrySet(target, name, value);
    }

    protected override bool HasOwnProperty(object target, EcmaPropertyKey name) {
      if (name.IsArrayIndex) {
        return name.ArrayIndex < GetLength(target);
      }
      return name.Name == "length";
    }

    protected abstract int GetLength(object target);

    protected abstract bool TryGetIndex(object target, long index, out EcmaValue value);

    protected abstract bool TrySetIndex(object target, long index, EcmaValue value);
  }
}
