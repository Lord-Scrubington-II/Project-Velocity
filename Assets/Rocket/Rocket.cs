using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public readonly bool debug = true; //debug state

    Rigidbody collisionMesh;
    private int thrusterForce = 10;

    // Start is called before the first frame update
    void Start()
    {
        collisionMesh = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            collisionMesh.AddRelativeForce(new Vector3(0, thrusterForce, 0), ForceMode.Force);
            //activate forward thrust
            if (debug)
            {
                print("Rocket is thrusting.");
            }
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward);
            //gyrate left
            if (debug)
            {
                print("Rocket is rotating left.");
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back);
            //gyrate right
            if (debug)
            {
                print("Rocket is rotating right.");
            }
        }
    }
}
