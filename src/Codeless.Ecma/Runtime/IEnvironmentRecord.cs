using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  internal interface IEnvironmentRecord {
    bool HasBinding(string name);
    void CreateMutableBinding(string name, bool deletable);
    void CreateImmutableBinding(string name, bool throwOnReinit);
    void InitializeBinding(string name, EcmaValue value);
    void SetMutableBinding(string name, EcmaValue value, bool throwOnError);
    EcmaValue GetBindingValue(string name, bool throwOnError);
    bool DeleteBinding(string name);
    bool HasThisBinding();
    bool HasSuperBinding();
    RuntimeObject WithBaseObject();
  }
}
