using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Codeless.Ecma.Intl {
  public enum LocaleMatcher {
    [StringValue("lookup")]
    Lookup,
    [StringValue("best fit")]
    BestFit
  }

  public abstract class IntlProvider<T> : RuntimeObject where T : IntlProviderOptions, new() {
    private bool inited;

    public IntlProvider(SharedObjectHandle proto)
      : base(proto) { }

    public IntlProvider(SharedObjectHandle proto, string locale)
      : base(proto) {
      Init(IntlUtility.CanonicalizeLocaleList(new[] { locale }), new T());
    }

    public IntlProvider(SharedObjectHandle proto, string locale, T options)
      : base(proto) {
      Init(IntlUtility.CanonicalizeLocaleList(new[] { locale }), options);
    }

    public IntlProvider(SharedObjectHandle proto, ICollection<string> locale)
      : base(proto) {
      Init(IntlUtility.CanonicalizeLocaleList(locale), new T());
    }

    public IntlProvider(SharedObjectHandle proto, ICollection<string> locale, T options)
      : base(proto) {
      Init(IntlUtility.CanonicalizeLocaleList(locale), options);
    }

    protected abstract ICollection<string> AvailableLocales { get; }

    internal void Init(ICollection<string> locales, T options) {
      Guard.ArgumentNotNull(locales, "locales");
      Guard.ArgumentNotNull(options, "options");
      InitInternal(locales, options);
      inited = true;
    }

    protected abstract void InitInternal(ICollection<string> locales, T options);

    protected void EnsureInitialized() {
      if (!inited) {
        Init(new string[0], new T());
      }
    }

    protected string ResolveLocale(ICollection<string> locales, LocaleMatcher matcher) {
      return ResolveLocale(locales, matcher, null, null, out _);
    }

    protected string ResolveLocale(ICollection<string> locales, LocaleMatcher matcher, IList<string> relevantExtensionKeys, Hashtable options) {
      return ResolveLocale(locales, matcher, relevantExtensionKeys, options, out _);
    }

    protected string ResolveLocale(ICollection<string> locales, LocaleMatcher matcher, IList<string> relevantExtensionKeys, Hashtable options, out Hashtable properties) {
      Guard.ArgumentNotNull(locales, "locales");
      ICollection<string> availableLocales = this.AvailableLocales;
      if (availableLocales == null) {
        throw new InvalidOperationException("IntlProvider.AvailableLocales must not return null");
      }
      string extension;
      string matched = IntlUtility.GetBestAvailableLocale(availableLocales, locales, matcher, out extension);
      string supportedExtension = "-u";
      properties = new Hashtable();
      if (relevantExtensionKeys != null) {
        foreach (string key in relevantExtensionKeys) {
          IList<string> supportedValue = IntlUtility.SupportedValues[key];
          string value = IntlUtility.GetDefaultExtensionValue(key);
          string supportedExtensionAddition = "";
          if (extension != "") {
            string requestedValue = IntlUtility.GetUnicodeExtensionValue(extension, key);
            if (requestedValue != null && supportedValue.Contains(requestedValue)) {
              value = requestedValue;
              if (value != "") {
                supportedExtensionAddition += "-" + key + "-" + value;
              } else {
                supportedExtensionAddition += "-" + key;
              }
            }
          }
          if (options[key] != null) {
            string optionValue = options[key].ToString();
            if (supportedValue.Contains(optionValue) && optionValue != value) {
              value = optionValue;
              supportedExtensionAddition = "";
            }
          }
          properties[key] = value;
          supportedExtension += supportedExtensionAddition;
        }
      }
      if (supportedExtension.Length <= 2) {
        return IntlUtility.CanonicalizeLanguageTag(matched);
      }
      int pos = matched.IndexOf("-x-");
      if (pos < 0) {
        return IntlUtility.CanonicalizeLanguageTag(matched + supportedExtension);
      }
      return IntlUtility.CanonicalizeLanguageTag(matched.Substring(0, pos) + supportedExtension + matched.Substring(pos));
    }
  }
}
