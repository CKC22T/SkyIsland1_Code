using CulterLib.UI.Popups;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutPopup : PopupWindow
{
    public static FadeOutPopup Instance { get; private set; }

    #region Inspector
    [TabGroup("Component"), SerializeField] Image m_FadeImage;
    #endregion

    #region Param
    [SerializeField] private float fadeTime = 0.2f;
    #endregion

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }

    protected override void OnStartOpen(string _opt)
    {
        base.OnStartOpen(_opt);

        m_FadeImage.color = Color.black;

        StartCoroutine(fadeOut());
        IEnumerator fadeOut()
        {
            while (m_FadeImage.color.a > 0)
            {
                m_FadeImage.color = m_FadeImage.color - Color.black * Time.deltaTime * (1.0f / fadeTime);
                yield return null;
            }
            Close();
        }
    }
    #endregion
}
