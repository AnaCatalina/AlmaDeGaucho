using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoBasico : MonoBehaviour
{
    public float speed = 5f;
    private float speedRotate = 200f;
    private Animator animacion;
    private Rigidbody physicBody;
    public bool isRunning;
    public float moveSpeed = 3.0f;
    public float runSpeed = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        animacion = GetComponent<Animator>();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        //transform.forward = Camera.main.transform.forward;
    }

    public void MovePlayer()
    {
        float vertical = Input.GetAxis("Vertical");
        //transform.Translate(new Vector3(0, 0.0f, vertical) * Time.deltaTime * speed);

        float rotationY = (Input.GetAxis("Mouse X"));
        transform.Rotate(new Vector3(0, rotationY, 0) * Time.deltaTime * speedRotate);
        //animacion.SetFloat("VelY", vertical);

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
            transform.Translate(new Vector3(0, 0.0f, vertical) * moveSpeed * Time.deltaTime);
            animacion.SetFloat("Vel", vertical * moveSpeed);
        }
        else
        {
            transform.Translate(new Vector3(0, 0.0f, vertical) * runSpeed * Time.deltaTime);
            animacion.SetFloat("Vel", vertical * runSpeed);
        }
    }
}
