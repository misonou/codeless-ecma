using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.InteropServices {
  internal interface INativeObjectWrapper {
    object Target { get; }
  }
}
