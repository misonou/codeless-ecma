using Codeless.Ecma.InteropServices;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime {
  [DebuggerDisplay("RuntimeRealm ID={ID}")]
  public class RuntimeRealm : IDisposable {
    private static readonly object syncRoot = new object();
    private static readonly Dictionary<Type, RuntimeModule> enumTypes = new Dictionary<Type, RuntimeModule>();
    private static readonly RuntimeRealm sharedRealm = new RuntimeRealm();
    private static readonly bool inited;

    private static RuntimeObject[][] sharedObjects = { };
    private static int accum;
    [ThreadStatic]
    private static RuntimeRealm current;
    [ThreadStatic]
    private static bool useSharedRealm;

    private readonly Dictionary<RuntimeObject, SharedObjectHandle> objectHandles = new Dictionary<RuntimeObject, SharedObjectHandle>();
    private readonly WeakKeyedCollection nativeWrappers = new WeakKeyedCollection();
    private readonly Hashtable ht = new Hashtable();
    private RuntimeObject[][] intrinsics = { };

    static RuntimeRealm() {
      current = sharedRealm;
      RegisterModule<IntrinsicModule>();
      current = null;
      inited = true;
    }

    public RuntimeRealm() {
      if (inited) {
        this.ID = Interlocked.Increment(ref accum);
        this.ExecutionContext = RuntimeExecution.Current;
        Initialized(this, EventArgs.Empty);
      }
    }

    public static event EventHandler Initialized = delegate { };

    public event EventHandler BeforeDisposed = delegate { };

    public int ID { get; }

    public RuntimeExecution ExecutionContext { get; }

    public bool Disposed { get; private set; }

    public RuntimeObject Global {
      get { return GetRuntimeObject(WellKnownObject.Global); }
    }

    public Hashtable Properties {
      get {
        if (this.Disposed) {
          throw new ObjectDisposedException("");
        }
        return ht;
      }
    }

    public static RuntimeModule IntrinsicModule {
      get { return enumTypes[typeof(WellKnownObject)]; }
    }

    public static RuntimeRealm Current {
      get {
        if (useSharedRealm) {
          return sharedRealm;
        }
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        if (invocation != null) {
          return invocation.FunctionObject.Realm;
        }
        if (current == null) {
          current = RuntimeExecution.Current.DefaultRealm;
        }
        return current;
      }
    }

    internal static RuntimeRealm SharedRealm {
      get { return sharedRealm; }
    }

    public static T RegisterModule<T>() where T : RuntimeModule, new() {
      bool previousUseSharedRealm = useSharedRealm;
      try {
        if (!useSharedRealm) {
          Monitor.Enter(syncRoot);
        }
        useSharedRealm = true;
        T module = new T();
        if (enumTypes.TryGetValue(module.EnumType, out RuntimeModule module1)) {
          return (T)module1;
        }
        using (SharedObjectContainer container = new SharedObjectContainer(module)) {
          return module;
        }
      } finally {
        if (!previousUseSharedRealm) {
          useSharedRealm = false;
          Monitor.Exit(syncRoot);
        }
      }
    }

    public static RuntimeRealm GetRealm(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        return obj.ToObject().Realm;
      }
      return RuntimeRealm.Current;
    }

    public RuntimeObject GetRuntimeObject(WellKnownObject type) {
      return GetSharedObject(0, (int)type);
    }

    public RuntimeObject GetRuntimeObject(SharedObjectHandle handle) {
      int index = handle.HandleValue;
      return GetSharedObject(index >> 16, index & 0xFFFF);
    }

    internal RuntimeObject ResolveRuntimeObject(object target) {
      if (target == null) {
        return null;
      }
      if (target is EcmaValue value) {
        target = value.GetUnderlyingObject();
      }
      if (target is RuntimeObject runtimeObject) {
        if (runtimeObject.Realm == this) {
          return runtimeObject;
        }
        if (TryResolveRuntimeObjectInRealm(runtimeObject, out RuntimeObject sharedObject)) {
          return sharedObject;
        }
        return runtimeObject.Clone(this);
      }
      return nativeWrappers.GetOrAdd(NativeObject.Create(target));
    }

    internal RuntimeObject ResolveRuntimeObjectInRealm(RuntimeObject sourceObj) {
      if (TryResolveRuntimeObjectInRealm(sourceObj, out RuntimeObject sharedObject)) {
        return sharedObject;
      }
      throw new ArgumentException("Object is not shared", "souceObj");
    }

    internal bool TryResolveRuntimeObjectInRealm(RuntimeObject sourceObj, out RuntimeObject runtimeObject) {
      Guard.ArgumentNotNull(sourceObj, "sourceObj");
      if (sourceObj.Realm.objectHandles.TryGetValue(sourceObj, out SharedObjectHandle handle)) {
        runtimeObject = GetRuntimeObject(handle);
        return true;
      }
      runtimeObject = null;
      return false;
    }

    internal static SharedObjectHandle GetSharedObjectHandle(Enum enumValue) {
      Guard.ArgumentNotNull(enumValue, "enumValue");
      if (!enumTypes.TryGetValue(enumValue.GetType(), out RuntimeModule module)) {
        throw new ArgumentException("Value must be a enum and the enum type must be registered", "enumValue");
      }
      return module.GetSharedObjectHandle(enumValue);
    }

    public void Execute(Action action) {
      if (this == sharedRealm) {
        throw new InvalidOperationException("Execution in shared realm is disallowed");
      }
      if (Thread.CurrentThread != this.ExecutionContext.Thread) {
        throw new InvalidOperationException("Execution in another thread is disallowed");
      }
      RuntimeRealm previous = current;
      try {
        current = this;
        action();
      } finally {
        current = previous;
      }
    }

    public void Enqueue(Action action) {
      this.ExecutionContext.Enqueue(this, action, 0, RuntimeExecutionFlags.None);
    }

    public void Enqueue(Action action, Task task) {
      this.ExecutionContext.Enqueue(this, action, task);
    }

    public RuntimeExecutionHandle Enqueue(Action action, int milliseconds, RuntimeExecutionFlags flags) {
      return this.ExecutionContext.Enqueue(this, action, milliseconds, flags);
    }

    public void Dispose() {
      if (!this.Disposed) {
        try {
          BeforeDisposed(this, EventArgs.Empty);
        } finally {
          if (current == this) {
            current = null;
          }
          this.Disposed = true;
        }
      }
    }

    private RuntimeObject GetSharedObject(int i, int j) {
      if (i == 0 && j == 0) {
        return null;
      }
      if (i >= sharedObjects.Length) {
        throw new ArgumentOutOfRangeException("index");
      }
      if (i >= intrinsics.Length && intrinsics.Length != sharedObjects.Length) {
        Array.Resize(ref intrinsics, sharedObjects.Length);
      }
      RuntimeObject[] arr = intrinsics[i];
      if (arr == null) {
        arr = new RuntimeObject[sharedObjects[i].Length];
        intrinsics[i] = arr;
      }
      if (j >= arr.Length) {
        throw new ArgumentOutOfRangeException("index");
      }
      RuntimeObject obj = arr[j];
      if (obj == null) {
        if (this != sharedRealm) {
          obj = sharedObjects[i][j].CloneSlim(this);
          objectHandles.Add(obj, new SharedObjectHandle(i, j));
        } else {
          obj = new EcmaObject();
        }
        arr[j] = obj;
      }
      return obj;
    }

    internal class SharedObjectContainer : ISharedObjectContainer, IDisposable {
      private readonly List<RuntimeObject> definedObj = new List<RuntimeObject>();
      private readonly bool previousUseSharedRealm = useSharedRealm;
      private readonly int initialLength;
      private bool flushed;

      public SharedObjectContainer()
        : this(0) { }

      public SharedObjectContainer(int initialLength) {
        if (!useSharedRealm) {
          Monitor.Enter(syncRoot);
        }
        useSharedRealm = true;
        this.initialLength = initialLength;
        this.ID = sharedObjects.Length;
        Array.Resize(ref sharedObjects, this.ID + 1);
        sharedRealm.intrinsics = sharedObjects;
        sharedObjects[this.ID] = new RuntimeObject[initialLength];
      }

      public SharedObjectContainer(RuntimeModule module)
        : this(GetInitialObjectCount(module)) {
        enumTypes[module.EnumType] = module;
        module.Init(this, sharedObjects[this.ID]);
      }

      public int ID { get; }

      public SharedObjectHandle Add(RuntimeObject obj) {
        Guard.ArgumentNotNull(obj, "obj");
        if (obj.Realm != sharedRealm) {
          throw new ArgumentException("Object must be created in the shared realm", "obj");
        }
        if (flushed) {
          throw new InvalidOperationException("Object can no longer be added to a freezed container");
        }
        int sharedIndex = definedObj.IndexOf(obj);
        if (sharedIndex < 0) {
          sharedIndex = definedObj.Count;
          definedObj.Add(obj);
        }
        return new SharedObjectHandle(this.ID, sharedIndex + initialLength);
      }

      public RuntimeObject[] FlushObjects() {
        if (!flushed) {
          flushed = true;
          Array.Resize(ref sharedObjects[this.ID], initialLength + definedObj.Count);
          RuntimeObject[] arr = sharedObjects[this.ID];
          definedObj.CopyTo(arr, initialLength);
          for (int i = 0, len = arr.Length; i < len; i++) {
            if (arr[i] != null) {
              sharedRealm.objectHandles.Add(arr[i], new SharedObjectHandle(this.ID, i));
            }
          }
          definedObj.Clear();
        }
        return sharedObjects[this.ID];
      }

      private static int GetInitialObjectCount(RuntimeModule module) {
        Guard.ArgumentNotNull(module, "module");
        Type enumType = module.EnumType;
        if (!enumType.IsEnum || !enumType.HasAttribute(out IntrinsicObjectEnumAttribute attr)) {
          throw new ArgumentException("Type must be a enum with IntrinsicObjectEnumAttribute", "enumType");
        }
        if (enumTypes.ContainsKey(enumType)) {
          throw new InvalidOperationException(String.Format("Type {0} has been registered", enumType.Name));
        }
        return attr.ObjectCount;
      }

      void IDisposable.Dispose() {
        try {
          FlushObjects();
        } finally {
          if (!previousUseSharedRealm) {
            useSharedRealm = false;
            Monitor.Exit(syncRoot);
          }
        }
      }
    }
  }
}
