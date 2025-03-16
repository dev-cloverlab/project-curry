using UnityEngine;
using UnityEngine.EventSystems;

namespace Common
{
    /// <summary>
    /// 特定のボタンの上にマウスカーソルがあるかを判定
    /// </summary>
    public class ButtonHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isHovered = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
        }

        public bool IsMouseOver()
        {
            return isHovered;
        }
    }
}
