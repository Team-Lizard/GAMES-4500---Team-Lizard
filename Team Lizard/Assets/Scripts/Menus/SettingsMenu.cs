using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Menus
{
    public class SettingsMenu: MonoBehaviour
    {

        [SerializeField]
        [Tooltip("The menu UI that will be loaded when pulling up the settings menu.")]
        private VisualTreeAsset m_menuUI;

        /// <summary>
        /// Stores script components containing the logic for each submenu
        /// </summary>
        private MonoBehaviour m_inputBindingsMenu;
        private MonoBehaviour m_debugMenu;
        private MonoBehaviour m_optionsMenu;

        private PanelRenderer m_panelRenderer;
        private VisualElement m_submenuContainer;

        /// <summary>
        /// Called when the object is first loaded
        /// </summary>
        private void Awake()
        {
            m_panelRenderer = GetComponent<PanelRenderer>();
            m_inputBindingsMenu = GetComponent<InputBindingsMenu>();
            m_debugMenu = GetComponent<DebugMenu>();
            m_optionsMenu = GetComponent<OptionsMenu>();

            m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        /// <summary>
        /// called whenever the UI is reloaded, sets up button callbacks to load submenus
        /// </summary>
        /// <param name="panelRenderer"> the MonoBehaviour containing this script </param>
        /// <param name="root"> this UI's root node </param>
        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
        {
            m_submenuContainer = root.Q<VisualElement>("SubmenuContainer");

            // if the submenu container doesn't exist, then the settings menu UI isn't active
            if (m_submenuContainer == null)
            {
                return;
            }

            root.Q<Button>("BindingMenuButton").clicked +=
                () => ShowSubmenu(m_inputBindingsMenu as ISubMenu);

            root.Q<Button>("OptionsMenuButton").clicked +=
                () => ShowSubmenu(m_optionsMenu as ISubMenu);

            root.Q<Button>("DebugMenuButton").clicked +=
                () => ShowSubmenu(m_debugMenu as ISubMenu);

            ShowSubmenu(m_inputBindingsMenu as ISubMenu);
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

        public void LoadMenuUI(PanelRenderer panelRenderer, VisualElement root, Action<PanelRenderer, VisualElement> backButtonCallback)
        {
            m_panelRenderer = panelRenderer;
            m_menuUI.CloneTree(root);

            OnUIReload(panelRenderer, root);

            root.Q<Button>("BackButton").clicked +=
                () => backButtonCallback(panelRenderer, root);
        }
    }
}
