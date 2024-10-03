using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmaPlayer : MonoBehaviour
{
    public GameObject facon;
    public BoxCollider faconCollider;
    private JuanMoveBehaviour moveBehaviour;

    public GameObject poncho;
    public BoxCollider ponchoCollider;

    public GameObject rifle;

    public GameObject boleadoras;
    //private bool tengoArma;
    private bool armaEquipada;
    public bool pressEquiparFacon;
    public bool pressManos;
    public bool pressRifle;
    public bool pressBoleadoras;
    //private Animator animator;

    
    // Start is called before the first frame update
    void Start()
    {
        moveBehaviour = GetComponent<JuanMoveBehaviour>();
        //animator = GetComponent<Animator>();
        DesactivarColliderFacon();//se desactivan los boxColliders del puño y de la espada
        //tengoArma = false;      //al iniciar la escena el player no tiene un arma
        armaEquipada = false;   //por lo tanto tampoco la tendria equipada
        pressEquiparFacon = false;
        pressManos = false;
        pressBoleadoras = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (pressEquiparFacon)
        {
            pressEquiparFacon= false;
            ActivarFacon();
        }
        if (pressManos)
        {
            pressManos= false;
            DesactivarArmas();
        }
        if (pressRifle)
        {
            pressRifle= false;
            ActivarRifle();
        }
        if (pressBoleadoras)
        {
            pressBoleadoras= false;
            moveBehaviour.playerCamera.transform.position = moveBehaviour.posCamera.transform.position;
            moveBehaviour.playerCamera.transform.rotation = moveBehaviour.posCamera.transform.rotation;
            
            //moveBehaviour.playerCamera.transform.position = new Vector3(moveBehaviour.p_Rigidbody.position.x + 0.7f,moveBehaviour.p_Rigidbody.position.y + 1f,moveBehaviour.p_Rigidbody.position.z - 2f);
            ActivarBoleadoras();
        }

        //este if sirve para equiparse y desaquiparse el arma cuando se apreta la tecla especificada
        if (Input.GetKey(KeyCode.E)) //también como condición tiene que antes se tendria que haber agarrado un arma
        {
            if (armaEquipada)       //evalúa si tiene o no equipada el arma
            {
                //DesactivarArma();
            }
            else
            {
                //ActivateArma();
            }
        }
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
        if (moveBehaviour.tengoFacon)
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

    public void ActivarFacon()
    {
        //tengoArma = true;
        facon.SetActive(true);   //activa el arma del player
        poncho.SetActive(true);
        rifle.SetActive(false);
        boleadoras.SetActive(false);
        armaEquipada = true;
        //animator.SetBool("armaEquipada", true);
        moveBehaviour.tengoFacon = true; //cambia el estado de una variable correspondiente al Script PlayerBehaviour
        moveBehaviour.tengoRifle=false;
        moveBehaviour.tengoBoleadoras = false;
    }

    public void DesactivarArmas()
    {
        rifle.SetActive(false);
        facon.SetActive(false);              //desactiva el arma del player
        poncho.SetActive(false );
        boleadoras.SetActive(false);
        armaEquipada = false;
        //animator.SetBool("armaEquipada", false);
        moveBehaviour.tengoFacon = false;
        moveBehaviour.tengoRifle = false;
        moveBehaviour.tengoBoleadoras = false;
    }

    public void ActivarRifle()
    {
        rifle.SetActive(true);
        facon.SetActive(false);              //desactiva el arma del player
        poncho.SetActive(false);
        boleadoras.SetActive(false);
        moveBehaviour.tengoRifle = true;
        moveBehaviour.tengoFacon = false;
        moveBehaviour.tengoBoleadoras = false;
    }

    public void ActivarBoleadoras()
    {
        boleadoras.SetActive(true);
        rifle.SetActive(false);
        facon.SetActive(false);              //desactiva el arma del player
        poncho.SetActive(false);
        moveBehaviour.tengoBoleadoras=true;
        moveBehaviour.tengoRifle = false;
        moveBehaviour.tengoFacon = false;
    }

}
