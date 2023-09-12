using Microsoft.AspNetCore.SignalR;
using NumGameWeb.Data;
using System.Text.Json;

namespace NumGameWeb
{
    public class UpdateHub : Hub {
        public IServices _service { get; }
        public UpdateHub(IServices service)  {
            _service = service;
        }
        public async Task<BetSaveResponce> ConfirmBetting(string BettingJson) {
            if (!string.IsNullOrEmpty(BettingJson)) {
                var listBet = JsonSerializer.Deserialize<List<Betting>>(BettingJson);
                BettingSaveRequest BetRequest = new BettingSaveRequest();
                BetRequest.token = _service.GetUserToken();
                BetRequest.created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                BetRequest.bettingInfo = listBet;
                return await _service.SaveBettingData(BetRequest);
            }
            else return null!;
        }

        public async Task<BetSaveResponce> ConfirmCoinBet(string betJson) {
            var listBet = JsonSerializer.Deserialize<List<CoinBet>>(betJson);
            if (listBet?.Count > 0) {
                CoinBetSaveRequest CoinBet = new CoinBetSaveRequest();
                CoinBet.token = _service.GetUserToken();
                CoinBet.created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                CoinBet.bettingInfo = listBet;
                return await _service.SaveCoinBet(CoinBet);
            }
            else return null!;
            
        }


    }
}
