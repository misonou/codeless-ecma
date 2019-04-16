using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics.VisualStudio {
  internal class EcmaValueDebuggerProxy {
    public EcmaValueDebuggerProxy(EcmaValue value) {
      if (value.Type == EcmaValueType.Object) {
        this.Root = new RuntimeObjectDebuggerProxy(value.ToObject());
      }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object Root { get; private set; }
  }
}
