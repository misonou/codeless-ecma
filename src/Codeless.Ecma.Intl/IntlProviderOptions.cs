using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public abstract class IntlProviderOptions {
    protected static readonly bool? BooleanNull;

    private readonly Dictionary<EcmaPropertyKey, object> parsedOptions = new Dictionary<EcmaPropertyKey, object>();
    private readonly RuntimeObject options;
    private readonly bool readOnly;

    public IntlProviderOptions() { }

    public IntlProviderOptions(EcmaValue value) {
      this.readOnly = true;
      this.options = value == default ? RuntimeObject.Create(null) : value.ToObject();
    }

    public LocaleMatcher LocaleMatcher {
      get { return GetOption(PropertyKey.LocaleMatcher, LocaleMatcher.BestFit); }
      set { SetOption(PropertyKey.LocaleMatcher, value); }
    }

    public static T ParseEnum<T>(string stringValue) where T : struct, Enum {
      if (StringValueMap<T>.Default.Forward.TryGetValue(stringValue, out T parsedValue)) {
        return parsedValue;
      }
      throw new EcmaRangeErrorException(String.Format("Invalid value: {0}", stringValue));
    }

    public static string ToStringValue<T>(T enumValue) where T : struct, Enum {
      if (StringValueMap<T>.Default.Reverse.TryGetValue(enumValue, out string stringValue)) {
        return stringValue;
      }
      return null;
    }

    protected NumberFormatDigitOptions GetNumberFormatDigitOptions(int minFractionDigits, int maxFractionDigits, NumberNotation notation) {
      NumberFormatDigitOptions info = new NumberFormatDigitOptions();
      info.MinimumIntegerDigits = GetOption(PropertyKey.MinimumIntegerDigits, 1, 21, 1);
      if (readOnly) {
        EcmaValue mnsd = options[PropertyKey.MinimumSignificantDigits];
        EcmaValue mxsd = options[PropertyKey.MaximumSignificantDigits];
        EcmaValue mnfd = options[PropertyKey.MinimumFractionDigits];
        EcmaValue mxfd = options[PropertyKey.MaximumFractionDigits];
        if (mnsd != default || mxsd != default) {
          info.MinimumSignificantDigits = ParseNumberOption(PropertyKey.MinimumSignificantDigits, mnsd, 1, 21, 1);
          info.MaximumSignificantDigits = ParseNumberOption(PropertyKey.MaximumSignificantDigits, mxsd, info.MinimumSignificantDigits, 21, 21);
          info.UseSignificantDigits = true;
          info.RoundingType = RoundingType.SignificantDigits;
        } else {
          info.RoundingType = notation == NumberNotation.Compact ? RoundingType.CompactRounding : RoundingType.FractionDigits;
          info.MinimumFractionDigits = ParseNumberOption(PropertyKey.MinimumFractionDigits, mnfd, 0, 20, minFractionDigits);
          info.MaximumFractionDigits = ParseNumberOption(PropertyKey.MaximumFractionDigits, mxfd, info.MinimumFractionDigits, 20, Math.Max(info.MinimumFractionDigits, maxFractionDigits));
        }
      } else {
        object mnsd, mxsd, mnfd, mxfd;
        parsedOptions.TryGetValue(PropertyKey.MinimumSignificantDigits, out mnsd);
        parsedOptions.TryGetValue(PropertyKey.MaximumSignificantDigits, out mxsd);
        parsedOptions.TryGetValue(PropertyKey.MinimumFractionDigits, out mnfd);
        parsedOptions.TryGetValue(PropertyKey.MaximumFractionDigits, out mxfd);
        if (mnsd != null || mxsd != null) {
          info.MinimumSignificantDigits = ParseNumberOption(PropertyKey.MinimumSignificantDigits, new EcmaValue(mnsd), 1, 21, 1);
          info.MaximumSignificantDigits = ParseNumberOption(PropertyKey.MaximumSignificantDigits, new EcmaValue(mxsd), info.MinimumSignificantDigits, 21, 21);
          info.UseSignificantDigits = true;
        } else {
          info.RoundingType = notation == NumberNotation.Compact ? RoundingType.CompactRounding : RoundingType.FractionDigits;
          info.MinimumFractionDigits = ParseNumberOption(PropertyKey.MinimumFractionDigits, new EcmaValue(mnfd), 0, 20, minFractionDigits);
          info.MaximumFractionDigits = ParseNumberOption(PropertyKey.MaximumFractionDigits, new EcmaValue(mxfd), info.MinimumFractionDigits, 20, Math.Max(info.MinimumFractionDigits, maxFractionDigits));
        }
      }
      return info;
    }

    protected T GetOption<T>(EcmaPropertyKey propertyKey, T defaultValue) where T : struct, Enum {
      if (parsedOptions.ContainsKey(propertyKey) && parsedOptions[propertyKey] is T savedValue) {
        return savedValue;
      }
      if (options == null) {
        return defaultValue;
      }
      EcmaValue value = options[propertyKey];
      if (value == default) {
        parsedOptions[propertyKey] = defaultValue;
        return defaultValue;
      }
      T parsedValue = ParseEnum<T>(value.ToStringOrThrow());
      parsedOptions[propertyKey] = parsedValue;
      return parsedValue;
    }

    protected bool? GetOption(EcmaPropertyKey propertyKey, bool? defaultValue) {
      if (parsedOptions.ContainsKey(propertyKey)) {
        return parsedOptions[propertyKey] is bool b ? b : default(bool?);
      }
      if (options == null) {
        return defaultValue;
      }
      EcmaValue value = options[propertyKey];
      bool? parsedValue = value == default ? defaultValue : value.ToBoolean();
      parsedOptions[propertyKey] = parsedValue;
      return parsedValue;
    }

    protected bool GetOption(EcmaPropertyKey propertyKey, bool defaultValue) {
      if (parsedOptions.ContainsKey(propertyKey) && parsedOptions[propertyKey] is bool savedValue) {
        return savedValue;
      }
      if (options == null) {
        return defaultValue;
      }
      EcmaValue value = options[propertyKey];
      bool parsedValue = value == default ? defaultValue : value.ToBoolean();
      parsedOptions[propertyKey] = parsedValue;
      return parsedValue;
    }

    protected string GetOption(EcmaPropertyKey propertyKey, string defaultValue) {
      if (parsedOptions.ContainsKey(propertyKey)) {
        return parsedOptions[propertyKey] as string;
      }
      if (options == null) {
        return defaultValue;
      }
      EcmaValue value = options[propertyKey];
      string parsedValue = value != default ? options[propertyKey].ToStringOrThrow() : defaultValue;
      parsedOptions[propertyKey] = parsedValue;
      return parsedValue;
    }

    protected int GetOption(EcmaPropertyKey propertyKey, int minimum, int maximum, int defaultValue) {
      if (parsedOptions.ContainsKey(propertyKey) && parsedOptions[propertyKey] is int savedValue) {
        return savedValue;
      }
      if (options == null) {
        return defaultValue;
      }
      int parsedValue = ParseNumberOption(propertyKey, options[propertyKey], minimum, maximum, defaultValue);
      parsedOptions[propertyKey] = parsedValue;
      return parsedValue;
    }

    protected void SetOption<T>(EcmaPropertyKey propertyKey, T value) where T : struct, Enum {
      ThrowIfReadOnly();
      parsedOptions[propertyKey] = value;
    }

    protected void SetOption(EcmaPropertyKey propertyKey, bool value) {
      ThrowIfReadOnly();
      parsedOptions[propertyKey] = value;
    }

    protected void SetOption(EcmaPropertyKey propertyKey, bool? value) {
      ThrowIfReadOnly();
      parsedOptions[propertyKey] = value;
    }

    protected void SetOption(EcmaPropertyKey propertyKey, string value) {
      ThrowIfReadOnly();
      parsedOptions[propertyKey] = value;
    }

    protected void SetOption(EcmaPropertyKey propertyKey, int value) {
      ThrowIfReadOnly();
      parsedOptions[propertyKey] = value;
    }

    private void ThrowIfReadOnly() {
      if (readOnly) {
        throw new InvalidOperationException("Options cannot be modified on a read-only instance");
      }
    }

    private static int ParseNumberOption(EcmaPropertyKey propertyKey, EcmaValue value, int minimum, int maximum, int defaultValue) {
      if (value == default) {
        return defaultValue;
      }
      value = value.ToNumber();
      if (value.IsNaN || value > maximum || value < minimum) {
        throw new EcmaRangeErrorException("{0} value is out of range", propertyKey);
      }
      return value.ToInt32();
    }
  }
}
