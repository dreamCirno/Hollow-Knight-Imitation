using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowBug : MonoBehaviour
{
    public Vector2 moveSpot;
    [Header("参数")]
    public float speed;
    [SerializeField] int minX;
    [SerializeField] int maxX;
    [SerializeField] int minY;
    [SerializeField] int maxY;
    [SerializeField] float minWaitTime;
    [SerializeField] float maxWaitTime;

    private float waitTime;
    private Vector3 initSpot;

    private void Start()
    {
        initSpot = transform.position;
        RandomAll();
    }

    private void FixedUpdate()
    {
        if (Vector2.Distance(transform.position, moveSpot) < 0.05f)
        {
            if (waitTime <= 0)
            {
                RandomAll();
            }
            else
            {
                waitTime -= Time.fixedDeltaTime;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, moveSpot, speed * Time.fixedDeltaTime);
        }
    }

    void RandomAll()
    {
        RandomWaitTime();
        RandomPosition();
    }

    void RandomPosition()
    {
        moveSpot = new Vector2(initSpot.x + Random.Range(minX, maxX), initSpot.y + Random.Range(minY, maxY));
    }

    void RandomWaitTime()
    {
        waitTime = Random.Range(minWaitTime, maxWaitTime);
    }
}
