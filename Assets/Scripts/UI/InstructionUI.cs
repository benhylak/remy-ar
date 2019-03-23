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

	private bool _hidden = false;

	public void Start()
	{
		Hide();
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
			if (_hidden)
			{
				Show();
			}
			
			_mainInstruction.text = step.Instruction;
		}
	}

	public void Show()
	{
		if (!_hidden) return; //already shown
		
		_hidden = false;
		TweenFade(1, 0.3f);
	}

	public void Hide()
	{
		if (_hidden) return; //already hdiden
		
		_hidden = true;
		
		TweenFade(0, 0.3f);
	}
	private void TweenFade(float toValue, float duration)
	{
		_background.DOFade(toValue, duration);
		_mainInstruction.DOFade(toValue, duration);
		_beingMade.DOFade(toValue, duration);
		_stepsLeftText.DOFade(toValue, duration);
	}	
}
