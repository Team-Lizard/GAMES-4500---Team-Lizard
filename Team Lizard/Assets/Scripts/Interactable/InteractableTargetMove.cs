using UnityEngine;

namespace Interactable
{
    public class InteractableTargetMove : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Total movement of object in local space.")]
        [SerializeField]
        private Vector3 m_movementAmount;

        [Tooltip("Position of control point along line in local space.")]
        [SerializeField]
        private Vector3 m_curveAmount;

        [Tooltip("Time it should take for object to get from one point to another.")]
        [SerializeField]
        private float m_transitTime;

        private Vector3 m_initialPosition;
        private Vector3 m_finalPosition;
        private Vector3 m_controlPosition;
        private float m_currentTime;
        private bool m_isMoving = false;

        void Awake() {
            m_initialPosition = transform.position;
            m_finalPosition = m_initialPosition + m_movementAmount;
            m_controlPosition = m_initialPosition + m_curveAmount;
            
            m_currentTime = 0;
        }
        public void OnActivate()
        {
            m_isMoving = true;
        }

        // FixedUpdate is called once per physics frame
        private void FixedUpdate()
        {            
            if (m_isMoving)
            {
                m_currentTime += Time.deltaTime;
                
                // Move between endpoints
                float scaledT = m_currentTime / m_transitTime;

                transform.position = BezierCurve.EvaluatePoint(m_initialPosition, m_finalPosition, m_controlPosition, scaledT);
                
                if (m_currentTime > m_transitTime)
                {
                    m_currentTime = 0;
                    m_isMoving = false;
                }
            }
        }
    }
}