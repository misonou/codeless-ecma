using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public static class New {
    public static EcmaValue Target {
      get {
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        if (invocation != null && invocation.NewTarget != null) {
          return invocation.NewTarget;
        }
        throw new EcmaSyntaxErrorException("new.target expression is not allowed here");
      }
    }
  }
}
