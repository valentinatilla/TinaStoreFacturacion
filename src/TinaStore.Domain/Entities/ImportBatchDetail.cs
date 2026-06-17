namespace TinaStore.Domain.Entities;

/// <summary>Resultado fila a fila de una importación masiva de productos (éxito o error con mensaje).</summary>
public class ImportBatchDetail : BaseEntity
{
    public int ImportBatchId { get; set; }
    public ImportBatch ImportBatch { get; set; } = null!;

    public int RowNumber { get; set; }
    public string? InternalCode { get; set; }
    public string? ProductName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawData { get; set; }
}
