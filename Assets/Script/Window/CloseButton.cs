using System.Collections;
using UnityEngine;

public class CloseButton : MonoBehaviour
{
	RectTransform _rectTransform;
	private void OnEnable()
	{
		StartCoroutine(SetRectPosition());
	}

	IEnumerator SetRectPosition()
	{
		yield return null;
		if (_rectTransform == null)
			_rectTransform = GetComponent<RectTransform>();

		_rectTransform.position = new Vector3(0, Screen.height, 0);
		_rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
	}
}
