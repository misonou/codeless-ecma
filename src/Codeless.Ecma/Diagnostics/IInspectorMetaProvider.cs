using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Diagnostics {
  public interface IInspectorMetaProvider {
    void FillInInspectorMetaObject(InspectorMetaObject meta);
  }
}
