using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorIntensity : MonoBehaviour
{
    public Projector projector;
        [Range(0.1f, 5)]
        public float mininumSize = 0.35f;
        [Range(0.1f, 5)]
        public float maximumSize = 0.7f;

    public float currentTerrainHeight;


	void Start ()
    {
        if (projector == null)

        projector = transform.GetComponent<Projector>();
	}
	
	void LateUpdate ()
    {
        //Get current Terrain + height of it at current position
        Vector3 pos = transform.position;
        pos.y = Terrain.activeTerrain.SampleHeight(transform.position);

        projector.orthographicSize = currentTerrainHeight / 2f;

        currentTerrainHeight = pos.y;

        //Limit projector size
        projector.orthographicSize = Mathf.Clamp(projector.orthographicSize, mininumSize, maximumSize);
    }
}
