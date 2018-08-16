using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorAbility : MonoBehaviour
{
    static public IndicatorAbility instance;

    [SerializeField] private float randomOffset = 5f;
    [SerializeField] private float maxDistance = 300f;
    private Image compassImage;
    private Image compassImageBackground;
    private float indicatorAngle;
    private IEnumerator pingCoroutine;
    private IEnumerator pingStartCoroutine;

    private GameObject player;   //  Start
    public Transform target;     //  Target

    public float targetTimer = 8f;
    public float currentTargetTime;
    public bool canSetNewTarget = true;

    void Start()
    {
        instance = this;

        if (NetworkGameManager.Instance.LocalCharacter.GetType() != typeof(Carnivore))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            player = GameObject.FindObjectOfType<Carnivore>().gameObject;

            compassImage = transform.GetChild(0).GetComponent<Image>();
            compassImageBackground = transform.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))    //  Activate
        {
            if(target)
            {
                SetNewPoint(target.transform.position);
            }
        }

        if (!canSetNewTarget)
        {
            currentTargetTime += 1 * Time.deltaTime;

        }

        if (currentTargetTime > targetTimer)    //  If bigger than targetTimer 
        {
            canSetNewTarget = true;
        }
        else
        {
            canSetNewTarget = false;
        }
    }

    public void SetNewPoint(Vector3 targetPoint)
    {
        if(canSetNewTarget)
        {
            //  Disable previous coroutine
            if (pingStartCoroutine != null)
            {
                StopCoroutine(pingStartCoroutine);
            }

            StartCoroutine(StartPing(player.transform, targetPoint));

            //  Reset Timer
            currentTargetTime = 0f;
            canSetNewTarget = false;
        }
    }

    IEnumerator StartPing(Transform player, Vector3 targetPoint)
    {
        float t = 0;
        while (t < Vector3.Distance(player.position, targetPoint))
        {
            t += Time.deltaTime * 20;
            yield return null;
        }
        if (pingCoroutine != null)
        {
            StopCoroutine(pingCoroutine);
        }

        pingCoroutine = SetNewPing(player, targetPoint);
        StartCoroutine(pingCoroutine);
    }

    IEnumerator SetNewPing(Transform player, Vector3 targetPoint)
    {
        float duration = 0.5f * Vector3.Distance(player.position, targetPoint);
        float timer = 0;
        float tempT = 5;

        compassImage.color = Color.red;

        while (timer < duration)
        {
            float distance = Vector3.Distance(player.position, targetPoint);
            duration = 0.2f * distance; //  set the duration to distance

            //  calculate the angles into 360 degrees
            timer += Time.deltaTime;

            if (Vector3.SignedAngle(player.position - targetPoint, player.forward, Vector3.up) < 0)
            {
                indicatorAngle = Vector3.Angle(player.forward, player.position - targetPoint);
            }
            else
            {
                indicatorAngle = 360 - Vector3.Angle(player.forward, player.position - targetPoint);
            }

            //  Update UI
            distance /= maxDistance;

            //  img1.color = Color.Lerp(img1.color, Color.clear, (tempT-duration) /duration * Time.deltaTime);

            compassImage.color = Color.Lerp(compassImage.color, Color.clear, 0.2f * (timer + tempT - duration) * Time.deltaTime);
            compassImage.transform.rotation = Quaternion.identity;
            compassImage.transform.Rotate(-Vector3.forward, indicatorAngle + 90 + distance / 2 * 360);
            compassImage.fillAmount = 0.5f - distance;

            //Debug.Log(Vector3.Angle(player.forward, player.position - targetPoint));
            yield return null;
        }
        compassImage.color = Color.clear;
    }
}