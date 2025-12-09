using Application.Abstractions.Interfaces.CommandHandler;
using Application.Manychat.Command;
using Application.Utilities.Dtos;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Application.Manychat.CommandHandler
{
    public class SetSurveyFieldsToUsersHandler : ICommandHandler<SetSurveyFieldsToUsersCommand, ManyChatUserResponse>
    {
        private readonly HttpClient _httpClient;

        public SetSurveyFieldsToUsersHandler(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders
                .Add("Authorization", $"Bearer {config["ManyChat:ApiKey"]}");
        }
        public async Task<ManyChatUserResponse> HandleAsync(SetSurveyFieldsToUsersCommand command, CancellationToken cancellationToken = default)
        {
            var url = $"https://api.manychat.com/fb/subscriber/getInfo?subscriber_id={command.SubscriberIds}";
            var response = await _httpClient.GetFromJsonAsync<ManyChatUserResponse>(url);

            if (response == null || response.Status != "success")
                return null;

            return response;
        }
    }
}
