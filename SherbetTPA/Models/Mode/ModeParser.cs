using System;
using RocketExtensions.Interfaces;
using RocketExtensions.Models;

namespace SherbetTPA.Models.Mode
{
    public class ModeParser : IStringParser
    {
        public Type Type => typeof(EMode);

        public T Parse<T>(string input, out EParseResult parseResult)
        {
            switch (input.ToLowerInvariant())
            {
                case "a":
                case "accept":
                    parseResult = EParseResult.Parsed;
                    return (T)(object)EMode.Accept;

                case "d":
                case "deny":
                    parseResult = EParseResult.Parsed;
                    return (T)(object)EMode.Deny;

                case "ab":
                case "abort":
                    parseResult = EParseResult.Parsed;
                    return (T)(object)EMode.Abort;

                default:
                    parseResult = EParseResult.Parsed;
                    return (T)(object)EMode.Request;
            }
        }
    }
}