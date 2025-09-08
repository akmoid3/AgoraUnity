using UnityEngine;

public class AudioManager : MonoBehaviour

{
    public static AudioManager instance;
    public AudioSource audioSource;
    public void Start()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }
        
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot(AudioClip clip)
    {
        if(clip != null)
            audioSource.PlayOneShot(clip);
    }

}
