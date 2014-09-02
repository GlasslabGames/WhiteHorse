using UnityEngine;
using System;
using System.Collections.Generic;


public class PhysicsBot : MonoBehaviour
{
	public GameObject m_projectilePrefab;
	public GameObject m_blastPrefab;
	public Transform m_blastSpawnPoint;

	private bool m_facingRight = false;

	private float m_currentVelocity = 0.0f;
	public float m_maxVelocity = 25.0f;

	public float m_shootPower = 2250.0f;
	public float m_recoilForce = 100.0f;


	public void Update()
	{
		if( Input.GetKeyDown( KeyCode.Space ) )
		{
			FirePills();
		}

		if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			m_currentVelocity = -m_maxVelocity;
			gameObject.transform.rotation = Quaternion.Euler( 0, 0, 0 );
			m_facingRight = false;
		}
		else if( Input.GetKey( KeyCode.RightArrow ) )
		{
			m_currentVelocity = m_maxVelocity;
			gameObject.transform.rotation = Quaternion.Euler( 0, 180, 0 );
			m_facingRight = true;
		}
		else
		{
			m_currentVelocity = 0.0f;
    	}

		rigidbody2D.AddRelativeForce( Vector2.right * m_currentVelocity );
	}

	public void FirePills()
	{
		//Transform pills = Instantiate( m_pillsPrefab, m_pillsSpawnPoint.position, Quaternion.identity ) as Transform;
		//pills.rigidbody2D.AddForce( m_pillsSpawnPoint.transform.up * m_shootPower );

		GameObject projectile = Instantiate( m_projectilePrefab, m_blastSpawnPoint.position, Quaternion.identity ) as GameObject;
		projectile.transform.rotation = m_blastSpawnPoint.rotation;

		GameObject blast = Instantiate( m_blastPrefab, m_blastSpawnPoint.position, Quaternion.identity ) as GameObject;
		blast.transform.parent = m_blastSpawnPoint;
		//blast.transform.rotation = m_blastSpawnPoint.rotation;

		// Create an impulse on the bot backward
		rigidbody2D.AddRelativeForce( ( m_facingRight ? -Vector2.right : Vector2.right ) * m_recoilForce, ForceMode2D.Impulse );
	}
}