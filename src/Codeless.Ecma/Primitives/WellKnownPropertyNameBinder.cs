using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Primitives {
  internal class WellKnownPropertyNameBinder : StringBinder {
    private static readonly Hashtable longDict = new Hashtable();
    private static readonly string[] strArr;

    public new static readonly WellKnownPropertyNameBinder Default = new WellKnownPropertyNameBinder();

    protected WellKnownPropertyNameBinder() { }

    static WellKnownPropertyNameBinder() {
      string[] names = Enum.GetNames(typeof(WellKnownPropertyName));
      strArr = new string[names.Length + 1];
      for (int i = 0; i < names.Length; i++) {
        string name = String.Intern(names[i].Substring(0, 1).ToLower() + names[i].Substring(1));
        strArr[i + 1] = name;
        longDict[name] = new EcmaValueHandle(i + 1);
      }
    }

    public static bool IsWellKnownPropertyName(string s) {
      return String.IsInterned(s) != null && longDict[s] != null;
    }

    public override string FromHandle(EcmaValueHandle handle) {
      int index = (int)handle.Value;
      if (index >= 0 && index < strArr.Length) {
        return strArr[index];
      }
      throw new ArgumentException("Invalid value", "handle");
    }

    public override EcmaValueHandle ToHandle(string value) {
      if (longDict[value] is EcmaValueHandle handle) {
        return handle;
      }
      throw new ArgumentException("Invalid value", "value");
    }
  }
}