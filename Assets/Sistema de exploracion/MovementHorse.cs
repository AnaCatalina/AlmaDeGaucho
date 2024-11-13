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
    public float turnSpeed = 200f;
    [Range(0.1f, 1.5f)]
    public float rotationSpeed;
    [Range (1f, 10f)]
    public float jumpForce;
    #endregion

    private Rigidbody rb;
    private Animator animator;

    private float currentSpeed;
    private bool isGalloping = false;
    private bool isRunning = false;

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

        #region Cam Dir
        camForwad = Vector3.Scale(cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        Vector3 camFlatFwd = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(cam.transform.right.x, 0, cam.transform.right.z);

        Vector3 m_CharForwad = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;
        #endregion

        // Movimiento hacia adelante y atrás
        //Vector3 move = transform.forward * moveDirection * currentSpeed * Time.deltaTime;
        //rb.MovePosition(rb.position + move);
        
        Vector3 move = v * m_CharForwad * currentSpeed + h * m_CharRight * currentSpeed;
        cam.transform.position += move * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, move, rotationSpeed, 0.0f));

        transform.position += move * Time.deltaTime;

        // Rotación del caballo
        /*if (h != 0)
        {
            Quaternion turn = Quaternion.Euler(0, h * turnSpeed * Time.deltaTime, 0);
            rb.MoveRotation(rb.rotation * turn);
        }*/

        // Salto
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Animaciones (asume que tienes un Animator configurado)
        //animator.SetFloat("Vel", Mathf.Abs(moveDirection));
        //animator.SetFloat("Vel", v * currentSpeed);
        animator.SetFloat("Vel", move.magnitude);
    }

    void HandleSpeedChange()
    {
        // Aumentar velocidad con Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!isGalloping)
            {
                currentSpeed = trotSpeed;
                isGalloping = true;
            }
            else if (!isRunning)
            {
                currentSpeed = runSpeed;
                isRunning = true;
            }
        }

        // Reducir velocidad con Ctrl
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isRunning)
            {
                currentSpeed = trotSpeed;
                isRunning = false;
            }
            else if (isGalloping)
            {
                currentSpeed = walkSpeed;
                isGalloping = false;
            }
        }
    }
}
