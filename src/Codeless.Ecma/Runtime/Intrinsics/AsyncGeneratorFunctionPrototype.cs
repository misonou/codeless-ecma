using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncGenerator, Prototype = WellKnownObject.FunctionPrototype)]
  internal static class AsyncGeneratorFunctionPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.AsyncGeneratorFunction;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Prototype = WellKnownObject.AsyncGeneratorPrototype;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Constructor = WellKnownObject.AsyncGeneratorFunction;
  }
}
