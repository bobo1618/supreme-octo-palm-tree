using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

[System.Serializable]
public class CardSaveData {
	public Sprite assignedImage;
	public bool isFlipped;
}

public class Card : MonoBehaviour, IPointerClickHandler
{
	[Header("Component references")]
	[SerializeField] Image cardImage, backImage;
	[SerializeField] Animator animator;

	[Header("Animation parameters and timings")]
	[SerializeField] string appearBoolParam, flipBoolParam, matchBoolParam;
	[SerializeField] float flipAnimTime;

	public bool IsFlipped => isFlipped;

	public delegate void OnClickEvent(Card card);
	public OnClickEvent OnClicked;

	Sprite assignedImage;
	bool isFlipped, isDisplaying = false;

	private void Awake() {
		//if (displayHolder) displayHolder.SetActive(false);
		if (!animator) animator = GetComponent<Animator>();
	}

	// Initialize card without displaying it. Call Display() to show card
	public void Initialize(bool startFlipped) {
		// Add other initialization processes if needed
		SetFlippedState(startFlipped);
		SetDisplay(false);
	}

	public void Initialize(string saveDataJson) {
		CardSaveData saveData = JsonUtility.FromJson<CardSaveData>(saveDataJson);
		SetFlippedState(saveData.isFlipped);
		SetImageSprite(saveData.assignedImage);
		SetDisplay(false);
	}

	public void SetFlippedState(bool toFlipped) {
		isFlipped = toFlipped;
		if (animator) animator.SetBool(flipBoolParam, toFlipped);
	}

	// Show or hide the entire card
	public void SetDisplay(bool toOn) {
		isDisplaying = toOn;
		if (animator) animator.SetBool(appearBoolParam, toOn);
	}

	public void SetMatched(bool isMatched) {
		if (animator) animator.SetBool(matchBoolParam, isMatched);
	}

	/// <summary>
	/// Sets the card's image to be matched
	/// </summary>
	public void SetImageSprite(Sprite sprite) {
		if (!cardImage) return;
		cardImage.sprite = assignedImage = sprite;
		cardImage.gameObject.SetActive(true);
	}

	/// <summary>
	/// Changes the texture of the back of the card (optional)
	/// </summary>
	public void SetBackSprite(Sprite sprite) {
		backImage.sprite = sprite;
	}

	public bool CheckMatch(Card otherCard) => otherCard.assignedImage == assignedImage;

	/// <summary>
	/// Implements the IPointerClickHandler interface. Calls an event on click
	/// </summary>
	public void OnPointerClick(PointerEventData eventData) {
		if (isFlipped || !isDisplaying) return;
		OnClicked?.Invoke(this);
	}


	public string GetSaveData() {
		CardSaveData data = new CardSaveData() {
			isFlipped = isFlipped,
			assignedImage = assignedImage
		};
		return JsonUtility.ToJson(data);
	}

}
