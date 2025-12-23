using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppTask 
{
    // Поля данных (только для чтения извне)
    public int id { get; }
    public string title { get; }
    public string workerName { get; }
    public int workerId { get; }
    public string description { get; }
    public int statusId { get; }
    public string masterName { get; }
    public int masterId { get; }
    public int user_task_id { get; } // ID исполнителя

    // Ссылка на кнопку в UI (чтобы можно было ее удалять или менять цвет)
    public GameObject TaskButton;

    // Конструктор
    public AppTask(int id, string title, string workerName, int workerId, string description, int statusId, string masterName, int masterId)
    {
        this.id = id;
        this.title = title;
        this.workerName = workerName;
        this.workerId = workerId;
        this.user_task_id = workerId; // Дублируем для удобства
        this.description = description;
        this.statusId = statusId;
        this.masterName = masterName;
        this.masterId = masterId;
    }

    // Метод для смены цвета кнопки без полной перезагрузки
    public void ChangeColor(string colorCode)
    {
        if (TaskButton == null) return;

        Transform statusTransform = TaskButton.transform.Find("Status");
        if (statusTransform != null)
        {
            if (!colorCode.StartsWith("#")) colorCode = "#" + colorCode;
            
            if (ColorUtility.TryParseHtmlString(colorCode, out Color newColor))
            {
                statusTransform.GetComponent<Image>().color = newColor;
            }
        }
    }

    // Метод создания кнопки в списке
    public void PutInPanel(GameObject TaskPrefab, GameObject TaskPanel, string colorCode, bool isMasterView)
    {
        // Создаем кнопку
        TaskButton = GameObject.Instantiate(TaskPrefab);
        // Важно: false, чтобы сохранить масштаб префаба
        TaskButton.transform.SetParent(TaskPanel.transform, false); 

        // Заполняем заголовок
        Transform titleTransform = TaskButton.transform.Find("Title");
        if (titleTransform != null) 
            titleTransform.GetComponent<TMP_Text>().text = title;

        // Заполняем имя (если смотрит Мастер - видим Воркера, если Воркер - видим Мастера)
        Transform nameTransform = TaskButton.transform.Find("Name");
        if (nameTransform != null)
        {
            nameTransform.GetComponent<TMP_Text>().text = isMasterView ? "Исполнитель: " + workerName : "Мастер: " + masterName;
        }

        // Красим статус
        Transform statusTransform = TaskButton.transform.Find("Status");
        if (statusTransform != null)
        {
            if (!colorCode.StartsWith("#")) colorCode = "#" + colorCode;

            if (ColorUtility.TryParseHtmlString(colorCode, out Color newColor))
            {
                statusTransform.GetComponent<Image>().color = newColor;
            }
        }
    }

    // Для отладки
    public string Print()
    {
        return $"Задача({id}): \"{title}\" Мастер: {masterName}, Исполнитель: {workerName}, СтатусID: {statusId}";
    }
}