using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public class EcmaArray : RuntimeObject, IList<EcmaValue> {
    private static EcmaPropertyKey propertyLength = new EcmaPropertyKey(WellKnownPropertyName.Length);
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
      }
    }

    public EcmaArray(IEnumerable<EcmaValue> values, RuntimeObject constructor)
      : base(WellKnownObject.ArrayPrototype, constructor) {
      if (values.Any()) {
        list = new List<EcmaValue>(values);
      }
    }

    public EcmaValue this[int index] {
      get {
        if (list == null || index < 0 || index >= list.Count) {
          return EcmaValue.Undefined;
        }
        return list[index];
      }
      set {
        // TODO: sparse
        if (index >= 0) {
          if (list == null) {
            list = new List<EcmaValue>();
          }
          while (index >= list.Count) {
            list.Add(EcmaValue.Undefined);
          }
          list[index] = value;
        }
        throw new NotImplementedException();
      }
    }

    public int Count {
      get { return (int)length; }
    }

    [EcmaSpecification("IsArray", EcmaSpecificationKind.AbstractOperations)]
    public static bool IsArray(EcmaValue value) {
      if (value.Type != EcmaValueType.Object) {
        return false;
      }
      RuntimeObject obj = value.ToRuntimeObject();
      if (obj is EcmaArray) {
        return true;
      }
      RuntimeObjectProxy proxy = obj as RuntimeObjectProxy;
      if (proxy != null) {
        return proxy.ProxyTarget is EcmaArray;
      }
      return false;
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
        // TODO: sparse array
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

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        // TODO
      }
      EcmaPropertyDescriptor descriptor = base.GetOwnProperty(propertyKey);
      if (descriptor == null) {
        return new EcmaPropertyDescriptor(length);
      }
      return descriptor;
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyLength.Equals(propertyKey)) {
        return SetLength(descriptor);
      }
      if (propertyKey.IsArrayIndex) {
        long index = propertyKey.ArrayIndex;
        if (index >= length && lengthReadOnly) {
          return false;
        }
        if (!SetItem(index)) {
          return false;
        }
        if (index >= length) {
          length = (uint)index + 1;
        }
        return true;
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return DeleteItem(propertyKey.ArrayIndex);
      }
      return base.Delete(propertyKey);
    }

    [EcmaSpecification("ArraySetLength", EcmaSpecificationKind.AbstractOperations)]
    protected bool SetLength(EcmaPropertyDescriptor descriptor) {
      if (descriptor.IsAccessorDescriptor) {
        return base.DefineOwnProperty(propertyLength, descriptor);
      }
      uint newLength = descriptor.Value.ToUInt32();
      if (newLength != descriptor.Value.ToNumber()) {
        throw new EcmaRangeErrorException("");
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

    protected bool SetItem(long v) {
      throw new NotImplementedException();
    }

    protected bool DeleteItem(long v) {
      throw new NotImplementedException();
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
