using UnityEngine;

public class ApiConfigService : MonoBehaviour
{
    public static ApiConfigService Instance { get; private set; }

    [Header("Auth")]
    public string token = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 给 Web / iframe / 登录流程调用
    /// </summary>
    public void SetToken(string newToken)
    {
        token = newToken;
        Debug.Log("[ApiConfig] Token set");
    }
}
