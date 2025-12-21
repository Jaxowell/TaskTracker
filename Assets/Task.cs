using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Task 
{
    public int id { get;}
    public GameObject TaskButton;
    public string title { get; }
    public string workerName { get; }
    public int workerId { get; }
    public string description { get; }
    public int statusId { get; }
    public string masterName { get; }
    public int masterId { get; }

    public Task(int id,string title, string workerName, int workerId, string description, int statusId, string masterName, int masterId)
    {
        this.id = id;
        this.title = title;
        this.workerName = workerName;
        this.workerId = workerId;
        this.description = description;
        this.statusId = statusId;
        this.masterName = masterName;
        this.masterId = masterId;
    }
    public void ChangeColor(string colorCode)
    {
        Transform statusTransform = TaskButton.transform.Find("Status");
        //colorCode = "#" + colorCode;
        ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        statusTransform.GetComponent<Image>().color = newColor;
    }
    public void PutInPanel(GameObject TaskPrefab, GameObject TaskPanel, string colorCode, bool master)
    {
        TaskButton = GameObject.Instantiate(TaskPrefab);
        TaskButton.transform.SetParent(TaskPanel.transform);

        Transform titleTransform = TaskButton.transform.Find("Title");
        titleTransform.GetComponent<TMP_Text>().text = title;

        Transform nameTransform = TaskButton.transform.Find("Name");
        nameTransform.GetComponent<TMP_Text>().text = master ? "Исполнитель:"  + workerName : "Тимлид:"+ masterName;

        Transform statusTransform = TaskButton.transform.Find("Status");

        colorCode = "#" + colorCode;
        ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        statusTransform.GetComponent<Image>().color = newColor;

        //tasksByMaster[number].description;
    }
    public string Print()
    {
        string res = $"Задача({id}):\" {title}\" назначена {masterName}({masterId}), выполняет {workerName}({workerId}), статус:{statusId}";
        return res;
    }
}
