using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InstructionUI : MonoBehaviour
{
	public Image _background;
	
	public TextMeshProUGUI _mainInstruction;

	public TextMeshProUGUI _beingMade;

	public TextMeshProUGUI _stepsLeftText;

	public Recipe _recipe;

	private InstructionsAnchorable _currentAnchor;
	
	private bool _hidden = false;

	public bool LookAtCamera = false;
	public void Start()
	{
		Hide();
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			Hide(0.3f);	
		}
		
		if (_currentAnchor != null)
		{
			Transform bestAnchorPoint = _currentAnchor.GetBestAnchorPoint();

			Quaternion targetRot; 
				
			if (LookAtCamera)
			{
				targetRot =
					Quaternion.LookRotation(transform.position - Camera.main.transform.position,
						Vector3.up);
			}
			else
			{
				targetRot = bestAnchorPoint.rotation;
			}
			
			var lerpAmt = 
				Time.deltaTime * (0.1f/Mathf.Lerp(0.02f, 0.15f,Vector3.Distance(transform.position, bestAnchorPoint.position)));
		
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRot,
				lerpAmt);

			transform.position = Vector3.Lerp(transform.position, bestAnchorPoint.position,
				lerpAmt);

			transform.localScale = Vector3.Lerp(transform.localScale, bestAnchorPoint.localScale, lerpAmt);
		}
	}

	public void SetRecipe(Recipe recipe)
	{
		_beingMade.text = recipe.Name;
		
		recipe.StepsLeftCount.Subscribe(UpdateStepsLeft);
		recipe.CurrentStep.Subscribe(UpdateStepInstructions);
	}

	private void UpdateStepsLeft(int stepsLeftCount)
	{
		if (stepsLeftCount == 0)
		{
			_stepsLeftText.text = "Last Step";
		}
		else if (stepsLeftCount == 1)
		{
			_stepsLeftText.text = "1 Step Left";
		}
		else
		{
			_stepsLeftText.text = $"{stepsLeftCount} Steps Left";
		}
	}

	private void UpdateStepInstructions(Recipe.RecipeStep step)
	{
		if (step.Instruction == "")
		{
			Hide();
		}
		else
		{			
			if (step.getAnchor != null)
			{
				_currentAnchor?.DeAnchor();
				_currentAnchor = step.getAnchor();
				_currentAnchor.AnchorInstructions(this);
			}
					
			_mainInstruction.text = step.Instruction;
		}
	}

	public void Show(float duration = 0.3f, float delay = 0.0f)
	{
		if (!_hidden) return; //already shown
		
		Debug.Log("Show Instructions");
		
		_hidden = false;
		TweenFade(1, duration, delay);
	}

	public void Hide(float duration = 0.3f, float delay = 0.0f)
	{
		//if (_hidden) return; //already hdiden
		
		Debug.Log("Hide Instructions");
		
		_hidden = true;		
		TweenFade(0, duration, delay);
	}
	
	private void TweenFade(float toValue, float duration, float delay =0.0f)
	{
		Sequence seq = DOTween.Sequence();
		seq.Append(
			_background.DOFade(toValue, duration));

		seq.Insert(0,
			_mainInstruction.DOFade(toValue, duration));

		seq.Insert(0,
			_beingMade.DOFade(toValue, duration));

		seq.Insert(0,
			_stepsLeftText.DOFade(toValue, duration));

		seq.SetDelay(delay);
		
		return;
	}	
}
