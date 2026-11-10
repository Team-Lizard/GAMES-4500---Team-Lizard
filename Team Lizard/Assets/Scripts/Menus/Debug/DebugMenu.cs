using Menus;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugMenu : MonoBehaviour, ISubMenu
{
    [SerializeField]
    [Tooltip("The UXML file containing the debug submenu")]
    private VisualTreeAsset m_menuUI;

    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }

    public void Build(VisualElement root)
    {
        // This'll be a separate PR, this one's already large enough lol
    }
}
