using System;
using System.Collections.Generic;
using Gallifrey.Harvest.Model;

namespace Gallifrey.Harvest
{
    public interface IHarvestClient
    {
        List<DayEntry> GetTimeLogs(DateTime requiredDate);
        List<Project> GetProjects(DateTime requiredDate);
        void AddTimelog(Project project, Task task, DateTime logDate, int hoursToLog, string notes = "");
    }
}