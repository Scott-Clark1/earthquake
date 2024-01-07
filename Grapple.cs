using UnityEngine;

public class Grapple : MonoBehaviour
{
    public Camera cameraObj;
    public bool isGrappling = false;
    public float range = 100f;

    public Rigidbody rb;

    private RaycastHit grappleHit;
    private float grappleLength;
    public float grappleTol = 0.1f;

    public LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer =  GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.0f;
        lineRenderer.endWidth = 0.0f;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Grapplin!");
            if (!isGrappling) {
                int layerMask = 1 << 10; // mask to only hit  "ground"
                bool didHit = Physics.Raycast(
                    cameraObj.transform.position,
                    cameraObj.transform.forward,
                    out grappleHit,
                    range,
                    layerMask);
                if (didHit) {
                    grappleLength = (rb.position - grappleHit.point).magnitude;
                    isGrappling = true;
                    lineRenderer.enabled = true;
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.1f;
                }
            } else {
                lineRenderer.enabled = false;
                isGrappling = false;
            }
        } else if (isGrappling) {
            // Debug.DrawRay(rb.position, (grappleHit.point - rb.position), Color.yellow);
            lineRenderer.SetPosition(0, grappleHit.point);
            lineRenderer.SetPosition(1, rb.position);
            if ((rb.position - grappleHit.point).magnitude >= (grappleLength + grappleTol)) {
                Vector3 normal = (rb.position - grappleHit.point).normalized;
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, normal);
            }
        }
    }
}
