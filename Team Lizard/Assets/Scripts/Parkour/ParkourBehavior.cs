using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "ParkourBehavior", menuName = "Scriptable Objects/ParkourBehavior")]
public abstract class ParkourBehavior : ScriptableObject
{   
    [SerializeField]
    [Tooltip("Minimum height that makes this parkour behavior possible.")]
    protected float m_minHeight;
    [SerializeField]
    [Tooltip("Maximum height that makes this parkour behavior possible.")]
    protected float m_maxHeight;

    [SerializeField]
    [Tooltip("Time that parkour action should take")]
    public float AnimationLength;

    [SerializeField]
    [Tooltip("Distance player should move in the forward direction.")]
    public float HorizontalDistance;

    /// <summary>
    /// Checks whether this parkour action can be performed.
    /// </summary>
    /// <param name="hitInfo">Object that was hit by raycast.</param>
    /// <returns>True if this parkour action is valid for the given object.</returns>
    public virtual bool IsParkourActionPossible(HitInfo hitInfo)
    {
        return !(hitInfo.ObstacleHeight < m_minHeight || hitInfo.ObstacleHeight > m_maxHeight);
    }

    /// <summary>
    /// Evaluate the direction and magnitude of the animation curve at point t.
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">Ending position</param>
    /// <param name="obstacleHeight">Height that needs to be scaled</param>
    /// <param name="t">Ranges from 0-1, where 0 is the beginning of the animation (at start pos) and 1 is the end of the animation (at end pos).</param>
    /// <returns>Direction and magnitude vector to be applied as player movement.</returns>
    public abstract Vector3 Evaluate(Vector3 start, Vector3 end, float obstacleHeight, float t);

    /// <summary>
    /// Evaluates derivative of bezier curve.
    /// </summary>
    /// <param name="start">Starting point of bezier curve</param>
    /// <param name="end">Ending point of bezier curve</param>
    /// <param name="control">Control point of bezier curve</param>
    /// <param name="t">Ranges from 0-1</param>
    /// <returns>Derivative of the bezier curve at point t.</returns>
    protected Vector3 EvaluateBezierDerivative(Vector3 start, Vector3 end, Vector3 control, float t)
    {
        // Scale t to animation length
        t /= AnimationLength;

        return  ((1f - t) * (control - start) + t * (end - control)) / AnimationLength;
    }
}
