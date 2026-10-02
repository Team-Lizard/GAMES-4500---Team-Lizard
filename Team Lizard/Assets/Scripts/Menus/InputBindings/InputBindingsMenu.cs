using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputBindingsMenu : MonoBehaviour
{

    [SerializeField] private InputActionAsset m_inputActionAsset;
    [SerializeField] private VisualTreeAsset m_bindingUI;

    private PanelRenderer m_panelRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_panelRenderer = GetComponent<PanelRenderer>();
        m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
    {
        ScrollView list = root.Q<ScrollView>("BindingScrollView");

        foreach (InputActionMap map in m_inputActionAsset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                TemplateContainer binding = m_bindingUI.Instantiate();

                binding.name = action.name;
                binding.Q<Label>().text = action.name;

                list.Add(binding);

            }
        }
    }


}
