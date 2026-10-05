namespace PetClaims.Api.Exceptions
{
    public class DuplicateInvoiceException : Exception
    {
        public DuplicateInvoiceException(string message)
            : base(message)
        {
        }
    }
}