using Codeless.Ecma.Intl.Utilities;
using System;

namespace Codeless.Ecma.Intl {
  public enum RelativeTimeStyle {
    [StringValue("narrow")]
    Narrow,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum RelativeTimeNumericFormat {
    [StringValue("always")]
    Always,
    [StringValue("auto")]
    Auto
  }

  public class RelativeTimeFormatOptions : IntlProviderOptions {
    public RelativeTimeFormatOptions() { }

    public RelativeTimeFormatOptions(EcmaValue value)
      : base(value) { }

    public RelativeTimeStyle Style {
      get => GetOption(PropertyKey.Style, RelativeTimeStyle.Long);
      set => SetOption(PropertyKey.Style, value);
    }

    public RelativeTimeNumericFormat Numeric {
      get => GetOption(PropertyKey.Numeric, RelativeTimeNumericFormat.Always);
      set => SetOption(PropertyKey.Numeric, value);
    }

    public string NumberingSystem {
      get => GetOption(PropertyKey.NumberingSystem, (string)null);
      set => SetOption(PropertyKey.NumberingSystem, value);
    }
  }
}
