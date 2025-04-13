using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public static class InputValidator
{
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
        // Common HTML attributes that can be abused for XSS
        "href=",
    };

    /// <summary>
    /// Checks if the input contains any known SQL injection or XSS keyword/pattern 
    /// (case-insensitive). Returns false if suspicious, true if none found.
    /// </summary>
    public static bool IsSafeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true; // Empty is "safe" from this naive standpoint

        string lower = input.ToLower();

        // Check SQL injection patterns
        foreach (var keyword in SQLInjectionKeywords)
        {
            if (lower.Contains(keyword))
            {
                return false;
            }
        }

        // Check XSS patterns
        foreach (var keyword in XSSKeywords)
        {
            if (lower.Contains(keyword))
            {
                return false;
            }
        }

        return true;
    }
}
