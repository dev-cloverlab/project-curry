using curry.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace curry.UI
{
    public class CreditsUI : UIBase
    {
        [SerializeField]
        private Scrollbar m_Scrollbar;

        [SerializeField]
        private Button m_CloseButton;

        private void Awake()
        {
            // ===== Button Event =====
            m_CloseButton.onClick.RemoveAllListeners();
            m_CloseButton.onClick.AddListener(Close);
        }

        public void OnEnable()
        {
            m_Scrollbar.value = 1.0f;
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

        public void Close()
        {
            SEPlayer.PlaySelectSE();
            Activate(false);
            CloseAnimation();
        }
    }
}
