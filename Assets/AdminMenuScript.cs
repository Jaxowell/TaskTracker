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
        if (string.IsNullOrEmpty(loginMenu.text)) inputProblem = true;
        if (string.IsNullOrEmpty(passwordMenu.text)) inputProblem = true;
        if (string.IsNullOrEmpty(emailMenu.text)) inputProblem = true;

        int roleId = roleMenu.value + 1; // 0->1 (Admin), 1->2 (Master)...

        if (!inputProblem)
        {
            // ЗАПУСКАЕМ КОРУТИНУ
            StartCoroutine(menuScript.db.AddUserWeb(loginMenu.text, emailMenu.text, passwordMenu.text, roleId));
            
            // Очистка
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
    }
}