using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    EnemyController[] enemies;
    public UIHandler uiHandler;

    // Спробувати більше: щоб Invoke не ставив перезапуск у чергу щокадру
    bool gameOver = false;

    void Start()
    {
        enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (gameOver) return;

        if (player.health <= 0)
        {
            uiHandler.DisplayLoseScreen();
            gameOver = true;
            Invoke(nameof(ReloadScene), 3f);
        }

        if (AllEnemiesFixed())
        {
            uiHandler.DisplayWinScreen();
            gameOver = true;
            Invoke(nameof(ReloadScene), 3f);
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool AllEnemiesFixed()
    {
        foreach (EnemyController enemy in enemies)
        {
            if (enemy.isBroken) return false;
        }
        return true;
    }
}
