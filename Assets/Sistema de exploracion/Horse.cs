using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : MonoBehaviour
{
    //Ray ray;
    public Transform saddlePosition; // Posición en la que el jugador se sentará
    public Transform bajada;
    //public HorseMovement HorseMovement;
    public MovimientoBasico HorseMovement;
    public LayerMask layerPlayer;
    public GameObject playerObj;
    public float rango = 2f;
    public float distX = 0.0f;
    public float distY = 0.0f;
    public float distZ = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit hit;
        //Ray ray;
        bool playerDetect = Physics.CheckSphere(transform.position - new Vector3(distX, distY, distZ), rango, layerPlayer);
        if (playerDetect )
        {
            //Debug.Log("detectado");
            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                player.CanMount(this);
            }

        }
        else
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null)
            {
                player.CanMount(null);
            }
        }
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position - new Vector3(distX, distY, distZ), rango);
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position - new Vector3(0, 0, dist), rango);
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("se produce una colision");
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.CanMount(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.CanMount(null);
            }
        }
    }*/
}
