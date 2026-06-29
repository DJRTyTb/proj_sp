namespace Server.Services;

public class RandomProvider : IRandomProvider
{
    public void Shuffle<T>(Span<T> values)
    {
        Random.Shared.Shuffle(values);
    }
}