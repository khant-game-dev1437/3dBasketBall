using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    public bool lineShow = true;
    LineRenderer lr;
    int maxPoints = 50;
    float timeStep = 0.04f;

    void Awake()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = maxPoints;
        lr.startWidth = 0.06f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = new Color(0.2f, 0.5f, 1f);
        lr.startColor = new Color(0.2f, 0.5f, 1f, 1f);
        lr.endColor = new Color(0.1f, 0.3f, 0.8f, 1f);
        lr.enabled = false;

        if (lineShow)
            Show();
        else
            Hide();
    }

    public void UpdateTrajectory(Vector3 startPos, Vector3 velocity)
    {
        if (!lr.enabled) return;
        if (velocity.magnitude < 0.5f)
        {
            lr.positionCount = 0;
            return;
        }

        Vector3 gravity = Physics.gravity;
        lr.positionCount = maxPoints;

        for (int i = 0; i < maxPoints; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * gravity * t * t;
            lr.SetPosition(i, point);

            if (point.y < 0f)
            {
                lr.positionCount = i + 1;
                break;
            }
        }
    }

    public void AimLineToggle()
    {
        if (lineShow)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        lr.enabled = true;
        lr.positionCount = maxPoints;
    }

    public void Hide()
    {
        lr.enabled = false;
    }
}
