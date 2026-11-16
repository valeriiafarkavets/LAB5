using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using LAB5.Commands;
using LAB5.Models;
using LAB5.Repositories;
using LAB5.Services;
namespace LAB5.ViewModels{
    public class MainViewModel : INotifyPropertyChanged{
        private readonly Repository<Student> _repository = new();
        private readonly JsonService _jsonService = new();
        private readonly CsvService _csvService = new();
        private Student? _selectedStudent;
        private string _studentName = "";
        private string _studentAge = "18";
        private string _studentMass = "60";
        private string _marksInput = "";
        private string _searchText = "";
        private string _minimumAverage = "";
        private string _selectedSort = "За ім'ям";
        private string _statusMessage = "Готово до роботи";
        public ObservableCollection<Student> Students { get; } = new();
        public ICollectionView StudentsView { get; }
        public List<string> SortOptions { get; } = new() {
            "За ім'ям",
            "За віком",
            "За масою",
            "За середнім балом",
            "Складене сортування"
        };
        public Student? SelectedStudent{
            get => _selectedStudent;
            set{
                if (SetProperty(ref _selectedStudent, value) && value != null){
                    StudentName = value.Name;
                    StudentAge = value.Age.ToString();
                    StudentMass = value.Mass.ToString(CultureInfo.CurrentCulture);
                    MarksInput = string.Join(", ", value.Marks);
                }
            }
        }
        public string StudentName{
            get => _studentName;
            set => SetProperty(ref _studentName, value);
        }
        public string StudentAge{
            get => _studentAge;
            set => SetProperty(ref _studentAge, value);
        }
        public string StudentMass{
            get => _studentMass;
            set => SetProperty(ref _studentMass, value);
        }
        public string MarksInput {
            get => _marksInput;
            set => SetProperty(ref _marksInput, value);
        }
        public string SearchText{
            get => _searchText;
            set{
                if (SetProperty(ref _searchText, value))
                    RefreshView();
            }
        }
        public string MinimumAverage{
            get => _minimumAverage;
            set {
                if (SetProperty(ref _minimumAverage, value))
                    RefreshView();
            }
        }
        public string SelectedSort{
            get => _selectedSort;
            set{
                if (SetProperty(ref _selectedSort, value))
                    RefreshView();
            }
        }
        public string StatusMessage{
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public ICommand AddStudentCommand { get; }
        public ICommand UpdateStudentCommand { get; }
        public ICommand RemoveStudentCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand SaveJsonCommand { get; }
        public ICommand LoadJsonCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public MainViewModel(){
            StudentsView = CollectionViewSource.GetDefaultView(Students);
            StudentsView.Filter = FilterStudent;
            AddStudentCommand = new RelayCommand(_ => AddStudent());
            UpdateStudentCommand = new RelayCommand(_ => UpdateStudent());
            RemoveStudentCommand = new RelayCommand(_ => RemoveStudent());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
            SaveJsonCommand = new RelayCommand(async _ => await SaveJsonAsync());
            LoadJsonCommand = new RelayCommand(async _ => await LoadJsonAsync());
            ExportCsvCommand = new RelayCommand(async _ => await ExportCsvAsync());
            RefreshView();
        }
        private bool FilterStudent(object item) {
            if (item is not Student student)
                return false;
            bool matchesName = string.IsNullOrWhiteSpace(SearchText) ||
                student.Name.Contains(
                 SearchText.Trim(),
                 StringComparison.CurrentCultureIgnoreCase);
            bool matchesAverage = true;
            if (!string.IsNullOrWhiteSpace(MinimumAverage)){
                if (double.TryParse( MinimumAverage,NumberStyles.Float,CultureInfo.CurrentCulture, out double minimum)){
                    matchesAverage = student.AverageMark >= minimum;
                }
                else{
                    matchesAverage = false;
                }
            }
            return matchesName && matchesAverage;
        }
        private void RefreshView(){
            if (StudentsView is ListCollectionView listView){
                Comparison<Student> comparison = SelectedSort switch {
                    "За віком" => (a, b) => a.Age.CompareTo(b.Age),
                    "За масою" => (a, b) => a.Mass.CompareTo(b.Mass),
                    "За середнім балом" => (a, b) => b.AverageMark.CompareTo(a.AverageMark),
                    "Складене сортування" => (a, b) => new StudentComparer().Compare(a, b), _ => (a, b) => string.Compare( a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase)
                };
                listView.CustomSort =
                    Comparer<Student>.Create(comparison);
            }
            StudentsView.Refresh();
        }
        private void AddStudent() {
            if (!TryReadForm(out string name, out int age,
                out double mass, out List<int> marks))
                return;
            var student = new Student{
                Name = name,
                Age = age,
                Mass = mass,
                Marks = marks
            };
            _repository.Add(student);
            Students.Add(student);
            StatusMessage = $"Додано студента: {student.Name}";
            ClearForm();
            RefreshView();
        }
        private void UpdateStudent(){
            if (SelectedStudent == null){
                StatusMessage = "Спочатку виберіть студента";
                return;
            }
            if (!TryReadForm(out string name, out int age,
                out double mass, out List<int> marks))
                return;
            SelectedStudent.Name = name;
            SelectedStudent.Age = age;
            SelectedStudent.Mass = mass;
            SelectedStudent.Marks = marks;
            StatusMessage = $"Оновлено студента: {name}";
            RefreshView();
        }
        private void RemoveStudent(){
            if (SelectedStudent == null){
                StatusMessage = "Виберіть студента для видалення";
                return;
            }
            Student student = SelectedStudent;
            _repository.Remove(student);
            Students.Remove(student);
            StatusMessage = $"Видалено студента: {student.Name}";
            ClearForm();
        }
        private bool TryReadForm(out string name,out int age,out double mass,out List<int> marks){
            name = StudentName.Trim();
            age = 0;
            mass = 0;
            marks = new List<int>();
            if (string.IsNullOrWhiteSpace(name)){
                StatusMessage = "Введіть ім'я студента";
                return false;
            }
            if (!int.TryParse(StudentAge, out age) || age < 1){
                StatusMessage = "Введіть правильний вік";
                return false;
            }
            if (!double.TryParse( StudentMass,NumberStyles.Float,CultureInfo.CurrentCulture,out mass) || mass <= 0) {
                StatusMessage = "Введіть правильну масу";
                return false;
            }
            string[] parts = MarksInput.Split( new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts) {
                if (!int.TryParse(part, out int mark) ||
                    mark < 1 || mark > 5) {
                    StatusMessage = "Оцінки мають бути від 1 до 5";
                    return false;
                }
                marks.Add(mark);
            }
            return true;
        }
        private async Task SaveJsonAsync(){
            var dialog = new SaveFileDialog{
                Filter = "JSON files (*.json)|*.json",
                FileName = "students.json"
            };
            if (dialog.ShowDialog() != true)
                return;
            try{
                await _jsonService.SaveAsync(
                    dialog.FileName, _repository.GetAll());

                StatusMessage = "Дані успішно збережено в JSON";
            }
            catch (Exception ex){
                StatusMessage = $"Помилка збереження: {ex.Message}";
            }
        }
        private async Task LoadJsonAsync() {
            var dialog = new OpenFileDialog{
                Filter = "JSON files (*.json)|*.json",
                FileName = "students.json",
                CheckFileExists = false
            };
            if (dialog.ShowDialog() != true)
                return;
            try {
                List<Student> loaded =
                    await _jsonService.LoadAsync(dialog.FileName);
                _repository.Clear();
                Students.Clear();
                foreach (Student student in loaded){
                    _repository.Add(student);
                    Students.Add(student);
                }
                ClearForm();
                RefreshView();
                StatusMessage = $"Завантажено студентів: {Students.Count}";
            }
            catch (Exception ex){
                StatusMessage = $"Помилка завантаження: {ex.Message}";
            }
        }
        private async Task ExportCsvAsync(){
            var dialog = new SaveFileDialog{
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "students.csv"
            };
            if (dialog.ShowDialog() != true)
                return;
            try{
                await _csvService.ExportAsync(
                    dialog.FileName, _repository.GetAll());
                StatusMessage = "Дані успішно експортовано в CSV";
            }
            catch (Exception ex){
                StatusMessage = $"Помилка експорту: {ex.Message}";
            }
        }
        private void ClearForm(){
            SelectedStudent = null;
            StudentName = "";
            StudentAge = "18";
            StudentMass = "60";
            MarksInput = "";
        }
        private bool SetProperty<T>(ref T field,T value,[CallerMemberName] string? propertyName = null){
            if (Equals(field, value))
                return false;
            field = value;
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
