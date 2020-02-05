using Codeless.Ecma.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace Codeless.Ecma.InteropServices {
  public abstract class ObjectLiteralBuilderBase {
    protected abstract RuntimeObject CreateObject();

    protected void DefineProperties(RuntimeObject target, IEnumerable<IPropertyDefinition> properties) {
      Guard.ArgumentNotNull(target, "target");
      Guard.ArgumentNotNull(properties, "properties");
      Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> dictionary = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
      foreach (IPropertyDefinition e in properties) {
        switch (e.Type) {
          case PropertyDefinitionType.Method:
            dictionary[e.PropertyName] = new EcmaPropertyDescriptor(e.Value.GetUnderlyingObject<RuntimeFunction>().AsHomedMethodOf(target), EcmaPropertyAttributes.DefaultDataProperty);
            break;
          case PropertyDefinitionType.Getter:
            if (!dictionary.ContainsKey(e.PropertyName)) {
              dictionary[e.PropertyName] = new EcmaPropertyDescriptor(EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable);
            }
            dictionary[e.PropertyName].Get = EnsureHomedMethod(e.Value, target, 0, InternalString.Error.GetterMustBeFunction, InternalString.Error.GetterMustHaveNoParam);
            break;
          case PropertyDefinitionType.Setter:
            if (!dictionary.ContainsKey(e.PropertyName)) {
              dictionary[e.PropertyName] = new EcmaPropertyDescriptor(EcmaPropertyAttributes.Enumerable | EcmaPropertyAttributes.Configurable);
            }
            dictionary[e.PropertyName].Set = EnsureHomedMethod(e.Value, target, 1, InternalString.Error.SetterMustBeFunction, InternalString.Error.SetterMustHaveOneParam);
            break;
          case PropertyDefinitionType.Spread:
            foreach(EcmaPropertyKey key in e.Value) {
              dictionary[key] = new EcmaPropertyDescriptor(e.Value[key], EcmaPropertyAttributes.DefaultDataProperty);
            }
            break;
          default:
            dictionary[e.PropertyName] = new EcmaPropertyDescriptor(e.Value, EcmaPropertyAttributes.DefaultDataProperty);
            break;
        }
      }
      foreach (KeyValuePair<EcmaPropertyKey, EcmaPropertyDescriptor> e in dictionary) {
        target.DefinePropertyOrThrow(e.Key, e.Value);
      }
    }

    private RuntimeFunction EnsureHomedMethod(EcmaValue value, RuntimeObject target, int requiredLength, string notFunctionError, string incorrectLengthError) {
      Guard.ArgumentIsCallable(value, notFunctionError);
      RuntimeFunction fn = value.GetUnderlyingObject<RuntimeFunction>();
      if (fn[WellKnownProperty.Length] != requiredLength) {
        throw new EcmaSyntaxErrorException(incorrectLengthError);
      }
      return fn.AsHomedMethodOf(target);
    }
  }
}
