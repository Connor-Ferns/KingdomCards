using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState
    {
        PlayerTurn,
        AITurn
    }

    public TurnState currentState;
    public int turnNumber = 1;
    [SerializeField] private TMP_Text turnNumberVisual;

    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnAITurnStart;
    public event Action OnAITurnEnd;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnManager - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        
    }

    private void StartGame()
    {
        ChangeTurn(TurnState.PlayerTurn);
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn)
            return;

        OnPlayerTurnEnd?.Invoke();

        ChangeTurn(TurnState.AITurn);
    }

    public void ChangeTurn(TurnState newstate)
    {
        currentState = newstate;

        switch (currentState)
        {
            case TurnState.PlayerTurn:
                // What should happen when player turn starts?
                Debug.Log("Player Turn Started");
                OnPlayerTurnStart?.Invoke();
                break;
            case TurnState.AITurn:
                // What should happen when AI turn starts?
                Debug.Log("AI Turn Started");
                OnAITurnStart?.Invoke();

                StartCoroutine(ProcessAITurn());
                break;
        }
    }

    private IEnumerator ProcessAITurn()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("AI thinking... (does nothing yet)");
        OnAITurnEnd?.Invoke();

        turnNumber++;
        turnNumberVisual.text = "Wave: " + turnNumber.ToString();
        ChangeTurn(TurnState.PlayerTurn);
    }
}
