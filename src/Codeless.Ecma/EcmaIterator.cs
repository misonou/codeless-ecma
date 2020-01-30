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

  public abstract class EcmaIterator : RuntimeObject, IInspectorMetaProvider {
    private static readonly string[] KindString = { "key", "value", "entry" };
    private readonly IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> iterator;

    public EcmaIterator(object target, EcmaIteratorResultKind kind, WellKnownObject proto)
      : base(proto) {
      Guard.ArgumentNotNull(target, "target");
      this.iterator = GetEnumerator(target);
      this.IteratedObject = target;
      this.ResultKind = kind;
    }

    public EcmaIteratorResultKind ResultKind { get; private set; }

    public object IteratedObject { get; private set; }

    public virtual EcmaValue Next() {
      if (this.IteratedObject == null) {
        return EcmaValueUtility.CreateIterResultObject(EcmaValue.Undefined, true);
      }
      if (iterator.MoveNext()) {
        KeyValuePair<EcmaValue, EcmaValue> entry = iterator.Current;
        switch (this.ResultKind) {
          case EcmaIteratorResultKind.Key:
            return EcmaValueUtility.CreateIterResultObject(entry.Key, false);
          case EcmaIteratorResultKind.Value:
            return EcmaValueUtility.CreateIterResultObject(entry.Value, false);
          case EcmaIteratorResultKind.Entry:
            return EcmaValueUtility.CreateIterResultObject(new EcmaArray(new[] { entry.Key, entry.Value }), false);
        }
        throw new InvalidOperationException("Unknown kind value");
      }
      Close();
      return EcmaValueUtility.CreateIterResultObject(EcmaValue.Undefined, true);
    }

    protected abstract IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject);

    protected void Close() {
      if (this.IteratedObject != null) {
        iterator.Dispose();
        this.IteratedObject = null;
      }
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[IterationKind]]", KindString[(int)this.ResultKind]);
      meta.EnumerableProperties.Add("[[IteratedObject]]", this.IteratedObject);
    }
  }
}
