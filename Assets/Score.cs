using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

	public delegate void ScoreIncreasedAction(int amountIncrease);
	public static event ScoreIncreasedAction ScoreIncreased;

	public static Score currentScore;

	private int score = 0;

	public void ScoreIncreasedEvent(int amountIncrease)
	{
		ScoreIncreased (amountIncrease);
	}

	void OnEnable ()
	{
		ScoreIncreased += OnScoreIncreased;
	}
	
	void OnDisable()
	{
		ScoreIncreased -= OnScoreIncreased;
	}

	void OnScoreIncreased(int amountIncrease)
	{
		score += amountIncrease;
	}

	// Use this for initialization
	void Start () {
		currentScore = this;
	}
	
	// Update is called once per frame
	void Update () {
		guiText.text = "Your Score: " + score.ToString ();
	}
}
