using System.Collections;
using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [Header("Tekst-indstillinger")]
    [SerializeField] private float titleFontSize = 32f;
    [SerializeField] private float statFontSize = 20f;
    [SerializeField] private float statusFontSize = 24f;

    [Header("Overskrifter")]
    [SerializeField] private TMP_Text playerBoardTitle;
    [SerializeField] private TMP_Text opponentBoardTitle;

    [Header("Spillerens statistik")]
    [SerializeField] private TMP_Text playerShipsRemaining;
    [SerializeField] private TMP_Text playerPartsHit;

    [Header("Modstanderens statistik")]
    [SerializeField] private TMP_Text opponentShipsRemaining;
    [SerializeField] private TMP_Text opponentPartsHit;

    [Header("Status-bar")]
    [SerializeField] private GameObject statusContainer;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private float autoHideSeconds = 3f;

    private Coroutine hideCoroutine;

    void Awake()
    {
        if (playerBoardTitle != null) playerBoardTitle.text = "Player board";
        if (opponentBoardTitle != null) opponentBoardTitle.text = "Opponent board";

        ApplyFontSizes();

        // Start med skjult status
        if (statusContainer != null) statusContainer.SetActive(false);
    }

    void OnValidate()
    {
        ApplyFontSizes();
    }

    private void ApplyFontSizes()
    {
        SetSize(playerBoardTitle, titleFontSize);
        SetSize(opponentBoardTitle, titleFontSize);

        SetSize(playerShipsRemaining, statFontSize);
        SetSize(playerPartsHit, statFontSize);
        SetSize(opponentShipsRemaining, statFontSize);
        SetSize(opponentPartsHit, statFontSize);

        SetSize(statusText, statusFontSize);
    }

    private void SetSize(TMP_Text field, float size)
    {
        if (field != null) field.fontSize = size;
    }

    public void UpdateStats(BoardModel playerBoard, BoardModel opponentBoard)
    {
        if (playerBoard != null)
        {
            SetShipsRemaining(playerShipsRemaining, playerBoard);
            SetPartsHit(playerPartsHit, playerBoard);
        }
        if (opponentBoard != null)
        {
            SetShipsRemaining(opponentShipsRemaining, opponentBoard);
            SetPartsHit(opponentPartsHit, opponentBoard);
        }
    }

    // Midlertidig besked - forsvinder automatisk efter autoHideSeconds
    public void SetStatus(string message)
    {
        ShowStatus(message, autoHide: true);
    }

    // Permanent besked - bliver stående indtil den erstattes eller skjules manuelt
    public void SetStatusPermanent(string message)
    {
        ShowStatus(message, autoHide: false);
    }

    public void HideStatus()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        if (statusContainer != null) statusContainer.SetActive(false);
    }

    private void ShowStatus(string message, bool autoHide)
    {
        if (statusContainer == null || statusText == null) return;

        // Afbryd eventuel igangværende skjul-timer
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        statusText.text = message;
        statusContainer.SetActive(true);

        if (autoHide)
        {
            hideCoroutine = StartCoroutine(HideAfterDelay(autoHideSeconds));
        }
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (statusContainer != null) statusContainer.SetActive(false);
        hideCoroutine = null;
    }

    private void SetShipsRemaining(TMP_Text field, BoardModel board)
    {
        if (field == null) return;

        int total = board.Ships.Count;
        int sunk = 0;
        foreach (var ship in board.Ships)
            if (ship.IsSunk()) sunk++;

        field.text = $"Skibe tilbage: {total - sunk} / {total}";
    }

    private void SetPartsHit(TMP_Text field, BoardModel board)
    {
        if (field == null) return;

        int hit = 0;
        int total = 0;
        foreach (var ship in board.Ships)
        {
            total += ship.Parts.Count;
            foreach (var part in ship.Parts)
                if (part.IsHit) hit++;
        }

        field.text = $"Ramte dele: {hit} / {total}";
    }
}