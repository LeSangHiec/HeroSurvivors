using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatEntry : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;

    [Header("Settings")]
    [SerializeField] private Color defaultValueColor = Color.white;

    void Awake()
    {
        // Auto-find components if not assigned
        if (icon == null)
            icon = transform.Find("Icon")?.GetComponent<Image>();

        if (labelText == null)
            labelText = transform.Find("LabelText")?.GetComponent<TMP_Text>();

        if (valueText == null)
            valueText = transform.Find("ValueText")?.GetComponent<TMP_Text>();
    }

    // ========== PUBLIC METHODS ==========

    public void Setup(string label, string value)
    {
        SetLabel(label);
        SetValue(value);
    }

    public void Setup(string label, string value, Sprite iconSprite)
    {
        SetIcon(iconSprite);
        SetLabel(label);
        SetValue(value);
    }

    public void Setup(string label, string value, Color iconColor)
    {
        SetIconColor(iconColor);
        SetLabel(label);
        SetValue(value);
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null && sprite != null)
        {
            icon.sprite = sprite;
            icon.enabled = true;
        }
    }

    public void SetIconColor(Color color)
    {
        if (icon != null)
        {
            icon.color = color;
        }
    }

    public void SetLabel(string text)
    {
        if (labelText != null)
        {
            labelText.text = text;
        }
    }

    public void SetValue(string text)
    {
        if (valueText != null)
        {
            valueText.text = text;
            valueText.color = defaultValueColor;
        }
    }

    public void SetValue(string text, Color color)
    {
        if (valueText != null)
        {
            valueText.text = text;
            valueText.color = color;
        }
    }

    public void SetValueColor(Color color)
    {
        if (valueText != null)
        {
            valueText.color = color;
        }
    }
}