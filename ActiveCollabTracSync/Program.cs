using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ActiveCollabTracSync.Data.ActiveCollab;
using ActiveCollabTracSync.Data.Trac;
using ActiveCollabTracSync.Entities.ActiveCollab;
using ActiveCollabTracSync.Entities.Trac;
using ActiveCollabSDK.SDK;

namespace ActiveCollabTracSync
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        /// <summary>Runs the program with the specified arguments.</summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Client.url = "https://app.activecollab.com/" + ConfigurationManager.AppSettings["ActiveCollabCloudInstanceID"];
            Client.key = ConfigurationManager.AppSettings["ActiveCollabApiKey"];

            var groupingField = ConfigurationManager.AppSettings["TracFieldForActiveCollabTaskGrouping"];
            var projectName = ConfigurationManager.AppSettings["ActiveCollabProjectName"];
            var maxAttempts = GetMaxAttempts();

            Console.Write("Getting data for Active Collab project \"" + projectName + "\"...");

            Project project = null;
            var getProjectNameAttemptCount = 0;
            var getProjectNameAttemptSucceeded = false;

            while (!getProjectNameAttemptSucceeded)
            {
                try
                {
                    project = ProjectDA.Get(projectName);
                    Console.WriteLine("Complete!");
                    getProjectNameAttemptSucceeded = true;
                }
                catch
                {
                    if (getProjectNameAttemptCount < maxAttempts)
                    {
                        getProjectNameAttemptCount++;
                        Console.Write("Attempt " + getProjectNameAttemptCount + " failed...");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (args.Length > 0)
            {
                // If a ticket ID is given, only sync that ticket.
                SyncTicket(TicketDA.Get(args[0], groupingField), project);
            }
            else
            {
                Console.WriteLine("\nSynchronizing all open tickets in Trac to Active Collab...\n");

                var openTracTickets = TicketDA.GetAllOpen(groupingField);
                SyncTickets(openTracTickets, project);

                Console.WriteLine("\nSynchronizing tickets with open tasks in Active Collab"
                        + " but no open ticket in Trac...\n");


                Console.Write("Getting list of all tickets with open tasks in the Active Collab project...");

                var getTasksAttemptCount = 0;
                var getTasksAttemptSucceeded = false;
                List<Task> activeCollabProjectTasks = null;

                while (!getTasksAttemptSucceeded)
                {
                    try
                    {
                        activeCollabProjectTasks = ProjectDA.GetTasks(project.Id);
                        Console.WriteLine("Complete!");
                        getTasksAttemptSucceeded = true;
                    }
                    catch
                    {
                        if (getTasksAttemptCount < maxAttempts)
                        {
                            getTasksAttemptCount++;
                            Console.Write("Attempt " + getTasksAttemptCount + " failed...");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                var tracTicketIdsFromActiveCollabTasks = new List<string>();
                foreach (var curTask in activeCollabProjectTasks)
                {
                    string curTracTicketId = GetTracTicketIdFromActiveCollabTaskName(curTask.Name);

                    if (curTracTicketId == null)
                    {
                        Console.Write("Completing task for ticket #" + curTracTicketId + "...");

                        var completeAttemptCount = 0;
                        var completeAttemptSucceeded = false;

                        while (!completeAttemptSucceeded)
                        {
                            try
                            {
                                TaskDA.Complete(curTask.Id);
                                Console.WriteLine("Complete!");
                                completeAttemptSucceeded = true;
                            }
                            catch
                            {
                                if (completeAttemptCount < maxAttempts)
                                {
                                    completeAttemptCount++;
                                    Console.Write("Attempt " + completeAttemptCount + " failed...");
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    else
                    {
                        tracTicketIdsFromActiveCollabTasks.Add(curTracTicketId);
                    }
                }

                Console.Write("\nRemoving tickets that have already been synchronized from the list...");

                tracTicketIdsFromActiveCollabTasks.RemoveAll(
                        ticketId => openTracTickets.Select(
                            ticket => ticket.Id
                        ).Contains(ticketId));

                Console.WriteLine("Complete!");

                SyncTickets(TicketDA.GetFromList(tracTicketIdsFromActiveCollabTasks, groupingField), project);

                Console.WriteLine("\nSync process complete!");
            }
        }

        /// <summary>
        /// Gets the name of the Trac ticket identifier from the Active Collab task.
        /// </summary>
        /// <param name="activeCollabTaskName">Name of the Active Collab task.</param>
        /// <returns></returns>
        private static string GetTracTicketIdFromActiveCollabTaskName(string activeCollabTaskName)
        {
            var dashIndex = activeCollabTaskName.IndexOf("-");

            if (dashIndex > 0)
            {
                var ticketNameSubstring = activeCollabTaskName.Substring(0, dashIndex);
                int ticketNumber;

                if (int.TryParse(ticketNameSubstring, out ticketNumber))
                {
                    return ticketNameSubstring;
                }
            }

            return null;
        }

        /// <summary>Synchronizes the ticket.</summary>
        /// <param name="tracTicket">The Trac ticket.</param>
        /// <param name="activeCollabProject">The Active Collab project.</param>
        private static void SyncTicket(Ticket tracTicket, Project activeCollabProject)
        {
            var users = Client.GetJson(Client.Get("users"));
            var activeCollabProjectTaskLists = ProjectDA.GetTaskLists(activeCollabProject.Id);
            var activeCollabProjectTasks = ProjectDA.GetTasks(activeCollabProject.Id);

            UpdateTicket(tracTicket, activeCollabProject, users,
                    activeCollabProjectTaskLists, activeCollabProjectTasks);
        }

        /// <summary>Synchronizes the tickets.</summary>
        /// <param name="tracTickets">The Trac tickets.</param>
        /// <param name="activeCollabProject">The Active Collab project.</param>
        private static void SyncTickets(List<Ticket> tracTickets, Project activeCollabProject)
        {
            var users = Client.GetJson(Client.Get("users"));
            var activeCollabProjectTaskLists = ProjectDA.GetTaskLists(activeCollabProject.Id);
            var activeCollabProjectTasks = ProjectDA.GetTasks(activeCollabProject.Id);

            foreach (var tracTicket in tracTickets)
            {
                UpdateTicket(tracTicket, activeCollabProject, users,
                        activeCollabProjectTaskLists, activeCollabProjectTasks);
            }
        }

        /// <summary>Updates the ticket.</summary>
        /// <param name="tracTicket">The Trac ticket.</param>
        /// <param name="activeCollabProject">The Active Collab project.</param>
        /// <param name="users">The users.</param>
        /// <param name="activeCollabProjectTaskLists">The Active Collab project task lists.</param>
        /// <param name="activeCollabProjectTasks">The Active Collab project tasks.</param>
        private static void UpdateTicket(Ticket tracTicket, Project activeCollabProject,
        Dictionary<string, object> users, List<TaskList> activeCollabProjectTaskLists,
        List<Task> activeCollabProjectTasks)
        {
            Console.Write("Synchronizing ticket #" + tracTicket.Id + "...");

            string[] includedUserEmails = ConfigurationManager.AppSettings["IncludedUserEmails"].Split(',');
            var maxAttempts = GetMaxAttempts();

            var newActiveCollabTaskName = tracTicket.Id + "-" + tracTicket.Summary;
            var newActiveCollabTaskDescription = tracTicket.Description;
            var newActiveCollabTaskIsCompleted = tracTicket.Status == "closed";
            var newActiveCollabTaskLabels = new List<string>();

            newActiveCollabTaskLabels.Add(tracTicket.Status.ToUpper());
            newActiveCollabTaskLabels.Add(tracTicket.Type.ToUpper());

            string assigneeId = "";

            if (includedUserEmails.Count() == 0 || includedUserEmails.Contains(tracTicket.Owner))
            {
                foreach (Dictionary<string, object> curUser in users.Values)
                {
                    if (curUser["email"].ToString() == tracTicket.Owner)
                    {
                        assigneeId = curUser["id"].ToString();
                        break;
                    }
                }
            }

            string taskListId = null;

            foreach (var curTaskList in activeCollabProjectTaskLists)
            {
                if (curTaskList.Name == tracTicket.Group)
                {
                    taskListId = curTaskList.Id;
                    break;
                }
            }

            if (taskListId == null)
            {
                taskListId = TaskListDA.Create(tracTicket.Group, activeCollabProject.Id);
                activeCollabProjectTaskLists.Add(new TaskList(taskListId, tracTicket.Group));
            }

            foreach (Entities.ActiveCollab.Task curActiveCollabProjectTask
                    in activeCollabProjectTasks)
            {
                if (GetTracTicketIdFromActiveCollabTaskName(curActiveCollabProjectTask.Name)
                        == tracTicket.Id)
                {
                    var updateAttemptCount = 0;
                    var updateAttemptSucceeded = false;

                    while (!updateAttemptSucceeded)
                    {
                        try
                        {
                            TaskDA.Update(curActiveCollabProjectTask.Id, newActiveCollabTaskName,
                                    newActiveCollabTaskDescription, activeCollabProject.Id,
                                    newActiveCollabTaskIsCompleted, taskListId, assigneeId,
                                    newActiveCollabTaskLabels);
                            Console.WriteLine("Complete!");
                            updateAttemptSucceeded = true;
                        }
                        catch
                        {
                            if (updateAttemptCount < maxAttempts)
                            {
                                updateAttemptCount++;
                                Console.Write("Attempt " + updateAttemptCount + " failed...");
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    return;
                }
            }

            var createAttemptCount = 0;
            var createAttemptSucceeded = false;

            while (!createAttemptSucceeded)
            {
                try
                {
                    TaskDA.Create(newActiveCollabTaskName, newActiveCollabTaskDescription,
                            activeCollabProject.Id, newActiveCollabTaskIsCompleted, taskListId,
                            assigneeId, newActiveCollabTaskLabels);
                    Console.WriteLine("Complete!");
                    createAttemptSucceeded = true;
                }
                catch
                {
                    if (createAttemptCount < maxAttempts)
                    {
                        createAttemptCount++;
                        Console.Write("Attempt " + createAttemptCount + " failed...");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>Gets the maximum attempts.</summary>
        /// <returns></returns>
        private static int GetMaxAttempts()
        {
            int maxAttempts;

            if (!int.TryParse(ConfigurationManager.AppSettings["MaxAttempts"], out maxAttempts)
                    || maxAttempts <= 0)
            {
                maxAttempts = 1;
            }

            return maxAttempts;
        }
    }
}
