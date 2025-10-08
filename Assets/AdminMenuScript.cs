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
            loginMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "”кажите логин!";
            inputProblem = true;
        }
        if (passwordMenu.text == "")
        {
            passwordMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "¬ведите пароль!";
            inputProblem = true;
        }
        if (loginMenu.text == "")
        {
            emailMenu.placeholder.GetComponent<TextMeshProUGUI>().text = "”кажите почту!";
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
}
