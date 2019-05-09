

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Itminus.InDirectLine.Core.Authorization
{
    public class MatchConversationAuthzHandler : AuthorizationHandler<MatchConversationAuthzRequirement>
    {
        private readonly ILogger<MatchConversationAuthzHandler> _logger;

        public MatchConversationAuthzHandler(ILogger<MatchConversationAuthzHandler> logger)
        {
            this._logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MatchConversationAuthzRequirement requirement)
        {
            if(context.Resource is AuthorizationFilterContext mvcContext)
            {
                var conversationId= mvcContext.RouteData.Values["conversationId"] as string;

                if( string.IsNullOrEmpty( conversationId ))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
                var conversationMatches=context.User.HasClaim(c => 
                    c.Type== TokenBuilder.ClaimTypeConversationID && 
                    c.Value == conversationId 
                );
                if(conversationMatches){
                    context.Succeed(requirement);
                }else{
                    context.Fail();
                }
            }
            else{
                context.Fail();
            }

            return Task.CompletedTask;
        }

    }
}