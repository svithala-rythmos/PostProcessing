using System;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEditor.Rendering.PostProcessing
{
    /// <!-- Badly formed XML comment ignored for member "T:UnityEditor.Rendering.PostProcessing.BaseEditor`1" -->
            public class BaseEditor<T> : Editor
        where T : MonoBehaviour
    {
        ///     <summary>
                ///     The target component.
                ///     </summary>
                        protected T m_Target
        {
            get { return (T)target; }
        }

        ///     <summary>
                ///     </summary>
                ///     <typeparam name="TValue"></typeparam>
                ///     <param name="expr"></param>
                ///     <returns></returns>
                        protected SerializedProperty FindProperty<TValue>(Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty(RuntimeUtilities.GetFieldPath(expr));
        }
    }
}
