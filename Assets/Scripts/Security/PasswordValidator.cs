using System.Text.RegularExpressions;

public static class PasswordValidator
{
    /// <summary>
    /// Validates that a given password meets the following requirements:
    /// - At least 8 characters long, and up to 16 characters long
    /// - Contains at least one lowercase letter
    /// - Contains at least one uppercase letter
    /// - Contains at least one digit
    /// - Contains at least one symbol
    /// </summary>
    /// <param name="password">The password string to validate.</param>
    /// <param name="errorMessage">An error message if validation fails.</param>
    /// <returns>True if the password meets all criteria; otherwise, false.</returns>
    public static bool IsValid(string password, out string errorMessage)
    {
        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters long.";
            return false;
        }
        if (password.Length > 16)
        {
            errorMessage = "Password must be up to 16 characters long.";
            return false;
        }
        if (!Regex.IsMatch(password, "[a-z]"))
        {
            errorMessage = "Password must contain at least one lowercase letter.";
            return false;
        }
        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }
        if (!Regex.IsMatch(password, "[0-9]"))
        {
            errorMessage = "Password must contain at least one digit.";
            return false;
        }
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            errorMessage = "Password must contain at least one symbol.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
