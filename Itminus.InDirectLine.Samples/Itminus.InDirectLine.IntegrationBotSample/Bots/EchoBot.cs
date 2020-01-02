// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v0.4.6

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itminus.InDirectLine.IntegrationBotSample.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Itminus.InDirectLine.IntegrationBotSample.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly ConversationState conversationState;
        private readonly UserState userState;
        private readonly MainDialog mainDialog;

        public EchoBot(ConversationState conversationState, UserState userState, MainDialog mainDialog)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            this.mainDialog = mainDialog;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var dialogStatePropertyAccessor = this.conversationState.CreateProperty<DialogState>("DialogState");

            var dialogSet = new DialogSet(dialogStatePropertyAccessor);
            dialogSet.Add(mainDialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext,cancellationToken);
            var dialogTurnResult=await dialogContext.ContinueDialogAsync(cancellationToken);
            if(dialogTurnResult.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(mainDialog.Id);
            }

            await this.conversationState.SaveChangesAsync(turnContext,false,cancellationToken);
            await this.userState.SaveChangesAsync(turnContext,false,cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"你好！请回复任意消息开始会话"), cancellationToken);
                }
            }
        }
    }
}
