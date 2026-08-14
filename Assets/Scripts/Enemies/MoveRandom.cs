using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AIMoveRandom : MonoBehaviour
{
    bool atTargetPosition;
    public Vector3 targetPosition;
    public NavMeshAgent agent;
    NavMeshTriangulation triangulation;
    
    void Start()
    {
        //targetPosition = agent.transform.position;
        agent = GetComponent<NavMeshAgent>();
        triangulation = NavMesh.CalculateTriangulation();
    }


    void Update()
    {
        atTargetPosition = Vector3.Distance(agent.transform.position, targetPosition) < 1 ? true : false;
        if(atTargetPosition)
        {
            GetPosition();
            agent.SetDestination(targetPosition);
            atTargetPosition = false;
        }
    }

    void GetPosition()
    {
        int i = Random.Range(0,triangulation.vertices.Length);
        if(NavMesh.SamplePosition(triangulation.vertices[i], out NavMeshHit hit, 2f, 0))
        {
            targetPosition = hit.position;
        }
    }
}
