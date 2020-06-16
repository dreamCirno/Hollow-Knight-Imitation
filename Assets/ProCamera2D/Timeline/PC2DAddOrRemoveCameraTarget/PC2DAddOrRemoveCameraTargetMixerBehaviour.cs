#if UNITY_2017_1_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Com.LuisPedroFonseca.ProCamera2D;

public class PC2DAddOrRemoveCameraTargetMixerBehaviour : PlayableBehaviour
{
	private float _currentTime;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		int inputCount = playable.GetInputCount();

		var rootPlayable = playable.GetGraph().GetRootPlayable(0);

		float time = (float)rootPlayable.GetTime();

		for (int i = 0; i < inputCount; i++)
		{
			var inputPlayable = (ScriptPlayable<PC2DAddOrRemoveCameraTargetBehaviour>)playable.GetInput(i);
			var input = inputPlayable.GetBehaviour();

			if (Application.isPlaying &&
				(_currentTime <= input.ClipStartTime) && (time > input.ClipStartTime))
			{
				if (input.action == PC2DAddOrRemoveCameraTarget.Add)
					ProCamera2D.Instance.AddCameraTarget(input.cameraTarget);
				else
					ProCamera2D.Instance.RemoveCameraTarget(input.cameraTarget);
			}
		}

		_currentTime = time;
	}
}
#endif