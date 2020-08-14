using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    public readonly bool debug = false; //debug state

    //game components
    private Rigidbody rigidBody;
    private AudioSource rocketAudio;
    private Light spotLight;

    [SerializeField] private float mainThrust = 1000.0f; //thrust force vector magnitude
    [SerializeField] private float rcsThrust = 200.0f; //rotation velocity vector magnitude
    [SerializeField] private float levelLoadDelay = 4.0f; //what it says on the tin

    //fuel and light
    [SerializeField] private bool useFuel = true;
    [SerializeField] private bool useLight = true;
    [SerializeField] private int fuel = 1500;

    //rocket health and state
    [SerializeField] private int HP = 20;
    [SerializeField] private int damageThreshold = 3;
    [SerializeField] private State state = State.Alive;

    //audio resources
    [SerializeField] private AudioClip mainEngine;
    [SerializeField] private AudioClip deathNoise;
    [SerializeField] private AudioClip goalReached;

    //particle aeffects
    [SerializeField] private ParticleSystem mainEngineParticles;
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem goalReachedParticles;

    private static int sceneCount;
    private bool useCollisions = true;

    private State Status { get => state; }

    enum State { Alive, Dying, Transitioning }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        rocketAudio = this.GetComponent<AudioSource>();
        spotLight = this.GetComponent<Light>();
        sceneCount = SceneManager.sceneCountInBuildSettings;
        //not working for some reason.
        if (!useLight)
        {
            spotLight.intensity = 0;
        }
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
            rigidBody.angularVelocity = Vector3.zero;
        }

        /*
        if(rigidBody.rotation.x >= 2 || rigidBody.rotation.y >= 2)
        {
            Kill();
        }*/

        //access debug menu when debug mode is on
        if (debug)
        {
            HandleDebugMenu();
        }
    }

    private void HandleDebugMenu()
    {
        if (Input.GetKeyDown(KeyCode.L)) //instant advancement
        {
            LoadNextLevel();
            print("Debug option: developer advanced to next level.");
        }

        if (Input.GetKeyDown(KeyCode.C)) //collision toggle
        {
            useCollisions = !useCollisions;
            print("Debug option: collisions toggled to " + useCollisions);
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
            
            //todo consider removal
            case "Fuel":
                //replenish fuel
                if (debug)
                {
                    print("Rocket collected fuelcell.");
                }

                break;

            case "Target":
                //next level!
                Transition();
                break;

            default:
                //calc damage
                if (CalcDamage(collision) && useCollisions)
                {
                    //death
                    Kill();
                }
                break;
        }
    }

    private void Transition()
    {
        if (debug)
        {
            print("Rocket landed!");
        }
        state = State.Transitioning;
        Invoke("LoadNextLevel", levelLoadDelay);
        HandleAudioEvent(goalReached);
        ClearParticles();
        goalReachedParticles.Play();
    }

    private void Kill()
    {
        state = State.Dying;
        Invoke("Restart", levelLoadDelay);
        HandleAudioEvent(deathNoise);
        ClearParticles();
        deathParticles.Play();
    }

    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < sceneCount)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Restart();
        }
    }
    private void Restart()
    {
        SceneManager.LoadScene(0);
    }

    private bool CalcDamage(Collision collision)
    {
        //todo test this!

        //get impulse applied to rocket on collision with generic obstacles.
        int velocity = (int)Math.Floor(collision.relativeVelocity.magnitude);
        //int impulse = (int)Math.Floor(collision.impulse.magnitude);

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
        rigidBody.AddRelativeForce(mainThrust * Vector3.up * Time.deltaTime); //ensure that impulse is FPS-agnostic
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
        rigidBody.angularVelocity = Vector3.zero; //all control of rotation is manual.

        float rotationThisFrame = rcsThrust * Time.deltaTime; //ensure that RPM is always same.

        Gyrate(rotationThisFrame);
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
