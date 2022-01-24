using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geex.Common.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Geex.Common.Messaging.Api.Aggregates.Messages;
using KuanFang.Rms.MessageManagement.Messages;
using Microsoft.Extensions.Logging;
using MongoDB.Entities;
using Entity = Geex.Common.Abstraction.Storage.Entity;

namespace Geex.Common.Messaging.Core.Aggregates.Messages
{
    /// <summary>
    /// 普通message
    /// </summary>
    public class Message : Entity, IMessage
    {
        private ILogger<Message> Logger => ServiceProvider.GetService<ILogger<Message>>();
        protected Message()
        {
        }
        public Message(string text, MessageSeverityType severity = MessageSeverityType.Info)
        : this()
        {
            Title = text;
            this.Severity = severity;
            this.MessageType = MessageType.Notification;
        }
        public Message(string text, IMessageContent content = default, MessageSeverityType severity = MessageSeverityType.Info) : this(text, severity)
        {
            this.Content = content;
            this.MessageType = content switch
            {
                ToDoContent => MessageType.Todo,
                InteractContent => MessageType.Interact,
                _ => MessageType.Notification
            };
        }
        public virtual IQueryable<MessageDistribution> Distributions => DbContext.Queryable<MessageDistribution>().Where(x => x.MessageId == this.Id);
        public string? FromUserId { get; private set; }
        public IList<string> ToUserIds => this.Distributions.ToList().Select(x => x.ToUserId).ToList();
        public string Title { get; set; }
        public DateTimeOffset Time => CreatedOn;

        public MessageType MessageType { get; set; }
        public IMessageContent Content { get; private set; }
        public MessageSeverityType Severity { get; set; }
        public async Task<Message> DistributeAsync(params string[] userIds)
        {
            if (this.Distributions.Any())
            {
                return this;
            }

            var distributions = userIds.Select(x => new MessageDistribution(this.Id, x)).ToList();
            DbContext.Attach(distributions);

            return this;
        }

        /// <summary>
        /// 标记当前消息针对特定用户已读
        /// </summary>
        /// <param name="userId"></param>
        public void MarkAsRead(string userId)
        {
            var userDistribution = this.Distributions.FirstOrDefault(x => x.ToUserId == userId);
            if (userDistribution != default)
            {
                userDistribution.IsRead = true;
            }
            else
            {
                Logger.LogWarning("试图标记不存在的消息分配记录已读.");
            }
        }
    }
}
