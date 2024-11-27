using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CambiarColorText : MonoBehaviour
{
    public TextMeshProUGUI texto;
    private Color color;
    private Color color2;

    // Start is called before the first frame update
    void Start()
    {
        color = Color.white;
        color2 = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CambiarColor()
    {
        texto.color = color;
    }

    public void ColorAnterior()
    {
        texto.color = color2;
    }

}
