using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public abstract class StatefulIterator : RuntimeObject, IGeneratorEnumerable {
    public StatefulIterator(WellKnownObject proto)
      : base(proto) { }

    public GeneratorState State { get; protected set; }

    public EcmaValue Next() {
      return Resume(GeneratorResumeState.Resume, EcmaValue.Undefined);
    }

    public EcmaValue Next(EcmaValue value) {
      return Resume(GeneratorResumeState.Resume, value);
    }

    public EcmaValue Throw() {
      return Resume(GeneratorResumeState.Throw, EcmaValue.Undefined);
    }

    public EcmaValue Throw(EcmaValue value) {
      return Resume(GeneratorResumeState.Throw, value);
    }

    public EcmaValue Return() {
      return Resume(GeneratorResumeState.Return, EcmaValue.Undefined);
    }

    public EcmaValue Return(EcmaValue value) {
      return Resume(GeneratorResumeState.Return, value);
    }

    public abstract EcmaValue Resume(GeneratorResumeState state, EcmaValue value);
  }
}
