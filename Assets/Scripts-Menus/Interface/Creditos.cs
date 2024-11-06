using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Creditos : MonoBehaviour
{
    // Este m�todo se llama al iniciar la escena
    // Invoca el m�todo "LoadMainMenu" despu�s de 31 segundos, que cambiar� a la escena "MainMenu"
    void Start()
    {
        Invoke("LoadMainMenu", 43);
    }

    // Este m�todo se llama una vez por frame
    // Revisa si se presiona Escape, la barra espaciadora, Enter o el clic derecho del mouse, y cambia a la escena "MainMenu"
    void Update()
    {
        // Verifica si alguna de las teclas son presionadas
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            LoadMainMenu();
        }
    }

    // Este m�todo cambia a la escena "MainMenu"
    // Se invoca autom�ticamente tras 31 segundos o cuando es llamado
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
