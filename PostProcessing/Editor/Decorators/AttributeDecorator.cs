using System;
using UnityEngine;

namespace UnityEditor.Rendering.PostProcessing
{
    ///     <summary>
        ///     The base abstract class for all attribute decorators.
        ///     </summary>
            public abstract class AttributeDecorator
    {
        /// <!-- Badly formed XML comment ignored for member "M:UnityEditor.Rendering.PostProcessing.AttributeDecorator.IsAutoProperty" -->
                        public virtual bool IsAutoProperty()
        {
            return true;
        }

        /// <!-- Badly formed XML comment ignored for member "M:UnityEditor.Rendering.PostProcessing.AttributeDecorator.OnGUI(SerializedProperty,SerializedProperty,GUIContent,System.Attribute)" -->
                        public abstract bool OnGUI(SerializedProperty property, SerializedProperty overrideState, GUIContent title, Attribute attribute);
    }
}
