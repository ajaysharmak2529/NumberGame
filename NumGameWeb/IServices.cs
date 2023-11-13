using NumGameWeb.Data;

namespace NumGameWeb
{
    public interface IServices
    {
        IHttpContextAccessor _HttpContextAccessor { get; }
        Task<UserDetail> GetUserDetail();
        Task<string> GetUserID();

        #region Number Bet Methods
        Task<ResponseResult<List<OpeningNumber>>> GetRecentOpenNumbers();
        string GetUserToken();
        Task<BetSaveResponce> SaveBettingData(BettingSaveRequest betting);
        List<int> GetMissingBettingNumbers(List<BettingInfo> userBets);
        int GetNumberWithLowestAmount(List<BettingInfo> userBets);
        List<BettingInfo> GetUsersWhoBetOnLowestAmountNumber(List<BettingInfo> userBets, int winNumber);
        Task<List<BettingInfo>> GetAllBettingDetails(string from, string to);
        Task<bool> SaveOpenNumber(int number);
        Task<decimal> UpdateUserWallet(int userId, int amount, int number, string token);
        #endregion

        #region Coin Bet Methods
        Task<BetSaveResponce> SaveCoinBet(CoinBetSaveRequest bet);
        Task<bool> SaveOpenCoinSide(string CoinSide);
        Task<List<CoinBetResponse>> GetAllInstallCoinBet(string from, string to);
        Task<WalletResponce> UpdatedCoinWinnerWallet(string token, string coinSide, decimal amount);
        Task<RecentOpenedCoin> GetRecentOpenCoinBet();
        Task<ResponseResult<List<CoinBetResponse>>> GetUserCoinBetHistory(UserHistoryRequest userHistoryRequest);
        #endregion

        Task<UserHistoryResponse> GetUserHistory(UserHistoryRequest userHistoryRequest);
        Task<ResponseResult<ApplicationUser>> RegisterUser(AuthModel authModel);

    }
}