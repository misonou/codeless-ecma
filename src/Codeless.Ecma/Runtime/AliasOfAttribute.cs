using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Method)]
  public class AliasOfAttribute : Attribute {
    public AliasOfAttribute(WellKnownObject objectType, string name)  {
      this.ObjectType = objectType;
      this.Name = name;
    }

    public AliasOfAttribute(WellKnownObject objectType, WellKnownSymbol symbol) {
      this.ObjectType = objectType;
      this.Symbol = symbol;
    }

    public WellKnownObject ObjectType { get; }
    public WellKnownSymbol Symbol { get; }
    public string Name { get; }
  }
}
