using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class ScriptFunction : RuntimeFunction {
    public ScriptFunction() {
      InitProperty("", 0);
      SetPrototypeInternal(RuntimeObject.CreateFromConstructor<EcmaObject>(this.Realm.GetRuntimeObject(WellKnownObject.ObjectConstructor), WellKnownObject.ObjectPrototype));
    }

    public override bool IsConstructor => true;

    public string ParameterList => "";

    public string Source => "";

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return EcmaValue.Undefined;
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      return RuntimeObject.CreateFromConstructor<EcmaObject>(this, WellKnownObject.ObjectPrototype);
    }
  }
}
