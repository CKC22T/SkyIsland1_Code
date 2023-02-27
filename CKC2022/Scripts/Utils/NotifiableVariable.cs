using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.LowLevel;

namespace Utils
{
    public interface INotifiable<T>
    {
        public T Value { get; }

        //call when Update
        public event Action OnChanged;
        public event Action<T> OnDataChanged;
        public event Action<T> OnDataChangedOnce;
        public event Action<T, T> OnDataChangedDelta;

        //call by Mono
        public event Action<T> OnUpdateNotify;
    }

    [Serializable]
    public class Notifier<T> : INotifiable<T>
    {
        [SerializeField]
        private T value;

        public T Value
        {
            get => value;
            set => Set(value, true);
        }


        private static EqualityComparer<T> defaultComparer = EqualityComparer<T>.Default;

        private EqualityComparer<T> overrideComparer = null;
        private EqualityComparer<T> OverrideComparer { get => overrideComparer ?? defaultComparer; }


        public virtual void Set(in T value, bool notify = true)
        {
            if (!OverrideComparer.Equals(this.value, value))
            {
                var lastData = this.value;
                this.value = value;

                if (notify)
                {
                    OnChanged?.Invoke();
                    OnDataChanged?.Invoke(this.value);
                    OnDataChangedOnce?.Invoke(this.value);
                    OnDataChangedDelta?.Invoke(lastData, this.value);
                    OnDataChangedOnce = null;
                }
            }
        }

        //call when Update
        public event Action OnChanged;
        public event Action<T> OnDataChanged;
        public event Action<T> OnDataChangedOnce;
        public event Action<T, T> OnDataChangedDelta;

        //call by Mono
        public event Action<T> OnUpdateNotify;

        public bool IsSubscribed => (OnDataChanged != null && OnDataChanged.GetInvocationList().Length > 0) || (OnDataChangedOnce != null && OnDataChangedOnce.GetInvocationList().Length > 0);
        public virtual bool IsDirty { get; protected set; }

        public int MonoNotifyMask { get; set; } = 0;

        public Notifier(in Action onChanged = null, in Action<T> changed = null)
        {
            OnChanged = onChanged;
            OnDataChanged = changed;
            OnDataChanged += SetDirty;

            void SetDirty(T next)
            {
                IsDirty = true;

                if (MonoNotifyMask != 0)
                {
                    MonoNotifierEventRiser.Instance.Add(OnMonoUpdateCall, (MonoNotifierEventRiser.InvokeType)MonoNotifyMask);
                }

                void OnMonoUpdateCall()
                {
                    OnUpdateNotify?.Invoke(Value);
                    MonoNotifierEventRiser.Instance.Remove(OnMonoUpdateCall);
                }
            }
        }

        public Notifier(in T value) : this()
        {
            this.value = value;
        }

        public Notifier(in T value, in EqualityComparer<T> overrideComparer) : this(value)
        {
            this.overrideComparer = overrideComparer;
        }

        public bool GetDirtyValue(out T value)
        {
            value = this.value;
            return IsDirty;
        }

        public void SetPristine()
        {
            IsDirty = false;
        }

        public override string ToString() => value.ToString();
    }

    public static class NotifierExtension
    {
        public static bool GetDirtyAndClear<T>(this Notifier<T> notifier, out T value)
        {
            var isDirty = notifier.GetDirtyValue(out value);
            notifier.SetPristine();

            return isDirty;
        }

        public static void SetMonoEvent<T>(this Notifier<T> notifier, MonoNotifierEventRiser.InvokeType type)
        {
            notifier.MonoNotifyMask = (int)type;
        }

        public static void AddMonoEvent<T>(this Notifier<T> notifier, MonoNotifierEventRiser.InvokeType type)
        {
            notifier.MonoNotifyMask |= (int)type;
        }
        public static void RemoveMonoEvent<T>(this Notifier<T> notifier, MonoNotifierEventRiser.InvokeType type)
        {
            notifier.MonoNotifyMask &= ~(int)type;
        }
        public static void ClearMonoEvent<T>(this Notifier<T> notifier)
        {
            notifier.MonoNotifyMask = 0;
        }
    }


    public class MonoNotifierEventRiser : MonoSingleton<MonoNotifierEventRiser>
    {
        [Flags]
        public enum InvokeType
        {
            None = 0,
            Update = 1 << 0,
            FixedUpdate = 1 << 1,
            LateUpdate = 1 << 2,
        }

        #region Value
        private readonly List<List<Action>> InvocationList = new List<List<Action>>
        {
            new List<Action>(),
            new List<Action>(),
            new List<Action>(),
        };

        private List<Action> UpdateInvocationList { get => InvocationList[0]; }
        private List<Action> FixedUpdateInvocationList { get => InvocationList[1]; }
        private List<Action> LateUpdateInvocationList { get => InvocationList[2]; }
        #endregion

        protected override void Initialize()
        {
            base.Initialize();

            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene unloaded)
        {
            CheckAndClearList(UpdateInvocationList);
            CheckAndClearList(FixedUpdateInvocationList);
            CheckAndClearList(LateUpdateInvocationList);
        }

        private void CheckAndClearList(in List<Action> invocationList)
        {
            var removeTargets = new List<Action>();

            foreach (var invocation in invocationList)
            {
                if (invocation.Target == null)
                    removeTargets.Add(invocation);
            }

            var deletion = invocationList.RemoveAll((invocation) => removeTargets.Contains(invocation));
            Debug.Log("Removed : \n" + string.Join(", ", removeTargets.Select((target) => target.Method.Name)));
        }

        #region Event

        internal void Init()
        {
        }

        //Unity Event
        private void Update()
        {
            InvokeAndClear(UpdateInvocationList);
        }

        private void FixedUpdate()
        {
            InvokeAndClear(FixedUpdateInvocationList);
        }

        private void LateUpdate()
        {
            InvokeAndClear(LateUpdateInvocationList);
        }

        private void InvokeAndClear(in List<Action> invocationList)
        {
            var invocations = invocationList.ToArray();
            invocationList.Clear();

            foreach (var invocation in invocations)
            {
                try
                {
                    invocation();
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventManager.Update Failed : {e.Message}\n{e.StackTrace}");
                }
            }
        }
        #endregion

        #region Function
        //Internal
        /// <summary>
        /// 호출할 이벤트를 추가합니다.
        /// </summary>
        /// <param name="_obsrv"></param>
        /// <param name="e"></param>
        internal void Add(Action e, InvokeType type = InvokeType.Update)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (type.HasFlag((InvokeType)(1 << i)))
                {
                    InvocationList[i].AddWhenNotContains(e);
                }
            }
        }

        /// <summary>
        /// 호출하려던 이벤트를 제거합니다.
        /// </summary>
        /// <param name="e"></param>
        internal void Remove(Action e, InvokeType type = InvokeType.Update | InvokeType.FixedUpdate | InvokeType.LateUpdate)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (type.HasFlag((InvokeType)(1 << i)))
                {
                    InvocationList[i].Remove(e);
                }
            }
        }
        #endregion

    }
}