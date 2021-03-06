﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class ScriptFunction : RuntimeFunction {
    public ScriptFunction() {
      this.ParameterList = "";
      this.Source = "function (" + this.ParameterList + ") { }";
      InitProperty("", 0, false);
      SetPrototypeInternal(RuntimeObject.CreateFromConstructor<EcmaObject>(this.Realm.GetRuntimeObject(WellKnownObject.ObjectConstructor), WellKnownObject.ObjectPrototype));
    }

    public string ParameterList { get; private set; }

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
