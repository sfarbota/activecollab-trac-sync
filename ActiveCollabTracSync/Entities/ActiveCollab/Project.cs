using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCollabTracSync.Entities.ActiveCollab
{
    /// <summary>
    /// 
    /// </summary>
    public class Project
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }
        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>Gets or sets the task lists.</summary>
        /// <value>The task lists.</value>
        public List<TaskList> TaskLists { get; set; }
        /// <summary>Gets or sets the tasks.</summary>
        /// <value>The tasks.</value>
        public List<Task> Tasks { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public Project(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
