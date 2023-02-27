using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingFade : LoadingOption
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime = 0.2f;

    protected override bool LoadingStart()
    {
        fadeImage.color = fadeImage.color + Color.black * Time.deltaTime * (1.0f / fadeTime);
        return fadeImage.color.a < 1;
    }

    protected override bool LoadingEnd()
    {
        fadeImage.color = fadeImage.color - Color.black * Time.deltaTime * (1.0f / fadeTime);
        return fadeImage.color.a > 0;
    }

    protected override void LoadingUpdate(AsyncOperation operation)
    {
    }
}
