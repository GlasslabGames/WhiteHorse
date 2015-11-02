using UnityEngine;
using System.Collections;

public class ObjectAccessor : SingletonBehavior<ObjectAccessor> {
	public GameObject StatesContainer;
	public SpriteRenderer Background;
	public GameObject WorkerPrefab;
	public GameObject PlayerPrefab;
	public GameObject ParticlePrefab;
	public Timer HarvestTimer;
}
