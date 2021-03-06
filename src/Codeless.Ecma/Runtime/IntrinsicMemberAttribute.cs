﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
  public class IntrinsicMemberAttribute : Attribute {
    public IntrinsicMemberAttribute() {
      this.FunctionLength = -1;
    }

    public IntrinsicMemberAttribute(EcmaPropertyAttributes attributes) {
      this.FunctionLength = -1;
      this.Attributes = attributes;
    }

    public IntrinsicMemberAttribute(string name)
      : this() {
      this.Name = name ?? String.Empty;
    }

    public IntrinsicMemberAttribute(WellKnownSymbol symbol)
      : this() {
      this.Symbol = symbol;
    }

    public IntrinsicMemberAttribute(string name, EcmaPropertyAttributes attributes)
      : this(attributes) {
      this.Name = name ?? String.Empty;
    }

    public IntrinsicMemberAttribute(WellKnownSymbol symbol, EcmaPropertyAttributes attributes)
      : this(attributes) {
      this.Symbol = symbol;
    }

    public string Name { get; private set; }
    public WellKnownSymbol Symbol { get; private set; }
    public EcmaPropertyAttributes? Attributes { get; private set; }

    public int FunctionLength { get; set; }
    public bool Getter { get; set; }
    public bool Setter { get; set; }
    public bool Global { get; set; }
    public bool Overridable { get; set; }
  }
}
