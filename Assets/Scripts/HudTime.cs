using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HudTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Brain brain;
    private (int,int) dateTimeCache;
    
    void Start()
    {
        text.text = "00:00";
        dateTimeCache = (0, 0);
    }
    
    void Update()
    {
        if (dateTimeCache != brain.DateTime)
        {
            dateTimeCache = brain.DateTime;
            text.text = $"{dateTimeCache.Item1}:{dateTimeCache.Item2:D2}";
        }
    }
}
