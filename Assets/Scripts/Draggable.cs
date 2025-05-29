using UnityEngine;

public class DraggablePoint : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private float distance;

    void Start()
    {
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        isDragging = true;
        distance = Vector3.Distance(transform.position, cam.transform.position);
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // Project mouse ray onto the same Z plane as the object
            Plane plane = new Plane(Vector3.forward, transform.position);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                transform.position = new Vector3(hitPoint.x, hitPoint.y, transform.position.z);
            }
        }
    }
}