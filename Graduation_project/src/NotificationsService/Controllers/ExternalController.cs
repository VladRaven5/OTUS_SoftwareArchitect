using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shared;

namespace NotificationsService
{
    [ApiController]
    [Route("")]
    public class ExternalController : ControllerBase
    {
        private readonly NotificationsRepository _notificationsRepository;

        public ExternalController(NotificationsRepository notificationsRepository)
        {
            _notificationsRepository = notificationsRepository;
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetNotifications()
        {
            return Ok(await _notificationsRepository.GetNotificationsAsync());
        }

        /// <summary>
        /// Get notifications of current user (by X-UserId header)
        /// </summary>
        /// <returns></returns>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<NotificationModel>>> GetMyNotifications()
        {
            if(!Request.Headers.TryGetValue(Constants.UserIdHeaderName, out StringValues userIdValue))
            {
                return BadRequest($"Header {Constants.UserIdHeaderName} must be specified");
            }

            string userId = userIdValue.ToString();

            return Ok(await _notificationsRepository.GetUserNotificationsAsync(userId));
        }
    }
}