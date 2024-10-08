﻿using BKey.Tetris.Logic.Board;
using BKey.Tetris.Logic.Input;
using BKey.Tetris.Logic.Tetrimino;
using System.Threading.Tasks;

namespace BKey.Tetris.Logic.Game;
public class GameController : IGameController
{
    private BoardBuffer BoardBuffer { get; }
    private ITetriminoFactory TetriminoFactory { get; }
    private IInputQueue<MovementRequest> MovementQueue { get; }
    private IGameScore Score { get; }

    private GameState CurrentState { get; set; }

    private const int Left = -1;
    private const int Right = 1;
    private const int Up = -1;
    private const int Down = 1;
    private const int None = 0;

    public GameController(
        BoardBuffer boardBuffer,
        ITetriminoFactory tetriminoFactory,
        IInputQueue<MovementRequest> inputQueue,
        IGameScore score)
    {
        BoardBuffer = boardBuffer;
        TetriminoFactory = tetriminoFactory;
        MovementQueue = inputQueue;
        CurrentState = GameState.NewPieceSpawn;
        Score = score;
    }

    public async Task Run()
    {
        while (true)
        {
            switch (CurrentState)
            {
                case GameState.Input:
                    HandleInput();
                    break;
                case GameState.Movement:
                    HandleMovement();
                    break;
                case GameState.Commit:
                    CommitTetrimino();
                    break;
                case GameState.LineClear:
                    ClearLines();
                    break;
                case GameState.NewPieceSpawn:
                    SpawnNewPiece();
                    break;
                case GameState.Render:
                    Render();
                    break;
            }

            await Task.Delay(10);
        }
    }

    private void HandleInput()
    {
        // Capture user input (e.g., left, right, rotate, drop)
        // For example, let's assume we're moving to the Rotation state after input
        // This would need to be expanded based on actual input handling

        if (MovementQueue.IsEmpty)
        {
            return;
        }

        CurrentState = GameState.Movement;
    }

    private void HandleMovement()
    {
        // Move the Tetrimino down (or based on user input) and check for collision
        // If movement is complete, move to the commit state

        var board = BoardBuffer.GetWriteBoard();
        var movement = MovementQueue.Dequeue();

        if (movement == MovementRequest.Rotate)
        {
            board.RotateTetrimino();
        }

        if (movement == MovementRequest.Left)
        {
            board.MoveTetrimino(Left, None);
        }

        if (movement == MovementRequest.Right)
        {
            board.MoveTetrimino(Right, None);
        }

        if (movement == MovementRequest.Down)
        {
            board.MoveTetrimino(None, Down);
        }

        CurrentState = GameState.Commit;
    }

    private void CommitTetrimino()
    {
        // Add the Tetrimino to the board and check if it is placed
        // If placed, move to the line clear state
        var board = BoardBuffer.GetWriteBoard();
        if (!board.CanMove(None, Down))
        {
            board.PlaceTetrimino();
            Score.AddPiecePlaced();
        }

        CurrentState = GameState.LineClear;
    }

    private void ClearLines()
    {
        // Check and clear any full lines on the board
        // After clearing lines, move to the new piece spawn state
        var board = BoardBuffer.GetWriteBoard();
        if (board.CurrentTetrimino == null)
        {
            var linesCleared = board.ClearLines();
            Score.AddLinesCleared(linesCleared);
        }

        CurrentState = GameState.NewPieceSpawn;
    }

    private void SpawnNewPiece()
    {
        var board = BoardBuffer.GetWriteBoard();
        if (board.CurrentTetrimino == null)
        {
            // Spawn a new Tetrimino and place it on the board
            board.AddTetrmino(TetriminoFactory.Next());

        }

        // Move to the render state
        CurrentState = GameState.Render;
    }

    private void Render()
    {
        // Render the current state of the board and the active Tetrimino
        BoardBuffer.SwapBuffers();

        // Move back to the input state after rendering
        CurrentState = GameState.Input;
    }
}
