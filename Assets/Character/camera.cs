using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Ціль (Заповнюється автоматично)")]
    public Transform target;

    [Header("Налаштування")]
    public float distance = 5f;
    public float minDistance = 1f;
    public float height = 1.5f;
    public float sensitivity = 200f;

    [Header("Плавність")]
    public float rotationSmoothTime = 0.12f; 
    public float autoResetSmooth = 1f;   

    [Header("Авто-скидання (Опціонально)")]
    public float autoResetDelay = 5f;


    [Header("Колізія")]
    public LayerMask collisionLayers;
    public Vector2 pitchMinMax = new Vector2(-40, 85);


    private float yaw;   
    private float pitch; 
    private float yawVelocity;
    private float pitchVelocity;
    private float lastInputTime;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lastInputTime = Time.time;
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0) return;
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;

                yaw = target.eulerAngles.y;
                pitch = 20f;
            }
            else
            {

                return;
            }
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;


        if (Mathf.Abs(mouseX) > 0.001f || Mathf.Abs(mouseY) > 0.001f)
        {
            lastInputTime = Time.time;
        }


        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);


        if (Time.time - lastInputTime > autoResetDelay)
        {

            float playerAngle = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, playerAngle, autoResetSmooth * Time.deltaTime);
            pitch = Mathf.Lerp(pitch, 20f, autoResetSmooth * Time.deltaTime);
        }

        float currentYaw = transform.eulerAngles.y;
        float currentPitch = transform.eulerAngles.x;

        float smoothYaw = Mathf.SmoothDampAngle(currentYaw, yaw, ref yawVelocity, rotationSmoothTime);
        float smoothPitch = Mathf.SmoothDampAngle(currentPitch, pitch, ref pitchVelocity, rotationSmoothTime);

        Quaternion finalRotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);

        Vector3 focusPoint = target.position + Vector3.up * height;

        Vector3 direction = finalRotation * -Vector3.forward;

        float targetDistance = distance;
        RaycastHit hit;

        if (Physics.SphereCast(focusPoint, 0.2f, direction, out hit, distance, collisionLayers))
        {
            targetDistance = hit.distance;
            // Ми прибираємо перевірку minDistance тут, бо стіна важливіша за комфортну дистанцію
        }
        // Якщо хочеш, щоб камера не входила в голову персонажа, краще налаштувати Near Clip Plane (див. пункт 3)

        // 8. Застосування позиції та повороту
        transform.position = focusPoint + direction * targetDistance;
        transform.rotation = finalRotation;
    }
}