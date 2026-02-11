using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace InnerG.Api.Exceptions.Helpers
{
    public static class IdentityErrorMapper
{
    public static ValidationException ToValidationException(
        IdentityResult result)
    {
        if (result.Succeeded)
            throw new InvalidOperationException(
                "Cannot map successful IdentityResult.");

        var errors = result.Errors
            .GroupBy(e => MapField(e.Code))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        return new ValidationException(errors);
    }

    private static string MapField(string code)
        => code switch
        {
            // Email
            "DuplicateEmail" => "email",
            "InvalidEmail" => "email",

            // Username
            "DuplicateUserName" => "username",
            "InvalidUserName" => "username",

            // Password
            "PasswordTooShort" => "password",
            "PasswordRequiresDigit" => "password",
            "PasswordRequiresUpper" => "password",
            "PasswordRequiresLower" => "password",
            "PasswordRequiresNonAlphanumeric" => "password",

            // Login
            "InvalidUserNameOrPassword" => "credentials",
            "UserLockedOut" => "credentials",

            // Default
            _ => "general"
        };
}
}