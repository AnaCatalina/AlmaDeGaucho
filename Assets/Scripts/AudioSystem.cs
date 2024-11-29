using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
#endif

public class AudioSystem : MonoBehaviour
{
    #region Audio System
    [Header("Footstep System")]
    public bool enableFootstepSounds = true;
    public AudioSource emisorAudioSource;
    public List<AudioClip> currentClipSet = new List<AudioClip>();
    public string tagCompare;
    #endregion
    // Start is called before the first frame update
    void Start()
    {

        emisorAudioSource = GetComponent<AudioSource>();
    }

    public void CallClip()
    {
        if (currentClipSet != null && currentClipSet.Any())
        {
            emisorAudioSource.PlayOneShot(currentClipSet[Random.Range(0, currentClipSet.Count())]);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("madera"))
        {
            CallClip();
        }

    }

}
