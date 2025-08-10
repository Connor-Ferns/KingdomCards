using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text ironText;
    [SerializeField] private TMP_Text woodText;

    private void Start()
    {
        // Subscribe to resource changes
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += UpdateResourceDisplay;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= UpdateResourceDisplay;
        }
    }

    private void UpdateResourceDisplay(ResourceManager.ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceManager.ResourceType.Food:
                if (foodText != null) foodText.text = amount + " / 20";
                break;
            case ResourceManager.ResourceType.Iron:
                if (ironText != null) ironText.text = amount + " / 20";
                break;
            case ResourceManager.ResourceType.Wood:
                if (woodText != null) woodText.text = amount + " / 20";
                break;
        }
    }
}