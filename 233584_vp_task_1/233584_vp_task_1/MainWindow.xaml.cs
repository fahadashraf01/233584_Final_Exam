using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace _233584_vp_task_1
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=DESKTOP-9BMMS5L\\SQLEXPRESS;Initial Catalog=lab_quiz;Integrated Security=True;Trust Server Certificate=True";
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
        private bool isInitialized = false; 

        public MainWindow()
        {
            InitializeComponent();
            QuestionsDataGrid.ItemsSource = Questions;
            LoadQuestions();
            isInitialized = true; 
        }

        private void LoadQuestions()
        {
            try
            {
                Questions.Clear();

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT * FROM Questions", connection);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Questions.Add(new Question
                        {
                            Id = (int)reader["Id"],
                            QuestionText = reader["QuestionText"].ToString(),
                            Options = reader["Options"].ToString(),
                            CorrectAnswer = reader["CorrectAnswer"].ToString(),
                            AssignedMarks = (int)reader["AssignedMarks"],
                            TimeLimit = (int)reader["TimeLimit"],
                            Topic = reader["Topic"]?.ToString(),
                            DifficultyLevel = reader["DifficultyLevel"]?.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading questions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newQuestion = new Question
                {
                    QuestionText = "New Question",
                    Options = "A|Option1|B|Option2|C|Option3|D|Option4",
                    CorrectAnswer = "A",
                    AssignedMarks = 5,
                    TimeLimit = 30,
                    Topic = "General",
                    DifficultyLevel = "Easy"
                };

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO Questions (QuestionText, Options, CorrectAnswer, AssignedMarks, TimeLimit, Topic, DifficultyLevel) OUTPUT INSERTED.Id VALUES (@QuestionText, @Options, @CorrectAnswer, @AssignedMarks, @TimeLimit, @Topic, @DifficultyLevel)",
                        connection);

                    command.Parameters.AddWithValue("@QuestionText", newQuestion.QuestionText);
                    command.Parameters.AddWithValue("@Options", newQuestion.Options);
                    command.Parameters.AddWithValue("@CorrectAnswer", newQuestion.CorrectAnswer);
                    command.Parameters.AddWithValue("@AssignedMarks", newQuestion.AssignedMarks);
                    command.Parameters.AddWithValue("@TimeLimit", newQuestion.TimeLimit);
                    command.Parameters.AddWithValue("@Topic", newQuestion.Topic);
                    command.Parameters.AddWithValue("@DifficultyLevel", newQuestion.DifficultyLevel);

                    newQuestion.Id = (int)command.ExecuteScalar();
                    Questions.Add(newQuestion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsDataGrid.SelectedItem is Question selectedQuestion)
            {
                try
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        var command = new SqlCommand(
                            "UPDATE Questions SET QuestionText = @QuestionText, Options = @Options, CorrectAnswer = @CorrectAnswer, AssignedMarks = @AssignedMarks, TimeLimit = @TimeLimit, Topic = @Topic, DifficultyLevel = @DifficultyLevel WHERE Id = @Id",
                            connection);

                        command.Parameters.AddWithValue("@QuestionText", selectedQuestion.QuestionText);
                        command.Parameters.AddWithValue("@Options", selectedQuestion.Options);
                        command.Parameters.AddWithValue("@CorrectAnswer", selectedQuestion.CorrectAnswer);
                        command.Parameters.AddWithValue("@AssignedMarks", selectedQuestion.AssignedMarks);
                        command.Parameters.AddWithValue("@TimeLimit", selectedQuestion.TimeLimit);
                        command.Parameters.AddWithValue("@Topic", selectedQuestion.Topic);
                        command.Parameters.AddWithValue("@DifficultyLevel", selectedQuestion.DifficultyLevel);
                        command.Parameters.AddWithValue("@Id", selectedQuestion.Id);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error editing question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a question to edit.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsDataGrid.SelectedItem is Question selectedQuestion)
            {
                try
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM Questions WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Id", selectedQuestion.Id);
                        command.ExecuteNonQuery();
                    }

                    Questions.Remove(selectedQuestion);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a question to delete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (!isInitialized) return;

            var selectedFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedFilter == "All")
            {
                QuestionsDataGrid.ItemsSource = Questions;
            }
            else if (selectedFilter == "Topic")
            {
                QuestionsDataGrid.ItemsSource = new ObservableCollection<Question>(
                    Questions); 
            }
            else if (selectedFilter == "Difficulty")
            {
                QuestionsDataGrid.ItemsSource = new ObservableCollection<Question>(
                    Questions); 
            }
        }
    }

    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string Options { get; set; }
        public string CorrectAnswer { get; set; }
        public int AssignedMarks { get; set; }
        public int TimeLimit { get; set; }
        public string Topic { get; set; }
        public string DifficultyLevel { get; set; }
    }
}
