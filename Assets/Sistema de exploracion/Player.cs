using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Horse currentHorse;
    private bool canMount = false;
    private JuanMoveBehaviour playerMovement;
    private Rigidbody rb;
    public Animator animator;
    //public bool b;
    //public bool 
    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<JuanMoveBehaviour>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMount && Input.GetKeyDown(KeyCode.E))
        {
            MountHorse();
           
        }
        else if (currentHorse != null && Input.GetKeyDown(KeyCode.Q))
        {
            DismountHorse();
        }
        
    }

    public void CanMount(Horse horse)
    {
        currentHorse = horse;
        canMount = horse != null;
    }

    private void MountHorse()
    {
        if (currentHorse != null)
        {
            transform.rotation = currentHorse.saddlePosition.rotation;
            transform.position = currentHorse.saddlePosition.position;
            transform.SetParent(currentHorse.HorseMovement.transform);
            animator.SetBool("Sentado", true);
            //transform.localScale = originalScale; // Asegura que la escala se mantenga
            playerMovement.enabled = false;//  Desactivar el control del jugador
            //rb.useGravity = false;
            
            currentHorse.HorseMovement.enabled = true;
            currentHorse.GetComponent<MovimientoBasico>().enabled = true; // Activar el control del caballo
            
        }
        
    }

    private void DismountHorse()
    {
        if (currentHorse != null)
        {
            animator.SetBool("Sentado", false);
            
            //transform.localScale = originalScale; // Asegura que la escala se mantenga
            
            //rb.useGravity = true;
            transform.rotation = currentHorse.bajada.rotation;
            transform.position = currentHorse.bajada.position;
            transform.SetParent(null);
            currentHorse.HorseMovement.enabled = true;
            currentHorse.GetComponent<MovimientoBasico>().enabled = false; // Desactivar el control del caballo
            currentHorse = null;
            playerMovement.enabled = true;//  Reactivar el control del jugador
        }
    }
}
