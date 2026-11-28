using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Responsible for updating the stamina values on regen as well as displaying it to the UI
/// </summary>
public class StaminaBar : MonoBehaviour
{
    private PanelRenderer m_panelRenderer;
    private ProgressBar m_staminaBar;
    private Stamina m_stamina;

    private float m_timeSinceSpending;

    [SerializeField]
    [Tooltip("Time to wait before regenerating stamina.")]
    private float m_regenThresholdTime = 1f;

    [SerializeField]
    [Tooltip("Rate at which stamina should regenerate in stamina/sec.")]
    private float m_regenRate = 50f;

    /// <summary>
    /// Called when this is first created; sets up some basic wiring needed to update the UI
    /// </summary>
    private void Awake()
    {
        m_panelRenderer = GetComponent<PanelRenderer>();
        m_panelRenderer.RegisterUIReloadCallback(OnUIReload);

        m_stamina = Stamina.Instance();
        m_stamina.StaminaGained += UpdateBar;
        m_stamina.StaminaSpent += OnSpend;
        m_timeSinceSpending = 0f;
    }

    void Update()
    {
        if (m_staminaBar != null && m_timeSinceSpending >= m_regenThresholdTime)
        {
            m_stamina.RegenerateStamina(Time.deltaTime * m_regenRate);
        }
        
        m_timeSinceSpending += Time.deltaTime;
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
            m_stamina.StaminaSpent -= OnSpend;
            m_stamina.StaminaGained -= UpdateBar;
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
        UpdateBar(Stamina.GetCurrentStaminaPercent());
    }

    private void OnSpend(float value)
    {
        m_timeSinceSpending = 0;
        UpdateBar(value);
    }

    private void UpdateBar(float value)
    {
        if (m_staminaBar != null)
        {
            m_staminaBar.value = value;
        }
    }
}
