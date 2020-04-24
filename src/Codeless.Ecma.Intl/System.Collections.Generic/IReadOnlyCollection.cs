#if NET35 || NET40
using System;
using System.Collections;

namespace System.Collections.Generic {
  /// <summary>
  /// Read-only collection of elements.
  /// </summary>
  /// <typeparam name="T">Element type.</typeparam>
  public interface IReadOnlyCollection<T> : IEnumerable<T>, IEnumerable {
    /// <summary>
    /// Number of elements in the collection.
    /// </summary>
    int Count { get; }
  }
}
#endif
