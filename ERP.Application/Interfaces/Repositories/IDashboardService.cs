using ERP.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IDashboardService
    {
        
            Task<List<DashboardCardMetric>> GetCardSummaryAsync();

    }
}
