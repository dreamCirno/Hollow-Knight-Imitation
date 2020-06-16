#if UNITY_2017_1_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Com.LuisPedroFonseca.ProCamera2D;

public enum PC2DAddOrRemoveCameraTarget
{
	Add,
	Remove
}

[Serializable]
public class PC2DAddOrRemoveCameraTargetBehaviour : PlayableBehaviour
{
	public Transform cameraTarget;
	public PC2DAddOrRemoveCameraTarget action;

	public TimelineClip ClipReference { get; set; }
	public float ClipStartTime { get { return (float)ClipReference.start; } }

	public override void OnGraphStart(Playable playable)
	{
		ClipReference.displayName = action == PC2DAddOrRemoveCameraTarget.Add ? "Add Camera Target" : "Remove Camera Target";
		base.OnGraphStart(playable);
	}
}
#endif