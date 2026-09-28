using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(fileName = "VaultBehavior", menuName = "Scriptable Objects/VaultBehavior")]
public class VaultBehavior : ParkourBehavior
{   
    [SerializeField]
    private float m_obstacleClearance;

    // Documented in parent class. The vault is a curve OVER the object.
    public override Vector3 Evaluate(Vector3 start, Vector3 end, float obstacleHeight, float t)
    {
        Vector3 control = Vector3.Lerp(start, end, 0.5f) + Vector3.up * (obstacleHeight + m_obstacleClearance);

        return EvaluateBezierDerivative(start, end, control, t );
    }
}
