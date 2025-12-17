using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerMenu : MonoBehaviour
{
    [SerializeField] MenuScript Mscript; // Ссылка на главное меню
    [SerializeField] GameObject TaskLayout; // Контейнер для кнопок (Content)
    [SerializeField] GameObject TaskPrefab; // Префаб кнопки
    
    // Список для хранения созданных кнопок
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private bool isMenuOpen = false;

    public void LoadMenu()
    {
        if (isMenuOpen) return;
        isMenuOpen = true;
        
        StartCoroutine(TasksUpdateLoop());
    }

    private void OnEnable()
    {
        LoadMenu();
    }

    private void OnDisable()
    {
        isMenuOpen = false;
        StopAllCoroutines();
    }

    IEnumerator TasksUpdateLoop()
    {
        while (isMenuOpen)
        {
            yield return StartCoroutine(LoadTasksRoutine());
            yield return new WaitForSeconds(3.0f); 
        }
    }

    IEnumerator LoadTasksRoutine()
    {
        if (Mscript == null || Mscript.db == null) yield break;

        int myId = Mscript.activeUserId;

        yield return StartCoroutine(Mscript.db.GetWorkerTasksWeb(myId, (tasks) => 
        {
            if (!isMenuOpen) return;

            foreach (GameObject btn in spawnedButtons) Destroy(btn);
            spawnedButtons.Clear();

            foreach (var task in tasks)
            {
                if (TaskPrefab != null && TaskLayout != null)
                {
                    GameObject newTask = Instantiate(TaskPrefab, TaskLayout.transform);
                    
                    TMP_Text btnText = newTask.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        string color = "#FFFFFF";
                        if (task.status_name == "В работе") color = "#FFFF00"; // Желтый
                        if (task.status_name == "Завершена") color = "#00FF00"; // Зеленый
                        
                        btnText.text = $"{task.title} <color={color}>({task.status_name})</color>";
                    }

                    Button btnComp = newTask.GetComponent<Button>();
                    if (btnComp != null)
                    {
                        btnComp.onClick.AddListener(() => OpenTaskDetails(task.idTask));
                    }

                    spawnedButtons.Add(newTask);
                }
            }
        }));
    }

    void OpenTaskDetails(int taskId)
    {
        if (ChatScript.Instance != null)
        {
            ChatScript.Instance.OpenChat(taskId, Mscript.activeUserId);
        }
        else
        {
            Debug.LogError("Ошибка: ChatScript не найден на сцене!");
        }
    }

    public void ExitToMenu()
    {
        isMenuOpen = false;
        if (Mscript != null) Mscript.ChangeMenu(1);
    }

    public void OpenChat()
    {
        if (ChatScript.Instance != null && Mscript != null)
        {
            ChatScript.Instance.OpenChat(0, Mscript.activeUserId);
        }
        else
        {
            Debug.LogError("ChatScript или MenuScript не найдены!");
        }
    }
}