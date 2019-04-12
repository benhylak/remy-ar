using System.Collections;
using System.Collections.Generic;
using BensToolBox.AR.Scripts;
using UnityEngine;

public class PushButtonBehavior : MonoBehaviour
{
	private float PRESSED_Y = -0.0106f;

	public bool isPressed;

	[SerializeField]
	public GameObject button;

	public GameObject buttonBase;

	public float lastPress = 0;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (button == null)
		{
			Debug.Log("button is null");
			
		}
		if (button.transform.localPosition.y <= PRESSED_Y)
		{
			if (!isPressed && Time.time - lastPress > 2f)
			{
				isPressed = true;
				lastPress = Time.time;
				OnPress();
			}
			
			button.transform.SetLocalPosY(PRESSED_Y);
		}
		else isPressed = false;
	}

	void OnPress()
	{
		var recipe = new RamenRecipe(BigKahuna.Instance.ramenUI);
		RecipeManager.Instance.StartRecipe(recipe);
	}
}
