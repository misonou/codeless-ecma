using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  internal static class AttributeHelper {
    public static bool HasAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute {
      object[] arr = member.GetCustomAttributes(typeof(T), false);
      if (arr.Length > 0) {
        attribute = (T)arr[0];
        return true;
      }
      attribute = null;
      return false;
    }
  }
}
