#if NET35 || NET40
using System;
using System.Collections;
using System.Reflection;

namespace System.Collections.Generic {
  /// <summary>
  /// Read-only list of elements.
  /// </summary>
  /// <typeparam name="T">Element type.</typeparam>
  public interface IReadOnlyList<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable {
    /// <summary>
    /// Gets the element at the specified index in the list.
    /// </summary>
    /// <param name="index">Zero-based index of the element to get.</param>
    /// <returns>Element at the specified index in the list.</returns>
    T this[int index] { get; }
  }
}
#endif
