using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public class EcmaRegExpStringEnumerator : IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
    private readonly EcmaValue re;
    private readonly bool unicode;
    private readonly bool global;
    private readonly string inputString;
    private RegExpPrototype.ExecCallback callback;
    private IEcmaRegExpResult result;

    public EcmaRegExpStringEnumerator(EcmaValue re, string inputString) {
      this.re = re;
      this.global = re[WellKnownProperty.Global].ToBoolean();
      this.unicode = re[WellKnownProperty.Unicode].ToBoolean();
      this.inputString = inputString;
      this.callback = RegExpPrototype.CreateExecCallback(re);
    }

    public KeyValuePair<EcmaValue, EcmaValue> Current {
      get { return new KeyValuePair<EcmaValue, EcmaValue>(default, result.ToValue()); }
    }

    public bool MoveNext() {
      if (callback == null) {
        return false;
      }
      result = callback(inputString);
      bool success = result != null;
      if (!global || !success) {
        callback = null;
      }
      if (success && result.Value.Length == 0) {
        RegExpPrototype.AdvanceStringIndex(re, inputString, unicode);
      }
      return success;
    }

    public void Reset() {
      throw new InvalidOperationException();
    }

    #region Interface
    object IEnumerator.Current {
      get { return this.Current; }
    }

    void IDisposable.Dispose() { }
    #endregion
  }
}
