using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chasing : MonoBehaviour
{
    public Transform leader;
    private Vector3 assignedPosition;
    private Vector3 target;

    public float speed;
    private float currentSpeed = 0;

    public float rotationSpeed = 2f;
    public float maxSpeed = 10f;
    public float minSpeed = 0.3f;

    public float acceleration = 1;
    public float slowingRadiusSqr = 6f;
    public float stopDistanceSqr = 1f;

    public float rayDistance;
    public float sideRayDistance;

    public float wallWeight = 1;
    public float avoidanceWeight = 1;
    private Vector3 wallTarget;
    [HideInInspector]
    public Vector3 offset = Vector3.zero;
    private Vector3 avoidance;

    public float alignment;
    public GameObject debug;
    //public GameObject stopRing;
    private Vector3 rotationTargetDir;

    private LineRenderer lookLine = new();
    private LineRenderer lookLine2 = new();

    public delegate void Behavior();
    public event Behavior CurrentBehavior;
    void Start()
    {
        assignedPosition = offset + leader.transform.position;

        lookLine = GetComponent<LineRenderer>();
        lookLine2 = GetComponentsInChildren<LineRenderer>()[1];
        leader = Leader.Instance.gameObject.transform;
        CurrentBehavior = RayCastAvoid;
        transform.position = assignedPosition;
        //UIMgr.Instance.ChangeText("Current: " + currentBehavior.Method.Name);
    }

    void Update()
    {
        lookLine.SetPosition(0, transform.position - transform.forward * 0.4f - transform.right * 0.2f);
        lookLine.SetPosition(1, transform.position + (transform.forward + transform.right * 0.2f) * sideRayDistance);
        lookLine2.SetPosition(0, transform.position - transform.forward * 0.4f + transform.right * 0.2f);
        lookLine2.SetPosition(1, transform.position + (transform.forward - transform.right * 0.2f) * sideRayDistance);

        CurrentBehavior?.Invoke();
    }

    private void FixedUpdate()
    {
        Ray ray = new(transform.position, transform.forward * rayDistance);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
        }
    }

    public bool turnnel1 = false;
    public bool turnnel2 = false;
    public void RayCastAvoid()
    {
        Ray ray = new(transform.position - transform.forward * 0.4f - transform.right * 0.2f , (transform.forward + transform.right * 0.2f) * sideRayDistance);
        Ray ray2 = new(transform.position - transform.forward * 0.4f + transform.right * 0.2f, (transform.forward - transform.right * 0.2f) * sideRayDistance);
        Ray rayF = new(transform.position, transform.forward * rayDistance);
        RaycastHit hit, hit2;
        if(Physics.Raycast(rayF, out hit, rayDistance, 1 << 7))
        {
            wallTarget = hit.normal * 2 + hit.point - transform.position + (transform.position - hit.collider.ClosestPoint(transform.position)).normalized;
        }
        else if (Physics.Raycast(ray, out hit, sideRayDistance, 1 << 7) && Physics.Raycast(ray2, out hit2, sideRayDistance, 1 << 7))
        {
            float dst = hit.distance - hit2.distance;
            wallTarget = transform.position + transform.forward + dst * transform.right;
            wallTarget.y = 0;
            Vector3 o = dst >= 0 ? hit2.collider.ClosestPoint(transform.position) : hit.collider.ClosestPoint(transform.position);
            Vector3 dis = transform.position - o;
            wallTarget += dis.normalized * Mathf.Clamp(1/dis.magnitude, 0.05f, 1.2f);
            //Debug.Log(hit.collider.name); if (debug != null) debug.transform.position = wallTarget;
        }
        else if(Physics.Raycast(ray, out hit, sideRayDistance, 1 << 7))
        {
            Vector3 dst = hit.point + hit.normal * 2f;
            wallTarget = dst - transform.position;
            wallTarget.y = 0;
            Vector3 dis = transform.position - hit.collider.ClosestPoint(transform.position);
            wallTarget += dis.normalized * Mathf.Clamp(1 / dis.magnitude, 0.05f, 1.2f);
        }
        else if(Physics.Raycast(ray2, out hit2, sideRayDistance, 1 << 7))
        {
            Vector3 dst = hit2.point + hit2.normal * 2f;
            wallTarget = dst - transform.position;
            wallTarget.y = 0;
            Vector3 dis = transform.position - hit2.collider.ClosestPoint(transform.position);
            wallTarget += dis.normalized * Mathf.Clamp(1 / dis.magnitude, 0.05f, 1.2f);
        }
        else
        {
            wallTarget = Vector3.zero;
        }

        if (!turnnel1 && !turnnel2)
        {
            assignedPosition = offset + leader.position;
            Pursue();
        }
        else
        {
            if (move)
            {
                assignedPosition = offset + leader.position; ;
                Turnnel1();
            }
        }
    }

    public Chasing[] group;
    public bool move = false;
    public void Turnnel1()
    {
        CalculateTarget();
        rotationTargetDir = (target - transform.position).magnitude <= 0.01 ? transform.forward : target - transform.position;

        Turn();
        SpeedUp(assignedPosition);
        Move();
    }

    public void Pursue()
    {
        GetNeighbors();
        CalculateTarget();
        rotationTargetDir = (target - transform.position).magnitude <= 0.01 ? transform.forward : target - transform.position;

        Turn();
        SpeedUp(assignedPosition);
        Move();
    }
   
    public void GetNeighbors()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, 2.5f, LayerMask.GetMask("bird"));
        avoidance = Vector3.zero;
        if(neighbors.Length != 0)
        {
            foreach(var b in neighbors)
            {
                float weight = Mathf.Clamp(-0.4f * Vector3.Distance(b.transform.position, transform.position) + 1, 0, 1);
                avoidance += (transform.position - b.transform.position).normalized * weight;
            }
            avoidance /= neighbors.Length;
        }
    }

    public void GetOffset(Vector3 input)
    {
        offset = input;
    }
    /// ----------------------------------------
    private void Turn()
    { 
        
        float angle = Vector3.SignedAngle(transform.forward, rotationTargetDir, Vector3.up);// * Mathf.Sign(Vector3.Cross(transform.forward, rotationTargetDir).y);
        transform.RotateAround(transform.position, Vector3.up, angle * Time.deltaTime * rotationSpeed);

    }

    private void SpeedUp(Vector3 moveTarget)
    {

        if (speed < maxSpeed && Vector3.SqrMagnitude(moveTarget - transform.position) >= slowingRadiusSqr)
        {
            speed += Time.deltaTime * acceleration;
        }
        else if (Vector3.SqrMagnitude(moveTarget - transform.position) >= stopDistanceSqr)
        {
            float percentage = (Vector3.SqrMagnitude(moveTarget - transform.position) - stopDistanceSqr) / (slowingRadiusSqr - stopDistanceSqr);
            speed = Mathf.Max(Mathf.Lerp(0, maxSpeed, percentage), minSpeed);
        }
        else
        {
            speed = 0;
        }

        currentSpeed = speed * Mathf.Clamp(Vector3.Magnitude(leader.transform.position + offset - transform.position) / 1f, 1, 2);
        //Debug.Log(Vector3.Magnitude(leader.transform.position + offset - transform.position));
        if (currentSpeed >= maxSpeed)
        {
            currentSpeed = maxSpeed;
        }
    }

    public void Move()
    {
        if(Vector3.Distance(transform.position, assignedPosition) > 0.1f )
            transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);

    }

    public void CalculateTarget()
    {
        Vector3 toPosition = assignedPosition - transform.position;
        toPosition.y = 0;
        
        target = toPosition.normalized + transform.position + avoidance * avoidanceWeight + wallTarget.normalized * wallWeight ;
        
        // Debug.DrawLine(transform.position, transform.position + target, Color.red, 0.1f);
    }


}
