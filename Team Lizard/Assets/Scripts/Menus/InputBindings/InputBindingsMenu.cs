using Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class InputBindingsMenu : MonoBehaviour, ISubMenu
{
    [SerializeField]
    [Tooltip("The UXML file containing the input bindings submenu")]
    private VisualTreeAsset m_menuUI;

    [SerializeField]
    [Tooltip("Asset containing all InputActions (player run, player jump, pause, etc)")]
    private InputActionAsset m_inputActionAsset;

    [SerializeField]
    [Tooltip("The UI for a single input binding (likely containing the name of the input and some way to change it)")]
    private VisualTreeAsset m_bindingUI;

    /// <summary>
    /// ran when this submenu is first loaded
    /// </summary>
    private void Awake()
    {
        if (PlayerPrefs.HasKey("rebinds"))
        {
            m_inputActionAsset.LoadBindingOverridesFromJson(
                PlayerPrefs.GetString("rebinds")
            );
        }
    }


    public void Build(VisualElement root)
    {
        SyncBindings();

        ScrollView list = root.Q<ScrollView>("BindingScrollView");
        list.Clear();

        // adding every single input across all InputActionMaps to a single menu.
        // this way, if the input set ever changes or if we have different maps for gameplay, menus, etc.
        // every input will show regardless
        foreach (InputActionMap map in m_inputActionAsset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {

                // making a new BindingUI, updating the information it shows, and displays it
                TemplateContainer bindingElement = m_bindingUI.Instantiate();
                bindingElement.AddToClassList("input-binding");

                bindingElement.name = action.name;
                bindingElement.Q<Label>().text = action.name;

                Button button = bindingElement.Q<Button>();
                button.clicked += Onclicked;

                // when clicking a binding button, we'll show the screen to rebind the input; the screen
                // will be responsible for overriding the current binding and destroying itself
                void Onclicked()
                {
                    button.text = "Waiting for new input:";
                    action.Disable();

                    action.PerformInteractiveRebinding()
                        .OnComplete(operation =>
                        {
                            operation.Dispose();
                            action.Enable();
                            button.text = "Rebind";
                            SyncBindings();
                        })
                        .Start();
                }

                list.Add(bindingElement);
            }
        }
    }

    /// <summary>
    /// Modifying input bindings through unity's input manager updates the PlayerInput instance, but because a player
    /// instance creates a copy of the input bindings, those bindings will become stale. This aims to sync the two.
    ///
    /// This function will also save the input bindings ot the playerPrefs, which should allow us to save between
    /// closing and re-opening the game
    /// </summary>
    private void SyncBindings()
    {
        // getting the input bindings
        string bindingOverrides = m_inputActionAsset.SaveBindingOverridesAsJson();

        // loops over all instances of the player class (idk why there'd be more than one, but for sake of avoiding weird multi-level shenanigans)
        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            // replaces the bindings currently on the player with the updated ones
            playerInput.actions.RemoveAllBindingOverrides();
            playerInput.actions.LoadBindingOverridesFromJson(bindingOverrides);
        }

        // saving the bindings to PlayerPrefs
        PlayerPrefs.SetString("rebinds", bindingOverrides);
        PlayerPrefs.Save();
    }


    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }
}
