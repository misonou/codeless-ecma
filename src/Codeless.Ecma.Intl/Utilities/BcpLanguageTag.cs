using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  [Flags]
  public enum BcpLanguageTagOptions {
    AllowLegacy = 1,
    IgnorePrivateExtensions = 2
  }

  public class BcpLanguageTag : IEquatable<BcpLanguageTag> {
    private static readonly Regex bcp47re = new Regex(@"^
      (?<unicode_language_id>
        root|(?:
          (?:(?<unicode_language_subtag>[a-z]{2,3}|[a-z]{5,8})(?:[-_](?<unicode_script_subtag>[a-z]{4}))?|(?<unicode_script_subtag>[a-z]{4}))
          (?:[-_](?<unicode_region_subtag>[a-z]{2}|\d{3}))?
          (?:[-_](?<unicode_variant_subtag>[a-z0-9]{5,8}|\d[a-z0-9]{3}|(?<legacy>min[-_]nan|be[-_]fr|be[-_]nl|ch[-_]de)))*
        )
      )
      (?<extensions>
        (?<unicode_locale_extensions>[-_][u]
          (?:
            (?:[-_](?<ufield>[a-z0-9][a-z](?:[-_](?<uvalue>[a-z0-9]{3,8}(?:[-_][a-z0-9]{3,8})*))?))+|
            (?:[-_](?<uattr>[a-z0-9]{3,8})+(?:[-_](?<ufield>[a-z0-9][a-z](?:[-_](?<uvalue>[a-z0-9]{3,8}(?:[-_][a-z0-9]{3,8})*))?)))*
          )
        )|
        (?<transformed_extensions>[-_][t]
          (?:
            (?:[-_](?<tlang>[a-z]{2,3}|[a-z]{5,8})(?:[-_][a-z]{4})?(?:[-_]([a-z]{2}|\d{3}))?([-_](?:[a-z0-9]{5,8}|\d[a-z0-9]{3}))*)(?:[-_](?<tfield>[a-z]\d(?:[-_][a-z0-9]{3,8})+))*|
            (?:[-_](?<tfield>[a-z]\d(?:[-_][a-z0-9]{3,8})+))+
          )
        )|
        (?<other_extensions>[-_][a-svwyz0-9](?:[-_](?<ofield>[a-z0-9]{2,8}))+)
      )*
      (?<pu_extensions>
        [-_][x](?:[-_](?<pfield>[a-z0-9]{1,8}))+
      )?
      $", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly ConcurrentDictionary<string, BcpLanguageTag> canonicalCache = new ConcurrentDictionary<string, BcpLanguageTag>();
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> aliasLookup = new ConcurrentDictionary<string, Dictionary<string, string>>();
    private static Dictionary<string, BcpLanguageTag> likelySubtags;

    private BcpLanguageTag canonical;
    private BcpLanguageTag maximal;
    private BcpLanguageTag minimal;
    private string stringValue;

    internal BcpLanguageTag(BcpLanguageTagBuilder builder) {
      Guard.ArgumentNotNull(builder, "builder");
      PopulateFromBuilder(builder);
    }

    private BcpLanguageTag() { }

    private BcpLanguageTag(string locale, BcpLanguageTagOptions options)
      : this(locale, options, bcp47re.Match(locale)) { }

    private BcpLanguageTag(string locale, BcpLanguageTagOptions options, Match match) {
      Guard.ArgumentNotNull(match, "match");
      if (!Validate(match, options)) {
        throw new FormatException(String.Format("'{0}' is not a value BCP-47 language tag", locale));
      }
      PopulateFromMatchResult(match, options);
    }

    public string Language { get; private set; }
    public string Script { get; private set; }
    public string Region { get; private set; }
    public string TLang { get; private set; }
    public IReadOnlyList<string> Variants { get; private set; }
    public IReadOnlyList<string> Attributes { get; private set; }
    public IReadOnlyDictionary<string, string> UExtensions { get; private set; }
    public IReadOnlyDictionary<string, string> TExtensions { get; private set; }
    public IReadOnlyDictionary<string, string> SingletonSubtags { get; private set; }

    public bool IsCanonical {
      get { return Canonicalize() == this; }
    }

    public bool IsMaximal {
      get { return Maximize() == this; }
    }

    public bool IsMinimal {
      get { return Minimize() == this; }
    }

    public bool IsInvariant {
      get { return this.Variants.Count == 0 || this.SingletonSubtags.Count == 0; }
    }

    public static BcpLanguageTag Parse(string locale) {
      return Parse(locale, 0);
    }

    public static BcpLanguageTag Parse(string locale, BcpLanguageTagOptions options) {
      Guard.ArgumentNotNull(locale, "locale");
      string key = locale.ToLowerInvariant().Replace('_', '-');
      if (options == 0 && canonicalCache.TryGetValue(key, out BcpLanguageTag cached)) {
        return cached;
      }
      return new BcpLanguageTag(locale, options);
    }

    public static bool TryParse(string locale, out BcpLanguageTag tag) {
      return TryParse(locale, 0, out tag);
    }

    public static bool TryParse(string locale, BcpLanguageTagOptions options, out BcpLanguageTag tag) {
      Guard.ArgumentNotNull(locale, "locale");
      string key = locale.ToLowerInvariant().Replace('_', '-');
      if (options == 0 && canonicalCache.TryGetValue(key, out BcpLanguageTag cached)) {
        tag = cached;
        return true;
      }
      Match match = bcp47re.Match(locale);
      if (!Validate(match, options)) {
        tag = null;
        return false;
      }
      tag = new BcpLanguageTag(locale, options, match);
      if (tag.IsCanonical) {
        canonicalCache.TryAdd(key, tag);
      }
      return true;
    }

    public static bool IsValid(string locale) {
      string key = locale.ToLowerInvariant().Replace('_', '-');
      return canonicalCache.ContainsKey(key) || Validate(bcp47re.Match(locale), 0);
    }

    public BcpLanguageTag Canonicalize() {
      if (canonical == null) {
        BcpLanguageTag clone = new BcpLanguageTag();
        clone.MakeCanonical(this);

        string canonicalForm = clone.ToString();
        string currentForm = this.ToString();
        canonical = canonicalForm == currentForm ? this : clone;
        canonicalCache.TryAdd(currentForm.ToLowerInvariant(), this);
        if (ReferenceEquals(this, canonical)) {
          canonicalCache.TryAdd(canonicalForm.ToLowerInvariant(), canonical);
        }
      }
      return canonical;
    }

    public BcpLanguageTag Minimize() {
      if (minimal != null) {
        return minimal;
      }
      Dictionary<string, BcpLanguageTag> likelySubtags = EnsureLikelySubtagData();
      BcpLanguageTag max = Maximize();
      BcpLanguageTag min = null;
      BcpLanguageTag likely;
      if (likelySubtags.TryGetValue(max.Language, out likely) && IsSameInvariant(likely, max)) {
        min = Parse(max.Language);
      } else if (likelySubtags.TryGetValue(String.Concat(max.Language, "-", max.Region), out likely) && IsSameInvariant(likely, max)) {
        min = Parse(String.Concat(max.Language, "-", max.Region));
      } else if (likelySubtags.TryGetValue(String.Concat(max.Language, "-", max.Script), out likely) && IsSameInvariant(likely, max)) {
        min = Parse(String.Concat(max.Language, "-", max.Script));
      }
      if (min == null) {
        minimal = max;
        minimal.minimal = minimal;
        return max;
      }
      if (IsSameInvariant(min, this)) {
        minimal = this;
        return this;
      }
      if (this.Variants.Count == 0 || this.SingletonSubtags.Count == 0) {
        minimal = min;
        minimal.minimal = minimal;
        return min;
      }
      BcpLanguageTagBuilder builder = new BcpLanguageTagBuilder(this);
      builder.Language = min.Language;
      builder.Script = min.Script;
      builder.Region = min.Region;
      minimal = builder.AsReadOnly();
      minimal.minimal = minimal;
      return minimal;
    }

    public BcpLanguageTag Maximize() {
      if (maximal != null) {
        return maximal;
      }
      Dictionary<string, BcpLanguageTag> likelySubtags = EnsureLikelySubtagData();
      BcpLanguageTag canonical = Canonicalize();
      BcpLanguageTag likely;
      if (likelySubtags.TryGetValue(String.Concat(canonical.Language, "-", canonical.Script, "-", canonical.Region), out likely) ||
          likelySubtags.TryGetValue(String.Concat(canonical.Language, "-", canonical.Region), out likely) ||
          likelySubtags.TryGetValue(String.Concat(canonical.Language, "-", canonical.Script), out likely) ||
          likelySubtags.TryGetValue(canonical.Language, out likely) ||
          likelySubtags.TryGetValue("und-" + canonical.Script, out likely)) {
      } else {
        likely = likelySubtags["und"];
      }
      if (IsSameInvariant(likely, canonical)) {
        maximal = canonical;
        maximal.maximal = maximal;
        return canonical;
      }
      BcpLanguageTagBuilder builder = new BcpLanguageTagBuilder(canonical);
      if (builder.Language == "" || builder.Language == "und") {
        builder.Language = likely.Language;
      }
      if (builder.Script == "") {
        builder.Script = likely.Script;
      }
      if (builder.Region == "") {
        builder.Region = likely.Region;
      }
      maximal = builder.AsReadOnly();
      maximal.maximal = maximal;
      return maximal;
    }

    public override string ToString() {
      if (stringValue == null) {
        StringBuilder sb = new StringBuilder();
        WriteBaseName(sb);
        WriteExtensions(sb);
        stringValue = sb.ToString();
      }
      return stringValue;
    }

    public bool Equals(BcpLanguageTag other) {
      return other != null && ToString() == other.ToString();
    }

    public override bool Equals(object obj) {
      return obj is BcpLanguageTag other && Equals(other);
    }

    public override int GetHashCode() {
      return ToString().GetHashCode();
    }

    public static string GetCanonicalExtensionValue(string key, string value) {
      Guard.ArgumentNotNull(key, "key");
      Guard.ArgumentNotNull(value, "value");
      Dictionary<string, string> cached = aliasLookup.GetOrAdd(key.ToLowerInvariant(), k => {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (k == "sd" || k == "rg") {
          XDocument supplementalData = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
          foreach (XElement elm in supplementalData.XPathSelectElements("/supplementalData/metadata/alias/subdivisionAlias")) {
            string from = elm.Attribute("type").Value.ToLowerInvariant().Replace('_', '-');
            string to = elm.Attribute("replacement").Value.Replace('_', '-');
            dict[from] = to;
          }
        } else {
          XDocument bcp47 = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.bcp47.xml.gz");
          foreach (XElement elm in bcp47.XPathSelectElements(String.Format("/ldmlBCP47/keyword/key[@name = '{0}']/type[@alias or @deprecated]", k))) {
            if (elm.Attribute("deprecated") != null) {
              dict[elm.Attribute("name").Value.ToLowerInvariant()] = elm.Attribute("preferred").Value;
            } else {
              dict[elm.Attribute("alias").Value.ToLowerInvariant()] = elm.Attribute("name").Value;
            }
          }
        }
        return dict;
      });
      return cached.TryGetValue(value.ToLowerInvariant(), out string canonical) ? canonical : value;
    }

    public static string GetCanonicalLanguageName(string value) {
      Guard.ArgumentNotNull(value, "value");
      Dictionary<string, string> cached = aliasLookup.GetOrAdd("language", _ => {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        XDocument supplementalData = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalMetadata.xml.gz");
        foreach (XElement elm in supplementalData.XPathSelectElements("/supplementalData/metadata/alias/languageAlias")) {
          string from = elm.Attribute("type").Value.ToLowerInvariant();
          string to = elm.Attribute("replacement").Value;
          dict[from] = to;
        }
        return dict;
      });
      return cached.TryGetValue(value.ToLowerInvariant(), out string canonical) ? canonical : value.ToLowerInvariant();
    }

    public static string GetCanonicalScriptName(string value) {
      Guard.ArgumentNotNull(value, "value");
      Dictionary<string, string> cached = aliasLookup.GetOrAdd("script", _ => {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        XDocument supplementalData = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalMetadata.xml.gz");
        foreach (XElement elm in supplementalData.XPathSelectElements("/supplementalData/metadata/alias/scriptAlias")) {
          string from = elm.Attribute("type").Value.ToLowerInvariant();
          string to = elm.Attribute("replacement").Value;
          dict[from] = to;
        }
        return dict;
      });
      return cached.TryGetValue(value.ToLowerInvariant(), out string canonical) ? canonical : value.Substring(0, 1).ToUpperInvariant() + value.Substring(1).ToLowerInvariant();
    }

    public static string GetCanonicalRegionName(string value, string lang) {
      Guard.ArgumentNotNull(value, "value");
      Guard.ArgumentNotNull(lang, "lang");
      Dictionary<string, string> cached = aliasLookup.GetOrAdd("territory", _ => {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        XDocument supplementalData = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalMetadata.xml.gz");
        foreach (XElement elm in supplementalData.XPathSelectElements("/supplementalData/metadata/alias/territoryAlias")) {
          string from = elm.Attribute("type").Value.ToLowerInvariant();
          string to = elm.Attribute("replacement").Value;
          dict[from] = to;
        }
        return dict;
      });
      if (cached.TryGetValue(value.ToLowerInvariant(), out string canonical)) {
        if (canonical.IndexOf(' ') <= 0) {
          return canonical;
        }
        Dictionary<string, BcpLanguageTag> likelySubtags = EnsureLikelySubtagData();
        string[] candidates = canonical.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string candidate in candidates) {
          if (likelySubtags.TryGetValue(String.Concat("und-", candidate), out BcpLanguageTag tag) && tag.Language == lang) {
            return candidate;
          }
        }
        return candidates[0];
      }
      return value.ToUpperInvariant();
    }

    public static string GetCanonicalVariantName(string value) {
      Guard.ArgumentNotNull(value, "value");
      Dictionary<string, string> cached = aliasLookup.GetOrAdd("variant", _ => {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        XDocument supplementalData = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalMetadata.xml.gz");
        foreach (XElement elm in supplementalData.XPathSelectElements("/supplementalData/metadata/alias/variantAlias")) {
          string from = elm.Attribute("type").Value.ToLowerInvariant().Replace('_', '-');
          string to = elm.Attribute("replacement").Value.Replace('_', '-');
          dict[from] = to;
        }
        return dict;
      });
      return cached.TryGetValue(value.ToLowerInvariant(), out string canonical) ? canonical : value.ToLowerInvariant();
    }

    public static bool operator ==(BcpLanguageTag x, BcpLanguageTag y) {
      bool isXNull = ReferenceEquals(x, null);
      bool isYNull = ReferenceEquals(y, null);
      if (isXNull || isYNull) {
        return isXNull && isYNull;
      }
      return x.ToString() == y.ToString();
    }

    public static bool operator !=(BcpLanguageTag x, BcpLanguageTag y) {
      return !(x == y);
    }

    private void PopulateFromMatchResult(Match match, BcpLanguageTagOptions options) {
      HashSet<string> variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      HashSet<string> attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> ukeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> tkeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> singletons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      Group languageId = match.Groups["unicode_language_id"];
      if (languageId.Value == "root") {
        this.Language = "root";
        this.Script = "";
        this.Region = "";
      } else {
        Group languageSubtag = match.Groups["unicode_language_subtag"];
        Group scriptSubtag = match.Groups["unicode_script_subtag"];
        Group regionSubtag = match.Groups["unicode_region_subtag"];
        Group variantSubtag = match.Groups["unicode_variant_subtag"];
        this.Language = languageSubtag.Success ? languageSubtag.Value.ToLowerInvariant() : "";
        this.Script = scriptSubtag.Success ? ToTitleCase(scriptSubtag.Value) : "";
        this.Region = regionSubtag.Success ? regionSubtag.Value.ToUpperInvariant() : "";
        if (variantSubtag.Success) {
          foreach (Capture item in variantSubtag.Captures) {
            variants.Add(item.Value.ToLowerInvariant());
          }
        }
      }
      Group tExtensions = match.Groups["transformed_extensions"];
      Group uExtensions = match.Groups["unicode_locale_extensions"];
      if (tExtensions.Success) {
        Group tlang = match.Groups["tlang"];
        if (tlang.Success) {
          this.TLang = tlang.Value;
        }
        foreach (Capture field in match.Groups["tfield"].Captures) {
          string key = field.Value.Substring(0, 2).ToLowerInvariant();
          if (!tkeys.ContainsKey(key)) {
            tkeys.Add(key, ToLowerCaseHyphenated(field.Value.Substring(3)));
          }
        }
      }
      if (this.TLang == null) {
        this.TLang = "";
      }
      if (uExtensions.Success) {
        Group uattr = match.Groups["uattr"];
        if (uattr.Success) {
          foreach (Capture item in uattr.Captures) {
            attributes.Add(item.Value.ToLowerInvariant());
          }
        }
        foreach (Capture field in match.Groups["ufield"].Captures) {
          string key = field.Value.Substring(0, 2).ToLowerInvariant();
          if (!ukeys.ContainsKey(key)) {
            ukeys.Add(key, ToLowerCaseHyphenated(field.Value.Substring(3)));
          }
        }
      }
      foreach (Capture ext in match.Groups["extensions"].Captures) {
        singletons.Add(ext.Value.Substring(1, 1).ToLowerInvariant(), ToLowerCaseHyphenated(ext.Value.Substring(3)));
      }
      if ((options & BcpLanguageTagOptions.IgnorePrivateExtensions) == 0) {
        Group privateExtensions = match.Groups["pu_extensions"];
        if (privateExtensions.Success) {
          singletons.Add("x", ToLowerCaseHyphenated(privateExtensions.Value.Substring(3)));
        }
      }
      this.Variants = new BcpSubtagCollection(variants, true);
      this.Attributes = new BcpSubtagCollection(attributes, true);
      this.UExtensions = new BcpSubtagDictionary(ukeys, true);
      this.TExtensions = new BcpSubtagDictionary(tkeys, true);
      this.SingletonSubtags = new BcpSubtagDictionary(singletons, true);
    }

    private void PopulateFromBuilder(BcpLanguageTagBuilder builder) {
      HashSet<string> variants = new HashSet<string>(builder.Variants.Select(v => v.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
      HashSet<string> attributes = new HashSet<string>(builder.Attributes.Select(v => v.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> uExtensions = new Dictionary<string, string>(builder.UExtensions.ToDictionary(v => v.Key.ToLowerInvariant(), v => ToLowerCaseHyphenated(v.Value)), StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> tExtensions = new Dictionary<string, string>(builder.TExtensions.ToDictionary(v => v.Key.ToLowerInvariant(), v => ToLowerCaseHyphenated(v.Value)), StringComparer.OrdinalIgnoreCase);
      Dictionary<string, string> oExtensions = new Dictionary<string, string>(builder.SingletonSubtags.Where(v => !String.IsNullOrEmpty(v.Value)).ToDictionary(v => v.Key.ToLowerInvariant(), v => ToLowerCaseHyphenated(v.Value)), StringComparer.OrdinalIgnoreCase);

      this.Language = builder.Language.ToLowerInvariant();
      this.Script = ToTitleCase(builder.Script);
      this.Region = builder.Region.ToUpperInvariant();
      this.TLang = builder.TLang.ToLowerInvariant();
      this.Variants = new BcpSubtagCollection(variants, true);
      this.Attributes = new BcpSubtagCollection(attributes, true);
      this.UExtensions = new BcpSubtagDictionary(uExtensions, true);
      this.TExtensions = new BcpSubtagDictionary(tExtensions, true);
      this.SingletonSubtags = new BcpSubtagDictionary(oExtensions, true);
    }

    private void MakeCanonical(BcpLanguageTag source) {
      SortedSet<string> variants = new SortedSet<string>(source.Variants, StringComparer.OrdinalIgnoreCase);
      SortedSet<string> attributes = new SortedSet<string>(source.Attributes, StringComparer.OrdinalIgnoreCase);
      SortedDictionary<string, string> uExtensions = new SortedDictionary<string, string>((IDictionary<string, string>)source.UExtensions, StringComparer.OrdinalIgnoreCase);
      SortedDictionary<string, string> tExtensions = new SortedDictionary<string, string>((IDictionary<string, string>)source.TExtensions, StringComparer.OrdinalIgnoreCase);
      SortedDictionary<string, string> otherExtensions = new SortedDictionary<string, string>((IDictionary<string, string>)source.SingletonSubtags, StringComparer.OrdinalIgnoreCase);

      this.Language = source.Language;
      this.Script = source.Script;
      this.Region = source.Region;
      this.TLang = source.TLang;
      if (!String.IsNullOrEmpty(this.Language)) {
        string replacement;
        bool removeVariant = false;
        bool removeScript = false;
        if (!TryGetCanonicalLanguageName(this.Language, out replacement)) {
          // legacy alias that switch from ll-Ssss-RR to ll-RR or vice versa
          if (!String.IsNullOrEmpty(this.Region) && !TryGetCanonicalLanguageName(String.Concat(this.Language, "-", this.Region), out replacement)) {
            if (!String.IsNullOrEmpty(this.Script) && TryGetCanonicalLanguageName(String.Concat(this.Language, "-", this.Region, "-", this.Script), out replacement)) {
              removeScript = true;
            }
          }
          // deprecated variants like no_BOKMAL, zh_min_nan, or sgn_BE_FR
          // which the latter part is collectively parsed as special cases of variant
          if (variants.Count > 0) {
            removeVariant = TryGetCanonicalLanguageName(String.Concat(this.Language, "-", variants.First()), out replacement);
          }
        }
        if (replacement != null) {
          if (removeScript) {
            this.Script = "";
          }
          if (removeVariant) {
            variants.Clear();
          }
          if (replacement.IndexOf('-') >= 0) {
            // if there are additional subtags in the replacement value, add them to the result
            // but only if there is no corresponding subtag already in the tag
            BcpLanguageTag tag = Parse(replacement);
            this.Language = tag.Language;
            if (!String.IsNullOrEmpty(tag.Script) && String.IsNullOrEmpty(this.Script)) {
              this.Script = tag.Script;
            }
            if (!String.IsNullOrEmpty(tag.Region) && String.IsNullOrEmpty(this.Region)) {
              this.Region = tag.Region;
            }
          } else {
            this.Language = replacement;
          }
        }
      }
      if (!String.IsNullOrEmpty(this.Script)) {
        this.Script = GetCanonicalScriptName(this.Script);
      }
      foreach (string variant in variants.ToArray()) {
        // the replacement may not be a variant subtag
        // in that case, the variant subtag is removed, and the other tag is substituted
        // for example, hy-FR-arevmda ⇒ hyw-FR
        string replacement = GetCanonicalVariantName(variant.ToLowerInvariant());
        if (replacement != variant) {
          variants.Remove(variant);
          if (replacement.Length < 4) {
            this.Language = replacement;
          } else {
            variants.Add(replacement);
          }
        }
      }
      if (!String.IsNullOrEmpty(this.Region)) {
        this.Region = GetCanonicalRegionName(this.Region, this.Language);
      }
      if (String.IsNullOrEmpty(this.Language)) {
        this.Language = "und";
      }
      if (!String.IsNullOrEmpty(this.TLang)) {
        this.TLang = Parse(this.TLang).Canonicalize().ToString();
      }
      foreach (KeyValuePair<string, string> e in tExtensions.ToArray()) {
        tExtensions[e.Key] = GetCanonicalExtensionValue(e.Key, ToLowerCaseHyphenated(e.Value));
      }
      foreach (KeyValuePair<string, string> e in uExtensions.ToArray()) {
        uExtensions[e.Key] = GetCanonicalExtensionValue(e.Key, ToLowerCaseHyphenated(e.Value));
      }
      foreach (KeyValuePair<string, string> e in otherExtensions.ToArray()) {
        otherExtensions[e.Key] = ToLowerCaseHyphenated(e.Value);
      }

      StringBuilder sb = new StringBuilder();
      bool hasTLang = !String.IsNullOrEmpty(this.TLang);
      if (hasTLang || tExtensions.Count > 0) {
        if (hasTLang) {
          sb.Append('-');
          sb.Append(this.TLang);
        }
        WriteDictionary(sb, tExtensions, true);
        otherExtensions["t"] = sb.ToString(1, sb.Length - 1);
      }
      if (attributes.Count > 0 || uExtensions.Count > 0) {
        sb.Remove(0, sb.Length);
        WriteCollection(sb, attributes);
        WriteDictionary(sb, uExtensions, true);
        otherExtensions["u"] = sb.ToString(1, sb.Length - 1);
      }
      this.Variants = new BcpSubtagCollection(variants, true);
      this.Attributes = new BcpSubtagCollection(attributes, true);
      this.UExtensions = new BcpSubtagDictionary(uExtensions, true);
      this.TExtensions = new BcpSubtagDictionary(tExtensions, true);
      this.SingletonSubtags = new BcpSubtagDictionary(otherExtensions, true);
      this.canonical = this;
    }

    private void WriteBaseName(StringBuilder sb) {
      if (!String.IsNullOrEmpty(this.Language)) {
        sb.Append(this.Language);
      }
      if (!String.IsNullOrEmpty(this.Script)) {
        if (sb.Length > 0) {
          sb.Append('-');
        }
        sb.Append(this.Script);
      }
      if (!String.IsNullOrEmpty(this.Region)) {
        sb.Append('-');
        sb.Append(this.Region);
      }
      if (this.Variants.Count > 0) {
        WriteCollection(sb, this.Variants);
      }
      if (sb.Length == 0) {
        sb.Append("und");
      }
    }

    private void WriteExtensions(StringBuilder sb) {
      string privateExtensions = null;
      foreach (KeyValuePair<string, string> e in this.SingletonSubtags) {
        if (e.Key[0] == 'x') {
          privateExtensions = e.Value;
          continue;
        }
        sb.Append('-');
        sb.Append(e.Key);
        sb.Append('-');
        sb.Append(e.Value);
      }
      if (privateExtensions != null) {
        sb.Append("-x-");
        sb.Append(privateExtensions);
      }
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

    private static bool Validate(Match match, BcpLanguageTagOptions options) {
      if (!match.Success) {
        return false;
      }
      if ((options & BcpLanguageTagOptions.AllowLegacy) == 0 && match.Groups["legacy"].Success) {
        return false;
      }
      if (match.Groups["transformed_extensions"].Captures.Count > 1 ||
          match.Groups["unicode_locale_extensions"].Captures.Count > 1) {
        return false;
      }
      HashSet<string> visitedKeys = new HashSet<string>();
      foreach (Capture capture in match.Groups["other_extensions"].Captures) {
        string key = capture.Value.Substring(1, 1).ToLowerInvariant();
        if (!visitedKeys.Add(key)) {
          return false;
        }
      }
      return true;
    }

    private static bool TryGetCanonicalLanguageName(string input, out string canonical) {
      string value = GetCanonicalLanguageName(input);
      if (!value.Equals(input, StringComparison.OrdinalIgnoreCase)) {
        canonical = value;
        return true;
      }
      canonical = null;
      return false;
    }

    private static string ToLowerCaseHyphenated(string value) {
      return value.Length == 0 ? "" : value.ToLowerInvariant().Replace('_', '-');
    }

    private static string ToTitleCase(string value) {
      return value.Length == 0 ? "" : value.Substring(0, 1).ToUpperInvariant() + value.Substring(1).ToLowerInvariant();
    }

    private static bool IsSameInvariant(BcpLanguageTag likely, BcpLanguageTag canonical) {
      return likely.Language == canonical.Language && likely.Script == canonical.Script && likely.Region == canonical.Region;
    }

    private static Dictionary<string, BcpLanguageTag> EnsureLikelySubtagData() {
      if (likelySubtags == null) {
        Dictionary<string, BcpLanguageTag> map = new Dictionary<string, BcpLanguageTag>();
        XDocument xDocument = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.likelySubtags.xml.gz");
        foreach (XElement elm in xDocument.XPathSelectElements("/supplementalData/likelySubtags/likelySubtag")) {
          map[elm.Attribute("from").Value] = Parse(elm.Attribute("to").Value);
        }
        likelySubtags = map;
      }
      return likelySubtags;
    }
  }
}
