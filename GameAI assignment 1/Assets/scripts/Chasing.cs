using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chasing : MonoBehaviour
{
    private float speed;

    public float rotationTime = 2f;
    public float maxSpeed = 10f;
    public float minSpeed = 0.3f;

    public float acceleration = 1;
    public float slowingRadiusSqr = 6f;
    public float stopDistanceSqr = 1f;

    public float rayDistance;
    [SerializeField]
    private Vector3 target;
    public float coneEvadeMulti = 4;

    //public GameObject stopRing;
    public GameObject player;
    private Vector3 rotationTargetDir;

    public LineRenderer lookLine;

    public LineRenderer coneLine1;
    public LineRenderer coneLine2;
    public GameObject targetObj;
    public float offsetTime = 0.05f;
    private float offset = 0;
    public delegate void Behavior();
    public static Behavior currentBehavior;
    private Behavior[] behs;
    private int i = 0;
    private Vector3 start;
    void Start()
    {
        start = transform.position;
        behs = new Behavior[] {RayCastAvoid, CollisionPrediction, ConeCheck };
        target = player.transform.position;
        currentBehavior = RayCastAvoid;
        UIMgr.Instance.ChangeText("Current: " + currentBehavior.Method.Name);
    }

    void Update()
    {
        lookLine.SetPosition(0, transform.position);
        lookLine.SetPosition(1, transform.position + transform.forward * rayDistance);

        currentBehavior?.Invoke();

    }

    private void FixedUpdate()
    {
        Ray ray = new(transform.position, transform.forward * rayDistance);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                ChangeBeh(0);
            }
            else if (!hit.collider.CompareTag("Player"))
            {
                ChangeBeh(1);
            }
        }
    }

    public void ChangeBeh()
    {
        lastTarget = player.transform.position;
        i++;
        if(i == 3)
        {
            i = 0;
        }
        currentBehavior = behs[i];
        UIMgr.Instance.ChangeText("Current: " + currentBehavior.Method.Name);
    }
    public void ChangeBeh(int n)
    {
        i = n;
        lastTarget = player.transform.position;
        currentBehavior = behs[i];
        UIMgr.Instance.ChangeText("Current: " + currentBehavior.Method.Name);
    }
    public void RayCastAvoid()
    {
        coneLine1.SetPosition(0, transform.position);
        coneLine1.SetPosition(1, transform.position);
        coneLine2.SetPosition(0, transform.position);
        coneLine2.SetPosition(1, transform.position);

        Ray ray = new(transform.position, transform.forward * rayDistance);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Vector3 dst = hit.point + hit.normal * 3f;
            target = dst;
            lastTarget = dst;
        }
        else
        {
            if (offsetTime <= 0.05f)
            {
                offsetTime += 0.05f;
                target = lastTarget;
            }
            else
                target = player.transform.position;
        }
        Pursue(target);

    }

    private Vector3 lastTarget;
    public void CollisionPrediction()
    {
        Vector3 characterToCollider;
        float dot;
        coneLine1.SetPosition(0, transform.position + transform.right * Mathf.Cos(Mathf.Deg2Rad * 55) * rayDistance + Mathf.Sin(Mathf.Deg2Rad * 55) * rayDistance * transform.forward);
        coneLine1.SetPosition(1, transform.position);
        coneLine2.SetPosition(0, transform.position + transform.right * -Mathf.Cos(Mathf.Deg2Rad * 55) * rayDistance + Mathf.Sin(Mathf.Deg2Rad * 55) * rayDistance * transform.forward);
        coneLine2.SetPosition(1, transform.position);

        Collider[] rawCols = Physics.OverlapSphere(transform.position, rayDistance, ~(1 << 2));
        List<Collider> cols = new();
        //check if in cone
        foreach (Collider collider in rawCols)
        {
            characterToCollider = (collider.ClosestPoint(transform.position) - transform.position).normalized;
            dot = Vector3.Dot(characterToCollider, transform.forward);
            //Debug.Log(dot + " " + Mathf.Cos(40 / 2));
            if (dot >= Mathf.Cos(Mathf.Deg2Rad * 70/2))
            {
                cols.Add(collider);
            }
        }
        
        Vector3 targetVelocity;
        float t = 0;
        float tmpT = 999;
        int count = -1;
        Vector3 myVelocity = transform.forward * speed;
        Vector3 relativePosition, relativeVelocity;
        //detected something
        if(cols.Count != 0)
        {
            Obstacle obs;
            //find the most dangerous object
            for(int i = 0; i < cols.Count; i++)
            {
                obs = cols[i].GetComponent<Obstacle>();
                if (obs != null) 
                {
                    targetVelocity = new Vector3(obs.speed.x , 0, obs.speed.y);
                    relativePosition = obs.transform.position - transform.position;
                }
                else
                {
                    targetVelocity = new Vector3(0, 0, 0);
                    relativePosition = cols[i].transform.position - transform.position;
                }
                
                relativeVelocity = targetVelocity - myVelocity;
                t = -Vector3.Dot(relativePosition, relativeVelocity) / (relativeVelocity.magnitude * relativeVelocity.magnitude);
                if (tmpT > t)
                {
                    tmpT = t;
                    count = i;
                }
            }
            //if found
            if (count != -1)
            {
                offset = 0;
                t = tmpT;
                obs = cols[count].GetComponent<Obstacle>();
                Vector3 targetPosition;
                if (obs != null)
                {
                    targetVelocity = new Vector3(obs.speed.x, 0, obs.speed.y);
                    targetPosition = obs.transform.position + t * targetVelocity;
                }
                else
                {
                    targetVelocity = Vector3.zero;
                    targetPosition = cols[count].transform.position + t * targetVelocity;
                }

                Vector3 myPosition = transform.position + t * myVelocity;

                
                target = targetPosition - myPosition;
                if (target.magnitude <= 1f || t < 0.6f)
                {
                    //my avoidance method
                    Vector3 targetCurrentPos = cols[count].transform.position;
                    targetCurrentPos.y = 0;
                    characterToCollider = (targetCurrentPos - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
                    dot = Vector3.Dot(characterToCollider, transform.forward);
                    float distanceOffset = Mathf.Clamp(5f / (targetCurrentPos - new Vector3(transform.position.x, 0, transform.position.z)).magnitude, 0.2f, 1);
                    float strength = dot / distanceOffset * Mathf.Sign(Vector3.Cross(characterToCollider, transform.forward).y);

                    Vector3 back = (transform.position - targetCurrentPos).normalized / distanceOffset;
                    Vector3 tmpTarget = back + transform.position + coneEvadeMulti * distanceOffset * (player.transform.position - transform.position).normalized + (coneEvadeMulti * strength * transform.right) / distanceOffset;
                    //Vector3 tmp = transform.position + transform.forward * 4 + (transform.position - targetPos).normalized;
  
                    lastTarget = tmpTarget;
                    Pursue(tmpTarget);
                }
                else
                {
                    if (offset <= offsetTime)
                    {
                        offset += Time.deltaTime;
                        target = lastTarget;
                    }
                    else
                    {
                        lastTarget = player.transform.position;
                        target = player.transform.position;
                    }
                    Debug.Log(2);
                    Pursue(target);
                }
            }
            else
            {
                if (offset <= offsetTime)
                {
                    offset += Time.deltaTime;
                    target = lastTarget;
                }
                else
                {
                    lastTarget = player.transform.position;
                    target = player.transform.position;
                }
                    

                Pursue(target);
            }
        }
        else
        {
            //nothing to collide
            if (offset <= offsetTime)
            {
                offset += Time.deltaTime;
                target = lastTarget;
            }
            else
                target = player.transform.position;
            Pursue(target);
        }
    }

    public void ConeCheck()
    {
        coneLine1.SetPosition(0, transform.position + transform.right * Mathf.Cos(Mathf.Deg2Rad * 45) * rayDistance + Mathf.Sin(Mathf.Deg2Rad * 45) * rayDistance * transform.forward);
        coneLine1.SetPosition(1, transform.position);
        coneLine2.SetPosition(0, transform.position + transform.right * -Mathf.Cos(Mathf.Deg2Rad * 45) * rayDistance + Mathf.Sin(Mathf.Deg2Rad * 45) * rayDistance * transform.forward);
        coneLine2.SetPosition(1, transform.position);

        Collider[] cols = Physics.OverlapSphere(transform.position, rayDistance, ~(1 << 2));
        Vector3 characterToCollider;
        float dot;
        int count = 0;
        float strength = 0;
        foreach (Collider collider in cols)
        {
            characterToCollider = (collider.transform.position - transform.position).normalized;
            dot = Vector3.Dot(characterToCollider, transform.forward);
            //Debug.Log(dot + " " + Mathf.Cos(40 / 2));
            if (dot >= Mathf.Cos(Mathf.Deg2Rad * 45/2))
            {
                count++;
                strength += dot * Mathf.Sign(Vector3.Cross(characterToCollider, transform.forward).y) * 1.2f ;
            }
        }
        if(count != 0)strength /= count;
        if(Mathf.Abs(strength) <= 0.02f)
        {
            if (offset <= offsetTime)
            {
                offset += Time.deltaTime;
                target = lastTarget;
            }
            else
                target = player.transform.position;
            Pursue(target);
        }
        else
        {
            target = player.transform.position;
            Vector3 tmpTarget = transform.position;
            strength *= coneEvadeMulti;
            tmpTarget +=  strength * transform.right + (target - transform.position).normalized * coneEvadeMulti;
            lastTarget = tmpTarget;
            Pursue(tmpTarget);
        }
        
    }

    public void Pursue()
    {

        target = Drive.Instance.transform.position + Drive.Instance.currentSpeed * Drive.Instance.transform.forward;
        

        rotationTargetDir = target - transform.position;
        Turn();
        SpeedUp(target);

        Move();
    }
    public void Pursue(Vector3 tmp)
    {
        targetObj.transform.position = new Vector3(tmp.x, -1.864f, tmp.z);

        rotationTargetDir = tmp - transform.position;
        Turn();
        SpeedUp(tmp);
        Move();
    }

    public void Flee()
    {

        target = Drive.Instance.transform.position + Drive.Instance.currentSpeed * Drive.Instance.transform.forward;
        rotationTargetDir = transform.position - target;

        targetObj.transform.position = target;

        Turn();
        SpeedUp(target);

        Move();

    }

   
    /// ----------------------------------------
    private void Turn()
    {
        if (Vector3.Angle(transform.forward, rotationTargetDir) >= 2)
        {
            float angle = Vector3.Angle(transform.forward, rotationTargetDir) * Mathf.Sign(Vector3.Cross(transform.forward, rotationTargetDir).y);
            transform.RotateAround(transform.position, Vector3.up, angle * Time.deltaTime * rotationTime);
        }

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

        if (speed >= maxSpeed)
        {
            speed = maxSpeed;
        }
    }

    public void Move()
    {

        transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);

    }

    public void ResetPosition()
    {
        ChangeBeh();
        transform.position = start;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            UIMgr.Instance.UpdateCollision();
    }
}
