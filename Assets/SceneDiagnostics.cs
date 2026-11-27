using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SceneDiagnostics
{
    [MenuItem("Tools/Robot Scene Diagnostics")]
    public static void RunDiagnostics()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== ROBOT & SCENE DIAGNOSTICS ===");
        sb.AppendLine($"IsPlaying: {EditorApplication.isPlaying}");

        string[] groundNames = { "Plane", "ExtendedGround" };
        foreach (string gName in groundNames)
        {
            GameObject go = GameObject.Find(gName);
            if (go != null)
            {
                Renderer rend = go.GetComponent<Renderer>();
                float topY = rend != null ? rend.bounds.max.y : go.transform.position.y;
                sb.AppendLine($"Ground [{gName}]: pos={go.transform.position}, scale={go.transform.localScale}, topY={topY:F3}");
            }
            else
            {
                sb.AppendLine($"Ground [{gName}]: NOT FOUND");
            }
        }

        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
        {
            sb.AppendLine($"SpawnPoint: pos={spawn.transform.position}");
        }
        else
        {
            sb.AppendLine("SpawnPoint: NOT FOUND");
        }

        GameObject robot = GameObject.Find("Robot");
        if (robot != null)
        {
            sb.AppendLine($"Robot: pos={robot.transform.position}, rot={robot.transform.eulerAngles}");

            Rigidbody rb = robot.GetComponent<Rigidbody>();
            if (rb != null)
            {
                sb.AppendLine($"Rigidbody: mass={rb.mass}, drag={rb.drag}, angularDrag={rb.angularDrag}, useGravity={rb.useGravity}, isKinematic={rb.isKinematic}");
            }
            else
            {
                sb.AppendLine("Rigidbody: MISSING");
            }

            BoxCollider col = robot.GetComponent<BoxCollider>();
            if (col != null)
            {
                float localBottom = col.center.y - (col.size.y * 0.5f);
                sb.AppendLine($"BoxCollider: center={col.center}, size={col.size}, localBottomY={localBottom:F3}, boundsMinY={col.bounds.min.y:F3}, boundsMaxY={col.bounds.max.y:F3}, isTrigger={col.isTrigger}");
            }
            else
            {
                sb.AppendLine("BoxCollider: MISSING");
            }

            RobotMover mover = robot.GetComponent<RobotMover>();
            if (mover != null)
            {
                sb.AppendLine($"RobotMover: speed={mover.speed}, jumpForce={mover.jumpForce}, spawnPoint={(mover.spawnPoint != null ? mover.spawnPoint.name : "NULL")}, cameraTarget={(mover.cameraTarget != null ? mover.cameraTarget.name : "NULL")}");
            }
        }
        else
        {
            sb.AppendLine("Robot: NOT FOUND");
        }

        sb.AppendLine("=== SCRIPT SCAN (drag / angularDrag / MovePosition / velocity) ===");
        string[] csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        foreach (string file in csFiles)
        {
            if (file.EndsWith("SceneDiagnostics.cs")) continue;
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.Contains("drag") || l.Contains("Drag") || l.Contains("MovePosition") || l.Contains("velocity"))
                {
                    string relPath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
                    sb.AppendLine($"{relPath}:{i + 1} -> {l.Trim()}");
                }
            }
        }

        string report = sb.ToString();
        EditorGUIUtility.systemCopyBuffer = report;
        Debug.Log(report);
        Debug.Log("Rapor panoya kopyalandi (Ctrl+V ile yapistirabilirsin).");
    }
}