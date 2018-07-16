using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]

public class ChargeAbility : MonoBehaviour {
    
    [SerializeField] private float coolDownTime;
    [SerializeField] private float chargeTime = 4;
    [SerializeField] private GameObject carnivore;
    [SerializeField] private Camera playerCam;
    Coroutine chargeRoutine;
    private float defaultFov;
    private bool onCooldown = false;
    private bool hitTarget = false;
    private bool charging = false;
    private float momentumTimer = 0;

    private void Start()
    {
        defaultFov = playerCam.fieldOfView;
    }


    private void Update() //FOR TESTING
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Charge(10);
        }
    }


    public void Charge(float speed)
    {
        if (onCooldown)
        {
            return;
        }
        else
        {
            chargeRoutine = StartCoroutine(ChargeForward(speed));
            StartCoroutine(ChargeCooldown());
            charging = true;
        }
    }


    private IEnumerator ChargeForward(float speed)
    {
        momentumTimer = 0;
        while (momentumTimer < chargeTime || hitTarget)
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, playerCam.fieldOfView + 2 * momentumTimer, 10 * Time.deltaTime);
            carnivore.gameObject.transform.Translate(Vector3.forward * momentumTimer * speed * Time.deltaTime);
            playerCam.fieldOfView = 
            momentumTimer += Time.deltaTime;
            yield return null;
        }
        charging = false;
        hitTarget = false;

    }
    
    private IEnumerator RestoreFov()
    {
        while (playerCam.fieldOfView > defaultFov + 2)
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defaultFov, Time.deltaTime);
            yield return null;
        }
        playerCam.fieldOfView = defaultFov;
    }

    private IEnumerator ChargeCooldown()
    {
        onCooldown = true;
        charging = true;
        yield return new WaitForSeconds(coolDownTime);
        onCooldown = false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && charging)
        {
            Debug.Log("osu");
            if ((momentumTimer / chargeTime) >= 0.4f)
            {
                //Kill target
                col.gameObject.GetComponent<Herbivore>().GetEaten(500);
            }
            else
            {
                //1 damage
                col.gameObject.GetComponent<Herbivore>().GetEaten(1);
            }
            hitTarget = true;
        }
        else if (col.tag == "Obstacle" && charging)
        {
            Debug.Log("osu");
            hitTarget = true;
            //Stun for 1.5s
        }
    }

}
