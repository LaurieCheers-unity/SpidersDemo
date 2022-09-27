using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLegIK : MonoBehaviour
{
    public GameObject foot;
    public GameObject knee;
    public GameObject target;

    Vector3 targetStartOffset;
    float targetStartYaw;
    Coroutine animation;

    private void Start()
    {
        targetStartOffset = target.transform.position - transform.position;
        targetStartYaw = Quaternion.LookRotation(targetStartOffset).eulerAngles.y;
    }

    void Update()
    {
        if (!Physics.Raycast(new Ray(target.transform.position + Vector3.up, Vector3.down), out RaycastHit hit))
        {
            return;
        }
        Vector3 hitPoint = hit.point;

        // triangle: H-K-F(=target)
        float thighLength = (knee.transform.position - transform.position).magnitude;
        float shinLength = (foot.transform.position - knee.transform.position).magnitude;
        float targetDistance = (hitPoint - transform.position).magnitude;
        float cosKnee = ((shinLength * shinLength) + (thighLength * thighLength) - (targetDistance * targetDistance)) / (2 * shinLength * thighLength);
        float kneeAngle = 180 - Mathf.Acos(cosKnee)*Mathf.Rad2Deg;

        if ((float.IsNaN(kneeAngle) || Mathf.Abs(kneeAngle) < 45) && animation == null)
        {
            animation = StartCoroutine(AnimateTarget());
            return;
        }

        knee.transform.localRotation = Quaternion.Euler(kneeAngle, 0, 0);

        float cosHip = ((targetDistance * targetDistance) + (thighLength * thighLength) - (shinLength * shinLength)) / (2 * targetDistance * thighLength);
        float hipAngle = Mathf.Acos(cosHip) * Mathf.Rad2Deg;
        Quaternion baseRotation = Quaternion.LookRotation(hitPoint - transform.position);

        if(Mathf.Abs(Mathf.DeltaAngle(baseRotation.eulerAngles.y, targetStartYaw)) > 45 && animation == null)
        {
            animation = StartCoroutine(AnimateTarget());
            return;
        }

        transform.rotation = Quaternion.Euler(baseRotation.eulerAngles.x - hipAngle, baseRotation.eulerAngles.y, 0);
    }

    IEnumerator AnimateTarget()
    {
        float duration = 0.2f;
        float startTime = Time.time;
        float endTime = Time.time + duration;
        Vector3 startPoint = target.transform.position;
        while (Time.time < endTime)
        {
            float fraction = (Time.time - startTime)/duration;
            target.transform.position = Vector3.Lerp(startPoint, targetStartOffset + transform.position, fraction);
            yield return null;
        }
        target.transform.position = targetStartOffset + transform.position;
        animation = null;
    }
}
