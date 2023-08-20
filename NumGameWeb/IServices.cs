using NumGameWeb.Data;

namespace NumGameWeb
{
    public interface IServices
    {
        IHttpContextAccessor _HttpContextAccessor { get; }
        Task<ResponseResult<List<OpeningNumber>>> GetRecentOpenNumbers();
        Task<UserDetail> GetUserDetail();
        Task<string> GetUserToken();
        Task<int> SaveBettingData(BettingSaveRequest betting);
        List<int> GetMissingBettingNumbers(List<BettingInfo> userBets);
        int GetNumberWithLowestAmount(List<BettingInfo> userBets);
        List<BettingInfo> GetUsersWhoBetOnLowestAmountNumber(List<BettingInfo> userBets, int winNumber);
        Task<List<BettingInfo>> GetAllBettingDetails(string from, string to);
        Task SaveOpenNumber(int number);
        Task<int> UpdateUserWallet(int userId, int amount, int number,string token);
        Task<string> GetUserIdBySession();
        Task<string> GetUserID();




    }
}