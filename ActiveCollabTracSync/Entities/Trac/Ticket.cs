using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCollabTracSync.Entities.Trac
{
    /// <summary>
    /// 
    /// </summary>
    public class Ticket
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }
        /// <summary>Gets or sets the summary.</summary>
        /// <value>The summary.</value>
        public string Summary { get; set; }
        /// <summary>Gets or sets the description.</summary>
        /// <value>The description.</value>
        public string Description { get; set; }
        /// <summary>Gets or sets the owner.</summary>
        /// <value>The owner.</value>
        public string Owner { get; set; }
        /// <summary>Gets or sets the status.</summary>
        /// <value>The status.</value>
        public string Status { get; set; }
        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public string Type { get; set; }
        /// <summary>Gets or sets the group.</summary>
        /// <value>The group.</value>
        public string Group { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Ticket"/> class.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="summary">The summary.</param>
        /// <param name="description">The description.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="status">The status.</param>
        /// <param name="type">The type.</param>
        /// <param name="group">The group.</param>
        public Ticket(string id, string summary, string description, string owner,
                string status, string type, string group)
        {
            Id = id;
            Summary = summary;
            Description = description;
            Owner = owner;
            Status = status;
            Type = type;
            Group = group;
        }
    }
}
