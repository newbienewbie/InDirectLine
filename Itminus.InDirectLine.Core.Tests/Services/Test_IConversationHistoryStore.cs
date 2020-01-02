using Itminus.InDirectLine.Core.Services;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Itminus.InDirectLine.Core.Tests.Services
{
    public class Test_IConversationHistoryStore : IClassFixture<InMemoryConversationHistoryStore>
    {
        private readonly IConversationHistoryStore _conversationHistory;

        public Test_IConversationHistoryStore(InMemoryConversationHistoryStore conversationHistory)
        {
            this._conversationHistory = conversationHistory;
        }

        [Fact]
        public async Task Test_CreateConversation_And_ConversationExists() 
        {
            var conversationId = Guid.NewGuid().ToString();
            var exists1= await this._conversationHistory.ConversationExistsAsync(conversationId);
            Assert.False(exists1, "conversation history must not exists before created");

            await this._conversationHistory.CreateConversationIfNotExistsAsync(conversationId);
            var exists2= await this._conversationHistory.ConversationExistsAsync(conversationId);
            Assert.True(exists2, "conversation history must exist after created");
        }


        [Fact]
        public async Task Test_AddActivity_And_GetActivitySet()
        {
            var conversationId = Guid.NewGuid().ToString();
            var activities = new List<Activity>() { 
                new Activity { Id = "Activity1", Name = "TestActivity1", },
                new Activity { Id = "Activity2", Name = "TestActivity2", },
            };
            foreach (var activity in activities)
            { 
                await this._conversationHistory.AddActivityAsync(conversationId, activity);
            }

            var originalWatermark = 0;
            var set = await this._conversationHistory.GetActivitySetAsync(conversationId, originalWatermark);
            var newWatermark = originalWatermark + activities.Count;
            Assert.Equal(newWatermark, set.Watermark);
            for(var i= 0; i< set.Activities.Count; i++)
            { 
                Assert.Equal(activities[i].Id, set.Activities[i].Id);
                Assert.Equal(activities[i].Name, set.Activities[i].Name);
            }

            // no more data at this watermark 
            var set2 = await this._conversationHistory.GetActivitySetAsync(conversationId, newWatermark);
            Assert.Equal(newWatermark, set2.Watermark);
            Assert.Equal(0, set2.Activities.Count);
        }



    }
}
