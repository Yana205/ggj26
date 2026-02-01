using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GrandmaSpeechBubble : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMP_Text speechText;
    [SerializeField] GameObject bubbleContainer;
    [SerializeField] float messageDisplayTime = 2.5f;
    [SerializeField] float fadeTime = 0.5f;

    [Header("Offset")]
    [SerializeField] Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);

    [Header("Messages")]
    [SerializeField] string[] feedingMessages = new string[]
    {
        "Here you go, little one!",
        "Eat up, {cat}!",
        "Hungry, aren't you, {cat}?",
        "There's a good kitty!",
        "Enjoy your meal, {cat}!"
    };

    [SerializeField] string[] alreadyFedMessages = new string[]
    {
        "Wait... I already fed you!",
        "Hey! You're that same cat!",
        "Nice try, but I remember you!",
        "I'm not THAT forgetful!",
        "You greedy little thing!"
    };

    [SerializeField] string[] noDisguiseMessages = new string[]
    {
        "I already fed you today!",
        "No more food for you!",
        "Come back tomorrow!",
        "You've had enough!"
    };

    [SerializeField] string[] idleThoughts = new string[]
    {
        "Such lovely cats today...",
        "I wonder who's hungry...",
        "Let me remember who I fed...",
        "*humming*"
    };

    CanvasGroup canvasGroup;
    Coroutine currentMessageCoroutine;
    Queue<string> messageQueue = new Queue<string>();
    bool isShowingMessage;

    void Start()
    {
        // Get or add CanvasGroup for fading
        if (bubbleContainer != null)
        {
            canvasGroup = bubbleContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bubbleContainer.AddComponent<CanvasGroup>();
            }
            bubbleContainer.SetActive(false);
        }

        // Start occasional idle thoughts
        StartCoroutine(IdleThoughtsRoutine());
    }

    void LateUpdate()
    {
        // Keep bubble above Grandma
        if (bubbleContainer != null)
        {
            bubbleContainer.transform.position = transform.position + bubbleOffset;
        }
    }

    public void ShowFeedingMessage(string catName)
    {
        string message = feedingMessages[Random.Range(0, feedingMessages.Length)];
        message = message.Replace("{cat}", catName);
        QueueMessage(message);
    }

    public void ShowAlreadyFedMessage(string catName)
    {
        string message = alreadyFedMessages[Random.Range(0, alreadyFedMessages.Length)];
        message = message.Replace("{cat}", catName);
        QueueMessage(message);
    }

    public void ShowNoDisguiseMessage()
    {
        string message = noDisguiseMessages[Random.Range(0, noDisguiseMessages.Length)];
        QueueMessage(message);
    }

    public void ShowCustomMessage(string message)
    {
        QueueMessage(message);
    }

    /// <summary>Shows message instantly, clearing queue and overriding any current text.</summary>
    public void ShowMessageImmediately(string message)
    {
        if (speechText == null || bubbleContainer == null) return;

        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }
        messageQueue.Clear();

        speechText.text = message;
        bubbleContainer.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        isShowingMessage = true;
        currentMessageCoroutine = StartCoroutine(ImmediateMessageRoutine());
    }

    IEnumerator ImmediateMessageRoutine()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1 - (elapsed / fadeTime);
                yield return null;
            }
        }
        bubbleContainer.SetActive(false);
        isShowingMessage = false;
        currentMessageCoroutine = null;
    }

    void QueueMessage(string message)
    {
        messageQueue.Enqueue(message);
        if (!isShowingMessage)
        {
            currentMessageCoroutine = StartCoroutine(ProcessMessageQueue());
        }
    }

    IEnumerator ProcessMessageQueue()
    {
        isShowingMessage = true;

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            yield return StartCoroutine(ShowMessageRoutine(message));
        }

        isShowingMessage = false;
        currentMessageCoroutine = null;
    }

    IEnumerator ShowMessageRoutine(string message)
    {
        if (speechText == null || bubbleContainer == null) yield break;

        // Show bubble
        speechText.text = message;
        bubbleContainer.SetActive(true);

        // Fade in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            float elapsed = 0;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / fadeTime;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        // Wait
        yield return new WaitForSeconds(messageDisplayTime);

        // Fade out
        if (canvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1 - (elapsed / fadeTime);
                yield return null;
            }
            canvasGroup.alpha = 0;
        }

        bubbleContainer.SetActive(false);
    }

    IEnumerator IdleThoughtsRoutine()
    {
        while (true)
        {
            // Wait random time (30-60 seconds)
            yield return new WaitForSeconds(Random.Range(30f, 60f));

            // Only show if not already showing a message and game is active
            if (!isShowingMessage && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            {
                string thought = idleThoughts[Random.Range(0, idleThoughts.Length)];
                
                // Sometimes mention a cat she fed
                if (Random.value < 0.4f && GameManager.Instance != null)
                {
                    string lastFed = GetRandomFedCat();
                    if (!string.IsNullOrEmpty(lastFed))
                    {
                        thought = $"I fed {lastFed} earlier...";
                    }
                }
                
                QueueMessage(thought);
            }
        }
    }

    string GetRandomFedCat()
    {
        // This would need access to GameManager's fed list
        // For now, return empty - we can enhance this later
        return "";
    }
}
