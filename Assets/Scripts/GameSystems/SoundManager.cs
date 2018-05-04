using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*By Andreas Nilsson*/

public class SoundManager : MonoBehaviour, IPausable
{
    [SerializeField]
    AudioSource efxSource, musicSource, environmentSource;

    public static SoundManager instance = null;

    [SerializeField]
    float lowPitchedRange = 0.95f, highPitchedRange = 1.05f;

	// Use this for initialization
	void Start ()
    {
        FindObjectOfType<PauseManager>().Pausables.Add(this);
		if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
	}

    public void PauseMe(bool pausing)
    {
        if (pausing)
        {
            efxSource.Pause();
            musicSource.Pause();
            environmentSource.Pause();
        }
        else
        {
            efxSource.UnPause();
            musicSource.UnPause();
            environmentSource.UnPause();
        }
    }

    public void PlaySingle(AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }

    public void RandomizeSfx(params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchedRange, highPitchedRange);

        efxSource.pitch = randomPitch;
        efxSource.clip = clips[randomIndex];
        efxSource.PlayOneShot(efxSource.clip);
    }
}
