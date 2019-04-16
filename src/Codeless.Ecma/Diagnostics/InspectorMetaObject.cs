using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public class InspectorMetaObject {
    public InspectorMetaObject(RuntimeObject obj) {
      Guard.ArgumentNotNull(obj, "obj");
      this.EnumerableProperties = new InspectorMetaObjectEntryCollection();
      this.NonEnumerableProperties = new InspectorMetaObjectEntryCollection();
      this.Prototype = new EcmaValue(obj.GetPrototypeOf());
      if (this.Prototype.IsNullOrUndefined) {
        this.Prototype = EcmaValue.Null;
      }
      for (RuntimeObject cur = obj; cur != null; cur = cur.GetPrototypeOf()) {
        foreach (EcmaPropertyKey property in cur.OwnPropertyKeys) {
          EcmaPropertyDescriptor descriptor = cur.GetOwnProperty(property);
          if (descriptor.IsDataDescriptor && cur != obj) {
            continue;
          }
          EcmaValue value;
          if (descriptor.IsDataDescriptor) {
            value = descriptor.Value;
          } else {
            try {
              value = descriptor.Get.Call(obj);
            } catch (EcmaException ex) {
              value = String.Format("{0}: {1}", ex.ErrorType, ex.Message);
            } catch (Exception ex) {
              value = ex.Message;
            }
            if (cur == obj) {
              if (descriptor.Get) {
                NonEnumerableProperties.Add("get " + property, descriptor.Get);
              }
              if (descriptor.Set) {
                NonEnumerableProperties.Add("set " + property, descriptor.Get);
              }
            }
          }
          if (descriptor.Enumerable.Value) {
            EnumerableProperties.Add(property, value);
          } else {
            NonEnumerableProperties.Add(property, value);
          }
        }
      }
      if (obj is IInspectorMetaProvider provider) {
        try {
          provider.FillInInspectorMetaObject(this);
        } catch { }
      }
    }

    public InspectorMetaObjectEntryCollection EnumerableProperties { get; private set; }
    public InspectorMetaObjectEntryCollection NonEnumerableProperties { get; private set; }
    public EcmaValue Prototype { get; private set; }
  }
}
