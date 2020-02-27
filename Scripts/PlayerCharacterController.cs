using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCharacterController : MonoBehaviour {

    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;


    //Movement Stuff
    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float characterVelocityY;
    private Vector3 characterVelocityMomentum;

    //Camera Stuff
    private Camera playerCamera;
    private CameraFov cameraFov;

    //Particles
    private ParticleSystem speedLinesParticleSystem;

    //Hookshot
    private Vector3 hookshotPosition;
    private float hookshotSize;
    public float hookshotTransformX;
    public float hookshotTransformY;
    private State state;
    public float hookDistance;

    public AudioSource reelSound;
    public AudioSource thrownSound;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI restartText;

    public float reachedHookshotPositionDistance;



    private enum State {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
    }


    //Get all the components basically
    private void Awake() {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        cameraFov = playerCamera.GetComponent<CameraFov>();
        speedLinesParticleSystem = transform.Find("Camera").Find("SpeedLinesParticleSystem").GetComponent<ParticleSystem>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);



        AudioSource[] audios = GetComponents<AudioSource>();
        reelSound = audios[0];
        thrownSound = audios[1];
        winText.text = "";
        restartText.text = "";
    }

    private void Update() {

        //Switches between normal (no hookshot out), throwing the hookshot, and reeling in the player
        switch (state) {
        default:
        case State.Normal:
            HandleCharacterLook();
           HandleCharacterJoyStick();
            HandleCharacterMovement();
            HandleHookshotStart();
            break;
        case State.HookshotThrown:
            HandleHookshotThrow();
            HandleCharacterLook();
            HandleCharacterJoyStick();
            HandleCharacterMovement();
            break;
        case State.HookshotFlyingPlayer:
            HandleCharacterLook();
            HandleCharacterJoyStick();
            HandleHookshotMovement();
            break;
        }
    }


    private void HandleCharacterLook() {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        // Rotate  transform with the input speed around local Y axis
        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        // Add vertical inputs to the camera's vertical angle
        cameraVerticalAngle -= lookY * mouseSensitivity;

        // Limit the camera's vertical angle to min/max
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        // Make camera pivot up and down
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }

    private void HandleCharacterJoyStick()
    {
        float lookX = Input.GetAxisRaw("Joystick X");
        float lookY = Input.GetAxisRaw("Joystick Y");

        // Rotate  transform with the input speed around local Y axis
        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        // Add vertical inputs to the camera's vertical angle
        cameraVerticalAngle -= lookY * mouseSensitivity;

        // Limit the camera's vertical angle to min/max
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        // Make camera pivot up and down
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }

    private void HandleCharacterMovement() {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        float moveSpeed = 20f;

        Vector3 characterVelocity = transform.right * moveX * moveSpeed + transform.forward * moveZ * moveSpeed;

        if (characterController.isGrounded) {
            characterVelocityY = 0f;
            // Jump
            if (TestInputJump()) {
                float jumpSpeed = 30f;
                characterVelocityY = jumpSpeed;
            }
        }

        // Apply gravity to the velocity
        float gravityDownForce = -60f;
        characterVelocityY += gravityDownForce * Time.deltaTime;

        // Apply Y velocity to move vector
        characterVelocity.y = characterVelocityY;

        // Apply momentum
        characterVelocity += characterVelocityMomentum;

        // Move Character Controller
        characterController.Move(characterVelocity * Time.deltaTime);

        // Dampen momentum
        if (characterVelocityMomentum.magnitude > 0f) {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f) {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void ResetGravityEffect() {
        characterVelocityY = 0f;
    }

    //When player presses button to launch the hookshot
    private void HandleHookshotStart() {
        if (TestInputDownHookshot()) {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit)) {
                // Hit something
                float dist = Vector3.Distance(playerCamera.transform.position, raycastHit.point);
                if (dist <= hookDistance)
                {
                  //  thrownSound.Play();
                    debugHitPointTransform.position = raycastHit.point;
                    hookshotPosition = raycastHit.point;
                    hookshotSize = 0f;
                    hookshotTransform.gameObject.SetActive(true);
                    hookshotTransform.localScale = Vector3.zero;
                    state = State.HookshotThrown;
                }
            }
        }
    }


    //While hookshot is travelling to position
    private void HandleHookshotThrow() {
        hookshotTransform.gameObject.SetActive(true);
        hookshotTransform.LookAt(hookshotPosition);
        reelSound.Play();
        float hookshotThrowSpeed = 500f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(hookshotTransformX, hookshotTransformY, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPosition)) {
            state = State.HookshotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
            speedLinesParticleSystem.Play();
        }
    }


    //While hookshot is moving player
    private void HandleHookshotMovement() {
        hookshotTransform.LookAt(hookshotPosition);

        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 5f;

        // Move Character Controller
        characterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

       
        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPositionDistance) {
            // Reached Hookshot Position
            StopHookshot();
        }

        if (TestInputDownHookshot()) {
            // Cancel Hookshot
            StopHookshot();
        }

        if (TestInputJump()) {
            // Cancelled with Jump
            float momentumExtraSpeed = 7f;
            characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 40f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookshot();
        }
    }

    private void StopHookshot() {
        reelSound.Stop();
        state = State.Normal;
        ResetGravityEffect();
        hookshotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
        speedLinesParticleSystem.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MacGuffin")
        {
            winText.text = "MISSION COMPLETE";
            restartText.text = "PRESS R TO RETURN TO MENU";
            Destroy(this);
            thrownSound.Play();
            Debug.Log("Win");
        }
        if (other.tag == "Enemy")
        {
            winText.text = "MISSION FAILED";
            restartText.text = "PRESS R TO RETURN TO MENU";
            Destroy(this);
            thrownSound.Play();
            Debug.Log("Lose");
        }
    }

    /*  
      private void OnCollisionEnter(Collision collision)
      {

          Debug.Log(collision.collider.name);
          if (collision.collider.tag == "MacGuffin")
          {
              Debug.Log("Win");
          }
      }
      */


    //if Player tries to use Hookshot
    //Possibly change this to mouse input instead of E (or both)
    private bool TestInputDownHookshot() {

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown("joystick button 3"))
            return true;
        else return false;
    }


    //If player tries to jump
    private bool TestInputJump() {

        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 0"))
            return true;
        else return false;
        
    }

}
