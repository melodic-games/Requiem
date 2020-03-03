using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;
    public Sound theme;

    public static AudioManager instance;

    private void Awake()
    {

        if (instance == null)
        {

            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source =  gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        theme.source = gameObject.AddComponent<AudioSource>();
        theme.source.clip = theme.clip;

        theme.source.volume = theme.volume;
        theme.source.pitch = theme.pitch;
        theme.source.loop = theme.loop;

    }

    private void Start()
    {
        
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    void Update()
    {
        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        theme.source.volume = theme.volume;
        theme.source.pitch = theme.pitch;
        theme.source.loop = theme.loop;



        if (theme.volume == 0)
        {
            theme.source.Stop();
        }
        else
        {
            if (!theme.source.isPlaying)
                theme.source.Play();
        }

    }
}
