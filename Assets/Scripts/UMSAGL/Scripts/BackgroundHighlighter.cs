using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Animation = Visualization.Animation.Animation;

namespace UMSAGL.Scripts
{
	public class BackgroundHighlighter : MonoBehaviour {

		private Color defaultColor;
		// private int highlight = 0; // TODO
		private void Awake()
		{
			defaultColor = GetComponentInChildren<Image>().color;
			// highlight = 0;
		}

		public void HighlightOutline()
		{
			GetComponentInChildren<Outline>().enabled = true;
		}

		public void HighlightBackground(Color color)
		{
			highlightBackground(color);
		}

		public void HighlightBackground()
		{
			highlightBackground(Animation.Instance.classColor);
		}

		private void highlightBackground(Color color)
		{
			// if (highlight == 0)
			// {
			RectTransform rc = GetComponent<RectTransform>();
			rc.DOScaleX(1.08f, 0.5f);
			rc.DOScaleY(1.08f, 0.5f);
			GetComponentInChildren<Image>().color = color;
			//}
			// highlight++;
		}

		public void UnhighlightOutline()
		{
			GetComponentInChildren<Outline>().enabled = false;
		}

		public void UnhighlightBackground(Color color)
		{
			unhighlightBackground(color);
		}

		public void UnhighlightBackground()
		{
			unhighlightBackground(defaultColor);
		}

		private void unhighlightBackground(Color color)
		{
			// if (highlight>0)
			// 	highlight--;
			// if (highlight == 0)
			// {
				RectTransform rc = GetComponent<RectTransform>();
				rc.DOScaleX(1f, 0.5f);
				rc.DOScaleY(1f, 0.5f);
				GetComponentInChildren<Image>().color = color;
			// }
		}

	}
}
