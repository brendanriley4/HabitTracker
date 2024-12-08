using System.Data.SQLite;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source = habits.db;Version=3;";
        InitializeDatabase(connectionString);

        Console.WriteLine("Welcome to Brendan's SQLite HabitTracker!");
        Console.WriteLine("");

        while (true)
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. View Habits");
            Console.WriteLine("2. Add Habit");
            Console.WriteLine("3. Mark Habit Completed");
            Console.WriteLine("4. Remove Habit");
            Console.WriteLine("5. Exit");
            Console.WriteLine("");
            Console.Write("Input Choice: ");
            string choice = Console.ReadLine();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    ViewHabits(connectionString);
                    break;
                case "2":
                    AddHabit(connectionString);
                    break;
                case "3":
                    MarkHabit(connectionString);
                    break;
                case "4":
                    RemoveHabit(connectionString);
                    break;
                case "5":
                    Console.WriteLine("GoodBye!");
                    Console.WriteLine("");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.WriteLine("");
                    break;
            }

        }   
    }

    static Dictionary<int, int> pseudoIdMap = new Dictionary<int, int>();

    static void InitializeDatabase(string connectionString)
    {
        using (var db = new SQLiteConnection(connectionString))
        {
            db.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    DateAdded TEXT NOT NULL,
                    DateCompleted TEXT
                )";

            string createCompletionsTableQuery = @"
                CREATE TABLE IF NOT EXISTS Completions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitId INTEGER NOT NULL,
                    DateCompleted TEXT NOT NULL,
                    FOREIGN KEY(HabitId) REFERENCES Habits(Id)
                )";

            using (var cmd = new SQLiteCommand(createTableQuery, db))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(createCompletionsTableQuery, db))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Database Initialized Successfully!");
            Console.WriteLine("");
        }
    }

    static void ViewHabits(string cs)
    {
        pseudoIdMap.Clear();

        using (var connection = new SQLiteConnection(cs))
        {
            connection.Open();

            string query = "SELECT Id, Name, DateAdded, DateCompleted FROM Habits";
            using (var command = new SQLiteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Habits:");
                int pseudoId = 1;
                
                while (reader.Read())
                {
                    string name = reader["Name"].ToString();
                    string dateAdded = reader["DateAdded"].ToString();
                    int id = int.Parse(reader["ID"].ToString());
                    pseudoIdMap[pseudoId] = id;
                    Console.WriteLine($"{pseudoId}: {name} (Added: {dateAdded})");
                    pseudoId++;
                    ShowCompletionDates(cs, id);
                }
                Console.WriteLine("");
            }
        }
    }

    static void ShowCompletionDates(string connectionString, int habitId)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT DateCompleted FROM Completions WHERE HabitId = @HabitId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@HabitId", habitId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string dateCompleted = reader["DateCompleted"].ToString();
                        Console.WriteLine($"   - Completed on: {dateCompleted}");
                    }
                }
            }
        }
    }

    static void AddHabit(string connectionString)
    {
        Console.Write("Enter the name of the new habit: ");
        string habitName = Console.ReadLine();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string insertQuery = "INSERT INTO Habits (Name, DateAdded) VALUES (@Name, @DateAdded)";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", habitName);
                command.Parameters.AddWithValue("@DateAdded", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }

            Console.WriteLine("Habit added successfully.");
            Console.WriteLine("");
        }
    }

    static void MarkHabit(string connectionString)
    {
        ViewHabits(connectionString);

        Console.Write("Enter the ID of the habit to mark as completed: ");
        if (int.TryParse(Console.ReadLine(), out int pseudoId) && pseudoIdMap.TryGetValue(pseudoId, out int habitId))
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Insert into the Completions table
                string insertCompletionQuery = "INSERT INTO Completions (HabitId, DateCompleted) VALUES (@HabitId, @DateCompleted)";
                using (var command = new SQLiteCommand(insertCompletionQuery, connection))
                {
                    command.Parameters.AddWithValue("@HabitId", habitId);
                    command.Parameters.AddWithValue("@DateCompleted", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Habit marked as completed.");
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("Habit not found.");
                        Console.WriteLine("");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Invalid ID.");
            Console.WriteLine("");
        }
    }

    static void RemoveHabit(string connectionString)
    {
        ViewHabits(connectionString);

        Console.Write("Enter the ID of the habit to remove: ");
        if (int.TryParse(Console.ReadLine(), out int pseudoId) && pseudoIdMap.TryGetValue(pseudoId, out int habitId))
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string deleteCompletionsQuery = "DELETE FROM Completions WHERE HabitId = @HabitId";
                using (var command = new SQLiteCommand(deleteCompletionsQuery, connection))
                {
                    command.Parameters.AddWithValue("@HabitId", habitId);
                    command.ExecuteNonQuery();
                }

                string deleteHabitQuery = "DELETE FROM Habits WHERE Id = @Id";
                using (var command = new SQLiteCommand(deleteHabitQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", habitId);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Habit removed successfully.");
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("Habit not found.");
                        Console.WriteLine("");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Invalid ID.");
            Console.WriteLine("");
        }
    }
}