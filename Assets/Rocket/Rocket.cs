using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] public bool debug = true; //debug state

    private Rigidbody collisionMesh;
    private AudioSource thrusterSound;

    [SerializeField] private float mainThrust = 18.0f;
    [SerializeField] private float rcsThrust = 200.0f;
    private int fuel = 200;

    // Start is called before the first frame update
    void Start()
    {
        collisionMesh = this.GetComponent<Rigidbody>();
        thrusterSound = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Thrust();
        Gyrate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (debug)
        {
            print("Rocket collided with " + " " + ".");
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //do nothing
                if (debug)
                {
                    print("Rocket hit a friendly object.");
                }

                break;
            case "Fuel":
                //replenish fuel
                if (debug)
                {
                    print("Rocket collected fuelcell.");
                }

                break;
            default:
                //death
                if (debug)
                {
                    print("Rocket collision caused death.");
                }

                break;
        }
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            //activate forward thrust
            collisionMesh.AddRelativeForce(mainThrust * Vector3.up);
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
    }

    private void Gyrate()
    {
        collisionMesh.freezeRotation = true; //all control of rotation is manual.


        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            //gyrate left
            transform.Rotate(Vector3.forward * rotationThisFrame);
            
            if (debug)
            {
                print("Rocket is rotating left.");
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //gyrate right
            transform.Rotate(Vector3.back * rotationThisFrame);
           
            if (debug)
            {
                print("Rocket is rotating right.");
            }
        }

        collisionMesh.freezeRotation = false; //resume physics
    }
}
