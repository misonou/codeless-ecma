using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpPrototype)]
  internal static class RegExpPrototype {
    [IntrinsicMember]
    public static void Compile([This] EcmaValue thisArg, EcmaValue pattern, EcmaValue flags) {
      EcmaValue newRegExp = RegExpConstructor.RegExp(pattern, flags);
      EcmaValueUtility.SetIntrinsicValue(thisArg, newRegExp);
    }

    [IntrinsicMember]
    public static EcmaValue Exec([This] EcmaValue thisArg, EcmaValue input) {
      // TODO
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      Match m = re.Execute(input.ToString(), thisArg["lastIndex"].ToInt32());
      if (!m.Success) {
        thisArg["lastIndex"] = 0;
        return EcmaValue.Null;
      }
      List<EcmaValue> values = new List<EcmaValue>();
      thisArg["lastIndex"] = m.Index + m.Length;
      values.Add(m.Value);
      foreach (Group group in m.Groups) {
        values.Add(group.Value);
      }
      return new EcmaArray(values);
    }
    
    [IntrinsicMember]
    public static EcmaValue Test([This] EcmaValue thisArg, EcmaValue str) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.Test(str.ToString());
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) {
      return String.Concat("/", Source(thisArg), "/", Flags(thisArg));
    }
    
    [IntrinsicMember(Getter = true)]
    public static EcmaValue Flags([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      StringBuilder sb = new StringBuilder(5);
      if (re.Global) {
        sb.Append('g');
      }
      if (re.IgnoreCase) {
        sb.Append('i');
      }
      if (re.Multiline) {
        sb.Append('m');
      }
      if (re.IsUnicode) {
        sb.Append('u');
      }
      if (re.Sticky) {
        sb.Append('y');
      }
      return sb.ToString();
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Global([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.Global;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue IgnoreCase([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.IgnoreCase;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Multiline([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.Multiline;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Source([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.Pattern;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Sticky([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.Sticky;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Unicode([This] EcmaValue thisArg) {
      EcmaRegExp re = EcmaValueUtility.GetIntrinsicValue<EcmaRegExp>(thisArg);
      return re.IsUnicode;
    }

    [IntrinsicMember(WellKnownSymbol.Match)]
    public static EcmaValue Match([This] EcmaValue thisArg, EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember(WellKnownSymbol.Replace)]
    public static EcmaValue Replace([This] EcmaValue thisArg, EcmaValue str, EcmaValue replacement) {
      throw new NotImplementedException();
    }

    [IntrinsicMember(WellKnownSymbol.Search)]
    public static EcmaValue Search([This] EcmaValue thisArg, EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember(WellKnownSymbol.Split)]
    public static EcmaValue Split([This] EcmaValue thisArg, EcmaValue str) {
      throw new NotImplementedException();
    }
  }
}
