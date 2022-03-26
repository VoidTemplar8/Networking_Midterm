using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 2f, 
            0, Input.GetAxis("Vertical") * Time.deltaTime *2f);   
    }
}
