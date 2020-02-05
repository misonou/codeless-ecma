using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.InteropServices {
  internal class PropertyDefinitionCollection : Collection<IPropertyDefinition> {
    public void Add(EcmaPropertyKey key, EcmaValue value) {
      Add(new PropertyDefinition(PropertyDefinitionType.Default, key, value));
    }
  }
}
