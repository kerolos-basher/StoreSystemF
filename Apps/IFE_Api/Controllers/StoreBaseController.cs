

using Infrastructure.Services.LogFile;
using Microsoft.AspNetCore.Mvc;
using Resources;

namespace Store_Api.Controllers;


public class StoreBaseController : Controller
{


    protected readonly LogFileService Logger;
    protected readonly IStoreUOW UOW;

    public StoreBaseController(LogFileService logger, IStoreUOW uow)
    {
        this.Logger = logger;
        this.UOW = uow;
    }
    protected IActionResult TryCatchLog(Func<IActionResult> function)
    {
        try
        {
            return function.Invoke();
        }
        catch (StoreException ex)
        {
            this.Logger.LogException(ex);
            //LoggerInstance.LogError(ex.ToString());
            return StatusCode(402, ex.StoreExceptionMessage);
        }
        catch (AggregateException aggEx)
        {
            if (aggEx.InnerException is StoreException)
            {
                this.Logger.LogValidation(aggEx);
                //LoggerInstance.LogError(aggEx.ToString());

                return StatusCode(402, (aggEx.InnerException as StoreException).StoreExceptionMessage);
            }
            else
            {
                this.Logger.LogValidation(aggEx);
                //LoggerInstance.LogError(aggEx.ToString());

                return StatusCode(500, ExceptionMessage.UnHandledException);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogException(ex);
            //LoggerInstance.LogError(ex.ToString());

            return StatusCode(500, ExceptionMessage.UnHandledException);
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
            this.Logger.LogException(ex);
            //LoggerInstance.LogError(ex.ToString());

            if (ex.StoreExceptionMessage.Trim().Contains(ExceptionMessage.LockedUser))
                return StatusCode(515, new { message = ex.StoreExceptionMessage.Trim() });

            return StatusCode(402, new { message = ex.StoreExceptionMessage.Trim() });
        }
        catch (AggregateException aggEx)
        {
            if (aggEx.InnerException is StoreException)
            {
                this.Logger.LogValidation(aggEx);
                //LoggerInstance.LogError(aggEx.ToString());

                return StatusCode(402, (aggEx.InnerException as StoreException).Message);
            }
            else
            {
                //LoggerInstance.LogError(aggEx.ToString());
                this.Logger.LogValidation(aggEx);

            }

            return StatusCode(402, (aggEx.InnerException as StoreException)?.StoreExceptionMessage ?? "AggregateException occurred");
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            this.Logger.LogException(ex);
            //LoggerInstance.LogError(ex.ToString());
            return StatusCode(500, new { message = ExceptionMessage.UnHandledException, detail = inner });
        }
    }
    protected long GetUserId()
    {
        long _userId;
        return long.TryParse(User.Claims.FirstOrDefault(s => s.Type == "AspNetUserId")?.Value, out _userId)
            ? _userId : throw new StoreException(ExceptionMessage.Unauthorized);
    }
    protected long GetUserCommitteeId()
    {
        long _committeeId;
        return long.TryParse(User.Claims.FirstOrDefault(s => s.Type == "CommitteeId")?.Value, out _committeeId)
            ? _committeeId : throw new StoreException(ExceptionMessage.Unauthorized);
    }
    protected bool IsArabic()
    {
        if (string.IsNullOrEmpty(Request.Headers["Accept-Language"].ToString()))
        {
            return true;
        }
        else
        {
            return Request.Headers["Accept-Language"].ToString() == "en-US" ? false : true;
        }
    }



}
