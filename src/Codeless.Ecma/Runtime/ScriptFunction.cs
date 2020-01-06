using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class ScriptFunction : RuntimeFunction {
    public ScriptFunction() {
      this.ParameterList = "";
      this.Source = "";
      InitProperty("", 0);
      SetPrototypeInternal(RuntimeObject.CreateFromConstructor<EcmaObject>(this.Realm.GetRuntimeObject(WellKnownObject.ObjectConstructor), WellKnownObject.ObjectPrototype));
    }

    public string ParameterList { get; private set; }

    public string Source { get; private set; }

    public override bool IsConstructor {
      get { return this.HomeObject == null || this.HomeObject == this; }
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return EcmaValue.Undefined;
    }

    protected override RuntimeObject ConstructThisValue(RuntimeObject newTarget) {
      return RuntimeObject.CreateFromConstructor<EcmaObject>(this, WellKnownObject.ObjectPrototype);
    }
  }
}
