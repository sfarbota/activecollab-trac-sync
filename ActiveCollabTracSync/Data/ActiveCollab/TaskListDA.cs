using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveCollabSDK.SDK;

namespace ActiveCollabTracSync.Data.ActiveCollab
{
    /// <summary>
    /// 
    /// </summary>
    public static class TaskListDA
    {
        /// <summary>Creates the specified name.</summary>
        /// <param name="name">The name.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        internal static string Create(string name, string projectId)
        {
            Dictionary<string, object> taskListRequest = new Dictionary<string, object>();
            taskListRequest["name"] = name;

            Dictionary<string, object> taskListResponse = Client.GetJson(Client.Post(
                    "projects/" + projectId + "/task-lists", taskListRequest));

            return ((Dictionary<string, object>)taskListResponse["single"])["id"].ToString();
        }
    }
}
