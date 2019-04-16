using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public class InspectorMetaObjectEntryCollection : Collection<InspectorMetaObjectEntry> {
    public void Add(EcmaPropertyKey key, EcmaValue value) {
      Add(new InspectorMetaObjectEntry(key, value));
    }

    public void Add(EcmaPropertyKey key, RuntimeObject value) {
      Add(new InspectorMetaObjectEntry(key, value));
    }

    public void Add(EcmaPropertyKey key, object value) {
      Add(new InspectorMetaObjectEntry(key, new EcmaValue(value)));
    }
  }
}
