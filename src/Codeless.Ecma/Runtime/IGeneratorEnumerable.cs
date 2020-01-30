using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal interface IGeneratorEnumerable {
    GeneratorState State { get; }
    EcmaValue Resume(GeneratorResumeState state, EcmaValue value);
  }
}
