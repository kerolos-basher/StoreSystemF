
using Application.UOW;
using Infrastructure.Services.LogFile;
using Microsoft.AspNetCore.Mvc;
using Resources;

namespace Store_Api.Controllers;

public abstract class StoreBaseController : ControllerBase
{
    protected readonly LogFileService Logger;
    protected readonly IStoreUOW? UOW;

    protected StoreBaseController(LogFileService logger)
        : this(logger, null)
    {
    }

    protected StoreBaseController(LogFileService logger, IStoreUOW? uow)
    {
        Logger = logger;
        UOW = uow;
    }

    protected IActionResult TryCatchLog(Func<IActionResult> function)
    {
        try
        {
            return function.Invoke();
        }
        catch (StoreException ex)
        {
            Logger.LogException(ex);
            if (ex.StoreExceptionMessage.Trim().Contains(ExceptionMessage.LockedUser))
                return StatusCode(515, new { message = ex.StoreExceptionMessage.Trim() });

            return StatusCode(402, new { message = ex.StoreExceptionMessage.Trim() });
        }
        catch (AggregateException aggEx)
        {
            if (aggEx.InnerException is StoreException storeEx)
            {
                Logger.LogValidation(aggEx);
                return StatusCode(402, new { message = storeEx.StoreExceptionMessage });
            }

            Logger.LogValidation(aggEx);
            return StatusCode(500, new { message = ExceptionMessage.UnHandledException });
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            return StatusCode(500, new { message = ExceptionMessage.UnHandledException, detail = ex.InnerException?.Message ?? ex.Message });
        }
    }

    protected async Task<IActionResult> TryCatchLogAsync(Func<Task<IActionResult>> function)
    {
        try
        {
            return await function.Invoke();
        }
        catch (StoreException ex)
        {
            Logger.LogException(ex);

            if (ex.StoreExceptionMessage.Trim().Contains(ExceptionMessage.LockedUser))
                return StatusCode(515, new { message = ex.StoreExceptionMessage.Trim() });

            return StatusCode(402, new { message = ex.StoreExceptionMessage.Trim() });
        }
        catch (AggregateException aggEx)
        {
            if (aggEx.InnerException is StoreException storeEx)
            {
                Logger.LogValidation(aggEx);
                return StatusCode(402, new { message = storeEx.StoreExceptionMessage });
            }

            Logger.LogValidation(aggEx);
            return StatusCode(402, (aggEx.InnerException as StoreException)?.StoreExceptionMessage ?? "AggregateException occurred");
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            Logger.LogException(ex);
            return StatusCode(500, new { message = ExceptionMessage.UnHandledException, detail = inner });
        }
    }

    protected long GetUserId()
    {
        return long.TryParse(User.Claims.FirstOrDefault(s => s.Type == "AspNetUserId")?.Value, out var userId)
            ? userId
            : throw new StoreException(ExceptionMessage.Unauthorized);
    }

    protected long GetUserCommitteeId()
    {
        return long.TryParse(User.Claims.FirstOrDefault(s => s.Type == "CommitteeId")?.Value, out var committeeId)
            ? committeeId
            : throw new StoreException(ExceptionMessage.Unauthorized);
    }

    protected bool IsArabic()
    {
        if (string.IsNullOrEmpty(Request.Headers.AcceptLanguage.ToString()))
            return true;

        return Request.Headers.AcceptLanguage.ToString() != "en-US";
    }
}
