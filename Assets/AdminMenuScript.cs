using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminMenuScript : MonoBehaviour
{
    [SerializeField] TMP_InputField loginMenu;
    [SerializeField] TMP_InputField passwordMenu;
    [SerializeField] TMP_InputField emailMenu;
    [SerializeField] TMP_Dropdown roleMenu;
    [SerializeField] MenuScript menuScript;

    public void SignUp()
    {

        bool inputProblem = false;
        if (loginMenu.text == "")
        {
            loginMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� �����!";
            inputProblem = true;
        }
        if (passwordMenu.text == "")
        {
            passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� ������!";
            inputProblem = true;
        }
        if (loginMenu.text == "")
        {
            emailMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "������� �����!";
            inputProblem = true;
        }
        int roleId = roleMenu.value+1;
        if (!inputProblem)
        {
            menuScript.db.AddUser(loginMenu.text, emailMenu.text, passwordMenu.text, roleId);
            loginMenu.text = "";
            emailMenu.text = "";
            passwordMenu.text = "";

        }
    }
    public void OpenAdminChat()
    {
        if (ChatScript.Instance != null)
        {
            int myId = menuScript.activeUserId;
            
            ChatScript.Instance.OpenChat(0, myId);
        }
        else
        {
            Debug.LogError("ChatScript не найден!");
        }
        
    }
}
