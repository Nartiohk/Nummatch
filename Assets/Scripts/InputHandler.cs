// InputHandler.cs
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public GridManager gridManager;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("pressed left mouse button");
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (!(hit == null) && hit.TryGetComponent<NumTileController>(out var tile))
            {
                gridManager.SelectTile(tile);
                
            }
        }
    }

    private int GetTileIndex(NumTileController tile)
    {
        for (int i = 0; i < gridManager.transform.childCount; i++)
        {
            var t = gridManager.transform.GetChild(i).GetComponent<NumTileController>();
            if (t == tile)
                return i;
        }
        return -1;
    }
}