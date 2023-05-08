using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudTalkBubble : MonoBehaviour
{
    [SerializeField] private Text talkText;
    
    private float _talkLifeTime;

    private void Start()
    {
        talkText.text = string.Empty;
    }

    private void Update()
    {
        if (_talkLifeTime > 0)
        {
            _talkLifeTime -= Time.deltaTime;
            
            if (_talkLifeTime <= 0)
            {
                talkText.text = "";
                _talkLifeTime = -1;
            }
        }
    }
    
    public void SetTalk(string content, float duration = -1)
    {
        _talkLifeTime = duration;
        talkText.text = content;
    }
}
