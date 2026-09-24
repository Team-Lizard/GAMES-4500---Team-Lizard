using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "ParkourBehavior", menuName = "Scriptable Objects/ParkourBehavior")]
public class ParkourBehavior : ScriptableObject
{
    [Tooltip("Exact string name of the animation we'd like to play.")]
    public AnimationCurve AnimationCurve;
    
    [SerializeField]
    [Tooltip("Minimum height that makes this parkour behavior possible.")]
    private float m_minHeight;
    [SerializeField]
    [Tooltip("Maximum height that makes this parkour behavior possible.")]
    private float m_maxHeight;

    public virtual bool IsParkourActionPossible(HitInfo hitInfo, Transform player)
    {
        if (hitInfo.obstacleHeight < m_minHeight || hitInfo.obstacleHeight > m_maxHeight)
        {
            return false;
        }
        return true;
    }
}
