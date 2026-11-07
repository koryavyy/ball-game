public sealed class LevelAnalysisResult
{
    public bool IsPlayable { get; }
    public float RequiredStartArea { get; }
    public int ShotsNeeded { get; }
    public string ErrorMessage { get; }

    private LevelAnalysisResult(
        bool isPlayable,
        float requiredStartArea,
        int shotsNeeded,
        string errorMessage)
    {
        IsPlayable = isPlayable;
        RequiredStartArea = requiredStartArea;
        ShotsNeeded = shotsNeeded;
        ErrorMessage = errorMessage;
    }

    public static LevelAnalysisResult Playable(float startArea, int shotsNeeded)
    {
        return new LevelAnalysisResult(true, startArea, shotsNeeded, null);
    }

    public static LevelAnalysisResult Unplayable(string errorMessage)
    {
        return new LevelAnalysisResult(false, 0f, 0, errorMessage);
    }
}
