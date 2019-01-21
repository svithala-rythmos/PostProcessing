using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.PostProcessing
{
    ///     <summary>
        ///     This class holds settings for the Fast Approximate Anti-aliasing (FXAA) effect.
        ///     </summary>
            [Serializable]
    public sealed class FastApproximateAntialiasing
    {
        /// <!-- Badly formed XML comment ignored for member "F:UnityEngine.Rendering.PostProcessing.FastApproximateAntialiasing.fastMode" -->
                        [FormerlySerializedAs("mobileOptimized")]
        [Tooltip("Boost performances by lowering the effect quality. This setting is meant to be used on mobile and other low-end platforms but can also provide a nice performance boost on desktops and consoles.")]
        public bool fastMode = false;

        /// <!-- Badly formed XML comment ignored for member "F:UnityEngine.Rendering.PostProcessing.FastApproximateAntialiasing.keepAlpha" -->
                        [Tooltip("Keep alpha channel. This will slightly lower the effect quality but allows rendering against a transparent background.")]
        public bool keepAlpha = false;
    }
}
