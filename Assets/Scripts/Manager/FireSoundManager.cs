using UnityEngine;
using curry.Common;

namespace curry.Sound
{
    public class FireSoundManager : SingletonMonoBehaviour<FireSoundManager>
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

            var prefab = HoldAssetLoadManager.Instance.LoadPrefab("FireSoundManager");
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "FireSoundManager";
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
