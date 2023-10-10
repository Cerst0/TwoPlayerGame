using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Sprite[] GamePreviews;

    bool disableLoadingScreen;

    CanvasGroup group;
    Image image;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        image = transform.GetChild(1).gameObject.GetComponent<Image>();

        group.alpha = 0;
    }

    private void FixedUpdate()
    {
        if (disableLoadingScreen)
        {
            group.alpha -= .15f;
            disableLoadingScreen = group.alpha != 0 ? true : false;
        }
    }

    public void SetLoadingScreen(int index)
    {
        disableLoadingScreen = false;
        LeanTween.cancel(gameObject);
        LeanTween.alphaCanvas(group, 1, .1f).setEaseInOutQuad().setIgnoreTimeScale(true);

        if (index < 0)
        {
            image.sprite = null;
        }
        else { image.sprite = GamePreviews[index]; }

        if (index == -2)
        {
            Debug.LogWarning("AddPartyLoadingScreenImage");
        }
    }
    public void DisableLoadingScreen()
    {
        disableLoadingScreen = true;
        LeanTween.cancel(gameObject);
    }
}
