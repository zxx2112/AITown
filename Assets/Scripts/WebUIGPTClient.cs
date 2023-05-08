using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class WebUIGPTClient : GPTClient
{

    [SerializeField] private string uri = "http://192.168.1.9:5000/api/v1/generate";

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

    public override IEnumerator Send(Chat chat)
    {
        _waiting = true;

        // var input =  chat.history.Select(x => new JObject()
        // {
        //     {"role", x.role},
        //     {"content", x.content}
        // }).ToArray();

        JObject req = new JObject();

        req["prompt"] = "In order to make homemade bread, follow these steps:\n1)";
        req["max_new_tokens"] = 250;
        req["do_sample"] = true;
        req["temperature"] = 1.3f;
        req["top_p"] = 0.1f;
        req["typical_p"] = 1;
        req["repetition_penalty"] = 1.18;
        req["top_k"] = 40;
        req["min_length"] = 0;
        req["no_repeat_ngram_size"] = 0;
        req["num_beams"] = 1;
        req["penalty_alpha"] = 0;
        req["length_penalty"] = 1;
        req["early_stopping"] = false;
        req["seed"] = -1;
        req["add_bos_token"] = true;
        req["truncation_length"] = 2048;
        req["ban_eos_token"] = false;
        req["skip_special_tokens"] = true;
        req["stopping_strings"] = new JArray();
        
        var jsonString = req.ToString();

        Debug.Log(jsonString);

        UnityWebRequest client = UnityWebRequest.PostWwwForm(uri, string.Empty);

        //client.SetRequestHeader("Authorization", $"Bearer {apiToken}");
        //client.SetRequestHeader("User-Agent", $"hexthedev/openai_api_unity");
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
