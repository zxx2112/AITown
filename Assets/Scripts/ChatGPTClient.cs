using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGPTClient : GPTClient
{
    public enum Model
    {
        Gpt3dot5,
        Gpt4
    }

    [ValueDropdown("Uris",FlattenTreeView =true)]
    [SerializeField] private string uri = "https://api.openai-asia.com/v1/chat/completions";
    [SerializeField] private string apiToken;
    [SerializeField] private Model model;

    [BoxGroup("Test")]
    [SerializeField] private List<Message> testMessages;
    
    [BoxGroup("Test")]
    [Button]
    public void Test()
    {
        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        var chat = new Chat();
        chat.history = testMessages;
        yield return Send(chat);
        Debug.Log(chat.response);
    }
    
    private bool _waiting;

    private static string[] Uris = new[]
    {
        "https://api.openai-asia.com/v1/chat/completions", 
        //"https://192.168.1.9:5000/api/v1/generate"
    };

    public override IEnumerator Send(Chat chat)
    {
        _waiting = true;

        var input =  chat.history.Select(x => new JObject()
        {
            {"role", x.role},
            {"content", x.content}
        }).ToArray();

        JObject req = new JObject();

        var modelString = "gpt-3.5-turbo";
        switch (model)
        {
            case Model.Gpt3dot5:
                modelString = "gpt-3.5-turbo";
                break;
            case Model.Gpt4:
                modelString = "gpt-4";
                break;
        }
        req["model"] = modelString;
        req["temperature"] = 0.8f;
        req["top_p"] = 1;
        req["presence_penalty"] = 1;
        
        req["messages"] = new JArray(input);

        var jsonString = req.ToString();

        Debug.Log(jsonString);

        UnityWebRequest client = UnityWebRequest.PostWwwForm(uri, string.Empty);

        client.SetRequestHeader("Authorization", $"Bearer {apiToken}");
        client.SetRequestHeader("User-Agent", $"hexthedev/openai_api_unity");
        //if (!string.IsNullOrEmpty(_authArgs.organization)) client.SetRequestHeader("OpenAI-Organization", _authArgs.organization);

        AddJsonToUnityWebRequest(client, jsonString);

        yield return client.SendWebRequest();
        client.uploadHandler.Dispose();
        string resultAsString = client.downloadHandler.text;

        Debug.Log(resultAsString);

        var response = JsonConvert.DeserializeObject<Response>(resultAsString);

        _waiting = false;

        chat.response = response;
    }
    
    private void AddJsonToUnityWebRequest(UnityWebRequest client, string json)
    {
        client.SetRequestHeader("Content-Type", "application/json");
        client.uploadHandler = new UploadHandlerRaw(
            Encoding.UTF8.GetBytes(json)
        );
    }
}
