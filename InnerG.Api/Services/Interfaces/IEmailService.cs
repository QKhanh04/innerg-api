using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerG.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string to, string subject, string html);
    }
}