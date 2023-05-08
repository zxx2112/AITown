using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Response
{
    [System.Serializable]
    public class Usage
    {
        public int prompt_token;
        public int completion_tokens;
        public int total_tokens;
    }
        
    [System.Serializable]
    public class Choice
    {
        public Message message;
        public string finish_reason;
        public int index;
    }

    public string id;
    public string @object;
    public int created;
    public string model;
    public Usage usage;
    public Choice[] choices;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
    
[System.Serializable]
public class Message
{
    [TableColumnWidth(20)]
    public string role;
    [Multiline]
    public string content;

    public override string ToString()
    {
        return $"{role}:{content}";
    }
}

[System.Serializable]
public class Chat
{
    public List<Message> history;
    public Response response;
}

public class GPTClient : MonoBehaviour
{
    public virtual IEnumerator Send(Chat chat)
    {
        yield return null;
    }
}
