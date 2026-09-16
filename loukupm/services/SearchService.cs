using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace loukupm.Services
{
   
    public class SearchService
    {
        public static List<Servies> SearchServices(List<Servies> services, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Servies>(services);

            var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

            return services
                .Where(s => MatchesSearchCriteria(s, normalizedSearchTerm))
                .ToList();
        }

        public static List<WorkTeam> SearchWorkTeams(List<WorkTeam> teams, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<WorkTeam>(teams);

            var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

            return teams
                .Where(t => MatchesWorkTeamCriteria(t, normalizedSearchTerm))
                .ToList();
        }

      
        private static string NormalizeSearchTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return string.Empty;

         
            var normalized = term.ToLower().Trim();

         
            normalized = Regex.Replace(normalized, @"\s+", " ");

            return normalized;
        }

        
        private static bool MatchesSearchCriteria(Servies service, string searchTerm)
        {
            if (service == null || string.IsNullOrWhiteSpace(searchTerm))
                return true;

          
            if (!string.IsNullOrWhiteSpace(service.NameServies) &&
                service.NameServies.ToLower().Contains(searchTerm))
                return true;

           
            if (service.Category != null &&
                !string.IsNullOrWhiteSpace(service.Category.Name) &&
                service.Category.Name.ToLower().Contains(searchTerm))
                return true;

          
            if (!string.IsNullOrWhiteSpace(service.PriceServies) &&
                service.PriceServies.ToLower().Contains(searchTerm))
                return true;

            return false;
        }

        
        private static bool MatchesWorkTeamCriteria(WorkTeam team, string searchTerm)
        {
            if (team == null || string.IsNullOrWhiteSpace(searchTerm))
                return true;

           
            if (!string.IsNullOrWhiteSpace(team.Name) &&
                team.Name.ToLower().Contains(searchTerm))
                return true;

          
            if (!string.IsNullOrWhiteSpace(team.Description) &&
                team.Description.ToLower().Contains(searchTerm))
                return true;

          
            if (!string.IsNullOrWhiteSpace(team.Job) &&
                team.Job.ToLower().Contains(searchTerm))
                return true;

            return false;
        }

       
        public static string HighlightMatch(string text, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchTerm))
                return text;

            var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
            return regex.Replace(text, match => $"<highlight>{match.Value}</highlight>");
        }

        
        public static int GetRelevanceScore(Servies service, string searchTerm)
        {
            if (service == null || string.IsNullOrWhiteSpace(searchTerm))
                return 0;

            int score = 0;
            var normalizedSearch = NormalizeSearchTerm(searchTerm);
            var lowerSearch = normalizedSearch.ToLower();

        
            if (service.NameServies?.ToLower() == lowerSearch)
                score += 100;
           
            else if (service.NameServies?.ToLower().StartsWith(lowerSearch) == true)
                score += 80;
            
            else if (service.NameServies?.ToLower().Contains(lowerSearch) == true)
                score += 50;

         
            if (service.Category?.Name?.ToLower().Contains(lowerSearch) == true)
                score += 30;

          
            if (service.PriceServies?.ToLower().Contains(lowerSearch) == true)
                score += 10;

            return score;
        }

       
        public static List<Servies> SortByRelevance(List<Servies> services, string searchTerm)
        {
            return services
                .OrderByDescending(s => GetRelevanceScore(s, searchTerm))
                .ToList();
        }
    }
}
