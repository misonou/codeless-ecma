using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class IntrinsicFunction : NativeRuntimeFunction {
    private readonly WellKnownObject parentObject;
    private readonly EcmaPropertyKey propertyKey;

    public IntrinsicFunction(string name, MethodInfo method, WellKnownObject parentObject, EcmaPropertyKey propertyKey)
      : base(name, method) {
      this.parentObject = parentObject;
      this.propertyKey = propertyKey;
    }

    public bool IsIntrinsicFunction(WellKnownObject obj, EcmaPropertyKey name) {
      return parentObject == obj && propertyKey == name;
    }
  }
}
