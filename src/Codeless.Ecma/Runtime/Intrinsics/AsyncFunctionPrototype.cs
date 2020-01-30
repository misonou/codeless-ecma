using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncFunctionPrototype, Prototype = WellKnownObject.FunctionPrototype)]
  internal static class AsyncFunctionPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.AsyncFunction;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable)]
    public const WellKnownObject Constructor = WellKnownObject.AsyncFunction;
  }
}
