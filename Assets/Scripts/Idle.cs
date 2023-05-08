using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : MonoBehaviour,IBehaviour
{
    public bool BehaviourComplete => false;
    public Brain Brain { get; }

    public Sprite Sprite => null;
    public void Reset()
    {
        
    }

    public void BehaviourUpdate(float deltaTime)
    {
        
    }
}
