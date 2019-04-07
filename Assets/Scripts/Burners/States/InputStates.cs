
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DG.Tweening;
using UnityEngine;
using UnitySDK.WebSocketSharp;



namespace Burners.States
{
	public static class InputStates
    {  
        public class TimerPromptState : State
        {
            public BurnerBehaviour _burner;
            
            public TimerPromptState(BurnerBehaviour burner, State lastState)
            {
                this._burner = burner;
                //this._burner.ShowProactiveTimer();
    
                if (lastState is BufferState || lastState is BurnerStates.AvailableState)
                {
                    _burner.HiddenToProactive();
                }
                else if (lastState is VoiceInputState)
                {
                    _burner.InputToProactive();
                }
                
                Debug.Log("Entering Proactive State...");
            }
    
            public override State Update()
            {
                if (this._burner._model.IsPotDetected.Value == false)
                {
                    _burner.HideProactivePrompt();
                    
                    return new BurnerStates.AvailableState(_burner);
                }
                else if (RecipeManager.Instance.IsWaitingForBurner())
                {
                    var recipe = RecipeManager.Instance.UseBurner(_burner);

                    if (recipe != null)
                    {
                        _burner.HideProactivePrompt();
                        return new RecipeStates.UseForRecipeState(_burner, recipe);
                    }          
                }
                else if (!BigKahuna.Instance.speechRecognizer.Active && this._burner._gazeReceiver.isLookedAt && _burner._gazeReceiver.currentGazeDuration > 0.4f)
                {
                    return new VoiceInputState(_burner);
                }

                return this;
            }
        }
        
        public class VoiceInputState : State
        {
            public BurnerBehaviour _burner;
    
            public float _lastLookedAt;
            public float _timeOut = 0.7f;
            
            private static readonly string DEFAULT_PROMPT_TEXT = "for example, you can say 'boil' or 'two minutes'";
            
            private static Regex timeRegex = new Regex(@"(?<minutes>\d+(?=\sminutes))|(?<seconds>\d+(?=\sseconds))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            private static Regex boilRegex = new Regex(@"(oil|boil|well)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
            //(?P<minutes>\d+(?=\sminutes))|(?P<seconds>\d+(?=\sseconds))
            
            
            public VoiceInputState(BurnerBehaviour burner)
            {
                this._burner = burner;
                Debug.Log("Voice input state");

                _burner.voicePromptText.text = "for example, you can say 'boil' or 'two minutes'";
                
                BigKahuna.Instance.speechRecognizer.Active = true;
                _burner.ShowInputPrompt();
                _burner.voicePromptText.DOKill();
                _burner.voicePromptText.DOFade(1f, 0.3f).SetEase(Ease.InSine);
            }

            private void ResetPromptText(bool hide= true)
            {
                if (hide)
                {
                    _burner.voicePromptText.DOKill();
                    _burner.voicePromptText.DOFade(0f, 0.3f).SetEase(Ease.InSine);
                }
           
                _burner.voicePromptText.text = DEFAULT_PROMPT_TEXT; 
            }
            
            public override State Update()
            {             
                if (_burner._gazeReceiver.timeSinceLastGaze > _timeOut)
                {                    
                    ResetPromptText();
                    BigKahuna.Instance.speechRecognizer.Active = false;
                    return new TimerPromptState(_burner, this);
                }
                else if(BigKahuna.Instance.DisableOtherListeners)
                {                                      
                    ResetPromptText();
                    return new TimerPromptState(_burner, this);
                }

                var recognizedText = BigKahuna.Instance.speechRecognizer.recognizedText;
                
                if (!recognizedText.IsNullOrEmpty())
                {
                    _burner.voicePromptText.text = recognizedText;
                }
              
                _burner.SetInputLevel(BigKahuna.Instance.speechRecognizer.audioLevel);
    
                if (BigKahuna.Instance.speechRecognizer.finalized)
                {
                    Debug.Log("Speech is Finalized. Result: " + BigKahuna.Instance.speechRecognizer.recognizedText);

                    recognizedText = recognizedText.Replace("two", "2");
                    recognizedText = recognizedText.Replace("one", "1");
                    recognizedText = recognizedText.Replace("three", "3");
                    recognizedText = recognizedText.Replace("four", "4");
                     
                    MatchCollection matches;
    
                    if (boilRegex.IsMatch(recognizedText))
                    {      
                        Debug.Log("Boil match");
                        
                        BigKahuna.Instance.speechRecognizer.Active = false;
                        BigKahuna.Instance.speechRecognizer.ClearResults();
                        
                        ResetPromptText();
                        
                        return new BurnerStates
                            .BurnerTransitionState(
                                _burner,
                                _burner.HideProactivePrompt(),
                                () => new BoilStates.WaitingForBoilState(_burner, 
                                        () => new BoilStates.BoilDoneTimerState(_burner)),
                                0.3f);                          
                    }
                    else if ((matches = timeRegex.Matches(recognizedText)).Count > 0)
                    {           
                        Debug.Log("time match");
                        
                        TimeSpan ts = new TimeSpan();
                        
                        foreach (Match m in matches)
                        {
                            GroupCollection groups = m.Groups;
                        
                            Debug.Log(m.Value);
    
                            Group minutes = groups["minutes"];            
                            Group seconds = groups["seconds"];

                            var minutesVal = 0;
                            var secondsVal = 0;

                            if (minutes.Success)
                            {
                                int.TryParse(minutes.Value, out minutesVal);
                            }
                            else if (seconds.Success)
                            {
                                int.TryParse(seconds.Value, out secondsVal);
                            }
                                                 
                            ts = ts.Add(new TimeSpan(0, minutesVal, secondsVal));
                        }
                        
                        Debug.Log(ts.ToString());
                        
                        BigKahuna.Instance.speechRecognizer.ClearResults();
                        BigKahuna.Instance.speechRecognizer.Active = false;
                        
                        ResetPromptText();
                        
                        return new BurnerStates
                            .BurnerTransitionState(
                                _burner,
                                _burner.HideProactivePrompt(),
                                () => new TimerStates.WaitingForTimerState(_burner, ts),
                                0.2f);                                        
                    }
                    else
                    {                 	
                        var seq = DOTween.Sequence();

                        seq.Append(
                            _burner.voicePromptText
                                .DOColor(new Color(255f, 0, 0, 80f), 0.2f));

                        seq.Append(
                            _burner.voicePromptText.DOFade(0, 0.3f)
                                .SetDelay(.35f)
                                .OnComplete(() =>
                                {
                                    _burner.voicePromptText.DOColor(new Color(255, 255, 255, 0), 0);
                                    _burner.voicePromptText.text = "sorry, i didn't get that";
                                })
                        );
                        
                        seq.Append(
                            _burner.voicePromptText.DOFade(1, 0.3f));
                        
                        seq.Append(
                            _burner.voicePromptText.DOFade(0, 0.2f)
                                .SetDelay(0.8f)
                                .OnComplete(() => { _burner.voicePromptText.text = DEFAULT_PROMPT_TEXT; })
                        );

                        seq.Append(
                            _burner.voicePromptText.DOFade(1, 0.3f));
                          
                        BigKahuna.Instance.speechRecognizer.ClearResults();
                    }
                }

                return this;
            }
    
        }    
    }
}
