using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public bool debug = true; //debug state

    private Rigidbody rigidBody;
    private AudioSource rocketAudio;

    [SerializeField] private float mainThrust = 1000.0f;
    [SerializeField] private float rcsThrust = 200.0f;
    [SerializeField] private float levelLoadDelay = 4.0f;


    [SerializeField] private bool useFuel = true;
    [SerializeField] private int fuel = 1500;
    [SerializeField] private int HP = 20;
    [SerializeField] private int damageThreshold = 3;
    [SerializeField] State state = State.Alive;

    [SerializeField] private AudioClip mainEngine;
    [SerializeField] private AudioClip deathNoise;
    [SerializeField] private AudioClip goalReached;

    [SerializeField] private ParticleSystem mainEngineParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem goalReachedParticles;

    private static int nextSceneIndex = 1;
    private static readonly int sceneCount = 3;

    enum State { Alive, Dying, Transitioning }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        rocketAudio = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.Alive)
        {
            HandleThrust();
            HandleGyration();
        }

        //todo consider removal
        if(state == State.Transitioning)
        {
            //stop physics engine from controlling rocket after target reached.
            rigidBody.freezeRotation = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (state != State.Alive) { return; }

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

            case "Target":
                //next level!
                if (debug)
                {
                    print("Rocket landed!");
                }
                state = State.Transitioning;
                Invoke("LoadNextLevel", levelLoadDelay);
                HandleAudioEvent(goalReached);
                ClearParticles();
                goalReachedParticles.Play();
                break;

            default:
                //calc damage
                if (CalcDamage(collision))
                {
                    //death
                    state = State.Dying;
                    Invoke("Restart", levelLoadDelay);
                    HandleAudioEvent(deathNoise);
                    ClearParticles();
                    deathParticles.Play();
                }
                break;
        }
    }
    
    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextSceneIndex);
        
        if(nextSceneIndex < sceneCount)
        {
            nextSceneIndex++;
        }
    }
    private void Restart()
    {
        SceneManager.LoadScene(0);
        nextSceneIndex = 1;
    }

    private bool CalcDamage(Collision collision)
    {
        //todo test this!

        //get rocket's relative velocity on collision with generic obstacles.
        int velocity = (int)Math.Floor(collision.relativeVelocity.magnitude);
        
        //rocket go boom?
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

            //rocket go boom
            if (HP <= 0)
            {
                death = true;
            }
        }
        return death;
    }

    private void HandleThrust()
    {
        if (Input.GetKey(KeyCode.Space) && fuel > 0)
        {
            DoThrust();
        }
        else
        {
            rocketAudio.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void DoThrust()
    {
        //activate forward thrust
        rigidBody.AddRelativeForce(mainThrust * Vector3.up * Time.deltaTime);
        if (useFuel)
        {
            fuel--;
        }

        //soundfx - thrust
        if (!rocketAudio.isPlaying)
        {
            HandleAudioEvent(mainEngine);
        }

        if (!mainEngineParticles.isEmitting)
        {
            mainEngineParticles.Play();
        }

        if (debug)
        {
            print("Rocket is thrusting. Fuel remaining: " + fuel + " units.");
        }
    }

    private void HandleGyration()
    {
        rigidBody.freezeRotation = true; //all control of rotation is manual.


        float rotationThisFrame = rcsThrust * Time.deltaTime; //ensure that RPM is always same.

        Gyrate(rotationThisFrame);

        rigidBody.freezeRotation = false; //resume physics
    }

    private void Gyrate(float rotationThisFrame)
    {
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
    }

    private void HandleAudioEvent(AudioClip clip)
    {
        rocketAudio.Stop();
        rocketAudio.PlayOneShot(clip);
    }

    private void ClearParticles()
    {
        mainEngineParticles.Stop();
        deathParticles.Stop();
        goalReachedParticles.Stop();
    }
}
