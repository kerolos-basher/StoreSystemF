
public class StoreException : Exception
{
    public string StoreExceptionMessage { get; set; }

    public StoreException(string message)
        : base($"{message}")
    {
        this.StoreExceptionMessage = message;
    }
}
