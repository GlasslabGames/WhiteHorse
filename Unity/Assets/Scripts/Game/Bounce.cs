using UnityEngine;
using System;
using System.Collections.Generic;


public class Bounce : MonoBehaviour
{
	public void OnCollisionEnter2D( Collision2D collider )
	{
	}

	public void OnTriggerEnter2D( Collider2D collider )
	{
		if( collider.tag == "Wall" )
		{
			// Bounce

		}
	}
}