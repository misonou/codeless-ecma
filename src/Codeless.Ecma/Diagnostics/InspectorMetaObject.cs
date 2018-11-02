using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public class InspectorMetaObject {
    public InspectorMetaObject(RuntimeObject obj) {
      this.EnumerableProperties = new InspectorMetaObjectEntryCollection();
      this.NonEnumerableProperties = new InspectorMetaObjectEntryCollection();
      this.Prototype = new EcmaValue(obj.GetPrototypeOf());
      if (this.Prototype.IsNullOrUndefined) {
        this.Prototype = EcmaValue.Null;
      }
      foreach(EcmaPropertyKey property in obj.OwnPropertyKeys) {
        if (obj.GetOwnProperty(property).Enumerable.Value) {
          EnumerableProperties.Add(new InspectorMetaObjectEntry(property, obj.Get(property)));
        } else {
          NonEnumerableProperties.Add(new InspectorMetaObjectEntry(property, obj.Get(property)));
        }
      }
      IInspectorMetaProvider provider = obj as IInspectorMetaProvider;
      if (provider != null) {
        provider.FillInInspectorMetaObject(this);
      }
    }

    public InspectorMetaObjectEntryCollection EnumerableProperties { get; private set; }
    public InspectorMetaObjectEntryCollection NonEnumerableProperties { get; private set; }
    public EcmaValue Prototype { get; private set; }
  }
}
