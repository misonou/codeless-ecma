using System;

namespace Codeless.Ecma.Intl.Utilities {
  [AttributeUsage(AttributeTargets.Field)]
  internal class StringValueAttribute : Attribute {
    public StringValueAttribute(string stringValue) {
      this.StringValue = stringValue;
    }

    public string StringValue { get; }
  }
}
