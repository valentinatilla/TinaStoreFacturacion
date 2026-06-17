using TinaStore.Domain.Enums;

namespace TinaStore.Domain.Entities;

/// <summary>Registro de una importación masiva de productos desde Excel.</summary>
public class ImportBatch : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int ErrorRows { get; set; }
    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Pending;
    public DateTime ProcessedAt { get; set; }
    public bool UpdateExisting { get; set; } = false;

    public ICollection<ImportBatchDetail> Details { get; set; } = [];
}
