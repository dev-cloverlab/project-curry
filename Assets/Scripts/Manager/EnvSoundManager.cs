using UnityEngine;
using curry.Common;

namespace curry.Sound
{
    public class EnvSoundManager : SingletonMonoBehaviour<EnvSoundManager>
    {
        [SerializeField]
        protected BgmPlayer m_BgmPlayer;
        public BgmPlayer Player => m_BgmPlayer;

        public new static void Create()
        {
            if (IsInstance())
            {
                return;
            }

            var prefab = HoldAssetLoadManager.Instance.LoadPrefab("EnvSoundManager");
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "EnvSoundManager";
        }

        protected new void Awake()
        {
            base.Awake();

            if (!IsThisInstance())
            {
                return;
            }

            DontDestroyOnLoad();
        }
    }
}
