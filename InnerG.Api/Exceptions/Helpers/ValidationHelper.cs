using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
namespace InnerG.Api.Exceptions.Helpers
{
    public static class ValidationHelper
    {
        public static void FromModelState(ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
            {
                return;
            }
            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => JsonNamingPolicy.CamelCase.ConvertName(x.Key),
                    x => x.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray());

            throw new ValidationException(errors);
        }
    }
}