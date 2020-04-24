using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Intl.Utilities;

namespace Codeless.Ecma.Intl {
  public enum NumberStyle {
    [StringValue("decimal")]
    Decimal,
    [StringValue("percent")]
    Percent,
    [StringValue("currency")]
    Currency,
    [StringValue("unit")]
    Unit
  }

  public enum NumberNotation {
    [StringValue("standard")]
    Standard,
    [StringValue("scientific")]
    Scientific,
    [StringValue("Engineering")]
    Engineering,
    [StringValue("compact")]
    Compact
  }

  public enum NumberCompactDisplayFormat {
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum CurrencyDisplayFormat {
    [StringValue("symbol")]
    Symbol,
    [StringValue("code")]
    Code,
    [StringValue("name")]
    Name
  }

  public enum CurrencySignFormat {
    [StringValue("standard")]
    Standard,
    [StringValue("accounting")]
    Accounting
  }

  public enum UnitDisplayFormat {
    Unspecified,
    [StringValue("narrow")]
    Narrow,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum SignDisplayFormat {
    [StringValue("auto")]
    Auto,
    [StringValue("never")]
    Never,
    [StringValue("always")]
    Always,
    [StringValue("exceptZero")]
    ExceptZero
  }

  public class NumberFormatOptions : IntlProviderOptions {
    public NumberFormatOptions() { }

    public NumberFormatOptions(EcmaValue value)
      : base(value) { }

    public NumberStyle Style {
      get => GetOption(PropertyKey.Style, NumberStyle.Decimal);
      set => SetOption(PropertyKey.Style, value);
    }

    public NumberNotation Notation {
      get => GetOption(PropertyKey.Notation, NumberNotation.Standard);
      set => SetOption(PropertyKey.Notation, value);
    }

    public string Currency {
      get => GetOption(PropertyKey.Currency, (string)null);
      set => SetOption(PropertyKey.Currency, value);
    }

    public CurrencyDisplayFormat CurrencyDisplay {
      get => GetOption(PropertyKey.CurrencyDisplay, CurrencyDisplayFormat.Symbol);
      set => SetOption(PropertyKey.CurrencyDisplay, value);
    }

    public CurrencySignFormat CurrencySign {
      get => GetOption(PropertyKey.CurrencySign, CurrencySignFormat.Standard);
      set => SetOption(PropertyKey.CurrencySign, value);
    }

    public string Unit {
      get => GetOption(PropertyKey.Unit, (string)null);
      set => SetOption(PropertyKey.Unit, value);
    }

    public UnitDisplayFormat UnitDisplay {
      get => GetOption(PropertyKey.UnitDisplay, UnitDisplayFormat.Short);
      set => SetOption(PropertyKey.UnitDisplay, value);
    }

    public NumberCompactDisplayFormat CompactDisplay {
      get => GetOption(PropertyKey.CompactDisplay, NumberCompactDisplayFormat.Short);
      set => SetOption(PropertyKey.CompactDisplay, value);
    }

    public SignDisplayFormat SignDisplay {
      get => GetOption(PropertyKey.SignDisplay, SignDisplayFormat.Auto);
      set => SetOption(PropertyKey.SignDisplay, value);
    }

    public bool UseGrouping {
      get => GetOption(PropertyKey.UseGrouping, true);
      set => SetOption(PropertyKey.UseGrouping, value);
    }

    public string NumberingSystem {
      get => GetOption(PropertyKey.NumberingSystem, (string)null);
      set => SetOption(PropertyKey.NumberingSystem, value);
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

    public new NumberFormatDigitOptions GetNumberFormatDigitOptions(int minFractionDigits, int maxFractionDigits, NumberNotation notation) {
      return base.GetNumberFormatDigitOptions(minFractionDigits, maxFractionDigits, notation);
    }
  }
}
