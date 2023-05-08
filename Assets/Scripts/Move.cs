using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour,IBehaviour
{
    //[SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform characterRoot;
    [SerializeField] private float speed = 10;
    [SerializeField] private Sprite sprite;
    
    public string TargetLocation { get; set; }
    public Vector3 TargetPosition { get; set; }
    
    public bool BehaviourComplete { get; private set; }
    public Brain Brain { get; private set; }
    
    public Sprite Sprite => sprite;

    
    
    public void Reset()
    {
        BehaviourComplete = false;
    }

    public void Setup(Brain brain)
    {
        Brain = brain;
    }
    
    public void BehaviourUpdate(float deltaTime)
    {
        characterRoot.position = Vector3.MoveTowards(characterRoot.position, TargetPosition, speed * deltaTime);

        if (!BehaviourComplete)
        {
            var distance = Vector3.Distance(characterRoot.position, TargetPosition);

            if (distance < 0.1f)
            {
                BehaviourComplete = true;
                Brain.SetCurrentLocation(TargetLocation);
                Brain.ClearBehaviour();
                Brain.RequestThink();
            }
        }
    }
}
