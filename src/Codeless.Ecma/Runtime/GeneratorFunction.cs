using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  public class GeneratorFunction : RuntimeFunction {
    private GeneratorDelegate generator;

    public GeneratorFunction(GeneratorDelegate generator)
      : base(WellKnownObject.Generator) {
      Init(generator);
    }

    internal void Init(GeneratorDelegate generator) {
      Guard.ArgumentNotNull(generator, "generator");
      this.generator = generator;
      this.Source = "function* () { [native code] }";
      this.DefinePropertyOrThrow(WellKnownProperty.Prototype, new EcmaPropertyDescriptor(RuntimeObject.Create(this.Realm.GetRuntimeObject(WellKnownObject.GeneratorPrototype)), EcmaPropertyAttributes.Writable));
    }

    protected override EcmaValue Invoke(RuntimeFunctionInvocation invocation, EcmaValue[] arguments) {
      return new Generator(invocation, new GeneratorDelegateEnumerator(generator));
    }
  }
}
