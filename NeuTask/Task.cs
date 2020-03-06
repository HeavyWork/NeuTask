using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeuTask
{

    /// <summary>
    /// The main Task class.
    /// </summary>
    public abstract class Task : INotifyPropertyChanged, IDisposable
    {

        #region DataContext

        /// <summary>
        /// The Id of the task.
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The name of the task.
        /// </summary>
        public string Name { get; } = "";

        private string _target = "";

        /// <summary>
        /// The processing files of the task.
        /// </summary>
        public string Target
        {
            get => _target;
            set
            {
                _target = value;
                OnPropertyChanged(nameof(Target));
            }
        }

        private TaskStatus _status = TaskStatus.Undefined;

        public TaskStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(DisplayStatus));
            }
        }

        /// <summary>
        /// The task status for display.
        /// </summary>
        public string DisplayStatus =>
            TaskStatusConverter.Current.Convert(
                _status,
                typeof(string),
                null,
                CultureInfo.CurrentCulture) as string;

        private string _message = "";

        /// <summary>
        /// Current message of the task.
        /// </summary>
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private double _percentage;

        /// <summary>
        /// Current percentage of the task.
        /// </summary>
        public double Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }

        private bool _handled;

        public bool Handled
        {
            get => _handled;
            set
            {
                _handled = value;
                OnPropertyChanged(nameof(Handled));
            }
        }

        #endregion

        #region Task Core

        /// <summary>
        /// Start the task.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stop the task.
        /// </summary>
        public abstract void Stop();

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (Status == TaskStatus.Running)
            {
                Stop();
            }
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    /// <summary>
    /// The task status enum.
    /// </summary>
    public enum TaskStatus
    {
        Undefined = 0,
        Waiting = 1,
        Running = 2,
        Complete = 3,
        Failed = 4
    }

    public class TaskStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;
            TaskStatusConvertDictionary.TryGetValue((TaskStatus) value, out string result);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;
            IEnumerable<KeyValuePair<TaskStatus, string>> result = TaskStatusConvertDictionary.Where(pair => pair.Value == value as string);
            if (result.Any()) return result.FirstOrDefault().Key;
            return null;
        }

        /// <summary>
        /// The static TaskStatusConverter.
        /// </summary>
        public static TaskStatusConverter Current { get; } = new TaskStatusConverter();

        public static Dictionary<TaskStatus, string> TaskStatusConvertDictionary { get; } =
            new Dictionary<TaskStatus, string>()
            {
                {TaskStatus.Undefined, ""},
                {TaskStatus.Waiting, "等待"},
                {TaskStatus.Running, "运行"},
                {TaskStatus.Complete, "完成"},
                {TaskStatus.Failed, "失败"}
            };
    }
}
