using Codeless.Ecma.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  internal class IntrinsicObject : RuntimeObject, IEcmaIntrinsicObject, IInspectorMetaProvider {
    private EcmaValue value;

    public IntrinsicObject(EcmaValue value, WellKnownObject defaultProto)
      : base(defaultProto) {
      this.value = value;
    }

    public IntrinsicObject(EcmaValue value, WellKnownObject defaultProto, RuntimeObject constructor)
      : base(defaultProto, constructor) {
      this.value = value;
    }

    public virtual EcmaValue IntrinsicValue {
      get { return value; }
      set { this.value = value; }
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add(new InspectorMetaObjectEntry("[[PrimitiveData]]", value));
    }
  }
}
