using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour {

    public float gameSpeed = 1f;
    public GameTime gameTime;
    public Text dayTimeTextField;

	// Use this for initialization
	void Start () {
        gameTime = new GameTime();
        gameTime.RegisterOnTimeChanged(OnTimeChanged);

        dayTimeTextField = GameObject.Find("DayTime").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        gameTime.Update(Time.deltaTime);
	}

    public void OnTimeChanged(GameTime time)
    {
        dayTimeTextField.text = time.GetTimeString();
    }

}
