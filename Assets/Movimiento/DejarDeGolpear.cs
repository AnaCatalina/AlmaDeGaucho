using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DejarDeGolpear : MonoBehaviour
{
    public JuanMoveBehaviour script;
    public Rigidbody rb;
    public bool avanzaSolo;
    public float impulsoEsquivar = 10f;
    public BoxCollider faconCollider;
    //public JuanMoveBehaviour moveBehaviour;
    public BoxCollider ponchoCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        if (avanzaSolo)
        {
            rb.velocity = -transform.forward * impulsoEsquivar;
        }
    }
    void DetenerAtaque()
    {
        script.atacando=false;
        script.walkingSpeed = 125;
    }

    void AvanzaSolo()
    {
        avanzaSolo = true;
    }
    void DejaDeAvanzar()
    {
        avanzaSolo=false;
    }

    public void DesactivarColliderFacon()
    {
        if (faconCollider != null)
        {
            faconCollider.enabled = false;
        }
        if (ponchoCollider != null)
        {
            ponchoCollider.enabled = false;
        }
        //punioBoxCollider.enabled = false;
    }

    //Activa los colliders del puño y de la espada
    public void ActivarColliderFacon()
    {
        if (script.tengoFacon)
        {
            if (faconCollider != null)
            {
                faconCollider.enabled = true;
            }
            if (ponchoCollider != null)
            {
                ponchoCollider.enabled = true;
            }
        }
        //else
        //{
        //    punioBoxCollider.enabled = true;
        //}

    }
}
