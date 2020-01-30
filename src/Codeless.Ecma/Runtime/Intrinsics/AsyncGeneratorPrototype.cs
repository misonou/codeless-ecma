using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncGeneratorPrototype, Prototype = WellKnownObject.AsyncIteratorPrototype)]
  internal static class AsyncGeneratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.AsyncGenerator;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Constructor = WellKnownObject.AsyncGenerator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue, EcmaValue value) {
      try {
        AsyncGenerator generator = thisValue.GetUnderlyingObject<AsyncGenerator>();
        return generator.Next(value);
      } catch (Exception ex) {
        return Promise.Reject(ex);
      }
    }

    [IntrinsicMember]
    public static EcmaValue Return([This] EcmaValue thisValue, EcmaValue value) {
      try {
        AsyncGenerator generator = thisValue.GetUnderlyingObject<AsyncGenerator>();
        return generator.Return(value);
      } catch (Exception ex) {
        return Promise.Reject(ex);
      }
    }

    [IntrinsicMember]
    public static EcmaValue Throw([This] EcmaValue thisValue, EcmaValue value) {
      try {
        AsyncGenerator generator = thisValue.GetUnderlyingObject<AsyncGenerator>();
        return generator.Throw(value);
      } catch (Exception ex) {
        return Promise.Reject(ex);
      }
    }
  }
}
