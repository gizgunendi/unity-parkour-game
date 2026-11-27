using UnityEngine;

public class CylinderPatrol : MonoBehaviour
{
    public float moveDistance = 1.6f;
    public float moveSpeed = 2.2f;
    public bool verticalMove = false;

    private Vector3 startPos;
    private float phaseOffset;

    void Start()
    {
        startPos = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed + phaseOffset) * moveDistance;

        if (verticalMove)
        {
            transform.position = new Vector3(startPos.x, startPos.y + Mathf.Abs(offset) * 0.5f, startPos.z);
        }
        else
        {
            transform.position = new Vector3(startPos.x + offset, startPos.y, startPos.z);
        }
    }
}
