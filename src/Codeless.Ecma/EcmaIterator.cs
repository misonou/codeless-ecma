using System;
using System.Collections.Generic;
using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public enum EcmaIteratorResultKind {
    Key,
    Value,
    Entry
  }

  public class EcmaIterator : RuntimeObject, IInspectorMetaProvider {
    private static readonly string[] KindString = { "key", "value", "entry" };
    private readonly IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> iterator;

    public EcmaIterator(EcmaValue target, EcmaIteratorResultKind kind, WellKnownObject proto)
      : base(proto) {
      this.iterator = new EcmaArrayEnumerator(target.ToObject());
      this.IteratedObject = target.ToObject();
      this.ResultKind = kind;
    }

    public EcmaIterator(EcmaMapBase target, EcmaIteratorResultKind kind, WellKnownObject proto)
      : base(proto) {
      Guard.ArgumentNotNull(target, "target");
      this.iterator = target.GetEnumerator();
      this.IteratedObject = target;
      this.ResultKind = kind;
    }

    public EcmaIteratorResultKind ResultKind { get; private set; }

    public RuntimeObject IteratedObject { get; private set; }

    public EcmaValue Next() {
      if (this.IteratedObject == null) {
        return CreateIterResultObject(EcmaValue.Undefined, true);
      }
      if (iterator.MoveNext()) {
        KeyValuePair<EcmaValue, EcmaValue> entry = iterator.Current;
        switch (this.ResultKind) {
          case EcmaIteratorResultKind.Key:
            return CreateIterResultObject(entry.Key, false);
          case EcmaIteratorResultKind.Value:
            return CreateIterResultObject(entry.Value, false);
          case EcmaIteratorResultKind.Entry:
            return CreateIterResultObject(new EcmaArray(new[] { entry.Key, entry.Value }), false);
        }
        throw new InvalidOperationException("Unknown kind value");
      }
      this.IteratedObject = null;
      return CreateIterResultObject(EcmaValue.Undefined, true);
    }

    [EcmaSpecification("CreateIterResultObject", EcmaSpecificationKind.AbstractOperations)]
    private EcmaValue CreateIterResultObject(EcmaValue value, bool done) {
      EcmaObject o = new EcmaObject();
      o.CreateDataProperty("value", value);
      o.CreateDataProperty("done", done);
      return o;
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[IterationKind]]", KindString[(int)this.ResultKind]);
      meta.EnumerableProperties.Add("[[IteratedObject]]", this.IteratedObject);
    }
  }
}
