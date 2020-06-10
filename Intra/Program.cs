using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TwilightSparkle.TestDoDiez
{
    class Program
    {
        static void Main(string[] args)
        {
            var isArgsCorrect = CheckIfArgsCorrect(args);
            if (!isArgsCorrect)
            {
                Console.WriteLine("Incorrect arguments");
                Console.ReadKey();

                return;
            }
            Console.WriteLine("Game started");

            while (true)
            {
                Console.WriteLine("Round started");
                var computerMove = GetComputerMove(args);
                var key = SecureRandom();
                var hmac = GetHMAC(args[computerMove], key);
                var hmacBefore = ToHexFromUTF8(hmac);
                Console.WriteLine($"HMAC of computer move before user move: {hmacBefore}");
                Console.WriteLine("It can be used to ensure that computer played fair");

                Console.WriteLine("Available moves:");
                for (var i = 1; i <= args.Length; i++)
                {
                    var arg = args[i - 1];
                    Console.WriteLine($"{i} - {arg}");
                }
                Console.WriteLine("0 - exit");
                Console.WriteLine("Please enter your move:");
                var userInput = Console.ReadLine();
                var parseResult = int.TryParse(userInput, out var intUserInput);
                if (!parseResult)
                {
                    continue;
                }

                if (intUserInput < 0 || intUserInput > args.Length)
                {
                    continue;
                }

                if (intUserInput == 0)
                {
                    break;
                }

                Console.WriteLine($"Your move: {args[intUserInput - 1]}");
                Console.WriteLine($"Computer move: {args[computerMove]}");
                var winMessage = GetWinMessage(intUserInput - 1, computerMove, args.Length);
                Console.WriteLine(winMessage);
                Console.WriteLine($"HMAC key: {key}");

                Console.WriteLine();
                Console.WriteLine("Use https://www.liavaag.org/English/SHA-Generator/HMAC/ to verify fair game by yourself.");
                Console.WriteLine("Use computer move as input with type text, hmac key as key with type HEX and algorithm SHA-256 with output type HEX");
                Console.WriteLine();
            }

            Console.ReadKey();
        }


        private static string ToHexFromUTF8(byte[] bytes)
        {
            var hexString = BitConverter.ToString(bytes).Replace("-", "").ToLower();

            return hexString;
        }

        private static byte[] HexDecode(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            return bytes;
        }

        private static bool CheckIfArgsCorrect(IReadOnlyCollection<string> args)
        {
            if (args == null)
            {
                return false;
            }

            if (args.Count < 3 || args.Count % 2 == 0)
            {
                return false;
            }

            var sortedArgs = args.OrderBy(a => a);

            string previousString = null;
            foreach (var arg in sortedArgs)
            {
                if (string.IsNullOrWhiteSpace(arg) || previousString != null && String.Equals(arg, previousString, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                previousString = arg;
            }

            return true;
        }

        private static int GetComputerMove(IReadOnlyCollection<string> availableMoves)
        {
            var random = new Random();
            var move = random.Next(0, availableMoves.Count);

            return move;
        }

        private static string SecureRandom()
        {
            using var provider = new RNGCryptoServiceProvider();
            var randomNumbers = new byte[32];
            provider.GetBytes(randomNumbers);
            var hexString = ToHexFromUTF8(randomNumbers);

            return hexString;
        }

        private static byte[] GetHMAC(string input, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(HexDecode(key)))
            {
                var computedHash = hmac.ComputeHash(Encoding.ASCII.GetBytes(input));

                return computedHash;
            }
        }

        private static string GetWinMessage(int userMove, int computerMove, int amountOfMoves)
        {
            if (userMove == computerMove)
            {
                return "Draw"; //ничья
            }
            else
            {
                if (computerMove < userMove)
                {
                    computerMove += userMove;
                }

                if (computerMove <= userMove + amountOfMoves / 2)
                {
                    return "Computer win";
                }

                return "You win";
            }
        }
    }
}