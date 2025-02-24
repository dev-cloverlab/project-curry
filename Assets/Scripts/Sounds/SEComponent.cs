using curry.Sound;
using UnityEngine;

namespace curry.Common
{
    /// <summary>
    /// アニメーション等、ソースコード以外からSEを呼び出す用
    /// </summary>
    public class SEComponent : MonoBehaviour
    {
        [SerializeField]
        private bool m_PlayOnAwake;

        [SerializeField]
        private bool m_PlayOnEnable;

        [SerializeField]
        private string m_AssetName;

        private AudioSource m_playAudioSource;

        private void Awake()
        {
            if (m_PlayOnAwake)
            {
                PlaySE(m_AssetName);
            }
        }

        private void OnEnable()
        {
            if (m_PlayOnEnable)
            {
                PlaySE(m_AssetName);
            }
        }

        public void PlaySE(string assetName)
        {
            SEPlayer.Play(assetName);
        }

        public void PlayLoopSE(string assetName)
        {
            m_playAudioSource = SEPlayer.Play(assetName,true);
        }
        public void StopSE()
        {
            if( null != m_playAudioSource ) {
                SEPlayer.Stop( m_playAudioSource );
                m_playAudioSource = null;
            }
        }
    }
}
