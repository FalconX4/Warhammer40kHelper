using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
	public static void ClearChilds(Transform transform, int leaveCount = 0)
	{
		for (int i = transform.childCount - 1; i >= leaveCount; i--)
			Object.Destroy(transform.GetChild(i).gameObject);

		List<Transform> childLeft = new List<Transform>();
		for (int i = 0; i < leaveCount; i++)
			childLeft.Add(transform.GetChild(i));
		transform.DetachChildren();
		for (int i = 0; i < leaveCount; i++)
			childLeft[i].SetParent(transform);
	}
}
