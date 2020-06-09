using System;
using System.Collections.Generic;
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

            while (true)
            {
                var computerMove = GetComputerMove(args);
                var key = SecureRandom();
                var hmac = GetHMAC(args[computerMove], key);

                Console.WriteLine("Available moves:");
                for (var i = 1; i <= args.Length; i++)
                {
                    var arg = args[i - 1];
                    Console.WriteLine($"{i} - {arg}");
                }
                Console.WriteLine("0 - exit");
                Console.Write("Your move: ");
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

                Console.WriteLine($"HMAC: {ToHexFromUTF8(hmac)}");
                Console.WriteLine($"Your move: {args[intUserInput - 1]}");
                Console.WriteLine($"Computer move: {args[computerMove]}");
                Console.WriteLine($"HMAC key: {key}");
                var winMessage = GetWinMessage(intUserInput - 1, computerMove, args.Length);
                Console.WriteLine(winMessage);
            }

            Console.ReadKey();
        }


        private static string ToHexFromUTF8(string input)
        {
            var bytes = Encoding.Default.GetBytes(input);
            var hexString = BitConverter.ToString(bytes).Replace("-", "");

            return hexString;
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
            var hexString = BitConverter.ToString(randomNumbers).Replace("-", "");

            return hexString;
        }

        private static string GetHMAC(string input, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
            {
                var computedHash = hmac.ComputeHash(Encoding.ASCII.GetBytes(input));

                return Encoding.ASCII.GetString(computedHash);
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