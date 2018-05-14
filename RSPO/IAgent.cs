using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPO
{
    [Entity]
    public interface IAgent
    {
        string Name { get; set; }
        string NickName { get; set; }
        string PasswordHash { get; set; }
        string Phone { get; set; }
        string Email { get; set; }
        RoleEnum Role { get; set; }
        string GUID { get; set; }
        ICollection<IProperty> Properties { get; set; }
    }

    public enum RoleEnum
    {
        Unknown,
        Agent,
        Owner,
        Buyer
    }
}
