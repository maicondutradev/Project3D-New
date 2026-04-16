using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    public Image skill1DarkMask;
    public Text skill1Text;

    public Image skill2DarkMask;
    public Text skill2Text;

    private void Start()
    {
        // Garante que os ícones de cooldown comecem desativados quando o jogo iniciar
        skill1DarkMask.gameObject.SetActive(false);
        skill1Text.gameObject.SetActive(false);

        skill2DarkMask.gameObject.SetActive(false);
        skill2Text.gameObject.SetActive(false);
    }

    public void UpdateSkillCooldown(int skillIndex, float currentCooldown, float maxCooldown)
    {
        Image mask = skillIndex == 1 ? skill1DarkMask : skill2DarkMask;
        Text text = skillIndex == 1 ? skill1Text : skill2Text;

        if (currentCooldown > 0)
        {
            // Liga a imagem e o texto se eles estiverem desligados
            if (!mask.gameObject.activeSelf) mask.gameObject.SetActive(true);
            if (!text.gameObject.activeSelf) text.gameObject.SetActive(true);

            mask.fillAmount = currentCooldown / maxCooldown;
            text.text = Mathf.CeilToInt(currentCooldown).ToString();
        }
        else
        {
            // Desliga a imagem e o texto quando o tempo acaba
            if (mask.gameObject.activeSelf) mask.gameObject.SetActive(false);
            if (text.gameObject.activeSelf) text.gameObject.SetActive(false);
        }
    }
}