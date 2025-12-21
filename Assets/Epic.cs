using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Epic 
{
    public int id { get;}
    public GameObject EpicButton;
    public string title { get; }
    public string description { get; }
    //public int statusId { get; }
    public int chatId { get; }
    public string chatName { get; }
    public string masterName { get; }
    public int masterId { get; }
    public int count { get; }
    public List<Task> subTasks { get; }
    //public List

    public Epic(int id,string title, string description, int chatId, string masterName, int masterId, string chatName, int count)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.masterName = masterName;
        this.masterId = masterId;
        this.chatId = chatId;
        this.chatName = chatName;
        this.count = count;
    }
    //public void ChangeColor(string colorCode)
    //{
    //    Transform statusTransform = TaskButton.transform.Find("Status");
    //    //colorCode = "#" + colorCode;
    //    ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
    //    statusTransform.GetComponent<Image>().color = newColor;
    //}
    public void PutInPanel(GameObject EpicPrefab, GameObject EpicPanel,bool master)
    {
        EpicButton = GameObject.Instantiate(EpicPrefab);
        EpicButton.transform.SetParent(EpicPanel.transform);

        Transform titleTransform = EpicButton.transform.Find("Title");
        titleTransform.GetComponent<TMP_Text>().text = title;
        
        if(!master)
        {
            Transform nameTransform = EpicButton.transform.Find("MasterName");
            nameTransform.GetComponent<TMP_Text>().text = "Тимлид:" + masterName;
        }

        Transform countTransform = EpicButton.transform.Find("Count");
        countTransform.GetComponent<TMP_Text>().text = "Количество исполнителей:" + count;

        //colorCode = "#" + colorCode;
        //ColorUtility.TryParseHtmlString(colorCode, out Color newColor);
        //statusTransform.GetComponent<Image>().color = newColor;

        //tasksByMaster[number].description;
    }
    public string Print()
    {
        string res = $"";
        return res;
    }
}
