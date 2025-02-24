using System;
using System.Linq;
using UnityEngine;

namespace kod.Common
{
    /// <summary>
    /// The setting for the canvas scaler adjuster
    /// </summary>
    [CreateAssetMenu(
        fileName = "CanvasScalerAdjusterSetting",
        menuName = "Custom/Canvas Scaler Adjuster")]
    public class CanvasScalerAdjusterSetting : ScriptableObject
    {
        [SerializeField]
        private Vector2Int _targetResolution = new(1440, 2560);
        [SerializeField]
        [Range(0, 1)]
        private float _defaultMatchValue;
        [SerializeField]
        private ScalerSetting[] _scalerSettings;

        public Vector2Int targetResolution => _targetResolution;

        /// <summary>
        /// Get the target match value
        /// </summary>
        /// <param name="screenRatio">The screen ratio of height to width</param>
        /// <returns>
        /// The matched match value. If the specified screen ratio is higher than all the
        /// ratio in the settings, it will return the default match value.
        /// </returns>
        public float GetMatchValue(float screenRatio)
        {
            var setting = _scalerSettings.FirstOrDefault(x =>
                x.ScreenRatioThreshold > screenRatio);

            return setting?.ScalerMatchValue ?? _defaultMatchValue;
        }

        #region Data Class

        [Serializable]
        public class ScalerSetting
        {
            [SerializeField]
            [Tooltip("The threshold of screen ratio of height to width")]
            public float ScreenRatioThreshold;
            [SerializeField]
            [Range(0, 1)]
            [Tooltip("The screen ratio below the threshold will use this value to set " +
                     "the match of the canvas scaler")]
            public float ScalerMatchValue;
        }

        #endregion
    }
}

