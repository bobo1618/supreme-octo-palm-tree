using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[Header("Grid specs")]
	[SerializeField] GridLayoutGroup cardGrid;
	[SerializeField] int gridColumnCount, gridRowCount;

	[Header("Gameplay tweaks")]
	[SerializeField] float timeGiven = 30;
	[SerializeField] int pointsPerMatch, pointsPerCombo;
	// If cardPreviewTime is set to 0, the cards will start face down
	[SerializeField] float cardAppearTime = 1f, cardPreviewTime = 2f, unflipDelay = 1f, resultsDelay = 1f;

	[Header("UI")]
	[SerializeField] Button startButton;
	[SerializeField] Button retryButton;
	[SerializeField] TMP_Text scoreText, comboText, timerText;
	[SerializeField] Image winUI, loseUI;

	[Header("Assets")]
	[SerializeField] Card cardPrefab;
	[SerializeField] List<Sprite> cardSprites;

	float timerValue;
	List<Card> curCards = new List<Card>();
	Card lastClickedCard = null;
	int matchCount = 0, matchTarget;
	int curScore, curCombo;

	bool isInitializing, isGameOver;

	private void Start() {
		// Remove invalid sprites
		cardSprites.RemoveAll(sprite => !sprite);
		if (winUI) winUI.gameObject.SetActive(false);
		if (loseUI) loseUI.gameObject.SetActive(false);

		if (startButton)
			startButton.onClick.AddListener(() => StartCoroutine(Initialize()));
		else
			StartCoroutine(Initialize());

		if (retryButton) {
			retryButton.onClick.AddListener(() => StartCoroutine(Initialize()));
			retryButton.gameObject.SetActive(false);
		}

		isInitializing = true;
		timerValue = timeGiven;
	}


	#region SETUP

	IEnumerator Initialize() {
		isInitializing = true;
		isGameOver = false;
		timerValue = timeGiven;

		if (startButton) startButton.gameObject.SetActive(false);
		if (retryButton) retryButton.gameObject.SetActive(false);

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
		bool cellCountIsOdd = cardCount % 2 != 0; // This will be used later to insert a blank space in the grid
		if (cellCountIsOdd) cardCount--;
		
		if (!cardGrid || cardCount < 2) {
			Debug.LogError("Need a grid layout that can fit at least 2 cards");
			yield break;
		}

		// Initialize counters
		matchCount = 0;
		matchTarget = cardCount / 2;
		curScore = 0;
		curCombo = 0;
		UpdateUI();


		//============ CARD GRID SETUP ===============

		// Clear any existing cards
		if (cardGrid.transform.childCount > 0) {
			curCards.ForEach(card => card.OnClicked -= OnCardClicked);
			curCards.Clear();

			// Clear any children of the grid, including blank cards
			var children = cardGrid.GetComponentsInChildren<Transform>(true);
			foreach (var child in children) {
				if (child != cardGrid.transform) Destroy(child.gameObject);
			}
			yield return null;
		}

		if (winUI) winUI.gameObject.SetActive(false);
		if (loseUI) loseUI.gameObject.SetActive(false);

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

			// Add a matching pair at random positions
			for (int i = 0; i < 2; i++)
				imageIndicesToUse.Insert(Random.Range(0, imageIndicesToUse.Count), imageIndexPool[indexToUse]);

			// Remove the index from the pool
			imageIndexPool.RemoveAt(indexToUse);
		}


		//============ INSTANTIATE THE CARDS ===============

		curCards = new List<Card>();

		// Add a blank card in the middle if the total is odd
		int addBlankCardAt = cellCountIsOdd ? cardCount / 2 : -1;
		bool showCardsAtStart = cardPreviewTime > 0;

		for (int i = 0; i < imageIndicesToUse.Count; i++) {
			if (i == addBlankCardAt) {
				GameObject blankObj = new GameObject("Blank", typeof(RectTransform));
				blankObj.transform.parent = cardGrid.transform;
			}

			Card newCard = Instantiate(cardPrefab, cardGrid.transform);
			newCard.SetImageSprite(cardSprites[imageIndicesToUse[i]]);
			newCard.Initialize(showCardsAtStart);
			newCard.OnClicked += OnCardClicked; // Register card click to this function
			curCards.Add(newCard);
		}

		float cardDisplayInterval = cardAppearTime / cardCount;
		foreach (var card in curCards) {
			card.SetDisplay(true);
			AudioManager.PlaySFX(SFXType.APPEAR);
			yield return new WaitForSeconds(cardDisplayInterval);
		}

		// Unflip cards after initial interval
		if (showCardsAtStart) {
			yield return new WaitForSeconds(cardPreviewTime);
			curCards.ForEach(card => card.SetFlippedState(false));
		}

		isInitializing = false;
		AudioManager.SetMusic(true);
	}

	#endregion


	#region GAMEPLAY

	void OnCardClicked(Card clickedCard) {
		if (isInitializing || isGameOver) return;

		// Note: Flipped cards don't register clicks, but condition added just in case
		if (!clickedCard || clickedCard.IsFlipped) return;

		clickedCard.SetFlippedState(true);
		AudioManager.PlaySFX(SFXType.FLIP);

		// Is this the second card being matched?
		if (lastClickedCard) {
			// Does this card match the last clicked card?
			bool isMatch = clickedCard.CheckMatch(lastClickedCard);
			
			if (isMatch) {
				matchCount++;
				curScore += pointsPerMatch + (curCombo * pointsPerCombo);
				curCombo++;
				AudioManager.PlaySFX(SFXType.MATCH, unflipDelay);

				// Matches, check if game is over
				if (matchCount == matchTarget) {
					// All cards matched, you're winner!
					isGameOver = true;
					StartCoroutine(ResultsUI(true, resultsDelay));
				}
			}
			else {
				// Doesn't match, unflip after delay
				StartCoroutine(UnflipCardCR(lastClickedCard, clickedCard));
				AudioManager.PlaySFX(SFXType.MISMATCH, unflipDelay);
				curCombo = 0;
			}

			UpdateUI();
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

	IEnumerator ResultsUI(bool isWin, float delayTime) {
		yield return new WaitForSeconds(delayTime);
		foreach (var card in curCards) card.SetDisplay(false);
		GameObject uiToShow = isWin ? winUI.gameObject : loseUI.gameObject;
		if (uiToShow) uiToShow.SetActive(true);
		if (retryButton) retryButton.gameObject.SetActive(true);
		AudioManager.PlaySFX(isWin ? SFXType.WIN : SFXType.LOSE);
		AudioManager.SetMusic(false);
	}

	void UpdateUI() {
		if (scoreText) scoreText.text = curScore.ToString();
		if (comboText) comboText.text = curCombo.ToString();
	}

	private void Update() {
		// Update the timer and end the game if it hits 0
		if (!isInitializing && !isGameOver && timerValue > 0) {
			timerValue -= Time.deltaTime;
			if (timerValue <= 0) {
				isGameOver = true;
				StartCoroutine(ResultsUI(false, 0));
			}
		}

		if (timerText) {
			var timeSpan = System.TimeSpan.FromSeconds(timerValue);
			timerText.text = timeSpan.ToString(@"mm\:ss");
		}
	}

	#endregion
}
