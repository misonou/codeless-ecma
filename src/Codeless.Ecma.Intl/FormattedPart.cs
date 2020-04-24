using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Codeless.Ecma.Intl {
  public enum FormattedPartType {
    Placeholder,
    StartRangePlaceholder,
    EndRangePlaceholder,
    SharedPlaceholder,
    [StringValue("literal")]
    Literal,
    [StringValue("element")]
    Element,
    [StringValue("decimal")]
    Decimal,
    [StringValue("fraction")]
    Fraction,
    [StringValue("group")]
    Group,
    [StringValue("infinity")]
    Infinity,
    [StringValue("integer")]
    Integer,
    [StringValue("minusSign")]
    MinusSign,
    [StringValue("nan")]
    NaN,
    [StringValue("plusSign")]
    PlusSign,
    [StringValue("percentSign")]
    PercentSign,
    [StringValue("day")]
    Day,
    [StringValue("dayPeriod")]
    DayPeriod,
    [StringValue("era")]
    Era,
    [StringValue("fractionalSecond")]
    FractionalSecond,
    [StringValue("hour")]
    Hour,
    [StringValue("minute")]
    Minute,
    [StringValue("month")]
    Month,
    [StringValue("relatedYear")]
    RelatedYear,
    [StringValue("second")]
    Second,
    [StringValue("timeZoneName")]
    TimeZoneName,
    [StringValue("weekday")]
    Weekday,
    [StringValue("year")]
    Year,
    [StringValue("yearName")]
    YearName,
    [StringValue("currency")]
    Currency,
    [StringValue("exponentSeparator")]
    ExponentSeparator,
    [StringValue("exponentInteger")]
    ExponentInteger
  }

  [DebuggerDisplay("{Value,nq}")]
  public readonly struct FormattedPart {
    public static readonly FormattedPart[] EmptyArray = new FormattedPart[0];

    public FormattedPart(FormattedPartType type, string value) {
      this.Type = type;
      this.Value = value;
    }

    public FormattedPartType Type { get; }

    public string Value { get; }

    public EcmaValue ToValue() {
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Type, IntlProviderOptions.ToStringValue(this.Type));
      obj.CreateDataPropertyOrThrow(PropertyKey.Value, this.Value);
      return obj;
    }
  }
}
