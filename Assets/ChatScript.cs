using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChatScript : MonoBehaviour
{
    // Бэкинг-филд и надёжный геттер, который найдёт скрипт даже на неактивном объекте в сцене
    private static ChatScript _instance;
    public static ChatScript Instance
    {
        get
        {
            if (_instance == null)
            {
                // Ищем все экземпляры, включая неактивные
                var all = Resources.FindObjectsOfTypeAll<ChatScript>();
                foreach (var cs in all)
                {
                    // берём тот, который принадлежит сцене (т.е. не prefab в проекте)
                    if (cs.gameObject.scene.IsValid())
                    {
                        _instance = cs;
                        break;
                    }
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Основные элементы")]
    [SerializeField] private GameObject chatWindow; // Сам объект чата (Canvas / Panel)
    [SerializeField] private ScrollRect scrollRect; // Scroll View -> ScrollRect
    [SerializeField] private Transform messageContainer; // Content внутри Scroll View
    [SerializeField] private GameObject messagePrefab; // Префаб сообщения (использует UI -> Text)

    [Header("Элементы ввода")]
    [SerializeField] private InputField messageInput; // Обычный (legacy) InputField
    [SerializeField] private Button sendButton; // Кнопка отправки
    [SerializeField] private Button closeButton; // Кнопка закрытия
    [Header("Поведение при старте")]
    [SerializeField] private bool hideChatWindowOnAwake = true; // скрывать окно чата как можно раньше
    [Header("Интеграция с БД / пользователем")]
    [SerializeField] private MenuScript menuScript; // ссылка на MenuScript, содержит activeUserId и db


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        // Если нужно — скрываем окно чата как можно раньше (в Awake, до Start)
        if (hideChatWindowOnAwake)
        {
            if (chatWindow == null)
            {
                // Попробуем найти по распространённым именам в сцене
                var found = GameObject.Find("Chat_window") ?? GameObject.Find("ChatWindow") ?? GameObject.Find("Chat_Window");
                if (found != null)
                {
                    chatWindow = found;
                    Debug.Log("ChatScript: chatWindow не был назначен — найден объект по имени и назначен автоматически.");
                }
            }

            if (chatWindow != null)
            {
                chatWindow.SetActive(false);
            }
            else
            {
                Debug.LogWarning("ChatScript: hideChatWindowOnAwake=true, но chatWindow не найден и не назначен.");
            }
        }
    }

    private void Start()
    {
        // Подписываемся на событие кнопки отправки
        if (sendButton != null) sendButton.onClick.AddListener(SendMessage);

        // Кнопка закрытия
        if (closeButton != null) closeButton.onClick.AddListener(CloseChat);

        // Скрыть чат при старте
        if (chatWindow != null) chatWindow.SetActive(false);

        // Очистить поле ввода
        if (messageInput != null) messageInput.text = string.Empty;

        // Если не назначен контейнер сообщений, попробуем взять его из scrollRect.content
        if (messageContainer == null && scrollRect != null && scrollRect.content != null)
        {
            messageContainer = scrollRect.content.transform;
            Debug.Log("ChatScript: messageContainer автоматически установлен из scrollRect.content");
        }

        // Диагностическое сообщение, если что-то не назначено
        if (messageContainer == null)
            Debug.LogWarning("ChatScript: messageContainer не назначен в инспекторе и не найден в scrollRect.content");
        if (scrollRect == null)
            Debug.LogWarning("ChatScript: scrollRect не назначен в инспекторе");

        // Проверяем и при необходимости добавляем компоненты layout на messageContainer
        if (messageContainer != null)
        {
            var vl = messageContainer.GetComponent<VerticalLayoutGroup>();
            if (vl == null)
            {
                vl = messageContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                vl.childAlignment = TextAnchor.UpperCenter;
                vl.childForceExpandHeight = false;
                vl.childForceExpandWidth = true;
                vl.spacing = 6f;
                Debug.Log("ChatScript: Добавлен VerticalLayoutGroup на messageContainer (автоматически)");
            }

            var csf = messageContainer.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = messageContainer.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                Debug.Log("ChatScript: Добавлен ContentSizeFitter на messageContainer (автоматически)");
            }
            
            // Убедимся, что Content растягивается по ширине внутри Viewport
            var rt = messageContainer as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
                rt.pivot = new Vector2(0.5f, rt.pivot.y);
            }
        }

        // Попытаемся автоматически подцепить MenuScript если не назначен
        if (menuScript == null)
        {
            menuScript = FindObjectOfType<MenuScript>();
            if (menuScript != null)
                Debug.Log("ChatScript: menuScript автоматически найден в сцене.");
        }
    }

    private void Update()
    {
        // Отправка по Enter когда поле ввода в фокусе
        if (messageInput != null && messageInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendMessage();
            }
        }
    }

    // Открыть чат
    public void OpenChat()
    {
        if (chatWindow != null) chatWindow.SetActive(true);
        if (messageInput != null) messageInput.ActivateInputField();
    }

    // Закрыть чат
    public void CloseChat()
    {
        if (chatWindow != null)
            chatWindow.SetActive(false);
    }

    public void SendMessage()
    {
        if (messageInput == null || messagePrefab == null || messageContainer == null || scrollRect == null)
            return;

        string messageRaw = messageInput.text.Trim();
        if (string.IsNullOrEmpty(messageRaw))
            return;

        // Получаем имя текущего пользователя
        string username = "Unknown";
        try
        {
            if (menuScript != null)
            {
                int uid = menuScript.activeUserId;
                if (uid != 0)
                {
                    var db = menuScript.db;
                    if (db != null)
                    {
                        var name = db.GetUserNameById(uid);
                        if (!string.IsNullOrEmpty(name)) username = name;
                    }
                }
            }
        }
        catch {
            // fallback to Unknown on any DB problem
        }

        string messageText = $"{username}: {messageRaw}";
        if (string.IsNullOrEmpty(messageText))
            return;

        // Создаем новое сообщение из префаба и правильно парентим (сохраняя локальные трансформы)
        GameObject newMessage = Instantiate(messagePrefab);
        newMessage.SetActive(true);
        newMessage.transform.SetParent(messageContainer, false); // false = сохранить локальные координаты для UI
        newMessage.transform.SetAsLastSibling(); // добавляем в конец, чтобы новые были снизу

        // Попытаться найти legacy Text внутри префаба и установить текст
        Text textComponent = newMessage.GetComponentInChildren<Text>(true);
        if (textComponent != null)
        {
            textComponent.text = messageText;
        }
        else
        {
            // Попробуем найти TextMeshPro компонент, если он используется
            var tmp = newMessage.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmp != null)
                tmp.text = messageText;
        }

        // Подготовить инстанс сообщения к layout (anchors, ContentSizeFitter на тексте и т.п.)
        ConfigureMessageForLayout(newMessage);

        // Очищаем поле ввода и возвращаем фокус
        messageInput.text = string.Empty;
        messageInput.ActivateInputField();

        // Убедимся, что scrollRect.content назначен
        if (scrollRect.content == null && messageContainer is RectTransform)
            scrollRect.content = messageContainer as RectTransform;

        // Принудительно пересобираем layout контента и прокручиваем вниз
        var rt = messageContainer as RectTransform;
        if (rt != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        Canvas.ForceUpdateCanvases();
        // Прокрутить в самый низ (новые сообщения снизу)
        scrollRect.verticalNormalizedPosition = 0f;
        // Сбросить скорость прокрутки чтобы визуально встало на место
        if (scrollRect.velocity.sqrMagnitude > 0f)
            scrollRect.velocity = Vector2.zero;
    }

    // Настраивает только что созданный элемент сообщения так, чтобы он корректно влиял на размер Content
    private void ConfigureMessageForLayout(GameObject newMessage)
    {
        if (newMessage == null) return;

        // Корневой RectTransform элемента
        var msgRt = newMessage.GetComponent<RectTransform>();
        if (msgRt != null)
        {
            msgRt.anchorMin = new Vector2(0f, msgRt.anchorMin.y);
            msgRt.anchorMax = new Vector2(1f, msgRt.anchorMax.y);
            msgRt.pivot = new Vector2(0.5f, msgRt.pivot.y);
            // Обнулим sizeDelta.x чтобы растягивалось по ширине
            var sd = msgRt.sizeDelta;
            sd.x = 0f;
            msgRt.sizeDelta = sd;
        }

        // Найдём текст (legacy)
        var text = newMessage.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            // Убедимся, что у текста есть ContentSizeFitter чтобы корректно сообщать высоту
            var csf = text.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = text.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            // Подправим RectTransform текста
            var textRt = text.GetComponent<RectTransform>();
            if (textRt != null)
            {
                textRt.anchorMin = new Vector2(0f, textRt.anchorMin.y);
                textRt.anchorMax = new Vector2(1f, textRt.anchorMax.y);
                var s = textRt.sizeDelta; s.x = 0f; textRt.sizeDelta = s;
            }

            // Форсируем пересчёт layout для этого элемента
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(textRt);
            return;
        }

        // Если нет legacy текста — попробуем TMP
        var tmp = newMessage.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null)
        {
            var csf2 = tmp.GetComponent<ContentSizeFitter>();
            if (csf2 == null)
            {
                csf2 = tmp.gameObject.AddComponent<ContentSizeFitter>();
                csf2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf2.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            var tmpRt = tmp.GetComponent<RectTransform>();
            if (tmpRt != null)
            {
                tmpRt.anchorMin = new Vector2(0f, tmpRt.anchorMin.y);
                tmpRt.anchorMax = new Vector2(1f, tmpRt.anchorMax.y);
                var s2 = tmpRt.sizeDelta; s2.x = 0f; tmpRt.sizeDelta = s2;
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tmpRt);
            }
        }
    }
}
