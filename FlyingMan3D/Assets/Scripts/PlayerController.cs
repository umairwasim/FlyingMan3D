using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{
    public static List<PlayerController> players;

    [SerializeField] private float maxLaunchSpeed = 60f;
    [SerializeField] private float movementSpeed = 100f;
    [SerializeField] private float mobileSpeed = 10f;

    [SerializeField] private Transform capsule;
    [SerializeField] private FixedJoint joint;

    [HideInInspector] public bool isPassed;
    [HideInInspector] public Rigidbody[] bodies;

    private float xValue;
    private Vector3 initialPos;
    private float time;
    private GameObject SelfHips;

    private void Awake()
    {
        if (players == null || players.Count == 0)
        {
            players = new List<PlayerController>();
        }
    }

    void Start()
    {

        SelfHips = GameObject.FindGameObjectWithTag("Hips");
        //SelfHips = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        bodies = GetComponentsInChildren<Rigidbody>();
        initialPos = capsule.position;

        players.Add(this);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            if (GameManager.Instance.isGameStarted)
            {
                xValue = Input.GetAxis("Mouse X");

                foreach (Rigidbody rb in bodies)
                {
                    rb.velocity += movementSpeed * Time.deltaTime * new Vector3(xValue, 0, 0);
                }
            }
            else
            {
                GameManager.Instance.CloseTapText();
                GameManager.Instance.isGameStarted = true;
            }
        }

#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            if (GameManager.Instance.isGameStarted)
            {
                Touch touch = Input.GetTouch(0);
                TouchPhase phase = touch.phase;

                if (phase == TouchPhase.Moved)
                {
                    xValue = touch.deltaPosition.x;

                    foreach (Rigidbody rb in bodies)
                    {
                        rb.velocity += new Vector3(xValue, 0, 0) * Time.deltaTime * mobileSpeed;
                    }
                }
            }
            else
            {
                GameManager.Instance.CloseTapText();
                GameManager.Instance.isGameStarted = true;
            }
        }
#endif

        CheckForBoundaries();
    }

    private void CheckForBoundaries()
    {
        float xPos = SelfHips.transform.position.x;
        Vector3 selfVelocity = SelfHips.GetComponent<Rigidbody>().velocity;

        if (xPos >= 20f || xPos <= -20f)
        {
            float newX = Mathf.Sign(xPos) * -3f;
            selfVelocity = new Vector3(newX, selfVelocity.y, selfVelocity.z);
        }
    }

    public IEnumerator ApplyLaunchForce(float factor)
    {
        Vector3 targetPos = initialPos + new Vector3(0f, -1f, -4f) * factor;

        while (time <= 0.8f)
        {
            capsule.position = Vector3.Lerp(initialPos, targetPos, time / 0.8f);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;

        while (time <= 0.1f)
        {
            capsule.position = Vector3.Lerp(targetPos, initialPos, time / 0.2f);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(joint);

        Vector3 forceVector = new Vector3(0, factor, factor * 2f) * maxLaunchSpeed;

        foreach (Rigidbody rb in bodies)
        {
            rb.velocity = forceVector;
        }

        if (factor > 0.1f)
        {
            Spawner.Instance.SpawnObjects(bodies[0].velocity);
        }
    }

}
