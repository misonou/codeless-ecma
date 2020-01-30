using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.GeneratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class GeneratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.Generator;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Constructor = WellKnownObject.Generator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue, EcmaValue value) {
      Generator generator = thisValue.GetUnderlyingObject<Generator>();
      return generator.Next(value);
    }

    [IntrinsicMember]
    public static EcmaValue Return([This] EcmaValue thisValue, EcmaValue value) {
      Generator generator = thisValue.GetUnderlyingObject<Generator>();
      return generator.Return(value);
    }

    [IntrinsicMember]
    public static EcmaValue Throw([This] EcmaValue thisValue, EcmaValue value) {
      Generator generator = thisValue.GetUnderlyingObject<Generator>();
      return generator.Throw(value);
    }
  }
}
