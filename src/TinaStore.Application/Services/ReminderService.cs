using TinaStore.Application.DTOs;
using TinaStore.Application.Interfaces;
using TinaStore.Domain.Entities;
using TinaStore.Domain.Enums;
using TinaStore.Domain.Interfaces;

namespace TinaStore.Application.Services;

public sealed class ReminderService : IReminderService
{
    private readonly IReminderRepository _reminders;
    private readonly ICustomerRepository _customers;
    private readonly IAppClock _clock;

    public ReminderService(IReminderRepository reminders, ICustomerRepository customers, IAppClock clock)
    {
        _reminders = reminders;
        _customers = customers;
        _clock = clock;
    }

    public async Task<ReminderHistoryDto> RegistrarRecordatorioWhatsAppAsync(RegistrarRecordatorioWhatsAppDto dto)
    {
        var customer = await _customers.GetByIdAsync(dto.CustomerId)
            ?? throw new ArgumentException($"Cliente {dto.CustomerId} no existe.");

        // Obtener o crear el Reminder para este cliente
        var reminder = await _reminders.GetByCustomerAsync(dto.CustomerId);
        if (reminder is null)
        {
            reminder = new Reminder
            {
                CustomerId = dto.CustomerId,
                Message    = dto.Message,
                Channel    = ReminderChannel.WhatsApp,
                Status     = ReminderStatus.Sent,
                SentAt     = _clock.Now
            };
            await _reminders.AddAsync(reminder);
            await _reminders.SaveChangesAsync();
        }
        else
        {
            reminder.Message = dto.Message;
            reminder.SentAt  = _clock.Now;
            reminder.Status  = ReminderStatus.Sent;
            await _reminders.UpdateAsync(reminder);
            await _reminders.SaveChangesAsync();
        }

        // Registrar el historial del envío
        var history = new ReminderHistory
        {
            ReminderId = reminder.Id,
            SentAt     = _clock.Now,
            Channel    = ReminderChannel.WhatsApp,
            Status     = ReminderStatus.Sent
        };

        await _reminders.AddHistoryAsync(history);

        return new ReminderHistoryDto(
            history.Id,
            customer.Id,
            customer.FullName,
            nameof(ReminderChannel.WhatsApp),
            nameof(ReminderStatus.Sent),
            history.SentAt,
            dto.Message
        );
    }

    public async Task<IEnumerable<ReminderHistoryDto>> GetHistorialAsync(int customerId)
    {
        var customer = await _customers.GetByIdAsync(customerId);
        if (customer is null) return [];

        var historial = await _reminders.GetHistoryByCustomerAsync(customerId);
        return historial.Select(h => new ReminderHistoryDto(
            h.Id,
            customerId,
            customer.FullName,
            h.Channel.ToString(),
            h.Status.ToString(),
            h.SentAt,
            h.Reminder?.Message ?? string.Empty
        ));
    }
}
