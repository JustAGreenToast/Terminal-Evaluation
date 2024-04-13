using UnityEngine;

public class MusicPlayerScript : MonoBehaviour
{
    AudioSource _player;
    AudioSource player
    {
        get
        {
            if (!_player) { _player = GetComponent<AudioSource>(); }
            return _player;
        }
    }
    public bool isPlaying { get { return player.isPlaying; } }
    public float timeLeft { get { return player.clip.length - player.time; } }
    float startVol;
    float targetVol;
    float startPitch;
    float targetPitch;
    float vol;
    float lerpVal = 1;
    float lerpSpeed;
    // Update is called once per frame
    void Update()
    {
        if (lerpVal != 1)
        {
            lerpVal = Mathf.MoveTowards(lerpVal, 1, lerpSpeed * Time.deltaTime);
            vol = Mathf.Lerp(startVol, targetVol, lerpVal);
            player.volume = vol * SettingsManager.settings.musicVol * SettingsManager.settings.masterVol;
            player.pitch = Mathf.Lerp(startPitch, targetPitch, lerpVal);
        }
    }
    public void PlaySong(AudioClip _clip, bool _loop = true)
    {
        if (player.clip != _clip)
        {
            lerpVal = 0;
            lerpSpeed = 1.25f;
            startVol = player.volume;
            startPitch = player.pitch;
            targetVol = 1;
            targetPitch = 1;
            player.clip = _clip;
            player.loop = _loop;
            player.Play();
        }
    }
    public void StopSong()
    {
        lerpVal = 0;
        lerpSpeed = 0.75f;
        startVol = vol;
        startPitch = player.pitch;
        targetVol = vol;
        targetPitch = 0;
    }
    public void ResumeSong()
    {
        lerpVal = 0;
        lerpSpeed = 0.75f;
        startVol = vol;
        startPitch = player.pitch;
        targetVol = vol;
        targetPitch = 1;
    }
}