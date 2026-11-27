using System.Collections.Generic;
using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [Header("Parkur Ayarlari")]
    public int starCount = 10;
    public float stepX = 8.0f;
    public float fixedZ = 0f;

    [Header("Silindir Hareketi")]
    public float cylinderMoveDistance = 1.6f;
    public float cylinderMoveSpeed = 2.4f;

    [Header("Zemin ve Gorunum")]
    public float startX = -3f;
    public float groundY = -0.5f;
    public float groundThickness = 1f;
    public Color skyColor = new Color(0.12f, 0.15f, 0.24f);

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        CourseGenerator[] allGenerators = FindObjectsOfType<CourseGenerator>();
        if (allGenerators.Length > 1 && allGenerators[0] != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = skyColor;
        }

        CleanupStaleObjects();
        BuildCourse();
    }

    public void RebuildCourse()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                spawnedObjects[i].SetActive(false);
                Destroy(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();

        CleanupStaleObjects();
        BuildCourse();
        StarCollector.ResetStarCounter();
    }

    private void CleanupStaleObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj == null || obj == gameObject)
            {
                continue;
            }

            if (obj.name == "ExtendedGround" ||
                obj.name.StartsWith("Obstacle_") ||
                obj.name.StartsWith("HazardCylinder_") ||
                obj.name.StartsWith("Star_"))
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
    }

    private void BuildCourse()
    {
        int activeStarCount = Mathf.Max(1, starCount);
        float totalLength = (activeStarCount + 2) * stepX + 18f;
        float centerX = startX + totalLength * 0.5f - 4f;

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "ExtendedGround";
        ground.transform.position = new Vector3(centerX, groundY, fixedZ);
        ground.transform.localScale = new Vector3(totalLength, groundThickness, 8f);
        ApplyColoredMaterial(ground, new Color(0.22f, 0.25f, 0.33f));
        spawnedObjects.Add(ground);

        PrimitiveType[] obstacleShapes =
        {
            PrimitiveType.Cube,
            PrimitiveType.Cylinder,
            PrimitiveType.Sphere
        };

        float surfaceY = groundY + groundThickness * 0.5f;

        for (int i = 0; i < activeStarCount; i++)
        {
            float baseX = startX + (i + 1) * stepX;

            PrimitiveType shape = obstacleShapes[Random.Range(0, obstacleShapes.Length)];
            GameObject obstacle = GameObject.CreatePrimitive(shape);

            float sx = Random.Range(1.2f, 1.9f);
            float sy = shape == PrimitiveType.Cylinder ? Random.Range(1.2f, 1.8f) : Random.Range(1.0f, 1.7f);
            float sz = shape == PrimitiveType.Sphere ? sx : Random.Range(1.2f, 1.9f);

            obstacle.transform.localScale = new Vector3(sx, sy, sz);
            float placedY = surfaceY + sy * 0.5f;
            obstacle.transform.position = new Vector3(baseX, placedY, fixedZ);

            StarCollector wrongStar = obstacle.GetComponent<StarCollector>();
            if (wrongStar != null)
            {
                Destroy(wrongStar);
            }

            Collider obsCol = obstacle.GetComponent<Collider>();
            if (obsCol != null)
            {
                obsCol.isTrigger = false;
            }

            if (shape == PrimitiveType.Cylinder)
            {
                obstacle.name = "HazardCylinder_" + (i + 1);
                obstacle.AddComponent<CylinderHazard>();

                Rigidbody cylRb = obstacle.AddComponent<Rigidbody>();
                cylRb.isKinematic = true;
                cylRb.useGravity = false;
                cylRb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

                CylinderPatrol patrol = obstacle.AddComponent<CylinderPatrol>();
                patrol.moveDistance = cylinderMoveDistance;
                patrol.moveSpeed = cylinderMoveSpeed + Random.Range(-0.4f, 0.6f);
                patrol.verticalMove = (i % 3 == 0);

                ApplyColoredMaterial(obstacle, new Color(0.95f, 0.16f, 0.2f));
            }
            else
            {
                obstacle.name = "Obstacle_" + (i + 1);
                Color obstacleColor = Color.HSVToRGB((i * 0.13f + Random.Range(0f, 0.1f)) % 1f, 0.78f, 0.95f);
                ApplyColoredMaterial(obstacle, obstacleColor);
            }

            spawnedObjects.Add(obstacle);

            float starX = baseX + (stepX * 0.5f);
            float starY = surfaceY + Random.Range(1.1f, 2.0f);

            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Star_" + (i + 1);
            star.transform.localScale = Vector3.one * 0.85f;
            star.transform.position = new Vector3(starX, starY, fixedZ);

            Collider starCol = star.GetComponent<Collider>();
            starCol.isTrigger = true;

            ApplyColoredMaterial(star, new Color(1f, 0.84f, 0.08f));
            star.AddComponent<StarCollector>();
            spawnedObjects.Add(star);
        }
    }

    private void ApplyColoredMaterial(GameObject target, Color color)
    {
        Renderer rend = target.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.sharedMaterial);
            mat.color = color;
            rend.material = mat;
        }
    }
}