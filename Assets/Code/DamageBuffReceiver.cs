using UnityEngine;

public class DamageBuffReceiver : MonoBehaviour
{
    private float damageMultiplier = 1f;
    private float buffEndTime;

    public float DamageMultiplier
    {
        get
        {
            if (Time.time >= buffEndTime)
                return 1f;

            return damageMultiplier;
        }
    }

    public bool IsBuffed => Time.time < buffEndTime;

    public void ApplyDamageBuff(float multiplier, float duration)
    {
        if (multiplier <= 1f || duration <= 0f)
            return;

        // Refresh the buff instead of stacking it.
        damageMultiplier = multiplier;
        buffEndTime = Time.time + duration;

        Debug.Log(gameObject.name + " received a damage buff!");
    }

    private void Update()
    {
        if (Time.time >= buffEndTime)
        {
            damageMultiplier = 1f;
        }
    }
}
