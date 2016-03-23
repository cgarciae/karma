using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    // Update tasks once per frame based on a priority
    [System.Diagnostics.DebuggerStepThrough]
    public class TaskUpdater<TTask>
    {
        readonly LinkedList<TaskInfo> _tasks = new LinkedList<TaskInfo>();
        readonly List<TaskInfo> _queuedTasks = new List<TaskInfo>();

        Action<TTask> _updateFunc;

        IEnumerable<TaskInfo> AllTasks
        {
            get
            {
                return ActiveTasks.Concat(_queuedTasks);
            }
        }

        IEnumerable<TaskInfo> ActiveTasks
        {
            get
            {
                return _tasks;
            }
        }

        public TaskUpdater(Action<TTask> updateFunc)
        {
            _updateFunc = updateFunc;
        }

        public void AddTask(TTask task, int priority)
        {
            AddTaskInternal(task, priority);
        }

        void AddTaskInternal(TTask task, int priority)
        {
            Assert.That(!AllTasks.Select(x => x.Task).Contains(task),
                "Duplicate task added to kernel with name '" + task.GetType().FullName + "'");

            // Wait until next frame to add the task, otherwise whether it gets updated
            // on the current frame depends on where in the update order it was added
            // from, so you might get off by one frame issues
            _queuedTasks.Add(new TaskInfo(task, priority));
        }

        public void RemoveTask(TTask task)
        {
            var info = AllTasks.Where(x => ReferenceEquals(x.Task, task)).Single();

            Assert.That(!info.IsRemoved, "Tried to remove task twice, task = " + task.GetType().Name);
            info.IsRemoved = true;
        }

        public void OnFrameStart()
        {
            // See above comment
            AddQueuedTasks();
        }

        public void UpdateAll()
        {
            UpdateRange(int.MinValue, int.MaxValue);
        }

        public void UpdateRange(int minPriority, int maxPriority)
        {
            var node = _tasks.First;

            while (node != null)
            {
                var next = node.Next;
                var taskInfo = node.Value;

                // Make sure that tasks with priority of int.MaxValue are updated when maxPriority is int.MaxValue
                if (!taskInfo.IsRemoved && taskInfo.Priority >= minPriority
                    && (maxPriority == int.MaxValue || taskInfo.Priority < maxPriority))
                {
                    _updateFunc(taskInfo.Task);
                }

                node = next;
            }

            ClearRemovedTasks(_tasks);
        }

        void ClearRemovedTasks(LinkedList<TaskInfo> tasks)
        {
            var node = tasks.First;

            while (node != null)
            {
                var next = node.Next;
                var info = node.Value;

                if (info.IsRemoved)
                {
                    //Log.Debug("Removed task '" + info.Task.GetType().ToString() + "'");
                    tasks.Remove(node);
                }

                node = next;
            }
        }

        void AddQueuedTasks()
        {
            foreach (var task in _queuedTasks)
            {
                if (!task.IsRemoved)
                {
                    InsertTaskSorted(task);
                }
            }
            _queuedTasks.Clear();
        }

        void InsertTaskSorted(TaskInfo task)
        {
            for (var current = _tasks.First; current != null; current = current.Next)
            {
                if (current.Value.Priority > task.Priority)
                {
                    _tasks.AddBefore(current, task);
                    return;
                }
            }

            _tasks.AddLast(task);
        }

        class TaskInfo
        {
            public TTask Task;
            public int Priority;
            public bool IsRemoved;

            public TaskInfo(TTask task, int priority)
            {
                Task = task;
                Priority = priority;
            }
        }
    }
}
