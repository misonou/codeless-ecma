using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncFromSyncIteratorPrototype, Prototype = WellKnownObject.AsyncIteratorPrototype)]
  internal static class AsyncFromSyncIteratorPrototype {
    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue, EcmaValue value) {
      AsyncFromSyncIterator generator = thisValue.GetUnderlyingObject<AsyncFromSyncIterator>();
      return generator.Next(value);
    }

    [IntrinsicMember]
    public static EcmaValue Return([This] EcmaValue thisValue, EcmaValue value) {
      AsyncFromSyncIterator generator = thisValue.GetUnderlyingObject<AsyncFromSyncIterator>();
      return generator.Return(value);
    }

    [IntrinsicMember]
    public static EcmaValue Throw([This] EcmaValue thisValue, EcmaValue value) {
      AsyncFromSyncIterator generator = thisValue.GetUnderlyingObject<AsyncFromSyncIterator>();
      return generator.Throw(value);
    }
  }
}
