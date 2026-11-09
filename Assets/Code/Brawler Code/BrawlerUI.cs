
using UnityEngine;
using TMPro;

public class BrawlerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UniversalHiring hiring;
    [SerializeField] private BrawlerAI brawlerAI;

    [Header("UI")]
    [SerializeField] private GameObject skillSlot;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text cooldownText;

    private void Start()
    {
        if (skillSlot != null)
            skillSlot.SetActive(false);

        if (numberText != null)
            numberText.text = "1";

        if (cooldownText != null)
            cooldownText.text = "";
    }

    private void Update()
    {
        if (hiring == null || brawlerAI == null)
            return;

        // Check whether the Brawler has been hired.
        bool hired = hiring.IsHired;

        // Only show the skill slot after hiring.
        if (skillSlot != null &&
            skillSlot.activeSelf != hired)
        {
            skillSlot.SetActive(hired);
        }

        if (!hired)
            return;

        // Get the remaining skill cooldown.
        float remaining = brawlerAI.CooldownRemaining;

        if (cooldownText != null)
        {
            if (remaining > 0f)
            {
                cooldownText.text =
                    Mathf.CeilToInt(remaining).ToString();
            }
            else
            {
                cooldownText.text = "";
            }
        }
    }
}



