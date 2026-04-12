using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AuthResponseDTO
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        /// <summary>Best role for admin UI (recruitment roles preferred when user has many).</summary>
        public string Role { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; }
    }
}
