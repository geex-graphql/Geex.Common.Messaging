using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geex.Common.Abstraction.MultiTenant;
using Geex.Common.Abstraction.Storage;
using Geex.Common.Messaging.Api.Aggregates.Messages;
using KuanFang.Rms.MessageManagement.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Geex.Common.Messaging.Core.Aggregates.Messages;

/// <summary>
///     ��ͨmessage
/// </summary>
public class Message : Entity, IMessage
{
    protected Message()
    {
    }

    public Message(string text, MessageSeverityType severity = MessageSeverityType.Info)
        : this()
    {
        Title = text;
        Severity = severity;
        MessageType = MessageType.Notification;
    }

    public Message(string text, IMessageContent content = default,
        MessageSeverityType severity = MessageSeverityType.Info) : this(text, severity)
    {
        Content = content;
        MessageType = content switch
        {
            ToDoContent => MessageType.Todo,
            InteractContent => MessageType.Interact,
            _ => MessageType.Notification
        };
    }

    public virtual IQueryable<MessageDistribution> Distributions =>
        DbContext.Queryable<MessageDistribution>().Where(x => x.MessageId == Id);

    private ILogger<Message> Logger => ServiceProvider.GetService<ILogger<Message>>();
    public IMessageContent Content { get; private set;}
    public string? FromUserId { get; private set; }

    public MessageType MessageType { get; set; }
    public MessageSeverityType Severity { get; set; }
    public DateTimeOffset Time => CreatedOn;
    public string Title { get; set; }
    public IList<string> ToUserIds => Distributions.ToList().Select(x => x.ToUserId).ToList();
    public string? TenantCode { get; protected set; }

    public async Task<Message> DistributeAsync(params string[] userIds)
    {
        if (Distributions.Any()) return this;

        var distributions = userIds.Select(x => new MessageDistribution(Id, x)).ToList();
        DbContext.Attach(distributions);

        return this;
    }

    /// <summary>
    ///     ��ǵ�ǰ��Ϣ����ض��û��Ѷ�
    /// </summary>
    /// <param name="userId"></param>
    public void MarkAsRead(string userId)
    {
        var userDistribution = Distributions.FirstOrDefault(x => x.ToUserId == userId);
        if (userDistribution != default)
            userDistribution.IsRead = true;
        else
            Logger.LogWarning("��ͼ��ǲ����ڵ���Ϣ�����¼�Ѷ�.");
    }
}