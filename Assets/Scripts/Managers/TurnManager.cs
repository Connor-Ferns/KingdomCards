using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public enum TurnState { PlayerTurn, AITurn }
    public TurnState currentState;
    public int turnNumber;
    public event Action OnTurnStart;
    public event Action OnTurnEnd;

    public void EndTurn()
    {
        OnTurnEnd?.Invoke();

        OnTurnStart?.Invoke();
    }
}
