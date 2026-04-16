using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    public Image skill1DarkMask;
    public Text skill1Text;

    public Image skill2DarkMask;
    public Text skill2Text;

    public Image skill3DarkMask;
    public Text skill3Text;

    private void Start()
    {
        if (skill1DarkMask != null) skill1DarkMask.gameObject.SetActive(false);
        if (skill1Text != null) skill1Text.gameObject.SetActive(false);

        if (skill2DarkMask != null) skill2DarkMask.gameObject.SetActive(false);
        if (skill2Text != null) skill2Text.gameObject.SetActive(false);

        if (skill3DarkMask != null) skill3DarkMask.gameObject.SetActive(false);
        if (skill3Text != null) skill3Text.gameObject.SetActive(false);
    }

    public void UpdateSkillCooldown(int skillIndex, float currentCooldown, float maxCooldown)
    {
        Image mask = null;
        Text text = null;

        if (skillIndex == 1)
        {
            mask = skill1DarkMask;
            text = skill1Text;
        }
        else if (skillIndex == 2)
        {
            mask = skill2DarkMask;
            text = skill2Text;
        }
        else if (skillIndex == 3)
        {
            mask = skill3DarkMask;
            text = skill3Text;
        }

        if (mask == null || text == null) return;

        if (currentCooldown > 0)
        {
            if (!mask.gameObject.activeSelf) mask.gameObject.SetActive(true);
            if (!text.gameObject.activeSelf) text.gameObject.SetActive(true);

            mask.fillAmount = currentCooldown / maxCooldown;
            text.text = Mathf.CeilToInt(currentCooldown).ToString();
        }
        else
        {
            if (mask.gameObject.activeSelf) mask.gameObject.SetActive(false);
            if (text.gameObject.activeSelf) text.gameObject.SetActive(false);
        }
    }
}