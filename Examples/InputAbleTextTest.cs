using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using RText;
public class InputAbleTextTest : MonoBehaviour {
    [SerializeField]
    InputAbleText _text;
    [SerializeField]
    private InputField prefab;
    [SerializeField]
    InputAbleTextModifer _modifyer;
    const string RegexURL = "_*";

    void Start()
    {
        _text.InitEnviroment(prefab);
        _text.SetInputField(RegexURL, OnEndEdit);


        _modifyer.InitEnviroment(prefab);
        _modifyer.SetInputField(RegexURL, OnEndEdit);
    }

    private void OnEndEdit(int arg1, string arg2)
    {
        Debug.Log(arg1 + ": " + arg2);
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Update"))
        {
            _text.UpdateInputFields();
        }
    }
}
