using System;
using System.Linq.Expressions;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEditor.Rendering.PostProcessing
{
    ///     <summary>
        ///     The class to inherit from when designing custom effect editors.
        ///     </summary>
        ///     <typeparam name="T">The effect type to create an editor for</typeparam>
            public class PostProcessEffectEditor<T> : PostProcessEffectBaseEditor
        where T : PostProcessEffectSettings
    {
        /// <!-- Badly formed XML comment ignored for member "M:UnityEditor.Rendering.PostProcessing.PostProcessEffectEditor`1.FindProperty``1(System.Linq.Expressions.Expression{System.Func{`0,``0}})" -->
                        protected SerializedProperty FindProperty<TValue>(Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty(RuntimeUtilities.GetFieldPath(expr));
        }

        /// <!-- Badly formed XML comment ignored for member "M:UnityEditor.Rendering.PostProcessing.PostProcessEffectEditor`1.FindParameterOverride``1(System.Linq.Expressions.Expression{System.Func{`0,``0}})" -->
                        protected SerializedParameterOverride FindParameterOverride<TValue>(Expression<Func<T, TValue>> expr)
        {
            var property = serializedObject.FindProperty(RuntimeUtilities.GetFieldPath(expr));
            var attributes = RuntimeUtilities.GetMemberAttributes(expr);
            return new SerializedParameterOverride(property, attributes);
        }
    }
}
