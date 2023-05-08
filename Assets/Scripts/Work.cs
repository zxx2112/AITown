using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Work : MonoBehaviour,IBehaviour
{
    public bool BehaviourComplete { get; private set; }
    public Brain Brain { get; }
    public void Reset()
    {
        BehaviourComplete = false;
    }

    public Sprite Sprite => sprite;

    [SerializeField] private Sprite sprite;

    public void BehaviourUpdate(float deltaTime)
    {
        
    }
}
