using System.Linq;
using System.Reflection;

namespace Codeless.Ecma.Intl.Utilities {
  internal class StringValueMap<T> : Map<string, T> {
    public static readonly StringValueMap<T> Default = new StringValueMap<T>();

    private StringValueMap() {
      foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public)) {
        StringValueAttribute attr = field.GetCustomAttributes(false).OfType<StringValueAttribute>().FirstOrDefault();
        if (attr != null) {
          Add(attr.StringValue, (T)field.GetValue(null));
        }
      }
    }
  }
}
