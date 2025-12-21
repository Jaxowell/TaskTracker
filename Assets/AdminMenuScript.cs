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

        int roleId = roleMenu.value + 1;

        if (!inputProblem)
        {
            // Используем сетевой метод
            StartCoroutine(menuScript.db.AddUserWeb(loginMenu.text, emailMenu.text, passwordMenu.text, roleId));
            
            loginMenu.text = "";
            emailMenu.text = "";
            passwordMenu.text = "";
        }
    }
}