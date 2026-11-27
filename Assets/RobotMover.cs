using UnityEngine;

public class RobotMover : MonoBehaviour
{
    public float speed = 6.5f;
    public float jumpForce = 14f;
    public Transform spawnPoint;
    public Transform cameraTarget;
    public float cameraSmoothSpeed = 10f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isMovementLocked = false;
    private float fixedCameraY;
    private float fixedCameraZ;
    private float cameraOffsetX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.drag = 2f;
            rb.angularDrag = 10f;
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }

        if (cameraTarget == null && Camera.main != null)
        {
            cameraTarget = Camera.main.transform;
        }

        if (cameraTarget != null)
        {
            fixedCameraY = cameraTarget.position.y;
            fixedCameraZ = cameraTarget.position.z;
            cameraOffsetX = cameraTarget.position.x - transform.position.x;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && spawnPoint != null)
        {
            RestartGame();
            return;
        }

        if (isMovementLocked)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        if (isMovementLocked || rb == null)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float targetVelX = h * speed;
        rb.velocity = new Vector3(targetVelX, rb.velocity.y, 0f);
    }

    void LateUpdate()
    {
        if (cameraTarget != null)
        {
            float desiredX = transform.position.x + cameraOffsetX;
            float newX = Mathf.Lerp(cameraTarget.position.x, desiredX, cameraSmoothSpeed * Time.deltaTime);
            cameraTarget.position = new Vector3(newX, fixedCameraY, fixedCameraZ);
        }
    }

    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            if (locked)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }

    public void KillRobot(string failMessage = "GAME OVER! You hit a cylinder obstacle! Press R to restart.")
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.OnPlayerHitHazard(failMessage);
        }
        else
        {
            SetMovementLocked(true);
            GameMessageUI.ShowMessage(failMessage);
        }
    }

    public void RestartGame()
    {
        isMovementLocked = false;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.drag = 2f;
            rb.angularDrag = 10f;
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;

            if (cameraTarget != null)
            {
                cameraTarget.position = new Vector3(spawnPoint.position.x + cameraOffsetX, fixedCameraY, fixedCameraZ);
            }
        }

        CourseGenerator generator = FindObjectOfType<CourseGenerator>();
        if (generator != null)
        {
            generator.RebuildCourse();
        }
        else
        {
            StarCollector.ResetStarCounter();
        }

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.ResetSessionCounters(keepPanelOpen: false);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}