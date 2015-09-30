using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugSettings : MonoBehaviour {
	public Slider workerSlider;
	public Text workerValueLabel;
	public Slider weekSlider;
	public Text weekValueLabel;

	public void Awake() {
		workerSlider.value = GameSettings.InstanceOrCreate.WorkerIncrement;
		weekSlider.value = GameSettings.InstanceOrCreate.TotalWeeks;
	}

	public void OnValueChanged() {
		workerValueLabel.text = Mathf.Round(workerSlider.value * 100).ToString() + "%";
		weekValueLabel.text = weekSlider.value.ToString();
	}
}
