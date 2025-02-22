using System;
using UnityEngine;

namespace curry.Common
{
    public interface ISingletonMonoBehaviour
    {
        void DontDestroyOnLoad();
        void DestroySelf();
    }

    public class SingletonMonoBehaviour<T> : MonoBehaviour, ISingletonMonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindAnyObjectByType(typeof(T));

                    if (instance == null)
                    {
                        Debug.Log(typeof(T) + "is nothing");
                    }
                }

                return instance;
            }
        }

        public static void DestroyInstance()
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        public void DestroySelf()
        {
            if (!IsThisInstance())
            {
                return;
            }
            Destroy(instance.gameObject);
            instance = null;
        }

        /// <summary>
        /// Instanceがあるか調べる
        /// </summary>
        /// <returns><c>true</c> if is instance; otherwise, <c>false</c>.</returns>
        public static bool IsInstance()
        {
            return instance != null;
        }

        public bool IsThisInstance()
        {
            return (Instance == this);
        }

        virtual protected void Awake()
        {
            CheckInstance();
        }

        virtual protected void OnDestroy()
        {
            if (IsThisInstance())
            {
                instance = null;
            }
        }

        public void DontDestroyOnLoad()
        {
            if (Instance == this)
            {
                DontDestroyOnLoad(this);
            }
        }

        protected bool CheckInstance()
        {
            if (instance == null)
            {
                instance = (T)this;
                return true;
            }
            else if (instance == this)
            {
                return true;
            }

            {
                Destroy(this.gameObject);
            }

            return false;
        }

        public static void Create()
        {
            if (IsInstance())
            {
                return;
            }

            new GameObject(typeof(T).Name, typeof(T));
        }
    }

    //===========================================================================
    //! ただのシングルトン
    //===========================================================================
    public class Singleton<T> where T : class, new()
    {
        private static T m_Instance = null;
        //!< インスタンス.

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new T() { };
                }
                return m_Instance;
            }
        }

        void OnDestroy()
        {
            if (m_Instance == this)
            {

                Debug.Log("Delete resource memory");
                m_Instance = null;
            }
        }
    }
}