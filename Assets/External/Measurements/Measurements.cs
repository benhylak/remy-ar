using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeasurementUnits
{
	centimeters, meters, kilometers, yards, inches, feet, miles
}
public enum MeasurementSources
{
	Renderer, Mesh, Collider
}

[ExecuteInEditMode]
public class Measurements : MonoBehaviour {

	/* *********************************************************************************
	 *									Settings
	 * *********************************************************************************/

		// Public
	public MeasurementUnits MeasurementUnit = MeasurementUnits.meters;
	public MeasurementSources MeasurementSource = MeasurementSources.Renderer;
	public GameObject DistanceObject;
		// Private
	private Bounds bounds;
	[HideInInspector] public bool showSourceError = false;

	/* *********************************************************************************
	 *									Calculations
	 *									
	 * Calculated variables for inspector viewing and for external reference. Can be used
	 * for realtime dimension and distance measurements. See below for explanation of vars.
	 * 
	 * {variable}_meters : The dimension/length in meters, aka unity units
	 * {variable}		 : The dimension/length in preferred measurement unit
	 * {variable}_string : The dimension/length in preferred measurement unit, for labels
	 * *********************************************************************************/

		// Dimensions
	[HideInInspector] public float width_meters;
	[HideInInspector] public float height_meters;
	[HideInInspector] public float depth_meters;
	[HideInInspector] public float width;
	[HideInInspector] public float height;
	[HideInInspector] public float depth;
	[HideInInspector] public string width_string;
	[HideInInspector] public string height_string;
	[HideInInspector] public string depth_string;
		// Distance
	[HideInInspector] public float center_to_center_meters;			//distance in meters
	[HideInInspector] public float center_to_center;
	[HideInInspector] public string center_to_center_string;
	[HideInInspector] public float edge_to_edge_meters;
	[HideInInspector] public float edge_to_edge;
	[HideInInspector] public string edge_to_edge_string;

	/* *********************************************************************************
	 *									Conversions
	 * *********************************************************************************/

	private float meter_to_feet = 3.28084f;
	private float meter_to_centimeters = 100f;
	private float meter_to_kilometers = 0.001f;
	private float meter_to_inches = 39.3701f;
	private float meter_to_yards = 1.09361f;
	private float meter_to_miles = 0.000621371f;

	/* *********************************************************************************
	 *								   API Functions
	 * *********************************************************************************/
		
		// Measurement Units
	public MeasurementUnits getMeasurementUnit(){ return MeasurementUnit; }
	public void setMeasurementUnit(MeasurementUnits unit){ MeasurementUnit = unit; }
		// Measurement Source
	public MeasurementSources getMeasurementSource() { return MeasurementSource; }
	public void setMeasurementSource(MeasurementSources source) { MeasurementSource = source; }
		// Distance Object
	public GameObject getDistanceObject() { return DistanceObject; }
	public void setDistanceObject(GameObject obj) { DistanceObject = obj; }
		// Calculated Values
	public Vector3 getDimensions(){ return new Vector3(width, height, depth); }
	public Vector3 getDimensionsInMeters() { return new Vector3(width_meters, height_meters, depth_meters); }
	public Vector2 getDistance() { return new Vector2(center_to_center, edge_to_edge); }
	public Vector2 getDistanceInMeters() { return new Vector2(center_to_center_meters, edge_to_edge_meters); }
		// Conversion Values
	public float getConversionValue(MeasurementUnits unit)
	{
		switch (unit)
		{
			case (MeasurementUnits.centimeters):
				return meter_to_centimeters;
			case (MeasurementUnits.kilometers):
				return meter_to_kilometers;
			case (MeasurementUnits.yards):
				return meter_to_yards;
			case (MeasurementUnits.inches):
				return meter_to_inches;
			case (MeasurementUnits.feet):
				return meter_to_feet;
			case (MeasurementUnits.miles):
				return meter_to_miles;
			default:
				return 1;
		}
	}


	/* *********************************************************************************
	 *								Internal Functions
	 * *********************************************************************************/

	void runCalculations()
	{
		showSourceError = false;
		if (MeasurementSource == MeasurementSources.Renderer)
		{
			if(GetComponent<Renderer>() != null)
				bounds = GetComponent<Renderer>().bounds;
			else
				showSourceError = true;
		}
		
		if (MeasurementSource == MeasurementSources.Mesh)
		{
			if(GetComponent<Mesh>() != null)
				bounds = GetComponent<Mesh>().bounds;
			else
				showSourceError = true;
		}
		
		if (MeasurementSource == MeasurementSources.Collider)
		{
			if(GetComponent<Collider>() != null)
				bounds = GetComponent<Collider>().bounds;
			else
				showSourceError = true;
		}

		width_meters = bounds.size.x;
		height_meters = bounds.size.y;
		depth_meters = bounds.size.z;

		width = unitConversion(width_meters);
		height = unitConversion(height_meters);
		depth = unitConversion(depth_meters);

		width_string = unitConversionString(width);
		height_string = unitConversionString(height);
		depth_string = unitConversionString(depth);

		if (DistanceObject != null)
		{
			center_to_center_meters = Vector3.Distance(DistanceObject.transform.position, transform.position);
			center_to_center = unitConversion(center_to_center_meters);
			center_to_center_string = unitConversionString(center_to_center);

			Vector3 point1 = bounds.ClosestPoint(DistanceObject.transform.position);
			Vector3 point2 = DistanceObject.transform.position;
			if (MeasurementSource == MeasurementSources.Renderer && DistanceObject.GetComponent<Renderer>() != null)
				point2 = DistanceObject.GetComponent<Renderer>().bounds.ClosestPoint(point1);
			if (MeasurementSource == MeasurementSources.Mesh && DistanceObject.GetComponent<Mesh>() != null)
				point2 = DistanceObject.GetComponent<Mesh>().bounds.ClosestPoint(point1);
			if (MeasurementSource == MeasurementSources.Collider && DistanceObject.GetComponent<Collider>() != null)
				point2 = DistanceObject.GetComponent<Collider>().bounds.ClosestPoint(point1);

			edge_to_edge_meters = Vector3.Distance(point1, point2);
			edge_to_edge = unitConversion(edge_to_edge_meters);
			edge_to_edge_string = unitConversionString(edge_to_edge);
		}
	}

	string unitConversionString(float val)
	{
		switch (MeasurementUnit)
		{
			case (MeasurementUnits.centimeters):
				return val.ToString() + " centimeters";
			case (MeasurementUnits.kilometers):
				return val.ToString() + " kilometers";
			case (MeasurementUnits.yards):
				return val.ToString() + " yards";
			case (MeasurementUnits.inches):
				return val.ToString() + " inches";
			case (MeasurementUnits.feet):
				return val.ToString() + " feet";
			case (MeasurementUnits.miles):
				return val.ToString() + " miles";
			default:
				return val.ToString() + " meters";
		}
	}
	float unitConversion(float val)
	{
		switch (MeasurementUnit)
		{
			case (MeasurementUnits.centimeters):
				return val * meter_to_centimeters;
			case (MeasurementUnits.kilometers):
				return val * meter_to_kilometers;
			case (MeasurementUnits.yards):
				return val * meter_to_yards;
			case (MeasurementUnits.inches):
				return val * meter_to_inches;
			case (MeasurementUnits.feet):
				return val * meter_to_feet;
			case (MeasurementUnits.miles):
				return val * meter_to_miles;
			default:
				return val;
		}
	}

	void Update()
	{
		runCalculations();
	}
}