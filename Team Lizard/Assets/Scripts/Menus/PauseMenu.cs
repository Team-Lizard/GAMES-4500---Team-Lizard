using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Menus
{
    public class PauseMenu : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("The player object that will be paused when in the pause menu.")]
        private FirstPersonController m_playerObject;

        [SerializeField]
        [Tooltip("The Pause Menu UI.")]
        private VisualTreeAsset m_menuUI;

        private PanelRenderer m_panelRenderer;
        // The PanelRenderer's root, used when swapping between pause and settings UI.
        private VisualElement m_root;
        private VisualElement m_pauseMenuContainer;

        private PlayerInput m_playerInput;
        private bool m_visible;
        private float m_prevTimeScale;

        private Button m_respawnButton;
        private Button m_restartButton;
        private Button m_settingsButton;

        /// <summary>
        /// Ran when the menu is first created;
        /// DON'T REPLACE THIS WITH START IF IT CAN BE AVOIDED; using Start for this code
        /// causes a lot of sillies with the ReloadScene calls when restarting the lvl
        /// </summary>
        private void Awake()
        {
            m_panelRenderer = GetComponent<PanelRenderer>();
            m_playerInput = m_playerObject.GetComponent<PlayerInput>();

            m_prevTimeScale = Time.timeScale;
            m_visible = false;

            m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        /// <summary>
        /// unregisters the UICallBack to avoid unnecessary resource allocation :P
        /// </summary>
        private void OnDestroy()
        {
            if (m_panelRenderer != null)
            {
                m_panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            }
        }

        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
        {
            m_root = root;
            m_pauseMenuContainer = root.Q<VisualElement>("PauseMenuContainer");

            if (m_pauseMenuContainer == null)
            {
                return;
            }

            m_respawnButton = root.Q<Button>("RespawnButton");
            m_respawnButton.clicked += RespawnPlayer;

            m_restartButton = root.Q<Button>("RestartButton");
            m_restartButton.clicked += RestartLevel;

            m_settingsButton = root.Q<Button>("SettingsButton");
            m_settingsButton.clicked += LoadSettingsMenu;

            SetVisibility(m_visible);
        }

        /// <summary>
        /// Pauses or unpauses the game when the pause action is performed.
        /// </summary>
        public void OnPause(InputAction.CallbackContext value)
        {
            // if we cancel the pause action for example, don't toggle pause
            if (!value.performed)
            {
                return;
            }
            TogglePauseMenu();
        }

        /// <summary>
        /// Sets the visibility of the menu and pauses/unpauses the game
        /// </summary>
        private void TogglePauseMenu()
        {
            m_visible = !m_visible;
            SetVisibility(m_visible);
            PauseUnpause(m_visible);
        }

        /// <summary>
        /// sets the visibility of the menu UI
        /// </summary>
        /// <param name="visible"> should the menu UI be made visible or not </param>
        private void SetVisibility(bool visible)
        {
            if (m_pauseMenuContainer != null)
            {
                m_pauseMenuContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// pauses or unpauses player input and overall game state
        /// </summary>
        /// <param name="visible"> should the game be paused or not (is the menu currently visible) </param>
        private void PauseUnpause(bool visible)
        {
            if (!visible)
            {
                Time.timeScale = m_prevTimeScale;
                m_playerInput?.ActivateInput();
            }
            else
            {
                m_prevTimeScale = Time.timeScale;
                Time.timeScale = 0;
                m_playerInput?.DeactivateInput();
            }
        }

        /// <summary>
        /// respawns the player... yea this is good
        /// </summary>
        private void RespawnPlayer()
        {
            if (m_playerObject == null)
            {
                return;
            }

            if (m_playerObject.TryGetComponent(out CheckpointManager checkpointManager))
            {
                checkpointManager.Respawn();
                TogglePauseMenu();
            }
        }

        /// <summary>
        /// Resets some pause data before reloading the scene
        /// </summary>
        private void RestartLevel()
        {
            m_visible = false;
            SetVisibility(false);
            Time.timeScale = 1f;
            m_playerInput?.ActivateInput();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void LoadPauseMenu(PanelRenderer panelRenderer, VisualElement root)
        {
            root.Clear();
            m_menuUI.CloneTree(root);
            OnUIReload(panelRenderer, root);
        }

        /// <summary>
        /// Swaps from the base pause menu to the settings menu
        /// </summary>
        private void LoadSettingsMenu()
        {
            m_root.Clear();
            GetComponent<SettingsMenu>().LoadMenuUI(m_panelRenderer, m_root, LoadPauseMenu);
        }
    }
}
