using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.StringExtensions
{

    public static class StringExtensions
    {
        #region IsUpper
        public static bool IsUpper(this string value)
        {
            // Consider string to be uppercase if it has no lowercase letters.
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsLower(value[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region IsLower
        public static bool IsLower(this string value)
        {
            // Consider string to be lowercase if it has no uppercase letters.
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

    }

}

namespace General
{
    public class StringFunctions
    {
        #region IsNullOrWhiteSpace
        //This method was introduced in .Net 4, it's here for 3.5 compatibility
        public static bool IsNullOrWhiteSpace(String value)
        {
            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!Char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }
        #endregion

        #region MaskString
        public static string MaskString(string Input, int CharsToShow, char Mask)
        {
            int intCharsToShow = Math.Min(CharsToShow, Input.Length);
            return new String(Mask, Input.Length - intCharsToShow) + Input.Substring(Input.Length - intCharsToShow);
        }
        #endregion

        #region ProperCase
        /// <summary>
        /// Returns a string in Proper Case (every word capitalized)
        /// </summary>
        public static string ProperCase(string input)
        {
            StringBuilder newString = new StringBuilder();
            StringBuilder nextString = new StringBuilder();
            string[] phraseArray;
            string theWord;
            string returnValue;
            phraseArray = input.Split(null);
            for (int i = 0; i < phraseArray.Length; i++)
            {
                theWord = phraseArray[i].ToLower();
                if (theWord.Length > 1)
                {
                    if (theWord.Substring(1, 1) == "'")
                    {
                        //Process word with apostrophe at position 1 in 0 based string.
                        if (nextString.Length > 0)
                            nextString.Replace(nextString.ToString(), null);
                        nextString.Append(theWord.Substring(0, 1).ToUpper());
                        nextString.Append("'");
                        nextString.Append(theWord.Substring(2, 1).ToUpper());
                        nextString.Append(theWord.Substring(3).ToLower());
                        nextString.Append(" ");
                    }
                    else
                    {
                        if (theWord.Length > 1 && theWord.Substring(0, 2) == "mc")
                        {
                            //Process McName.
                            if (nextString.Length > 0)
                                nextString.Replace(nextString.ToString(), null);
                            nextString.Append("Mc");
                            nextString.Append(theWord.Substring(2, 1).ToUpper());
                            nextString.Append(theWord.Substring(3).ToLower());
                            nextString.Append(" ");
                        }
                        else
                        {
                            if (theWord.Length > 2 && theWord.Substring(0, 3) == "mac")
                            {
                                //Process MacName.
                                if (nextString.Length > 0)
                                    nextString.Replace(nextString.ToString(), null);
                                nextString.Append("Mac");
                                nextString.Append(theWord.Substring(3, 1).ToUpper());
                                nextString.Append(theWord.Substring(4).ToLower());
                                nextString.Append(" ");
                            }
                            else
                            {
                                //Process normal word (possible apostrophe near end of word.
                                if (nextString.Length > 0)
                                    nextString.Replace(nextString.ToString(), null);
                                nextString.Append(theWord.Substring(0, 1).ToUpper());
                                nextString.Append(theWord.Substring(1).ToLower());
                                nextString.Append(" ");
                            }
                        }
                    }
                }
                else
                {
                    //Process normal single character length word.
                    if (nextString.Length > 0)
                        nextString.Replace(nextString.ToString(), null);
                    nextString.Append(theWord.ToUpper());
                    nextString.Append(" ");
                }
                newString.Append(nextString);
            }
            returnValue = newString.ToString();
            return returnValue.Trim();


        }
        #endregion

        #region IsUpper
        /// <summary>
        /// Returns true if character is upper case
        /// </summary>
        public static bool IsUpper(char ch)
        {
            return Char.IsUpper(ch);
        }

        public static bool IsUpper(string value)
        {
            return General.StringExtensions.StringExtensions.IsUpper(value);
        }
        #endregion

        #region IsLower
        /// <summary>
        /// Returns true if character is lower case
        /// </summary>
        public static bool IsLower(char ch)
        {
            return Char.IsLower(ch);
        }

        public static bool IsLower(string value)
        {
            return General.StringExtensions.StringExtensions.IsLower(value);
        }
        #endregion

        #region IsNumeric
        /// <summary>
        /// Returns true if string contains only digits
        /// </summary>
        public static bool IsNumeric(string input)
        {
            return IsNumeric(input, false);
        }

        /// <summary>
        /// Returns true if string contains only digits
        /// </summary>
        public static bool IsNumeric(string input, bool decimalAllowed)
        {
            if (decimalAllowed)
                return (System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9.]+$"));
            return (System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9]+$"));
        }
        #endregion

        #region IsAlphaOnly
        /// <summary>
        /// Returns true if string contains only letters
        /// </summary>
        public static bool IsAlphaOnly(string input)
        {
            return (System.Text.RegularExpressions.Regex.IsMatch(input, "^[a-zA-Z_\\s-]+$"));
        }
        #endregion

        #region IsAlphaNumeric
        /// <summary>
        /// Returns true if string contains only letters and numbers
        /// </summary>
        public static bool IsAlphaNumeric(string input)
        {
            return (System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9a-zA-Z_\\s-]+$"));
        }
        #endregion

        #region CountAlphaLetters
        /// <summary>
        /// Returns the number of letters found in a string
        /// </summary>
        public static int CountAlphaLetters(string input)
        {
            return System.Text.RegularExpressions.Regex.Matches(input, "[a-zA-Z]").Count;
        }
        #endregion

        #region ForceAlphaNumeric
        /// <summary>
        /// Returns a string stripped of all non-numeric characters
        /// </summary>
        public static string ForceAlphaNumeric(string input)
        {
            return ForceAlphaNumeric(input, false);
        }

        public static string ForceAlphaNumeric(string input, bool blnAllowHyphen)
        {
            if (input == null)
                return "";
            else if (blnAllowHyphen)
                return (System.Text.RegularExpressions.Regex.Replace(input, @"[^A-Za-z0-9_-]*", ""));
            else
                return (System.Text.RegularExpressions.Regex.Replace(input, @"\W*", ""));
        }
        #endregion

        #region ForceNumeric
        /// <summary>
        /// Returns a string stripped of all non-numeric characters
        /// </summary>
        public static string ForceNumeric(string input)
        {
            if (input == null)
                return "0";
            else
                return (System.Text.RegularExpressions.Regex.Replace(input, "[^0-9.]+", ""));
        }
        #endregion

        #region ForceInteger
        /// <summary>
        /// Returns a string stripped of all non-numeric characters
        /// </summary>
        public static string ForceInteger(string input)
        {
            if (input == null)
                return "0";
            else
                return (System.Text.RegularExpressions.Regex.Replace(input, "[^0-9]+", ""));
        }
        #endregion

        #region TwoDigitFormat
        /// <summary>
        /// Returns a string with a leading zero on digits less than 10
        /// </summary>
        public static string TwoDigitFormat(int input)
        {
            string result;
            result = input.ToString();
            if (result.Length == 1)
            {
                result = "0" + result;
            }
            return (result);
        }
        #endregion

        #region Shave
        /// <summary>
        /// Removes a specified number of characters from the end of a string
        /// </summary>
        public static string Shave(string input, int count)
        {
            string result = input;
            if (count > 0)
            {
                if (input.Length > count - 1)
                    result = Left(input, input.Length - count);
                else
                    result = String.Empty;
            }
            else if (count < 0) //Shave from beginning of string
            {
                count = Math.Abs(count);
                if (input.Length > count - 1)
                    result = Right(input, input.Length - count);
                else
                    result = String.Empty;
            }
            return (result);
        }
        #endregion

        #region Left
        /// <summary>
        /// Returns a specified number of characters from the beginning of a string
        /// </summary>
        public static string Left(string input, int length)
        {
            if (input.Length <= length)
                return input;
            return input.Substring(0, length);
        }
        #endregion

        #region Right
        /// <summary>
        /// Returns a specified number of characters from the end of a string
        /// </summary>
        public static string Right(string input, int length)
        {
            string result;
            result = input.Substring((input.Length - length), length);
            return (result);
        }
        #endregion

        #region AllBefore
        /// <summary>
        /// Returns a portion of a string before a specified position
        /// </summary>
        public static string AllBefore(string input, int end_pos)
        {
            if (end_pos <= 0)
                return "";
            return Left(input, end_pos - 1);
        }

        /// <summary>
        /// Returns a portion of a string before a specified search string
        /// </summary>
        public static string AllBefore(string input, string search)
        {
            string result;
            if (search == "")
                result = "";
            else if (!Contains(input, search))
                result = "";
            else
                result = AllBefore(input, input.IndexOf(search) + search.Length);
            return result;
        }
        #endregion

        #region AllBetween
        /// <summary>
        /// Returns a portion of a string from beginning and ending search string
        /// </summary>
        public static string AllBetween(string input, string search_start, string search_end)
        {
            return (AllBetween(input, search_start, search_end, false));
        }

        /// <summary>
        /// Returns a portion of a string from beginning and ending search string
        /// </summary>
        public static string AllBetween(string input, string search_start, string search_end, bool include_search)
        {
            string result;
            if (input.IndexOf(search_start) == -1 || input.IndexOf(search_end) == -1)
                throw new Exception("No match found for search query. (" + search_start + " <-> " + search_end + ")");
            if (include_search)
            {
                result = input.Substring(input.IndexOf(search_start), input.IndexOf(search_end, input.IndexOf(search_start) + search_start.Length) - input.IndexOf(search_start) + search_end.Length);
            }
            else
                result = input.Substring(input.IndexOf(search_start) + search_start.Length, input.IndexOf(search_end, input.IndexOf(search_start) + search_start.Length) - (input.IndexOf(search_start) + search_start.Length));
            return (result);
        }
        #endregion

        #region AllAfter
        /// <summary>
        /// Returns a portion of a string after a specified position
        /// </summary>
        public static string AllAfter(string input, int start_pos)
        {
            string result;
            if (start_pos + 1 > input.Length)
                result = "";
            else
                result = input.Substring(start_pos + 1, (input.Length - (start_pos + 1)));
            return (result);
        }

        /// <summary>
        /// Returns a portion of a string after a specified search string
        /// </summary>
        public static string AllAfter(string input, string search)
        {
            return AllAfter(input, search, false);
        }

        /// <summary>
        /// Returns a portion of a string after a specified search string
        /// </summary>
        public static string AllAfter(string input, string search, bool IgnoreCase)
        {
            string result;
            if (search == "")
                result = "";
            else if (!Contains(input, search, IgnoreCase))
                result = "";
            else
                result = AllAfter(input, input.IndexOf(search, StringComparison.OrdinalIgnoreCase) + (search.Length - 1));
            return result;
        }

        /// <summary>
        /// Returns a portion of a string after a specified search string
        /// </summary>
        public static string AllAfterReverse(string input, string search)
        {
            string result;
            if (search == "")
                result = "";
            else if (!Contains(input, search))
                result = "";
            else
                result = AllAfter(input, input.LastIndexOf(search) + (search.Length - 1));
            return result;
        }
        #endregion

        #region Contains
        /// <summary>
        /// Returns true if a string contains the specified search string
        /// </summary>
        public static bool Contains(string Input, string Search)
        {
            if (Input.IndexOf(Search) == -1)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns true if a string contains the specified search string
        /// </summary>
        public static bool Contains(string Input, string Search, bool IgnoreCase)
        {
            if (!IgnoreCase)
                return Contains(Input, Search);

            if (Input.IndexOf(Search, StringComparison.OrdinalIgnoreCase) == -1)
                return false;
            else
                return true;
        }
        #endregion

        #region ContainsBefore
        /// <summary>
        /// Returns true if a string contains the specified search string before an anchor point
        /// </summary>
        public static bool ContainsBefore(string Input, string Search, string SearchBefore)
        {
            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.IndexOf(Search) > Input.IndexOf(SearchBefore))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns true if a string contains the specified search string before an anchor point
        /// </summary>
        public static bool ContainsBefore(string Input, string Search, int SearchBefore)
        {
            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.IndexOf(Search) > SearchBefore)
                return false;
            else
                return true;
        }
        #endregion

        #region ContainsBeforeDigits
        /// <summary>
        /// Returns true if a string contains digits before a search string
        /// </summary>
        public static bool ContainsBeforeDigits(string Input, string Search)
        {
            char[] aryDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.IndexOf(Search) > Input.IndexOfAny(aryDigits))
                return false;
            else
                return true;
        }
        #endregion

        #region ContainsAfter
        /// <summary>
        /// Returns true if a string contains the specified search string after an anchor point
        /// </summary>
        public static bool ContainsAfter(string Input, string Search, string SearchAfter)
        {
            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.LastIndexOf(Search) < Input.IndexOf(SearchAfter))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns true if a string contains the specified search string after an anchor point
        /// </summary>
        public static bool ContainsAfter(string Input, string Search, int SearchAfter)
        {
            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.LastIndexOf(Search) < SearchAfter)
                return false;
            else
                return true;
        }
        #endregion

        #region ContainsAfterDigits
        /// <summary>
        /// Returns true if a string contains digits after a search string
        /// </summary>
        public static bool ContainsAfterDigits(string Input, string Search)
        {
            char[] aryDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (Input.IndexOf(Search) == -1)
                return false;
            else if (Input.LastIndexOf(Search) < Input.IndexOfAny(aryDigits))
                return false;
            else
                return true;
        }
        #endregion

        #region ContainsHTML
        /// <summary>
        /// Returns true if a string contains any HTML markup
        /// </summary>
        private static System.Text.RegularExpressions.Regex rxHTMLMatch = new System.Text.RegularExpressions.Regex("</?\\w+((\\s+\\w+(\\s*=\\s*(?:\".*?\"|'.*?'|[^'\">\\s]+))?)+\\s*|\\s*)/?>", System.Text.RegularExpressions.RegexOptions.Compiled);
        public static bool ContainsHTML(string Input)
        {
            return (rxHTMLMatch.IsMatch(Input));
        }
        #endregion

        #region Count
        /// <summary>
        /// Returns the number of matches found in a string from a specified search string
        /// </summary>
        public static int Count(string Input, string Search)
        {
            if (!Contains(Input, Search))
                return (0);
            else
            {
                int intIndex = 0;
                int intCount = 0;
                while ((intIndex = Input.IndexOf(Search, intIndex)) != -1)
                {
                    intCount++;
                    intIndex++;
                }
                return intCount;
            }
        }
        #endregion

        #region StartsWith
        /// <summary>
        /// Returns true if a string starts with a specified search string
        /// </summary>
        public static bool StartsWith(string Input, string Search)
        {
            if (Search.Length > Input.Length)
                return (false);

            if (Left(Input, Search.Length) == Search)
                return (true);
            else
                return (false);
        }
        #endregion

        #region JSEncode
        /// <summary>
        /// Replaces ' with &quot;
        /// </summary>
        public static string JSEncode(string input)
        {
            return (input.Replace("'", "&rsquo;").Replace("\"", "&quot;"));
        }
        #endregion

        #region RepeatString
        /// <summary>
        /// Builds a string containing the specified string
        /// repeated the specified number of times.
        /// </summary>
        /// <param name="s">string - The string to repeat</param>
        /// <param name="r">int - The number of times to repeat</param>
        /// <returns>string</returns>
        public static string RepeatString(string s, int r)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < r; i++) { sb.Append(s); }
            return sb.ToString();
        }
        #endregion

        #region ReplaceCharacters
        /// <summary>
        /// Replaces all instances of each string in an array of strings with a
        /// specified string.
        /// </summary>
        /// <param name="strSubject">string - The subject string in which to replace characters</param>
        /// <param name="astrFrom">string[] - An array of strings to replace</param>
        /// <param name="strTo">string - The string that will replace each string in the array</param>
        /// <returns>string</returns>
        public static string ReplaceCharacters(string strSubject, string[] astrFrom, string strTo)
        {
            #region Validation
            if (strSubject == null || strSubject == string.Empty) return string.Empty;
            #endregion

            foreach (string strReplace in astrFrom)
            {
                strSubject = strSubject.Replace(strReplace, strTo);
            }
            return strSubject;
        }
        #endregion

        #region ReplaceCaseInsensitive
        public static string ReplaceCaseInsensitive(string original,
                    string pattern, string replacement)
        {
            if (replacement == null)
                replacement = String.Empty;
            if (original.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == -1)
                return original;
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }
        #endregion

        #region ReplaceOnce
        public static string ReplaceOnce(string strInput, string strFind, string strReplace)
        {
            int intStartIndex = strInput.IndexOf(strFind);
            if (intStartIndex > -1)
                return strInput.Remove(intStartIndex, strFind.Length).Insert(intStartIndex, strReplace);
            else
                return strInput;
        }
        #endregion

        #region RemoveAccentCharacters
        /// <summary>
        /// Replaces all oddly accented characters in a string with there basic latin equivalent.
        /// specified string.
        /// </summary>
        /// <param name="strInput">string - The subject string in which to replace characters</param>
        /// <returns>string</returns>
        public static string RemoveAccentCharacters(string strInput)
        {
            #region Validation
            if (strInput == null || strInput == string.Empty) return string.Empty;
            #endregion

            string strTarget;

            strTarget = "à,á,â,ã,ä,å,a,a,a";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "a");
            strTarget = "ÀÁÂÃÄÅAAA";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "A");

            strTarget = "è,é,ê,ë,e,e,e,e,e";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "e");
            strTarget = "È,É,Ê,Ë,E,E,E,E,E";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "E");

            strTarget = "ì,í,î,ï,i,i,i";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "i");
            strTarget = "Ì,Í,Î,Ï,I,I,I";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "I");

            strTarget = "ò,ó,ô,õ,ö,ø,o,o,o";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "o");
            strTarget = "Ò,Ó,Ô,Õ,Ö,Ø,O,O,O";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "O");

            strTarget = "ù,ú,û,ü,u,u,u,u,u,u,u,u,u";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "u");
            strTarget = "Ù,Ú,Û,Ü,U,U,U,U,U,U,U,U,U,U,U";
            strInput = ReplaceCharacters(strInput, strTarget.Split(','), "U");




            return strInput;
        }
        #endregion

        #region ConvertCamelCaseToWords
        public static string ConvertCamelCaseToWords(string strInput)
        {
            string strOutput = System.Text.RegularExpressions.Regex.Replace(
                strInput,
                "([A-Z])",
                " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
            return strOutput;
        }
        #endregion

        #region MakeNameURLSafe
        public static string MakeNameURLSafe(string strName)
        {
            //return strName.Replace(" ", "_").Replace("&", "").Replace("/", "_").Replace("\\", "_").Replace("'","").Replace("\"","");
            return ForceAlphaNumeric(strName.Replace(" ", "_"));
        }
        #endregion

        #region NormalizeLineBreaks
        public static string NormalizeLineBreaks(string input)
        {
            // Allow 10% as a rough guess of how much the string may grow.
            // If we're wrong we'll either waste space or have extra copies -
            // it will still work
            StringBuilder builder = new StringBuilder((int)(input.Length * 1.1));

            bool lastWasCR = false;

            foreach (char c in input)
            {
                if (lastWasCR)
                {
                    lastWasCR = false;
                    if (c == '\n')
                    {
                        continue; // Already written \r\n
                    }
                }
                switch (c)
                {
                    case '\r':
                        builder.Append("\r\n");
                        lastWasCR = true;
                        break;
                    case '\n':
                        builder.Append("\r\n");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            return builder.ToString();
        }
        #endregion

        #region GetOrdinalNumber
        public static string GetOrdinalNumber(int number)
        {
            string suffix = String.Empty;

            int ones = number % 10;
            int tens = (int)Math.Floor(number / 10M) % 10;

            if (tens == 1)
            {
                suffix = "th";
            }
            else
            {
                switch (ones)
                {
                    case 1:
                        suffix = "st";
                        break;

                    case 2:
                        suffix = "nd";
                        break;

                    case 3:
                        suffix = "rd";
                        break;

                    default:
                        suffix = "th";
                        break;
                }
            }
            return String.Format("{0}{1}", number, suffix);
        }
        #endregion

    }
}
