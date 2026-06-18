namespace TinaStore.Application.DTOs;

/// <summary>Datos para registrar un recordatorio enviado por WhatsApp.</summary>
public record RegistrarRecordatorioWhatsAppDto(
    int CustomerId,
    string Message
);

/// <summary>Resumen de un envío de recordatorio registrado en historial.</summary>
public record ReminderHistoryDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string Channel,
    string Status,
    DateTime SentAt,
    string Message
);
