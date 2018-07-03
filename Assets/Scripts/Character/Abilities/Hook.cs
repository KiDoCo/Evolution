using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour {


    private Vector3 direction;
    private float speed = 0;
    private GameObject parentObj;
    private GameObject hitObject;

    public bool hitTarget;
    [SerializeField]private string targetTag;


	void Start ()
    {
		
	}

    public void SetValues(GameObject _parent, Vector3 _direction, float _speed)
    {
        parentObj = _parent;
        direction = _direction;
        speed = _speed;
    }
	

	void Update ()
    {
        if (hitTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, parentObj.transform.position, 10 * Time.deltaTime);
            hitObject.transform.position = transform.position;
        }
        else
        {
            transform.Translate(direction * Time.deltaTime * speed);
        }
	}

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == targetTag)
        {
            hitObject = col.gameObject;
            transform.position = hitObject.transform.position;
            hitTarget = true;
            Debug.Log("osu");
        }
    }
}
