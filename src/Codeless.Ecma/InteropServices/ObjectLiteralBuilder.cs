using Codeless.Ecma.Runtime;
using System;
using System.Collections;

namespace Codeless.Ecma.InteropServices {
  public abstract class ObjectLiteralBuilder : ObjectLiteralBuilderBase {
    private readonly PropertyDefinitionCollection properties = new PropertyDefinitionCollection();

    public EcmaValue this[EcmaPropertyKey key] {
      set { properties.Add(key, value); }
    }

    public EcmaValue this[PropertyDefinitionType type, EcmaPropertyKey key] {
      set { properties.Add(new PropertyDefinition(type, key, value)); }
    }

    public EcmaValue this[PropertyDefinitionType type] {
      set {
        if (type != PropertyDefinitionType.Spread) {
          throw new ArgumentException("Property definiton type must be Spread", "type");
        }
        properties.Add(new PropertyDefinition(PropertyDefinitionType.Spread, default, value));
      }
    }

    protected override RuntimeObject CreateObject() {
      EcmaObject target = new EcmaObject();
      DefineProperties(target, properties);
      return target;
    }
  }
}
