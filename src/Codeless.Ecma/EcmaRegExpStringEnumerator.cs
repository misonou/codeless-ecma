using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class EcmaRegExpStringEnumerator : EcmaIterator, IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
    private readonly EcmaValue re;
    private readonly bool unicode;
    private readonly bool global;
    private readonly string inputString;
    private bool done;
    private IEcmaRegExpResult result;

    public EcmaRegExpStringEnumerator(EcmaValue re, string inputString)
      : base(re, EcmaIteratorResultKind.Value, WellKnownObject.RegExpStringIteratorPrototype) {
      this.re = re;
      this.global = re[WellKnownProperty.Global].ToBoolean();
      this.unicode = re[WellKnownProperty.Unicode].ToBoolean();
      this.inputString = inputString;
    }

    public KeyValuePair<EcmaValue, EcmaValue> Current {
      get { return new KeyValuePair<EcmaValue, EcmaValue>(default, result.ToValue()); }
    }

    public bool MoveNext() {
      if (done) {
        return false;
      }
      result = RegExpPrototype.CreateExecCallback(re)(inputString);
      bool success = result != null;
      if (!global || !success) {
        done = true;
      }
      if (success && result.Value.Length == 0) {
        RegExpPrototype.AdvanceStringIndex(re, inputString, unicode);
      }
      return success;
    }

    protected override IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject) {
      return this;
    }

    #region Interface
    object IEnumerator.Current {
      get { return this.Current; }
    }

    void IEnumerator.Reset() {
      throw new InvalidOperationException();
    }

    void IDisposable.Dispose() { }
    #endregion
  }
}
