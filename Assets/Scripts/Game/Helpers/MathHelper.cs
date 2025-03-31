using UnityEngine;

public static class MathHelper 
{
    // <summary>
    // Checks if two Vector3 instances are approximately equal within a specified tolerance.
    // </summary>
    // <param name="a">The first Vector3 to compare.</param>
    // <param name="b">The second Vector3 to compare.</param>
    // <param name="tolerance">The maximum allowable difference for each component (default is 0.4f).</param>
    public static bool AreVectorApproximatelyEqual (Vector3 a, Vector3 b, float tolerance = 0.4f)
    {   
        // calculate difference in each component then check
        return Mathf.Abs(a.x - b.x) < tolerance && Mathf.Abs(a.y - b.y) < tolerance && Mathf.Abs(a.z - b.z) < tolerance;
    }

    // <summary>
    // Checks if two Quaternions are approximately equal within a specified tolerance.
    // </summary>
    // <param name="q1">The first Quaternion to compare.</param>
    // <param name="q2">The second Quaternion to compare.</param>
    // <param name="tolerance">The maximum allowable difference for each component (default is 0.004f).</param>
    // <returns>True if the quaternions are approximately equal; otherwise, false.</returns>
    public static bool AreQuaternionsApproximatelyEqual (Quaternion q1, Quaternion q2, float tolerance = 0.004f)
    {
        // Calculate the difference in each component
        float wDiff = Mathf.Abs(q1.w - q2.w);
        float xDiff = Mathf.Abs(q1.x - q2.x);
        float yDiff = Mathf.Abs(q1.y - q2.y);
        float zDiff = Mathf.Abs(q1.z - q2.z);
        
        // Check if all differences are within the specified epsilon
        return wDiff < tolerance && xDiff < tolerance && yDiff < tolerance && zDiff < tolerance;
    }
}
