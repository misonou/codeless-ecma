using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal class CldrPluralRules {
    private delegate bool Predicate(double n, double i, int v, int w, double f, double t);

    private const string Xpath = "/supplementalData/plurals/pluralRules";
    private const string CardinalFileName = "Codeless.Ecma.Intl.Data.plurals.xml.gz";
    private const string OrdinalFileName = "Codeless.Ecma.Intl.Data.ordinals.xml.gz";

    private static readonly ICollection<string> availableLocales = CldrUtility.LoadXml(CardinalFileName).XPathSelectElements(Xpath).SelectMany(v => v.Attribute("locales").Value.Split(' ')).ToList().AsReadOnly();
    private static readonly Dictionary<string, ParameterExpression> param = new Dictionary<string, ParameterExpression> {
      ["n"] = Expression.Parameter(typeof(double), "n"),
      ["i"] = Expression.Parameter(typeof(double), "i"),
      ["v"] = Expression.Parameter(typeof(int), "v"),
      ["w"] = Expression.Parameter(typeof(int), "w"),
      ["f"] = Expression.Parameter(typeof(double), "f"),
      ["t"] = Expression.Parameter(typeof(double), "t")
    };
    private static readonly Regex relationPattern = new Regex(@"(?:(?<param>\w+)(?:\s*%\s*(?<mod>\d+))?)\s*(?<op>\!\=|\=)\s*(?<values>\d+(?:\.\.\d+)?(?:\s*,\s*\d+(?:\.\.\d+)?)*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly ConcurrentDictionary<string, CldrPluralRules> cardinalRules = new ConcurrentDictionary<string, CldrPluralRules>();
    private static readonly ConcurrentDictionary<string, CldrPluralRules> ordinalRules = new ConcurrentDictionary<string, CldrPluralRules>();

    private readonly Dictionary<PluralCategories, Predicate> ruleset = new Dictionary<PluralCategories, Predicate>();

    private CldrPluralRules(XElement rules) {
      this.PluralCategories = PluralCategories.Other;
      foreach (XElement node in rules.Elements("pluralRule")) {
        PluralCategories category = IntlProviderOptions.ParseEnum<PluralCategories>(node.Attribute("count").Value);
        if (category != PluralCategories.Other) {
          string textContent = node.Value;
          ruleset[category] = ParseExpression(textContent);
          this.PluralCategories |= category;
        }
      }
    }

    public PluralCategories PluralCategories { get; }

    public static ICollection<string> AvailableLocales => availableLocales;

    public static CldrPluralRules Resolve(PluralRuleType type, string locale) {
      string normalizedLocale = IntlUtility.RemoveUnicodeExtensions(locale);
      ConcurrentDictionary<string, CldrPluralRules> cache = type == PluralRuleType.Cardinal ? cardinalRules : ordinalRules;
      return cache.GetOrAdd(normalizedLocale, _ => {
        XDocument doc = CldrUtility.LoadXml(type == PluralRuleType.Cardinal ? CardinalFileName : OrdinalFileName);
        foreach (XElement node in doc.XPathSelectElements(Xpath)) {
          string[] locales = node.Attribute("locales").Value.Split(' ');
          if (locales.Contains(normalizedLocale)) {
            CldrPluralRules rule = new CldrPluralRules(node);
            foreach (string v in locales) {
              cache.TryAdd(v, rule);
            }
            return rule;
          }
        }
        return Resolve(type, CldrUtility.GetParentLocale(normalizedLocale));
      });
    }

    public PluralCategories Match(double value) {
      string str = value.ToString(CultureInfo.InvariantCulture);
      int w = 0;
      double t = 0;
      double n = Math.Abs(value);
      double i = Math.Floor(value);
      int dot = str.IndexOf('.');
      if (dot >= 0) {
        w = str.Length - dot - 1;
        t = Double.Parse(str.Substring(dot + 1));
      }
      foreach (KeyValuePair<PluralCategories, Predicate> rule in ruleset) {
        if (rule.Value(n, i, w, w, t, t)) {
          return rule.Key;
        }
      }
      return PluralCategories.Other;
    }

    public IEnumerable<PluralCategories> EnumerateCategories() {
      foreach (PluralCategories kind in Enum.GetValues(typeof(PluralCategories))) {
        if ((kind & this.PluralCategories) != 0) {
          yield return kind;
        }
      }
    }

    private static Predicate ParseExpression(string expr) {
      List<Expression> clauses = new List<Expression>();
      Expression body = null;
      int lastIndex = 0;
      foreach (Match m in relationPattern.Matches(expr)) {
        Expression comparand = param[m.Groups["param"].Value];
        if (comparand.Type == typeof(double)) {
          comparand = ParseCompareExpression(comparand, Double.Parse, m.Groups["values"].Value, m.Groups["mod"]);
        } else {
          comparand = ParseCompareExpression(comparand, Int32.Parse, m.Groups["values"].Value, m.Groups["mod"]);
        }
        if (m.Index - lastIndex > 0) {
          string op = expr.Substring(lastIndex, m.Index - lastIndex).Trim();
          if (op == "or") {
            if (body != null) {
              clauses.Add(body);
            }
            body = comparand;
          } else {
            body = Expression.AndAlso(body, comparand);
          }
        } else {
          body = comparand;
        }
        lastIndex = m.Index + m.Length;
      }
      clauses.Add(body);
      return Expression.Lambda<Predicate>(clauses.Aggregate(Expression.OrElse), param.Values.ToArray()).Compile();
    }

    private static Expression ParseCompareExpression<T>(Expression value, Func<string, T> toNumber, string expr, Group mod) {
      Expression comparand = value;
      if (mod.Success) {
        comparand = Expression.Modulo(value, Expression.Constant(toNumber(mod.Value)));
      }
      if (expr.IndexOf("..") >= 0) {
        Expression clause = comparand;
#if !NET35
        ParameterExpression variable = Expression.Parameter(typeof(T));
        clause = variable;
#endif
        clause = expr.Split(',').Select(v => {
          int rangeOperator = v.IndexOf("..");
          if (rangeOperator >= 0) {
            return Expression.AndAlso(
              Expression.GreaterThanOrEqual(clause, Expression.Constant(toNumber(v.Substring(0, rangeOperator)))),
              Expression.LessThanOrEqual(clause, Expression.Constant(toNumber(v.Substring(rangeOperator + 2)))));
          } else {
            return Expression.Equal(clause, Expression.Constant(toNumber(v)));
          }
        }).Aggregate(Expression.OrElse);
#if !NET35
        clause = Expression.Block(new[] { variable }, Expression.Assign(variable, comparand), clause);
#endif
        return clause;
      }
      if (expr.IndexOf(',') >= 0) {
        return Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(T) }, Expression.Constant(expr.Split(',').Select(toNumber).ToArray()), comparand);
      }
      return Expression.Equal(comparand, Expression.Constant(toNumber(expr)));
    }
  }
}
