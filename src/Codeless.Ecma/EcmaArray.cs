using Codeless.Ecma.InteropServices;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [Cloneable(true)]
  public class EcmaArray : RuntimeObject, IList<EcmaValue> {
    private List<EcmaValue> list;
    private LinkedList<ArrayChunk> chunks;

    public EcmaArray()
      : base(WellKnownObject.ArrayPrototype) {
      SetLength(0);
    }

    public EcmaArray(long length)
      : base(WellKnownObject.ArrayPrototype) {
      SetLength(length);
    }

    public EcmaArray(List<EcmaValue> list)
      : base(WellKnownObject.ArrayPrototype) {
      Guard.ArgumentNotNull(list, "list");
      this.list = list;
      if (!SetLengthDirect(list.Count)) {
        throw new EcmaTypeErrorException(InternalString.Error.SetPropertyFailed);
      }
    }

    public EcmaArray(params EcmaValue[] values)
      : base(WellKnownObject.ArrayPrototype) {
      Guard.ArgumentNotNull(values, "values");
      Init(values);
    }

    public long Length {
      get { return Get(WellKnownProperty.Length).ToLength(); }
      set { SetLength(value); }
    }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.Array; }
    }

    internal bool FallbackMode { get; private set; }

    internal void Init(IEnumerable<EcmaValue> values) {
      list = new List<EcmaValue>(values);
      if (!SetLengthDirect(list.Count)) {
        throw new EcmaTypeErrorException(InternalString.Error.SetPropertyFailed);
      }
    }

    [EcmaSpecification("IsArray", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsArray(EcmaValue value) {
      if (value.Type != EcmaValueType.Object) {
        return false;
      }
      RuntimeObject obj = value.ToObject();
      while (obj is RuntimeObjectProxy proxy) {
        obj = proxy.ProxyTarget;
      }
      return obj is EcmaArray || obj is NativeArrayObject;
    }

    public static EcmaArray Of(params EcmaValue[] elements) {
      return new EcmaArray(elements);
    }

    public long IndexOf(EcmaValue searchElement) {
      return IndexOf(searchElement, 0);
    }

    public long IndexOf(EcmaValue searchElement, long fromIndex) {
      if (this.FallbackMode) {
        return ArrayPrototype.IndexOf(this, searchElement, fromIndex).ToInt64();
      }
      if (list != null) {
        int realIndex = chunks == null || chunks.Count == 0 ? (int)fromIndex : GetRealIndex(fromIndex, out _, out _);
        return (int)GetVirtIndex(list.FindIndex(realIndex, v => v.Equals(searchElement, EcmaValueComparison.Strict)));
      }
      return -1;
    }

    public long LastIndexOf(EcmaValue searchElement) {
      if (this.FallbackMode) {
        return ArrayPrototype.IndexOf(this, searchElement, null).ToInt64();
      }
      if (list != null) {
        return (int)GetVirtIndex(list.FindLastIndex(v => v.Equals(searchElement, EcmaValueComparison.Strict)));
      }
      return -1;
    }

    public long LastIndexOf(EcmaValue searchElement, long fromIndex) {
      if (this.FallbackMode) {
        return ArrayPrototype.LastIndexOf(this, searchElement, fromIndex).ToInt64();
      }
      if (list != null) {
        int realIndex = chunks == null || chunks.Count == 0 ? (int)fromIndex : GetRealIndex(fromIndex, out _, out _);
        return (int)GetVirtIndex(list.FindLastIndex(realIndex, v => v.Equals(searchElement, EcmaValueComparison.Strict)));
      }
      return -1;
    }

    public long Includes(EcmaValue searchElement) {
      return Includes(searchElement, 0);
    }

    public long Includes(EcmaValue searchElement, long fromIndex) {
      if (this.FallbackMode) {
        return ArrayPrototype.Includes(this, searchElement, fromIndex).ToInt64();
      }
      if (list == null) {
        return -1;
      }
      long offset = 0;
      LinkedListNode<ArrayChunk> node = null;
      int realIndex = chunks == null || chunks.Count == 0 ? (int)fromIndex : GetRealIndex(fromIndex, out node, out offset);
      if (searchElement == default && offset < 0) {
        return fromIndex;
      }
      int result = list.FindIndex(realIndex, v => v.Equals(searchElement, EcmaValueComparison.SameValueZero));
      if (result != -1) {
        return GetVirtIndex(result);
      }
      if (searchElement == default && node != null && node.Next != null) {
        return fromIndex + node.Value.Count - offset;
      }
      return -1;
    }

    public long Unshift(params EcmaValue[] elements) {
      if (this.FallbackMode) {
        return (long)ArrayPrototype.Unshift(this, elements);
      }
      InsertRange(0, elements);
      return this.Length;
    }

    public long Push(params EcmaValue[] elements) {
      if (this.FallbackMode) {
        return (long)ArrayPrototype.Push(this, elements);
      }
      long cur = this.Length;
      InsertRange(cur, elements);
      return cur + elements.Length;
    }

    public EcmaValue Shift() {
      if (this.FallbackMode) {
        return ArrayPrototype.Shift(this);
      }
      EcmaValue item = Get(0);
      RemoveRange(0, 1);
      return item;
    }

    public EcmaValue Pop() {
      if (this.FallbackMode) {
        return ArrayPrototype.Pop(this);
      }
      long cur = this.Length;
      if (cur == 0) {
        return default;
      }
      EcmaValue item = Get(cur - 1);
      RemoveRange(cur - 1, 1);
      return item;
    }

    public EcmaArray Slice(long start) {
      return Slice(start, this.Length);
    }

    public EcmaArray Slice(long start, long end) {
      if (this.FallbackMode) {
        return ArrayPrototype.Slice(this, start, end).GetUnderlyingObject<EcmaArray>();
      }
      long len = this.Length;
      start = start < 0 ? Math.Max(0, start + len) : Math.Min(start, len);
      end = end < 0 ? Math.Max(0, end + len) : Math.Min(end, len);

      EcmaArray target = (EcmaArray)ArrayPrototype.SpeciesCreate(this, Math.Max(end - start, 0));
      if (start < len && start < end) {
        RemoveRange(start, end - start, true, target);
      }
      return target;
    }

    public EcmaArray Splice(long start) {
      return Splice(start, this.Length);
    }

    public EcmaArray Splice(long start, long deleteCount, params EcmaValue[] elements) {
      if (this.FallbackMode) {
        EcmaValue[] args = ArrayHelper.Combine(new EcmaValue[] { start, deleteCount }, elements);
        return ArrayPrototype.Splice(this, args).GetUnderlyingObject<EcmaArray>();
      }
      long len = this.Length;
      start = start < 0 ? Math.Max(0, start + len) : Math.Min(start, len);
      deleteCount = Math.Max(0, Math.Min(deleteCount, len - start));
      ArrayPrototype.ThrowIfLengthExceeded(len - deleteCount + elements.Length);

      EcmaArray target = (EcmaArray)ArrayPrototype.SpeciesCreate(this, deleteCount);
      SpliceInternal(start, deleteCount, elements, target);
      return target;
    }

    public EcmaArray Sort() {
      if (this.FallbackMode) {
        ArrayPrototype.Sort(this, EcmaValue.Undefined);
        return this;
      }
      long len = this.Length;
      int itemCount = list != null ? list.Count : 0;
      if (itemCount > 0) {
        list.Sort((x, y) => ArrayPrototype.SortCompare(x, y, EcmaValue.Undefined));
      }
      if (chunks != null) {
        if (len > itemCount) {
          chunks.Clear();
          chunks.AddFirst(new ArrayChunk(itemCount, (int)(len - itemCount)));
        }
      }
      return this;
    }

    public EcmaArray Reverse() {
      if (this.FallbackMode) {
        ArrayPrototype.Reverse(this);
        return this;
      }
      if (list != null) {
        list.Reverse();
        if (chunks != null && chunks.Count > 0) {
          ArrayChunk final = chunks.First.Value;
          long initialOffset = final.Offset;
          if (chunks.Count > 1) {
            chunks.Reverse();
            for (LinkedListNode<ArrayChunk> cur = chunks.First; cur.Next != null; cur = cur.Next) {
              cur.Value.Count = cur.Next.Value.Count;
            }
          }
          if (initialOffset == 0) {
            final.Offset = this.Length - list.Count;
            final.Count = 0;
          } else {
            final.Offset = 0;
            final.Count = list.Count;
          }
        }
      }
      return this;
    }

    public void ForEach(Action<EcmaValue> callback) {
      ForEach(callback, EcmaValue.Undefined);
    }

    public void ForEach(Action<EcmaValue, long> callback) {
      ForEach(callback, EcmaValue.Undefined);
    }

    public void ForEach(Action<EcmaValue, long, EcmaArray> callback) {
      ForEach(callback, EcmaValue.Undefined);
    }

    public void ForEach(EcmaValue callback, EcmaValue thisArg) {
      if (this.FallbackMode) {
        ArrayPrototype.ForEach(this, callback, thisArg);
        return;
      }
      Guard.ArgumentIsCallable(callback);
      foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
        callback.Call(thisArg, entry.Value, entry.Key, this);
      }
    }

    public bool Some(Func<EcmaValue, bool> callback) {
      return Some(callback, EcmaValue.Undefined);
    }

    public bool Some(Func<EcmaValue, long, bool> callback) {
      return Some(callback, EcmaValue.Undefined);
    }

    public bool Some(Func<EcmaValue, long, EcmaArray, bool> callback) {
      return Some(callback, EcmaValue.Undefined);
    }

    public bool Some(EcmaValue callback, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return (bool)ArrayPrototype.Some(this, callback, thisArg);
      }
      Guard.ArgumentIsCallable(callback);
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
          if (callback.Call(thisArg, entry.Value, entry.Key, this)) {
            return true;
          }
        }
      }
      return false;
    }

    public bool Every(Func<EcmaValue, bool> callback) {
      return Every(callback, EcmaValue.Undefined);
    }

    public bool Every(Func<EcmaValue, long, bool> callback) {
      return Every(callback, EcmaValue.Undefined);
    }

    public bool Every(Func<EcmaValue, long, EcmaArray, bool> callback) {
      return Every(callback, EcmaValue.Undefined);
    }

    public bool Every(EcmaValue callback, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return (bool)ArrayPrototype.Every(this, callback, thisArg);
      }
      Guard.ArgumentIsCallable(callback);
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true, true)) {
          if (!(bool)callback.Call(thisArg, entry.Value, entry.Key, this)) {
            return false;
          }
        }
      }
      return true;
    }

    public EcmaArray Filter(Func<EcmaValue, bool> callback) {
      return Filter(callback, EcmaValue.Undefined);
    }

    public EcmaArray Filter(Func<EcmaValue, long, bool> callback) {
      return Filter(callback, EcmaValue.Undefined);
    }

    public EcmaArray Filter(Func<EcmaValue, long, EcmaArray, bool> callback) {
      return Filter(callback, EcmaValue.Undefined);
    }

    public EcmaArray Filter(EcmaValue callback, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return ArrayPrototype.Filter(this, callback, thisArg).GetUnderlyingObject<EcmaArray>();
      }
      Guard.ArgumentIsCallable(callback);
      List<EcmaValue> arr = new List<EcmaValue>();
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
          if (callback.Call(thisArg, entry.Value, entry.Key, this)) {
            arr.Add(entry.Value);
          }
        }
      }
      return new EcmaArray(arr);
    }

    public EcmaArray Map(Func<EcmaValue, EcmaValue> callback) {
      return Map(callback, EcmaValue.Undefined);
    }

    public EcmaArray Map(Func<EcmaValue, long, EcmaValue> callback) {
      return Map(callback, EcmaValue.Undefined);
    }

    public EcmaArray Map(Func<EcmaValue, long, EcmaArray, EcmaValue> callback) {
      return Map(callback, EcmaValue.Undefined);
    }

    public EcmaArray Map(EcmaValue callback, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return ArrayPrototype.Map(this, callback, thisArg).GetUnderlyingObject<EcmaArray>();
      }
      Guard.ArgumentIsCallable(callback);
      List<EcmaValue> arr = new List<EcmaValue>();
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
          arr.Add(callback.Call(thisArg, entry.Value, entry.Key, this));
        }
      }
      return new EcmaArray(arr);
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, T> callback) {
      return Reduce(new DelegateRuntimeFunction(callback));
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, T> callback, T initialValue) {
      return Reduce(new DelegateRuntimeFunction(callback), new EcmaValue(initialValue));
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, long, T> callback) {
      return Reduce(new DelegateRuntimeFunction(callback));
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, long, T> callback, T initialValue) {
      return Reduce(new DelegateRuntimeFunction(callback), new EcmaValue(initialValue));
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, long, EcmaArray, T> callback) {
      return Reduce(new DelegateRuntimeFunction(callback));
    }

    public EcmaValue Reduce<T>(Func<T, EcmaValue, long, EcmaArray, T> callback, T initialValue) {
      return Reduce(new DelegateRuntimeFunction(callback), new EcmaValue(initialValue));
    }

    public EcmaValue Reduce(EcmaValue callback) {
      if (this.FallbackMode) {
        return ArrayPrototype.Reduce(this, callback, null);
      }
      if (list == null || list.Count == 0) {
        throw new EcmaTypeErrorException(InternalString.Error.ReduceEmptyArray);
      }
      Guard.ArgumentIsCallable(callback);
      bool reduce = false;
      EcmaValue initialValue = default;
      foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
        if (reduce) {
          initialValue = callback.Call(EcmaValue.Undefined, initialValue, entry.Value, entry.Key, this);
        } else {
          initialValue = entry.Value;
        }
      }
      return initialValue;
    }

    public EcmaValue Reduce(EcmaValue callback, EcmaValue initialValue) {
      if (this.FallbackMode) {
        return ArrayPrototype.Reduce(this, callback, initialValue);
      }
      Guard.ArgumentIsCallable(callback);
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
          initialValue = callback.Call(EcmaValue.Undefined, initialValue, entry.Value, entry.Key, this);
        }
      }
      return initialValue;
    }

    public EcmaValue Find(Func<EcmaValue, bool> predicate) {
      return Find(predicate, EcmaValue.Undefined);
    }

    public EcmaValue Find(Func<EcmaValue, long, bool> predicate) {
      return Find(predicate, EcmaValue.Undefined);
    }

    public EcmaValue Find(Func<EcmaValue, long, EcmaArray, bool> predicate) {
      return Find(predicate, EcmaValue.Undefined);
    }

    public EcmaValue Find(EcmaValue predicate, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return ArrayPrototype.Find(this, predicate, thisArg);
      }
      Guard.ArgumentIsCallable(predicate);
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(false, true)) {
          if (predicate.Call(thisArg, entry.Value, entry.Key, this)) {
            return entry.Value;
          }
        }
      }
      return default;
    }

    public long FindIndex(Func<EcmaValue, bool> predicate) {
      return FindIndex(predicate, EcmaValue.Undefined);
    }

    public long FindIndex(Func<EcmaValue, long, bool> predicate) {
      return FindIndex(predicate, EcmaValue.Undefined);
    }

    public long FindIndex(Func<EcmaValue, long, EcmaArray, bool> predicate) {
      return FindIndex(predicate, EcmaValue.Undefined);
    }

    public long FindIndex(EcmaValue predicate, EcmaValue thisArg) {
      if (this.FallbackMode) {
        return (long)ArrayPrototype.FindIndex(this, predicate, thisArg);
      }
      Guard.ArgumentIsCallable(predicate);
      if (list != null) {
        foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(false, true)) {
          if (predicate.Call(thisArg, entry.Value, entry.Key, this)) {
            return entry.Key.ToInt64();
          }
        }
      }
      return -1;
    }

    public string Join() {
      return Join(",");
    }

    public string Join(string separater) {
      return ArrayPrototype.Join(this, separater).ToString();
    }

    public EcmaArray Concat(params EcmaValue[] elements) {
      return ArrayPrototype.Concat(this, elements).GetUnderlyingObject<EcmaArray>();
    }

    public EcmaArray Flat() {
      return Flat(1);
    }

    public EcmaArray Flat(int depth) {
      return ArrayPrototype.Flat(this, depth).GetUnderlyingObject<EcmaArray>();
    }

    public EcmaArray FlatMap(Func<EcmaValue, EcmaValue> callback) {
      return FlatMap(callback, EcmaValue.Undefined);
    }

    public EcmaArray FlatMap(Func<EcmaValue, long, EcmaValue> callback) {
      return FlatMap(callback, EcmaValue.Undefined);
    }

    public EcmaArray FlatMap(Func<EcmaValue, long, EcmaArray, EcmaValue> callback) {
      return FlatMap(callback, EcmaValue.Undefined);
    }

    public EcmaArray FlatMap(EcmaValue callback, EcmaValue thisArg) {
      return ArrayPrototype.FlatMap(this, callback, thisArg).GetUnderlyingObject<EcmaArray>();
    }

    public EcmaIterator Keys() {
      if (!this.FallbackMode) {
        return new EcmaArrayIterator(EnumerateEntries(false).GetEnumerator(), EcmaIteratorResultKind.Key);
      }
      return new EcmaArrayIterator(this, EcmaIteratorResultKind.Key);
    }

    public EcmaIterator Values() {
      if (!this.FallbackMode) {
        return new EcmaArrayIterator(EnumerateEntries(false).GetEnumerator(), EcmaIteratorResultKind.Value);
      }
      return new EcmaArrayIterator(this, EcmaIteratorResultKind.Value);
    }

    public EcmaIterator Entries() {
      if (!this.FallbackMode) {
        return new EcmaArrayIterator(EnumerateEntries(false).GetEnumerator(), EcmaIteratorResultKind.Entry);
      }
      return new EcmaArrayIterator(this, EcmaIteratorResultKind.Entry);
    }

    internal void SliceInternal(long start, long count, EcmaArray target) {
      RemoveRange(start, count, true, target);
    }

    internal void SpliceInternal(long start, long deleteCount, EcmaValue[] elements, EcmaArray target) {
      RemoveRange(start, deleteCount, false, target);
      InsertRange(start, elements);
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      if (this.FallbackMode) {
        return base.GetOwnPropertyKeys();
      }
      return EnumerateOwnPropertyIndex().Concat(base.GetOwnPropertyKeys());
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (ShouldLookupFromList(propertyKey, out long index)) {
        int i = GetRealIndex(index);
        if (i >= 0) {
          return list[i];
        }
      }
      return base.Get(propertyKey, receiver);
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      if (ShouldLookupFromList(propertyKey, out long index)) {
        return SetItem(index, value);
      }
      return base.Set(propertyKey, value, receiver);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (ShouldLookupFromList(propertyKey, out long index)) {
        return DeleteItem(index);
      }
      return base.Delete(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (ShouldLookupFromList(propertyKey, out long index) && GetRealIndex(index) >= 0) {
        return true;
      }
      return base.HasProperty(propertyKey);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (ShouldLookupFromList(propertyKey, out long index)) {
        int i = GetRealIndex(index);
        return i >= 0 ? new EcmaPropertyDescriptor(list[i], EcmaPropertyAttributes.DefaultDataProperty) : null;
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyKey == WellKnownProperty.Length) {
        return SetLength(descriptor);
      }
      if (propertyKey.IsArrayIndex) {
        long index = propertyKey.ToArrayIndex();
        if (index >= 0) {
          if (!this.FallbackMode) {
            if (descriptor.IsDataDescriptor && descriptor.Writable) {
              return SetItem(index, descriptor.Value);
            }
            SetFallbackMode();
          }
          if (index >= this.Length && !base.DefineOwnProperty(WellKnownProperty.Length, new EcmaPropertyDescriptor(index + 1))) {
            return false;
          }
        }
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    public override bool SetIntegrityLevel(RuntimeObjectIntegrityLevel level) {
      if (!this.FallbackMode) {
        try {
          this.FallbackMode = true;
          return base.SetIntegrityLevel(level);
        } finally {
          this.FallbackMode = false;
        }
      }
      return base.SetIntegrityLevel(level);
    }

    public override bool SetPrototypeOf(RuntimeObject proto) {
      bool result = base.SetPrototypeOf(proto);
      if (result && proto != null && !proto.IsWellknownObject(WellKnownObject.ArrayPrototype)) {
        SetFallbackMode();
      }
      return result;
    }

    public IEnumerator<EcmaValue> GetEnumerator() {
      if (this.FallbackMode) {
        EcmaArrayEnumerator iterator = new EcmaArrayEnumerator(this);
        while (iterator.MoveNext()) {
          yield return iterator.Current.Value;
        }
      } else if (list == null) {
        yield break;
      } else if (chunks == null || chunks.Count == 0) {
        for (int i = 0, len = list.Count; i < len; i++) {
          yield return list[i];
        }
      } else {
        int i = 0;
        foreach (ArrayChunk s in chunks) {
          for (long j = 0, len = s.Offset; j < len; j++) {
            yield return default;
          }
          for (long j = 0, len = s.Count; j < len; j++) {
            yield return list[i++];
          }
        }
      }
    }

    protected override RuntimeObjectIntegrityLevel TestIntegrityLevel() {
      return !this.FallbackMode ? RuntimeObjectIntegrityLevel.Default : base.TestIntegrityLevel();
    }

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      if (!this.FallbackMode) {
        if (chunks != null) {
          chunks = new LinkedList<ArrayChunk>(chunks);
        }
        if (list != null) {
          list = new List<EcmaValue>(list.Select(context.Clone));
        }
      }
    }

    private bool SetLength(long length) {
      return DefineOwnProperty(WellKnownProperty.Length, new EcmaPropertyDescriptor(length, EcmaPropertyAttributes.Writable));
    }

    [EcmaSpecification("ArraySetLength", EcmaSpecificationKind.AbstractOperations)]
    private bool SetLength(EcmaPropertyDescriptor descriptor) {
      if (descriptor.HasValue) {
        uint newLen = descriptor.Value.ToUInt32();
        if (newLen != descriptor.Value.ToNumber()) {
          throw new EcmaRangeErrorException(InternalString.Error.InvalidArrayLength);
        }
        long curLen = this.Length;
        if (!this.FallbackMode) {
          if (newLen < curLen) {
            RemoveRange(newLen, curLen - newLen);
          } else if (newLen > curLen) {
            EnsureChunkList();
            chunks.AddLast(new ArrayChunk(newLen - curLen, 0));
          }
        } else {
          if (newLen < curLen) {
            for (long i = newLen; i < curLen; i++) {
              Delete(i);
            }
          }
        }
      }
      return base.DefineOwnProperty(WellKnownProperty.Length, descriptor);
    }

    private bool SetLengthDirect(long newLen) {
      return base.DefineOwnProperty(WellKnownProperty.Length, new EcmaPropertyDescriptor(newLen, EcmaPropertyAttributes.Writable));
    }

    private bool ShouldLookupFromList(EcmaPropertyKey propertyKey, out long index) {
      if (propertyKey.IsArrayIndex && !this.FallbackMode) {
        index = propertyKey.ToArrayIndex();
        return index >= 0;
      }
      index = default;
      return false;
    }

    private void SetFallbackMode() {
      if (this.FallbackMode) {
        return;
      }
      this.FallbackMode = true;
      if (list == null) {
        return;
      }
      foreach (KeyValuePair<EcmaValue, EcmaValue> entry in EnumerateEntries(true)) {
        DefineOwnPropertyNoChecked(EcmaPropertyKey.FromValue(entry.Key), new EcmaPropertyDescriptor(entry.Value, EcmaPropertyAttributes.DefaultDataProperty));
      }
      list = null;
      chunks = null;
    }

    private int GetRealIndex(long index, out LinkedListNode<ArrayChunk> node, out long offset) {
      long curLen = this.Length;
      long cur = 0;
      int realIndex = 0;
      if (index >= curLen) {
        node = chunks.Last;
        offset = index - curLen + node.Value.Count;
        return list.Count;
      }
      for (node = chunks.First; node != null; node = node.Next) {
        ArrayChunk chunk = node.Value;
        cur += chunk.Offset;
        if (index < cur) {
          break;
        }
        if (index <= cur + chunk.Count) {
          offset = index - cur;
          return realIndex + (int)offset;
        }
        cur += chunk.Count;
        realIndex += checked((int)chunk.Count);
      }
      offset = index - cur;
      return realIndex;
    }

    private int GetRealIndex(long index) {
      if (list == null || index >= this.Length) {
        return -1;
      }
      if (chunks == null || chunks.Count == 0) {
        return index < list.Count ? (int)index : -1;
      }
      int realIndex = GetRealIndex(index, out LinkedListNode<ArrayChunk> node, out long offset);
      return offset >= 0 && offset < node.Value.Count ? realIndex : -1;
    }

    private long GetVirtIndex(int index) {
      if (list == null || index < 0 || index >= list.Count) {
        return -1;
      }
      if (chunks == null || chunks.Count == 0) {
        return index;
      }
      long cur = 0;
      long count = 0;
      foreach (ArrayChunk chunk in chunks) {
        cur += chunk.Offset;
        if (index < count + chunk.Count) {
          break;
        }
        cur += chunk.Count;
        count += chunk.Count;
      }
      return cur + index - count;
    }

    private bool SetItem(long index, EcmaValue value) {
      if (this.FallbackMode) {
        return base.DefineOwnProperty(index, new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.DefaultDataProperty));
      }
      if (this.IntegrityLevel >= RuntimeObjectIntegrityLevel.Frozen) {
        return false;
      }
      long curLen = this.Length;
      long newLen = Math.Max(curLen, index + 1);
      if (list == null) {
        list = new List<EcmaValue>();
      }
      if (chunks == null || chunks.Count == 0) {
        if (index < curLen) {
          list[(int)index] = value;
          return true;
        }
        if (!this.IsExtensible || !SetLengthDirect(newLen)) {
          return false;
        }
        if (index > curLen) {
          EnsureChunkList();
          chunks.AddLast(new ArrayChunk(index - curLen, 1));
        }
        list.Add(value);
        return true;
      }

      int realIndex = GetRealIndex(index, out LinkedListNode<ArrayChunk> node, out long offset);
      ArrayChunk chunk = node.Value;
      if (offset >= 0 && offset < chunk.Count) {
        list[realIndex] = value;
        return true;
      }
      if (!this.IsExtensible || !SetLengthDirect(newLen)) {
        return false;
      }
      if (offset > chunk.Count) {
        chunks.AddAfter(node, new ArrayChunk(offset - chunk.Count, 1));
      } else if (offset == chunk.Count) {
        chunk.Count++;
        LinkedListNode<ArrayChunk> next = node.Next;
        if (next != null && --next.Value.Offset == 0) {
          chunk.Count += next.Value.Count;
          chunks.Remove(next);
        }
      } else if (offset == -1) {
        chunk.Count++;
        chunk.Offset--;
      } else {
        chunks.AddBefore(node, new ArrayChunk(chunk.Offset + offset, 1));
        chunk.Offset = -offset - 1;
      }
      if (chunks.Count == 1 && chunks.First.Value.Offset == 0) {
        chunks.Clear();
      }
      list.Insert(realIndex, value);
      return true;
    }

    private bool DeleteItem(long index) {
      if (this.FallbackMode) {
        throw new InvalidOperationException();
      }
      if (list == null || index >= this.Length) {
        return true;
      }
      if (chunks == null || chunks.Count == 0) {
        if (this.IntegrityLevel >= RuntimeObjectIntegrityLevel.Sealed) {
          return false;
        }
        if (index < list.Count - 1) {
          EnsureChunkList();
          chunks.First.Value.Count = (int)index;
          chunks.AddLast(new ArrayChunk(1, list.Count - (int)index - 1));
        }
        list.RemoveAt((int)index);
        return true;
      }

      int realIndex = GetRealIndex(index, out LinkedListNode<ArrayChunk> node, out long offset);
      ArrayChunk chunk = node.Value;
      if (offset < 0 || offset >= chunk.Count) {
        return true;
      }
      if (this.IntegrityLevel >= RuntimeObjectIntegrityLevel.Sealed) {
        return false;
      }
      if (offset == chunk.Count - 1) {
        LinkedListNode<ArrayChunk> next = node.Next ?? chunks.AddAfter(node, new ArrayChunk(0, 0));
        next.Value.Offset++;
        if (--chunk.Count == 0) {
          next.Value.Offset += chunk.Offset;
          chunks.Remove(node);
        }
      } else if (offset == 0) {
        chunk.Offset++;
        chunk.Count--;
      } else {
        chunks.AddAfter(node, new ArrayChunk(offset - 1, chunk.Count - offset - 1));
        chunk.Count = offset;
      }
      list.RemoveAt(realIndex);
      return true;
    }

    private bool InsertRange(long index, ICollection<EcmaValue> value) {
      long curLen = this.Length;
      long addLen = value.Count;
      ArrayPrototype.ThrowIfLengthExceeded(curLen + addLen);
      if (!SetLengthDirect(curLen + addLen)) {
        throw new EcmaTypeErrorException(InternalString.Error.SetPropertyFailed);
      }
      if (addLen == 0) {
        return true;
      }
      if (this.FallbackMode) {
        ArrayPrototype.CopyWithin(this, index + addLen, index, curLen);
        foreach (EcmaValue item in value) {
          this.CreateDataPropertyOrThrow(index++, item);
        }
        return true;
      }
      if (!this.IsExtensible) {
        throw new EcmaTypeErrorException(InternalString.Error.CreatePropertyThrow);
      }
      if (list == null) {
        list = new List<EcmaValue>();
      }
      if (chunks == null || chunks.Count == 0) {
        if (index <= list.Count) {
          list.InsertRange((int)index, value);
        } else {
          EnsureChunkList();
          chunks.AddLast(new ArrayChunk(index, value.Count));
          list.AddRange(value);
        }
        return true;
      }

      int realIndex = GetRealIndex(index, out LinkedListNode<ArrayChunk> node, out long offset);
      ArrayChunk chunk = node.Value;
      if (offset >= 0) {
        chunk.Count += value.Count;
      } else {
        chunks.AddBefore(node, new ArrayChunk(chunk.Offset + offset, value.Count));
        chunk.Offset = -offset;
      }
      list.InsertRange(realIndex, value);
      return true;
    }

    private bool RemoveRange(long index, long count, bool silent = false, EcmaArray clone = null) {
      if (this.FallbackMode) {
        throw new InvalidOperationException();
      }
      long curLen = this.Length;
      if (list == null) {
        return true;
      }
      if (!silent) {
        if (!SetLengthDirect(curLen - count)) {
          throw new EcmaTypeErrorException(InternalString.Error.SetPropertyFailed);
        }
        if (this.IntegrityLevel >= RuntimeObjectIntegrityLevel.Sealed) {
          throw new EcmaTypeErrorException(InternalString.Error.DeletePropertyThrow);
        }
      }
      if (chunks == null || chunks.Count == 0) {
        if (index < list.Count) {
          int removeCount = (int)Math.Min(count, list.Count - index);
          if (clone != null) {
            CopyTo(clone, 0, (int)index, removeCount);
          }
          if (!silent) {
            list.RemoveRange((int)index, removeCount);
          }
        }
        return true;
      }

      long until = Math.Min(index + count, curLen);
      long cur = 0;
      int realIndex = 0;
      int startIndex = -1;
      int startOffset = -1;
      int endOffset = -1;
      long targetIndex = 0;
      LinkedListNode<ArrayChunk> startNode = null;
      LinkedListNode<ArrayChunk> node = chunks.First;
      for (; node != null; node = node.Next) {
        ArrayChunk chunk = node.Value;
        cur += chunk.Offset;
        if (startNode == null && index < cur + chunk.Count) {
          startNode = node;
          startOffset = (int)(index - cur);
          startIndex = realIndex + Math.Max(0, startOffset);
        }
        endOffset = (int)(until - cur);
        if (clone != null) {
          int cloneCount = checked((int)(chunk.Count - Math.Max(0, endOffset)));
          if (node == startNode) {
            targetIndex = CopyTo(clone, startOffset < 0 ? Math.Min(0, endOffset) - startOffset : 0, startIndex, cloneCount - (startIndex - realIndex));
          } else {
            targetIndex = CopyTo(clone, targetIndex + chunk.Offset + Math.Min(0, endOffset), realIndex, cloneCount);
          }
        }
        if (until < cur + chunk.Count) {
          break;
        }
        cur += chunk.Count;
        realIndex += checked((int)chunk.Count);
      }
      if (!silent) {
        if (startOffset > 0) {
          startNode.Value.Count = startOffset + (endOffset >= 0 ? node.Value.Count - endOffset : 0);
          startNode = startNode.Next;
        } else if (endOffset > 0) {
          node.Value.Count = node.Value.Count - endOffset;
          node.Value.Offset = startNode.Value.Offset + startOffset;
          node = node.Previous;
        } else {
          node.Value.Offset = startNode.Value.Offset + startOffset - endOffset;
          node = node.Previous;
        }
        if (startNode != null) {
          while (startNode != node) {
            chunks.Remove(startNode.Next);
          }
          chunks.Remove(startNode);
        }
        list.RemoveRange(startIndex, realIndex - startIndex + Math.Max(0, endOffset));
      }
      return true;
    }

    private void EnsureChunkList() {
      if (chunks == null) {
        chunks = new LinkedList<ArrayChunk>();
      }
      if (chunks.Count == 0 && list != null && list.Count > 0) {
        chunks.AddFirst(new ArrayChunk(0, list.Count));
      }
    }

    private IEnumerable<EcmaPropertyKey> EnumerateOwnPropertyIndex() {
      if (list != null) {
        if (chunks == null || chunks.Count == 0) {
          for (long i = 0, len = list.Count; i < len; i++) {
            yield return i;
          }
        } else {
          long i = 0;
          foreach (ArrayChunk s in chunks) {
            i += s.Offset;
            for (long j = 0, len = s.Count; j < len; i++, j++) {
              yield return i;
            }
          }
        }
      }
    }

    private IEnumerable<KeyValuePair<EcmaValue, EcmaValue>> EnumerateEntries(bool skipSparse, bool keepLength = false) {
      long i = 0;
      long len = 0;
      if (keepLength) {
        len = this.Length;
      }
      start:
      if (!keepLength) {
        len = this.Length;
      }
      if (list == null) {
        yield break;
      }
      List<EcmaValue>.Enumerator t = list.GetEnumerator();
      if (chunks == null || chunks.Count == 0) {
        int curLen = list.Count;
        for (int j = (int)i; i < len; i++, j++) {
          if (IsCollectionModified(t)) {
            goto start;
          }
          yield return new KeyValuePair<EcmaValue, EcmaValue>(i, j < curLen ? list[j] : EcmaValue.Undefined);
        }
      } else {
        int j = GetRealIndex(i, out LinkedListNode<ArrayChunk> node, out long offset);
        while (i < len) {
          if (offset < 0) {
            if (skipSparse) {
              i -= offset;
              offset = 0;
            } else {
              for (; offset < 0; i++, offset++) {
                if (IsCollectionModified(t)) {
                  goto start;
                }
                yield return new KeyValuePair<EcmaValue, EcmaValue>(i, EcmaValue.Undefined);
              }
            }
          }
          for (long until = Math.Min(len, i + node.Value.Count - offset); i < until; i++, j++) {
            if (IsCollectionModified(t)) {
              goto start;
            }
            yield return new KeyValuePair<EcmaValue, EcmaValue>(i, list[j]);
          }
          node = node.Next;
          if (node == null) {
            break;
          }
          offset = -node.Value.Offset;
        }
      }
    }

    private bool IsCollectionModified(List<EcmaValue>.Enumerator iterator) {
      try {
        iterator.MoveNext();
        return false;
      } catch (InvalidOperationException) {
        return true;
      }
    }

    private long CopyTo(EcmaArray target, long index, int sourceIndex, int count) {
      for (int until = sourceIndex + count; sourceIndex < until; sourceIndex++, index++) {
        target.SetItem(index, list[sourceIndex]);
      }
      return index;
    }

    #region Interface
    int IList<EcmaValue>.IndexOf(EcmaValue item) {
      return (int)IndexOf(item);
    }

    void IList<EcmaValue>.Insert(int index, EcmaValue item) {
      if (!InsertRange(index, new[] { item })) {
        throw new InvalidOperationException();
      }
    }

    void IList<EcmaValue>.RemoveAt(int index) {
      if (!RemoveRange(index, 1)) {
        throw new InvalidOperationException();
      }
    }

    int ICollection<EcmaValue>.Count {
      get { return (int)this.Length; }
    }

    void ICollection<EcmaValue>.Add(EcmaValue item) {
      if (!SetItem(this.Length, item)) {
        throw new InvalidOperationException();
      }
    }

    void ICollection<EcmaValue>.Clear() {
      if (!SetLength(0)) {
        throw new InvalidOperationException();
      }
    }

    void ICollection<EcmaValue>.CopyTo(EcmaValue[] array, int arrayIndex) {
      throw new NotImplementedException();
    }

    bool ICollection<EcmaValue>.Contains(EcmaValue item) {
      return list != null && Includes(item) >= 0;
    }

    bool ICollection<EcmaValue>.Remove(EcmaValue item) {
      if (list != null) {
        long index = Includes(item);
        return index >= 0 && RemoveRange(index, 1);
      }
      return false;
    }

    bool ICollection<EcmaValue>.IsReadOnly {
      get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
    #endregion

    #region Helper class
    [DebuggerDisplay("({Offset}, {Count})")]
    private class ArrayChunk {
      public ArrayChunk(long offset, long count) {
        this.Offset = offset;
        this.Count = count;
      }

      public long Offset { get; set; }
      public long Count { get; set; }
    }
    #endregion
  }
}
