using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Native {
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

    public override bool IsExtensible => !isFixedSize;

    public override IList<EcmaPropertyKey> OwnPropertyKeys {
      get { return Enumerable.Range(0, Target.Count).Select(v => new EcmaPropertyKey(v)).Concat(new[] { new EcmaPropertyKey(WellKnownPropertyName.Length) }).Concat(base.OwnPropertyKeys).ToList(); }
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (propertyKey.IsArrayIndex) {
        return new EcmaValue(Target[(int)propertyKey.ArrayIndex]);
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return Target.Count;
      }
      return base.Get(propertyKey, receiver);
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      if (propertyKey.IsArrayIndex) {
        if (isReadOnly || (isFixedSize && propertyKey.ArrayIndex > Target.Count)) {
          return false;
        }
        Target[(int)propertyKey.ArrayIndex] = value;
        return true;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
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
      if (propertyKey == WellKnownPropertyName.Length) {
        return false;
      }
      return base.Delete(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return propertyKey.ArrayIndex < Target.Count;
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return true;
      }
      return base.HasProperty(propertyKey);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsArrayIndex) {
        return new EcmaPropertyDescriptor(Get(propertyKey, null));
      }
      if (propertyKey == WellKnownPropertyName.Length) {
        return new EcmaPropertyDescriptor(Get(propertyKey, null), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable);
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (propertyKey.IsArrayIndex || propertyKey == WellKnownPropertyName.Length) {
        return Set(propertyKey, descriptor.Value, this);
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }
  }
}
