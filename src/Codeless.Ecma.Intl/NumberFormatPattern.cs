using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Intl {
  internal class NumberFormatPattern {
    private static readonly ConcurrentDictionary<string, NumberFormatPattern> cache = new ConcurrentDictionary<string, NumberFormatPattern>();

    public NumberFormatPattern(FormattedString pattern)
      : this(pattern, pattern, pattern) { }

    public NumberFormatPattern(FormattedString positivePattern, FormattedString negativePattern)
      : this(positivePattern, negativePattern, positivePattern) { }

    public NumberFormatPattern(FormattedString positivePattern, FormattedString negativePattern, FormattedString zeroPattern) {
      Guard.ArgumentNotNull(positivePattern, "positivePattern");
      Guard.ArgumentNotNull(negativePattern, "negativePattern");
      Guard.ArgumentNotNull(zeroPattern, "zeroPattern");
      this.PositivePattern = positivePattern;
      this.NegativePattern = negativePattern;
      this.ZeroPattern = zeroPattern;
    }

    public FormattedString PositivePattern { get; }
    public FormattedString NegativePattern { get; }
    public FormattedString ZeroPattern { get; }

    public NumberFormatPattern ToDisplayFormat(NumberFormat formatter) {
      Guard.ArgumentNotNull(formatter, "formatter");
      int mnd = formatter.Digits.MinimumIntegerDigits;
      int mnfd = 21;
      int mxfd = 21;
      if (formatter.Digits.RoundingType != RoundingType.SignificantDigits) {
        mnfd = formatter.Digits.MinimumFractionDigits;
        mxfd = formatter.Digits.MaximumFractionDigits;
      }
      string key = String.Join(";", new[] { this.PositivePattern.ToString(), this.NegativePattern.ToString(), this.ZeroPattern.ToString(), formatter.SignDisplay.ToString(), formatter.UseGrouping.ToString(), mnd.ToString(), mnfd.ToString(), mxfd.ToString() });
      return cache.GetOrAdd(key, _ => {
        FormattedString[] patterns = new[] { this.PositivePattern, this.NegativePattern, this.ZeroPattern };
        string negativePattern = this.NegativePattern.First(v => v.Type == FormattedPartType.Placeholder).Value;
        bool[] addPlusSign = new bool[3];
        addPlusSign[0] = formatter.SignDisplay == SignDisplayFormat.Always || formatter.SignDisplay == SignDisplayFormat.ExceptZero;
        addPlusSign[2] = formatter.SignDisplay == SignDisplayFormat.Always;

        for (int i = 0; i < 3; i++) {
          FormattedPart[] parts = patterns[i].GetParts();
          int index = Array.FindIndex(parts, v => v.Type == FormattedPartType.Placeholder);
          string pattern = MakePattern(parts[index].Value, negativePattern, addPlusSign[i], formatter.UseGrouping, mnd, mnfd, mxfd);
          if (pattern != parts[index].Value) {
            parts[index] = new FormattedPart(FormattedPartType.Placeholder, pattern);
            patterns[i] = new FormattedString(parts);
          }
        }
        return new NumberFormatPattern(patterns[0], patterns[1], patterns[2]);
      });
    }

    private static string MakePattern(string pattern, string negativePattern, bool addPlusSign, bool useGrouping, int mnd, int mnfd, int mxfd) {
      if (addPlusSign && pattern.IndexOf('+') < 0) {
        pattern = negativePattern.IndexOf('-') >= 0 ? negativePattern.Replace('-', '+') : "+" + negativePattern;
      }
      if (!useGrouping) {
        pattern = pattern.Replace(",", "");
      }
      int pos = pattern.IndexOf('.');
      if (pos < 0) {
        pos = pattern.Length;
      }
      StringBuilder sb = new StringBuilder(pattern);
      if (mnd > 1) {
        int count1 = 0;
        for (int j = pos - 1; j >= 0 && count1 < mnd; j--) {
          if (sb[j] == '#' || sb[j] == '0') {
            sb[j] = '0';
            count1++;
          }
        }
        if (count1 < mnd) {
          sb.Insert(0, "0", mnd - count1);
        }
      }
      if (pos == pattern.Length) {
        sb.Append('.');
      }
      int count = 0;
      int i = pos + 1;
      for (; i < pattern.Length; i++) {
        if (sb[i] == '#' || sb[i] == '0') {
          sb[i] = count < mnfd ? '0' : '#';
          count++;
        } else {
          break;
        }
      }
      int diff = mnfd - count;
      if (diff > 0) {
        sb.Insert(i, "0", diff);
        count += diff;
        i += diff;
      }
      diff = mxfd - count;
      if (diff > 0) {
        sb.Insert(i, "#", diff);
      } else if (diff < 0) {
        sb.Remove(i + diff, -diff);
      }
      return sb.ToString();
    }
  }
}
