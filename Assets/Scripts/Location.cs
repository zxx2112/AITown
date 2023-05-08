using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    private void Start()
    {
        LocationService.RegisterLocation(gameObject);
    }
}
