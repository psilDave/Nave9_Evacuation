using System;
using System.Collections.Generic;
using Godot;

namespace Nave9_Evacuation.Scripts;

public partial class MazeGenerator : Node3D
{
    [Export] public int Width { get; set; } = 15;
    [Export] public int Height { get; set; } = 15;
    [Export] public float CellSize { get; set; } = 3.0f;

    [Export] public PackedScene WallScene { get; set; } = null!;
    [Export] public PackedScene BatteryScene { get; set; } = null!;
    [Export] public PackedScene EscapePodScene { get; set; } = null!;

    public event Action<EscapePod>? OnEscapePodSpawned;

    private int[,] _grid = null!;
    private readonly List<Vector2I> _freeCells = new();
    private static readonly Random _rng = new();


    public void GenerateAndBuild(Player player, Node3D itemsContainer)
    {
        if (Width % 2 == 0) Width++;
        if (Height % 2 == 0) Height++;

        _grid = new int[Width, Height];
        _freeCells.Clear();

        GenerateMazeMatrix();
        BuildWallInstances();

        SpawnEntities(player, itemsContainer);
    }

    private void GenerateMazeMatrix()
    {
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

        Vector2I[] stepDirections =
        {
            new(0, -2),
            new(0, 2),
            new(-2, 0),
            new(2, 0)
        };

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var validNeighbors = new List<Vector2I>();

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
                var chosen = validNeighbors[_rng.Next(validNeighbors.Count)];

                int wallX = current.X + (chosen.X - current.X) / 2;
                int wallY = current.Y + (chosen.Y - current.Y) / 2;
                _grid[wallX, wallY] = 0;

                _grid[chosen.X, chosen.Y] = 0;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }
    }

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

    private void SpawnEntities(Player player, Node3D itemsContainer)
    {
        if (_freeCells.Count < 6) return;

        for (int i = _freeCells.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (_freeCells[i], _freeCells[j]) = (_freeCells[j], _freeCells[i]);
        }

        Vector3 spawnPlayerPos = GridToWorldPosition(_freeCells[0]);
        player.Position = new Vector3(spawnPlayerPos.X, 1.0f, spawnPlayerPos.Z);

        Vector3 podPos = GridToWorldPosition(_freeCells[^1]);
        var pod = EscapePodScene.Instantiate<EscapePod>();
        pod.Position = new Vector3(podPos.X, 0.0f, podPos.Z);
        itemsContainer.AddChild(pod);

        OnEscapePodSpawned?.Invoke(pod);

        for (int i = 1; i <= Player.TotalBatteries; i++)
        {
            Vector3 batteryPos = GridToWorldPosition(_freeCells[i]);
            var battery = BatteryScene.Instantiate<Node3D>();
            battery.Position = new Vector3(batteryPos.X, 0.35f, batteryPos.Z);
            itemsContainer.AddChild(battery);
        }
    }

    private Vector3 GridToWorldPosition(Vector2I cell)
    {
        return new Vector3(cell.X * CellSize, 0, cell.Y * CellSize);
    }
}