using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscena : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip buttonPresed;
    public AudioClip buttonPointer;
    public string nameScene;
    // Llamamos a esta función para cambiar de escena
    public void CambiarEscenas(string nombre)
    {
        nameScene = nombre;
        Invoke("ChangeEscena",0.5f);
    }
    public void ChangeEscena()
    {
        SceneManager.LoadScene(nameScene);
    }

    public void PlayClip()
    {
        AudioSource.PlayOneShot(buttonPresed);
    }

    public void ClipPointerPlay()
    {
        AudioSource.PlayOneShot(buttonPointer);
    }
}