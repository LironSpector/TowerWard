using System.Text.RegularExpressions;

/// <summary>
/// Description:
/// Provides a static method to validate a password against specific complexity requirements.
/// The password must pass all requirements. If the password fails any of these tests, an appropriate
/// error message is returned via the out parameter.
/// </summary>
public static class PasswordValidator
{
    /// <summary>
    /// Validates that a given password meets the following security requirements:
    /// - Length between 8 and 16 characters.
    /// - At least one lowercase letter.
    /// - At least one uppercase letter.
    /// - At least one digit.
    /// - At least one symbol (non-alphanumeric character).
    /// </summary>
    /// <param name="password">The password string to validate.</param>
    /// <param name="errorMessage">
    /// Output parameter containing an error message if the password is invalid.
    /// </param>
    /// <returns>True if the password meets all criteria; otherwise, false.</returns>
    public static bool IsValid(string password, out string errorMessage)
    {
        // Check for minimum length.
        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters long.";
            return false;
        }

        // Check for maximum length.
        if (password.Length > 16)
        {
            errorMessage = "Password must be up to 16 characters long.";
            return false;
        }

        // Ensure there is at least one lowercase letter.
        if (!Regex.IsMatch(password, "[a-z]"))
        {
            errorMessage = "Password must contain at least one lowercase letter.";
            return false;
        }

        // Ensure there is at least one uppercase letter.
        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }

        // Ensure there is at least one digit.
        if (!Regex.IsMatch(password, "[0-9]"))
        {
            errorMessage = "Password must contain at least one digit.";
            return false;
        }

        // Ensure there is at least one symbol (a non-alphanumeric character).
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            errorMessage = "Password must contain at least one symbol.";
            return false;
        }

        // All checks passed.
        errorMessage = string.Empty;
        return true;
    }
}
