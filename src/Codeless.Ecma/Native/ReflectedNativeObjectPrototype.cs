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

    private ReflectedNativeObjectPrototype(RuntimeRealm.SharedObjectContainer container, Type type, bool defaultHideMembers)
      : base(WellKnownObject.ObjectPrototype) {
      PropertyInfo[] properties = type.GetProperties();
      bool showAttributedOnly = defaultHideMembers || properties.Any(v => v.HasAttribute(out IntrinsicMemberAttribute _));
      foreach (PropertyInfo property in properties) {
        IntrinsicMemberAttribute attribute;
        property.HasAttribute(out attribute);
        if (!showAttributedOnly || attribute != null) {
          string propertyName = attribute != null ? attribute.Name : property.Name;
          DefineOwnPropertyNoChecked(propertyName, new EcmaPropertyDescriptor(
            property.GetGetMethod() != null ? container.Add(new NativeRuntimeFunction(propertyName, property.GetGetMethod())).ToValue() : EcmaValue.Undefined,
            property.GetSetMethod() != null ? container.Add(new NativeRuntimeFunction(propertyName, property.GetSetMethod())).ToValue() : EcmaValue.Undefined,
            EcmaPropertyAttributes.DefaultDataProperty));
        }
      }
      foreach (MethodInfo method in type.GetMethods()) {
        if (method.HasAttribute(out IntrinsicMemberAttribute attribute)) {
          string methodName = attribute.Name ?? method.Name;
          DefineOwnPropertyNoChecked(methodName, new EcmaPropertyDescriptor(container.Add(new NativeRuntimeFunction(methodName, method)).ToValue(), EcmaPropertyAttributes.DefaultMethodProperty));
        }
      }
      DefineOwnPropertyNoChecked(WellKnownProperty.Constructor, new EcmaPropertyDescriptor(container.Add(new Constructor(type.Name)).ToValue(), EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
    }

    public static RuntimeObject FromType(Type type) {
      RuntimeRealm.SharedObjectHandle sharedValue = default;
      if (hashtable[type] is RuntimeRealm.SharedObjectHandle o) {
        sharedValue = o;
      } else {
        lock (hashtable) {
          if (hashtable[type] is RuntimeRealm.SharedObjectHandle o2) {
            sharedValue = o2;
          } else {
            using (RuntimeRealm.SharedObjectContainer container = new RuntimeRealm.SharedObjectContainer()) {
              RuntimeObject o3 = new ReflectedNativeObjectPrototype(container, type, false);
              sharedValue = container.Add(o3);
              hashtable[type] = sharedValue;
            }
          }
        }
      }
      return sharedValue.GetRuntimeObject(RuntimeRealm.Current);
    }

    private class Constructor : RuntimeFunction {
      public override bool IsConstructor => true;

      public Constructor(string name) {
        InitProperty(name, 0);
      }
    }
  }
}
