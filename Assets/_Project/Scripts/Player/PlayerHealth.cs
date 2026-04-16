using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Image healthBarFill;

    public Image gameOverScreen;
    public float fadeDuration = 1.5f;
    public float restartDelay = 2f;

    public bool IsDead { get; private set; }

    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        UpdateHealthUI();

        if (gameOverScreen != null)
        {
            gameOverScreen.gameObject.SetActive(false);
            Color c = gameOverScreen.color;
            c.a = 0f;
            gameOverScreen.color = c;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        IsDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.gameObject.SetActive(true);
            Color color = gameOverScreen.color;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                color.a = Mathf.Lerp(0, 1, t / fadeDuration);
                gameOverScreen.color = color;
                yield return null;
            }

            color.a = 1;
            gameOverScreen.color = color;
        }

        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}