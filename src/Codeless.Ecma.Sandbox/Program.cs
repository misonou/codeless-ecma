using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Sandbox {
  class Program {
    static void Main(string[] args) {
      EcmaValue str = new EcmaValue("abs");
      EcmaValue num = 1;
      EcmaObject obj = new EcmaObject();
      EcmaValue sym = new EcmaValue(Symbol.Species);

      EcmaValue ht = new EcmaValue(new Hashtable());
      ht["prop"] = 1;
      EcmaValue htProp = ht["prop"];

      EcmaDate date = new EcmaDate();
      EcmaValue dateVal = new EcmaValue(new EcmaDate());
      
      EcmaValue dateGetDay = dateVal.Invoke("getDay");
      EcmaValue getDay = dateVal["getDay"];
      EcmaValue getDayCall = getDay.Call(dateVal);

      ((dynamic)dateVal).setHours(0, 0);
      EcmaValue midnight = dateVal.ToString();

    
      dynamic htDyn = ht;
      htDyn.prop = 2;

      dynamic shouldBeHt = (htDyn["prop"] - 2) || ht;
      string shouldBeStr2 = htDyn.prop.toString();
      
      dynamic objProto = new EcmaValue(RuntimeRealm.GetWellKnownObject(WellKnownObject.ObjectPrototype));
      string result = objProto.toString.call(objProto);
      string result2 = objProto.toString.call(dateVal);

      RuntimeObject trans = num.ToRuntimeObject();

      RuntimeObject global = RuntimeRealm.GetWellKnownObject(WellKnownObject.Global);

      Debugger.Break();
    }
  }
}
