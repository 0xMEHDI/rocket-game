using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] AudioClip engineSound;
    [SerializeField] AudioClip successSound;
    [SerializeField] AudioClip deathSound;

    [SerializeField] ParticleSystem thrustFX;

    [SerializeField] float verticalThrust = 100f;
    [SerializeField] float horizontalThrust = 100f;

    [SerializeField] float levelLoadDelay = 2f;

    Rigidbody rigidBody;
    AudioSource audioSource;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;

    bool collisionsEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            ProcessThrust();
            ProcessRotation();
        }
        if (Debug.isDebugBuild) 
            ProcessDebugKeys();       
    }

    private void ProcessDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
            LoadNextLevel();

        else if (Input.GetKeyDown(KeyCode.C))           
            collisionsEnabled = !collisionsEnabled;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !collisionsEnabled) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.PlayOneShot(successSound);
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (currentSceneIndex < lastSceneIndex) 
            SceneManager.LoadScene(nextSceneIndex);

        else if (currentSceneIndex == lastSceneIndex)
            LoadFirstLevel();
    }

    private void ProcessThrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Thrust();
            thrustFX.Play();
        }

        else
        {
            audioSource.Stop();
            thrustFX.Stop();
        }   
    }

    private void Thrust()
    {
        float thrustThisFrame = verticalThrust * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);

        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(engineSound);
    }

    private void ProcessRotation()
    {
        rigidBody.freezeRotation = true;
        float rotationThisFrame = horizontalThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.forward * rotationThisFrame);

        else if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.back * rotationThisFrame);

        rigidBody.freezeRotation = false;
    }
}