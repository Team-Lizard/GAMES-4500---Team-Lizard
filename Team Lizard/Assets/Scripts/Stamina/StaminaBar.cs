using UnityEngine;
using UnityEngine.UIElements;

public class StaminaBar : MonoBehaviour
{
    private PanelRenderer m_panelRenderer;
    private ProgressBar m_staminaBar;
    private Stamina.Stamina m_stamina;

    /// <summary>
    /// Called when this is first created; sets up some basic wiring needed to update the UI
    /// </summary>
    private void Awake()
    {
        m_panelRenderer = GetComponent<PanelRenderer>();
        m_panelRenderer.RegisterUIReloadCallback(OnUIReload);

        m_stamina = Stamina.Stamina.Instance();
        m_stamina.StaminaChanged += UpdateBar;
    }

    /// <summary>
    /// Called when this is destroyed; really only here to prevent memory leaks
    /// </summary>
    private void OnDestroy()
    {
        if (m_panelRenderer != null)
        {
            m_panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        if (m_stamina != null)
        {
            m_stamina.StaminaChanged -= UpdateBar;
        }
    }

    /// <summary>
    /// Called whenever the UI is reloaded to prevent any sort of desync
    /// </summary>
    /// <param name="panelRenderer"> The panelRenderer object representing this UI </param>
    /// <param name="root"> the root element of the panelRenderer's UI tree </param>
    private void OnUIReload(
        PanelRenderer panelRenderer,
        VisualElement root)
    {
        m_staminaBar = root.Q<ProgressBar>("StaminaBar");

        // GetCurrentStaminaPercent returns a value between 0 and 100.
        m_staminaBar.lowValue = 0f;
        m_staminaBar.highValue = 100f;
        UpdateBar(Stamina.Stamina.GetCurrentStaminaPercent());
    }

    private void UpdateBar(float value)
    {
        if (m_staminaBar != null)
        {
            m_staminaBar.value = value;
        }
    }
}
