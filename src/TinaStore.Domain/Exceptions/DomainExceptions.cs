namespace TinaStore.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int available, int requested)
        : base($"Stock insuficiente para '{productName}'. Disponible: {available}, solicitado: {requested}.") { }
}

public class InvoiceCancelledException : DomainException
{
    public InvoiceCancelledException(int invoiceId)
        : base($"La factura #{invoiceId} está anulada y no puede modificarse.") { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"No se encontró '{entityName}' con Id {id}.") { }
}
