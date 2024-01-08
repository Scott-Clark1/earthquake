using UnityEngine;

public class Grapple : MonoBehaviour
{
    public Camera cameraObj;
    public bool isGrappling = false;
    public float range = 100f;

    public Rigidbody rb;

    private RaycastHit grappleHit;
    private float grappleLength;
    public float grappleVel = 0.1f;

    public float minGrappleLength = 0.5f;
    public LineRenderer lineRenderer;
    public float termRadialVel;

    public PlayerMvmt player;

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

                    Vector3 normal = (grappleHit.point - rb.position).normalized;
                    float orthoVelMag = Vector3.ProjectOnPlane(rb.velocity, normal).magnitude;
                    termRadialVel = Mathf.Max(player.termGroundVel, orthoVelMag); // FIXME: this sucks, capping player vel is very antisource
                }
            } else {
                lineRenderer.enabled = false;
                isGrappling = false;
            }
        } else if (isGrappling) {
            // Debug.DrawRay(rb.position, (grappleHit.point - rb.position), Color.yellow);
            lineRenderer.SetPosition(0, grappleHit.point);
            lineRenderer.SetPosition(1, rb.position);

            float dist = (rb.position - grappleHit.point).magnitude;
            if (dist >= (grappleLength)) {
                Vector3 normal = (grappleHit.point - rb.position).normalized;

                float forwardVel = Vector3.Dot(rb.velocity, normal);
                Vector3 velChange = Vector3.zero;

                velChange = normal * grappleVel;

                Vector3 orthoVel = Vector3.ProjectOnPlane(rb.velocity, normal);
                float futVel = Mathf.Min(orthoVel.magnitude, termRadialVel);

                rb.velocity = orthoVel.normalized * futVel + velChange;
            }

            if (grappleLength > minGrappleLength) {
                grappleLength -= grappleVel * Time.deltaTime;
            } else {
                isGrappling = false;
                lineRenderer.enabled = false;
            }

        }
    }
}
