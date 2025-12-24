using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AnswerSubmitController : MonoBehaviour
{
    // ======================
    // 数据模型（就放这里）
    // ======================

    [System.Serializable]
    public class AnswerRequest
    {
        public string murderer;
        public string motivation;
        public string method;
    }

    [System.Serializable]
    public class AnswerResponse
    {
        public bool murderer;
        public bool motivation;
        public bool method;
    }

    // ======================
    // Backend
    // ======================

    public string baseUrl = "http://localhost:8082";

    public string selectedMurderer;
    public string motivationText;
    public string methodText;

    public void SubmitAnswer()
    {
        AnswerRequest req = new AnswerRequest
        {
            murderer = selectedMurderer,
            motivation = motivationText,
            method = methodText
        };

        Debug.Log("Ready for posting answers");

        StartCoroutine(PostAnswer(req));
    }

    private IEnumerator PostAnswer(AnswerRequest answer)
    {
        string url = $"{baseUrl}/api/answer";

        string json = JsonUtility.ToJson(answer);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        Debug.Log("Posted answers!!");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        AnswerResponse resp =
            JsonUtility.FromJson<AnswerResponse>(request.downloadHandler.text);

        HandleResult(resp);
    }

    private void HandleResult(AnswerResponse result)
    {
        Debug.Log($"murderer: {result.murderer}");
        Debug.Log($"motivation: {result.motivation}");
        Debug.Log($"method: {result.method}");
    }
}
