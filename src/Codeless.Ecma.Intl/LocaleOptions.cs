using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public class LocaleOptions : IntlProviderOptions {
    public LocaleOptions() { }

    public LocaleOptions(EcmaValue value)
      : base(value) { }

    public string Language {
      get => GetOption(PropertyKey.Language, (string)null);
      set => SetOption(PropertyKey.Language, value);
    }

    public string Script {
      get => GetOption(PropertyKey.Script, (string)null);
      set => SetOption(PropertyKey.Script, value);
    }

    public string Region {
      get => GetOption(PropertyKey.Region, (string)null);
      set => SetOption(PropertyKey.Region, value);
    }

    public string Collation {
      get => GetOption(PropertyKey.Collation, (string)null);
      set => SetOption(PropertyKey.Collation, value);
    }

    public string NumberingSystem {
      get => GetOption(PropertyKey.NumberingSystem, (string)null);
      set => SetOption(PropertyKey.NumberingSystem, value);
    }

    public string Calendar {
      get => GetOption(PropertyKey.Calendar, (string)null);
      set => SetOption(PropertyKey.Calendar, value);
    }

    public string HourCycle {
      get => GetOption(PropertyKey.HourCycle, (string)null);
      set => SetOption(PropertyKey.HourCycle, value);
    }

    public string CaseFirst {
      get => GetOption(PropertyKey.CaseFirst, (string)null);
      set => SetOption(PropertyKey.CaseFirst, value);
    }

    public bool? Numeric {
      get => GetOption(PropertyKey.Numeric, (bool?)null);
      set => SetOption(PropertyKey.Numeric, value);
    }
  }
}
