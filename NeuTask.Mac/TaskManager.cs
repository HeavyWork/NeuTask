﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeuTask.Mac
{

    /// <summary>
    /// The main TaskManager class.
    /// </summary>
    public sealed class TaskManager : INotifyPropertyChanged
    {

        public TaskManager()
        {
            TaskList.CollectionChanged += TaskListCollectionChanged;
        }

        #region Current

        /// <summary>
        /// The current task manager.
        /// </summary>
        public static TaskManager Current { get; } = new TaskManager();

        #endregion

        #region TaskList Operators

        /// <summary>
        /// The task list.
        /// </summary>
        public ObservableCollection<Task> TaskList { get; set; } = new ObservableCollection<Task>();

        /// <summary>
        /// Add task to queue.
        /// </summary>
        /// <param name="task">The task.</param>
        public void Push(Task task)
        {
            TaskList.Add(task);
        }

        /// <summary>
        /// Remove task from queue.
        /// </summary>
        /// <param name="task">The task.</param>
        public void Remove(Task task)
        {
            if (TaskList.Contains(task)) TaskList.Remove(task);
        }

        /// <summary>
        /// Force stop the queue.
        /// </summary>
        public void Stop()
        {
            Queue = false;
            CurrentTask?.Stop();
        }

        /// <summary>
        /// Clear the queue. Do nothing when the current task is running.
        /// </summary>
        public void Clear()
        {
            if (Status == TaskStatus.Running) return;
            _totalTask = 0;
            TaskList.Clear();
        }

        #endregion

        #region TaskManager Operator

        private bool _queue;

        /// <summary>
        /// The queue dispatching status.
        /// </summary>
        public bool Queue
        {
            get => _queue;
            set
            {
                if (!(CurrentTask is null))
                {
                    if (_queue && !value)
                        CurrentTask.PropertyChanged -= QueueDispatch;
                    else if (value && !_queue)
                    {
                        DispatchStart(true);
                        CurrentTask.PropertyChanged += QueueDispatch;
                    }
                }
                _queue = value;
                OnPropertyChanged(nameof(Queue));
            }
        }

        #endregion

        #region TaskManager Status

        public TaskStatus Status => CurrentTask?.Status ?? TaskStatus.Undefined;

        public string DisplayStatus => CurrentTask?.DisplayStatus ?? "";

        public string Message => CurrentTask?.Message ?? "";

        public double TaskPercentage => CurrentTask?.Percentage ?? 0;

        public double TotalPercentage => (TaskList.Count - 1) / (double)_totalTask;

        #endregion

        #region TotalPercentage Calculator

        private int _totalTask;

        #endregion

        #region Queue Dispatcher

        private void QueueDispatch(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CurrentTask.Status) || !TaskList.Contains(CurrentTask)) return;
            if (
                CurrentTask.Status == TaskStatus.Complete ||
                CurrentTask.Status == TaskStatus.Failed && CurrentTask.Handled
            ) TaskList.Remove(CurrentTask);
        }

        private void DispatchStart(bool flag = false)
        {
            if (!flag)
            {
                if (Queue && !(CurrentTask is null) && CurrentTask.Status == TaskStatus.Waiting) CurrentTask.Start();
            }
            else
                if (!(CurrentTask is null) && CurrentTask.Status == TaskStatus.Waiting) CurrentTask.Start();
        }

        #endregion

        #region CurrentTask Holder

        private Task _currentTask;

        /// <summary>
        /// The current task: the first task in queue.
        /// </summary>
        public Task CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;
                OnPropertyChanged(nameof(CurrentTask));
            }
        }

        #endregion

        #region CurrentTask Event Handler

        private void CurrentTaskOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TaskPropertyReflectionDictionary.TryGetValue(e.PropertyName, out string value);
            if (!string.IsNullOrEmpty(value)) OnPropertyChanged(value);
        }

        private static readonly Dictionary<string, string> TaskPropertyReflectionDictionary = new Dictionary<string, string>
        {
            { nameof(Task.Status), nameof(Status) },
            { nameof(Task.DisplayStatus), nameof(DisplayStatus) },
            { nameof(Task.Message), nameof(Message) },
            { nameof(Task.Percentage), nameof(TaskPercentage) }
        };

        #endregion

        #region TaskList Event Handler

        private void TaskListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!TaskList.Any())
            {
                _totalTask = 0;
                return;
            }
            if (e.Action == NotifyCollectionChangedAction.Add) _totalTask++;
            if (CurrentTask != null && CurrentTask.Equals(TaskList[0])) return;
            // Unplug Event Handler
            if (CurrentTask != null)
            {
                if (Queue) CurrentTask.PropertyChanged -= QueueDispatch;
                CurrentTask.PropertyChanged -= CurrentTaskOnPropertyChanged;
                CurrentTask.Dispose();
            }
            // Replug Event Handler
            CurrentTask = TaskList[0];
            if (Queue) CurrentTask.PropertyChanged += QueueDispatch;
            CurrentTask.PropertyChanged += CurrentTaskOnPropertyChanged;

            DispatchStart();
            OnPropertyChanged(nameof(TotalPercentage));
        }

        #endregion

        #region PropertyChanged Handler

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

}
