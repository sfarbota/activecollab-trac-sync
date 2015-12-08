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
    public class Task
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }
        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>Gets or sets the assignee.</summary>
        /// <value>The assignee.</value>
        public string Assignee { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleted { get; set; }
        /// <summary>Gets or sets the labels.</summary>
        /// <value>The labels.</value>
        public List<string> Labels { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Task"/> class.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="isCompleted">if set to <c>true</c> [is completed].</param>
        /// <param name="assignee">The assignee.</param>
        /// <param name="labels">The labels.</param>
        public Task(string id, string name, string description, bool isCompleted,
                string assignee = null, List<string> labels = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Assignee = assignee;
            IsCompleted = isCompleted;
            Labels = labels;
        }
    }
}
