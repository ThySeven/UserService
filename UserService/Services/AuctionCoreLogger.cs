using NLog;
using ILogger = NLog.ILogger;

namespace UserService.Services
{
    public class AuctionCoreLogger
    {
        public static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();
    }
}