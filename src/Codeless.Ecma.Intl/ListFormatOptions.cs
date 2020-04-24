using Codeless.Ecma.Intl.Utilities;
using System;

namespace Codeless.Ecma.Intl {
  public enum ListStyle {
    Unspecified,
    [StringValue("narrow")]
    Narrow,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum ListType {
    [StringValue("conjunction")]
    Conjunction,
    [StringValue("disjunction")]
    Disjuction,
    [StringValue("unit")]
    Unit
  }

  public class ListFormatOptions : IntlProviderOptions {
    public ListFormatOptions() { }

    public ListFormatOptions(EcmaValue value)
      : base(value) { }

    public ListStyle Style {
      get => GetOption(PropertyKey.Style, ListStyle.Long);
      set => SetOption(PropertyKey.Style, value);
    }

    public ListType Type {
      get => GetOption(PropertyKey.Type, ListType.Conjunction);
      set => SetOption(PropertyKey.Type, value);
    }
  }
}
