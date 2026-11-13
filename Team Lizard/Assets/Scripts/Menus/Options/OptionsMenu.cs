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
    private Behaviour m_playerObject;

    private FirstPersonController m_firstPersonController;
    private InputController m_inputController;

    private const string SPRINT_TOGGLE_TEXT = "Hold to sprint";
    private const string SLIDE_TOGGLE_TEXT = "Hold to slide";


    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }

    public void Build(VisualElement root)
    {
        Dictionary<string, Action<bool>> bindingLambdas = new Dictionary<string, Action<bool>>();
        bindingLambdas[SPRINT_TOGGLE_TEXT] = ToggleSprint;
        bindingLambdas[SLIDE_TOGGLE_TEXT] = ToggleSlide;

        m_firstPersonController = m_playerObject.GetComponent<FirstPersonController>();
        m_inputController = m_playerObject.GetComponent<InputController>();


        foreach (KeyValuePair<string, Action<bool>> binding in bindingLambdas)
        {
            TemplateContainer bindingElement = m_bindingUI.Instantiate();
            bindingElement.Q<Label>().text = binding.Key;

            Toggle toggle = bindingElement.Q<Toggle>();
            toggle.RegisterValueChangedCallback(change => binding.Value(change.newValue));
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
