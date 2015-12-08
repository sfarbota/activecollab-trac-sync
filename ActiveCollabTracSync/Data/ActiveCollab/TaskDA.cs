using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ActiveCollabSDK.SDK;
using ActiveCollabTracSync.Entities.ActiveCollab;

namespace ActiveCollabTracSync.Data.ActiveCollab
{
    /// <summary>
    /// 
    /// </summary>
    public static class TaskDA
    {
        /// <summary>Updates the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="isCompleted">if set to <c>true</c> [is completed].</param>
        /// <param name="taskListId">The task list identifier.</param>
        /// <param name="assigneeId">The assignee identifier.</param>
        /// <param name="labels">The labels.</param>
        internal static void Update(string id, string name, string description,
                string projectId, bool isCompleted, string taskListId, string assigneeId,
                List<string> labels)
        {
            Dictionary<string, object> taskRequest = new Dictionary<string, object>();
            taskRequest["name"] = name;
            taskRequest["body"] = WebUtility.HtmlEncode(description).Replace("\r\n", "<br/>");
            taskRequest["task_list_id"] = taskListId;
            taskRequest["assignee_id"] = assigneeId;
            taskRequest["labels"] = labels.ToArray();

            Dictionary<string, object> taskResponse = Client.GetJson(Client.Put(
                    "projects/" + projectId + "/tasks/" + id, taskRequest));
            bool isAlreadyCompleted = ((Dictionary<string, object>)taskResponse["single"])
                    ["is_completed"].ToString() == "true";

            if (isCompleted && !isAlreadyCompleted)
            {
                Client.Put("complete/task/" + id);
            }
            else if (!isCompleted && isAlreadyCompleted)
            {
                Client.Put("open/task/" + id);
            }
        }

        /// <summary>Creates the specified name.</summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="isCompleted">if set to <c>true</c> [is completed].</param>
        /// <param name="taskListId">The task list identifier.</param>
        /// <param name="assigneeId">The assignee identifier.</param>
        /// <param name="labels">The labels.</param>
        /// <returns></returns>
        internal static string Create(string name, string description, string projectId,
                bool isCompleted, string taskListId, string assigneeId,
                List<string> labels)
        {
            Dictionary<string, object> taskRequest = new Dictionary<string, object>();
            taskRequest["name"] = name;
            taskRequest["body"] = WebUtility.HtmlEncode(description).Replace("\n", "<br/>");
            taskRequest["task_list_id"] = taskListId;
            taskRequest["assignee_id"] = assigneeId;
            taskRequest["labels"] = labels.ToArray();

            Dictionary<string, object> taskResponse = Client.GetJson(Client.Post(
                    "projects/" + projectId + "/tasks", taskRequest));

            if (isCompleted)
            {
                Client.Post("complete/task/" + taskResponse["id"].ToString());
            }

            return ((Dictionary<string, object>) taskResponse["single"])["id"].ToString();
        }

        /// <summary>Completes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        internal static void Complete(string id)
        {
            Client.Put("complete/task/" + id);
        }
    }
}
