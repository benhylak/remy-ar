
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnitySDK.WebSocketSharp;

public class Burner
{
    public enum BurnerPosition
    {
        LOWER_LEFT,
        LOWER_RIGHT,
        UPPER_LEFT,
        UPPER_RIGHT
    }
    
    string _name
    {
        set
        {
            var parseSuccess = BurnerPosition.TryParse(value.ToUpper(), out Position);

            if (!parseSuccess)
            {
                Debug.Log("Couldn't parse " + value + "into a burner position");
            }
        }
    }

    public static BurnerPosition ParsePosition(string s)
    {
        BurnerPosition pos;
        var parseSuccess = BurnerPosition.TryParse(s.ToUpper(), out pos);
        
        if(!parseSuccess) Debug.Log("Big fail");
        
        return pos;
    }

    public Burner()
    {
        IsOn = new ReactiveProperty<bool>();
        IsPotDetected = new ReactiveProperty<bool>();
        IsBoiling = new ReactiveProperty<bool>();
        Temperature = new ReactiveProperty<double>();
    }
    
    public ReactiveProperty<bool> IsOn { get; private set; }
    
    public ReactiveProperty<bool> IsPotDetected { get; private set; }
    
    public ReactiveProperty<bool> IsBoiling { get; private set; }
    public ReactiveProperty<double> Temperature { get; private set; }
    
    public BurnerPosition Position;

    public Recipe RecipeInProgress;

    public override string ToString()
    {
        return string.Format("Position: {0} On: {1} Temp: {2}", Position, IsOn.Value,Temperature.Value);
    }

    public void MapFromDict(IDictionary<string, object> source)
    {
        foreach (var item in source)
        {
            switch (item.Key)
            {
                case "on":
                    IsOn.Value = (bool) item.Value;
                    break;
                
                case "pot_detected":
                    IsPotDetected.Value = (bool) item.Value;
                    break;
                
                case "boiling":
                    IsBoiling.Value = (bool) item.Value;
                    break;
                
                case "temp":
                    Temperature.Value = (double) item.Value;
                    break;
                
                case "name":
                    _name = (string) item.Value;
                    break;

                case "recipe":
                    var values = item.Value as Dictionary<string, object>;

                    if (values == null)
                    {
                        Debug.Log("Cast failed");
                        return;
                    }

                    if (RecipeInProgress == null)
                    {
                        //start one? -- means a recipe was detected remotely without starting
                    }
                    else
                    {
                        var status = (string)values["status"];

                        if (status.IsNullOrEmpty() || RecipeInProgress.Status.Equals(status))
                        {
                            return;
                        }
                            
                        RecipeInProgress.UpdateStatus(status);
                    }
                    
                    break;

                
                default:
                    //Debug.Log("Could not map key: " + item.Key);
                    break;
                    
            }
        }
    }
    
    public static Burner CreateFromDict(IDictionary<string, object> source)
    {
        var burner = new Burner();

        burner.MapFromDict(source);

        return burner;
    }

    public IDictionary<string, object> AsDictionary(BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        return GetType().GetProperties(bindingAttr).ToDictionary
        (
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(this, null)
        );
    }
}