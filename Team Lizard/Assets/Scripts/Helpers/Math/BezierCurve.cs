using UnityEngine;

public static class BezierCurve
{

    public static Vector3 EvaluatePoint(Vector3 start, Vector3 end, Vector3 control, float t)
    {
        return (1f-t)*(1f-t)*start + 2*(1f-t)*t*control + t*t*end;
    }

    /// <summary>
    /// Evaluates derivative of bezier curve.
    /// </summary>
    /// <param name="start">Starting point of bezier curve</param>
    /// <param name="end">Ending point of bezier curve</param>
    /// <param name="control">Control point of bezier curve</param>
    /// <param name="t">Ranges from 0-1</param>
    /// <returns>Derivative of the bezier curve at point t.</returns>
    public static Vector3 EvaluateDerivative(Vector3 start, Vector3 end, Vector3 control, float t)
    {
        return  ((1f - t) * (control - start) + t * (end - control));
    }
}
