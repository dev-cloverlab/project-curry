using UnityEngine;

namespace Extensions
{
    public static class RectTransformExtensions
    {
        private static Vector2 vector2;

        public static void SetSizeDelta( this RectTransform rectTransform, float x, float y ){
            vector2.Set (x, y);
            rectTransform.sizeDelta = vector2;
        }
        public static void SetSizeDeltaWidth( this RectTransform rectTransform, float x ){
            vector2.Set (x, rectTransform.sizeDelta.y );
            rectTransform.sizeDelta = vector2;
        }
        public static void SetSizeDeltaHeight( this RectTransform rectTransform, float y ){
            vector2.Set (rectTransform.sizeDelta.x, y);
            rectTransform.sizeDelta = vector2;
        }
    }
}
