using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public sealed class FormattedString : IList<FormattedPart> {
    public static readonly FormattedString Empty = new FormattedString(FormattedPart.EmptyArray);

    private readonly FormattedPart[] parts;
    private string textValue;

    public FormattedString(FormattedPart[] parts) {
      Guard.ArgumentNotNull(parts, "parts");
      this.parts = (FormattedPart[])parts.Clone();
    }

    public FormattedString(IEnumerable<FormattedPart> parts) {
      Guard.ArgumentNotNull(parts, "parts");
      this.parts = parts.ToArray();
    }

    public FormattedPart this[int index] {
      get { return parts[index]; }
    }

    public int PartCount {
      get { return parts.Length; }
    }

    public static FormattedString Parse(string pattern) {
      Guard.ArgumentNotNull(pattern, "pattern");
      int index, lastIndex = 0;
      List<FormattedPart> parts = new List<FormattedPart>();
      while ((index = pattern.IndexOf('{', lastIndex)) >= 0) {
        if (index - lastIndex > 0) {
          parts.Add(new FormattedPart(FormattedPartType.Literal, pattern.Substring(lastIndex, index - lastIndex)));
        }
        lastIndex = pattern.IndexOf('}', index);
        if (lastIndex < 0) {
          break;
        }
        lastIndex += 1;
        parts.Add(new FormattedPart(FormattedPartType.Placeholder, pattern.Substring(index, lastIndex - index)));
      }
      if (pattern.Length - lastIndex > 0) {
        parts.Add(new FormattedPart(FormattedPartType.Literal, pattern.Substring(lastIndex)));
      }
      return new FormattedString(parts);
    }

    public override string ToString() {
      if (textValue != null) {
        return textValue;
      }
      FormattedPart[] parts = this.GetParts();
      if (parts.Length == 0) {
        return "";
      }
      if (parts.Length == 1) {
        return parts[0].Value;
      }
      StringBuilder sb = new StringBuilder();
      foreach (FormattedPart part in parts) {
        sb.Append(part.Value);
      }
      textValue = sb.ToString();
      return textValue;
    }

    public FormattedPart[] GetParts() {
      return (FormattedPart[])parts.Clone();
    }

    public EcmaArray ToPartArray() {
      return new EcmaArray(parts.Select(v => v.ToValue()).ToArray());
    }

    public EcmaArray ToPartArray(EcmaPropertyKey annotationProperty, string[] annotations) {
      Guard.ArgumentNotNull(annotations, "annotations");
      if (annotations.Length != parts.Length) {
        throw new ArgumentException("Supplied array must have the same length of part array", "unitAnnotations");
      }
      return new EcmaArray(parts.Select((v, i) => {
        RuntimeObject obj = v.ToValue().ToObject();
        if (annotations[i] != null) {
          obj.CreateDataPropertyOrThrow(annotationProperty, annotations[i]);
        }
        return obj.ToValue();
      }).ToArray());
    }

    #region Interfaces
    int ICollection<FormattedPart>.Count => this.PartCount;

    bool ICollection<FormattedPart>.IsReadOnly => true;

    FormattedPart IList<FormattedPart>.this[int index] {
      get => parts[index];
      set => throw new InvalidOperationException("Collection is read-only");
    }

    int IList<FormattedPart>.IndexOf(FormattedPart item) {
      return Array.IndexOf(parts, item);
    }

    void IList<FormattedPart>.Insert(int index, FormattedPart item) {
      throw new InvalidOperationException("Collection is read-only");
    }

    void IList<FormattedPart>.RemoveAt(int index) {
      throw new InvalidOperationException("Collection is read-only");
    }

    void ICollection<FormattedPart>.Add(FormattedPart item) {
      throw new InvalidOperationException("Collection is read-only");
    }

    void ICollection<FormattedPart>.Clear() {
      throw new InvalidOperationException("Collection is read-only");
    }

    bool ICollection<FormattedPart>.Contains(FormattedPart item) {
      return Array.IndexOf(parts, item) >= 0;
    }

    void ICollection<FormattedPart>.CopyTo(FormattedPart[] array, int arrayIndex) {
      parts.CopyTo(array, arrayIndex);
    }

    bool ICollection<FormattedPart>.Remove(FormattedPart item) {
      throw new InvalidOperationException("Collection is read-only");
    }

    IEnumerator<FormattedPart> IEnumerable<FormattedPart>.GetEnumerator() {
      for (int i = 0, len = parts.Length; i < len; i++) {
        yield return parts[i];
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return parts.GetEnumerator();
    }
    #endregion
  }
}
