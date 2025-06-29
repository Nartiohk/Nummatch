using UnityEngine;

public class NumTileController : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;
    public SpriteRenderer numberRenderer; 
    public Sprite[] numberSprites;

    [SerializeField]
    private int _value = 0; // 0 = empty
    [SerializeField]
    private bool _isMatched = false;


    /** * Sets the value of the tile and updates the sprite renderer accordingly.
     * @param newValue The new value to set for the tile.
     * If the value is 0, the number renderer is disabled.
     * If the value is out of bounds for the sprite array, a warning is logged.
     */
    public void SetValue(int newValue)
    {
        _value = newValue;
        _isMatched = false;
        Color tempColor = numberRenderer.color;
        tempColor.a = 1f;
        numberRenderer.color = tempColor; // Reset color to opaque
        if (_value == 0)
        {
            numberRenderer.enabled = false;
            return;
        }

        numberRenderer.enabled = true;

        int spriteIndex = _value - 1;
        if (spriteIndex >= 0 && spriteIndex < numberSprites.Length)
        {
            numberRenderer.sprite = numberSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning($"No sprite for value {_value}");
            numberRenderer.sprite = null;
        }
    }
    
    public void SetHighlight(bool isActive)
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = isActive ? Color.red: new Color(0.85f, 0.85f, 0.85f, 1f); // Set to red if active, gray otherwise
        }
    }
    public int GetValue() => _value;

    public void Clear()
    {
        _value = 0;
        _isMatched = false;
        numberRenderer.enabled = false;
        SetHighlight(false);
    }

    public bool IsEmpty() => _value == 0;
    public bool IsMatched() => _isMatched;

    public void MarkMatched()
    {
        SetHighlight(false);
        _isMatched = true;
        Color tempColor = numberRenderer.color;
        tempColor.a = 0.3f; // Make the number semi-transparent
        numberRenderer.color = tempColor;
    }
}