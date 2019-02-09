
using UnityEngine;

public class Burner
{
    public enum BurnerPosition
    {
        LOWER_LEFT,
        LOWER_RIGHT,
        UPPER_LEFT,
        UPPER_RIGHT
    }
    
    public string name
    {
        get { return position.ToString().ToLower(); }
        set
        {
            var parseSuccess = BurnerPosition.TryParse(value.ToUpper(), out position);

            if (!parseSuccess)
            {
                Debug.Log("Couldn't parse " + value + "into a burner position");
            }
        }
    }
    
    public bool on;
    public double temp;
    public bool pot_detected;
    public bool boiling;

    public BurnerPosition position;

    public override string ToString()
    {
        return string.Format("Position: {0} On: {1} Temp: {2}", position, on, temp);
    }
}