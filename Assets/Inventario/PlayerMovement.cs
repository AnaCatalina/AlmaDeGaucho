using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 12f;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
         MovePlayer();
        
    }

    void MovePlayer()
    {
        // Movimiento horizontal
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 newPosition = rb.position + move * speed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }
}
