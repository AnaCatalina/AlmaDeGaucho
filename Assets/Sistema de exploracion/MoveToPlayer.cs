using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPlayer : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public float speed = 3f; // Velocidad de movimiento
    private bool moveTowardsPlayer = false; // Bandera para iniciar el movimiento
    public float stopDistance = 6f; // Distancia a la cual el objeto se detendra frente al jugador
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Detectar si se presiona el boton "H"
        if (Input.GetKeyDown(KeyCode.H))
        {
            moveTowardsPlayer = true; // Activar el movimiento
        }

        // Si la bandera esta activada, mover el objeto hacia el jugador
        if (moveTowardsPlayer)
        {  
            // Calcular la posicion objetivo a una distancia de 4 unidades frente al jugador
            Vector3 targetPosition = player.position + player.forward * stopDistance;

            // Mover el objeto hacia la posicion objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Rotar el objeto para que mire hacia el jugador
            Vector3 directionToPlayer = player.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);

            // Comprobar si el objeto ha alcanzado la posicion y rotacion deseada
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f && Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                moveTowardsPlayer = false; // Desactivar el movimiento
            }
        }
        
    }
}
