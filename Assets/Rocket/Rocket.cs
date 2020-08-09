using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public bool debug = true; //debug state

    private Rigidbody collisionMesh;
    private AudioSource thrusterSound;

    [SerializeField] private float mainThrust = 18.0f;
    [SerializeField] private float rcsThrust = 200.0f;
    [SerializeField] private bool useFuel = true;
    [SerializeField] private int fuel = 1500;
    [SerializeField] private int HP = 50;
    [SerializeField] private int damageThreshold = 3;

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
            print("Rocket collided with " + collision.collider.ToString() + ".");
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //do nothing and/or win
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
                //calc damage
                if (CalcDamage(collision))
                {
                    //death
                }
                break;
        }
    }

    private bool CalcDamage(Collision collision)
    {
        //TODO: test this!

        //get rocket's relative velocity on collision with generic obstacles.
        int velocity = (int)Math.Floor(collision.relativeVelocity.magnitude);
        
        //rocket go boom
        bool death = false;

        if (velocity > damageThreshold)
        {
            HP -= velocity - damageThreshold;

            if (debug)
            {
                print("Rocket collided with an unfriendly object. HP Remaining: " + HP);

                if (HP <= 0)
                {
                    print("Rocket collision caused death.");
                }
            }

            if (HP <= 0)
            {
                death = true;
            }
        }
        return death;
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space) && fuel > 0)
        {
            //activate forward thrust
            collisionMesh.AddRelativeForce(mainThrust * Vector3.up);
            if (useFuel)
            {
                fuel--;
            }

            //soundfx - thrust
            if (!thrusterSound.isPlaying)
            {
                thrusterSound.Play();
            }

            if (debug)
            {
                print("Rocket is thrusting. Fuel remaining: " + fuel + " units.");
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


        float rotationThisFrame = rcsThrust * Time.deltaTime; //ensure that RPM is always same.

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
