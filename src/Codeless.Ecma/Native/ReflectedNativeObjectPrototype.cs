using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Codeless.Ecma.Native {
  internal class ReflectedNativeObjectPrototype : RuntimeObject {
    private static readonly Hashtable hashtable = new Hashtable();

    public ReflectedNativeObjectPrototype(Type type, bool defaultHideMembers)
      : base(WellKnownObject.ObjectPrototype) {
      PropertyInfo[] properties = type.GetProperties();
      bool showAttributedOnly = defaultHideMembers || properties.Any(v => v.HasAttribute(out IntrinsicMemberAttribute _));
      foreach (PropertyInfo property in properties) {
        IntrinsicMemberAttribute attribute;
        property.HasAttribute(out attribute);
        if (!showAttributedOnly || attribute != null) {
          string propertyName = attribute != null ? attribute.Name : property.Name;
          DefineOwnPropertyNoChecked(propertyName, new EcmaPropertyDescriptor(
            property.GetGetMethod() != null ? new NativeRuntimeFunction(propertyName, property.GetGetMethod()) : EcmaValue.Undefined,
            property.GetSetMethod() != null ? new NativeRuntimeFunction(propertyName, property.GetSetMethod()) : EcmaValue.Undefined));
        }
      }
      DefineOwnPropertyNoChecked(WellKnownPropertyName.Constructor, new EcmaPropertyDescriptor(new Constructor(type.Name), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
    }

    public static RuntimeObject FromType(Type type) {
      if (hashtable[type] is RuntimeObject o) {
        return o;
      }
      lock (hashtable) {
        if (hashtable[type] is RuntimeObject o2) {
          return o2;
        }
        hashtable[type] = new ReflectedNativeObjectPrototype(type, false);
      }
      return hashtable[type] as RuntimeObject;
    }

    private class Constructor : RuntimeFunction {
      public override bool IsConstructor => true;

      public Constructor(string name) {
        InitProperty(name, 0);
      }
    }
  }
}
