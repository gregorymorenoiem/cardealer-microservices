using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoleService.Domain.Entities;

namespace RoleService.Domain.Interfaces
{
    public interface IRoleLogRepository
    {
        Task<RoleLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<RoleLog>> GetAsync(RoleLogQuery query);
        Task AddAsync(RoleLog roleLog);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<string>> GetServiceNamesAsync();
        Task<RoleLogStats> GetStatsAsync(DateTime? from = null, DateTime? to = null);
    }

    public class RoleLogQuery
    {
        public string? ServiceName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class RoleLogStats
    {
        public int TotalErrors { get; set; }
        public int ErrorsLast24Hours { get; set; }
        public int ErrorsLast7Days { get; set; }
        public Dictionary<string, int> ErrorsByService { get; set; } = new Dictionary<string, int>();
        public Dictionary<int, int> ErrorsByStatusCode { get; set; } = new Dictionary<int, int>();
    }
}
