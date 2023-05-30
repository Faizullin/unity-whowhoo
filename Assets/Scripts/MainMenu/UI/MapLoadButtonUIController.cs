using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MapLoadButtonUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_editButton;
    [SerializeField]
    private GameObject m_deleteButton;
    [SerializeField]
    private GameObject m_openButton;
    [SerializeField]
    private GameObject m_openHostButton;
    [SerializeField]
    private GameObject m_title;

    public void Init(string path, Action<string> editCallback, Action<string> deleteCallback, Action<string, bool> openCallback)
    {
        m_deleteButton.GetComponent<Button>().onClick.AddListener(delegate { deleteCallback(path); });
        m_editButton.GetComponent<Button>().onClick.AddListener(delegate { editCallback(path); });
        m_openButton.GetComponent<Button>().onClick.AddListener(delegate { openCallback(path, false); });
        m_openHostButton.GetComponent<Button>().onClick.AddListener(delegate { openCallback(path, true); });
        string text = "Map: " + path;
        text = text.Length > 14 ? text.Substring(0, 14) + "..." : text;
        m_title.GetComponent<TextMeshProUGUI>().text = text;
    }
}
