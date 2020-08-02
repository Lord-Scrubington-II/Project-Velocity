using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public readonly bool debug = true; //debug state

    private Rigidbody collisionMesh;
    private AudioSource thrusterSound;
    private readonly int thrusterForce = 16;

    // Start is called before the first frame update
    void Start()
    {
        collisionMesh = this.GetComponent<Rigidbody>();
        thrusterSound = this.GetComponent<AudioSource>();
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
            //activate forward thrust
            collisionMesh.AddRelativeForce(new Vector3(0, thrusterForce, 0), ForceMode.Force);
            if (!thrusterSound.isPlaying)
            {
                thrusterSound.Play();
            }
            if (debug)
            {
                print("Rocket is thrusting.");
            }
        }
        else
        {
            thrusterSound.Stop();
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            //gyrate left
            transform.Rotate(Vector3.forward);
            if (debug)
            {
                print("Rocket is rotating left.");
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //gyrate right
            transform.Rotate(Vector3.back);
            if (debug)
            {
                print("Rocket is rotating right.");
            }
        }
    }
}
