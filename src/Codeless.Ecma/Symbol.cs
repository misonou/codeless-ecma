using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.SymbolConstructor)]
  public class Symbol {
    private static readonly ConcurrentDictionary<string, Symbol> globalSymbolDict = new ConcurrentDictionary<string, Symbol>();
    private static readonly Symbol[] enumDict;

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Iterator = new Symbol("iterator", WellKnownSymbol.Iterator);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Match = new Symbol("match", WellKnownSymbol.Match);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol MatchAll = new Symbol("matchAll", WellKnownSymbol.MatchAll);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Replace = new Symbol("replace", WellKnownSymbol.Replace);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Search = new Symbol("search", WellKnownSymbol.Search);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Split = new Symbol("split", WellKnownSymbol.Split);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol HasInstance = new Symbol("hasInstance", WellKnownSymbol.HasInstance);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol IsConcatSpreadable = new Symbol("isConcatSpreadable", WellKnownSymbol.IsConcatSpreadable);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Unscopables = new Symbol("unscopables", WellKnownSymbol.Unscopables);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol Species = new Symbol("species", WellKnownSymbol.Species);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol ToPrimitive = new Symbol("toPrimitive", WellKnownSymbol.ToPrimitive);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol ToStringTag = new Symbol("toStringTag", WellKnownSymbol.ToStringTag);
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly Symbol AsyncIterator = new Symbol("asyncIterator", WellKnownSymbol.AsyncIterator);

    static Symbol() {
      enumDict = typeof(Symbol).GetFields(BindingFlags.Static | BindingFlags.Public).Select(v => (Symbol)v.GetValue(null)).OrderBy(v => v.SymbolType).ToArray();
    }

    public Symbol()
      : this(null) { }

    public Symbol(string description) {
      this.Description = description;
    }

    private Symbol(string description, WellKnownSymbol lookupValue)
      : this("Symbol." + description) {
      this.SymbolType = lookupValue;
    }

    public string Description { get; private set; }

    public WellKnownSymbol SymbolType { get; private set; }

    [IntrinsicMember]
    public static Symbol For(string key) {
      return globalSymbolDict.GetOrAdd(key, k => new Symbol(k));
    }

    [IntrinsicMember]
    public static string KeyFor(Symbol sym) {
      foreach (KeyValuePair<string, Symbol> e in globalSymbolDict) {
        if (e.Value == sym) {
          return e.Key;
        }
      }
      return null;
    }

    public static Symbol GetSymbol(WellKnownSymbol symbol) {
      if ((int)symbol < 1 || (int)symbol > enumDict.Length) {
        throw new ArgumentException("Invalid symbol type.", "symbol");
      }
      return enumDict[(int)symbol - 1];
    }

    [EcmaSpecification("SymbolDescriptiveString", EcmaSpecificationKind.RuntimeSemantics)]
    public override string ToString() {
      return "Symbol(" + this.Description + ")";
    }
  }
}
