using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal class IntrinsicModule : RuntimeModule<WellKnownObject> {
    protected override void OnBeforeInitializing(IList<RuntimeObject> moduleObjects) {
      moduleObjects[(int)WellKnownObject.ObjectPrototype] = RuntimeObject.Create(null);
      moduleObjects[(int)WellKnownObject.NumberPrototype] = new PrimitiveObject(0, WellKnownObject.ObjectPrototype);
      moduleObjects[(int)WellKnownObject.StringPrototype] = new PrimitiveObject("", WellKnownObject.ObjectPrototype);
      moduleObjects[(int)WellKnownObject.BooleanPrototype] = new PrimitiveObject(false, WellKnownObject.ObjectPrototype);
      moduleObjects[(int)WellKnownObject.ArrayPrototype] = new EcmaArray(WellKnownObject.ObjectPrototype);
    }

    protected override void OnAfterInitializing(IList<RuntimeObject> moduleObjects) {
      moduleObjects[(int)WellKnownObject.ThrowTypeError].SetIntegrityLevel(RuntimeObjectIntegrityLevel.Frozen);
    }

    protected override void DefineIntrinsicObjectsFromAssembly(Assembly assembly) {
      DefineIntrinsicObjectFromType(typeof(FunctionConstructor));
      base.DefineIntrinsicObjectsFromAssembly(assembly);
    }
  }
}
