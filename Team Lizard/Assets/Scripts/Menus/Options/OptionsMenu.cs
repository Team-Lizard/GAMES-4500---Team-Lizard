using System.Collections.Generic;
using System.Reflection;
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

    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }

    public void Build(VisualElement root)
    {
        m_firstPersonController = m_playerObject.GetComponent<FirstPersonController>();
        m_inputController = m_playerObject.GetComponent<InputController>();

    }

}
