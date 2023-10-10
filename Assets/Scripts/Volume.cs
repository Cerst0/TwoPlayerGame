using UnityEngine;

public class Volume : MonoBehaviour
{
    public bool isMusic;
    float volume;

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<AudioSource>().volume;
    }

    void Update()
    // Update is called once per frame
    {
        if (isMusic) { GetComponent<AudioSource>().volume = (volume * PlayerPrefs.GetFloat("MusicVolume")) * PlayerPrefs.GetFloat("GeneralVolume"); }
        else { GetComponent<AudioSource>().volume = (volume * PlayerPrefs.GetFloat("SoundVolume")) * PlayerPrefs.GetFloat("GeneralVolume"); }
    }
}
