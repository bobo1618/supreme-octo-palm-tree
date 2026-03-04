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
	[SerializeField] Image cardImage, backImage;
	[SerializeField] GameObject flippedObj, unflippedObj, displayHolder;

	public bool IsFlipped => isFlipped;

	public delegate void OnClickEvent(Card card);
	public OnClickEvent OnClicked;

	Sprite assignedImage;
	bool isFlipped;

	private void Awake() {
		if (displayHolder) displayHolder.SetActive(false);
	}

	// Initialize card without displaying it. Call Display() to show card
	public void Initialize(bool startFlipped) {
		// Add other initialization processes if needed
		SetFlippedState(startFlipped);
		displayHolder.SetActive(false);
	}

	public void Initialize(string saveDataJson) {
		CardSaveData saveData = JsonUtility.FromJson<CardSaveData>(saveDataJson);
		SetFlippedState(saveData.isFlipped);
		SetImageSprite(saveData.assignedImage);
		displayHolder.SetActive(false);
	}

	public void SetFlippedState(bool toFlipped) {
		isFlipped = toFlipped;
		flippedObj.SetActive(toFlipped);
		unflippedObj.SetActive(!toFlipped);
	}

	// Actually show the card
	public void Display() {
		displayHolder.SetActive(true);
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
		if (isFlipped || !displayHolder.activeSelf) return;
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
