using UnityEngine;
using System;
using MiniJSON;
using System.Collections.Generic;


public class PhysicsBot : MonoBehaviour
{
	public GameObject m_projectilePrefab;
	public GameObject m_blastPrefab;
	public Transform m_blastSpawnPoint;

	public float m_currentVelocity = 0.0f;
	public float m_accelerationRate = 1.0f;
	public float m_decelerationRate = 0.95f;
	public float m_maxVelocity = 4.0f;

	public float m_shootPower = 2250.0f;


	public void Update()
	{
		if( Input.GetKeyDown( KeyCode.Space ) )
		{
			FirePills();
		}

		if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			m_currentVelocity = m_currentVelocity - ( m_accelerationRate * Time.deltaTime );
			gameObject.transform.rotation = Quaternion.Euler( 0, 0, 0 );

			if( m_currentVelocity <= -m_maxVelocity )
			{
				m_currentVelocity = -m_maxVelocity;
      		}
		}
		else if( Input.GetKey( KeyCode.RightArrow ) )
		{
			m_currentVelocity = m_currentVelocity + ( m_accelerationRate * Time.deltaTime );
			gameObject.transform.rotation = Quaternion.Euler( 0, 180, 0 );

			if( m_currentVelocity >= m_maxVelocity )
			{
				m_currentVelocity = m_maxVelocity;
      		}
		}
		else
		{
			m_currentVelocity *= m_decelerationRate;
    	}

		rigidbody2D.velocity = new Vector2( m_currentVelocity, 0 );
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
	}
}