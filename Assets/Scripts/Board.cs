using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UI;
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Text gameOverText;
    public Text scoreText;
    public GameObject panel;
    // public Text scoreText;
    public GameObject scorePanel;
    public Text finalscore;
    private int finalScore = 0;


    //new code 4
    private int score = 0;
    //new code 4

    //new code 6
    private bool isGameOver = false;
    //new code 6 ends

    public RectInt Bounds
    {
        get
        {
            //we need the size and position. BoardSize
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    public void Start()
    {
        SpawnPiece();
        UpdateScoreText();
        scorePanel.SetActive(true);
    }

    public void SpawnPiece()
    {

        // new code 3
        if (IsGameOver())
        {
            isGameOver = true;
            Debug.Log("Game ended"); // Log the game over message
            return; // Stop spawning new pieces
        }


        //new code 3 ends
        //pick a random element from the array tetrominoes

        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);
        Set(this.activePiece);

    }


    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("Score Text is not assigned in the Inspector!");
        }
    }

    private bool IsGameOver()
    {
        // Get the top two rows (indices: topRow = maxY, secondTopRow = maxY - 1)
        int topRowY = boardSize.y / 2 - 1;
        int secondTopRowY = topRowY - 1;

        // Check if any tile in the second topmost row is occupied
        for (int x = -boardSize.x / 2; x < boardSize.x / 2; x++)
        {
            Vector3Int topRowPos = new Vector3Int(x, topRowY, 0);
            Vector3Int secondTopRowPos = new Vector3Int(x, secondTopRowY, 0);

            // If any tile in the second top row is occupied, stop the game
            if (tilemap.HasTile(secondTopRowPos))
            {
                Debug.Log("Second top row has a tile. Game over!");
                if (gameOverText != null)
                {
                    gameOverText.gameObject.SetActive(true);
                    panel.gameObject.SetActive(true);
                    scoreText.gameObject.SetActive(true);
                    gameOverText.text += $"\nFinal Score: {score}";
                    finalScore = score;
                    finalscore.text = "Final Score: " + finalScore; 
                    finalscore.gameObject.SetActive(true);
                    gameOverText.text = "Game Over"; // Set the text, just in case it isn't set in the Editor
                }
                else
                {
                    Debug.LogError("Game Over Text is not assigned in the Inspector!");
                }
                return true; // Stop the game
            }
        }

        return false;
    }

    //new code 5 ends

    public void Set(Piece piece)
    {
        //new code 6
        if (isGameOver) return; // Stop setting pieces if game is over
        //new code 6
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }
    //to clear the tiles when rotated
    public void Clear(Piece piece)
    {
        //new code 6
        if (isGameOver) return; // Stop setting pieces if game is over
        //new code 6

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null); //unsetting the tile, since the position is changed
        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        //new code 6
        if (isGameOver) return false; // Stop setting pieces if game is over
        //new code 6
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            //if its out of bound or if tile has occupied the space
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
            //to check out of bound
        }
        return true;
    }

    //new code
    public void CheckAndClearTiles(Piece piece)
    {
        //new code 2

        List<Vector3Int> clearedTiles = new List<Vector3Int>();
        //new code 2 ends
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;

            // Check all four neighboring directions
            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0), // Right
                new Vector3Int(-1, 0, 0), // Left
                new Vector3Int(0, 1, 0), // Up
                new Vector3Int(0, -1, 0) // Down
            };

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborPosition = tilePosition + direction;

                if (tilemap.HasTile(neighborPosition))
                {
                    // Check if the neighboring tile satisfies the Rock-Paper-Scissors rule
                    Tetromino neighborTetromino = GetTetrominoAtPosition(neighborPosition);
                    Tetromino currentTetromino = piece.data.tetromino;

                    if (ShouldClearTile(currentTetromino, neighborTetromino))
                    {
                        // Clear both the current tile and the neighboring tile
                        tilemap.SetTile(tilePosition, null);
                        tilemap.SetTile(neighborPosition, null);

                        //new code 2

                        clearedTiles.Add(tilePosition);
                        clearedTiles.Add(neighborPosition);
                        //new code 2

                        //new code 4
                        score += 10;
                        Debug.Log($"Score: {score}");
                        UpdateScoreText(); // Update the score display
                        //new code 4 ends
                    }
                }
            }
        }

        //new code 2

        MoveTilesDown(clearedTiles);
        //new code 2 ends
    }

    //new code 2
    private void MoveTilesDown(List<Vector3Int> clearedTiles)
    {
        // Sort cleared tiles by their Y position in descending order
        clearedTiles.Sort((a, b) => b.y.CompareTo(a.y));

        // Create a HashSet to track already processed tiles
        HashSet<Vector3Int> processedTiles = new HashSet<Vector3Int>();

        foreach (Vector3Int tile in clearedTiles)
        {
            // Start from the cleared tile and check for tiles above
            Vector3Int aboveTilePosition = tile + new Vector3Int(0, 1, 0);

            // Check if there is a tile above
            if (tilemap.HasTile(aboveTilePosition) && !processedTiles.Contains(aboveTilePosition))
            {
                // Move the tile down
                TileBase tileToMove = tilemap.GetTile(aboveTilePosition);
                if (tileToMove != null)
                {
                    Vector3Int newPosition = tile + new Vector3Int(0, -1, 0);

                    // Check for grid boundaries
                    if (newPosition.y >= Bounds.min.y) // Ensure it does not go below the grid
                    {
                        tilemap.SetTile(newPosition, tileToMove); // Move it down
                        tilemap.SetTile(aboveTilePosition, null); // Clear the original position
                        processedTiles.Add(aboveTilePosition); // Mark this tile as processed
                    }
                    else
                    {
                        // If it goes out of bounds, do not move it
                        tilemap.SetTile(tile, null); // Optionally remove the tile if out of bounds
                    }
                }
            }
        }

        // After processing cleared tiles, check again if any tile needs to fall down
        // Repeat the process until no tiles can be moved
        bool hasMoved;
        do
        {
            hasMoved = false;

            foreach (Vector3Int tile in clearedTiles)
            {
                Vector3Int aboveTilePosition = tile + new Vector3Int(0, 1, 0);

                // Check if there is a tile above
                if (tilemap.HasTile(aboveTilePosition) && !processedTiles.Contains(aboveTilePosition))
                {
                    // Move the tile down
                    TileBase tileToMove = tilemap.GetTile(aboveTilePosition);
                    if (tileToMove != null)
                    {
                        Vector3Int newPosition = tile + new Vector3Int(0, -1, 0);

                        // Check for grid boundaries
                        if (newPosition.y >= Bounds.min.y) // Ensure it does not go below the grid
                        {
                            tilemap.SetTile(newPosition, tileToMove); // Move it down
                            tilemap.SetTile(aboveTilePosition, null); // Clear the original position
                            processedTiles.Add(aboveTilePosition); // Mark this tile as processed
                            hasMoved = true; // Indicate that a tile has moved
                        }
                        else
                        {
                            // If it goes out of bounds, do not move it
                            tilemap.SetTile(tile, null); // Optionally remove the tile if out of bounds
                        }
                    }
                }
            }
        } while (hasMoved); // Repeat if any tile has moved
    }




    //new code 2 end


    // Helper function to determine if the tile should be cleared
    private bool ShouldClearTile(Tetromino current, Tetromino neighbor)
    {
        // Apply the Rock-Paper-Scissors rule
        if ((current == Tetromino.I || current == Tetromino.J || current == Tetromino.S) && // Paper
            (neighbor == Tetromino.T || neighbor == Tetromino.Z)) // Rock
        {
            return true;
        }
        else if ((current == Tetromino.O || current == Tetromino.L) && // Scissors
                 (neighbor == Tetromino.I || neighbor == Tetromino.J || neighbor == Tetromino.S)) // Paper
        {
            return true;
        }
        else if ((current == Tetromino.T || current == Tetromino.Z) && // Rock
                 (neighbor == Tetromino.O || neighbor == Tetromino.L)) // Scissors
        {
            return true;
        }

        return false;
    }

    // Helper function to get the Tetromino at a specific position
    private Tetromino GetTetrominoAtPosition(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);

        foreach (TetrominoData data in tetrominoes)
        {
            if (data.tile == tile)
            {
                return data.tetromino;
            }
        }

        return Tetromino.I; // Default return in case of an error, though this should never happen
    }

}

