using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarEscenaConPantallaDeCarga : MonoBehaviour
{
    public void IniciarCambioDeEscena(string nombreEscena)
    {
        // Guardamos el nombre de la escena destino en PlayerPrefs
        PlayerPrefs.SetString("EscenaDestino", nombreEscena);

        // Cargamos la escena de la pantalla de carga
        SceneManager.LoadScene("LoadingScreen");
    }
}