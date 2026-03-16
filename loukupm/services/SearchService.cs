using loukupm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace loukupm.services
{
    /// <summary>
    /// Fast, local search service for filtering services
    /// Provides string matching with case-insensitive search
    /// </summary>
    public class SearchService
    {
        /// <summary>
        /// Search services by name or category with fast filtering
        /// </summary>
        public static List<Servies> SearchServices(List<Servies> services, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Servies>(services);

            var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

            return services
                .Where(s => MatchesSearchCriteria(s, normalizedSearchTerm))
                .ToList();
        }

        /// <summary>
        /// Search work team members by name
        /// </summary>
        public static List<WorkTeam> SearchWorkTeams(List<WorkTeam> teams, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<WorkTeam>(teams);

            var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

            return teams
                .Where(t => MatchesWorkTeamCriteria(t, normalizedSearchTerm))
                .ToList();
        }

        /// <summary>
        /// Normalize search term: lowercase, trim, remove extra spaces
        /// </summary>
        private static string NormalizeSearchTerm(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return string.Empty;

            // Convert to lowercase and trim
            var normalized = term.ToLower().Trim();

            // Remove extra spaces
            normalized = Regex.Replace(normalized, @"\s+", " ");

            return normalized;
        }

        /// <summary>
        /// Check if a service matches search criteria
        /// Searches by name and category
        /// </summary>
        private static bool MatchesSearchCriteria(Servies service, string searchTerm)
        {
            if (service == null || string.IsNullOrWhiteSpace(searchTerm))
                return true;

            // Check service name
            if (!string.IsNullOrWhiteSpace(service.NameServies) &&
                service.NameServies.ToLower().Contains(searchTerm))
                return true;

            // Check category name
            if (service.Category != null &&
                !string.IsNullOrWhiteSpace(service.Category.Name) &&
                service.Category.Name.ToLower().Contains(searchTerm))
                return true;

            // Check price (search for price patterns like "50" for "50€")
            if (!string.IsNullOrWhiteSpace(service.PriceServies) &&
                service.PriceServies.ToLower().Contains(searchTerm))
                return true;

            return false;
        }

        /// <summary>
        /// Check if a work team matches search criteria
        /// </summary>
        private static bool MatchesWorkTeamCriteria(WorkTeam team, string searchTerm)
        {
            if (team == null || string.IsNullOrWhiteSpace(searchTerm))
                return true;

            // Check team name
            if (!string.IsNullOrWhiteSpace(team.Name) &&
                team.Name.ToLower().Contains(searchTerm))
                return true;

            // Check description if available
            if (!string.IsNullOrWhiteSpace(team.Description) &&
                team.Description.ToLower().Contains(searchTerm))
                return true;

            // Check job title if available
            if (!string.IsNullOrWhiteSpace(team.Job) &&
                team.Job.ToLower().Contains(searchTerm))
                return true;

            return false;
        }

        /// <summary>
        /// Highlight matching text in a string
        /// Returns HTML-like markup for highlighting (optional usage)
        /// </summary>
        public static string HighlightMatch(string text, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(searchTerm))
                return text;

            var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
            return regex.Replace(text, match => $"<highlight>{match.Value}</highlight>");
        }

        /// <summary>
        /// Get match relevance score (0-100) for sorting results
        /// Higher score = better match
        /// </summary>
        public static int GetRelevanceScore(Servies service, string searchTerm)
        {
            if (service == null || string.IsNullOrWhiteSpace(searchTerm))
                return 0;

            int score = 0;
            var normalizedSearch = NormalizeSearchTerm(searchTerm);
            var lowerSearch = normalizedSearch.ToLower();

            // Exact match on name: 100 points
            if (service.NameServies?.ToLower() == lowerSearch)
                score += 100;
            // Name starts with search term: 80 points
            else if (service.NameServies?.ToLower().StartsWith(lowerSearch) == true)
                score += 80;
            // Name contains search term: 50 points
            else if (service.NameServies?.ToLower().Contains(lowerSearch) == true)
                score += 50;

            // Category match: 30 points
            if (service.Category?.Name?.ToLower().Contains(lowerSearch) == true)
                score += 30;

            // Price match: 10 points
            if (service.PriceServies?.ToLower().Contains(lowerSearch) == true)
                score += 10;

            return score;
        }

        /// <summary>
        /// Sort services by relevance (descending)
        /// </summary>
        public static List<Servies> SortByRelevance(List<Servies> services, string searchTerm)
        {
            return services
                .OrderByDescending(s => GetRelevanceScore(s, searchTerm))
                .ToList();
        }
    }
}
