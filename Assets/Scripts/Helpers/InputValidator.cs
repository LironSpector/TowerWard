using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Description:
/// Provides a static method to validate input strings by checking for potential SQL injection and XSS attack patterns.
/// The method performs a case-insensitive search for known keywords or patterns that might indicate malicious input.
/// Returns false if any suspicious pattern is found, and true otherwise.
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// Contains keywords and patterns commonly associated with SQL injection attacks.
    /// </summary>
    private static readonly List<string> SQLInjectionKeywords = new List<string>
    {
        "select ", "select*",
        "drop ", "drop table", "drop database",
        "delete ", "delete from",
        "insert ", "insert into",
        "update ", "update ",
        "truncate ",
        "create ", "create table", "create database", "create user",
        "alter ", "alter table",
        "rename ",
        "xp_", "sp_",
        "exec ", "execute ",
        "declare ",
        "union ", " union ", "union select",
        " or 1=1", " and 1=1", " or '1'='1", " and '1'='1", " or \"1\"=\"1",
        " group by ", " having ", " order by ",
        ";--", "-- ", "/*", "*/",
        "sleep(", "benchmark(",
        " cast(", " char(", " nchar(", " varchar(", " nvarchar(",
    };

    /// <summary>
    /// Contains keywords and patterns commonly associated with Cross-Site Scripting (XSS) attacks.
    /// </summary>
    private static readonly List<string> XSSKeywords = new List<string>
    {
        "<script", "script>",
        "<iframe", "</iframe",
        "<object", "</object",
        "<embed", "</embed",
        "<style", "</style",
        "<svg", "</svg",
        "<link",
        "javascript:",
        "onerror=", "onload=", "onclick=", "onmouseover=", "onfocus=",
        "alert(", "prompt(", "confirm(", "eval(",
        "href=",
    };

    /// <summary>
    /// Checks if the provided input string is safe by ensuring it does not contain known SQL injection
    /// or XSS-related keywords and patterns. The check is performed in a case-insensitive manner.
    /// </summary>
    /// <param name="input">The string input to validate.</param>
    /// <returns>
    /// True if the input does not contain any suspicious keywords; otherwise, false.
    /// An empty or null input is considered safe.
    /// </returns>
    public static bool IsSafeInput(string input)
    {
        // If input is null or empty, return true as it is considered safe.
        if (string.IsNullOrEmpty(input))
            return true;

        // Convert the input to lowercase for case-insensitive comparison.
        string lower = input.ToLower();

        // Check for any SQL injection-related keywords.
        foreach (var keyword in SQLInjectionKeywords)
        {
            if (lower.Contains(keyword))
            {
                return false;
            }
        }

        // Check for any XSS-related keywords.
        foreach (var keyword in XSSKeywords)
        {
            if (lower.Contains(keyword))
            {
                return false;
            }
        }

        // If no suspicious patterns were found, the input is considered safe.
        return true;
    }
}
