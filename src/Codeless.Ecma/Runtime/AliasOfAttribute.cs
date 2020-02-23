using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Method)]
  public class AliasOfAttribute : Attribute {
    public AliasOfAttribute(object objectType, string name)  {
      Guard.ArgumentNotNull(objectType, "objectType");
      Guard.ArgumentNotNull(name, "name");
      this.ObjectType = objectType;
      this.Name = name;
    }

    public AliasOfAttribute(object objectType, WellKnownSymbol symbol) {
      Guard.ArgumentNotNull(objectType, "objectType");
      this.ObjectType = objectType;
      this.Symbol = symbol;
    }

    public object ObjectType { get; }
    public WellKnownSymbol Symbol { get; }
    public string Name { get; }
  }
}
