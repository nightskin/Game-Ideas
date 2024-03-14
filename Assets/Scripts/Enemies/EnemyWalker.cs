using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalker : MonoBehaviour
{
    [SerializeField] Transform leftFootTarget;
    [SerializeField] Transform rightFootTarget;

    [SerializeField] AnimationCurve horizontalCurve;
    [SerializeField] AnimationCurve verticalCurve;

    [SerializeField][Range(0, 1)] float horDistance = 0.5f;
    [SerializeField][Range(0, 1)] float vertDistance = 0.5f;

    private Vector3 leftFootOffset;
    private Vector3 rightFootOffset;

    private float leftLegLast;
    private float rightLegLast;

    void Start()
    {
        leftFootOffset = leftFootTarget.localPosition;
        rightFootOffset = rightFootTarget.localPosition;
    }

    
    void Update()
    {
        float leftLegForwardMovment = horizontalCurve.Evaluate(Time.time);
        float rightLegForwardMovment = horizontalCurve.Evaluate(Time.time - 1);

        leftFootTarget.localPosition = leftFootOffset +
            this.transform.InverseTransformVector(leftFootTarget.forward) * leftLegForwardMovment  * horDistance +
            this.transform.InverseTransformVector(leftFootTarget.up) * verticalCurve.Evaluate(Time.time + 0.5f) * vertDistance;
        
        rightFootTarget.localPosition = rightFootOffset +
            this.transform.InverseTransformVector(rightFootTarget.forward) * rightLegForwardMovment * horDistance +
            this.transform.InverseTransformVector(rightFootTarget.up) * verticalCurve.Evaluate(Time.time - 0.5f) * vertDistance;
    }
}
