using UnityEngine;
using System.Collections;

public class SlashPivotSwing : MonoBehaviour
{
    public enum SwingType { Slash, Lunge, Low }

    [Header("Slash — горизонтальный удар")]
    public float slashWindup  = 30f;
    public float slashAttack  = -80f;
    public float slashReturn  = 0f;

    [Header("Lunge — выпад вперёд")]
    public float lungeWindup  = 20f;
    public float lungeAttack  = -20f;
    public float lungeReturn  = 0f;

    [Header("Low — удар снизу")]
    public float lowWindup    = -60f;
    public float lowAttack    = 30f;
    public float lowReturn    = 0f;

    [Header("Timing")]
    public float windupTime = 0.06f;
    public float attackTime = 0.08f;
    public float returnTime = 0.12f;

    bool swinging;

    public void PlaySwing(SwingType type = SwingType.Slash)
    {
        if (swinging) return;
        StartCoroutine(SwingRoutine(type));
    }

    IEnumerator SwingRoutine(SwingType type)
    {
        swinging = true;

        float windup, attack, ret;

        switch (type)
        {
            case SwingType.Lunge:
                windup = lungeWindup; attack = lungeAttack; ret = lungeReturn;
                break;
            case SwingType.Low:
                windup = lowWindup; attack = lowAttack; ret = lowReturn;
                break;
            default:
                windup = slashWindup; attack = slashAttack; ret = slashReturn;
                break;
        }

        yield return RotateTo(windup, windupTime);
        yield return RotateTo(attack, attackTime);
        yield return RotateTo(ret,    returnTime);

        swinging = false;
    }

    IEnumerator RotateTo(float target, float time)
    {
        float start = NormalizeAngle(transform.localEulerAngles.z);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, time);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(start, target, t));
            yield return null;
        }
    }

    float NormalizeAngle(float z) => z > 180f ? z - 360f : z;
}