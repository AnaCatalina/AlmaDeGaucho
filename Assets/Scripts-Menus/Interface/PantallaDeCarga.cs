using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PantallaDeCarga : MonoBehaviour
{
    public Slider barraProgreso;        // Asigna el Slider de progreso desde el Inspector
    public TMP_Text textoCargando;      // Asigna el texto que dice "Cargando..."
    public TMP_Text textoMensajes;      // Asigna el texto donde se mostrarán mensajes

    private string escenaDestino;

    // Lista de mensajes
    private string[] mensajes = new string[]
    {
        "El que tiene paciencia, tiene fuerza",
        "En la pelea, solo vence quien lucha",
        "Nunca es triste la verdad, lo que no tiene es remedio",
        "Más vale un grito a tiempo que mil silencios tardíos",
        "El que se atreve, triunfa",
        "El coraje no se hereda, se gana",
        "El hombre ha nacido libre, y el respeto lo hace hombre",
        "El que a hierro mata, a hierro muere",
        "El destino es el que baraja, pero tú eliges las cartas",
        "Solo el tiempo sabe lo que traerá el viento"
    };

    void Start()
    {
        // Obtenemos el nombre de la escena que queremos cargar desde PlayerPrefs
        escenaDestino = PlayerPrefs.GetString("EscenaDestino", "MainMenu");

        // Muestra un mensaje aleatorio
        MostrarMensajeAleatorio();

        // Iniciamos la corutina para cambiar mensajes
        StartCoroutine(CambiarMensajeCada5Segundos());

        // Iniciamos la corutina para cargar la escena asíncronamente
        StartCoroutine(CargarEscenaAsync());
    }

    void MostrarMensajeAleatorio()
    {
        // Selecciona un índice aleatorio de la lista de mensajes
        int indiceAleatorio = Random.Range(0, mensajes.Length);
        textoMensajes.text = mensajes[indiceAleatorio];
    }

    IEnumerator CambiarMensajeCada5Segundos()
    {
        while (true) // Ciclo infinito para cambiar el mensaje cada 5 segundos
        {
            yield return new WaitForSeconds(10f); // Esperar 5 segundos
            MostrarMensajeAleatorio(); // Mostrar un nuevo mensaje
        }
    }

    IEnumerator CargarEscenaAsync()
    {
        // Empieza a cargar la escena de manera asíncrona
        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaDestino);

        // Evitamos que la escena se active automáticamente hasta que esté completamente cargada
        operacion.allowSceneActivation = false;

        // Variables de progreso
        float progresoVisual = 0f; // Progreso visual en porcentaje

        // Actualizamos la barra de progreso y el texto mientras se carga la escena
        while (!operacion.isDone)
        {
            // Incrementar el progreso visual de forma fluida
            if (progresoVisual < 1f) // Asegurarse de que no exceda el 100%
            {
                progresoVisual += Time.deltaTime / 10f; // Ajusta la velocidad de incremento aquí
            }

            // Actualizar la barra de progreso
            barraProgreso.value = progresoVisual;

            // Actualizar el texto de carga con el porcentaje
            textoCargando.text = "Cargando " + (int)(progresoVisual * 100) + "%";

            // Cuando el progreso visual es 100%, activamos la nueva escena
            if (progresoVisual >= 1f)
            {
                // Esperamos un breve periodo antes de activar la escena (opcional)
                yield return new WaitForSeconds(0.5f);
                operacion.allowSceneActivation = true;
            }

            yield return null; // Espera hasta el siguiente frame
        }
    }
}