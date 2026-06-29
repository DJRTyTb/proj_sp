namespace Server.Services;

public interface IRandomProvider
{
    void Shuffle<T>(Span<T> values);
}