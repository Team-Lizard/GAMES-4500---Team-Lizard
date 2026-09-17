using UnityEngine;

namespace Stamina
{
    public class Stamina : ScriptableObject
    {
        private static Stamina m_instance;
        private float m_maxStamina;
        private float m_currentStamina;

        /// <summary>
        /// Called once when the component is first loaded; sets itself to the singleton instance if no other stamina
        /// bar is set, then initializes values
        /// </summary>
        void Awake()
        {
            m_instance = Instance();
            m_maxStamina = 100;
            m_currentStamina = m_maxStamina;
        }

        /// <summary>
        /// Attempts to spend a given amount of stamina
        /// </summary>
        /// <param name="amount"> the amount being spent; the 'cost' of an action </param>
        /// <returns> true if there is enough stamina to spend the specified amount, false otherwise </returns>
        public static bool TrySpendStamina(float amount)
        {
            if (Instance().m_currentStamina < amount)
            {
                return false;
            }

            Instance().m_currentStamina -= amount;
            return true;
        }

        /// <summary>
        /// gets the current amount of stamina left as a percentage
        /// </summary>
        /// <returns> the current amount of stamina left as a percentage </returns>
        public static float GetCurrentStaminaPercent()
        {
            return Instance().m_currentStamina / Instance().m_maxStamina;
        }

        /// <summary>
        /// Accesses the singleton instance of the Stamina class used in the above functions
        /// </summary>
        /// <returns> the singleton instance of the Stamina class </returns>
        public static Stamina Instance()
        {
            if (m_instance != null)
            {
                return m_instance;
            }

            m_instance = ScriptableObject.CreateInstance<Stamina>();
            return m_instance;
        }
    }
}
