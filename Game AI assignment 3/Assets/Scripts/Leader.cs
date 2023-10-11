using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : MonoBehaviour
{
    public static Leader Instance;
    public List<GameObject> agents;
    [SerializeField]
    private List<Chasing> chasings = new();
    public float speed = 0;
    public LineRenderer path;

    public static float width = 1.8f;

    private bool move = true;
    void Awake()
    {
        Instance = this;
        foreach (var agent in agents)
        {
            chasings.Add(agent.GetComponent<Chasing>());
        }
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].GetOffset(CalculateOffset(i));
        }
    }

    private void Start()
    {
        
    }
    // Update is called once per frame
    private bool flag = true;
    private bool endT1 = true;
    private bool endT2 = true;
    private float cd = 0;
    private float cd2 = 0;
    private void Update()
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < chasings.Count; i++)
        {
            center += agents[i].transform.position;
        }
        center /= agents.Count;
        float currentSpeed = speed * Mathf.Clamp( -0.2f * Vector3.Distance(center, transform.position) + 1, 0.01f, 1);
        if(move)Move(currentSpeed);

        if (!endT2)
        {
            for (int i = 0; i < chasings.Count; i++)
            {
                chasings[i].GetOffset(ThreeInRow(i));
            }
        }
        else
        {
            for (int i = 0; i < chasings.Count; i++)
            {
                chasings[i].GetOffset(CalculateOffset(i));
            }
        }

        
        Ray ray = new(transform.position + transform.forward * 0.25f, transform.right);
        Ray ray2 = new(transform.position + transform.forward * 0.25f, -transform.right);
        Debug.DrawLine(transform.position + transform.forward * 0.25f, transform.position + transform.forward * 0.25f + transform.right);
        Debug.DrawLine(transform.position + transform.forward * 0.25f, transform.position + transform.forward * 0.25f - gameObject.transform.right);

        RaycastHit hit, hit2;
        if (flag)
        {
            if (Physics.Raycast(ray, out hit, 1.5f, 1 << 7) && Physics.Raycast(ray2, out hit2, 1.5f, 1 << 7))
            {
                float distance = Vector3.Distance(hit.point, hit2.point);
                if (distance <= 1.5f * width)
                {
                    Turnnel();
                    flag = false;
                    endT1 = false;
                }
                else
                {
                    Turnnel2();
                    flag = false;
                    endT2 = false;
                }
            }
        }
        else if(!endT1 && cd >= 1)
        {
            Check();
        }
        else if (!endT2 && cd2 >= 1.25f)
        {
            Check2();
        }
        if (cd < 1) cd += Time.deltaTime;
        if (cd2 < 1.25f) cd2 += Time.deltaTime;
    }

    private int count = 0;
    public void Move(float v)
    {
        Vector3[] nodes = new Vector3[path.positionCount];
        path.GetPositions(nodes);

        if(Vector3.Distance(transform.position, nodes[count]) <= 0.05f)
        {
            count++;
            if(count == path.positionCount) count = 0;
        }
        Vector3 target = nodes[count];
        Vector3 dir = target - transform.position;
        //rotate
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);// * Mathf.Sign(Vector3.Cross(transform.forward, rotationTargetDir).y);
        transform.RotateAround(transform.position, Vector3.up, angle * Time.deltaTime * 3);

        transform.Translate(Time.deltaTime * v * dir.normalized, Space.World);
    }

    public void Turnnel()
    {
        move = false;
        transform.position += transform.forward * 8.5f;
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].turnnel1 = true;
            chasings[i].move = false;
            chasings[i].transform.LookAt(transform.position, Vector3.up);
        }
        chasings[0].move = true;
    }

    public void Turnnel2()
    {
        move = false;
        transform.position += transform.forward * 9f;
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].turnnel2 = true;
            
            chasings[i].move = false;
            chasings[i].transform.LookAt(transform.position, Vector3.up);
        }
        chasings[0].move = true;
        chasings[1].move = true;
        chasings[2].move = true;
    }
    public void Check()
    {
        for (int i = 0; i < chasings.Count; i++)
        {
            if(!chasings[i].move)
            {
                GameObject last = chasings[i - 1].gameObject;
                Ray ray = new(last.transform.position - last.transform.right * 0.25f, last.transform.right + last.transform.position);
                Ray ray2 = new(last.transform.position + last.transform.right * 0.25f, -last.transform.right + last.transform.position);
                Debug.DrawLine(last.transform.position, last.transform.position + last.transform.right);
                Debug.DrawLine(last.transform.position, last.transform.position - last.transform.right);
                if (Physics.Raycast(ray, out RaycastHit hit, 1.2f, 1 << 7) && Physics.Raycast(ray2, out RaycastHit hit2, 1.2f, 1 << 7))
                {
                    cd = 0;
                    chasings[i].move = true;
                    break;
                }
                else if (Physics.Raycast(ray, out _, 1.2f, 1 << 7))
                {
                    chasings[i - 1].transform.Translate(-Vector3.right * 0.1f, Space.Self);
                }
                else if (Physics.Raycast(ray2, out _, 1.2f, 1 << 7))
                {
                    chasings[i - 1].transform.Translate(Vector3.right * 0.1f, Space.Self);
                }
            }
        }

        if (chasings[^1].move && Vector3.Distance(chasings[^1].transform.position, transform.position) <= 5)
        {
            EndTurnnel1();
        }
    }

    private void Check2()
    {
        for (int i = 0; i < chasings.Count; i++)
        {
            if (i % 3 == 1 && !chasings[i].move)
            {
                GameObject last = chasings[i - 3].gameObject;
                Ray ray = new(last.transform.position - last.transform.right * 0.25f, last.transform.right + last.transform.position);
                Ray ray2 = new(last.transform.position + last.transform.right * 0.25f, -last.transform.right + last.transform.position);
                Debug.DrawLine(last.transform.position, last.transform.position + last.transform.right * 2);
                Debug.DrawLine(last.transform.position, last.transform.position - last.transform.right * 2);
                if (Physics.Raycast(ray, out RaycastHit hit, 3f, 1 << 7) && Physics.Raycast(ray2, out RaycastHit hit2, 3f, 1 << 7))
                {
                    cd2 = 0;
                    if (i != chasings.Count - 1)chasings[i + 1].move = true;
                    chasings[i].move = true;
                    chasings[i - 1].move = true;
                    break;
                }
                else if(Physics.Raycast(ray, out _, 3f, 1 << 7))
                {
                    chasings[i - 3].transform.Translate(-Vector3.right * 0.2f, Space.Self);
                }
                else if (Physics.Raycast(ray2, out _, 3f, 1 << 7))
                {
                    chasings[i - 3].transform.Translate(Vector3.right * 0.2f, Space.Self);
                }
            }
            else if( i == chasings.Count - 1)
            {
                if(chasings[i - 1].move )
                    chasings[i].move = true;
            }
        }

        if (chasings[^1].move && Vector3.Distance(chasings[^1].transform.position, transform.position) <= 5)
        {
            EndTurnnel2();
        }
    }

    public void EndTurnnel1()
    {
        move = true;
        endT1 = true;
        flag = true;
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].turnnel1 = false;
            chasings[i].move = false;
        }
    }
    public void EndTurnnel2()
    {
        move = true;
        endT2 = true;
        flag = true;
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].turnnel2 = false;
            chasings[i].move = false;
        }
    }

    public void Remove(GameObject bird)
    {
        agents.Remove(bird);
        chasings.Clear();
        for (int i = 0; i < agents.Count; i++)
        {
            chasings.Add(agents[i].GetComponent<Chasing>());
        }
        for (int i = 0; i < chasings.Count; i++)
        {
            chasings[i].GetOffset(CalculateOffset(i));
        }
    }

    public Vector3 CalculateOffset(int index)
    {
        int row = 0;
        int count = 0;
        for(int i = 0; i < 10; i++)
        {
            float c = (i + 1) / 2.0f;
            if(c * (i + 2) >= (index + 1))
            {
                row = i;
                count = index == 0 ? 0 : i - (int)((i + 1) / 2.0f * (i + 2) - index - 1);
                break;
            }
        }

        float targetX = (count + 0.5f - (row + 1) / 2.0f) * width/2;
        float targetZ = -width / 2 * row;
        Vector3 target = new Vector3(targetX, 0, targetZ);
        target = transform.rotation * target;
        return target;
    }

    public Vector3 ThreeInRow(int index)
    {
        int row = 0;
        int count = 0;
        for (int i = 0; i < 10; i++)
        {
            if ((i+1) * 3 >= (index + 1))
            {
                row = i;
                count = index % 3;
                break;
            }
        }

        float targetX = (count + 0.5f - 3 / 2.0f) * width / 2;
        float targetZ = -width / 2 * row;
        Vector3 target = new Vector3(targetX, 0, targetZ);
        target = transform.rotation * target;
        return target;
    }

    private void OnDrawGizmos()
    {
        if (!endT2)
        {
            for (int i = 0; i < chasings.Count; i++)
            {
                Gizmos.DrawSphere(ThreeInRow(i) + transform.position, 0.1f);
            }
        }
        else
        {

            for (int i = 0; i < chasings.Count; i++)
            {
                Gizmos.DrawSphere(CalculateOffset(i) + transform.position, 0.1f);
            }
        }
        
    }
}