using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseMovement : MonoBehaviour
{
    public float mouseHorizontal = 3f;
    public float mouseVertical = 2f;

    public Animator anim;

    public Rigidbody rb;

    float h_mouse;
    float v_mouse;

    public float moveSpeed = 3.0f;
    public float runSpeed = 10.0f;
    public float runDuration = 30.0f; // Duración del tiempo que el caballo puede correr
    public float cooldownDuration = 10.0f; // Tiempo que debe caminar antes de poder correr de nuevo
    private float runTimer;
    private float cooldownTimer;
    public bool isRunning;
    public bool isWalk;
    public Vector3 vel;
    public float VelXZ;


    float h;
    float v;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        runTimer = 0.0f;
        cooldownTimer = 0.0f;
        isRunning = false;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        //vel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        //VelXZ = vel.magnitude;
        //anim.SetFloat("VelXZ", rb.velocity.z);
        //Debug.Log(rb.velocity.z);
        //anim.SetBool("isRun", isRunning);
        //anim.SetBool("isWalk", isWalk);
    }

    void Move()
    {
        h_mouse = mouseHorizontal * Input.GetAxis("Mouse X");
        v_mouse = mouseVertical * Input.GetAxis("Mouse Y");

        transform.Rotate(0, h_mouse, 0);
        //Cam.transform.Rotate(-v_mouse, 0, 0);

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //Vector3 direction = new Vector3(h, 0, v);
        Vector3 direction = new Vector3(0, 0, v);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunning = false;
        }
        if (!isRunning)
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            anim.SetFloat("Vel", v * moveSpeed);
        }else
        {
            transform.Translate(direction * runSpeed * Time.deltaTime);
            anim.SetFloat("Vel", v * runSpeed);
        }
        /*if (Input.GetButton("Fire3") && cooldownTimer <= 0)
        {
            isRunning = true;
            isWalk = false;
            //runTimer = runDuration;
            //cooldownTimer = cooldownDuration;

        }
        if (isRunning)
        {
            transform.Translate(direction * runSpeed * Time.deltaTime);
            //runTimer -= Time.deltaTime;
            anim.SetFloat("Vel", runSpeed);
            isWalk = false;

            if (runTimer <= 0)
            {
                isRunning = false;
            }
        }    
        else
        {
            //isRunning = false;
            isWalk = true;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            anim.SetFloat("Vel", moveSpeed);
            //cooldownTimer -= Time.deltaTime;
            //anim.SetBool("walk", h != 0 || v != 0);
            //anim.SetBool("walk", true);
        }*/


    }
}
