using Codeless.Ecma.Runtime;
using System.Collections;

namespace Codeless.Ecma.InteropServices {
  internal class NativeObject {
    public static RuntimeObject Create(object target) {
      switch (target) {
        case IDictionary dict:
          return new NativeDictionaryObject(dict);
        case IList list:
          return new NativeArrayObject(list);
        default:
          return new ReflectedNativeObject(target);
      }
    }
  }

  internal abstract class NativeObject<T> : RuntimeObject, INativeObjectWrapper {
    protected NativeObject(T target, WellKnownObject proto)
      : base(target) {
      SetPrototypeOf(RuntimeRealm.Current.GetRuntimeObject(proto));
      this.Target = target;
    }

    public new T Target { get; }

    object INativeObjectWrapper.Target => this.Target;
  }
}
