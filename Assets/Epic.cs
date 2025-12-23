using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Epic 
{
    public int id { get; }
    public string title { get; }
    public string description { get; }
    public int chatId { get; }
    public string chatName { get; }
    public string masterName { get; }
    public int masterId { get; }
    public int count { get; } // Количество подзадач (можно использовать subTasks.Count)
    
    // Список подзадач внутри эпика (ВАЖНО: Тип AppTask, а не Task)
    public List<AppTask> subTasks { get; set; }
    
    // Ссылка на кнопку
    public GameObject EpicButton;

    public Epic(int id, string title, string description, int chatId, string masterName, int masterId, string chatName, int count)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.masterName = masterName;
        this.masterId = masterId;
        this.chatId = chatId;
        this.chatName = chatName;
        this.count = count;
        this.subTasks = new List<AppTask>(); // Инициализируем пустой список сразу
    }

    public void PutInPanel(GameObject EpicPrefab, GameObject EpicPanel, bool isMasterView)
    {
        EpicButton = GameObject.Instantiate(EpicPrefab);
        EpicButton.transform.SetParent(EpicPanel.transform, false);

        Transform titleTransform = EpicButton.transform.Find("Title");
        if(titleTransform != null) 
            titleTransform.GetComponent<TMP_Text>().text = title;
        
        // Если смотрит не мастер (например, админ), показываем чье это
        if(!isMasterView)
        {
            Transform nameTransform = EpicButton.transform.Find("MasterName");
            if(nameTransform != null)
                nameTransform.GetComponent<TMP_Text>().text = "Мастер: " + masterName;
        }

        // Счетчик задач
        Transform countTransform = EpicButton.transform.Find("Count");
        if(countTransform != null)
        {
            int currentCount = (subTasks != null) ? subTasks.Count : count;
            countTransform.GetComponent<TMP_Text>().text = "Подзадач: " + currentCount;
        }
    }
}