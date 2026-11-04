#nullable enable

using System;
using System.Collections.Generic;
using Godot;

namespace Nave9_Evacuation.Scripts;

/// <summary>
/// Procedural 3D maze generator and level builder.
/// Utilizes a stack-based randomized Depth-First Search (Recursive Backtracker) algorithm
/// to construct a maze layout, instantiates wall meshes, and dynamically places the Player,
/// EscapePod, and collectible Battery items across navigable corridors.
/// </summary>
public partial class MazeGenerator : Node3D
{
    [ExportGroup("Maze Dimensions")]
    [Export] public int Width { get; set; } = 15;
    [Export] public int Height { get; set; } = 15;
    [Export] public float CellSize { get; set; } = 3.0f;

    [ExportGroup("Scene Prefabs")]
    [Export] public PackedScene WallScene { get; set; } = null!;
    [Export] public PackedScene BatteryScene { get; set; } = null!;
    [Export] public PackedScene EscapePodScene { get; set; } = null!;

    /// <summary>
    /// Event fired once the EscapePod instance has been instantiated and placed in the world.
    /// </summary>
    public event Action<EscapePod>? OnEscapePodSpawned;

    private int[,] _grid = null!;
    private readonly List<Vector2I> _freeCells = new();
    private static readonly Random _rng = new();

    /// <summary>
    /// Main entry point for maze generation.
    /// Ensures odd grid dimensions, generates the matrix, instantiates walls, and spawns entities.
    /// </summary>
    /// <param name="player">The player instance to position at the starting tile.</param>
    /// <param name="itemsContainer">Container node to parent spawned items and pods.</param>
    public void GenerateAndBuild(Player player, Node3D itemsContainer)
    {
        // Maze generation requires odd dimensions for wall/passage alternation
        if (Width % 2 == 0) Width++;
        if (Height % 2 == 0) Height++;

        _grid = new int[Width, Height];
        _freeCells.Clear();

        GenerateMazeMatrix();
        BuildWallInstances();
        SpawnEntities(player, itemsContainer);
    }

    /// <summary>
    /// Generates a perfect maze matrix using randomized Depth-First Search with backtracking.
    /// 1 represents a solid wall; 0 represents an open, walkable path.
    /// </summary>
    private void GenerateMazeMatrix()
    {
        // Initialize entire grid as solid walls
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                _grid[x, z] = 1;
            }
        }

        var stack = new Stack<Vector2I>();
        var start = new Vector2I(1, 1);
        _grid[start.X, start.Y] = 0;
        stack.Push(start);

        // Movement offsets stepping 2 tiles at a time
        Vector2I[] stepDirections =
        {
            new(0, -2), // North
            new(0, 2),  // South
            new(-2, 0), // West
            new(2, 0)   // East
        };

        // Recursive backtracking loop
        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var validNeighbors = new List<Vector2I>();

            // Find unvisited neighbors 2 steps away within bounds
            foreach (var dir in stepDirections)
            {
                int nx = current.X + dir.X;
                int ny = current.Y + dir.Y;

                if (nx > 0 && nx < Width - 1 && ny > 0 && ny < Height - 1)
                {
                    if (_grid[nx, ny] == 1)
                    {
                        validNeighbors.Add(new Vector2I(nx, ny));
                    }
                }
            }

            if (validNeighbors.Count > 0)
            {
                // Choose a random unvisited neighbor
                var chosen = validNeighbors[_rng.Next(validNeighbors.Count)];

                // Carve a path through the dividing wall between current and chosen cell
                int wallX = current.X + (chosen.X - current.X) / 2;
                int wallY = current.Y + (chosen.Y - current.Y) / 2;
                _grid[wallX, wallY] = 0;

                // Carve target cell and push to stack
                _grid[chosen.X, chosen.Y] = 0;
                stack.Push(chosen);
            }
            else
            {
                // Dead end reached, backtrack
                stack.Pop();
            }
        }
    }

    /// <summary>
    /// Instantiates 3D wall block scenes in the world for all solid cells,
    /// and collects all open cells into the free cells list for entity placement.
    /// </summary>
    private void BuildWallInstances()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                Vector3 worldPos = new Vector3(x * CellSize, 1.75f, z * CellSize);

                if (_grid[x, z] == 1)
                {
                    var wall = WallScene.Instantiate<Node3D>();
                    wall.Position = worldPos;
                    AddChild(wall);
                }
                else
                {
                    _freeCells.Add(new Vector2I(x, z));
                }
            }
        }
    }

    /// <summary>
    /// Spawns the player, escape pod, and battery items randomly across the free corridor tiles.
    /// Utilizes Fisher-Yates shuffle to ensure uniform random distribution.
    /// </summary>
    /// <param name="player">Player instance to position.</param>
    /// <param name="itemsContainer">Parent node for spawned dynamic items.</param>
    private void SpawnEntities(Player player, Node3D itemsContainer)
    {
        if (_freeCells.Count < 6) return;

        // Shuffle open cells using Fisher-Yates algorithm
        for (int i = _freeCells.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (_freeCells[i], _freeCells[j]) = (_freeCells[j], _freeCells[i]);
        }

        // Spawn player at first available free cell
        Vector3 spawnPlayerPos = GridToWorldPosition(_freeCells[0]);
        player.Position = new Vector3(spawnPlayerPos.X, 1.0f, spawnPlayerPos.Z);

        // Spawn escape pod at the furthest cell in the list
        Vector3 podPos = GridToWorldPosition(_freeCells[^1]);
        var pod = EscapePodScene.Instantiate<EscapePod>();
        pod.Position = new Vector3(podPos.X, 0.0f, podPos.Z);
        itemsContainer.AddChild(pod);

        // Notify listeners that escape pod is ready
        OnEscapePodSpawned?.Invoke(pod);

        // Spawn required batteries in intermediate open cells
        for (int i = 1; i <= Player.TotalBatteries; i++)
        {
            Vector3 batteryPos = GridToWorldPosition(_freeCells[i]);
            var battery = BatteryScene.Instantiate<Node3D>();
            battery.Position = new Vector3(batteryPos.X, 0.35f, batteryPos.Z);
            itemsContainer.AddChild(battery);
        }
    }

    /// <summary>
    /// Converts 2D integer grid coordinates into 3D world space coordinates based on CellSize.
    /// </summary>
    /// <param name="cell">Grid coordinate (X, Y).</param>
    /// <returns>World coordinate Vector3.</returns>
    private Vector3 GridToWorldPosition(Vector2I cell)
    {
        return new Vector3(cell.X * CellSize, 0, cell.Y * CellSize);
    }
}
