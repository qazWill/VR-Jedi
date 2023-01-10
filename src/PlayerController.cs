using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private InputManager input;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

        // slight gravity
        //rb.AddForce(new Vector3(0f, -2f, 0f), ForceMode.Acceleration);

        if (input.getRightGrip())
        {
         
        }

    }
}
