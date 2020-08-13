using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector = new Vector3(10.0f, 10.0f, 10.0f);
    [SerializeField] float period = 2.0f;
    private float movementFactor; // 0 for not moved, -1 or 1 for fully moved.

    [Range(0, 2)]
    [SerializeField]
    private float oscillatorVelocityFactor = 0.8f;

    private Vector3 initPos;

    // Start is called before the first frame update
    void Start()
    {
        initPos = this.transform.position;    
    }

    // Update is called once per frame
    void Update()
    {
        if (period == Mathf.Epsilon) return;
        float cycles = (Time.time / period) * oscillatorVelocityFactor; //grows from 0 through runtime

        const float tau = 2.0f * Mathf.PI;
        float rawSineMagnitude = Mathf.Sin(cycles * tau);

        //for moving obstacles that init one extreme of their oscillation paths
        //movementFactor = rawSineMagnitude / 2.0f + 0.5f;

        //for moving obstacles that init at the centre of their oscillation paths
        movementFactor = rawSineMagnitude;

        Vector3 offset = movementVector * movementFactor;
        transform.position = initPos + offset;
    }
}
