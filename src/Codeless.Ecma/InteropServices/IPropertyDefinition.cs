using System;

namespace Codeless.Ecma.InteropServices {
  public enum PropertyDefinitionType {
    Default,
    Method,
    Getter,
    Setter,
    Spread
  }

  public interface IPropertyDefinition {
    PropertyDefinitionType Type { get; }
    EcmaPropertyKey PropertyName { get; }
    EcmaValue Value { get; }
  }
}
