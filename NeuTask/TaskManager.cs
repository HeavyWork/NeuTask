using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuTask
{

    /// <summary>
    /// The main TaskManager class.
    /// </summary>
    public sealed class TaskManager
    {

        #region Current

        /// <summary>
        /// The current task manager.
        /// </summary>
        public static TaskManager Current { get; } = new TaskManager();

        #endregion

    }

}
