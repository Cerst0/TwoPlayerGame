using UnityEngine;

public class SceneBrain : MonoBehaviour
{
    private void Awake()
    {
        Setup();
    }

    private void OnLevelWasLoaded(int level)
    {
        Setup();
    }

    void Setup()
    {
        LeanTween.cancelAll();
        LeanTween.reset();
    }
}
