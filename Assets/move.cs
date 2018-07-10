using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public bool isMoving;
    public float AscendSpeed = 2f;
    public float Speed = 2f;
    public bool isMovingVertical;
    public bool isReversing;
    private Vector3 MovementInputVector;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {



    }
    void FixedUpdate()
    {
        Move();
    }


    protected void Move()
    {
        //tarkista peruuttaako
        isMoving = false;
        if (Input.GetAxisRaw("Vertical") < 0)
        {
            isReversing = true;

        }
        else
            isReversing = false;
        
        // laske liikkumisvektorit

        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * Speed) * Time.deltaTime;

        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * AscendSpeed) * Time.deltaTime;

        if (inputvectorZ.magnitude != 0)
        {
            isMoving = true;
            isMovingVertical = true;

        }
        
        //tarkista liikkuuko ylös alas
        if (inputvectorZ.magnitude != 0)
        {
            isMovingVertical = true;
        }
        else if (inputvectorZ.magnitude == 0)
        {
            isMovingVertical = false;
        }

        //liiku
        MovementInputVector = inputvectorY + inputvectorZ;
        transform.Translate(MovementInputVector);




    }
}