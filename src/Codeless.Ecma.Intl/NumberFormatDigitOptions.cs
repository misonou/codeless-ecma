using System;

namespace Codeless.Ecma.Intl {
  public enum RoundingType {
    SignificantDigits,
    FractionDigits,
    CompactRounding
  }

  public class NumberFormatDigitOptions {
    internal NumberFormatDigitOptions() { }

    public bool UseSignificantDigits { get; internal set; }
    public RoundingType RoundingType { get; internal set; }

    public int MinimumIntegerDigits { get; internal set; }
    public int MinimumFractionDigits { get; internal set; }
    public int MaximumFractionDigits { get; internal set; }
    public int MinimumSignificantDigits { get; internal set; }
    public int MaximumSignificantDigits { get; internal set; }
  }
}
