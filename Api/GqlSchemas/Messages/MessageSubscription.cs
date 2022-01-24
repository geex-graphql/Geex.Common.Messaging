using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Geex.Common.Abstractions;
using Geex.Common.Gql.Roots;
using Geex.Common.Messaging.Api.Aggregates.FrontendCalls;
using Geex.Common.Messaging.Api.Aggregates.Messages;
using Geex.Common.Messaging.Core.Aggregates.FrontendCalls;

using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace Geex.Common.Messaging.Api.GqlSchemas.Messages
{
    public class MessageSubscription : SubscriptionTypeExtension<MessageSubscription>
    {
        /// <summary>
        /// 订阅服务器对单个用户的前端调用
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<IFrontendCall>> OnFrontendCall([Service] ITopicEventReceiver receiver, [Service] LazyFactory<ClaimsPrincipal> claimsPrincipal)
        {
            return receiver.SubscribeAsync<string, IFrontendCall>($"{nameof(OnFrontendCall)}:{claimsPrincipal.Value?.FindUserId()}");
        }

        /// <summary>
        /// 订阅广播
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        [SubscribeAndResolve]
        public ValueTask<ISourceStream<IFrontendCall>> OnBroadcast([Service] ITopicEventReceiver receiver)
        {
            return receiver.SubscribeAsync<string, IFrontendCall>(nameof(OnBroadcast));
        }

        //public ValueTask<ISourceStream<IFrontendCall>> SubscribeToIFrontendCalls(
        //[Service] ITopicEventReceiver receiver)
        //=> receiver.SubscribeAsync<string, IFrontendCall>(nameof(OnBroadcast));

        //[Subscribe(With = nameof(SubscribeToIFrontendCalls))]
        //public IFrontendCall OnBroadcast([EventMessage] IFrontendCall book)
        //    => book;
    }
}
