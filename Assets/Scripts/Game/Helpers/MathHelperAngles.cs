using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MathHelperAngles
{
    // Normalize the angle to be between -180 and 180 degrees
    public static float NormalizeAngle (float angle)
    {
        angle = angle % 360; // Keep the angle between 0 and 360
        if (angle > 180) angle -= 360; // Convert to -180 to 180 range

        return angle;
    }
}
