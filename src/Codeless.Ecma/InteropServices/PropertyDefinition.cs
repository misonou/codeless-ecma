using System;

namespace Codeless.Ecma.InteropServices {
  internal class PropertyDefinition : IPropertyDefinition {
    public PropertyDefinition(PropertyDefinitionType type, EcmaPropertyKey key, EcmaValue value) {
      this.Type = type;
      this.PropertyName = key;
      this.Value = value;
    }

    public PropertyDefinitionType Type { get; }
    public EcmaPropertyKey PropertyName { get; }
    public EcmaValue Value { get; }
  }
}
