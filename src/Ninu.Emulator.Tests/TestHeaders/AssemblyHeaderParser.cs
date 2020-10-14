using Ninu.Emulator.Tests.Cpu.Expectations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Ninu.Emulator.Tests.TestHeaders
{
    public static class AssemblyHeaderParser
    {
        // ;
        private static readonly Regex _blankComment = new Regex(@"^\s*;\s*$", RegexOptions.Compiled);

        // ; #Init
        private static readonly Regex _init = new Regex(@"^\s*;\s*#Init\s*(;.*)?$", RegexOptions.Compiled);

        // ; #Checkpoint 01
        // ; #Checkpoint c7
        private static readonly Regex _checkpoint = new Regex(@"^\s*;\s*#Checkpoint\s*([0-9a-fA-F]{1,2})\s*(;.*)?$", RegexOptions.Compiled);

        // ; [7] == fa
        // ; [18fb] = 3
        private static readonly Regex _singleMemoryExpectation = new Regex(@"^\s*;\s*\[\s*(?<location>[0-9a-fA-F]{1,4})\s*\]\s*==\s*(?<value>[0-9a-zA-Z]{1,2})\s*(?:;.*)?$", RegexOptions.Compiled);

        // ; [7:92] == fa
        // ; [18fb:39a6] = 3
        private static readonly Regex _scalarRangeMemoryExpectation = new Regex(@"^\s*;\s*\[\s*(?<locationStart>[0-9a-fA-F]{1,4})\s*\:\s*(?<locationEnd>[0-9a-fA-F]{1,4})\s*\]\s*==\s*(?<value>[0-9a-zA-Z]{1,2})\s*(?:;.*)?$", RegexOptions.Compiled);

        // ; [7:92] == fa .. 97
        // ; [18fb:39a6] = 3 .. f
        private static readonly Regex _linearRangeMemoryExpectation = new Regex(@"^\s*;\s*\[\s*(?<locationStart>[0-9a-fA-F]{1,4})\s*\:\s*(?<locationEnd>[0-9a-fA-F]{1,4})\s*\]\s*==\s*(?<valueStart>[0-9a-zA-Z]{1,2})\s*\.\.\s*(?<valueEnd>[0-9a-zA-Z]{1,2})\s*(?:;.*)?$", RegexOptions.Compiled);

        private static readonly Regex _collectionRangeMemoryExpectation = new Regex(@"^\s*;\s*\[\s*(?<locationStart>[0-9a-fA-F]{1,4})\s*\:\s*(?<locationEnd>[0-9a-fA-F]{1,4})\s*\]\s*==\s*(?<firstValue>[0-9a-zA-Z]{1,2})(?:\s+(?<otherValues>[0-9a-zA-Z]{1,2}))+\s*(?:;.*)?$", RegexOptions.Compiled);

        // ; p == fa
        // ; a == fa
        // ; x == fa
        // ; y == fa
        private static readonly Regex _registerExpectation = new Regex(@"^\s*;\s*(?<register>[pP]|[aA]|[xX]|[yY])\s*==\s*(?<value>[0-9a-zA-Z]{1,2})\s*(?:;.*)?$", RegexOptions.Compiled);

        public static IEnumerable<Checkpoint> ParseHeaders(string assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            using var reader = new StringReader(assembly);

            var isInCheckpoint = false;

            var checkpointIdentifier = (byte)0;
            var expectations = new List<IExpectation>();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Match match;

                if ((match = _init.Match(line)).Success)
                {
                    // Return a checkpoint if one was created previously.
                    if (isInCheckpoint)
                    {
                        yield return new Checkpoint(checkpointIdentifier, expectations);
                    }

                    checkpointIdentifier = 0;
                    expectations.Clear();

                    isInCheckpoint = true;
                }
                else if ((match = _checkpoint.Match(line)).Success)
                {
                    // Return a checkpoint if one was created previously.
                    if (isInCheckpoint)
                    {
                        yield return new Checkpoint(checkpointIdentifier, expectations);
                    }

                    checkpointIdentifier = byte.Parse(match.Groups[1].Value); // Will always be a valid integer if the regex matches.
                    expectations.Clear();

                    isInCheckpoint = true;
                }
                else if ((match = _singleMemoryExpectation.Match(line)).Success)
                {
                    var location = Convert.ToInt32(match.Groups["location"].Value, 16);
                    var value = Convert.ToByte(match.Groups["value"].Value, 16);

                    expectations.Add(new SingleMemoryExpectation(location, value));
                }
                else if ((match = _scalarRangeMemoryExpectation.Match(line)).Success)
                {
                    var locationStart = Convert.ToInt32(match.Groups["locationStart"].Value, 16);
                    var locationEnd = Convert.ToInt32(match.Groups["locationEnd"].Value, 16);

                    var memoryRange = new Range(locationStart, locationEnd);

                    var value = Convert.ToByte(match.Groups["value"].Value, 16);

                    expectations.Add(new ScalarMemoryRangeExpectation(memoryRange, value));
                }
                else if ((match = _linearRangeMemoryExpectation.Match(line)).Success)
                {
                    var locationStart = Convert.ToInt32(match.Groups["locationStart"].Value, 16);
                    var locationEnd = Convert.ToInt32(match.Groups["locationEnd"].Value, 16);

                    var memoryRange = new Range(locationStart, locationEnd);

                    var valueStart = Convert.ToByte(match.Groups["valueStart"].Value, 16);
                    var valueEnd = Convert.ToByte(match.Groups["valueEnd"].Value, 16);

                    expectations.Add(new LinearMemoryRangeExpectation(memoryRange, valueStart, valueEnd));
                }
                else if ((match = _collectionRangeMemoryExpectation.Match(line)).Success)
                {
                    var locationStart = Convert.ToInt32(match.Groups["locationStart"].Value, 16);
                    var locationEnd = Convert.ToInt32(match.Groups["locationEnd"].Value, 16);

                    var memoryRange = new Range(locationStart, locationEnd);

                    var values = new List<byte>
                    {
                        Convert.ToByte(match.Groups["firstValue"].Value, 16),
                    };

                    foreach (Capture? otherValueCapture in match.Groups["otherValues"].Captures)
                    {
                        values.Add(Convert.ToByte(otherValueCapture!.Value, 16));
                    }

                    expectations.Add(new CollectionMemoryRangeExpectation(memoryRange, values));
                }
                else if ((match = _registerExpectation.Match(line)).Success)
                {
                    var register = match.Groups["register"].Value.ToLowerInvariant();
                    var value = Convert.ToByte(match.Groups["value"].Value, 16);

                    expectations.Add(register switch
                    {
                        "p" => new RegisterPExpectation(value),
                        "a" => new RegisterAExpectation(value),
                        "x" => new RegisterXExpectation(value),
                        "y" => new RegisterYExpectation(value),
                        _ => throw new InvalidOperationException($"An unexpected register type of {register} was found."),
                    });
                }
                else if (_blankComment.IsMatch(line))
                {
                    // Don't do anything. Continue a checkpoint block if we are already in one.
                }
                else // We are breaking out of a comment block if we were in one previously.
                {
                    // Return a checkpoint if one was created previously.
                    if (isInCheckpoint)
                    {
                        yield return new Checkpoint(checkpointIdentifier, expectations);
                    }

                    isInCheckpoint = false;
                }
            }
        }
    }
}