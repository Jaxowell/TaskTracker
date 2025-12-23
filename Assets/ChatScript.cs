using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;          

public class ChatScript : MonoBehaviour
{
    public static ChatScript Instance;

    [Header("UI Чата")]
    [SerializeField] private GameObject chatWindowRoot;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePrefab;

    // Временная ссылка на базу, создадим свою или возьмем из MenuScript
    private DBScript db;
    private int currentTaskId = -1; 
    private int currentUserId = 0;  
    private bool isChatOpen = false;

    private int currentEntityId = -1; 
    private string currentType = "task"; // task, epic, subtask

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Чтобы не зависеть от порядка загрузки, можно создать экземпляр тут
        db = new DBScript();
        
        // Найдем URL из MenuScript, если он есть на сцене
        MenuScript ms = FindObjectOfType<MenuScript>();
        if(ms != null && ms.db != null) db.serverUrl = ms.db.serverUrl;

        if (chatWindowRoot) chatWindowRoot.SetActive(false);
    }

    public void OpenChat(int id, int userId, string type)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        currentEntityId = id;
        currentUserId = userId;
        currentType = type; // Запоминаем тип!
        
        isChatOpen = true;
        chatWindowRoot.SetActive(true);
        
        StartCoroutine(ChatLoop());
    }

    public void CloseChat()
    {
        isChatOpen = false;
        StopAllCoroutines();
        chatWindowRoot.SetActive(false);
    }

    public void OnSendButton()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim())) return;
        string text = inputField.text.Trim();
        inputField.text = "";

        // Передаем тип в запрос
        StartCoroutine(db.SendMessageWeb(currentEntityId, currentUserId, text, currentType));
        Invoke("ForceUpdate", 0.2f);
    }

    void ForceUpdate() { if(isChatOpen) StartCoroutine(UpdateMessagesRoutine()); }

    IEnumerator ChatLoop()
    {
        yield return StartCoroutine(UpdateMessagesRoutine());
        while (isChatOpen)
        {
            yield return new WaitForSeconds(2.0f);
            if (isChatOpen) yield return StartCoroutine(UpdateMessagesRoutine());
        }
    }

    IEnumerator UpdateMessagesRoutine()
    {
        // Передаем тип в запрос
        yield return StartCoroutine(db.GetChatMessagesWeb(currentEntityId, currentType, (messages) => 
        {
            if (!isChatOpen) return;
            foreach (Transform child in contentContainer) Destroy(child.gameObject);
            foreach (var msg in messages) CreateMessageBubble(msg.SenderName, msg.Text, msg.Time); 
        }));
    }

    private void CreateMessageBubble(string userName, string text, string time)
    {
        if (messagePrefab == null) return;
        GameObject bubble = Instantiate(messagePrefab, contentContainer);
        TMP_Text tmp = bubble.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = $"<b>{userName}:</b> {text}\n<size=50%><color=#aaaaaa>{time}</color></size>";
    }
}