using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeDictionaryBinder : ReflectedObjectBinder {
    public NativeDictionaryBinder(Type type)
      : base(type, true) { }

    protected override Type RestrictedType {
      get { return typeof(IDictionary); }
    }

    protected override IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target) {
      return ((IDictionary)target).Keys.OfType<object>().Select(v => new EcmaPropertyKey(v.ToString()));
    }

    protected override bool HasOwnProperty(object target, EcmaPropertyKey name) {
      if (name.IsSymbol) {
        return false;
      }
      return ((IDictionary)target).Contains(name.Name);
    }

    protected override bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value) {
      if (!name.IsSymbol) {
        IDictionary dictionary = (IDictionary)target;
        if (dictionary.Contains(name.Name)) {
          value = new EcmaValue(dictionary[name.Name]);
          return true;
        }
      }
      value = default(EcmaValue);
      return false;
    }

    protected override bool TrySet(object target, EcmaPropertyKey name, EcmaValue value) {
      if (name.IsSymbol) {
        return false;
      }
      IDictionary dictionary = (IDictionary)target;
      if (dictionary.IsReadOnly) {
        return false;
      }
      dictionary[name.Name] = value.GetUnderlyingObject();
      return true;
    }
  }

  internal class NativeDictionaryBinder<TKey, TItem> : ReflectedObjectBinder {
    public NativeDictionaryBinder(Type type)
      : base(type, true) { }

    protected override Type RestrictedType {
      get { return typeof(IDictionary<TKey, TItem>); }
    }

    protected override IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target) {
      return ((IDictionary<TKey, TItem>)target).Keys.Select(v => new EcmaPropertyKey(v.ToString()));
    }

    protected override bool HasOwnProperty(object target, EcmaPropertyKey name) {
      if (name.IsSymbol) {
        return false;
      }
      return ((IDictionary<TKey, TItem>)target).ContainsKey((TKey)Convert.ChangeType(name.Name, typeof(TKey)));
    }

    protected override bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value) {
      if (!name.IsSymbol) {
        IDictionary<TKey, TItem> dictionary = (IDictionary<TKey, TItem>)target;
        TKey typedKey = (TKey)Convert.ChangeType(name.Name, typeof(TKey));
        TItem nativeValue;
        if (dictionary.TryGetValue(typedKey, out nativeValue)) {
          value = new EcmaValue(nativeValue);
          return true;
        }
      }
      value = default(EcmaValue);
      return false;
    }

    protected override bool TrySet(object target, EcmaPropertyKey name, EcmaValue value) {
      if (name.IsSymbol) {
        return false;
      }
      IDictionary<TKey, TItem> dictionary = (IDictionary<TKey, TItem>)target;
      TKey typedKey = (TKey)EcmaValue.ChangeType(name.Name, typeof(TKey));
      if (dictionary.IsReadOnly) {
        return false;
      }
      dictionary[typedKey] = (TItem)EcmaValue.ChangeType(value, typeof(TItem));
      return true;
    }
  }
}
