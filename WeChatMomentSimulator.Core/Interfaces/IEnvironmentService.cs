using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatMomentSimulator.Core.Interfaces
{
    public interface IEnvironmentService
    {
        bool IsDevelopment();
        string GetCurrentEnvironment();
    }
}