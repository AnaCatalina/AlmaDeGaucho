using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public int rutina;
    public float cronometro;
    public Animator animator;
    public Quaternion angulo;
    public float grado;

    public GameObject target;
    public bool atacando;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        target = GameObject.Find("Juan");
    }

    // Update is called once per frame
    void Update()
    {
        ComportamientoEnemigo();
    }

    public void ComportamientoEnemigo()
    {
        if(Vector3.Distance(transform.position, target.transform.position) > 10) 
        {
            //animator.SetBool("run", false);
            cronometro += 1 * Time.deltaTime;
            if (cronometro >= 4)
            {
                rutina = Random.Range(0, 2);
                cronometro = 0;
            }
            switch (rutina)
            {
                case 0:
                    animator.SetBool("run", false);
                    break;

                case 1:
                    grado = Random.Range(0, 360);
                    angulo = Quaternion.Euler(0, grado, 0);
                    rutina++;
                    break;

                case 2:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, angulo, 0.5f);
                    transform.Translate(Vector3.forward * 1 * Time.deltaTime);
                    animator.SetBool("run", true);
                    break;
            }
        }
        else
        {
            if(Vector3.Distance(transform.position, target.transform.position)>2 && !atacando)
            {
                var lookPos = target.transform.position - transform.position;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 2);
                animator.SetBool("walk", false);
                animator.SetBool("run", true);
                transform.Translate(Vector3.forward * 2 * Time.deltaTime);
                animator.SetBool("atacar", false) ;
            }
            else
            {
                animator.SetBool("walk", false);
                animator.SetBool("run", false);

                animator.SetBool("atacar", true);
                atacando = true;
            }
            
        }
        
    }

    public void Final_Anim()
    {
        animator.SetBool("atacar", false );
        atacando = false;
    }
}
