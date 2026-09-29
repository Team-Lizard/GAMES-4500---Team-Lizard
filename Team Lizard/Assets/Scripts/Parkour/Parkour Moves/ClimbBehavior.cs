using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "ClimbBehavior", menuName = "Scriptable Objects/ClimbBehavior")]
public class ClimbBehavior : ParkourBehavior
{   
    [SerializeField]
    private float m_obstacleClearance;

    // Documented in parent. Climbing is a curve ONTO the object.
    public override Vector3 Evaluate(Vector3 start, Vector3 end, float obstacleHeight, float t)
    {
        end.y = obstacleHeight;
        Vector3 control = Vector3.Lerp(start, end, 0.5f);
        control.y = obstacleHeight + m_obstacleClearance;

        return EvaluateBezierDerivative(start, end, control, t);
    }
}
