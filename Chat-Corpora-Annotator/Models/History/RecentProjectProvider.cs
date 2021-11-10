using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace ChatCorporaAnnotator.Models.History
{
    internal class RecentProjectProvider
    {
        public ObservableCollection<IRecentProject> RecentProjects { get; }

        private int _maxRecentProjectsCount;
        public int MaxRecentProjectsCount
        {
            get => _maxRecentProjectsCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be greater than or equal to zero.");

                while (RecentProjects.Count > value)
                {
                    RecentProjects.RemoveAt(RecentProjects.Count - 1);
                }

                _maxRecentProjectsCount = value;
            }
        }

        public RecentProjectProvider(IEnumerable<IRecentProject> recentProjects = null, int maxRecentProjectsCount = 5)
        {
            RecentProjects = new ObservableCollection<IRecentProject>(recentProjects ?? new IRecentProject[0]);
            MaxRecentProjectsCount = maxRecentProjectsCount;
        }

        public void AddProject(IRecentProject project)
        {
            if (RecentProjects.Contains(project))
            {
                RecentProjects.Remove(project);
                RecentProjects.Insert(0, project);

                return;
            }

            RecentProjects.Insert(0, project);

            if (RecentProjects.Count > MaxRecentProjectsCount)
                RecentProjects.RemoveAt(RecentProjects.Count - 1);
        }

        public void AddProjects(IEnumerable<IRecentProject> projects)
        {
            if (projects == null)
                return;

            foreach (var project in projects)
                AddProject(project);
        }

        public bool RemoveProject(IRecentProject project)
        {
            return RecentProjects.Remove(project);
        }

        public bool RemoveLastProject()
        {
            if (RecentProjects.Count == 0)
                return false;

            RecentProjects.RemoveAt(RecentProjects.Count - 1);
            return true;
        }

        public void Clear()
        {
            RecentProjects.Clear();
        }
    }
}
