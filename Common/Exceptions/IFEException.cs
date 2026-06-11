
public class StoreException : Exception
{
    public string StoreExceptionMessage { get; set; }

    public StoreException(string message)
        : base($"{message}")
    {
        this.StoreExceptionMessage = message;
    }
}

public class IFEException : StoreException
{
    public string IFEExceptionMessage => StoreExceptionMessage;

    public IFEException(string message) : base(message) { }
}
