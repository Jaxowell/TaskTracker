using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterMenuScript : MonoBehaviour
{
    // 0-main, 1-tasks, 2-the task, 3-new task, 4-epics, 5-the epic, 6-new epic, 7-new subtask, 8-the subtask, 9-chat
    [Header("Панели и Меню")]
    [SerializeField] GameObject[] Menus; 

    [Header("Префабы и Контейнеры")]
    [SerializeField] GameObject TaskPrefab;
    [SerializeField] GameObject EpicPrefab;
    [SerializeField] GameObject TaskPanel;
    [SerializeField] GameObject SubTaskPanel;
    [SerializeField] GameObject EpicPanel;

    [Header("Ссылки")]
    [SerializeField] MenuScript Mscript;
    public DBScript db; // Получаем из Mscript

    [Header("UI Элементы просмотра Задачи")]
    [SerializeField] GameObject TaskTitle;
    [SerializeField] GameObject WorkerName;
    [SerializeField] GameObject TaskColor;
    [SerializeField] GameObject TaskStatus;
    [SerializeField] GameObject TaskDescription;
    [SerializeField] GameObject MasterTaskName;

    [Header("UI Элементы просмотра Эпика")]
    [SerializeField] GameObject EpicTitle;
    [SerializeField] GameObject EpicDescription;

    // Поля ввода для Новой Задачи
    [Header("Создание Задачи")]
    [SerializeField] TMP_InputField titleTask;
    [SerializeField] TMP_InputField descriptionTask;
    [SerializeField] TMP_Dropdown WorkerTaskDropDown;

    // Поля ввода для Нового Эпика
    [Header("Создание Эпика")]
    [SerializeField] TMP_InputField titleEpic;
    [SerializeField] TMP_InputField descriptionEpic;

    // Поля ввода для Нового Сабтаска
    [Header("Создание Подзадачи")]
    [SerializeField] TMP_InputField titleSubTask;
    [SerializeField] TMP_InputField descriptionSubTask;
    [SerializeField] TMP_Dropdown WorkerSubTaskDropDown;

    [Header("Статистика")]
    [SerializeField] TextMeshProUGUI stat;

    // Внутренние данные
    int activeMenuId = 0;
    int activeEpicId = 0; // ID текущего открытого эпика (из базы)
    int activeEpicListIndex = 0; // Индекс эпика в локальном списке

    List<Task> tasksByMaster = new List<Task>();
    List<Epic> epicsByMaster = new List<Epic>();

    public void Start()
    {
        // Инициализация при старте (если нужно)
    }

    // --- ГЛАВНЫЙ МЕТОД ЗАГРУЗКИ ---
    public void LoadMenu()
    {
        db = Mscript.db;
        
        // 1. Загружаем цвета
        StartCoroutine(db.LoadColorsWeb());

        // 2. Загружаем список работников для Dropdown
        LoadWorkers();

        // 3. Последовательно грузим Задачи -> Эпики -> Сабтаски -> Включаем меню
        StartCoroutine(LoadAllDataRoutine());
    }

    // Цепочка загрузок, чтобы данные появились корректно
    IEnumerator LoadAllDataRoutine()
    {
        // --- ШАГ 1: Задачи Мастера ---
        yield return StartCoroutine(db.GetTasksByMasterWeb(Mscript.activeUserId, (tasks) => 
        {
            tasksByMaster = tasks;
            
            // Очистка панели
            foreach(Transform child in TaskPanel.transform) Destroy(child.gameObject);

            // Отрисовка
            for (int i = 0; i < tasksByMaster.Count; i++)
            {
                string colorCode = GetColorByStatusId(tasksByMaster[i].statusId);
                tasksByMaster[i].PutInPanel(TaskPrefab, TaskPanel, colorCode, true);
                
                int index = i;
                tasksByMaster[i].TaskButton.GetComponent<Button>().onClick.AddListener(() => ShowTask(index));
            }
        }));

        // --- ШАГ 2: Эпики Мастера ---
        yield return StartCoroutine(db.GetEpicsByMasterWeb(Mscript.activeUserId, (epics) => 
        {
            epicsByMaster = epics;
            // Очистка панели эпиков
            foreach(Transform child in EpicPanel.transform) Destroy(child.gameObject);
        }));

        // --- ШАГ 3: Сабтаски для Эпиков (Рекурсивно) ---
        yield return StartCoroutine(LoadSubTasksRecursively(0));
    }

    IEnumerator LoadSubTasksRecursively(int index)
    {
        // Если прошли все эпики - финиш, включаем меню
        if (index >= epicsByMaster.Count)
        {
            FinishLoading();
            yield break;
        }

        Epic epic = epicsByMaster[index];
        
        // Загружаем сабтаски для текущего эпика
        yield return StartCoroutine(db.GetSubTasksByEpicWeb(epic.id, Mscript.activeUserId, "Me", (subTasks) => 
        {
            epic.subTasks = subTasks;
            
            // Отрисовываем кнопку эпика
            epic.PutInPanel(EpicPrefab, EpicPanel, true);
            
            int epicIndex = index;
            epic.EpicButton.GetComponent<Button>().onClick.AddListener(() => ShowEpic(epicIndex));
        }));

        // Грузим следующий
        StartCoroutine(LoadSubTasksRecursively(index + 1));
    }

    void FinishLoading()
    {
        // Выключаем все меню и включаем главное
        SwitchMenu(activeMenuId); 
    }

    // --- ОТОБРАЖЕНИЕ ---

    private void ShowTask(int listIndex)
    {
        Task t = tasksByMaster[listIndex];
        TaskTitle.GetComponent<TMP_Text>().text = t.title;
        WorkerName.GetComponent<TMP_Text>().text = "Исполнитель: " + t.workerName;
        TaskDescription.GetComponent<TMP_Text>().text = t.description;
        MasterTaskName.GetComponent<TMP_Text>().text = "Мастер: " + t.masterName;
        TaskStatus.GetComponent<TMP_Text>().text = db.GetStatusById(t.statusId);

        string colorCode = "#" + GetColorByStatusId(t.statusId);
        if(ColorUtility.TryParseHtmlString(colorCode, out Color newColor))
            TaskColor.GetComponent<Image>().color = newColor;

        SwitchMenu(2); // Меню просмотра задачи
    }

    private void ShowEpic(int listIndex)
    {
        Epic e = epicsByMaster[listIndex];
        activeEpicId = e.id;       // ID из базы для запросов
        activeEpicListIndex = listIndex; // Индекс в локальном списке

        EpicTitle.GetComponent<TMP_Text>().text = e.title;
        EpicDescription.GetComponent<TMP_Text>().text = e.description;

        // Очищаем панель сабтасков
        foreach(Transform child in SubTaskPanel.transform) Destroy(child.gameObject);

        // Отрисовываем сабтаски
        for (int i = 0; i < e.subTasks.Count; i++)
        {
            string colorCode = GetColorByStatusId(e.subTasks[i].statusId);
            e.subTasks[i].PutInPanel(TaskPrefab, SubTaskPanel, colorCode, true);
        }

        SwitchMenu(5); // Меню просмотра эпика
    }

    // --- ДОБАВЛЕНИЕ (CRUD) ---

    public void AddTask()
    {
        if (string.IsNullOrEmpty(titleTask.text) || string.IsNullOrEmpty(descriptionTask.text)) return;
        if (WorkerTaskDropDown.options.Count == 0) return;

        string workerEmail = WorkerTaskDropDown.options[WorkerTaskDropDown.value].text;

        // 1. Узнаем ID воркера
        StartCoroutine(db.GetUserIdByEmailWeb(workerEmail, (workerId) => 
        {
            if (workerId > 0)
            {
                int masterId = Mscript.activeUserId;
                
                // 2. Отправляем задачу на сервер
                StartCoroutine(db.AddTaskWeb(titleTask.text, descriptionTask.text, masterId, workerId, (newId) => 
                {
                    // 3. Обновляем UI локально (чтобы не перезагружать всё)
                    Task task = new Task(newId, titleTask.text, db.GetStatusById(1), workerId, descriptionTask.text, 1, "Me", masterId); // workerName временно статус или заглушка, т.к. имени не знаем без доп запроса
                    tasksByMaster.Add(task);
                    
                    task.PutInPanel(TaskPrefab, TaskPanel, GetColorByStatusId(1), true);
                    int idx = tasksByMaster.Count - 1;
                    task.TaskButton.GetComponent<Button>().onClick.AddListener(() => ShowTask(idx));

                    // Очистка
                    titleTask.text = ""; descriptionTask.text = ""; WorkerTaskDropDown.value = 0;
                    SwitchMenu(1); // Возврат к списку задач
                }));
            }
        }));
    }

    public void AddEpic()
    {
        if (string.IsNullOrEmpty(titleEpic.text) || string.IsNullOrEmpty(descriptionEpic.text)) return;

        int masterId = Mscript.activeUserId;

        StartCoroutine(db.AddEpicWeb(titleEpic.text, descriptionEpic.text, masterId, (newId) => 
        {
            Epic epic = new Epic(newId, titleEpic.text, descriptionEpic.text, 0, "Me", masterId, "Chat", 0);
            epic.subTasks = new List<Task>();
            epicsByMaster.Add(epic);

            epic.PutInPanel(EpicPrefab, EpicPanel, true);
            int idx = epicsByMaster.Count - 1;
            epic.EpicButton.GetComponent<Button>().onClick.AddListener(() => ShowEpic(idx));

            titleEpic.text = ""; descriptionEpic.text = "";
            SwitchMenu(4); // Возврат к списку эпиков
        }));
    }

    public void AddSubTask()
    {
        if (string.IsNullOrEmpty(titleSubTask.text) || string.IsNullOrEmpty(descriptionSubTask.text)) return;
        if (WorkerSubTaskDropDown.options.Count == 0) return;

        string workerEmail = WorkerSubTaskDropDown.options[WorkerSubTaskDropDown.value].text;

        StartCoroutine(db.GetUserIdByEmailWeb(workerEmail, (workerId) => 
        {
            if (workerId > 0)
            {
                // Используем сохраненный ID эпика
                StartCoroutine(db.AddSubTaskWeb(titleSubTask.text, descriptionSubTask.text, activeEpicId, workerId, (newId) => 
                {
                    // Добавляем в локальный список текущего эпика
                    Task sub = new Task(newId, titleSubTask.text, "Worker", workerId, descriptionSubTask.text, 1, "Me", Mscript.activeUserId);
                    epicsByMaster[activeEpicListIndex].subTasks.Add(sub);

                    // Отрисовываем
                    sub.PutInPanel(TaskPrefab, SubTaskPanel, GetColorByStatusId(1), true);

                    titleSubTask.text = ""; descriptionSubTask.text = ""; WorkerSubTaskDropDown.value = 0;
                    SwitchMenu(5); // Возврат к просмотру эпика
                }));
            }
        }));
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ ---

    void LoadWorkers()
    {
        StartCoroutine(db.GetWorkersEmailsWeb((emails) => 
        {
            WorkerTaskDropDown.ClearOptions();
            WorkerSubTaskDropDown.ClearOptions();
            
            WorkerTaskDropDown.AddOptions(emails);
            WorkerSubTaskDropDown.AddOptions(emails);
        }));
    }

    public void OpenChat()
    {
        // 9 - это индекс меню чата в твоем массиве Menus
        SwitchMenu(9); 
    }

    public void Exit()
    {
        // Очистка списков кнопок перед выходом
        foreach (var t in tasksByMaster) if(t.TaskButton) Destroy(t.TaskButton);
        tasksByMaster.Clear();
        
        foreach (var e in epicsByMaster) {
            if(e.EpicButton) Destroy(e.EpicButton);
            if(e.subTasks != null) 
                foreach(var s in e.subTasks) if(s.TaskButton) Destroy(s.TaskButton);
        }
        epicsByMaster.Clear();

        Mscript.ChangeMenu(1); // Возврат на экран входа
    }

    // Универсальный переключатель меню
    public void SwitchMenu(int newActiveId)
    {
        if (Menus == null || Menus.Length == 0) return;

        // Выключаем всё
        foreach(var menu in Menus) if(menu != null) menu.SetActive(false);

        // Включаем нужное
        if (newActiveId >= 0 && newActiveId < Menus.Length && Menus[newActiveId] != null)
        {
            Menus[newActiveId].SetActive(true);
            activeMenuId = newActiveId;
        }
    }

    // Методы для кнопок навигации (назначай их в Unity Inspector)
    public void OpenMainMenu() => SwitchMenu(0);
    public void OpenTasksMenu() => SwitchMenu(1);
    public void OpenEpicsMenu() => SwitchMenu(4);
    public void OpenNewTaskMenu() => SwitchMenu(3);
    public void OpenNewEpicMenu() => SwitchMenu(6);
    public void OpenNewSubTaskMenu() => SwitchMenu(7);

    // Безопасное получение цвета
    string GetColorByStatusId(int id)
    {
        if (db.statusColors != null && id > 0 && id <= db.statusColors.Length)
            return db.statusColors[id - 1];
        return "FFFFFF";
    }
}