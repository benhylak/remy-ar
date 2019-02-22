using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
 
public class DrawCircle : MonoBehaviour
{
    public enum Axis { X, Y, Z };
 
    [SerializeField]
    [Tooltip("The number of lines that will be used to draw the circle. The more lines, the more the circle will be \"flexible\".")]
    [Range(0, 1000)]
    private int _segmentsInCircle = 60;

    private int _segments;
 
    [SerializeField]
    [Tooltip("The radius of the horizontal axis.")]
    private float _horizRadius = 10;
 
    [SerializeField]
    [Tooltip("The radius of the vertical axis.")]
    private float _vertRadius = 10;
 
    [SerializeField]
    [Tooltip("The offset will be applied in the direction of the axis.")]
    private float _offset = 0;
 
    [SerializeField]
    [Tooltip("The axis about which the circle is drawn.")]
    private Axis _axis = Axis.Z;
 
    [SerializeField]
    [Tooltip("If checked, the circle will be rendered again each time one of the parameters change.")]
    private bool _checkValuesChanged = true;

    [SerializeField]
    private float SkipAngle = 60f;
 
    private int _previousSegmentsValue;
    private float _previousHorizRadiusValue;
    private float _previousVertRadiusValue;
    private float _previousOffsetValue;
    private Axis _previousAxisValue;
    private float _previousPercentFilled;
    
    private float _segmentsInGap;
    
    private LineRenderer _line;

    [SerializeField] private float _percentFilled = .75f;
 
    void Start()
    {
        _line = gameObject.GetComponent<LineRenderer>();
 
        _segments = _segmentsInCircle - (int) (SkipAngle/360 * _segmentsInCircle);
        //_line.SetVertexCount(adjustedSegments + 1);
        _line.useWorldSpace = false;
 
        //UpdateValuesChanged();
       // _segmentsInGap = (int) (SkipAngle / 360 * _segments);
 
        SetPercentFilled(_percentFilled);
    }

    public void SetPercentFilled(float percent)
    {
        _percentFilled = percent;
        _previousPercentFilled = percent;
        
        CreatePoints();
    }
    
    void Update()
    {
        if (_previousPercentFilled != _percentFilled)
        {
            SetPercentFilled(_percentFilled);
        }
//        if (_checkValuesChanged)
//        {
//            if (_previousSegmentsValue != _segments ||
//                _previousHorizRadiusValue != _horizRadius ||
//                _previousVertRadiusValue != _vertRadius ||
//                _previousOffsetValue != _offset ||
//                _previousAxisValue != _axis)
//            {
//                CreatePoints();
//            }
// 
//            //UpdateValuesChanged();
//        }
    }
 
//    void UpdateValuesChanged()
//    {
//        _previousSegmentsValue = _segments;
//        _previousHorizRadiusValue = _horizRadius;
//        _previousVertRadiusValue = _vertRadius;
//        _previousOffsetValue = _offset;
//        _previousAxisValue = _axis;
//    }
// 
    void CreatePoints()
    {
        if (_percentFilled <= 0)
        {
            return;
        }
        
        int segmentsToFill = _segments - (int)((1f-_percentFilled) * _segments);
     
        _line.SetVertexCount(segmentsToFill + 1);
        
        float x;
        float y;
        float z = _offset;

        float angle = SkipAngle;
 
        for (int i = 0; i < (segmentsToFill + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * _horizRadius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * _vertRadius;
 
            switch(_axis)
            {
                case Axis.X: _line.SetPosition(i, new Vector3(z, y, x));
                    break;
                case Axis.Y: _line.SetPosition(i, new Vector3(y, z, x));
                    break;
                case Axis.Z: _line.SetPosition(i, new Vector3(x, y, z));
                    break;
                default:
                    break;
            }
 
            angle += (360f / _segmentsInCircle);
        }
    }
}