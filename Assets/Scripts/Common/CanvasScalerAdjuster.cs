using UnityEngine;
using UnityEngine.UI;

namespace kod.Common
{

    /// <summary>
    /// The canvas ratio adjuster
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerAdjuster : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private CanvasScaler _canvasScaler;
        [SerializeField]
        private CanvasScalerAdjusterSetting _setting;
        [SerializeField]
        private bool _setupAtAwake = true;

        private void Reset()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
        }

        private void Awake()
        {
            if (_setupAtAwake)
                SetUpScaler();
        }

        /// <summary>
        /// Set up the match value of scaler according to the size of screen
        /// </summary>
        public void SetUpScaler()
        {
            Debug.Assert(
                _setting != null, "The setting is not specified.");
            var ratio = (float)Screen.height / Screen.width;
            _canvasScaler.referenceResolution = _setting.targetResolution;
            _canvasScaler.matchWidthOrHeight = _setting.GetMatchValue(ratio);
        }
    }
}
