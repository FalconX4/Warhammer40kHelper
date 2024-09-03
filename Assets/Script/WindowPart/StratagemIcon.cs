using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StratagemIcon : MonoBehaviour
{
	public Image Frame;
	public Image Icon;
	public TextMeshProUGUI Text;

	public void SetData(StratagemsScriptableObject.StratagemIcon icon, int CP, Color color)
	{
		var isCP = icon == StratagemsScriptableObject.StratagemIcon.CP;
		Frame.color = color;
		Text.gameObject.SetActive(isCP);
		if (isCP)
		{
			Text.text = $"{CP}CP";
			Text.color = color;
		}

		Icon.gameObject.SetActive(!isCP);
		if (!isCP)
		{
			Icon.sprite = Database.Instance.StratagemsScriptableObject.Get(icon).Sprite;
			Icon.color = color;
		}
	}
}
