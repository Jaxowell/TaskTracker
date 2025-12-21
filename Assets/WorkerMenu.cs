using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerMenu : MonoBehaviour
{
    // 0-main, 1-tasks, 2-the task
    [Header("UI Панели")]
    [SerializeField] GameObject[] Menus; 
    
    [Header("Контейнеры")]
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject TaskPrefab;

    [Header("Ссылки")]
    [SerializeField] MenuScript Mscript;
    public DBScript db;

    // Внутренние данные
    int activeMenuId = 0;
    int activeTaskIndex = 0; // Индекс задачи в списке (не ID из базы)
    List<Task> tasksByWorker = new List<Task>();

    [Header("Элементы просмотра задачи")]
    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] TMP_Dropdown StatusDropDown;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] GameObject MasterName;

    public void Start()
    {
        // Инициализация
    }

    // --- ЗАГРУЗКА МЕНЮ ---
    public void LoadMenu()
    {
        db = Mscript.db;
        
        // 1. Грузим цвета (на всякий случай)
        StartCoroutine(db.LoadColorsWeb());

        // 2. Грузим задачи воркера с сервера
        StartCoroutine(db.GetTasksByWorkerWeb(Mscript.activeUserId, (tasks) => 
        {
            tasksByWorker = tasks;
            Debug.Log("Загружено задач воркера: " + tasksByWorker.Count);

            // Очистка панели
            foreach(Transform child in TaskPanel.transform) Destroy(child.gameObject);

            // Отрисовка кнопок
            for (int i = 0; i < tasksByWorker.Count; i++)
            {
                // Безопасное получение цвета
                string colorCode = "FFFFFF";
                if(tasksByWorker[i].statusId > 0 && tasksByWorker[i].statusId <= db.statusColors.Length)
                    colorCode = db.statusColors[tasksByWorker[i].statusId - 1];

                tasksByWorker[i].PutInPanel(TaskPrefab, TaskPanel, colorCode, false);
                
                // Привязка нажатия
                int index = i;
                tasksByWorker[i].TaskButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowTask(index);
                });
            }

            LoadStatuses();
            SwitchMenu(1); // Показываем список задач
        }));
        
        // Скрываем всё перед загрузкой
        foreach (var m in Menus) m.SetActive(false);
    }

    // --- ПРОСМОТР ЗАДАЧИ ---
    private void ShowTask(int index)
    {
        activeTaskIndex = index;
        Task t = tasksByWorker[index];

        TaskTitle.GetComponent<TMP_Text>().text = t.title;
        WorkerName.GetComponent<TMP_Text>().text = "Исполнитель: " + t.workerName;
        TaskDescription.GetComponent<TMP_Text>().text = t.description;
        MasterName.GetComponent<TMP_Text>().text = "Мастер: " + t.masterName;
        
        // Устанавливаем значение в Dropdown (id статуса 1..4 -> индекс 0..3)
        StatusDropDown.SetValueWithoutNotify(t.statusId - 1);

        // Красим плашку цвета
        UpdateColorBox(t.statusId - 1);

        SwitchMenu(2);
    }

    // --- ИЗМЕНЕНИЕ СТАТУСА ---
    public void ChangeStatus()
    {
        int newStatusIndex = StatusDropDown.value; // 0..3
        int newStatusId = newStatusIndex + 1;      // 1..4 (для базы)

        // 1. Обновляем цвет локально
        UpdateColorBox(newStatusIndex);
        
        // 2. Меняем цвет кнопки в списке (визуально)
        string colorCode = "#" + db.statusColors[newStatusIndex];
        tasksByWorker[activeTaskIndex].ChangeColor(colorCode);
        
        // 3. Отправляем на сервер
        int taskId = tasksByWorker[activeTaskIndex].id;
        StartCoroutine(db.ChangeStatusWeb(taskId, newStatusId));
    }

    void UpdateColorBox(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < db.statusColors.Length)
        {
            string colorCode = "#" + db.statusColors[colorIndex];
            if (ColorUtility.TryParseHtmlString(colorCode, out Color newColor))
                TaskColor.GetComponent<Image>().color = newColor;
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ ---
    void LoadStatuses()
    {
        StatusDropDown.ClearOptions();
        StatusDropDown.AddOptions(db.LoadStatuses());
    }

    public void SwitchMenu(int newActiveId)
    {
        if (Menus == null || Menus.Length == 0) return;

        foreach (var m in Menus) m.SetActive(false);
        
        if(newActiveId >= 0 && newActiveId < Menus.Length)
        {
            Menus[newActiveId].SetActive(true);
            activeMenuId = newActiveId;
        }
    }

    public void Exit()
    {
        // Очистка кнопок
        foreach (var t in tasksByWorker) if(t.TaskButton) Destroy(t.TaskButton);
        tasksByWorker.Clear();

        Mscript.ChangeMenu(1); // Выход в логин
    }
    
    // Если нужна кнопка чата внутри задачи
    public void OpenChat()
    {
        if (ChatScript.Instance != null && tasksByWorker.Count > 0)
        {
            // Открываем чат конкретной задачи
            int taskId = tasksByWorker[activeTaskIndex].id;
            ChatScript.Instance.OpenChat(taskId, Mscript.activeUserId);
        }
    }
}