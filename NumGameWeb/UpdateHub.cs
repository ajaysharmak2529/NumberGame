using Microsoft.AspNetCore.SignalR;
using NumGameWeb.Data;
using System.Net;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NumGameWeb
{
    public class UpdateHub : Hub
    {
        public IServices _service { get; }
        public UpdateHub(IServices service)
        {
            _service = service;
        }
        public async Task<BetSaveResponce> ConfirmBeting(string BettingJson)
        {
            if (!string.IsNullOrEmpty(BettingJson))
            {
                var listBet = JsonSerializer.Deserialize<List<Betting>>(BettingJson);

                BettingSaveRequest BetRequest = new BettingSaveRequest();
                BetRequest.token = await _service.GetUserToken();
                BetRequest.created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                BetRequest.bettingInfo = listBet;

                return await _service.SaveBettingData(BetRequest);


            }
            else
            {
                return null;

            }

        }
        
    }
}
