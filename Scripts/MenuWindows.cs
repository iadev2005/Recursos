using UnityEngine;

public class MenuWindows : MonoBehaviour
{

    public void OpenWindow(CanvasGroup windowCanvas)
    {
        Menu.ins.windowOpen = true;
        Menu.ins.animating = true;
        Menu.ins.FadeIn(windowCanvas);
        StartCoroutine(Menu.ins.FadeOut(Menu.ins.mainMenuCanvas));
    }

    public void CloseWindow(CanvasGroup windowCanvas)
    {
        Menu.ins.animating = true;
        Menu.ins.FadeIn(Menu.ins.mainMenuCanvas);
        StartCoroutine(Menu.ins.FadeOut(windowCanvas));
        Menu.ins.windowOpen = false;
    }

}