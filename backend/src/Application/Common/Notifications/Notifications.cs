using MediatR;

namespace Crm.Application.Common.Notifications;

public record UserActivityLoggedNotification(
    long UserId,
    string Activity,
    string EntityType,
    long? EntityId,
    Dictionary<string, object>? Metadata
) : INotification;

public record EntityChangedNotification(
    string EntityType,
    long EntityId,
    string ChangeType,
    long? ChangedById,
    Dictionary<string, object>? OldValues,
    Dictionary<string, object>? NewValues
) : INotification;

public record EntityCreatedNotification<T>(
    long EntityId,
    T Entity
) : INotification;

public record EntityUpdatedNotification<T>(
    long EntityId,
    T Entity
) : INotification;

public record EntityDeletedNotification<T>(
    long EntityId
) : INotification;