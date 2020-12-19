using System;
using System.Text;

static class TextTransform
{
    public static string RemoveNonAlphanum(string str)
    {
        var sb = new StringBuilder();
        int ix = 0;
        while (ix < str.Length)
        {
            char ch = str[ix];
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                ix++;
            }
            else if (char.IsWhiteSpace(ch))
            {
                sb.Append(ch);
                while (ix < str.Length && char.IsWhiteSpace(str[ix]))
                    ix++;
            }
            else
                ix++;
        }

        return sb.ToString();
    }
}