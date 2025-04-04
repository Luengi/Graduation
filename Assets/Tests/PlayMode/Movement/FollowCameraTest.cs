using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

public class FollowCameraTest
{
	// Note: If this test fails, the mocked time might be too short. Try increasing the simulatedTime variable.
    [UnityTest]
	public IEnumerator PlayMode_FollowCameraTest_Follow()
	{
		FollowConfiguration followConfiguration = new FollowConfiguration();
		followConfiguration._target = new GameObject().transform;
		followConfiguration._speed = 5f;
		followConfiguration._offset = new Vector3(0, 0, 0);
		followConfiguration._lookAtTarget = false;
		followConfiguration._distance = 1;

		FollowCamera followCamera = new GameObject().AddComponent<FollowCamera>();
		followCamera._followConfiguration = followConfiguration;

		followConfiguration.Target.position = new Vector3(3, 0, 4);

		// Mock passage of time by manually calling LateUpdate
        float simulatedTime = 40f;
		float fixedDeltaTime = 1f / 60f; // Fixed deltaTime for each iteration (60 FPS equivalent)
        int steps = Mathf.CeilToInt(simulatedTime / fixedDeltaTime);

        for (int i = 0; i < steps; i++) // Around 2500 frame updates
        {
            followCamera.LateUpdate();
        }
		
        Vector3 expectedPosition = followConfiguration.Target.position + followConfiguration.Target.forward * followConfiguration.Distance + followConfiguration.Offset;
        Assert.IsTrue(MathHelper.AreVectorApproximatelyEqual(followCamera.transform.position, expectedPosition), $"Expected: {expectedPosition}, But was: {followCamera.transform.position}");
		
		yield return null;
	}
}
