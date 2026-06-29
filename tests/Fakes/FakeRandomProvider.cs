using Server.Services;

namespace Tests.Fakes;

public class FakeRandomProvider : IRandomProvider
{
    public void Shuffle<T>(Span<T> values)
    {
    }
}