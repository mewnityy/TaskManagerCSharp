using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace TaskManagerCSharp
{
    internal class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public int Difficulty { get; set; }
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    internal class SaveData
    {
        public List<TaskItem> Tasks { get; set; }
        public int NextId { get; set; }
    }

    internal class TaskManager
    {
        private List<TaskItem> _tasks = new List<TaskItem>();
        private List<string> _history = new List<string>();
        private int _nextId = 1;
        private string _saveFile = "tasks.json";

        public TaskManager()
        {
            LoadFromFile();
        }

        public void AddTask(string titleFromCommand, string priorityFromCommand)
        {
            Console.WriteLine();
            Console.WriteLine("--- New Task ---");

            // title
            string title = titleFromCommand;
            if (title == "")
            {
                Console.Write("Title: ");
                title = Console.ReadLine();
                if (title == "")
                {
                    Console.WriteLine("Title cannot be empty. Task cancelled.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Title: " + title);
            }

            Console.Write("Description (press Enter to skip): ");
            string description = Console.ReadLine();

            string priority = priorityFromCommand.ToLower();
            if (priority != "low" && priority != "normal" && priority != "high")
            {
                Console.Write("Priority (low / normal / high): ");
                priority = Console.ReadLine().ToLower();

                if (priority != "low" && priority != "normal" && priority != "high")
                {
                    Console.WriteLine("Wrong priority. Using normal.");
                    priority = "normal";
                }
            }
            else
            {
                Console.WriteLine("Priority: " + priority);
            }

            Console.Write("Difficulty (1-5): ");
            string diffInput = Console.ReadLine();
            int difficulty = 0;
            bool diffParsed = int.TryParse(diffInput, out difficulty);

            if (!diffParsed || difficulty < 1 || difficulty > 5)
            {
                Console.WriteLine("Wrong difficulty. Using 1.");
                difficulty = 1;
            }

            Console.WriteLine("----------------");

            TaskItem task = new TaskItem();
            task.Id = _nextId;
            task.Title = title;
            task.Description = description;
            task.Priority = priority;
            task.Difficulty = difficulty;
            task.IsDone = false;
            task.CreatedAt = DateTime.Now;

            _tasks.Add(task);
            _nextId++;

            Console.WriteLine("Task added --> [" + task.Id + "] " + task.Title);
            SaveToFile();
        }

        public void ListTasks(string filter)
        {
            filter = filter.ToLower();

            List<TaskItem> result = new List<TaskItem>();

            if (filter == "all")
            {
                result = _tasks;
            }
            else if (filter == "done")
            {
                foreach (TaskItem t in _tasks)
                {
                    if (t.IsDone)
                        result.Add(t);
                }
            }
            else if (filter == "todo")
            {
                foreach (TaskItem t in _tasks)
                {
                    if (!t.IsDone)
                        result.Add(t);
                }
            }
            else if (filter == "low" || filter == "normal" || filter == "high")
            {
                foreach (TaskItem t in _tasks)
                {
                    if (t.Priority == filter)
                        result.Add(t);
                }
            }
            else
            {
                Console.WriteLine("Unknown filter. Use: all, done, todo, low, normal, high");
                return;
            }

            if (result.Count == 0)
            {
                Console.WriteLine("No tasks found.");
                return;
            }

            Console.WriteLine();

            foreach (TaskItem t in result)
            {
                string status = "TODO";
                if (t.IsDone)
                    status = "DONE";

                string stars = "";
                if (t.Difficulty == 0)
                    stars = "-----";
                else
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        if (i <= t.Difficulty)
                            stars += "*";
                        else
                            stars += "-";
                    }
                }

                string prio = t.Priority.ToUpper().PadRight(6);
                string id = ("#" + t.Id).PadRight(4);

                if (t.IsDone)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (t.Priority == "high")
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (t.Priority == "low")
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(id + " [" + status + "]  " + prio + "  [" + stars + "]  " + t.Title);

                if (t.Description != null && t.Description != "")
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("          >> " + t.Description);
                }

                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine("Total: " + result.Count + " task(s)");
            Console.WriteLine();
        }

        public void MarkDone(string idStr)
        {
            TaskItem task = FindTask(idStr);

            if (task == null)
                return;

            if (task.IsDone)
            {
                Console.WriteLine("Already done --> [" + task.Id + "] " + task.Title);
                return;
            }

            task.IsDone = true;
            Console.WriteLine("Marked as done --> [" + task.Id + "] " + task.Title);
            SaveToFile();
        }

        public void MarkUndone(string idStr)
        {
            TaskItem task = FindTask(idStr);

            if (task == null)
                return;

            if (!task.IsDone)
            {
                Console.WriteLine("Already not done --> [" + task.Id + "] " + task.Title);
                return;
            }

            task.IsDone = false;
            Console.WriteLine("Marked as not done --> [" + task.Id + "] " + task.Title);
            SaveToFile();
        }

        public void DeleteTask(string idStr)
        {
            TaskItem task = FindTask(idStr);

            if (task == null)
                return;

            _tasks.Remove(task);
            RecalculateNextId();
            Console.WriteLine("Task deleted --> [" + task.Id + "] " + task.Title);
            SaveToFile();
        }

        public void EditTask(string idStr, string newTitle)
        {
            TaskItem task = FindTask(idStr);

            if (task == null)
                return;

            string oldTitle = task.Title;
            task.Title = newTitle;

            Console.WriteLine("Task updated --> " + oldTitle + " --> " + newTitle);
            SaveToFile();
        }

        public void ChangePriority(string idStr, string priority)
        {
            TaskItem task = FindTask(idStr);

            if (task == null)
                return;

            priority = priority.ToLower();

            if (priority != "low" && priority != "normal" && priority != "high")
            {
                Console.WriteLine("Priority must be: low, normal or high");
                return;
            }

            task.Priority = priority;
            Console.WriteLine("Priority changed --> [" + task.Id + "] " + task.Title + " is now " + priority);
            SaveToFile();
        }

        public void Search(string text)
        {
            Console.WriteLine();
            Console.WriteLine("Searching --> " + text);
            Console.WriteLine();

            int found = 0;

            foreach (TaskItem t in _tasks)
            {
                if (t.Title.ToLower().Contains(text.ToLower()))
                {
                    string status = "TODO";
                    if (t.IsDone)
                        status = "DONE";

                    string stars = "";
                    if (t.Difficulty == 0)
                        stars = "-----";
                    else
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (i <= t.Difficulty)
                                stars += "*";
                            else
                                stars += "-";
                        }
                    }

                    string id = ("#" + t.Id).PadRight(4);

                    if (t.IsDone)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    else if (t.Priority == "high")
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (t.Priority == "low")
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    else
                        Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(id + " [" + status + "]  " + t.Priority.ToUpper().PadRight(6) + "  [" + stars + "]  " + t.Title);

                    if (t.Description != null && t.Description != "")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("          >> " + t.Description);
                    }

                    Console.ResetColor();
                    Console.WriteLine();
                    found++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Found " + found + " result(s)");
            Console.WriteLine();
        }

        public void ClearDone()
        {
            int count = 0;

            for (int i = _tasks.Count - 1; i >= 0; i--)
            {
                if (_tasks[i].IsDone)
                {
                    _tasks.RemoveAt(i);
                    count++;
                }
            }

            if (count == 0)
                Console.WriteLine("No completed tasks to remove.");
            else
            {
                RecalculateNextId();
                Console.WriteLine("Removed " + count + " completed task(s).");
                SaveToFile();
            }
        }

        private void RecalculateNextId()
        {
            if (_tasks.Count == 0)
            {
                _nextId = 1;
                return;
            }

            int maxId = 0;
            foreach (TaskItem t in _tasks)
            {
                if (t.Id > maxId)
                    maxId = t.Id;
            }
            _nextId = maxId + 1;
        }

        private TaskItem FindTask(string idStr)
        {
            int id = 0;
            bool parsed = int.TryParse(idStr, out id);

            if (!parsed)
            {
                Console.WriteLine("Invalid ID --> " + idStr);
                return null;
            }

            foreach (TaskItem t in _tasks)
            {
                if (t.Id == id)
                    return t;
            }

            Console.WriteLine("Task not found --> #" + id);
            return null;
        }

        private void SaveToFile()
        {
            SaveData data = new SaveData();
            data.Tasks = _tasks;
            data.NextId = _nextId;

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_saveFile, json);
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_saveFile))
                return;

            string json = File.ReadAllText(_saveFile);

            SaveData data = JsonSerializer.Deserialize<SaveData>(json);

            if (data == null)
                return;

            _tasks = data.Tasks;
            RecalculateNextId();

            Console.WriteLine("Loaded " + _tasks.Count + " task(s) from " + _saveFile);
        }

        public void AddHistory(string command)
        {
            _history.Add(command);
        }

        public void ShowHistory()
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("History is empty.");
                return;
            }

            Console.WriteLine();
            for (int i = 0; i < _history.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + _history[i]);
            }
            Console.WriteLine();
        }

        public void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("-------------------");
            Console.WriteLine("add      - add new task");
            Console.WriteLine("list     - show tasks");
            Console.WriteLine("done     - mark task as done");
            Console.WriteLine("undone   - mark task as not done");
            Console.WriteLine("del      - delete task");
            Console.WriteLine("edit     - edit task title");
            Console.WriteLine("priority - change task priority");
            Console.WriteLine("search   - search tasks by text");
            Console.WriteLine("clear    - remove all completed tasks");
            Console.WriteLine("history  - show command history");
            Console.WriteLine("cls      - clear screen");
            Console.WriteLine("help     - show this help");
            Console.WriteLine("exit     - exit program");
            Console.WriteLine();
        }

        public void ShowHelpDetail(string command)
        {
            Console.WriteLine();
            switch (command.ToLower())
            {
                case "add":
                    Console.WriteLine("add - adds a new task");
                    Console.WriteLine("usage: add <title> [low/normal/high]");
                    Console.WriteLine("example: add \"Buy groceries\" high");
                    break;
                case "list":
                    Console.WriteLine("list - shows tasks");
                    Console.WriteLine("usage: list [all/done/todo/low/normal/high]");
                    Console.WriteLine("example: list high");
                    break;
                case "done":
                    Console.WriteLine("done - marks task as completed");
                    Console.WriteLine("usage: done <id>");
                    break;
                case "undone":
                    Console.WriteLine("undone - marks task as not completed");
                    Console.WriteLine("usage: undone <id>");
                    break;
                case "del":
                    Console.WriteLine("del - deletes task by id");
                    Console.WriteLine("usage: del <id>");
                    break;
                case "edit":
                    Console.WriteLine("edit - changes task title");
                    Console.WriteLine("usage: edit <id> <new title>");
                    break;
                case "priority":
                    Console.WriteLine("priority - changes task priority");
                    Console.WriteLine("usage: priority <id> <low/normal/high>");
                    break;
                case "search":
                    Console.WriteLine("search - finds tasks that contain the text");
                    Console.WriteLine("usage: search <text>");
                    break;
                case "clear":
                    Console.WriteLine("clear - removes all tasks marked as done");
                    break;
                case "history":
                    Console.WriteLine("history - shows all commands typed this session");
                    break;
                case "cls":
                    Console.WriteLine("cls - clears the screen");
                    break;
                default:
                    Console.WriteLine("No help for --> " + command);
                    break;
            }
            Console.WriteLine();
        }
    }
}