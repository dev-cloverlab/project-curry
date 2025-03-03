using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using curry.Common;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace curry.Sound
{
    public class BgmManager : SingletonMonoBehaviour<BgmManager>
    {
        public const int SplitMax = 20;
        public const float MaxVolume = 0.2f;

        [SerializeField]
        protected BgmPlayer m_BgmPlayer;
        public BgmPlayer Player => m_BgmPlayer;

        public new static void Create()
        {
            if (IsInstance())
            {
                return;
            }

            var prefab = HoldAssetLoadManager.Instance.LoadPrefab("BgmManager");
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            instance.name = "BgmManager";
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

