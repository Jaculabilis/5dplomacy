namespace MultiversalDiplomacy.Adjudicate.Logging;

public interface IAdjudicatorLogger
{
    public void Log(int contextLevel, string message, params object[] args);
}