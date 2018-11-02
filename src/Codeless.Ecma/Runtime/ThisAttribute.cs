using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Parameter)]
  public class ThisAttribute : Attribute { }

  [AttributeUsage(AttributeTargets.Parameter)]
  public class NewTargetAttribute : Attribute { }
}
