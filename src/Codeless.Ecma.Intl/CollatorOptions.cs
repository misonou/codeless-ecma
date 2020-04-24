using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Intl.Utilities;

namespace Codeless.Ecma.Intl {
  public enum CollatorUsage {
    [StringValue("sort")]
    Sort,
    [StringValue("search")]
    Search
  }

  public enum CollatorSensitivity {
    Unspecified,
    [StringValue("base")]
    Base,
    [StringValue("accent")]
    Accent,
    [StringValue("case")]
    Case,
    [StringValue("variant")]
    Variant
  }

  public enum CollatorCaseFirst {
    Unspecified,
    [StringValue("false")]
    False,
    [StringValue("upper")]
    Upper,
    [StringValue("lower")]
    Lower
  }

  public class CollatorOptions : IntlProviderOptions {
    public CollatorOptions() { }

    public CollatorOptions(EcmaValue value)
      : base(value) { }

    public CollatorUsage Usage {
      get { return GetOption(PropertyKey.Usage, CollatorUsage.Sort); }
      set { SetOption(PropertyKey.Usage, value); }
    }

    public CollatorCaseFirst CaseFirst {
      get { return GetOption(PropertyKey.CaseFirst, CollatorCaseFirst.Unspecified); }
      set { SetOption(PropertyKey.CaseFirst, value); }
    }

    public CollatorSensitivity Sensitivity {
      get { return GetOption(PropertyKey.Sensitivity, CollatorSensitivity.Unspecified); }
      set { SetOption(PropertyKey.Sensitivity, value); }
    }

    public bool? Numeric {
      get { return GetOption(PropertyKey.Numeric, BooleanNull); }
      set { SetOption(PropertyKey.Numeric, value); }
    }

    public bool IgnorePunctuation {
      get { return GetOption(PropertyKey.IgnorePunctuation, false); }
      set { SetOption(PropertyKey.IgnorePunctuation, value); }
    }
  }
}
