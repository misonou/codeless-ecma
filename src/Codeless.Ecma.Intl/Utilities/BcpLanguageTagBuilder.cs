using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.Intl.Utilities {
  public class BcpLanguageTagBuilder {
    private const string reLang = "[a-z]{2,3}|[a-z]{5,8}";
    private const string reScript = "[a-z]{4}";
    private const string reRegion = "[a-z]{2}|\\d{3}";
    private const string reVariant = "[a-z0-9]{5,8}|\\d[a-z0-9]{3}";
    private const string reValue = "[a-z0-9]{3,8}";
    private const string reUKey = "[a-z0-9][a-z]";
    private const string reTKey = "[a-z]\\d";
    private const string reSValue = "[a-z0-9]{2,8}";
    private const string reXValue = "[a-z0-9]{1,8}";

    private string language = "";
    private string script = "";
    private string region = "";
    private string tlang = "";
    private bool suppressChange;

    public BcpLanguageTagBuilder() {
      this.Variants = new AttributeCollection(this, reVariant);
      this.Attributes = new AttributeCollection(this, reValue);
      this.UExtensions = new ExtensionDictionary(this, reUKey, reValue);
      this.TExtensions = new ExtensionDictionary(this, reTKey, reValue);
      this.SingletonSubtags = new SingletonDictionary(this);
    }

    public BcpLanguageTagBuilder(string baseName)
      : this(BcpLanguageTag.Parse(baseName), 0) { }

    public BcpLanguageTagBuilder(string baseName, BcpLanguageTagOptions options)
      : this(BcpLanguageTag.Parse(baseName, options), options) { }

    public BcpLanguageTagBuilder(BcpLanguageTag parent)
      : this(parent, 0) { }

    public BcpLanguageTagBuilder(BcpLanguageTag parent, BcpLanguageTagOptions options)
      : this() {
      Guard.ArgumentNotNull(parent, "parent");
      this.Language = parent.Language;
      this.Script = parent.Script;
      this.Region = parent.Region;
      foreach (string variant in parent.Variants) {
        this.Variants.Add(variant);
      }
      this.TLang = parent.TLang;
      foreach (KeyValuePair<string, string> e in parent.TExtensions) {
        this.TExtensions.Add(e);
      }
      foreach (string attribute in parent.Attributes) {
        this.Attributes.Add(attribute);
      }
      foreach (KeyValuePair<string, string> e in parent.UExtensions) {
        this.UExtensions.Add(e);
      }
      if ((options & BcpLanguageTagOptions.IgnorePrivateExtensions) == 0) {
        foreach (KeyValuePair<string, string> e in parent.SingletonSubtags) {
          this.SingletonSubtags.Add(e);
        }
      }
    }

    public ICollection<string> Variants { get; private set; }
    public ICollection<string> Attributes { get; private set; }
    public IDictionary<string, string> UExtensions { get; private set; }
    public IDictionary<string, string> TExtensions { get; private set; }
    public IDictionary<string, string> SingletonSubtags { get; private set; }

    public string Language {
      get {
        return language;
      }
      set {
        if (value == null || value == "") {
          language = "";
        } else {
          if (!Regex.IsMatch(value, reLang, RegexOptions.IgnoreCase)) {
            throw new FormatException(String.Format("'{0}' is not a valid language sub tag", value));
          }
          language = value;
        }
      }
    }

    public string Script {
      get {
        return script;
      }
      set {
        if (value == null || value == "") {
          script = "";
        } else {
          if (!Regex.IsMatch(value, reScript, RegexOptions.IgnoreCase)) {
            throw new FormatException(String.Format("'{0}' is not a valid script sub tag", value));
          }
          script = value;
        }
      }
    }

    public string Region {
      get {
        return region;
      }
      set {
        if (value == null || value == "") {
          region = "";
        } else {
          if (!Regex.IsMatch(value, reRegion, RegexOptions.IgnoreCase)) {
            throw new FormatException(String.Format("'{0}' is not a valid region sub tag", value));
          }
          region = value;
        }
      }
    }

    public string TLang {
      get {
        return tlang;
      }
      set {
        if (value == null || value == "") {
          tlang = "";
        } else {
          tlang = value;
        }
        SingletonSubtags["t"] = FormatTExtension();
      }
    }

    public BcpLanguageTag AsReadOnly() {
      return new BcpLanguageTag(this);
    }

    public BcpLanguageTag Canonicalize() {
      return new BcpLanguageTag(this).Canonicalize();
    }

    public override string ToString() {
      return new BcpLanguageTag(this).ToString();
    }

    private void OnChange(ICollection<string> collection) {
      if (!suppressChange) {
        try {
          suppressChange = true;
          if (collection == Attributes) {
            SingletonSubtags["u"] = FormatUExtension();
          }
        } finally {
          suppressChange = false;
        }
      }
    }

    private void OnChange(IDictionary<string, string> collection, string[] changedKeys) {
      if (!suppressChange) {
        try {
          suppressChange = true;
          if (collection == UExtensions) {
            SingletonSubtags["u"] = FormatUExtension();
          } else if (collection == TExtensions) {
            SingletonSubtags["t"] = FormatTExtension();
          } else {
            if (Array.IndexOf(changedKeys, "t") >= 0 || Array.IndexOf(changedKeys, "T") >= 0) {
              TExtensions.Clear();
              if (collection.TryGetValue("t", out string value) && !String.IsNullOrEmpty(value)) {
                BcpLanguageTag t = BcpLanguageTag.Parse("root-t-" + value);
                foreach (KeyValuePair<string, string> a in t.TExtensions) {
                  TExtensions.Add(a);
                }
                TLang = t.TLang;
              } else {
                TLang = "";
              }
            }
            if (Array.IndexOf(changedKeys, "u") >= 0 || Array.IndexOf(changedKeys, "U") >= 0) {
              UExtensions.Clear();
              Attributes.Clear();
              if (collection.TryGetValue("u", out string value) && !String.IsNullOrEmpty(value)) {
                BcpLanguageTag t = BcpLanguageTag.Parse("root-u-" + value);
                foreach (KeyValuePair<string, string> a in t.UExtensions) {
                  UExtensions.Add(a);
                }
                foreach (string a in t.Attributes) {
                  Attributes.Add(a);
                }
              }
            }
          }
        } finally {
          suppressChange = false;
        }
      }
    }

    private string FormatTExtension() {
      bool hasTLang = !String.IsNullOrEmpty(this.TLang);
      if (hasTLang || this.TExtensions.Count > 0) {
        StringBuilder sb = new StringBuilder();
        if (hasTLang) {
          sb.Append('-');
          sb.Append(this.TLang);
        }
        WriteDictionary(sb, this.TExtensions, true);
        return sb.ToString(1, sb.Length - 1);
      }
      return "";
    }

    private string FormatUExtension() {
      if (this.Attributes.Count > 0 || this.UExtensions.Count > 0) {
        StringBuilder sb = new StringBuilder();
        WriteCollection(sb, this.Attributes);
        WriteDictionary(sb, this.UExtensions, true);
        return sb.ToString(1, sb.Length - 1);
      }
      return "";
    }

    private void WriteCollection(StringBuilder sb, IEnumerable<string> collection) {
      foreach (string value in collection) {
        sb.Append('-');
        sb.Append(value);
      }
    }

    private void WriteDictionary(StringBuilder sb, IEnumerable<KeyValuePair<string, string>> collection, bool omitTrueValue) {
      foreach (KeyValuePair<string, string> e in collection) {
        sb.Append('-');
        sb.Append(e.Key);
        if (!omitTrueValue || e.Value != "true") {
          sb.Append('-');
          sb.Append(e.Value);
        }
      }
    }

    #region Helper classes
    private class AttributeCollection : BcpSubtagCollection {
      private readonly BcpLanguageTagBuilder builder;
      private readonly string pattern;

      public AttributeCollection(BcpLanguageTagBuilder builder, string pattern) {
        this.builder = builder;
        this.pattern = pattern;
      }

      protected override void OnChange() {
        builder.OnChange(this);
      }

      protected override bool Validate(string value) {
        if (!Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase)) {
          throw new FormatException(String.Format("Invalid extension value {0}", value));
        }
        return base.Validate(value);
      }
    }

    private class ExtensionDictionary : BcpSubtagDictionary {
      private readonly BcpLanguageTagBuilder builder;
      private readonly string keyPattern;
      private readonly string valuePattern;

      public ExtensionDictionary(BcpLanguageTagBuilder builder, string keyPattern, string valuePattern) {
        this.builder = builder;
        this.keyPattern = keyPattern;
        this.valuePattern = valuePattern;
      }

      protected override void OnChange(string[] keys) {
        builder.OnChange(this, keys);
      }

      protected override bool Validate(string key, string value) {
        Guard.ArgumentNotNull(key, "key");
        Guard.ArgumentNotNull(value, "value");
        if (!Regex.IsMatch(key, keyPattern, RegexOptions.IgnoreCase)) {
          throw new FormatException(String.Format("Invalid extension key {0}", key));
        }
        if (!Regex.IsMatch(value, valuePattern, RegexOptions.IgnoreCase)) {
          throw new FormatException(String.Format("Invalid extension value {0}", value));
        }
        return true;
      }
    }

    private class SingletonDictionary : BcpSubtagDictionary {
      private readonly BcpLanguageTagBuilder builder;

      public SingletonDictionary(BcpLanguageTagBuilder builder) {
        this.builder = builder;
      }

      protected override void OnChange(string[] keys) {
        builder.OnChange(this, keys);
      }

      protected override bool Validate(string key, string value) {
        Guard.ArgumentNotNull(key, "key");
        Guard.ArgumentNotNull(value, "value");
        if (key.Length != 1 && !Char.IsLetterOrDigit(key[0])) {
          throw new FormatException("Singleton must be a single letter or digit");
        }
        if (!String.IsNullOrEmpty(value)) {
          bool result;
          switch (key[0]) {
            case 'u':
            case 'U':
              result = BcpLanguageTag.IsValid("root-u-" + value);
              break;
            case 't':
            case 'T':
              result = BcpLanguageTag.IsValid("root-t-" + value);
              break;
            case 'x':
            case 'X':
              result = Regex.IsMatch(value, reXValue, RegexOptions.IgnoreCase);
              break;
            default:
              result = Regex.IsMatch(value, reSValue, RegexOptions.IgnoreCase);
              break;
          }
          if (!result) {
            throw new FormatException(String.Format("'{0}' is not a valid sequence for {1} singleton", value, key));
          }
        }
        return true;
      }
    }
    #endregion
  }
}
