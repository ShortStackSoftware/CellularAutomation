using UnityEngine;

public class AlivePathfinding : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveInterval = 1.5f; 
    
    private GridPlacer gridPlacer;
    private float nextMoveTime;
    private Vector2Int currentGridPosition;

   
    void Start()
    {
       
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true; 
        }

        gridPlacer = FindFirstObjectByType<GridPlacer>();
        if (gridPlacer == null)
        {
            Debug.LogError("GridPlacer not found in the scene!");
            return;
        }
        
       
        nextMoveTime = Time.time + moveInterval;
        

        UpdateCurrentGridPosition();
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.name.StartsWith("FoodTile"))
        {
            
            Vector3 foodPos = other.transform.position - gridPlacer.transform.position;
            int foodX = Mathf.RoundToInt(foodPos.x / gridPlacer.cellSize);
            int foodY = Mathf.RoundToInt(foodPos.y / gridPlacer.cellSize);

            
            if (gridPlacer.gridTiles[foodX, foodY] == other.gameObject)
            {
                gridPlacer.gridTiles[foodX, foodY] = null;
            }

            
            Destroy(other.gameObject);
        }
    }

    void Update()
    {
        if (Time.time >= nextMoveTime)
        {
            MoveTowardsFood();
            nextMoveTime = Time.time + moveInterval;
        }
    }

    void UpdateCurrentGridPosition()
    {
        
        Vector3 pos = transform.position - gridPlacer.transform.position;
        currentGridPosition = new Vector2Int(
            Mathf.RoundToInt(pos.x / gridPlacer.cellSize),
            Mathf.RoundToInt(pos.y / gridPlacer.cellSize)
        );
    }

    void MoveTowardsFood()
    {
       
        Vector2Int? foodPosition = FindNearestFoodTile();
        if (!foodPosition.HasValue) return;

       
        Vector2Int targetPos = foodPosition.Value;
        Vector2Int moveDirection = new Vector2Int(0, 0);

       
        int xDiff = targetPos.x - currentGridPosition.x;
        int yDiff = targetPos.y - currentGridPosition.y;

       
        if (xDiff != 0)
        {
            moveDirection.x = (int)Mathf.Sign(xDiff);
        }
        else if (yDiff != 0)
        {
            moveDirection.y = (int)Mathf.Sign(yDiff);
        }

        
        if (moveDirection.x == 0 && moveDirection.y == 0) return;

        
        Vector2Int newPos = currentGridPosition + moveDirection;

        
        if (IsValidGridPosition(newPos))
        {
            
            Vector3 newWorldPos = gridPlacer.transform.position + new Vector3(
                (newPos.x * gridPlacer.cellSize) + (gridPlacer.cellSize / 2f),
                (newPos.y * gridPlacer.cellSize) + (gridPlacer.cellSize / 2f),
                0
            );

            
            transform.position = newWorldPos;
            
            
            UpdateGridTiles(newPos);

            
            currentGridPosition = newPos;
        }
    }

    Vector2Int? FindNearestFoodTile()
    {
        float closestDistance = float.MaxValue;
        Vector2Int? nearestFood = null;

       
        for (int x = 0; x < gridPlacer.gridWidth; x++)
        {
            for (int y = 0; y < gridPlacer.gridHeight; y++)
            {
                GameObject tile = gridPlacer.GetTileAt(x, y);
                if (tile != null && tile.name.StartsWith("FoodTile"))
                {
                    float distance = Vector2Int.Distance(currentGridPosition, new Vector2Int(x, y));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nearestFood = new Vector2Int(x, y);
                    }
                }
            }
        }

        return nearestFood;
    }

    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridPlacer.gridWidth &&
               pos.y >= 0 && pos.y < gridPlacer.gridHeight;
    }

    void UpdateGridTiles(Vector2Int newPos)
    {
        
        if (currentGridPosition == newPos) return;

        
        Vector3 position = gridPlacer.transform.position + new Vector3(
            (currentGridPosition.x * gridPlacer.cellSize) + (gridPlacer.cellSize / 2f),
            (currentGridPosition.y * gridPlacer.cellSize) + (gridPlacer.cellSize / 2f),
            0
        );

        
        GameObject deadTile = Instantiate(gridPlacer.deadTilePrefab, position, Quaternion.identity);
        deadTile.transform.parent = gridPlacer.transform;
        deadTile.name = $"DeadTile_{currentGridPosition.x}_{currentGridPosition.y}";
        
        
        gridPlacer.gridTiles[currentGridPosition.x, currentGridPosition.y] = deadTile;

        
        if (gridPlacer.gridTiles[newPos.x, newPos.y] != null)
        {
            
            if (gridPlacer.gridTiles[newPos.x, newPos.y].name.StartsWith("FoodTile"))
            {
                Destroy(gridPlacer.gridTiles[newPos.x, newPos.y]);
            }
            else 
            {
                Destroy(gridPlacer.gridTiles[newPos.x, newPos.y]);
            }
        }

       
        gridPlacer.gridTiles[newPos.x, newPos.y] = gameObject;
    }
}
