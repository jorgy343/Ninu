using System;
using System.Text;

namespace Ninu.Assembler.Library
{
    public class Preprocessor
    {
        public string Parse(string source)
        {
            var sb = new StringBuilder(source.Length);
            var position = 0;

            bool TryGetNext(out char value)
            {
                if (position < source.Length)
                {
                    value = source[position];
                    return true;
                }

                value = default;
                return false;
            }

            bool TryPeak(out char value)
            {
                if (position + 1 < source.Length)
                {
                    value = source[position + 1];
                    return true;
                }

                value = default;
                return false;
            }

            var beginningOfLine = true;

            while (TryGetNext(out var value))
            {
                switch (value)
                {
                    case '#':
                        if (beginningOfLine)
                        {

                        }
                        else
                        {
                            sb.Append(value);
                        }
                        break;

                    case '\r':
                    case '\n':
                        beginningOfLine = true;
                        break;

                    case ' ':
                    case '\t':
                        sb.Append(value);
                        break;

                    default:
                        beginningOfLine = false;
                        sb.Append(value);
                        break;
                }
            }
        }

        protected bool CheckForString(ReadOnlySpan<char> range, string str)
        {
            for (var i = 0; i < str.Length; i++)
            {
                if (i >= range.Length)
                {
                    return false;
                }

                if (range[i] != str[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}