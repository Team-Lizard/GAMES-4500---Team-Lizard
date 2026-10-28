using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Menus
{
    public class PauseMenu: MonoBehaviour
    {
        /// <summary>
        /// Stores script components containing the logic for each submenu
        /// </summary>
        private MonoBehaviour m_inputBindingsMenu;
        private MonoBehaviour m_debugMenu;

        private PanelRenderer m_panelRenderer;
        private VisualElement m_menuContainer;
        private VisualElement m_submenuContainer;
        private bool m_visible;

        /// <summary>
        /// Called when the object is first loaded
        /// </summary>
        private void Start()
        {
            m_panelRenderer = GetComponent<PanelRenderer>();
            m_inputBindingsMenu = GetComponent<InputBindingsMenu>();
            m_debugMenu = GetComponent<DebugMenu>();

            m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
            m_visible = false;
        }

        /// <summary>
        /// called whenever the UI is reloaded, sets up button callbacks to load submenus
        /// </summary>
        /// <param name="panelRenderer"> the MonoBehaviour containing this script </param>
        /// <param name="root"> this UI's root node </param>
        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
        {
            m_menuContainer = root.Q<VisualElement>("MenuContainer");
            m_submenuContainer = root.Q<VisualElement>("SubmenuContainer");

            root.Q<Button>("BindingMenuButton").clicked +=
                () => ShowSubmenu(m_inputBindingsMenu as ISubMenu);

            root.Q<Button>("DebugMenuButton").clicked +=
                () => ShowSubmenu(m_debugMenu as ISubMenu);

            ShowSubmenu(m_inputBindingsMenu as ISubMenu);

            // making sure that the menu is disabled whenever it's first loaded
            SetVisibility(false);
        }


        /// <summary>
        /// Sets up the submenu UI
        /// </summary>
        /// <param name="submenu"> the submenu script used to set up the submenu </param>
        private void ShowSubmenu(ISubMenu submenu)
        {
            m_submenuContainer.Clear();

            if (submenu == null)
            {
                Debug.LogError("PauseMenu submenu is not assigned or does not implement ISubMenu.");
                return;
            }

            VisualTreeAsset submenuAsset = submenu.GetVisualTreeAsset();
            if (submenuAsset == null)
            {
                Debug.LogError("PauseMenu submenu does not have a UXML asset assigned.");
                return;
            }

            TemplateContainer submenuRoot = submenuAsset.Instantiate();
            m_submenuContainer.Add(submenuRoot);

            // any UI elements loaded via C# code should be in the Build function, so this sets up the submenu
            submenu.Build(submenuRoot);

        }

        /// <summary>
        /// pauses / unpauses the game
        /// </summary>
        /// <param name="value"> stores the pause key press </param>
        public void OnPause(InputAction.CallbackContext value)
        {
            ToggleVisibility();
        }

        /// <summary>
        /// toggles the visibility var and sets the visibility respectively
        /// </summary>
        private void ToggleVisibility()
        {
            m_visible = !m_visible;
            SetVisibility(m_visible);
        }


        /// <summary>
        /// turns on/off the menu
        /// </summary>
        /// <param name="visible"> whether the menu should be visible </param>
        private void SetVisibility(bool visible)
        {
            if (m_menuContainer != null)
            {
                m_menuContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
