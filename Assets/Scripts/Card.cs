using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] Image cardImage, backImage;
	[SerializeField] GameObject flippedObj, unflippedObj;

	[SerializeField, HideInInspector] public bool IsFlipped { get; private set; }

	[SerializeField, HideInInspector] Sprite assignedImage;

	public delegate void OnClickEvent(Card card);
	public OnClickEvent OnClicked;

	private void Awake() {
		if (cardImage) cardImage.gameObject.SetActive(false);
	}

	public void Initialize(bool startFlipped) {
		// Add other initialization processes if needed
		SetFlippedState(startFlipped);
	}

	public void SetFlippedState(bool isFlipped) {
		IsFlipped = isFlipped;
		flippedObj.SetActive(isFlipped);
		unflippedObj.SetActive(!isFlipped);
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

	public bool DoesImageMatch(Sprite sprite) => sprite == assignedImage;


	public void OnPointerClick(PointerEventData eventData) {
		if (IsFlipped) return;
		OnClicked?.Invoke(this);
	}
}
