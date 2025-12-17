using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;          

public class ChatScript : MonoBehaviour
{
    public static ChatScript Instance;

    [Header("UI")]
    [SerializeField] private GameObject chatWindowRoot;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePrefab;

    

    private DBScript db;
    private int currentTaskId = -1; 
    private int currentUserId = 1;  
    private bool isChatOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        db = new DBScript();
        
        if (chatWindowRoot != null) chatWindowRoot.SetActive(false);
    }

    public void OpenChat(int taskId, int userId)
    {
        // Если сам объект Chat выключен, включаем
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        currentTaskId = taskId;
        currentUserId = userId;
        
        isChatOpen = true;
        chatWindowRoot.SetActive(true);
        
        // 1. Сразу загружаем сообщения один раз
        StartCoroutine(UpdateMessagesRoutine());

        // 2. Запускаем цикл автообновления
        StopAllCoroutines(); // На всякий случай останавливаем старые
        StartCoroutine(ChatLoop());
    }

    public void CloseChat()
    {
        isChatOpen = false;
        StopAllCoroutines(); // ОСТАНАВЛИВАЕМ таймер, чтобы не грузить сеть
        chatWindowRoot.SetActive(false);
    }

    public void OnSendButton()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim())) return;
        
        string text = inputField.text.Trim();
        inputField.text = "";
        inputField.ActivateInputField(); // Вернуть фокус, чтобы писать дальше удобно

        // Отправляем
        StartCoroutine(db.SendMessageWeb(currentTaskId, currentUserId, text));

        // Обновляем чат чуть погодя (даем серверу 0.1 сек на запись)
        Invoke("ForceUpdate", 0.1f);
    }

    void ForceUpdate() 
    {
        if(isChatOpen) StartCoroutine(UpdateMessagesRoutine()); 
    }

    // --- ГЛАВНАЯ МАГИЯ: ЦИКЛ АВТООБНОВЛЕНИЯ ---
    IEnumerator ChatLoop()
    {
        while (isChatOpen)
        {
            // Ждем 1.5 секунды
            yield return new WaitForSeconds(1.5f);
            
            // Если окно всё еще открыто - обновляем
            if (isChatOpen)
            {
                yield return StartCoroutine(UpdateMessagesRoutine());
            }
        }
    }

    IEnumerator UpdateMessagesRoutine()
    {
        // Запрашиваем сообщения
        yield return StartCoroutine(db.GetChatMessagesWeb(currentTaskId, (messages) => 
        {
            // Если окно закрыли, пока ждали ответ - ничего не делаем
            if (!isChatOpen) return;

            // Очистка (удаляем старые сообщения)
            foreach (Transform child in contentContainer) Destroy(child.gameObject);

            // Создание новых
            foreach (var msg in messages)
            {
                CreateMessageBubble(msg.SenderName, msg.Text); 
            }
            
            // Принудительный скролл вниз (можно убрать, если мешает читать)
            // StartCoroutine(ForceScrollDown());
        }));
    }

    private void CreateMessageBubble(string userName, string text)
    {
        if (messagePrefab == null) return;
        
        GameObject bubble = Instantiate(messagePrefab, contentContainer);
        TMP_Text tmp = bubble.GetComponentInChildren<TMP_Text>();
        
        if (tmp != null)
        {
            // Получаем имя текущего юзера (для раскраски)
            // Тут можно доработать, сравнивая ID, но пока сравним имена или просто покрасим всех
            // Для простоты пока красим всех одинаково, или выделяем "себя" если знаем своё имя
            
            // Простая раскраска: Имя жирным, текст обычным
            tmp.text = $"<b><color=#FFA500>{userName}:</color></b> {text}";
        }
    }

    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) scrollRect.verticalNormalizedPosition = 0f; 
    }
}