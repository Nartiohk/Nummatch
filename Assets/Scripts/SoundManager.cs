using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tileMatchSound;
    [SerializeField] private AudioClip tileSelectSound;
    [SerializeField] private AudioClip rowClearSound;
    [SerializeField] private AudioClip helpSound;
    
    public void PlayTileMatchSound()
    {
        if (tileMatchSound != null)
        {
            audioSource.PlayOneShot(tileMatchSound);
        }
        else
        {
            Debug.LogWarning("Tile match sound not set!");
        }
    }

    public void PlayTileSelectSound()
    {
        if (tileSelectSound != null)
        {
            audioSource.PlayOneShot(tileSelectSound);
        }
        else
        {
            Debug.LogWarning("Tile match sound not set!");
        }
    }
    
    public void PlayRowClearSound()
    {
        if (rowClearSound != null)
        {
            audioSource.PlayOneShot(rowClearSound);
        }
        else
        {
            Debug.LogWarning("Tile match sound not set!");
        }
    }
    
    public void PlayHelpSound()
    {
        if (helpSound != null)
        {
            audioSource.PlayOneShot(helpSound);
        }
        else
        {
            Debug.LogWarning("Tile match sound not set!");
        }
    }
}
