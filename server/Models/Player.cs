using Server.Enums;

namespace Server.Models;

public class Player(string name)
{
    public Guid Token { get; init; } = Guid.NewGuid();

    public string Name { get; init; } = name;

    public string ConnectionId { get; set; } = "";

    public FrogColor Color { get; set; }

    public bool HasUsedInitialRemoval { get; set; }

    public bool HasMadeJump { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime? DisconnectedAt { get; set; }
}