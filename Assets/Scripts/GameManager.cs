using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[SerializeField] Card cardPrefab;
	[SerializeField] List<Sprite> cardSprites;
	[SerializeField] int gridRowCount, gridColumnCount;
	[SerializeField] GridLayoutGroup cardGrid;
	[SerializeField] float initialCardShowTime = 2f, unflipDelay = 1f, victoryDelay = 1f;

	List<Card> curCards = new List<Card>();
	Card lastClickedCard = null;
	int matchCount = 0, matchTarget;
	int curScore;

	private void Start() {
		// Remove invalid sprites
		cardSprites.RemoveAll(sprite => !sprite);

		StartCoroutine(Initialize());
	}


	#region SETUP

	IEnumerator Initialize() {
		if (cardSprites.Count == 0) {
			Debug.LogError("At least one card image required");
			yield break;
		}

		if (!cardPrefab) {
			Debug.LogError("Card prefab required");
			yield break;
		}

		int cardCount = gridRowCount * gridColumnCount;

		// Take note if our grid has an odd number of cells, and ensure our card count is even
		bool cellCountIsOdd = cardCount % 2 != 0;
		if (cellCountIsOdd) cardCount--;
		
		if (!cardGrid || cardCount < 2) {
			Debug.LogError("Need a grid layout that can fit at least 2 cards");
			yield break;
		}

		matchCount = 0;
		matchTarget = cardCount / 2;


		//============ CARD GRID SETUP ===============

		// Clear any existing cards
		if (cardGrid.transform.childCount > 0) {
			curCards.ForEach(card => card.OnClicked -= OnCardClicked);
			curCards.Clear();

			// Clear any children of the grid, including blank cards
			var children = cardGrid.GetComponentsInChildren<GameObject>(true);
			foreach (var child in children) {
				if (child != cardGrid.gameObject) Destroy(child);
			}
			yield return null;
		}

		// Set up the grid to follow the defined width and height
		cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		cardGrid.constraintCount = gridColumnCount;

		// Calculate the required cell size
		RectTransform gridRect = cardGrid.GetComponent<RectTransform>();
		yield return null; // Wait one update loop for the grid to initialize to its proper size
		Vector2 cellSize = gridRect.rect.size;
		cellSize.x = (cellSize.x / gridColumnCount) - (cardGrid.spacing.x + 5f); // 5f buffer just in case
		cellSize.y = (cellSize.y / gridRowCount) - (cardGrid.spacing.y + 5f);
		cardGrid.cellSize = cellSize;


		//============ DECIDE WHAT CARDS TO USE ===============

		List<int> imageIndexPool = new List<int>(); // We'll pick randomly from this pool of image indices to decide which ones to use
		List<int> imageIndicesToUse = new List<int>();

		while (imageIndicesToUse.Count < cardCount) {
			if (imageIndexPool.Count == 0) {
				// Refill the index pool, in case we have more cards than image pairs
				for (int i = 0; i < cardSprites.Count; i++) imageIndexPool.Add(i);
			}

			// Choose a random image
			int indexToUse = Random.Range(0, imageIndexPool.Count);

			// Add a matching pair
			imageIndicesToUse.Add(imageIndexPool[indexToUse]);
			imageIndicesToUse.Add(imageIndexPool[indexToUse]);

			// Remove the index from the pool
			imageIndexPool.RemoveAt(indexToUse);
		}

		// Shuffle the cards by using a random sort
		imageIndicesToUse.Sort((x, y) => Random.value > 0.5f ? 1 : -1);


		//============ INSTANTIATE THE CARDS ===============

		curCards = new List<Card>();

		// Add a blank card in the middle if the total is odd
		int addBlankCardAt = cellCountIsOdd ? cardCount / 2 : -1;
		bool showCardsAtStart = initialCardShowTime > 0;

		for (int i = 0; i < imageIndicesToUse.Count; i++) {
			if (i == addBlankCardAt) {
				GameObject blankObj = new GameObject("Blank", typeof(RectTransform));
				blankObj.transform.parent = cardGrid.transform;
			}

			Card newCard = Instantiate(cardPrefab, cardGrid.transform);
			newCard.SetImageSprite(cardSprites[imageIndicesToUse[i]]);
			newCard.Initialize(showCardsAtStart);
			newCard.OnClicked += OnCardClicked;
			curCards.Add(newCard);
		}

		// Unflip cards after initial interval
		if (showCardsAtStart) {
			yield return new WaitForSeconds(initialCardShowTime);
			curCards.ForEach(card => card.SetFlippedState(false));
		}
	}

	#endregion


	#region GAMEPLAY

	void OnCardClicked(Card clickedCard) {
		// Note: Flipped cards don't register clicks, but condition added just in case
		if (!clickedCard || clickedCard.IsFlipped) return;

		clickedCard.SetFlippedState(true);
		AudioManager.PlaySFX(SFXType.FLIP);

		// Is this the second card being matched?
		if (lastClickedCard) {
			// Does this card match the last clicked card?
			bool isMatch = clickedCard.CheckMatch(lastClickedCard);
			
			if (isMatch) {
				// Matches, check if game is over
				matchCount++;
				AudioManager.PlaySFX(SFXType.MATCH);

				if (matchCount == matchTarget) {
					// All cards matched, you're winner!
					StartCoroutine(VictoryCR());
				}
			}
			else {
				// Doesn't match, unflip after delay
				StartCoroutine(UnflipCardCR(lastClickedCard, clickedCard));
				AudioManager.PlaySFX(SFXType.UNMATCH);
			}

			lastClickedCard = null;
		}
		else {
			lastClickedCard = clickedCard;
		}
	}

	IEnumerator UnflipCardCR(params Card[] cards) {
		yield return new WaitForSeconds(unflipDelay);
		foreach (Card card in cards) card.SetFlippedState(false);
	}

	IEnumerator VictoryCR() {
		yield return new WaitForSeconds(victoryDelay);
		print("YOU'RE WINNER!");
		AudioManager.PlaySFX(SFXType.WIN);
	}

	#endregion
}
