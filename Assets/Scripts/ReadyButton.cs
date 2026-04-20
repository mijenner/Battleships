using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReadyButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void Update()
    {
        // Vis kun knappen under Placing-state
        if (gameManager == null) return;
        bool shouldBeVisible = gameManager.State == GameState.Placing;
        if (gameObject.activeSelf != shouldBeVisible)
            gameObject.SetActive(shouldBeVisible);
    }

    private void OnClick()
    {
        gameManager.PlayerReady();
    }
}
