using System.Collections.Generic;
using System.Text;
using System.IO;
using Grasshopper.Kernel;

namespace GhPython.Component
{
/*
# based on DocStrings as defined in
# http://google-styleguide.googlecode.com/svn/trunk/pyguide.html#Comments
""""""
Title: Arithmetic Series
Description: Computes the Sum of an Arithmetic Progression, or the
sum of all numbers from F to L, included.
    Args:
        F: the first number included in the series.
        L: the last number included in the series.
    Returns:
        S: If F > L, then sum of all numbers [F,L].
            If F = L, then 0.
            If F < L, then sum of all numbers (L,F).
        K: Not used.
    Help:
        See also the Gauss elementary school story:
        http://mathworld.wolfram.com/ArithmeticSeries.html
""""""
*/
  class DocStringUtils
  {
    public static bool FindApplyDocString(string code, ScriptingAncestorComponent component)
    {
      var reader = new StringReader(code);

      string line;
      for (; ; ) //proceeds to begin of docstrings, or leave method
      {
        line = reader.ReadLine();
        if (line == null) return false;
        if (IsEmptyOrFullyCommentedOutLine(line)) continue;
        if (IsDocStringStart(line)) break;
        return false;
      }

      //strips the docstring start chars
      line = line.Substring(line.IndexOf(_docStringSeparator) + _docStringSeparator.Length);
      int firstLevelIndent = GetIndent(line);
      int secondLevelIndent = -1;

      string variable = "%description";
      StringBuilder result = new StringBuilder();
      KeywordType type = KeywordType.Description;

      do //consumes docstring lines and then leaves
      {
        int endSeparator = line.IndexOf(_docStringSeparator);
        if (endSeparator != -1) line = line.Substring(0, endSeparator);

        if (IsEmptyLine(line)) {
          if (endSeparator != -1) break;
          continue;
        }
        int newIndent = GetIndent(line);
        if (newIndent > firstLevelIndent)
        {
          if (secondLevelIndent == -1 ||
            newIndent <= secondLevelIndent) //second level
          {
            //we could check for faulty indentation here
            secondLevelIndent = newIndent;

            string keyword;
            if (IsNewKeywordDeclared(line, out keyword))
            {
              var nextType = type;
              string nextVariable = null;
              bool match = true;
              switch (keyword.ToUpperInvariant())
              {
                case "ARG":
                case "ARGS":
                case "ARGUMENT":
                case "ARGUMENTS":
                case "INPUT":
                case "INPUTS":
                  nextType = KeywordType.Argument;
                  break;
                case "RETURN":
                case "RETURNS":
                case "OUTPUT":
                case "OUTPUTS":
                  nextType = KeywordType.Return;
                  break;
                case "HELP":
                case "REMARK":
                case "REMARKS":
                  nextType = KeywordType.Help;
                  nextVariable = "%help";
                  break;
                default:
                  match = false;
                  break;
              }

              if (match)
              {
                Send(variable, ref result, type, component);
                AddLine(result, line.Substring(line.IndexOf(":") + 1).TrimStart(_toTrim));
                variable = nextVariable;
                type = nextType;
              }
              else
                AddLine(result, line);
            }
            else
              AddLine(result, line);
          }
          else //third level
          {
            string keyword;
            if (IsNewKeywordDeclared(line, out keyword) && (variable == null || !variable.StartsWith("%")))
            {
              Send(variable, ref result, type, component);
              AddLine(result, line.Substring(line.IndexOf(":") + 1).TrimStart(_toTrim));
              variable = keyword;
            }
            else
              AddLine(result, line.Trim(_toTrim));
          }
        }
        else
          AddLine(result, line);
        if (endSeparator != -1) break;
      }
      while ((line = reader.ReadLine()) != null);

      Send(variable, ref result, type, component);
      return true;
    }

    private static void Send(string variable, ref StringBuilder result, KeywordType type, ScriptingAncestorComponent component)
    {
      if (variable != null)
      {
        switch(type)
        {
          case KeywordType.Description:
            component.Description = result.ToString();
            break;
          case KeywordType.Argument:
            FindAndDescribe(component.Params.Input, variable, result.ToString());
            break;
          case KeywordType.Return:
            FindAndDescribe(component.Params.Output, variable, result.ToString());
            break;
          case KeywordType.Help:
            component.SpecialPythonHelpContent = result.ToString();
            break;
        }
        result = new StringBuilder();
      }
    }

    private static void FindAndDescribe(List<IGH_Param> list, string variable, string p)
    {
      int i = list.FindIndex(match => match.NickName == variable);
      if (i != -1)
      {
        var item = list[i];
        if (item != null)
        {
          item.Description = p;
        }
      }
    }

    enum KeywordType
    {
      Description,
      Argument,
      Return,
      Help,
    }

    //private static void AppendText(StringBuilder result, string text)
    //{
    //  if (result.Length != 0) result.Append(" ");
    //  result.Append(text);
    //}

    private static void AddLine(StringBuilder result, string text)
    {
      if (result.Length != 0) result.AppendLine();
      result.Append(text);
    }

    private static bool IsNewKeywordDeclared(string line, out string keyword)
    {
      keyword = null;
      string piece = line.TrimStart(_toTrim);
      int end = piece.IndexOf(":");
      if (end == -1) return false;
      piece = piece.Substring(0, end).TrimEnd(_toTrim);
      if (piece.IndexOfAny(_toTrim) != -1) return false;
      keyword = piece;
      return true;
    }

    private static int GetIndent(string line)
    {
      int i;
      for (i = 0; i < line.Length; i++)
        if (!(line[i].Equals(' ') || line[i].Equals('\t')))
          break;
      return i;
    }

    static readonly char[] _toTrim = new char[] { ' ', '\t' };
    const string _docStringSeparator = "\"\"\"";

    private static bool IsDocStringStart(string line)
    {
      return line.TrimStart(_toTrim).StartsWith(_docStringSeparator);
    }

    private static bool IsEmptyLine(string line)
    {
      return line.TrimStart(_toTrim).Length == 0;
    }

    private static bool IsEmptyOrFullyCommentedOutLine(string line)
    {
      var after = line.TrimStart(_toTrim);
      if (after.Length == 0)
        return true;
      return after.IndexOf("#") == 0;
    }

    public static string Htmlify(string input)
    {
      string firstRepl = System.Text.RegularExpressions.Regex.Replace(input, "[<>&\"'\n]",
        t =>
        {
          switch (t.Value)
          {
            case "<":
              return "&lt;";
            case ">":
              return "&gt;";
            case "&":
              return "&amp;";
            case "\"":
              return "&#34;";
            case "'":
              return "&#39;";
            case "\n":
              return "<br>";
          }
          return string.Empty;
        });

      return System.Text.RegularExpressions.Regex.Replace
      (firstRepl,
        "(http|https|ftp|sftp|mailto|skype):[A-Za-z0-9!*'\\(\\);:@&=+$,/?#\\[\\]\\-_.~]+",
        t =>
        {
          string shortName = t.Value;
          if (t.Value.StartsWith("http"))
            shortName = shortName.Substring(7);
          if (t.Value.StartsWith("mailto"))
            shortName = shortName.Substring(7);

          return "<a href=\"" + t.Value + "\" target=\"_blank\" title=\"External link to: " +
            t.Value + "\">" + shortName + "</a>";
        });
    }
  }
}