using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public static class Actions
{
    public const string DoNothing = "Do nothing";
    public const string Move = "Move";
    public const string Work = "Work";
    public const string Sleep = "Sleep";
}

public static class Parameters
{
    public const string Action = "Action";
    public const string ActionName = "Action Name";
    public const string Yes = "Yes";
    public const string No = "No";
    public const string Parameter = "Parameter";
    public const string Destination = "Destination";
    public const string WorkContent = "Work content";
    public const string Speech = "Speech";
}
    

//角色可以进行的行为
public interface IBehaviour
{
    void BehaviourUpdate(float deltaTime);
    
    bool BehaviourComplete { get; }
    
    Brain Brain { get; }

    void Reset();
    
    Sprite Sprite { get; }
}

// [System.Serializable]
// public class ResponseBehaviour
// {
//     public string 行为名;
//     public JObject 参数;
// }

// [System.Serializable]
// public class CharacterThinkResult
// {
//     public string 想法;
//     public string 原因;
//     public JContainer 计划;
//     public JObject 行为;
//     public string 说话;
// }

//接入语言模型,计算行动并执行
public class Brain : MonoBehaviour
{
    [SerializeField] private TextAsset promptTemplate;
    
    // [Multiline(16)]
    // [SerializeField] private string testJson;
    
    [FoldoutGroup("Debug")]
    [Multiline(16)]
    [SerializeField] private string inputPrompt;
    [FoldoutGroup("Debug")]
    [Multiline(16)]
    [SerializeField] private string responseJson;
    
    [SerializeField] private GPTClient chatClient;

    [SerializeField] private Vector2Int startDateTime = new Vector2Int(8,0);

    [SerializeField] private string currentLocation;
    
    [SerializeField] private Idle idle;
    [SerializeField] private Move move;
    [SerializeField] private Work work;
    [SerializeField] private HudState hudState;//临时直接依赖UI
    [SerializeField] private HudTalkBubble hudTalkBubble;
    [SerializeField] private Sprite sleepSprite;

    [Range(0,600)]
    [SerializeField] private float timeScale = 10f;

    [SerializeField] private bool needWaitConfirm = true;
    [SerializeField] private bool thinkInEveryWholeHour = false;

    [ShowInInspector,ReadOnly]
    private string CurrentBehaviourName { get; set; }
    [ShowInInspector,ReadOnly]
    private IBehaviour CurrentBehaviour { get; set; }
    
    [ShowInInspector]
    private bool WaitConfirm { get; set; }
    
    [ShowInInspector,ReadOnly]
    private bool Thinking { get; set; }
    
    private int _seconds;

    private int _hourCache;

    [ShowInInspector,ReadOnly]
    private int _dateHour;
    [ShowInInspector,ReadOnly]
    private int _dateMinute;
    
    private float _cumulativeTime;

    public string DateTimeStr
    {
        get
        {
            if (_seconds < 12 * 3600)
            {
                return $"{_dateHour}:{_dateMinute:D2} am";
            }
            else
            {
                return $"{_dateHour-12}:{_dateMinute:D2} pm";
            }
        }
    }
    
    public (int,int) DateTime => (_dateHour,_dateMinute);

    private void Start()
    {
        ClearBehaviour();

        WaitConfirm = false;
        Thinking = false;

        //idle.Setup(this);
        move.Setup(this);
        
        SetSeconds(startDateTime.x * 3600 + startDateTime.y * 60);
        _hourCache = 0;
    }

    private void Update()
    {
        //思考过程中暂停游戏时间
        if (Thinking)
        {
            Time.timeScale = 0;
            return;
        }
        
        Time.timeScale = 1;
        
        //时间更新
        _cumulativeTime += Time.deltaTime * timeScale;
        if (_cumulativeTime >= 60)
        {
            _cumulativeTime -= 60;

            var targetSeconds = _seconds + 60;
            
            if(targetSeconds > 24 * 3600)
                targetSeconds = 0;
            
            SetSeconds(targetSeconds);
        }
        
        //整点强制思考
        if(_hourCache != _dateHour)
        {
            _hourCache = _dateHour;

            if (thinkInEveryWholeHour && !Thinking)
            {
                StartThink();
                return;
            }
        }
        
        if(CurrentBehaviour == null)
            return;
        
        CurrentBehaviour.BehaviourUpdate(Time.deltaTime);

        //在满足条件后进行思考
        if (CurrentBehaviour.BehaviourComplete)
        {
            
        }
    }

    private void SetSeconds(int seconds)
    {
        _seconds = seconds;

        _dateHour = _seconds / 3600;
        _dateMinute = (_seconds % 3600) / 60;
    }

    [Button]
    public void RequestThink()
    {
        StartThink();
    }
    
    public void StartThink()
    {
        StartCoroutine(Think());
    }

    [Button]
    public IEnumerator Think()
    {
        Debug.Log("Start Think");
        
        Thinking = true;
        
        //等待确认思考
        WaitConfirm = true;

        if(needWaitConfirm)
            yield return new WaitUntil(() => WaitConfirm == false);

        //Debug
        inputPrompt = promptTemplate.text;

        inputPrompt = inputPrompt.Replace("<Location>", currentLocation);
        inputPrompt = inputPrompt.Replace("<Time>", DateTimeStr);
        inputPrompt = inputPrompt.Replace("<Behaviour>", CurrentBehaviourName);
        inputPrompt = inputPrompt.Replace("<WorkTime>",_seconds is >= 9 * 3600 and < 18 * 3600 ? Parameters.Yes : Parameters.No);
        inputPrompt = inputPrompt.Replace("<SleepTime>",_seconds is >= 22 * 3600 or < 6 * 3600 ? Parameters.Yes : Parameters.No);
        
        var chatHistory = new List<Message>() {new (){role = "system",content = inputPrompt}};
        var chat = new Chat() {history = chatHistory};
        yield return chatClient.Send(chat);
        var chatResponse = chat.response;

        var jsonResponse = chatResponse.choices[0].message.content;

        //Debug
        responseJson = jsonResponse;

        var characterThinkResult = TryParse(jsonResponse);
        
        if(characterThinkResult != null)
        {
                IBehaviour behaviour = null;
        
                var behaviourName = (string)characterThinkResult[Parameters.Action][Parameters.ActionName]; 
                switch (behaviourName)
                {
                    case Actions.DoNothing:
                        behaviour = idle;
                        break;
                    case Actions.Move:
                        behaviour = move;
                        var arg = characterThinkResult[Parameters.Action][Parameters.Parameter]?[Parameters.Destination];
                        if (arg != null)
                        {
                            var targetLocation = arg.ToString();
                            move.TargetLocation = targetLocation;
                            move.TargetPosition = LocationService.GetLocationPosition(targetLocation);
                        }
                        break;
                    case Actions.Work:
                        behaviour = work;
                        break;
                }
                
                CurrentBehaviourName = behaviourName;
                
                if(behaviour != null)
                    behaviour.Reset();
            
                CurrentBehaviour = behaviour;

                if (behaviour != null)
                {
                    var iconDescription = string.Empty;
                    
                    if (CurrentBehaviourName == Actions.Work)
                    {
                        iconDescription = characterThinkResult[Parameters.Action][Parameters.Parameter]?[Parameters.WorkContent]?.ToString();
                    }

                    hudState.SetIcon(behaviour.Sprite,iconDescription);
                }
                else
                {
                    if (CurrentBehaviourName == Actions.Sleep)
                    {
                        hudState.SetIcon(sleepSprite);
                    }
                    else
                    {
                        hudState.SetIcon(null);    
                    }
                }

                if(!string.IsNullOrEmpty(characterThinkResult[Parameters.Speech].ToString()))
                    hudTalkBubble.SetTalk(characterThinkResult[Parameters.Speech].ToString(),5f);
                
                Debug.Log(CurrentBehaviourName.GetType());
                Debug.Log("Finish Think");

        }
        Thinking = false;
    }

    private static JObject TryParse(string jsonResponse)
    {
        try
        {
            var characterThinkResult = JsonConvert.DeserializeObject(jsonResponse);
            return (JObject)characterThinkResult;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
        
    }

    // public void Test()
    // {
    //     var response = JsonConvert.DeserializeObject<CharacterThinkResult>(responseJson);
    // }
    
    public void SetCurrentLocation(string location)
    {
        currentLocation = location;
    }

    public void ClearBehaviour()
    {
        CurrentBehaviourName = Actions.DoNothing;
        CurrentBehaviour = idle;
        
        hudState.SetIcon(null);
        hudTalkBubble.SetTalk(string.Empty);
    }
}
