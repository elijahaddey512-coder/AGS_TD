
using UnityEngine;
using TMPro;

public class BrawlerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BrawlerHiring hiring;
    [SerializeField] private BrawlerSkill skill;

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
    }

    private void Update()
    {
        if (hiring == null || skill == null)
            return;

        bool hired = hiring.IsHired;

        if (skillSlot != null &&
            skillSlot.activeSelf != hired)
        {
            skillSlot.SetActive(hired);
        }

        if (!hired)
            return;

        float remaining = skill.CooldownRemaining;

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


