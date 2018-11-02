using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  [DebuggerDisplay("{Value.DebuggerDisplay,nq}", Name = "{Key.ToString(),nq}")]
  public class InspectorMetaObjectEntry {
    public InspectorMetaObjectEntry(EcmaPropertyKey key, EcmaValue value) {
      this.Key = key;
      this.Value = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public EcmaPropertyKey Key { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public EcmaValue Value { get; set; }
  }
}
