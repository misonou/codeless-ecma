using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Codeless.Ecma.Intl.Utilities {
  internal class Map<T1, T2> {
    private readonly Dictionary<T1, T2> forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> reverse = new Dictionary<T2, T1>();

    public Map() {
      this.Forward = new ReadOnlyDictionary<T1, T2>(forward);
      this.Reverse = new ReadOnlyDictionary<T2, T1>(reverse);
    }

    public ReadOnlyDictionary<T1, T2> Forward { get; }
    public ReadOnlyDictionary<T2, T1> Reverse { get; }

    public void Add(T1 key, T2 value) {
      forward[key] = value;
      reverse[value] = key;
    }
  }
}
