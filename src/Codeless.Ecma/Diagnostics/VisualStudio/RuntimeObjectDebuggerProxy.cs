using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics.VisualStudio {
  internal class RuntimeObjectDebuggerProxy {
    public RuntimeObjectDebuggerProxy(RuntimeObject obj) {
      InspectorMetaObject meta = new InspectorMetaObject(obj);
      this.EnumerableProperties = meta.EnumerableProperties.ToArray();
      this.NonEnumerableProperties = meta.NonEnumerableProperties.ToArray();
      this.Prototype = meta.Prototype;
    }

    [DebuggerDisplay("{Prototype.DebuggerDisplay,nq}")]
    public EcmaValue Prototype { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public InspectorMetaObjectEntry[] EnumerableProperties { get; private set; }

    [DebuggerDisplay("{NonEnumerablePropertiesDebuggerDisplay,nq}", Name = "Non-enumerable properties")]
    public InspectorMetaObjectEntry[] NonEnumerableProperties { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string NonEnumerablePropertiesDebuggerDisplay {
      get {
        StringBuilder sb = new StringBuilder("{");
        foreach (InspectorMetaObjectEntry e in NonEnumerableProperties) {
          if (sb.Length > 50) {
            sb.Append(", ...");
            break;
          }
          if (sb.Length > 1) {
            sb.Append(", ");
          }
          sb.Append(e.Key);
        }
        sb.Append("}");
        return sb.ToString();
      }
    }
  }
}
