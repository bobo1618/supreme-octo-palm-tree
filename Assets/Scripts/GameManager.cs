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
	[SerializeField] bool showCardsAtStart;

	List<Card> curCards = new List<Card>();

	private void Start() {
		// Remove invalid sprites
		cardSprites.RemoveAll(sprite => !sprite);

		StartCoroutine(Initialize());
	}


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
		var gridRect = cardGrid.GetComponent<RectTransform>();
		yield return null; // Wait one update loop for the grid to initialize to its proper size
		Vector2 cellSize = gridRect.rect.size;
		cellSize.x = (cellSize.x / gridColumnCount) - (cardGrid.spacing.x * (gridColumnCount - 1)) - 5f;
		cellSize.y = (cellSize.y / gridRowCount) - (cardGrid.spacing.y * (gridRowCount - 1)) - 5f; // 5f buffer just in case



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

		for (int i = 0; i < imageIndicesToUse.Count; i++) {
			Card newCard = Instantiate(cardPrefab, cardGrid.transform);
			newCard.SetImageSprite(cardSprites[imageIndicesToUse[i]]);
			newCard.Initialize(showCardsAtStart);
			newCard.OnClicked += OnCardClicked;
			curCards.Add(newCard);

			if (i == addBlankCardAt) {
				GameObject blankObj = new GameObject("Blank", typeof(RectTransform));
				blankObj.transform.parent = cardGrid.transform;
			}
		}
	}


	void OnCardClicked(Card clickedCard) {
		if (clickedCard) return;
	}
}
