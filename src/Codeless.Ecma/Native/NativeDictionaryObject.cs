using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Native {
  internal class NativeDictionaryObject : NativeObject<IDictionary> {
    private static readonly Hashtable ht = new Hashtable();
    private readonly bool isReadOnly;

    public NativeDictionaryObject(IDictionary target)
      : base(target, WellKnownObject.ObjectPrototype) {
      Type type = target.GetType();
      if (!ht.ContainsKey(type)) {
        Type interfaceType = type.GetInterface(typeof(IDictionary<,>).FullName);
        if (interfaceType == null) {
          ht[type] = typeof(object);
        } else if (interfaceType.GetGenericArguments()[0].IsAssignableFrom(typeof(string))) {
          Type itemType = interfaceType.GetGenericArguments()[1];
          ht[type] = itemType.IsAssignableFrom(typeof(EcmaValue)) ? itemType : null;
        }
      }
      isReadOnly = ht[type] == null || target.IsReadOnly;
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      return Target.Keys.OfType<object>().Select(v => new EcmaValue(v)).Where(v => EcmaPropertyKey.IsPropertyKey(v)).Select(v => EcmaPropertyKey.FromValue(v)).Concat(base.GetOwnPropertyKeys());
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      if (propertyKey.IsSymbol) {
        return base.Get(propertyKey, receiver);
      }
      string key = propertyKey.ToString();
      return Target.Contains(key) ? new EcmaValue(Target[key]) : default;
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      if (propertyKey.IsSymbol) {
        return base.Set(propertyKey, value, receiver);
      }
      if (isReadOnly) {
        return false;
      }
      Target[propertyKey.ToString()] = value;
      return true;
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      if (propertyKey.IsSymbol) {
        return base.Delete(propertyKey);
      }
      if (isReadOnly) {
        return false;
      }
      Target.Remove(propertyKey.ToString());
      return true;
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      return Target.Contains(propertyKey.ToString()) || base.HasProperty(propertyKey);
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (Target.Contains(propertyKey.ToString())) {
        return new EcmaPropertyDescriptor(Get(propertyKey, null), EcmaPropertyAttributes.DefaultDataProperty);
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (!propertyKey.IsSymbol && (Target.Contains(propertyKey.ToString()) || !isReadOnly)) {
        return Set(propertyKey, descriptor.Value, null);
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }
  }
}
