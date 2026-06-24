namespace TinaStore.Web.Services;

/// <summary>
/// Puerta de sincronizaciÃ³n que garantiza que SessionRestorer haya intentado
/// restaurar la sesiÃ³n desde la cookie antes de que Routes renderice las rutas
/// protegidas. Se registra como Scoped (un circuito = un Ã¡mbito).
/// </summary>
public sealed class SessionRestoreGate
{
    private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Task que completa cuando SessionRestorer finalizÃ³ su inicializaciÃ³n.</summary>
    public Task Completed => _tcs.Task;

    /// <summary>Llamado por SessionRestorer al terminar OnInitializedAsync.</summary>
    public void SignalDone() => _tcs.TrySetResult();
}
