using CamaraTerceraPersona;
using SUPERCharacte;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanzarBoleadora : MonoBehaviour
{
    public JuanMoveBehaviour moveBehaviour;
    public Camera camaraPro;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Boleadora Function
    public void LanzarBoleadoras()
    {
        if (moveBehaviour.tengoBoleadoras)
        {
            // Crear y lanzar la boleadora
            GameObject boleadora = Instantiate(moveBehaviour.boleadoraPrefab, moveBehaviour.lanzamientoPos.position, Quaternion.identity);

            // Dirección de lanzamiento hacia donde mira la cámara
            Vector3 direccion = camaraPro.transform.forward;
            Rigidbody rb = boleadora.GetComponent<Rigidbody>();
            rb.velocity = direccion * moveBehaviour.fuerzaLanzamiento;

            // Ocultar el objeto en la mano
            //GetComponent<ArmaPlayer>().boleadoras.SetActive(false);
            moveBehaviour.tengoBoleadoras = false;
        }
    }
    #endregion
}
