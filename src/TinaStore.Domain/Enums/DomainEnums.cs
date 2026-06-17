namespace TinaStore.Domain.Enums;

public enum InvoiceStatus
{
    Pending = 0,
    Paid = 1,
    Partial = 2,
    Cancelled = 3
}

public enum PaymentMethodType
{
    Cash = 0,
    BankTransfer = 1,
    Nequi = 2,
    Daviplata = 3,
    Card = 4,
    Credit = 5,
    Other = 6
}

public enum InventoryMovementType
{
    Entry = 0,
    Exit = 1,
    Adjustment = 2,
    ReturnFromSale = 3
}

public enum ExpenseStatus
{
    Active = 0,
    Cancelled = 1
}

public enum ImportBatchStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4
}

public enum ReminderStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public enum ReminderChannel
{
    Email = 0,
    WhatsApp = 1,
    Sms = 2
}

public enum UserRole
{
    Admin = 0,
    Seller = 1,
    ReadOnly = 2
}
