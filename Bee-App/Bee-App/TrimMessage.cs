using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Globalization;

public class TrimMessage
{
    List<string> messageList = new List<string>();

    public string CleanMessage(string txt)
    {
        string[] messages = Regex.Split(txt, "(?<=[;\\r?\\n])");
        string result = "";
        foreach (string message in messages)
        {
            if (messageList.IndexOf(message) == -1)
            {
                messageList.Add(message);
                result += RemoveWhiteSpaces(message);
            }
        }
        return result;
    }
    private string RemoveWhiteSpaces(string txt)
    {
        return Regex.Replace(txt, @"\t|\n|\r", "");
    }
}