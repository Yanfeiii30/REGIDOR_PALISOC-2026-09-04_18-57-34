using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    [Header("Camera Views")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public GameObject firstPersonGun;
    public GameObject crosshair;
    public float normalFOV = 60f;
    public float scopeFOV = 35f;

    [Header("Robot Animation")]
    public Animator robotAnimator;

    [Header("Jump")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 1.15f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float aimDistance = 100f;
    public float bulletSpawnOffset = 0.08f;
    public ParticleSystem muzzleFlash;

    [Header("Ammo")]
    public int maxAmmo = 6;
    public TextMeshProUGUI ammoText;
    public int CurrentAmmo { get; private set; }

    private float xRotation;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isScoping;
    private bool checkingLastShot;
    private Quaternion firstPersonBaseRotation;
    private Quaternion thirdPersonBaseRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();

        CurrentAmmo = maxAmmo;
        UpdateAmmoText();

        if (firstPersonCamera != null)
        {
            firstPersonBaseRotation = firstPersonCamera.transform.localRotation;
            firstPersonCamera.fieldOfView = scopeFOV;
        }

        if (thirdPersonCamera != null)
        {
            thirdPersonBaseRotation = thirdPersonCamera.transform.localRotation;
            thirdPersonCamera.fieldOfView = normalFOV;
        }

        SetCameraMode(false);
    }

    void Update()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.IsLevelEnded)
        {
            return;
        }

        HandleCameraMode();
        HandleMouseLook();
        UpdateGroundedState();
        HandleMovement();
        HandleJump();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void HandleCameraMode()
    {
        bool shouldScope = Input.GetMouseButton(1);

        if (shouldScope != isScoping)
        {
            SetCameraMode(shouldScope);
        }
    }

    void SetCameraMode(bool shouldScope)
    {
        isScoping = shouldScope;

        if (firstPersonCamera != null)
        {
            firstPersonCamera.enabled = shouldScope;

            AudioListener listener = firstPersonCamera.GetComponent<AudioListener>();

            if (listener != null)
            {
                listener.enabled = shouldScope;
            }
        }

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.enabled = !shouldScope;

            AudioListener listener = thirdPersonCamera.GetComponent<AudioListener>();

            if (listener != null)
            {
                listener.enabled = !shouldScope;
            }
        }

        if (firstPersonGun != null)
        {
            firstPersonGun.SetActive(shouldScope);
        }

        if (crosshair != null)
        {
            crosshair.SetActive(shouldScope);
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Quaternion pitchRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (firstPersonCamera != null)
        {
            firstPersonCamera.transform.localRotation =
                firstPersonBaseRotation * pitchRotation;
        }

        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.transform.localRotation =
                thirdPersonBaseRotation * pitchRotation;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        bool isMoving = move.sqrMagnitude > 0.01f;

        bool isRunning = isMoving &&
            (Input.GetKey(KeyCode.LeftShift) ||
             Input.GetKey(KeyCode.RightShift));

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isMoving)
        {
            transform.Translate(
                move.normalized * currentSpeed * Time.deltaTime,
                Space.World
            );
        }

        if (robotAnimator != null)
        {
            robotAnimator.SetBool("IsMoving", isMoving);
            robotAnimator.SetBool("IsRunning", isRunning);
        }
    }

    void UpdateGroundedState()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.05f;

        isGrounded = Physics.Raycast(
            rayStart,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || !isGrounded || rb == null)
        {
            return;
        }

        if (robotAnimator != null)
        {
            robotAnimator.SetTrigger("Jump");
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Shoot()
    {
        Camera aimCamera = isScoping ? firstPersonCamera : thirdPersonCamera;

        if (aimCamera == null)
        {
            aimCamera = firstPersonCamera;
        }

        if (CurrentAmmo <= 0 ||
            bulletPrefab == null ||
            firePoint == null ||
            aimCamera == null)
        {
            return;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play(true);
        }

        Ray aimRay = new Ray(
            aimCamera.transform.position,
            aimCamera.transform.forward
        );

        Vector3 aimPoint = aimRay.GetPoint(aimDistance);

        if (Physics.Raycast(
            aimRay,
            out RaycastHit hit,
            aimDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Vector3 shotDirection = (aimPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position + shotDirection * bulletSpawnOffset,
            Quaternion.LookRotation(shotDirection)
        );

        BounceBullet bulletScript = bullet.GetComponent<BounceBullet>();

        if (bulletScript != null)
        {
            bulletScript.Launch(shotDirection);
        }

        CurrentAmmo--;
        UpdateAmmoText();

        if (CurrentAmmo == 0 && !checkingLastShot)
        {
            StartCoroutine(CheckForAmmoFailure());
        }
    }

    IEnumerator CheckForAmmoFailure()
    {
        checkingLastShot = true;

        yield return null;

        while (LevelManager.Instance != null &&
               !LevelManager.Instance.IsLevelEnded &&
               FindAnyObjectByType<BounceBullet>() != null)
        {
            yield return null;
        }

        if (LevelManager.Instance != null &&
            !LevelManager.Instance.IsLevelEnded &&
            LevelManager.Instance.CorrectObjectives <
            LevelManager.Instance.totalObjectives)
        {
            LevelManager.Instance.FailLevel("OUT OF AMMO!");
        }

        checkingLastShot = false;
    }

    void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = "AMMO: " + CurrentAmmo + " / " + maxAmmo;
        }
    }
}