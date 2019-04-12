using System.Collections.Generic;
using System.Threading.Tasks;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BurnerOnPhysicalNotif : PhysicalNotification
{
	public BurnerOnVisualizer _visualizer;
	// Use this for initialization
	public override void Start ()
	{
		base.Start();
	}
	
	// Update is called once per frame
	// ReSharper disable once RedundantOverriddenMember
	public override void Update ()
	{
		base.Update();
	}
	
	public override async Task Launch(int delay = 1000) 
	{	
		_visualizer.Show();
		base.Launch();
	}
    
	public override Tween Hide()
	{
		_visualizer.Hide();
		return base.Hide();
	}
}
