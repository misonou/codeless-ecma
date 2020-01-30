using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  internal class RecursiveYieldAsyncEnumerator : RecursiveYieldEnumerator {
    public RecursiveYieldAsyncEnumerator(IGeneratorEnumerable iterator)
      : base(iterator) { }

    protected override EcmaValue GetNextValue(GeneratorResumeState state, EcmaValue value) {
      return Promise.Resolve(this.Iterator.Resume(state, value), UnwrapResult, null);
    }
  }
}
