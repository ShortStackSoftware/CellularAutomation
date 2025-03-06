using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 16;
    public int gridHeight = 16;
    public float cellSize = 1f;
    public Color gridColor = Color.white;
    public bool showGrid = true;

    [Header("Tile Settings")]
    public GameObject deadTilePrefab; // Reference to the DeadTile prefab
    public GameObject aliveTilePrefab; // Reference to the AliveTile prefab
    public GameObject foodTilePrefab; // Reference to the FoodTile prefab
    public GameObject[,] gridTiles; // Array to store tile references

    [Header("Spawn Settings")]
    public float spawnCycleTime = 24f; // Time between spawn cycles in seconds
    public int foodSpawnsPerCycle = 2; // Number of food tiles to spawn each cycle

    private Vector2 gridOrigin;
    private Camera mainCamera;
    private float nextSpawnTime;

 
    void Start()
    {
    
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene!");
            return;
        }

      
        gridOrigin = transform.position;
        
        
        gridTiles = new GameObject[gridWidth, gridHeight];
        
       
        FillGridWithDeadTiles();

       
        PlaceRandomAliveTile();

        
        SetupCamera();

       
        nextSpawnTime = Time.time + spawnCycleTime;
    }

    void SetupCamera()
    {
        if (mainCamera == null) return;

    
        mainCamera.orthographic = true;


        float gridWorldWidth = gridWidth * cellSize;
        float gridWorldHeight = gridHeight * cellSize;

   
        Vector3 gridCenter = transform.position + new Vector3(
            gridWorldWidth * 0.5f,
            gridWorldHeight * 0.5f,
            0
        );


        mainCamera.transform.position = new Vector3(gridCenter.x, gridCenter.y, -10f);


        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = gridWorldWidth / gridWorldHeight;

        float padding = 1.2f; 

        if (screenRatio >= targetRatio)
        {

            mainCamera.orthographicSize = (gridWorldHeight * 0.5f) * padding;
        }
        else
        {
            mainCamera.orthographicSize = (gridWorldWidth * 0.5f) / screenRatio * padding;
        }

        float minSize = Mathf.Max(gridWorldWidth, gridWorldHeight) * 0.5f * padding;
        mainCamera.orthographicSize = Mathf.Max(mainCamera.orthographicSize, minSize);
    }

    void FillGridWithDeadTiles()
    {
        if (deadTilePrefab == null)
        {
            Debug.LogError("DeadTile prefab is not assigned!");
            return;
        }

        SpriteRenderer prefabSprite = deadTilePrefab.GetComponent<SpriteRenderer>();
        if (prefabSprite == null)
        {
            Debug.LogError("DeadTile prefab must have a SpriteRenderer component!");
            return;
        }

        Vector2 spriteSize = prefabSprite.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
  
                Vector3 position = transform.position + new Vector3(
                    (x * cellSize) + (cellSize / 2f), 
                    (y * cellSize) + (cellSize / 2f), 
                    0
                );
                
                GameObject tile = Instantiate(deadTilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                
  
                tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                

                tile.name = $"DeadTile_{x}_{y}";
                
                gridTiles[x, y] = tile;
            }
        }
    }

    void PlaceRandomAliveTile()
    {
        if (aliveTilePrefab == null)
        {
            Debug.LogError("AliveTile prefab is not assigned!");
            return;
        }


        int randomX = Random.Range(0, gridWidth);
        int randomY = Random.Range(0, gridHeight);


        if (gridTiles[randomX, randomY] != null)
        {
            Destroy(gridTiles[randomX, randomY]);
        }


        Vector3 position = transform.position + new Vector3(
            (randomX * cellSize) + (cellSize / 2f),
            (randomY * cellSize) + (cellSize / 2f),
            0
        );


        SpriteRenderer aliveSprite = aliveTilePrefab.GetComponent<SpriteRenderer>();
        if (aliveSprite == null)
        {
            Debug.LogError("AliveTile prefab must have a SpriteRenderer component!");
            return;
        }


        Vector2 spriteSize = aliveSprite.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;


        GameObject aliveTile = Instantiate(aliveTilePrefab, position, Quaternion.identity);
        aliveTile.transform.parent = transform;
        aliveTile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        aliveTile.name = $"AliveTile_{randomX}_{randomY}";


        gridTiles[randomX, randomY] = aliveTile;
    }


    void Update()
    {
  
        if (Time.time >= nextSpawnTime)
        {

            for (int i = 0; i < foodSpawnsPerCycle; i++)
            {
                SpawnFoodTile();
            }
            

            nextSpawnTime = Time.time + spawnCycleTime;
        }

        if (Input.GetMouseButton(0))
        {

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 snappedPos = SnapToGrid(mousePos);


        }
    }

    void SpawnFoodTile()
    {
        if (foodTilePrefab == null)
        {
            Debug.LogError("FoodTile prefab is not assigned!");
            return;
        }

        // Try to find an empty spot (maximum 10 attempts to prevent infinite loop)
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Generate random position
            int randomX = Random.Range(0, gridWidth);
            int randomY = Random.Range(0, gridHeight);

            // Check if the current tile is a DeadTile (we only replace dead tiles)
            if (gridTiles[randomX, randomY] != null && 
                gridTiles[randomX, randomY].name.StartsWith("DeadTile"))
            {
                // Calculate the position for the new tile
                Vector3 position = transform.position + new Vector3(
                    (randomX * cellSize) + (cellSize / 2f),
                    (randomY * cellSize) + (cellSize / 2f),
                    0
                );

                // Destroy the existing tile
                Destroy(gridTiles[randomX, randomY]);

                // Get the sprite renderer from the prefab to calculate proper scaling
                SpriteRenderer foodSprite = foodTilePrefab.GetComponent<SpriteRenderer>();
                if (foodSprite == null)
                {
                    Debug.LogError("FoodTile prefab must have a SpriteRenderer component!");
                    return;
                }

                // Calculate the scale needed to make the sprite fit exactly in one cell
                Vector2 spriteSize = foodSprite.sprite.bounds.size;
                float scaleX = cellSize / spriteSize.x;
                float scaleY = cellSize / spriteSize.y;

                // Create the FoodTile
                GameObject foodTile = Instantiate(foodTilePrefab, position, Quaternion.identity);
                foodTile.transform.parent = transform;
                foodTile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                foodTile.name = $"FoodTile_{randomX}_{randomY}";

                // Add BoxCollider2D if it doesn't exist
                if (foodTile.GetComponent<BoxCollider2D>() == null)
                {
                    BoxCollider2D collider = foodTile.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true; // Make it a trigger to avoid physics interactions
                }

                // Update the grid array with the new tile
                gridTiles[randomX, randomY] = foodTile;
                break;
            }
        }
    }


    public Vector2 SnapToGrid(Vector3 worldPosition)
    {
        float x = Mathf.Round((worldPosition.x - gridOrigin.x) / cellSize) * cellSize + gridOrigin.x;
        float y = Mathf.Round((worldPosition.y - gridOrigin.y) / cellSize) * cellSize + gridOrigin.y;
        return new Vector2(x, y);
    }


    void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;


        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 startPos = transform.position + new Vector3(x * cellSize, 0, 0);
            Vector3 endPos = startPos + new Vector3(0, gridHeight * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }


        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 startPos = transform.position + new Vector3(0, y * cellSize, 0);
            Vector3 endPos = startPos + new Vector3(gridWidth * cellSize, 0, 0);
            Gizmos.DrawLine(startPos, endPos);
        }
    }

    // Add this new method to get a tile at a specific position
    public GameObject GetTileAt(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return gridTiles[x, y];
        }
        return null;
    }
}
