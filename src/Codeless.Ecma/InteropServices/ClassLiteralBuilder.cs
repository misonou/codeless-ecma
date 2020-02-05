using Codeless.Ecma.Runtime;
using System;
using System.Collections;

namespace Codeless.Ecma.InteropServices {
  public interface IStaticModifier {
    bool IsStatic { get; }
  }

  public interface IExtendsModifier {
    EcmaValue Target { get; }
  }

  public abstract class ClassLiteralBuilder : ObjectLiteralBuilderBase {
    public static readonly IStaticModifier Static = new StaticModifier();

    private static readonly Action<EcmaValue[]> defaultInheritedConstructor = DefaultInheritedConstructor;
    private static readonly Action<EcmaValue[]> defaultEmptyConstructor = DefaultEmptyConstructor;
    private readonly IExtendsModifier extends;
    private readonly string name;
    private RuntimeFunction constructor;
    private PropertyDefinitionCollection properties;
    private PropertyDefinitionCollection staticProperties;

    public ClassLiteralBuilder(string name, IExtendsModifier super) {
      this.name = name;
      this.extends = super;
    }

    public EcmaValue this[EcmaPropertyKey key] {
      set {
        if (key == WellKnownProperty.Constructor) {
          constructor = value.GetUnderlyingObject<RuntimeFunction>();
        } else {
          EnsureBuilder(ref properties).Add(key, value);
        }
      }
    }

    public EcmaValue this[PropertyDefinitionType type, EcmaPropertyKey key] {
      set { EnsureBuilder(ref properties).Add(new PropertyDefinition(type, key, value)); }
    }

    public EcmaValue this[IStaticModifier staticModifier, EcmaPropertyKey key] {
      set {
        Guard.ArgumentNotNull(staticModifier, "staticModifier");
        if (staticModifier.IsStatic) {
          EnsureBuilder(ref staticProperties).Add(key, value);
        } else {
          this[key] = value;
        }
      }
    }

    public EcmaValue this[IStaticModifier staticModifier, PropertyDefinitionType type, EcmaPropertyKey key] {
      set {
        Guard.ArgumentNotNull(staticModifier, "staticModifier");
        if (staticModifier.IsStatic) {
          EnsureBuilder(ref staticProperties).Add(new PropertyDefinition(type, key, value));
        } else {
          this[type, key] = value;
        }
      }
    }

    protected override RuntimeObject CreateObject() {
      RuntimeFunction constructor = this.constructor ?? new DelegateRuntimeFunction(name, extends != null ? defaultInheritedConstructor : defaultEmptyConstructor);
      constructor.AsDerivedClassConstructorOf(extends != null ? extends.Target.ToObject() : null);
      if (name != null) {
        constructor.DefinePropertyOrThrow(WellKnownProperty.Name, new EcmaPropertyDescriptor(name, EcmaPropertyAttributes.Configurable));
      }
      if (staticProperties != null) {
        DefineProperties(constructor, staticProperties);
      }
      if (properties != null) {
        DefineProperties(constructor.Prototype, properties);
      }
      return constructor;
    }

    public static IExtendsModifier Extends(EcmaValue from) {
      return new ExtendsModifier(from);
    }

    private static void DefaultInheritedConstructor(params EcmaValue[] args) {
      Keywords.Super.Construct(args);
    }

    private static void DefaultEmptyConstructor(params EcmaValue[] args) {
    }

    private static PropertyDefinitionCollection EnsureBuilder(ref PropertyDefinitionCollection builder) {
      if (builder == null) {
        builder = new PropertyDefinitionCollection();
      }
      return builder;
    }

    private class ExtendsModifier : IExtendsModifier {
      public ExtendsModifier(EcmaValue superClass) {
        this.Target = superClass;
      }

      public EcmaValue Target { get; }
    }

    private class StaticModifier : IStaticModifier {
      public bool IsStatic => true;
    }
  }
}
