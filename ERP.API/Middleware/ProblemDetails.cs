using Microsoft.AspNetCore.Http;

namespace ERP.Infrastructure.Repositories
{
    internal class ProblemDetails
    {
        public string Detail { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public PathString Instance { get; set; }
    }
}