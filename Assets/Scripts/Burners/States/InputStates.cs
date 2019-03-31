
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

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
    
                if (lastState is BufferState || lastState is BurnerStateMachine.AvailableState)
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
                    
                    return new BurnerStateMachine.AvailableState(_burner);
                }
                else if (RecipeManager.Instance.IsWaitingForBurner())
                {
                    var recipe = RecipeManager.Instance.UseBurner(_burner);
           
                    return new RecipeStates.UseForRecipeState(_burner, recipe);
                }
                else if (this._burner._gazeReceiver.isLookedAt && _burner._gazeReceiver.currentGazeDuration > 0.25f)
                {
                    return new VoiceInputState(_burner);
                }
                
                return null;
            }
        }
        
        public class VoiceInputState : State
        {
            public BurnerBehaviour _burner;
    
            public float _lastLookedAt;
            public float _timeOut = 1.8f;
            
            private static Regex timeRegex = new Regex(@"(?<minutes>\d+(?=\sminutes))|(?<seconds>\d+(?=\sseconds))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            private static Regex boilRegex = new Regex(@"(oil|boil|well)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
            //(?P<minutes>\d+(?=\sminutes))|(?P<seconds>\d+(?=\sseconds))
            
            
            public VoiceInputState(BurnerBehaviour burner)
            {
                this._burner = burner;
                Debug.Log("Voice input state");
                
                BigKahuna.Instance.speechRecognizer.Active = true;
                _burner.ShowInputPrompt();
            }
            
            public override State Update()
            {                    
                if (_burner._gazeReceiver.timeSinceLastGaze > _timeOut)
                {
                    BigKahuna.Instance.speechRecognizer.Active = false;
                    return new TimerPromptState(_burner, this);
                }
                
                _burner.SetInputLevel(BigKahuna.Instance.speechRecognizer.audioLevel);
    
                if (BigKahuna.Instance.speechRecognizer.finalized)
                {
                    MatchCollection matches;

                    var recognizedText = BigKahuna.Instance.speechRecognizer.recognizedText;
                                     
                    BigKahuna.Instance.speechRecognizer.recognizedText = "";
                    BigKahuna.Instance.speechRecognizer.finalized = false;
                    BigKahuna.Instance.speechRecognizer.Active = false;
    
                    if (boilRegex.IsMatch(recognizedText))
                    {                           
                        return new BurnerStateMachine
                            .BurnerTransitionState(
                                _burner,
                                _burner.HideProactivePrompt(),
                                () => new BoilStates.WaitingForBoilState(_burner, 
                                        () => new BoilStates.BoilDoneTimerState(_burner)),
                                0.3f);                          
                    }
                    else if ((matches = timeRegex.Matches(recognizedText)).Count > 0)
                    {               
                        TimeSpan ts = new TimeSpan();
                        
                        foreach (Match m in matches)
                        {
                            GroupCollection groups = m.Groups;
                        
                            Debug.Log(m.Value);
    
                            Group minutes = groups["minutes"];            
                            Group seconds = groups["seconds"];
                        
                            var minutesVal = minutes.Success ? int.Parse(minutes.Value) : 0;       
                            var secondsVal = seconds.Success ? int.Parse(seconds.Value) : 0;
                            
                            ts = ts.Add(new TimeSpan(0, minutesVal, secondsVal));
                        }
                        
                        Debug.Log(ts.ToString());
                        
                        BigKahuna.Instance.speechRecognizer.Active = false;
                        
                        return new BurnerStateMachine
                            .BurnerTransitionState(
                                _burner,
                                _burner.HideProactivePrompt(),
                                () => new TimerStates.WaitingForTimerState(_burner, ts),
                                0.2f);                                        
                    }
                }
    
                return null;
            }
    
        }    
    }
}
