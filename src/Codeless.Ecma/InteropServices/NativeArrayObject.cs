using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.InteropServices {
  internal class NativeArrayObject : NativeObject<IList> {
    private static readonly Hashtable ht = new Hashtable();
    private readonly bool isReadOnly;
    private readonly bool isFixedSize;

    public NativeArrayObject(IList target)
      : base(target, WellKnownObject.ArrayPrototype) {
      Type type = target.GetType();
      if (!ht.ContainsKey(type)) {
        Type interfaceType = type.GetInterface(typeof(IList<>).FullName);
        if (interfaceType == null) {
          ht[type] = typeof(object);
        } else {
          Type itemType = interfaceType.GetGenericArguments()[0];
          ht[type] = itemType.IsAssignableFrom(typeof(EcmaValue)) ? itemType : null;
        }
      }
      isReadOnly = ht[type] == null || target.IsReadOnly;
      isFixedSize = isReadOnly || target.IsFixedSize;
    }

    public override bool IsExtensible {
      get { return !isFixedSize; }
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      return Enumerable.Range(0, Target.Count).Select(v => new EcmaPropertyKey(v)).Concat(new[] { WellKnownProperty.Length }).Concat(base.GetOwnPropertyKeys());
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (EcmaValueUtility.TryIndexByPropertyKey(this.Target, propertyKey, out EcmaValue value)) {
        return value;
      }
      if (propertyKey == WellKnownProperty.Length) {
        return Target.Count;
      }
      return base.Get(propertyKey, receiver);
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      if (propertyKey.IsArrayIndex) {
        if (isReadOnly || (isFixedSize && propertyKey.ToArrayIndex() > Target.Count)) {
          return false;
        }
        Target[(int)propertyKey.ToArrayIndex()] = value;
        return true;
      }
      if (propertyKey == WellKnownProperty.Length) {
        if (isFixedSize) {
          return false;
        }
        int len = (int)value;
        while (Target.Count < len) {
          Target.Add(EcmaValue.Undefined);
        }
        while (Target.Count > len) {
          Target.RemoveAt(Target.Count - 1);
        }
        return true;
      }
      return base.Set(propertyKey, value, receiver);
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return false;
      }
      if (propertyKey == WellKnownProperty.Length) {
        return false;
      }
      return base.Delete(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return propertyKey.ToArrayIndex() < Target.Count;
      }
      if (propertyKey == WellKnownProperty.Length) {
        return true;
      }
      return base.HasProperty(propertyKey);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return new EcmaPropertyDescriptor(Get(propertyKey, null), EcmaPropertyAttributes.DefaultDataProperty);
      }
      if (propertyKey == WellKnownProperty.Length) {
        return new EcmaPropertyDescriptor(Get(propertyKey, null), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyKey.IsArrayIndex || propertyKey == WellKnownProperty.Length) {
        return Set(propertyKey, descriptor.Value, this);
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }
  }
}
