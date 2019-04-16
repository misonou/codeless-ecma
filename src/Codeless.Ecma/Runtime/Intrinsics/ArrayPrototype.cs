using System;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayPrototype)]
  internal static class ArrayPrototype {
    [IntrinsicMember(EcmaPropertyAttributes.Writable)]
    public const int Length = 0;

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      return Join(thisValue, EcmaValue.Undefined);
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisValue) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      if (length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(",");
        }
        EcmaValue item = obj.Get(i);
        if (!item.IsNullOrUndefined) {
          sb.Append(item.Invoke("toLocaleString"));
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Join([This] EcmaValue thisValue, EcmaValue separater) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      if (length == 0) {
        return String.Empty;
      }
      string sep = separater.Type == EcmaValueType.Undefined ? "," : separater.ToString();
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(sep);
        }
        EcmaValue item = obj.Get(i);
        if (!item.IsNullOrUndefined) {
          sb.Append(item.ToString());
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Pop([This] EcmaValue thisValue) {
      // TODO: Array.prototype.pop
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Push([This] EcmaValue thisValue) {
      // TODO: Array.prototype.push
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Reverse([This] EcmaValue thisValue) {
      // TODO: Array.prototype.reverse
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Shift([This] EcmaValue thisValue) {
      // TODO: Array.prototype.shift
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Unshift([This] EcmaValue thisValue) {
      // TODO: Array.prototype.unshift
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisValue) {
      // TODO: Array.prototype.slice
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Splice([This] EcmaValue thisValue) {
      // TODO: Array.prototype.splice
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Sort([This] EcmaValue thisValue) {
      // TODO: Array.prototype.sort
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Filter([This] EcmaValue thisValue) {
      // TODO: Array.prototype.filter
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Some([This] EcmaValue thisValue) {
      // TODO: Array.prototype.some
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Every([This] EcmaValue thisValue) {
      // TODO: Array.prototype.every
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Map([This] EcmaValue thisValue) {
      // TODO: Array.prototype.map
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue IndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex = default) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      if (length == 0) {
        return -1;
      }
      int from = fromIndex.HasValue ? (int)fromIndex : 0;
      if (from >= length) {
        return -1;
      }
      if (from < 0) {
        from = Math.Max(0, from + length);
      }
      for (int i = from; i < length; i++) {
        if (obj.HasProperty(i) && obj.Get(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue LastIndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex = default) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      if (length == 0) {
        return -1;
      }
      int from = fromIndex.HasValue ? Math.Min(length - 1, (int)fromIndex.Value) : length - 1;
      if (from < 0) {
        from += length;
      }
      for (int i = from; i >= 0; i--) {
        if (obj.HasProperty(i) && obj.Get(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue Reduce([This] EcmaValue thisValue) {
      // TODO: Array.prototype.reduce
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue ReduceRight([This] EcmaValue thisValue) {
      // TODO: Array.prototype.reduceRight
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue CopyWithin([This] EcmaValue thisValue) {
      // TODO: Array.prototype.copyWithin
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Find([This] EcmaValue thisValue, EcmaValue predicate, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      Guard.ArgumentIsCallable(predicate);
      for (int i = 0; i < length; i++) {
        EcmaValue value = obj.Get(i);
        if (predicate.Call(thisArg, value, i, thisValue)) {
          return value;
        }
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue FindIndex([This] EcmaValue thisValue, EcmaValue predicate, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      Guard.ArgumentIsCallable(predicate);
      for (int i = 0; i < length; i++) {
        if (predicate.Call(thisArg, obj.Get(i), i, thisValue)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue Fill([This] EcmaValue thisValue) {
      // TODO: Array.prototype.fill
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Includes([This] EcmaValue thisValue) {
      // TODO: Array.prototype.includes
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Entries([This] EcmaValue thisValue) {
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Entry, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    public static EcmaValue ForEach([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      int length = obj.Get(WellKnownPropertyName.Length).ToInt32();
      Guard.ArgumentIsCallable(callback);
      for (int i = 0; i < length; i++) {
        if (obj.HasProperty(i)) {
          callback.Call(thisArg, obj.Get(i), i, thisValue);
        }
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisValue) {
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Key, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    public static EcmaValue Concat([This] EcmaValue thisValue) {
      // TODO: Array.prototype.concat
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Values([This] EcmaValue thisValue) {
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Value, WellKnownObject.ArrayIteratorPrototype);
    }

    [EcmaSpecification("ArraySpeciesCreate", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue SpeciesCreate(EcmaValue originalArray, uint length) {
      if (!EcmaArray.IsArray(originalArray)) {
        return new EcmaArray(length);
      }
      EcmaValue constructor = originalArray[WellKnownPropertyName.Constructor];
      RuntimeObject obj = constructor.ToObject(true);
      if (obj != null && obj.IsWellknownObject(WellKnownObject.ArrayConstructor) && RuntimeRealm.Current != obj.Realm) {
        constructor = default;
      }
      if (constructor.Type == EcmaValueType.Object) {
        constructor = constructor[WellKnownSymbol.Species];
        if (constructor.IsNullOrUndefined) {
          constructor = default;
        }
      }
      if (constructor.Type == EcmaValueType.Undefined) {
        return new EcmaArray(length);
      }
      RuntimeObject obj2 = constructor.ToObject(true);
      if (obj2 != null && obj2.IsConstructor) {
        return obj2.Construct(length);
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
    }
  }
}
