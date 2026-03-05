using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SaveDataHolder
{
	public List<CardSaveData> cardSaveDatas;
	public float timeLeft;
	public int matchCount, score, combo, gridColumnCount, gridRowCount;
}


[System.Serializable]
public class CardSaveData {
	public Sprite assignedImage;
	public bool isMatched;
}