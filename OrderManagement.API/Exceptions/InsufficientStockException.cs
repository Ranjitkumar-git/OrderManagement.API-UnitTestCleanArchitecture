namespace OrderManagement.API.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message)
            : base(message)
        {
        }
    }
}
