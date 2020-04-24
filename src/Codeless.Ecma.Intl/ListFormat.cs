using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl {
  public class ListFormat : IntlProvider<ListFormatOptions> {
    private static readonly XDocument xDocument = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.listPatterns.xml.gz");
    private static readonly ICollection<string> availableLocales = xDocument.XPathSelectElements("/root/listPatterns").Select(v => v.Attribute("locale").Value).ToList().AsReadOnly();
    private static readonly ConcurrentDictionary<string, FormattedString[]> resolvedPatterns = new ConcurrentDictionary<string, FormattedString[]>();

    private FormattedString[] patterns;

    public ListFormat()
      : base(IntlModule.ListFormatPrototype) { }

    public ListFormat(string locale)
      : base(IntlModule.ListFormatPrototype, locale) { }

    public ListFormat(string locale, ListFormatOptions options)
      : base(IntlModule.ListFormatPrototype, locale, options) { }

    public ListFormat(ICollection<string> locale)
      : base(IntlModule.ListFormatPrototype, locale) { }

    public ListFormat(ICollection<string> locale, ListFormatOptions options)
      : base(IntlModule.ListFormatPrototype, locale, options) { }

    public string Locale { get; private set; }
    public ListStyle Style { get; private set; }
    public ListType Type { get; private set; }
    public EcmaValue BoundFormat { get; private set; }

    protected override ICollection<string> AvailableLocales => availableLocales;

    protected override void InitInternal(ICollection<string> locales, ListFormatOptions options) {
      this.Locale = ResolveLocale(locales, options.LocaleMatcher);
      this.Type = options.Type;
      this.Style = options.Style;
      this.BoundFormat = Literal.FunctionLiteral(this.FormatInternal);
    }

    public FormattedString Format(EcmaValue list) {
      return Format(CreateStringList(list));
    }

    public FormattedString Format(IList<string> items) {
      Guard.ArgumentNotNull(items, "items");
      EnsureInitialized();
      if (items.Count == 0) {
        return FormattedString.Empty;
      }
      if (items.Count == 1) {
        return new FormattedString(new[] { new FormattedPart(FormattedPartType.Element, items[0]) });
      }
      int countMinus2 = items.Count - 2;
      if (patterns == null) {
        patterns = ResolvePatterns(this.Locale, ToLdmlListPatternType(this.Type, this.Style));
      }
      if (countMinus2 == 0) {
        FormattedPart[] parts = patterns[3].GetParts();
        for (int i = 0, len = parts.Length; i < len; i++) {
          if (parts[i].Type == FormattedPartType.Placeholder) {
            parts[i] = new FormattedPart(FormattedPartType.Element, parts[i].Value == "{0}" ? items[0] : items[1]);
          }
        }
        return new FormattedString(parts);
      }
      List<FormattedPart> list = new List<FormattedPart> { new FormattedPart(FormattedPartType.Element, items[countMinus2 + 1]) };
      for (int j = countMinus2; j >= 0; j--) {
        FormattedPart[] parts = patterns[j == 0 ? 0 : j < countMinus2 ? 1 : 2].GetParts();
        int sliceIndex = 0;
        for (int i = 0, len = parts.Length; i < len; i++) {
          if (parts[i].Type == FormattedPartType.Placeholder) {
            if (parts[i].Value == "{0}") {
              parts[i] = new FormattedPart(FormattedPartType.Element, items[j]);
            } else {
              sliceIndex = i;
            }
          }
        }
        list.InsertRange(0, parts.Take(sliceIndex));
        list.AddRange(parts.Skip(sliceIndex + 1));
      }
      return new FormattedString(list);
    }

    [IntrinsicMember]
    private EcmaValue FormatInternal(EcmaValue list) {
      List<string> items = CreateStringList(list);
      if (items.Count == 0) {
        return "";
      }
      if (items.Count == 1) {
        return items[0];
      }
      return Format(items).ToString();
    }

    private static List<string> CreateStringList(EcmaValue list) {
      List<string> items = new List<string>();
      if (list != default) {
        foreach (EcmaValue value in list.ForOf()) {
          if (value.Type != EcmaValueType.String) {
            throw new EcmaTypeErrorException("Iterable yielded {0} which is not a string", value);
          }
          items.Add(value.ToString());
        }
      }
      return items;
    }

    private static FormattedString[] ResolvePatterns(string locale, string type) {
      string key = locale + "/" + type;
      if (resolvedPatterns.TryGetValue(key, out FormattedString[] cached)) {
        return cached;
      }
      XElement listPattern = xDocument.XPathSelectElement("/root/listPatterns[@locale = '" + locale + "']/listPattern[" + (type == "standard" ? "not(@type)" : "@type = '" + type + "'") + "]");
      if (listPattern == null) {
        FormattedString[] parentPatterns = CldrUtility.GetParentPatterns(locale, type, ResolvePatterns);
        return resolvedPatterns.GetOrAdd(key, parentPatterns);
      }
      if (CldrUtility.IsAlias(listPattern, out string use)) {
        return resolvedPatterns.GetOrAdd(key, ResolvePatterns(locale, use));
      }
      FormattedString[] patterns = new FormattedString[4];
      foreach (XElement parts in listPattern.Elements("listPatternPart")) {
        int index;
        switch (parts.Attribute("type").Value) {
          case "start":
            index = 0;
            break;
          case "middle":
            index = 1;
            break;
          case "end":
            index = 2;
            break;
          case "2":
            index = 3;
            break;
          default:
            continue;
        }
        patterns[index] = FormattedString.Parse(parts.Value);
      }
      if (listPattern.Attribute("inherits") != null) {
        FormattedString[] parentPatterns = CldrUtility.GetParentPatterns(locale, type, ResolvePatterns);
        CldrUtility.CopyPatternFromParent(patterns, parentPatterns);
      }
      for (int i = patterns.Length - 1; i >= 0; i--) {
        if (patterns[i] == null) {
          patterns[i] = FormattedString.Empty;
        }
      }
      return resolvedPatterns.GetOrAdd(key, patterns);
    }

    private static string ToLdmlListPatternType(ListType type, ListStyle style) {
      string str;
      switch (type) {
        case ListType.Disjuction:
          str = "or";
          break;
        case ListType.Unit:
          str = "unit";
          break;
        default:
          str = "standard";
          break;
      }
      if (style != ListStyle.Long) {
        str += "-" + IntlProviderOptions.ToStringValue(style);
      }
      return str;
    }
  }
}
