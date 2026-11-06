using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Menus;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugMenu : MonoBehaviour, ISubMenu
{
    [SerializeField]
    [Tooltip("The UXML file containing the debug submenu")]
    private VisualTreeAsset m_menuUI;

    [SerializeField]
    [Tooltip("The player object associated with this scene")]
    private Behaviour m_playerObject;

    [SerializeField]
    [Tooltip("The UI template for a single debug binding")]
    private VisualTreeAsset m_bindingUI;

    private FirstPersonController m_firstPersonController;
    private InputController m_inputController;

    private List<DebugBinding> m_debugBindings;

    public VisualTreeAsset GetVisualTreeAsset()
    {
        return m_menuUI;
    }

    public void Build(VisualElement root)
    {
        m_firstPersonController = m_playerObject.GetComponent<FirstPersonController>();
        m_inputController = m_playerObject.GetComponent<InputController>();

        DebugData data = DebugData.GetDebugData(m_firstPersonController,  m_inputController);
        ScrollView list = root.Q<ScrollView>("DebugScrollView");

        list.Clear();
        m_debugBindings = new List<DebugBinding>();

        foreach (FieldInfo field in typeof(DebugData).GetFields(BindingFlags.Public |  BindingFlags.Instance))
        {
            DebugBinding binding = new DebugBinding(
                m_bindingUI,
                field.Name,
                (float)field.GetValue(data),
                _ => OnDebugDataChanged(DebugData.FromBindings(m_debugBindings)));

            m_debugBindings.Add(binding);
            list.Add(binding.Element);
        }
    }


    /// <summary>
    /// called whenever one of the debug values is changed within the menu
    /// </summary>
    /// <param name="data"> the values in the menu converted into a Debugdata instance </param>
    private void OnDebugDataChanged(DebugData data)
    {
        m_inputController.UpdateViaDebugData(data);
        m_firstPersonController.UpdateViaDebugData(data);
    }

    /// <summary>
    /// represents a single Binding between a float input and a variable
    /// holds the name, current value, and UI for the binding
    /// </summary>
    public class DebugBinding
    {
        public string Name { get; }
        public float Value { get; private set; }
        public TemplateContainer Element { get; }

        public DebugBinding(VisualTreeAsset template, string name, float value, Action<DebugBinding> onValueChanged)
        {
            Name = VarToNormal(name);
            Value = value;
            Element = template.Instantiate();

            Element.Q<Label>(className: "binding-label").text = Name;

            TextField input = Element.Q<TextField>(className: "binding-input");
            input.SetValueWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
            input.RegisterValueChangedCallback(change =>
            {
                if (!float.TryParse(change.newValue, out float newValue))
                {
                    input.value = Value.ToString();
                }

                Value = newValue;
                onValueChanged(this);
            });
        }

        /// <summary>
        /// takes a variable name like "m_moveSpeed" and converts it to something more readable like "Move Speed"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string VarToNormal(string s)
        {
            // strip the m_
            if (s.StartsWith("m_"))
            {
                s = s.Substring(2);
            }

            // capitalize the first letter
            s = s.FirstCharacterToUpper();

            string normalizedS = "";
            for (int i = 0; i < s.Length; i++)
            {
                // appending a space before each capital letter besides the first
                if (s[i] >= 65 && s[i] <= 90 && i != 0)
                {
                    normalizedS += " ";
                }

                // appending the character itself
                normalizedS += s[i];
            }

            return normalizedS;
        }
    }

    /// <summary>
    /// A silly lil data class storing everything in the debug menu
    /// </summary>
    public class DebugData
    {
        public float m_groundFriction;
        public float m_airFriction;
        public float m_moveSpeed;
        public float m_sprintMultiplier;
        public float m_jumpHeight;
        public float m_extraJumpHeight;
        public float m_lookSensitivity;

        /// <summary>
        /// Generates an instance with garbage data
        /// </summary>
        private DebugData()
        {
            m_groundFriction = 0;
            m_airFriction = 0;
            m_moveSpeed = 0;
            m_sprintMultiplier = 0;
            m_jumpHeight = 0;
            m_extraJumpHeight = 0;
            m_lookSensitivity = 0;
        }


        /// <summary>
        /// gets an instance based on the current values in the input and firstPerson controllers
        /// </summary>
        /// <param name="firstPersonController"> the player's FPController </param>
        /// <param name="inputController"> the player's inputController </param>
        /// <returns></returns>
        public static DebugData GetDebugData(FirstPersonController firstPersonController, InputController inputController)
        {
            DebugData toSend = new DebugData();

            firstPersonController.UpdateDebugData(toSend);
            inputController.UpdateDebugData(toSend);

            return toSend;
        }

        /// <summary>
        /// takes a list of Bindings and generates a DebugData instance from it
        /// </summary>
        /// <param name="bindings"> read summary </param>
        /// <returns> read summary </returns>
        public static DebugData FromBindings(List<DebugBinding> bindings)
        {
            DebugData toSend = new DebugData();

            foreach (DebugBinding binding in bindings)
            {
                FieldInfo field = typeof(DebugData).GetField(binding.Name, BindingFlags.Public | BindingFlags.Instance);
                field?.SetValue(toSend, binding.Value);
            }

            return toSend;
        }
    }
}
