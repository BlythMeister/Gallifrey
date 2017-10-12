using Exceptionless;
using Exceptionless.Models;
using Exceptionless.Models.Data;

namespace Gallifrey.UI.Modern.Models
{
    public class ErrorModel
    {
        public Event ExceptionlessEvent { get; }
        public string EmailAddress { get; set; }
        public string Description { get; set; }

        public ErrorModel(Event exceptionlessEvent, string initialEmailAddress)
        {
            ExceptionlessEvent = exceptionlessEvent;
            EmailAddress = initialEmailAddress;
        }

        public void SetUserDetailsInEvent()
        {
            ExceptionlessEvent.SetUserDescription(new UserDescription(EmailAddress, Description));
        }
    }
}