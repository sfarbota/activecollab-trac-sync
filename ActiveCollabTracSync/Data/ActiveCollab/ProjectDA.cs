using System;
using System.Collections.Generic;
using System.Linq;
using ActiveCollabSDK.SDK;
using ActiveCollabTracSync.Entities.ActiveCollab;

namespace ActiveCollabTracSync.Data.ActiveCollab
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProjectDA
    {
        /// <summary>Gets the specified project name.</summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Requested project not found.</exception>
        public static Project Get(string projectName)
        {
            Dictionary<string, object> projects = Client.GetJson(Client.Get("projects"));
            Dictionary<string, object> requestedProject = null;

            foreach (Dictionary<string, object> curProject in projects.Values)
            {
                if (curProject["name"].ToString() == projectName)
                {
                    requestedProject = curProject;
                }
            }

            if (requestedProject == null)
            {
                throw new Exception("Requested project not found.");
            }
            else
            {
                return new Project(requestedProject["id"].ToString(), projectName);
            }
        }

        /// <summary>Gets the tasks.</summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public static List<Task> GetTasks(string projectId)
        {
            var tasks = (Dictionary<string, object>)Client.GetJson(Client.Get("projects/" + projectId + "/tasks"))["tasks"];
            var users = Client.GetJson(Client.Get("users"));
            var taskList = new List<Task>();

            foreach (Dictionary<string, object> task in tasks.Values)
            {
                var assigneeId = task["assignee_id"].ToString();
                var assigneeEmail = "";

                foreach (Dictionary<string, object> curUser in users.Values)
                {
                    if (curUser["id"].ToString() == assigneeId)
                    {
                        assigneeEmail = curUser["email"].ToString();
                        break;
                    }
                }

                var labels = new List<string>();
                foreach (var label in ((Dictionary<string, object>)task["labels"]).Values)
                {
                    labels.Add(label.ToString());
                }

                taskList.Add(new Task(task["id"].ToString(), task["name"].ToString(),
                        task["body"].ToString(), task["is_completed"].ToString() == "true",
                        assigneeEmail, labels));
            }

            return taskList;
        }

        /// <summary>Gets the task lists.</summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns></returns>
        public static List<TaskList> GetTaskLists(string projectId)
        {
            Dictionary<string, object> taskLists = Client.GetJson(Client.Get("projects/" + projectId + "/task-lists"));
            List<TaskList> taskListList = new List<TaskList>();

            foreach (Dictionary<string, object> taskList in taskLists.Values)
            {
                taskListList.Add(new TaskList(taskList["id"].ToString(), taskList["name"].ToString()));
            }

            return taskListList;
        }
    }
}
