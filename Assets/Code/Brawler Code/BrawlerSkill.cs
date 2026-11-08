
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrawlerSkill : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BrawlerHiring hiring;
    [SerializeField] private BrawlerAI brawlerAI;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform model;

    [Header("Skill Settings")]
    [SerializeField] private KeyCode skillKey = KeyCode.Alpha1;
    [SerializeField] private float cooldown = 30f;
    [SerializeField] private float skillDamage = 50f;
    [SerializeField] private float impactRadius = 3f;
    [SerializeField] private float smashAnimationDuration = 0.8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.8f;

    private float nextSkillTime;
    private bool isUsingSkill;

    public float CooldownRemaining =>
        Mathf.Max(0f, nextSkillTime - Time.time);

    private void Awake()
    {
        if (hiring == null)
            hiring = GetComponent<BrawlerHiring>();

        if (brawlerAI == null)
            brawlerAI = GetComponent<BrawlerAI>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (model == null && animator != null)
            model = animator.transform;
    }

    private void Update()
    {
        if (hiring == null || !hiring.IsHired)
            return;

        if (isUsingSkill)
            return;

        if (Time.time < nextSkillTime)
            return;

        if (Input.GetKeyDown(skillKey))
        {
            StartCoroutine(JumpAndSmash());
        }
    }

    private IEnumerator JumpAndSmash()
    {
        if (model == null)
            yield break;

        isUsingSkill = true;

        nextSkillTime = Time.time + cooldown;

        if (brawlerAI != null)
            brawlerAI.SetUsingSkill(true);

        Vector3 startingPosition = model.localPosition;

        float elapsedTime = 0f;

        // Jump up and return to the original height.
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / jumpDuration);

            float height =
                Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            model.localPosition =
                startingPosition + Vector3.up * height;

            yield return null;
        }

        model.localPosition = startingPosition;

        // Play the smash animation.
        if (animator != null)
            animator.SetTrigger("Attack");

        // Damage nearby enemies.
        DealImpactDamage();

        Debug.Log("Brawler used Jump and Smash!");

        // Keep AI paused while the smash animation plays.
        yield return new WaitForSeconds(smashAnimationDuration);

        isUsingSkill = false;

        if (brawlerAI != null)
            brawlerAI.SetUsingSkill(false);
    }

    private void DealImpactDamage()
    {
        Collider[] hitObjects =
            Physics.OverlapSphere(
                transform.position,
                impactRadius
            );

        HashSet<HealthSystem> damagedEnemies =
            new HashSet<HealthSystem>();

        foreach (Collider hitObject in hitObjects)
        {
            if (!hitObject.CompareTag("Enemy"))
                continue;

            HealthSystem health =
                hitObject.GetComponentInParent<HealthSystem>();

            if (health == null || health.IsDead)
                continue;

            if (damagedEnemies.Add(health))
            {
                health.TakeDamage(skillDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            impactRadius
        );
    }
}


