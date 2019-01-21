using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace UnityEngine.Rendering.PostProcessing
{
    /// <!-- Badly formed XML comment ignored for member "T:UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings" -->
            [Serializable]
    public class PostProcessEffectSettings : ScriptableObject
    {
        ///     <summary>
                ///     The active state of the set of parameter defined in this class.
                ///     </summary>
                ///     <seealso cref="F:UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings.enabled"/>
                        public bool active = true;

        /// <!-- Badly formed XML comment ignored for member "F:UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings.enabled" -->
                        public BoolParameter enabled = new BoolParameter { overrideState = true, value = false };

        internal ReadOnlyCollection<ParameterOverride> parameters;

        void OnEnable()
        {
            // Automatically grab all fields of type ParameterOverride for this instance
            parameters = GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t.FieldType.IsSubclassOf(typeof(ParameterOverride)))
                .OrderBy(t => t.MetadataToken) // Guaranteed order
                .Select(t => (ParameterOverride)t.GetValue(this))
                .ToList()
                .AsReadOnly();

            foreach (var parameter in parameters)
                parameter.OnEnable();
        }

        void OnDisable()
        {
            if (parameters == null)
                return;

            foreach (var parameter in parameters)
                parameter.OnDisable();
        }

        /// <!-- Badly formed XML comment ignored for member "M:UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings.SetAllOverridesTo(System.Boolean,System.Boolean)" -->
                        public void SetAllOverridesTo(bool state, bool excludeEnabled = true)
        {
            foreach (var prop in parameters)
            {
                if (excludeEnabled && prop == enabled)
                    continue;

                prop.overrideState = state;
            }
        }
        
        /// <!-- Badly formed XML comment ignored for member "M:UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings.IsEnabledAndSupported(UnityEngine.Rendering.PostProcessing.PostProcessRenderContext)" -->
                        public virtual bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }

        ///     <summary>
                ///     Returns the computed hash code for this parameter.
                ///     </summary>
                ///     <returns>A computed hash code</returns>
                        public int GetHash()
        {
            // Custom hashing function used to compare the state of settings (it's not meant to be
            // unique but to be a quick way to check if two setting sets have the same state or not).
            // Hash collision rate should be pretty low.
            unchecked
            {
                //return parameters.Aggregate(17, (i, p) => i * 23 + p.GetHash());

                int hash = 17;

                foreach (var p in parameters)
                    hash = hash * 23 + p.GetHash();

                return hash;
            }
        }
    }
}
