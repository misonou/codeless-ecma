using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public class WeakKeyedItem : IEquatable<WeakKeyedItem> {
    private readonly WeakReference reference;
    private readonly int hashCode;

    protected WeakKeyedItem() {
      reference = new WeakReference(this);
      hashCode = base.GetHashCode();
    }

    internal WeakKeyedItem(object target) {
      Guard.ArgumentNotNull(target, "target");
      reference = new WeakReference(target);
      hashCode = target.GetHashCode();
    }

    protected internal bool IsTargetAlive {
      get { return reference.IsAlive; }
    }

    protected internal object Target {
      get { return reference.Target; }
    }

    public bool Equals(WeakKeyedItem other) {
      if (other == null || !reference.IsAlive || !other.reference.IsAlive) {
        return false;
      }
      return reference.Target.Equals(other.reference.Target);
    }

    public override int GetHashCode() {
      return hashCode;
    }
  }
}
