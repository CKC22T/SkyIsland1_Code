using CulterLib.UI.Popups;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BookUI : PopupWindow
{
    public static BookUI Instance { get; private set; }

    #region Event
    protected override void OnInitSingleton()
    {
        base.OnInitSingleton();
        Instance = this;
    }

    //EventTrigger Event
    public void OnNext(BaseEventData _data)
    {
        if (BookOpener.Instance.IsOpened)
        {
            if (!BookOpener.Instance.IsDelay)
                BookOpener.Instance.NextPage();
            else
                BookOpener.Instance.Fast();
        }
    }
    public void OnSkip(BaseEventData _data)
    {
        BookOpener.Instance.Skip();
    }
    #endregion
}
