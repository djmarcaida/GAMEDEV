/// <summary>
/// Dimensions of the corridor shell. These must match the Floor / Walls / Ceiling transforms in SampleScene.
/// Floor top is y = 0, the player enters at -Z and the exit is at +Z.
/// </summary>
public static class CorridorLayout
{
    public const float HalfWidth = 6f;
    public const float HalfLength = 60f;
    public const float Height = 6f;

    /// <summary>Structural pilasters (and ceiling beams) are placed every BaySize metres, starting at FirstPilasterZ.</summary>
    public const float BaySize = 8f;
    public const float FirstPilasterZ = -56f;
    public const int PilasterCount = 15;
}
