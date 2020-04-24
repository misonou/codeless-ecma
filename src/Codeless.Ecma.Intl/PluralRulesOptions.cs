using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Intl.Utilities;

namespace Codeless.Ecma.Intl {
  public enum PluralRuleType {
    [StringValue("cardinal")]
    Cardinal,
    [StringValue("ordinal")]
    Ordinal
  }

  public class PluralRulesOptions : IntlProviderOptions {
    public PluralRulesOptions() { }

    public PluralRulesOptions(EcmaValue value)
      : base(value) { }

    public PluralRuleType Type {
      get => GetOption(PropertyKey.Type, PluralRuleType.Cardinal);
      set => SetOption(PropertyKey.Type, value);
    }

    public NumberFormatDigitOptions Digits {
      get { return GetNumberFormatDigitOptions(0, 3, NumberNotation.Standard); }
    }

    public int MinimumIntegerDigits {
      set => SetOption(PropertyKey.MinimumIntegerDigits, value);
    }

    public int MinimumFractionDigits {
      set => SetOption(PropertyKey.MinimumFractionDigits, value);
    }

    public int MaximumFractionDigits {
      set => SetOption(PropertyKey.MaximumFractionDigits, value);
    }

    public int MinimumSignificantDigits {
      set => SetOption(PropertyKey.MinimumSignificantDigits, value);
    }

    public int MaximumSignificantDigits {
      set => SetOption(PropertyKey.MaximumSignificantDigits, value);
    }
  }
}
