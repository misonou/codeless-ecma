using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public class EcmaArray : RuntimeObject, IList<EcmaValue> {
    private static readonly EcmaPropertyKey propertyLength = new EcmaPropertyKey(WellKnownPropertyName.Length);
    private List<EcmaValue> list;
    private uint length;
    private bool lengthReadOnly;

    public EcmaArray()
      : base(WellKnownObject.ArrayPrototype) { }

    public EcmaArray(uint length)
      : base(WellKnownObject.ArrayPrototype) {
      this.length = length;
    }

    public EcmaArray(uint length, RuntimeObject constructor)
      : base(WellKnownObject.ArrayPrototype, constructor) {
      this.length = length;
    }

    public EcmaArray(IEnumerable<EcmaValue> values)
      : base(WellKnownObject.ArrayPrototype) {
      if (values.Any()) {
        list = new List<EcmaValue>(values);
        length = (uint)list.Count;
      }
    }

    public EcmaArray(IEnumerable<EcmaValue> values, RuntimeObject constructor)
      : base(WellKnownObject.ArrayPrototype, constructor) {
      if (values.Any()) {
        list = new List<EcmaValue>(values);
        length = (uint)list.Count;
      }
    }

    public EcmaValue this[int index] {
      get {
        if (list == null || index < 0 || index >= list.Count) {
          return default;
        }
        return list[index];
      }
      set {
        if (index >= 0) {
          if (list == null) {
            list = new List<EcmaValue>();
          }
          while (index >= list.Count) {
            list.Add(EcmaValue.Undefined);
          }
          list[index] = value;
        }
      }
    }

    public int Count {
      get { return (int)length; }
    }

    protected override string ToStringTag {
      get { return InternalString.ObjectTag.Array; }
    }

    [EcmaSpecification("IsArray", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsArray(EcmaValue value) {
      if (value.Type != EcmaValueType.Object) {
        return false;
      }
      RuntimeObject obj = value.ToObject();
      if (obj is RuntimeObjectProxy proxy) {
        obj = proxy.ProxyTarget;
      }
      return obj is EcmaArray || obj is NativeArrayObject;
    }

    public static EcmaArray Of(params EcmaValue[] elements) {
      return new EcmaArray(elements);
    }

    public void Add(EcmaValue item) {
      if (list == null) {
        list = new List<EcmaValue>();
      }
      list.Add(item);
    }

    public void Clear() {
      if (list != null) {
        list.Clear();
      }
    }

    public bool Contains(EcmaValue item) {
      if (list == null) {
        return false;
      }
      return list.Contains(item);
    }

    public void CopyTo(EcmaValue[] array, int arrayIndex) {
      if (list != null) {
        list.CopyTo(array, arrayIndex);
      }
    }

    public IEnumerator<EcmaValue> GetEnumerator() {
      if (list != null) {
        return list.GetEnumerator();
      }
      return Enumerable.Empty<EcmaValue>().GetEnumerator();
    }

    public int IndexOf(EcmaValue item) {
      if (list != null) {
        return list.IndexOf(item);
      }
      return -1;
    }

    public void Insert(int index, EcmaValue item) {
      if (index >= 0) {
        if (list == null) {
          list = new List<EcmaValue>();
        }
        // TODO: EcmaArray.Insert
        while (index > list.Count) {
          list.Add(EcmaValue.Undefined);
        }
        list.Add(item);
      }
      throw new NotImplementedException();
    }

    public bool Remove(EcmaValue item) {
      if (list != null) {
        return list.Remove(item);
      }
      return false;
    }

    public void RemoveAt(int index) {
      if (list != null && index >= 0 && index < list.Count) {
        list.RemoveAt(index);
      }
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      IEnumerable<EcmaPropertyKey> indexes = list != null ? Enumerable.Range(0, list.Count).Select(v => new EcmaPropertyKey(v)) : Enumerable.Empty<EcmaPropertyKey>();
      return indexes.Concat(new[] { propertyLength }).Concat(base.GetOwnPropertyKeys());
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (EcmaValueUtility.TryIndexByPropertyKey(list, propertyKey, out EcmaValue value)) {
        return value;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return length;
      }
      return base.Get(propertyKey, receiver);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return false;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return false;
      }
      return base.Delete(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return list != null && propertyKey.ToArrayIndex() < list.Count;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return true;
      }
      return base.HasProperty(propertyKey);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (EcmaValueUtility.TryIndexByPropertyKey(list, propertyKey, out EcmaValue value)) {
        return new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.DefaultDataProperty);
      }
      if (propertyKey == propertyLength) {
        return new EcmaPropertyDescriptor(length, EcmaPropertyAttributes.Configurable & (lengthReadOnly ? 0 : EcmaPropertyAttributes.Configurable));
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyKey == WellKnownPropertyName.Length) {
        return SetLength(descriptor);
      }
      if (propertyKey.IsArrayIndex) {
        long index = propertyKey.ToArrayIndex();
        if (index >= length && lengthReadOnly) {
          return false;
        }
        if (!SetItem(index, descriptor.Value)) {
          return false;
        }
        if (index >= length) {
          length = (uint)index + 1;
        }
        return true;
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    [EcmaSpecification("ArraySetLength", EcmaSpecificationKind.AbstractOperations)]
    protected bool SetLength(EcmaPropertyDescriptor descriptor) {
      if (descriptor.IsAccessorDescriptor) {
        return base.DefineOwnProperty(propertyLength, descriptor);
      }
      uint newLength = descriptor.Value.ToUInt32();
      if (newLength != descriptor.Value.ToNumber()) {
        throw new EcmaRangeErrorException(InternalString.Error.InvalidArrayLength);
      }
      if (newLength >= length) {
        length = newLength;
        return true;
      }
      if (lengthReadOnly) {
        return false;
      }
      while (newLength < length) {
        if (!DeleteItem((long)length - 1)) {
          return false;
        }
        length--;
      }
      if (descriptor.Writable == false) {
        lengthReadOnly = true;
      }
      return true;
    }

    protected bool SetItem(long index, EcmaValue value) {
      if (index >= 0) {
        this[(int)index] = value;
        return true;
      }
      return base.DefineOwnProperty(index, new EcmaPropertyDescriptor(value));
    }

    protected bool DeleteItem(long index) {
      if (index >= 0) {
        RemoveAt((int)index);
        return true;
      }
      return base.Delete(index);
    }

    #region Interface
    bool ICollection<EcmaValue>.IsReadOnly {
      get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
    #endregion
  }
}
