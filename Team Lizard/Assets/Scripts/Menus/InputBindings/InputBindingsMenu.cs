using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputBindingsMenu : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Asset containing all InputActions (player run, player jump, pause, etc)")]    InputActionAsset m_inputActionAsset;

    [SerializeField]
    [Tooltip("The UI for a single input binding (likely containing the name of the input and some way to change it)")]
    private VisualTreeAsset m_bindingUI;

    /// <summary>
    /// the PanelRenderer object the script is attached to
    /// </summary>
    private PanelRenderer m_panelRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_panelRenderer = GetComponent<PanelRenderer>();
        m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    /// <summary>
    /// Called whenever the UI is reloaded to prevent any sort of desync
    /// </summary>
    /// <param name="panelRenderer"> The panelRenderer object representing this UI </param>
    /// <param name="root"> the root element of the panelRenderer's UI tree </param>
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
    {
        ScrollView list = root.Q<ScrollView>("BindingScrollView");

        // adding every single input across all InputActionMaps to a single menu.
        // this way, if the input set ever changes or if we have different maps for gameplay, menus, etc.
        // every input will show regardless
        foreach (InputActionMap map in m_inputActionAsset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                // making a new BindingUI, updating the information it shows, and displays it
                TemplateContainer binding = m_bindingUI.Instantiate();

                binding.name = action.name;
                binding.Q<Label>().text = action.name;

                list.Add(binding);
            }
        }

        if (panelRenderer.enabled)
        {
            ToggleVisibility();
        }
    }

    public void OnPause(InputAction.CallbackContext value)
    {
        ToggleVisibility();
    }

    private void ToggleVisibility()
    {
        m_panelRenderer.enabled = !m_panelRenderer.enabled;
    }
}
