using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Creditos : MonoBehaviour
{
    // Este método se llama al iniciar la escena
    // Invoca el método "LoadMainMenu" después de 31 segundos, que cambiará a la escena "MainMenu"
    void Start()
    {
        Invoke("LoadMainMenu", 43);
    }

    // Este método se llama una vez por frame
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

    // Este método cambia a la escena "MainMenu"
    // Se invoca automáticamente tras 31 segundos o cuando es llamado
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
