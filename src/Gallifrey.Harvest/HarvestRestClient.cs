using System;
using System.Collections.Generic;
using System.Net;
using Gallifrey.Harvest.Model;
using Gallifrey.Rest;

namespace Gallifrey.Harvest
{
    public class HarvestRestClient : IHarvestClient
    {
        private readonly ISimpleRestClient restClient;

        public HarvestRestClient(string baseUrl, string username, string password)
        {
            restClient = new SimpleRestClient(baseUrl, username, password, (x) => new List<string>());
        }

        public List<DayEntry> GetTimeLogs(DateTime requiredDate)
        {
            var dailyRecord = GetDailyRecord(requiredDate);
            return dailyRecord.day_entries;
        }

        public List<Project> GetProjects(DateTime requiredDate)
        {
            var dailyRecord = GetDailyRecord(requiredDate);
            return dailyRecord.projects;
        }

        public void AddTimelog(Project project, Task task, DateTime logDate, int hoursToLog, string notes = "")
        {
            var postData = new Dictionary<string, object>
            {
                { "hours" , hoursToLog},
                { "notes" , notes},
                { "project_id" , project.id.ToString()},
                { "task_id" , task.id.ToString()},
                { "spent_at" , logDate.ToString("yyyy-M-dd") }
            };

            restClient.Post(HttpStatusCode.Created, "daily/add", postData);
        }

        private DailyRecord GetDailyRecord(DateTime date)
        {
            return restClient.Get<DailyRecord>(HttpStatusCode.OK, $"daily/{date.DayOfYear}/{date.Year}");
        }
    }
}
