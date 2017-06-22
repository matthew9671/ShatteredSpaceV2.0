using UnityEngine;
using System.Collections.Generic;

public class SplineDecorator : MonoBehaviour {

	public BezierSpline spline;

	public int frequency;

	public bool lookForward;

	public Transform[] originalItems;
	public List<Transform> items = new List<Transform>();

	private bool initialized = false;

	public void decorate()
	// Put the objects in the right positions according to the bezier curve
	{
		if (frequency <= 0 || originalItems == null || originalItems.Length == 0) {
			return;
		}
		float stepSize = frequency * originalItems.Length;
		if (spline.Loop || stepSize == 1) {
			stepSize = 1f / stepSize;
		}
		else {
			stepSize = 1f / (stepSize - 1);
		}
		for (int p = 0, f = 0; f < frequency; f++) {
			for (int i = 0; i < originalItems.Length; i++, p++) {
				Transform item;
				if (!initialized)
				{
					item = Instantiate(originalItems[i]) as Transform;
					items.Add(item);
				}
				else
				{
					item = items[p];
				}
				Vector3 position = spline.GetPoint(p * stepSize);
				item.transform.localPosition = position;
				if (lookForward) {
					item.transform.LookAt(position + spline.GetDirection(p * stepSize));
				}
				item.transform.parent = transform;
			}
		}
		show_curve(true);
		initialized = true;
	}

	public void show_curve(bool b)
	// SHow the curve if b is true; otherwise hide it
	{
		foreach (Transform item in items)
		{
			item.gameObject.SetActive(b);
		}
	}

	public void clear()
	// Delete all the objects generated
	{
		foreach (Transform item in items)
		{
			Destroy(item.gameObject);
		}
		items.Clear();
		initialized = false;
	}
}