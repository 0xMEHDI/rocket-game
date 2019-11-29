using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [SerializeField] AudioClip engineSound;
    [SerializeField] AudioClip successSound;
    [SerializeField] AudioClip deathSound;

    [SerializeField] GameObject engineFX;
    [SerializeField] GameObject successFX;
    [SerializeField] GameObject deathFX;
    
    [SerializeField] float verticalThrust = 1000f;
    [SerializeField] float horizontalThrust = 100f;

    [SerializeField] float levelLoadDelay = 2f;

    Rigidbody rigidBody;
    AudioSource audioSource;
    MeshRenderer[] meshes;
    Light pointLight;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;

    bool collisionsEnabled = true;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        meshes = GetComponentsInChildren<MeshRenderer>();
        pointLight = GetComponentInChildren<Light>();

        successFX.gameObject.SetActive(false);
        deathFX.SetActive(false);
    }

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

        if (collision.gameObject.CompareTag("Finish"))
            StartSuccessSequence();

        else if (!collision.gameObject.CompareTag("Friendly"))
            StartDeathSequence();
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;

        audioSource.PlayOneShot(successSound);

        successFX.SetActive(true);

        rigidBody.isKinematic = true;

        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;

        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].enabled = false;
        }

        pointLight.gameObject.SetActive(false);
        engineFX.gameObject.SetActive(false);
        deathFX.SetActive(true);

        rigidBody.isKinematic = true;

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
            engineFX.gameObject.SetActive(true);
        }
            
        else
        {
            audioSource.Stop();
            engineFX.gameObject.SetActive(false);
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

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.forward * rotationThisFrame);

        else if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.back * rotationThisFrame);

        rigidBody.freezeRotation = false;
    }
}