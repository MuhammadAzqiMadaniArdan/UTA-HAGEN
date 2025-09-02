using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StealthSystem : MonoBehaviour
{
    [Header("CCTV Settings")]
    public List<CCTVCamera> cctvCameras = new List<CCTVCamera>();

    [Header("UI Warning")]
    public TMPro.TextMeshProUGUI detectionText; // assign di inspector

    private FirstPersonController playerController;
    private bool isDetectedByCCTV = false;

    private void Start()
    {
        playerController = FindObjectOfType<FirstPersonController>();

        foreach (var cam in cctvCameras)
            cam.Init();

        if (detectionText != null)
            detectionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        bool detectedByAnyCamera = false;

        foreach (CCTVCamera camera in cctvCameras)
        {
            camera.UpdatePatrol();

            if (camera.IsPlayerInView(playerController.transform))
            {
                detectedByAnyCamera = true;
                GameManager.Instance.AddSuspicion(15f * Time.deltaTime); // naik per detik
                Debug.Log("⚠ Player TERLIHAT CCTV: " + camera.cameraTransform.name);
            }
        }

        if (detectedByAnyCamera != isDetectedByCCTV)
        {
            isDetectedByCCTV = detectedByAnyCamera;
            if (detectionText != null)
                detectionText.gameObject.SetActive(isDetectedByCCTV);
            if (isDetectedByCCTV)
                detectionText.text = "⚠ Kamu terlihat CCTV!";
        }
    }

    private void OnDrawGizmos()
    {
        if (cctvCameras == null) return;
        foreach (var cam in cctvCameras)
            cam.DrawGizmos();
    }
}

[System.Serializable]
public class CCTVCamera
{
    public Transform cameraTransform;
    public float detectionRange = 10f;
    [Range(10f, 180f)] public float fieldOfView = 90f;
    public LayerMask playerLayer;
    public LayerMask obstructionMask;

    [Header("Patrol Settings")]
    public bool enablePatrol = true;
    public float rotationSpeed = 30f;   // derajat per detik
    public float rotationAngle = 45f;   // batas kanan-kiri
    private float startY;
    private bool rotatingRight = true;
    public float tiltDownAngle = 40f; // sudut menunduk (X), bisa diatur di inspector

    [Header("Visuals")]
    public Light spotLight; // assign spotlight di inspector

    public void Init()
    {
        if (cameraTransform != null)
            startY = cameraTransform.eulerAngles.y;

        if (spotLight != null)
        {
            spotLight.spotAngle = fieldOfView;
            spotLight.range = detectionRange;
        }
    }

    public void UpdatePatrol()
    {
        if (!enablePatrol || cameraTransform == null) return;

        float targetY = startY + (rotatingRight ? rotationAngle : -rotationAngle);
        float currentY = cameraTransform.eulerAngles.y;

        float step = rotationSpeed * Time.deltaTime;
        float newY = Mathf.MoveTowardsAngle(currentY, targetY, step);

        // Apply rotasi hanya di Y, X dikunci nunduk
        cameraTransform.rotation = Quaternion.Euler(tiltDownAngle, newY, 0);

        if (Mathf.Abs(Mathf.DeltaAngle(newY, targetY)) < 0.5f)
            rotatingRight = !rotatingRight;

        // Update spotlight
        if (spotLight != null)
        {
            spotLight.spotAngle = fieldOfView;
            spotLight.range = detectionRange;
            spotLight.transform.rotation = cameraTransform.rotation;
        }
    }

    public bool IsPlayerInView(Transform playerTransform)
    {
        if (playerTransform == null || cameraTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(cameraTransform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerTransform.position - cameraTransform.position).normalized;
            float angle = Vector3.Angle(cameraTransform.forward, directionToPlayer);

            if (angle < fieldOfView / 2f)
            {
                // Mulai raycast sedikit di depan kamera supaya tidak ketabrak dirinya sendiri
                Vector3 rayOrigin = cameraTransform.position + cameraTransform.forward * 0.2f;

                if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, detectionRange, ~0))
                {
                    if (hit.transform == playerTransform)
                        return true;
                }
            }
        }
        return false;
    }

    // Gizmos untuk melihat FOV
    public void DrawGizmos()
    {
        if (cameraTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cameraTransform.position, detectionRange);

        Vector3 leftBoundary = DirFromAngle(-fieldOfView / 2, cameraTransform.eulerAngles.y);
        Vector3 rightBoundary = DirFromAngle(fieldOfView / 2, cameraTransform.eulerAngles.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + leftBoundary * detectionRange);
        Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + rightBoundary * detectionRange);

        // Forward line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * detectionRange);
    }

    private Vector3 DirFromAngle(float angleInDegrees, float globalYAngle)
    {
        float angle = angleInDegrees + globalYAngle;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}

[System.Serializable]
public class HidingSpot
{
    public Transform hidingTransform;
    public float hidingRadius = 2f;

    public bool IsPlayerHidden(Transform playerTransform)
    {
        if (playerTransform == null) return false;
        float distance = Vector3.Distance(hidingTransform.position, playerTransform.position);
        return distance <= hidingRadius;
    }
}
