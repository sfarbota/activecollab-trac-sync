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
    enum TicketGroupingOption
    {
        /// <summary>The type</summary>
        Type,
        /// <summary>The component</summary>
        Component,
        /// <summary>The severity</summary>
        Severity,
        /// <summary>The priority</summary>
        Priority,
        /// <summary>The owner</summary>
        Owner,
        /// <summary>The reporter</summary>
        Reporter,
        /// <summary>The milestone</summary>
        Milestone,
        /// <summary>The status</summary>
        Status
    }
}
