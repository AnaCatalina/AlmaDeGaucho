using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MostrarOcultarText : MonoBehaviour
{
    //public TextMeshProUGUI text;

    public void MostrarTexto()
    {
        //text.enabled = true;
        gameObject.SetActive(true);
    }
    public void OcultarTexto()
    {
        //text.enabled = false;
        gameObject.SetActive(false);
    }
}
