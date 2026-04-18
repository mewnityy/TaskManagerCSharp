namespace TaskManagerCSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            TaskManager manager = new TaskManager();

            Console.WriteLine("================================");
            Console.WriteLine("   Task Manager by C#  v1.0");
            Console.WriteLine("   Copyright (C) 2026");
            Console.WriteLine("================================");
            Console.WriteLine("   Type 'help' to get started");
            Console.WriteLine("================================");
            Console.WriteLine();

            while (true)
            {
                Console.Write("TM> ");
                string input = Console.ReadLine();

                if (input == "")
                    continue;

                manager.AddHistory(input);

                string[] parts = CommandParser.Parse(input);
                string command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "add":
                            string title = parts.Length >= 2 ? parts[1] : "";
                            string priority = parts.Length >= 3 ? parts[2] : "";
                            manager.AddTask(title, priority);
                            break;

                        case "list":
                            string filter = parts.Length >= 2 ? parts[1] : "all";
                            manager.ListTasks(filter);
                            break;

                        case "done":
                            if (parts.Length < 2) { Console.WriteLine("Correct usage: done <id>"); break; }
                            manager.MarkDone(parts[1]);
                            break;

                        case "undone":
                            if (parts.Length < 2) { Console.WriteLine("Correct usage: undone <id>"); break; }
                            manager.MarkUndone(parts[1]);
                            break;

                        case "del":
                            if (parts.Length < 2) { Console.WriteLine("Correct usage: del <id>"); break; }
                            manager.DeleteTask(parts[1]);
                            break;

                        case "edit":
                            if (parts.Length < 3) { Console.WriteLine("Correct usage: edit <id> <new title>"); break; }
                            manager.EditTask(parts[1], parts[2]);
                            break;

                        case "priority":
                            if (parts.Length < 3) { Console.WriteLine("Correct usage: priority <id> <low/normal/high>"); break; }
                            manager.ChangePriority(parts[1], parts[2]);
                            break;

                        case "search":
                            if (parts.Length < 2) { Console.WriteLine("Correct usage: search <text>"); break; }
                            manager.Search(parts[1]);
                            break;

                        case "clear":
                            manager.ClearDone();
                            break;

                        case "history":
                            manager.ShowHistory();
                            break;

                        case "cls":
                            Console.Clear();
                            break;

                        case "help":
                            if (parts.Length > 1)
                                manager.ShowHelpDetail(parts[1]);
                            else
                                manager.ShowHelp();
                            break;

                        case "exit":
                            return;

                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("'" + command + "' is not recognized. Type 'help' to see the list of them.");
                            Console.ResetColor();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error --> " + ex.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}