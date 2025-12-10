using System.Collections;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using UnityEngine.Networking; 
using Unity.VisualScripting.Dependencies.Sqlite;
using System.Xml.Linq;
using Mono.Data.Sqlite;

public class ChatScript : MonoBehaviour
{
    public static ChatScript Instance;

    [Header("UI Компоненты")]
    [SerializeField] private GameObject chatWindowRoot;     // Вся панель чата (включая фон)
    [SerializeField] private ScrollRect scrollRect;         // Скролл
    [SerializeField] private Transform contentContainer;    // Content внутри скролла
    [SerializeField] private TMP_InputField inputField;     // Поле ввода (TMP!)
    [SerializeField] private GameObject messagePrefab;      // Префаб сообщения (должен быть в Project)

    // Внутренние данные
    private DBScript db;
    private int currentTaskId = -1; 
    private int currentUserId = 1;  // Нужно получать при логине
    private bool isChatOpen = false;

    private void Awake()
    {
        SqliteConnection.ClearAllPools();
        // Singleton - чтобы был один экземпляр
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        db = new DBScript();
        
        // Гарантированно создаем таблицы при старте, чтобы всё работало
        db.InitChatTables();

        // Скрываем окно при запуске
        if (chatWindowRoot != null) chatWindowRoot.SetActive(false);
    }

    /// <summary>
    /// Метод открытия чата
    /// </summary>
    /// <param name="taskId">ID задачи (Task ID)</param>
    /// <param name="userId">ID текущего пользователя (кто открыл)</param>
    public void OpenChat(int taskId, int userId)
    {
        // Включаем сам Chat объект (если он вдруг выключен в иерархии)
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        currentTaskId = taskId;
        currentUserId = userId;

        Debug.Log($"Открыт чат. Task: {taskId}, User: {userId}");

        ShowInterface();
    }
    
    // Для совместимости, если где-то вызывают без параметров
    public void OpenChat() 
    {
        // Временная заглушка, лучше не использовать
        OpenChat(1, 1);
    }

    private void ShowInterface()
    {
        isChatOpen = true;
        chatWindowRoot.SetActive(true);
        
        // Сразу загружаем сообщения
        UpdateMessages();
        
        // Запускаем автообновление (поллинг) раз в 1.5 секунды
        // Это симуляция работы сервера, где новые сообщения могут прийти от других
        StopAllCoroutines();
        StartCoroutine(ChatLoop());
    }

    public void CloseChat()
    {
        isChatOpen = false;
        StopAllCoroutines();
        chatWindowRoot.SetActive(false);
    }

    // Метод для кнопки "Отправить"
    public void OnSendButton()
    {
        if (inputField == null) return;
        string text = inputField.text.Trim();

        if (string.IsNullOrEmpty(text)) return;

        // 1. Отправляем в базу
        db.SendMessage(currentTaskId, currentUserId, text);

        // 2. Очищаем поле
        inputField.text = "";
        
        // 3. Держим фокус на поле ввода (удобство)
        inputField.ActivateInputField();

        // 4. Мгновенно обновляем чат
        UpdateMessages();
        
        // 5. Принудительный скролл вниз
        StartCoroutine(ForceScrollDown());
    }

    // Перерисовка сообщений
    private void UpdateMessages()
    {
        // Сначала очищаем старые плашки (можно оптимизировать через пул объектов, но пока так)
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Получаем данные из базы
        var messages = db.GetChatMessages(currentTaskId);

        // Создаем новые
        foreach (var msg in messages)
        {
            CreateMessageBubble(msg.SenderName, msg.Text, msg.SenderName == GetCurrentUserName());
        }
    }


    private void CreateMessageBubble(string userName, string text, bool isMe)
    {
        if (messagePrefab == null) return;

        GameObject bubble = Instantiate(messagePrefab, contentContainer);
        
        TMP_Text tmp = bubble.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            string colorHex = isMe ? "#00FF00" : "#FFA500"; 
            tmp.text = $"<color={colorHex}><b>{userName}:</b></color> {text}";
        }
    }

    private string GetCurrentUserName()
    {
        return db.GetUserNameById(currentUserId); 
    }

    IEnumerator ChatLoop()
    {
        while (isChatOpen)
        {
            yield return new WaitForSeconds(1.5f);
            UpdateMessages(); 
        }
    }

    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) scrollRect.verticalNormalizedPosition = 0f; 
        Canvas.ForceUpdateCanvases();
    }
}