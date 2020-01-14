using Codeless.Ecma.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  [Cloneable(false)]
  internal class PrimitiveObject : RuntimeObject, IInspectorMetaProvider {
    private EcmaValue value;

    public PrimitiveObject()
      : base(WellKnownObject.ObjectPrototype) { }

    public PrimitiveObject(EcmaValue value, WellKnownObject defaultProto)
      : base(defaultProto) {
      this.value = value;
    }

    public PrimitiveObject(EcmaValue value, WellKnownObject defaultProto, RuntimeObject constructor)
      : base(defaultProto, constructor) {
      this.value = value;
    }

    public virtual EcmaValue PrimitiveValue {
      get { return value; }
      set { this.value = value; }
    }

    protected override string ToStringTag {
      get { return value.ToStringTag; }
    }

    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      IEnumerable<EcmaPropertyKey> ownKeys = base.GetOwnPropertyKeys();
      if (value.Type == EcmaValueType.String) {
        IEnumerable<EcmaPropertyKey> indexes = Enumerable.Range(0, value.ToString().Length).Select(v => new EcmaPropertyKey(v));
        if (ownKeys.Any()) {
          return indexes.Concat(new[] { new EcmaPropertyKey("length") });
        }
        return indexes.Concat(ownKeys);
      }
      return ownKeys;
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      if (value.Type == EcmaValueType.String) {
        string str = value.ToString();
        if (EcmaValueUtility.TryIndexByPropertyKey(str, propertyKey, out EcmaValue ch)) {
          return new EcmaPropertyDescriptor(ch, EcmaPropertyAttributes.Enumerable);
        }
        if (propertyKey == WellKnownProperty.Length) {
          return new EcmaPropertyDescriptor(str.Length, EcmaPropertyAttributes.None);
        }
      }
      return base.GetOwnProperty(propertyKey);
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      if (value.Type == EcmaValueType.String) {
        if (propertyKey.IsArrayIndex && propertyKey.ToArrayIndex() < value.ToString().Length) {
          return true;
        }
        if (propertyKey == WellKnownProperty.Length) {
          return true;
        }
      }
      return base.HasProperty(propertyKey);
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (value.Type == EcmaValueType.String && this.IsExtensible) {
        if (base.GetOwnPropertyKeys().Any()) {
          DefineOwnPropertyNoChecked(WellKnownProperty.Length, GetOwnProperty(WellKnownProperty.Length));
        }
      }
      return base.DefineOwnProperty(propertyKey, descriptor);
    }

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      switch (value.Type) {
        case EcmaValueType.Boolean:
          SetPrototypeOf(this.Realm.GetRuntimeObject(WellKnownObject.BooleanPrototype));
          break;
        case EcmaValueType.Number:
          SetPrototypeOf(this.Realm.GetRuntimeObject(WellKnownObject.NumberPrototype));
          break;
        case EcmaValueType.String:
          SetPrototypeOf(this.Realm.GetRuntimeObject(WellKnownObject.StringPrototype));
          break;
        case EcmaValueType.Symbol:
          throw new RuntimeObjectCloneException("Symbol value cannot be cloned");
      }
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[PrimitiveData]]", value);
    }
  }
}
