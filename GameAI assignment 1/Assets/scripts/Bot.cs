
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    private float speed;

    public float rotationTime = 2f;
    public float maxSpeed = 10f;
    public float minSpeed = 0.3f;

    public float acceleration = 1;
    public float slowingRadiusSqr = 6f;
    public float stopDistanceSqr = 1f;

    [SerializeField]
    private Vector3 target;

    public GameObject targetObj;
    //public GameObject stopRing;
    public GameObject slowRing;
    public GameObject player;
    private Vector3 rotationTargetDir;

    public Camera cam;
    public LineRenderer line;

    public delegate void Behavior();
    public Behavior[] behaviorArray = new Behavior[4];
    public Behavior currentBehavior;
    private int i = 0;

    private int point = 0;
    void Start()
    {
        Time.timeScale = 1;
        target = player.transform.position;
        behaviorArray[0] = Wander;
        behaviorArray[1] = Pursue;
        behaviorArray[2] = Flee;
        behaviorArray[3] = PathFollow;
        currentBehavior = Wander;
    }

    void Update()
    {
        
        currentBehavior?.Invoke();
    }

    public void ChangeBehavior()
    {
        i++;
        if(i > 3)
        {
            i = 0;
        }
        currentBehavior = behaviorArray[i];
        
        GetComponent<UIManager>().UpdateB();
    }

    private float time = 0;
    public void Wander()
    {
        time += Time.deltaTime;
        Vector3 pos = new Vector3(transform.position.x + 10 * transform.forward.x, 0.01f, transform.position.z + 10 * transform.forward.z);
        slowRing.transform.position = pos;
        slowRing.transform.localScale = new Vector3(12, 0.01f, 12);

        if (time > 3)
        {
            time -= 3;
            target = transform.position + 10 * transform.forward + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)).normalized * 6f;
            Debug.Log(transform.forward);
            //target = transform.position + 10 * transform.forward + new Vector3(1, 0, 0) * 6f;
        }
        Pursue(target);
    }

    public void Pursue()
    {

        target = Drive.Instance.transform.position + Drive.Instance.currentSpeed * Drive.Instance.transform.forward;
        slowRing.transform.position = new Vector3(target.x, 0, target.z);
        slowRing.transform.localScale = new Vector3(Mathf.Sqrt(slowingRadiusSqr), 0.01f, Mathf.Sqrt(slowingRadiusSqr));
        targetObj.transform.position = target;

        rotationTargetDir = target - transform.position;
        Turn();
        SpeedUp(target);
        
        Move();
    }
    public void Pursue(Vector3 tmp)
    {
        targetObj.transform.position = tmp;

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

    public void PathFollow()
    {
        target = line.GetPosition(point);
       
        if (Vector3.SqrMagnitude(transform.position - target) <= stopDistanceSqr + 0.1f && point < line.positionCount) 
        {
            point++;
        }
        Pursue(target);
    }
    /// ----------------------------------------
    private void Turn()
    {
        if(Vector3.Angle(transform.forward, rotationTargetDir) >= 2)
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
        else if(Vector3.SqrMagnitude(moveTarget - transform.position) >= stopDistanceSqr)
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
        transform.position = new Vector3(0, 1.3f, 0);
    }
}
