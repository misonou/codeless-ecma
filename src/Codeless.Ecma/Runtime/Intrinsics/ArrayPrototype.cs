using System;
using System.Collections.Generic;
using System.Linq;
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
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      for (long i = 0; i < length; i++) {
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
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return String.Empty;
      }
      string sep = separater.Type == EcmaValueType.Undefined ? "," : separater.ToString();
      StringBuilder sb = new StringBuilder();
      for (long i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(sep);
        }
        EcmaValue item = obj.Get(i);
        if (!item.IsNullOrUndefined) {
          sb.Append(item.ToString(true));
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue Pop([This] EcmaValue thisValue) {
      RuntimeObject obj = thisValue.ToObject();
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Pop();
      }
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      if (len == 0) {
        obj.SetOrThrow(WellKnownProperty.Length, 0);
        return default;
      }
      long newLen = len - 1;
      EcmaValue value = obj.Get(newLen);
      obj.DeletePropertyOrThrow(newLen);
      obj.SetOrThrow(WellKnownProperty.Length, newLen);
      return value;
    }

    [IntrinsicMember]
    public static EcmaValue Push([This] EcmaValue thisValue, params EcmaValue[] elements) {
      RuntimeObject obj = thisValue.ToObject();
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Push(elements);
      }
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      ThrowIfLengthExceeded(len + elements.LongLength);
      foreach (EcmaValue element in elements) {
        obj.SetOrThrow(len++, element);
      }
      obj.SetOrThrow(WellKnownProperty.Length, len);
      return len;
    }

    [IntrinsicMember]
    public static EcmaValue Reverse([This] EcmaValue thisValue) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long middle = len >> 1;
      long lower = 0;
      long upper = len - 1;
      while (lower != middle) {
        bool lowerExists = obj.HasProperty(lower);
        EcmaValue lowerValue = lowerExists ? obj.Get(lower) : default;
        bool upperExists = obj.HasProperty(upper);
        EcmaValue upperValue = upperExists ? obj.Get(upper) : default;
        if (lowerExists && upperExists) {
          obj.SetOrThrow(lower, upperValue);
          obj.SetOrThrow(upper, lowerValue);
        } else if (upperExists) {
          obj.SetOrThrow(lower, upperValue);
          obj.DeletePropertyOrThrow(upper);
        } else if (lowerExists) {
          obj.DeletePropertyOrThrow(lower);
          obj.SetOrThrow(upper, lowerValue);
        }
        lower++;
        upper--;
      }
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Shift([This] EcmaValue thisValue) {
      RuntimeObject obj = thisValue.ToObject();
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Shift();
      }
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      if (len == 0) {
        obj.SetOrThrow(WellKnownProperty.Length, 0);
        return default;
      }
      EcmaValue value = obj.Get(0);
      CopyWithinInternal(obj, 1, 0, len - 1);
      obj.DeletePropertyOrThrow(len - 1);
      obj.SetOrThrow(WellKnownProperty.Length, len - 1);
      return value;
    }

    [IntrinsicMember]
    public static EcmaValue Unshift([This] EcmaValue thisValue, params EcmaValue[] elements) {
      RuntimeObject obj = thisValue.ToObject();
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Unshift(elements);
      }
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long newLen = len + elements.LongLength;
      if (elements.Length > 0) {
        ThrowIfLengthExceeded(newLen);
        CopyWithinInternal(obj, len - 1, newLen - 1, -len);
        for (int i = 0, until = elements.Length; i < until; i++) {
          obj.SetOrThrow(i, elements[i]);
        }
      }
      obj.SetOrThrow(WellKnownProperty.Length, newLen);
      return newLen;
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisValue, EcmaValue start, EcmaValue end) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long from = GetBoundIndex(start, len, 0);
      long until = GetBoundIndex(end, len, len);
      if (obj is EcmaArray arr) {
        return arr.Slice(from, until);
      }
      RuntimeObject target = SpeciesCreate(thisValue, Math.Max(until - from, 0));
      int i = 0;
      while (from < until) {
        if (obj.HasProperty(from)) {
          target.CreateDataPropertyOrThrow(i++, obj.Get(from++));
        }
      }
      target.SetOrThrow(WellKnownProperty.Length, i);
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue Splice([This] EcmaValue thisValue, EcmaValue start, EcmaValue? count, params EcmaValue[] elements) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long from = GetBoundIndex(start, len, 0);
      long insertCount = elements.Length;
      long deleteCount = count.HasValue ? GetBoundIndex(count.Value, len - from, 0) : 0;
      long newLen = len + insertCount - deleteCount;
      ThrowIfLengthExceeded(newLen);
      if (obj is EcmaArray arr) {
        return arr.Splice(from, deleteCount, elements);
      }

      RuntimeObject target = SpeciesCreate(thisValue, deleteCount);
      for (long i = 0; i < deleteCount; i++) {
        if (obj.HasProperty(i)) {
          target.CreateDataPropertyOrThrow(i, obj.Get(from + i));
        }
      }
      target.SetOrThrow(WellKnownProperty.Length, deleteCount);

      long moveCount = len - from - deleteCount;
      if (insertCount < deleteCount) {
        CopyWithinInternal(obj, from + deleteCount, from + insertCount, moveCount);
        for (long i = len - 1, until = newLen; i >= until; i--) {
          obj.DeletePropertyOrThrow(i);
        }
      } else if (insertCount > deleteCount) {
        CopyWithinInternal(obj, len - 1, newLen - 1, -moveCount);
      }
      for (long i = 1; i < elements.Length; i++) {
        obj.SetOrThrow(from + i - 1, elements[i]);
      }
      obj.SetOrThrow(WellKnownProperty.Length, newLen);
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Sort([This] EcmaValue thisValue, EcmaValue callback) {
      if (callback != default) {
        Guard.ArgumentIsCallable(callback);
      }
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      List<EcmaValue> list = new List<EcmaValue>();
      for (long i = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          list.Add(obj.Get(i));
        }
      }
      list.Sort((x, y) => SortCompare(x, y, callback));
      for (int i = 0; i < list.Count; i++) {
        obj.SetOrThrow(i, list[i]);
      }
      for (long i = list.Count; i < len; i++) {
        obj.DeletePropertyOrThrow(i);
      }
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Filter([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Filter((RuntimeFunction)callback.GetUnderlyingObject(), thisArg);
      }
      RuntimeObject target = SpeciesCreate(thisArg, 0);
      for (long i = 0, j = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          EcmaValue value = obj.Get(i);
          if (callback.Call(thisArg, value, i, thisValue)) {
            target.CreateDataPropertyOrThrow(j++, value);
          }
        }
      }
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue Some([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Some((RuntimeFunction)callback.GetUnderlyingObject(), thisArg);
      }
      for (long i = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          EcmaValue value = obj.Get(i);
          if (callback.Call(thisArg, value, i, thisValue)) {
            return true;
          }
        }
      }
      return false;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Every([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Every((RuntimeFunction)callback.GetUnderlyingObject(), thisArg);
      }
      for (long i = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          EcmaValue value = obj.Get(i);
          if (!(bool)callback.Call(thisArg, value, i, thisValue)) {
            return false;
          }
        }
      }
      return true;
    }

    [IntrinsicMember]
    public static EcmaValue Map([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Map((RuntimeFunction)callback.GetUnderlyingObject(), thisArg);
      }
      RuntimeObject target = SpeciesCreate(thisArg, 0);
      for (long i = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          EcmaValue value = obj.Get(i);
          target.CreateDataPropertyOrThrow(i, callback.Call(thisArg, value, i, thisValue));
        }
      }
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue IndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return -1;
      }
      long from = fromIndex.HasValue ? (long)fromIndex : 0;
      if (from >= length) {
        return -1;
      }
      if (from < 0) {
        from = Math.Max(0, from + length);
      }
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.IndexOf(searchElement, from);
      }
      for (long i = from; i < length; i++) {
        if (obj.HasProperty(i) && obj.Get(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue LastIndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return -1;
      }
      long from = fromIndex.HasValue ? Math.Min(length - 1, (long)fromIndex.Value) : length - 1;
      if (from < 0) {
        from += length;
      }
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.LastIndexOf(searchElement, from);
      }
      for (long i = from; i >= 0; i--) {
        if (obj.HasProperty(i) && obj.Get(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue Reduce([This] EcmaValue thisValue, EcmaValue callback, EcmaValue? initialValue) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (length == 0 && !initialValue.HasValue) {
        throw new EcmaTypeErrorException("");
      }
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Reduce((RuntimeFunction)callback.GetUnderlyingObject(), initialValue.GetValueOrDefault());
      }
      long i = 0;
      EcmaValue value = default;
      if (initialValue.HasValue) {
        value = initialValue.Value;
      } else {
        for (; i < length; i++) {
          if (obj.HasProperty(i)) {
            value = obj.Get(i);
            break;
          }
        }
        if (i == length) {
          throw new EcmaTypeErrorException(InternalString.Error.ReduceEmptyArray);
        }
      }
      for (; i < length; i++) {
        if (obj.HasProperty(i)) {
          value = callback.Call(EcmaValue.Undefined, value, obj.Get(i), i, thisValue);
        }
      }
      return value;
    }

    [IntrinsicMember]
    public static EcmaValue ReduceRight([This] EcmaValue thisValue, EcmaValue callback, EcmaValue? initialValue) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (length == 0 && !initialValue.HasValue) {
        throw new EcmaTypeErrorException("");
      }
      long i = length - 1;
      EcmaValue value = default;
      if (initialValue.HasValue) {
        value = initialValue.Value;
      } else {
        for (; i >= 0; i--) {
          if (obj.HasProperty(i)) {
            value = obj.Get(i);
            break;
          }
        }
        if (i < 0) {
          throw new EcmaTypeErrorException(InternalString.Error.ReduceEmptyArray);
        }
      }
      for (; i >= 0; i--) {
        if (obj.HasProperty(i)) {
          value = callback.Call(EcmaValue.Undefined, value, obj.Get(i), i, thisValue);
        }
      }
      return value;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue CopyWithin([This] EcmaValue thisValue, EcmaValue target, EcmaValue start, EcmaValue end) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long to = GetBoundIndex(target, len, 0);
      long from = GetBoundIndex(start, len, 0);
      long until = GetBoundIndex(end, len, len);
      long count = Math.Min(until - from, len - to);
      if (count > 0) {
        if (from < to && to < from + count) {
          CopyWithinInternal(obj, from + count - 1, to + count - 1, -count);
        } else {
          CopyWithinInternal(obj, from, to, count);
        }
      }
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Find([This] EcmaValue thisValue, EcmaValue predicate, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(predicate);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.Find((RuntimeFunction)predicate.GetUnderlyingObject(), thisArg);
      }
      for (long i = 0; i < length; i++) {
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
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(predicate);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        return arr.FindIndex((RuntimeFunction)predicate.GetUnderlyingObject(), thisArg);
      }
      for (int i = 0; i < length; i++) {
        if (predicate.Call(thisArg, obj.Get(i), i, thisValue)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Fill([This] EcmaValue thisValue, EcmaValue value, EcmaValue start, EcmaValue end) {
      RuntimeObject obj = thisValue.ToObject();
      long len = obj.Get(WellKnownProperty.Length).ToLength();
      long from = GetBoundIndex(start, len, 0);
      long until = GetBoundIndex(end, len, len);
      for (long i = from; i < until; i++) {
        obj.SetOrThrow(i, value);
      }
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Includes([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return -1;
      }
      long from = fromIndex.HasValue ? (long)fromIndex : 0;
      if (from >= length) {
        return -1;
      }
      if (from < 0) {
        from = Math.Max(0, from + length);
      }
      for (long i = from; i < length; i++) {
        if (obj.Get(i).Equals(searchElement, EcmaValueComparison.SameValueZero)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue Entries([This] EcmaValue thisValue) {
      if (thisValue.GetUnderlyingObject() is EcmaArray arr && !arr.FallbackMode) {
        return arr.Entries();
      }
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Entry, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    public static EcmaValue ForEach([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      Guard.ArgumentIsCallable(callback);
      if (obj is EcmaArray arr && !arr.FallbackMode) {
        arr.ForEach((RuntimeFunction)callback.GetUnderlyingObject(), thisArg);
        return default;
      }
      for (long i = 0; i < length; i++) {
        if (obj.HasProperty(i)) {
          callback.Call(thisArg, obj.Get(i), i, thisValue);
        }
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisValue) {
      if (thisValue.GetUnderlyingObject() is EcmaArray arr && !arr.FallbackMode) {
        return arr.Keys();
      }
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Key, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    public static EcmaValue Concat([This] EcmaValue thisValue, params EcmaValue[] elements) {
      Guard.RequireObjectCoercible(thisValue);
      RuntimeObject target = SpeciesCreate(thisValue, 0);
      long i = 0;
      foreach (EcmaValue item in new[] { thisValue }.Concat(elements)) {
        if (item.Type == EcmaValueType.Object) {
          EcmaValue spreadable = item[WellKnownSymbol.IsConcatSpreadable];
          if (spreadable != default ? spreadable : EcmaArray.IsArray(item)) {
            long len = item[WellKnownProperty.Length].ToLength();
            ThrowIfLengthExceeded(i + len);
            for (long j = 0; j < len; i++, j++) {
              if (item.HasProperty(j)) {
                target.CreateDataPropertyOrThrow(i, item[j]);
              }
            }
            continue;
          }
        }
        ThrowIfLengthExceeded(i + 1);
        target.CreateDataPropertyOrThrow(i++, item);
      }
      target.SetOrThrow(WellKnownProperty.Length, i);
      return target;
    }

    [IntrinsicMember]
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Values([This] EcmaValue thisValue) {
      if (thisValue.GetUnderlyingObject() is EcmaArray arr && !arr.FallbackMode) {
        return arr.Values();
      }
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Value, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember(WellKnownSymbol.Unscopables, EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Unscopables() {
      RuntimeObject unscopableList = RuntimeRealm.Current.Properties["ArrayPrototypeUnscopables"] as RuntimeObject;
      if (unscopableList == null) {
        unscopableList = RuntimeObject.Create(null);
        unscopableList.CreateDataProperty("copyWithin", true);
        unscopableList.CreateDataProperty("entries", true);
        unscopableList.CreateDataProperty("fill", true);
        unscopableList.CreateDataProperty("find", true);
        unscopableList.CreateDataProperty("findIndex", true);
        unscopableList.CreateDataProperty("flat", true);
        unscopableList.CreateDataProperty("flatMap", true);
        unscopableList.CreateDataProperty("includes", true);
        unscopableList.CreateDataProperty("keys", true);
        unscopableList.CreateDataProperty("values", true);
        RuntimeRealm.Current.Properties["ArrayPrototypeUnscopables"] = unscopableList;
      }
      return unscopableList;
    }

    [EcmaSpecification("ArraySpeciesCreate", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject SpeciesCreate(EcmaValue originalArray, long length) {
      if (!EcmaArray.IsArray(originalArray)) {
        return new EcmaArray(length);
      }
      EcmaValue constructor = originalArray[WellKnownProperty.Constructor];
      RuntimeObject obj = constructor.Type == EcmaValueType.Object ? constructor.ToObject() : null;
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
      RuntimeObject obj2 = constructor.Type == EcmaValueType.Object ? constructor.ToObject() : null;
      if (obj2 != null && obj2.IsConstructor) {
        return obj2.Construct(length).ToObject();
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
    }

    [EcmaSpecification("SortCompare", EcmaSpecificationKind.RuntimeSemantics)]
    public static int SortCompare(EcmaValue x, EcmaValue y, EcmaValue compareFn) {
      bool xUndef = x == default;
      bool yUndef = y == default;
      if (xUndef && yUndef) {
        return 0;
      }
      if (xUndef) {
        return 1;
      }
      if (yUndef) {
        return -1;
      }
      if (compareFn != default) {
        EcmaValue v = compareFn.Call(EcmaValue.Undefined, x, y).ToNumber();
        return v.IsNaN || v == 0 ? 0 : v > 0 ? 1 : -1;
      }
      string xString = x.ToString();
      string yString = y.ToString();
      return xString.CompareTo(yString);
    }

    public static void ThrowIfLengthExceeded(long newLen) {
      if (newLen >= (1L << 53)) {
        throw new EcmaTypeErrorException(InternalString.Error.ExceedMaximumLength);
      }
    }

    private static long GetBoundIndex(EcmaValue index, long len, long defaultValue) {
      if (index == default) {
        return defaultValue;
      }
      index = index.ToInteger();
      return (index < 0 ? EcmaMath.Max(len + index, 0) : EcmaMath.Min(index, len)).ToInt64();
    }

    private static void CopyWithinInternal(RuntimeObject obj, long from, long to, long count) {
      long until = from + count;
      long step = count > 0 ? 1 : -1;
      while (from != until) {
        if (obj.HasProperty(from)) {
          obj.SetOrThrow(to, obj.Get(from));
        } else {
          obj.DeletePropertyOrThrow(to);
        }
        from += step;
        to += step;
      }
    }
  }
}
