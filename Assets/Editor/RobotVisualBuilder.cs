using UnityEditor;
using UnityEngine;

public class RobotVisualBuilder : EditorWindow
{
    [MenuItem("Tools/Build Styled Robot")]
    public static void BuildRobotVisuals()
    {
        GameObject robot = GameObject.Find("Robot");
        if (robot == null)
        {
            EditorUtility.DisplayDialog("Robot Bulunamadı", "Sahnede 'Robot' adında bir obje bulunamadı.", "Tamam");
            return;
        }

        int removedMissingScripts = RemoveMissingScriptsInScene();

        Material darkMat = FindMaterialByName("RobotDarkMat");
        Material glowMat = FindMaterialByName("RobotGlowMat");
        Material bodyCyanMat = FindOrCreateBodyCyanMaterial();
        Material headLightMat = FindOrCreateHeadLightMaterial();
        Material groundPastelMat = FindOrCreateGroundPastelMaterial();

        ConfigureMaterial(
            darkMat,
            new Color(0.14f, 0.16f, 0.2f),
            0.45f,
            0.55f,
            Color.black,
            false
        );

        ConfigureMaterial(
            glowMat,
            new Color(0.15f, 1f, 0.25f),
            0.15f,
            0.85f,
            new Color(0.2f, 1f, 0.3f) * 2.2f,
            true
        );

        Transform body = robot.transform.Find("Body");
        if (body != null)
        {
            Undo.RecordObject(body, "Update Body");
            body.localPosition = new Vector3(0f, 0.12f, 0f);
            body.localEulerAngles = Vector3.zero;
            body.localScale = new Vector3(0.76f, 0.72f, 0.64f);
            ApplyMaterialAndCleanCollider(body, bodyCyanMat);
        }

        Transform chest = robot.transform.Find("ChestPanel");
        if (chest != null)
        {
            Undo.RecordObject(chest, "Update ChestPanel");
            chest.localPosition = new Vector3(0f, 0.14f, 0.34f);
            chest.localEulerAngles = Vector3.zero;
            chest.localScale = new Vector3(0.44f, 0.34f, 0.05f);
            ApplyMaterialAndCleanCollider(chest, darkMat);
        }

        Transform head = robot.transform.Find("Head");
        if (head != null)
        {
            Undo.RecordObject(head, "Update Head");
            head.localPosition = new Vector3(0f, 0.72f, 0.02f);
            head.localEulerAngles = Vector3.zero;
            head.localScale = new Vector3(0.54f, 0.44f, 0.54f);
            ApplyMaterialAndCleanCollider(head, headLightMat);

            Transform visor = head.Find("Visor");
            if (visor != null)
            {
                Undo.RecordObject(visor, "Update Visor");
                visor.localPosition = new Vector3(0f, 0.04f, 0.46f);
                visor.localEulerAngles = Vector3.zero;
                visor.localScale = new Vector3(0.76f, 0.34f, 0.16f);
                ApplyMaterialAndCleanCollider(visor, darkMat);
            }

            Transform leftEye = head.Find("LeftEye");
            if (leftEye != null)
            {
                Undo.RecordObject(leftEye, "Update LeftEye");
                leftEye.localPosition = new Vector3(-0.2f, 0.05f, 0.54f);
                leftEye.localScale = new Vector3(0.14f, 0.16f, 0.12f);
                ApplyMaterialAndCleanCollider(leftEye, glowMat);
            }

            Transform rightEye = head.Find("RightEye");
            if (rightEye != null)
            {
                Undo.RecordObject(rightEye, "Update RightEye");
                rightEye.localPosition = new Vector3(0.2f, 0.05f, 0.54f);
                rightEye.localScale = new Vector3(0.14f, 0.16f, 0.12f);
                ApplyMaterialAndCleanCollider(rightEye, glowMat);
            }

            Transform rod = head.Find("AntennaRod");
            if (rod != null)
            {
                Undo.RecordObject(rod, "Update AntennaRod");
                rod.localPosition = new Vector3(0f, 0.68f, 0f);
                rod.localEulerAngles = Vector3.zero;
                rod.localScale = new Vector3(0.08f, 0.28f, 0.08f);
                ApplyMaterialAndCleanCollider(rod, darkMat);
            }
        }

        Transform antennaParent = head != null ? head : robot.transform;
        Vector3 tipPos = head != null ? new Vector3(0f, 1.02f, 0f) : new Vector3(0f, 1.25f, 0f);
        Vector3 tipScale = head != null ? new Vector3(0.22f, 0.26f, 0.22f) : new Vector3(0.14f, 0.14f, 0.14f);

        CreateOrUpdatePart(antennaParent, "AntennaTip", PrimitiveType.Sphere, tipPos, Vector3.zero, tipScale, glowMat);

        CreateOrUpdatePart(robot.transform, "LeftShoulder", PrimitiveType.Sphere,
            new Vector3(-0.45f, 0.24f, 0f), Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), darkMat);

        CreateOrUpdatePart(robot.transform, "LeftArm", PrimitiveType.Capsule,
            new Vector3(-0.48f, -0.02f, 0.04f), new Vector3(8f, 0f, 0f), new Vector3(0.14f, 0.22f, 0.14f), bodyCyanMat);

        CreateOrUpdatePart(robot.transform, "RightShoulder", PrimitiveType.Sphere,
            new Vector3(0.45f, 0.24f, 0f), Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), darkMat);

        CreateOrUpdatePart(robot.transform, "RightArm", PrimitiveType.Capsule,
            new Vector3(0.48f, -0.02f, 0.04f), new Vector3(8f, 0f, 0f), new Vector3(0.14f, 0.22f, 0.14f), bodyCyanMat);

        CreateOrUpdatePart(robot.transform, "LeftWheel", PrimitiveType.Cylinder,
            new Vector3(-0.32f, -0.26f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.26f, 0.09f, 0.26f), darkMat);

        CreateOrUpdatePart(robot.transform, "RightWheel", PrimitiveType.Cylinder,
            new Vector3(0.32f, -0.26f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.26f, 0.09f, 0.26f), darkMat);

        ApplyGroundPastelMaterialToAllGrounds(groundPastelMat);
        EnsureRobotBoxCollider(robot.transform);
        StabilizeRigidbody(robot.transform);
        AlignRobotBottomToGround(robot.transform, 0f);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Tamamlandı",
            "Robot'a BoxCollider eklendi, eksik script (" + removedMissingScripts + ") temizlendi ve hizalandı.",
            "Tamam"
        );
    }

    private static void EnsureRobotBoxCollider(Transform robotTransform)
    {
        BoxCollider box = robotTransform.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = Undo.AddComponent<BoxCollider>(robotTransform.gameObject);
        }

        Undo.RecordObject(box, "Configure Robot BoxCollider");
        box.center = new Vector3(0f, 0.05f, 0f);
        box.size = new Vector3(0.9f, 0.8f, 0.8f);
        box.isTrigger = false;
    }

    private static int RemoveMissingScriptsInScene()
    {
        int totalRemoved = 0;
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Scripts");
                    totalRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                }
            }
        }
        return totalRemoved;
    }

    private static Material FindOrCreateHeadLightMaterial()
    {
        Material existing = FindMaterialByName("RobotHeadLightMat");
        if (existing == null)
        {
            Shader shader = Shader.Find("Standard");
            existing = new Material(shader);
            AssetDatabase.CreateAsset(existing, "Assets/RobotHeadLightMat.mat");
        }

        ConfigureMaterial(
            existing,
            new Color(0.85f, 0.88f, 0.92f),
            0.55f,
            0.7f,
            Color.black,
            false
        );

        return existing;
    }

    private static Material FindOrCreateGroundPastelMaterial()
    {
        Material existing = FindMaterialByName("GroundPastelMat");
        if (existing == null)
        {
            Shader shader = Shader.Find("Standard");
            existing = new Material(shader);
            AssetDatabase.CreateAsset(existing, "Assets/GroundPastelMat.mat");
        }

        ConfigureMaterial(
            existing,
            new Color(0.62f, 0.66f, 0.72f),
            0.05f,
            0.2f,
            Color.black,
            false
        );

        return existing;
    }

    private static void ApplyGroundPastelMaterialToAllGrounds(Material groundMat)
    {
        if (groundMat == null)
        {
            return;
        }

        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer renderer in allRenderers)
        {
            string objName = renderer.gameObject.name.ToLower();
            if (objName.Contains("ground") || objName.Contains("plane") || objName.Contains("floor") || objName.Contains("platform"))
            {
                Undo.RecordObject(renderer, "Assign Pastel Ground Material");
                renderer.sharedMaterials = new Material[] { groundMat };
            }
        }
    }

    private static Material FindOrCreateBodyCyanMaterial()
    {
        Material existing = FindMaterialByName("RobotBodyCyanMat");
        if (existing == null)
        {
            Shader shader = Shader.Find("Standard");
            existing = new Material(shader);
            AssetDatabase.CreateAsset(existing, "Assets/RobotBodyCyanMat.mat");
        }

        ConfigureMaterial(
            existing,
            new Color(0.05f, 0.9f, 1f),
            0.35f,
            0.78f,
            new Color(0.02f, 0.35f, 0.45f),
            true
        );

        return existing;
    }

    private static void StabilizeRigidbody(Transform robotTransform)
    {
        Rigidbody rb = robotTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Undo.RecordObject(rb, "Stabilize Rigidbody");
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            rb.angularDrag = 10f;
            rb.drag = 2f;
        }
    }

    private static void ConfigureMaterial(
        Material mat,
        Color baseColor,
        float metallic,
        float smoothness,
        Color emissionColor,
        bool enableEmission
    )
    {
        if (mat == null)
        {
            return;
        }

        Undo.RecordObject(mat, "Configure Material");
        mat.color = baseColor;

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        if (mat.HasProperty("_Glossiness"))
        {
            mat.SetFloat("_Glossiness", smoothness);
        }

        if (enableEmission)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emissionColor);
            }
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }

        EditorUtility.SetDirty(mat);
    }

    private static void AlignRobotBottomToGround(Transform robotTransform, float groundY)
    {
        Renderer[] renderers = robotTransform.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        float minY = float.MaxValue;
        foreach (Renderer r in renderers)
        {
            if (r.bounds.min.y < minY)
            {
                minY = r.bounds.min.y;
            }
        }

        float deltaY = groundY - minY;
        Undo.RecordObject(robotTransform, "Align Robot To Ground");
        robotTransform.position += new Vector3(0f, deltaY, 0f);
    }

    private static Material FindMaterialByName(string matName)
    {
        string[] guids = AssetDatabase.FindAssets(matName + " t:Material");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }
        return null;
    }

    private static void CreateOrUpdatePart(
        Transform parent,
        string partName,
        PrimitiveType primitiveType,
        Vector3 localPos,
        Vector3 localEuler,
        Vector3 localScale,
        Material mat
    )
    {
        Transform existing = parent.Find(partName);
        GameObject partObj = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);

        RegisterCreatedObjectUndoIfNeeded(existing == null, partObj, "Create " + partName);
        Undo.RecordObject(partObj.transform, "Update " + partName);

        partObj.name = partName;
        partObj.transform.SetParent(parent, false);
        partObj.transform.localPosition = localPos;
        partObj.transform.localEulerAngles = localEuler;
        partObj.transform.localScale = localScale;

        ApplyMaterialAndCleanCollider(partObj.transform, mat);
    }

    private static void ApplyMaterialAndCleanCollider(Transform target, Material mat)
    {
        if (target == null)
        {
            return;
        }

        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            Undo.DestroyObjectImmediate(col);
        }

        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null && mat != null)
        {
            Undo.RecordObject(renderer, "Assign Material");
            renderer.sharedMaterials = new Material[] { mat };
        }
    }

    private static void RegisterCreatedObjectUndoIfNeeded(bool isNew, GameObject obj, string name)
    {
        if (isNew && obj != null)
        {
            Undo.RegisterCreatedObjectUndo(obj, name);
        }
    }
}
