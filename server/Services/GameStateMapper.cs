using Server.DTOs;
using Server.Enums;
using Server.Models;

namespace Server.Services;

public static class GameStateMapper
{
    public static GameStateDto ToDto(Game game)
    {
        return new()
        {
            GameId = game.Id,

            CurrentPlayer = game.CurrentPlayer.Name,

            Players = game.Players.Select((player, index) => new PlayerDto
            {
                Name = player.Name,
                Color = player.Color.ToString().ToLower(),
                IsCurrentTurn = index == game.CurrentPlayerIndex,
                HasUsedInitialRemoval = player.HasUsedInitialRemoval
            }),

            Cells = game.Board.Frogs.Select(x => new BoardCellDto
            {
                Row = x.Key.Row - 1,
                Col = x.Key.Col - 1,
                Color = x.Value.Color == FrogColor.Green ? "green" : "orange"
            }),

            IsFinished = game.Status == GameStatus.Finished,

            Winner = game.Status == GameStatus.Finished
                ? game.FinishedMessage ?? game.LastJumpPlayer?.Name
                : null,

            Message = game.LastGameMessage
        };
    }
}