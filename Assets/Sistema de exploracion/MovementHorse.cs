using CamaraTerceraPersona;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHorse : MonoBehaviour
{
    #region
    public float walkSpeed = 2f;
    public float trotSpeed = 6f;
    public float runSpeed = 12f;
    public float turnSpeed = 5f;
    [Range(0f, 1f)]
    public float rotationSpeed = 0.2f;
    [Range (1f, 10f)]
    public float jumpForce = 5f;
    #endregion

    
    public Rigidbody rb;
    private Animator animator;

    private float currentSpeed;
    private bool isGalloping = false;
    private bool isRunning = false;

    public float rayDistance = 1.5f;
    public float rayUp = 0.5f;
    public float radioCast = 0.1f;

    #region Camara
    public Camera cam;
    public CamaraBahaviour cameraPro;
    private Vector3 camForwad;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        
        HandleMovement();
        HandleSpeedChange();
        Debug.DrawRay(transform.position + Vector3.up * rayUp, transform.forward * rayDistance, Color.red);
    }

    private void FixedUpdate()
    {
        //HandleMovement();
        //HandleSpeedChange();
    }

    void HandleMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        #region Direccion de la camara
        //camForwad = Vector3.Scale(cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        camForwad = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = Vector3.Scale(cam.transform.right, new Vector3(1, 0, 1).normalized);
        /*Vector3 camFlatFwd = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(cam.transform.right.x, 0, cam.transform.right.z);

        Vector3 m_CharForwad = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;*/
        #endregion

        //Se crea Vector de movimiento
        //Vector3 move = v * camForwad * currentSpeed + h * camRight * currentSpeed;
        Vector3 move = (v * camForwad + h * camRight).normalized * currentSpeed;

        RaycastHit hit;
        bool obstaculo = Physics.SphereCast(transform.position + Vector3.up * rayUp, radioCast,transform.forward, out hit, rayDistance);

        

        if (obstaculo && Vector3.Dot(transform.forward, move) > 0)
        {
            move = Vector3.zero;
            //animator.SetFloat("Vel", 0);
        }

        if(move.magnitude >0.1f || h != 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move.magnitude > 0 ? move : transform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
        }


        //Rotación hacia la dirección del movimiento si se esta moviendo
       /* if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
        }*/

        //Aplicar movimiento normalizado
        rb.MovePosition(rb.position + move * Time.deltaTime);
        /*Vector3 move = v * m_CharForwad * currentSpeed + h * m_CharRight * currentSpeed;
        cam.transform.position += move * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, move, rotationSpeed, 0.0f));

        transform.position += move * Time.deltaTime;*/
        // Rotación del caballo
        /*if (h != 0)
        {
            Quaternion turn = Quaternion.Euler(0, h * turnSpeed * Time.deltaTime, 0);
            rb.MoveRotation(rb.rotation * turn);
        }*/

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Animaciones
        //animator.SetFloat("Vel", Mathf.Abs(moveDirection));
        //animator.SetFloat("Vel", v * currentSpeed);
        animator.SetFloat("Vel", move.magnitude);

        //Resetea la velocidad al detener el movimiento
        if (move.magnitude == 0)
        {
            currentSpeed = walkSpeed;
            //currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, Time.deltaTime * 5f);
            isRunning = false;
            isGalloping = false;
        }
    }

    void HandleSpeedChange()
    {
        // Aumentar velocidad con Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!isGalloping)
            {
                currentSpeed = trotSpeed;
                //currentSpeed = Mathf.Lerp(currentSpeed, trotSpeed, Time.deltaTime * 5f);
                isGalloping = true;
            }
            else if (!isRunning)
            {
                currentSpeed = runSpeed;
                //currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, Time.deltaTime * 5f);
                isRunning = true;
            }
        }

        // Reducir velocidad con Ctrl
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isRunning)
            {
                currentSpeed = trotSpeed;
                //currentSpeed = Mathf.Lerp(currentSpeed, trotSpeed, Time.deltaTime * 5f);
                isRunning = false;
            }
            else if (isGalloping)
            {
                currentSpeed = walkSpeed;
                //currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, Time.deltaTime * 5f);
                isGalloping = false;
            }
        }
    }
}
