using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.Generator, Prototype = WellKnownObject.FunctionPrototype)]
  internal static class GeneratorFunctionPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.GeneratorFunction;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Prototype = WellKnownObject.GeneratorPrototype;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Constructor = WellKnownObject.GeneratorFunction;
  }
}
