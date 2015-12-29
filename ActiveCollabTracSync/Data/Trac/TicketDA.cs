using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ActiveCollabTracSync.Entities.Trac;
using MySql.Data.MySqlClient;

namespace ActiveCollabTracSync.Data.Trac
{
    /// <summary>
    /// 
    /// </summary>
    public static class TicketDA
    {
        /// <summary>Gets the specified ticket identifier.</summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="groupingField">The grouping field.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Ticket  + ticketId +  not found in Trac database.</exception>
        public static Ticket Get(string ticketId, string groupingField = "")
        {
            // Get open Trac tickets without a custom field.
            var sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type" +
                            " FROM ticket t" +
                            " WHERE t.id = @ticketId" +
                            " ORDER BY t.id;";

            if (!Enum.IsDefined(typeof(TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
            {
                // Assume groupingField is a custom Trac ticket field and should be used for grouping in ActiveCollab.
                sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type, tc.value AS 'group'" +
                            " FROM ticket t" +
                                " LEFT JOIN ticket_custom tc" +
                                    " ON t.id = tc.ticket" +
                                        " AND tc.name = @groupingField" +
                            " WHERE t.id = @ticketId" +
                            " ORDER BY t.id;";
            }

            var tickets = GetTicketsFromDatabaseById(sql, groupingField, ticketId);

            if (tickets.Count > 0)
            {
                return tickets[0];
            }
            else
            {
                throw new Exception("Ticket " + ticketId + " not found in Trac database.");
            }
        }

        /// <summary>Gets all open.</summary>
        /// <param name="groupingField">The grouping field.</param>
        /// <returns></returns>
        public static List<Ticket> GetAllOpen(string groupingField = "")
        {
            // Get open Trac tickets without a custom field.
            var sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type" +
                            " FROM ticket t" +
                            " WHERE t.status <> 'closed'" +
                            " ORDER BY t.id;";

            if (!Enum.IsDefined(typeof(TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
            {
                // Assume groupingField is a custom Trac ticket field and should be used for grouping in ActiveCollab.
                sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type, tc.value AS 'group'" +
                            " FROM ticket t" +
                                " LEFT JOIN ticket_custom tc" +
                                    " ON t.id = tc.ticket" +
                                        " AND tc.name = @groupingField" +
                            " WHERE t.status <> 'closed'" +
                            " ORDER BY t.id;";
            }

            return GetTicketsFromDatabase(sql, groupingField);
        }

        /// <summary>Gets from list.</summary>
        /// <param name="ticketIdList">The ticket identifier list.</param>
        /// <param name="groupingField">The grouping field.</param>
        /// <returns></returns>
        public static List<Ticket> GetFromList(List<string> ticketIdList, string groupingField = "")
        {
            if (ticketIdList.Count > 0)
            {
                // Get Trac tickets from list without a custom field.
                var sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type" +
                                " FROM ticket t" +
                                " WHERE t.id IN (" + String.Join(",", ticketIdList) + ")" +
                                " ORDER BY t.id;";

                if (!Enum.IsDefined(typeof(TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
                {
                    // Assume groupingField is a custom Trac ticket field and should be used for grouping in Active Collab.
                    sql =   "SELECT t.id, t.summary, t.description, t.owner, t.status, t.type, tc.value AS 'group'" +
                                " FROM ticket t" +
                                    " LEFT JOIN ticket_custom tc" +
                                        " ON t.id = tc.ticket" +
                                            " AND tc.name = @groupingField" +
                                " WHERE t.id IN (" + String.Join(",", ticketIdList) + ")" +
                                " ORDER BY t.id;";
                }

                return GetTicketsFromDatabase(sql, groupingField);
            }
            else
            {
                return new List<Ticket>();
            }
        }

        /// <summary>Gets the tickets from database.</summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="groupingField">The grouping field.</param>
        /// <returns></returns>
        private static List<Ticket> GetTicketsFromDatabase(string sql, string groupingField)
        {
            using (var connection = new MySqlConnection(
                    ConfigurationManager.AppSettings["TracMySqlConnectionString"]))
            {
                connection.Open();

                var command = new MySqlCommand(sql, connection);

                if (!Enum.IsDefined(typeof (TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
                {
                    command.Parameters.AddWithValue("@groupingField", groupingField);
                }
                
                var dataReader = command.ExecuteReader();
                var ticketList = new List<Ticket>();

                while (dataReader.Read())
                {
                    var id = dataReader.GetString("id");
                    var summary = dataReader.GetString("summary");
                    var description = dataReader.GetString("description");
                    var owner = dataReader.GetString("owner");
                    var status = dataReader.GetString("status");
                    var type = dataReader.GetString("type");
                    var group = "";

                    if (!Enum.IsDefined(typeof(TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
                    {
                        group = dataReader.IsDBNull(dataReader.GetOrdinal("group"))
                            || string.IsNullOrEmpty(dataReader.GetString("group"))
                                ? "Other"
                                : dataReader.GetString("group");
                    }

                    ticketList.Add(new Ticket(id, summary, description, owner, status, type, group));
                }

                return ticketList;
            }
        }

        /// <summary>Gets the tickets from database by identifier.</summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="groupingField">The grouping field.</param>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <returns></returns>
        private static List<Ticket> GetTicketsFromDatabaseById(string sql, string groupingField, string ticketId)
        {
            using (var connection = new MySqlConnection(
                    ConfigurationManager.AppSettings["TracMySqlConnectionString"]))
            {
                connection.Open();

                var command = new MySqlCommand(sql, connection);

                if (!Enum.IsDefined(typeof (TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
                {
                    command.Parameters.AddWithValue("@groupingField", groupingField);
                }

                if (!string.IsNullOrEmpty(ticketId))
                {
                    command.Parameters.AddWithValue("@ticketId", ticketId);
                }
                
                var dataReader = command.ExecuteReader();
                var ticketList = new List<Ticket>();

                while (dataReader.Read())
                {
                    var id = dataReader.GetString("id");
                    var summary = dataReader.GetString("summary");
                    var description = dataReader.GetString("description");
                    var owner = dataReader.GetString("owner");
                    var status = dataReader.GetString("status");
                    var type = dataReader.GetString("type");
                    var group = "";

                    if (!Enum.IsDefined(typeof(TicketGroupingOption), groupingField) && !string.IsNullOrEmpty(groupingField))
                    {
                        group = dataReader.IsDBNull(dataReader.GetOrdinal("group"))
                            || string.IsNullOrEmpty(dataReader.GetString("group"))
                                ? "Other"
                                : dataReader.GetString("group");
                    }

                    ticketList.Add(new Ticket(id, summary, description, owner, status, type, group));
                }

                return ticketList;
            }
        }
    }
}
