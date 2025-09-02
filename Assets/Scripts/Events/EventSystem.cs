using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EventSystem : MonoBehaviour
{
    [Header("Event Settings")]
    public List<GameEvent> availableEvents = new List<GameEvent>();
    public float eventCooldown = 30f;
    
    private float lastEventTime;
    private GameEvent currentEvent;
    private bool isEventActive = false;
    
    public System.Action<GameEvent> OnEventStarted;
    public System.Action<GameEvent, bool> OnEventCompleted;
    
    private void Start()
    {
        lastEventTime = Time.time;
    }
    
    private void Update()
    {
        if (!isEventActive && Time.time - lastEventTime >= eventCooldown)
        {
            TriggerRandomEvent();
        }
    }
    
    private void TriggerRandomEvent()
    {
        if (availableEvents.Count == 0) return;
        
        GameEvent randomEvent = availableEvents[Random.Range(0, availableEvents.Count)];
        StartEvent(randomEvent);
    }
    
    public void StartEvent(GameEvent gameEvent)
    {
        currentEvent = gameEvent;
        isEventActive = true;
        lastEventTime = Time.time;
        
        GameManager.Instance.ChangeGameState(GameState.Event);
        OnEventStarted?.Invoke(currentEvent);
        
        // Start event timer
        StartCoroutine(EventTimer(currentEvent.timeLimit));
    }
    
    private IEnumerator EventTimer(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);
        
        if (isEventActive)
        {
            // Time's up - player failed
            CompleteEvent(false);
        }
    }
    
    public void PlayerRespondedToEvent(bool correctResponse)
    {
        if (!isEventActive) return;
        
        CompleteEvent(correctResponse);
    }
    
    private void CompleteEvent(bool success)
    {
        isEventActive = false;
        
        if (success)
        {
            // Reward player
            GameManager.Instance.AdvanceStory();
            GameManager.Instance.AddGreed(currentEvent.greedReward);
        }
        else
        {
            // Penalize player
            GameManager.Instance.AddSuspicion(currentEvent.suspicionPenalty);
        }
        
        OnEventCompleted?.Invoke(currentEvent, success);
        GameManager.Instance.ChangeGameState(GameState.Playing);
        
        currentEvent = null;
    }
}

[System.Serializable]
public class GameEvent
{
    public string eventName;
    public string eventDescription;
    public EventType eventType;
    public float timeLimit = 10f;
    public float greedReward = 5f;
    public float suspicionPenalty = 10f;
    public string correctAction;
}

public enum EventType
{
    Transaction,
    Stealth,
    Dialog,
    Puzzle
}
