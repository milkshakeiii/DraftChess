using UnityEngine;
using System.Collections;

public abstract class BoardObserver : MonoBehaviour {



	// Use this for initialization
	void Start () {
		print ("start from abstract class");
		Board.CurrentBoard.AddObserver (this);
		ConcreteStart ();
	}

	public virtual void ConcreteStart()
	{

	}

	
	// Update is called once per frame
	void Update () {
	
	}

	public abstract void BoardChangedNotification();
	
}
