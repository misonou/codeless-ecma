using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class WellKnownPropertyNameBinder : NativeStringBinder {
    public static readonly WellKnownPropertyNameBinder Default = new WellKnownPropertyNameBinder();

    protected WellKnownPropertyNameBinder() { }

    private static readonly Map<string, long> dictionary = new Map<string, long>();

    static WellKnownPropertyNameBinder() {
      int i = 1;
      foreach (string str in Enum.GetNames(typeof(WellKnownPropertyName))) {
        dictionary.Add(str.Substring(0, 1).ToLower() + str.Substring(1), i++);
      }
    }

    public override string FromHandle(EcmaValueHandle handle) {
      return dictionary.Reverse[(int)handle.Value];
    }

    public override EcmaValueHandle ToHandle(string value) {
      return new EcmaValueHandle(dictionary.Forward[value]);
    }
  }
}