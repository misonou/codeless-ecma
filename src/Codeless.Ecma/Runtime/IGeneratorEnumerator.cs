using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  internal interface IGeneratorEnumerator : IEnumerator<EcmaValue>, IEnumerable<EcmaValue> {
    void Init(IGeneratorContext context);
    void PushStack(IGeneratorEnumerator other);
    bool SetException(Exception ex);
    bool SetFinally();
  }
}
