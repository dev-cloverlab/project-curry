using System.Collections.Generic;
using System.Collections.ObjectModel;
using curry.Common;
using curry.Enums;
using curry.Localization;
using curry.Sound;
using curry.UserData;
using curry.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace curry.UI
{
    public class SettingUI : UIBase
    {
        [SerializeField]
        private Slider m_EnvVolumeSlider;

        [SerializeField]
        private Slider m_SEVolumeSlider;

        [SerializeField]
        private Toggle[] m_LanguageToggles;

        [SerializeField]
        private Button m_CloseButton;

        private OptionData m_OptionData;

        private const int kJapaneseToggleIdx = 0;
        private const int kEnglishToggleIdx = 1;

        private bool m_IsDirty;

        private ReadOnlyDictionary<int, LocalizationLocaleType> m_LanguageIds =
            new (new Dictionary<int, LocalizationLocaleType>
                {
                    { kJapaneseToggleIdx, LocalizationLocaleType.ja },
                    { kEnglishToggleIdx, LocalizationLocaleType.en },
                });

        private void Awake()
        {
            m_OptionData = UserDataManager.Instance.Option;

            // ===== Volume Control =====
            m_EnvVolumeSlider.onValueChanged.AddListener( (val) => SetEnvVolume(Mathf.RoundToInt(val)));
            m_SEVolumeSlider.onValueChanged.AddListener((val) => SetSeVolume(Mathf.RoundToInt(val)));

            var envVolume = m_OptionData.m_EnvVolume;
            var seVolume = m_OptionData.m_SEVolume;

            m_EnvVolumeSlider.value = envVolume;
            m_SEVolumeSlider.value = seVolume;

            // ===== Language Setting =====
            var isJa = m_OptionData.m_Language == LocalizationLocaleType.ja;
            m_LanguageToggles[kJapaneseToggleIdx].SetIsOnWithoutNotify(isJa);
            m_LanguageToggles[kEnglishToggleIdx].SetIsOnWithoutNotify(!isJa);

            for (var i = 0; i < m_LanguageToggles.Length; i++)
            {
                if (i == kJapaneseToggleIdx)
                {
                    m_LanguageToggles[i].onValueChanged.AddListener(SetLanguageJapanese);
                }
                else
                {
                    m_LanguageToggles[i].onValueChanged.AddListener(SetLanguageEnglish);
                }
            }

            // ===== Button Event =====
            m_CloseButton.onClick.RemoveAllListeners();
            m_CloseButton.onClick.AddListener(Close);
        }

        public void Setup()
        {

        }

        private void Update()
        {
            if (!IsActivate)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void SetEnvVolume(int value)
        {
            m_OptionData.m_EnvVolume = value;

            BgmManager.Instance.Player.VolumeSet(m_OptionData.m_EnvVolume);
            EnvSoundManager.Instance.Player.VolumeSet(m_OptionData.m_EnvVolume);
            FireSoundManager.Instance.Player.VolumeSet(m_OptionData.m_EnvVolume);

            m_IsDirty = true;
        }

        private void SetSeVolume(int value)
        {
            m_OptionData.m_SEVolume = value;

            SEPlayer.VolumeSet(m_OptionData.m_SEVolume);

            m_IsDirty = true;
        }

        private void SetLanguageJapanese(bool val)
        {
            if (!val)
            {
                return;
            }

            SetToggle(kJapaneseToggleIdx);
        }

        private void SetLanguageEnglish(bool val)
        {
            if (!val)
            {
                return;
            }

            SetToggle(kEnglishToggleIdx);
        }

        private void SetToggle(int idx)
        {
            if (m_LanguageIds.TryGetValue(idx, out LocalizationLocaleType value))
            {
                m_OptionData.m_Language = value;
                LocalizationUtility.SetLocale( m_OptionData.m_Language );
                LocalizationTextManager.ChangeText();
                m_IsDirty = true;
            }
            else
            {
                DebugLogWrapper.LogAssertion($"Invalid Language");
            }
        }

        public void Close()
        {
            SEPlayer.PlaySelectSE();
            CloseAsync(false).Forget();
        }

        private async UniTask CloseAsync(bool noSave)
        {
            Activate(false);

            if (!noSave && m_IsDirty)
            {
                UserDataManager.Instance.Option = m_OptionData;
                LocalizationUtility.SetLocale( UserDataManager.Instance.Option.m_Language );
                LocalizationTextManager.ChangeText();
                await SaveLoadManager.Instance.Save();
            }
            else
            {
                LocalizationTextManager.ChangeText();
            }

            CloseAnimation();
        }

        public void CloseNoSave()
        {
            CloseAsync(true).Forget();
        }
    }
}
