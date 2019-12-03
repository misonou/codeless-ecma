using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Primitives {
  internal class WellKnownPropertyNameBinder : StringBinder {
    private static readonly Hashtable longDict = new Hashtable();
    private static readonly List<string> strArr = new List<string>();

    public new static readonly WellKnownPropertyNameBinder Default = new WellKnownPropertyNameBinder();

    protected WellKnownPropertyNameBinder() { }

    public static bool IsWellKnownPropertyName(string s) {
      return s != null && String.IsInterned(s) != null && s.Length < 25;
    }

    public override string FromHandle(EcmaValueHandle handle) {
      return strArr[(int)handle.Value];
    }

    public override EcmaValueHandle ToHandle(string value) {
      if (longDict[value] is EcmaValueHandle handle) {
        return handle;
      }
      lock (longDict) {
        if (longDict[value] is EcmaValueHandle handle2) {
          return handle2;
        }
        EcmaValueHandle handle3 = new EcmaValueHandle(strArr.Count);
        longDict[value] = handle3;
        strArr.Add(value);
        return handle3;
      }
    }
  }
}
