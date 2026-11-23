using System;
using System.Collections.Generic;
using Menus;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsMenu : MonoBehaviour, ISubMenu
{
    [SerializeField]
    [Tooltip("The UXML file containing the options submenu")]
    private VisualTreeAsset m_menuUI;

    [SerializeField]
    [Tooltip("The UXML file containing a single options binding")]
    private VisualTreeAsset m_bindingUI;

    [SerializeField]
    [Tooltip("The player object associated with this scene")]
    private MonoBehaviour m_playerObject;

    private InputController m_inputController;

    private const string SPRINT_TOGGLE_TEXT = "Hold to sprint";
    private const string SLIDE_TOGGLE_TEXT = "Hold to slide";

    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }

    public void Build(VisualElement root)
    {
        List<OptionData> options = new List<OptionData>();
        options.Add(new OptionData(SPRINT_TOGGLE_TEXT, ToggleSprint, "SprintToggle"));
        options.Add(new OptionData(SLIDE_TOGGLE_TEXT, ToggleSlide, "SlideToggle"));

        m_inputController = m_playerObject.GetComponent<InputController>();

        foreach (OptionData data in options)
        {
            TemplateContainer bindingElement = m_bindingUI.Instantiate();
            bindingElement.Q<Label>().text = data.name;

            Toggle toggle = bindingElement.Q<Toggle>();
            toggle.RegisterValueChangedCallback(change => data.onActivate(change.newValue));

            // this deals with bools, idk if we want a different loop or even brute force to generate non-toggles
            // notice the == 0; this is because we want the toggle to be true if we want to hold (if toggle is false)
            bindingElement.Q<Toggle>().value = PlayerPrefs.GetInt(data.playerPrefName) == 0;

            root.Add(bindingElement);
        }
    }

    private void ToggleSprint(bool holdToSprint)
    {
        m_inputController.SetSprintToggleMode(!holdToSprint);
    }

    private void ToggleSlide(bool holdToSlide)
    {
        m_inputController.SetSlideToggleMode(!holdToSlide);
    }

}

public class OptionData
{

    public string name;
    public Action<bool> onActivate;
    public string playerPrefName;
    public OptionData(string name, Action<bool> onActivate, string playerPrefName)
    {
        this.name = name;
        this.onActivate = onActivate;
        this.playerPrefName = playerPrefName;
    }
}
