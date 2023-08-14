using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChatServer
{
    public interface IMenuItem
    {
        string? Title { get; }
        string? Description { get; }
        void Process();
    }
    public interface IMenu : IMenuItem
    {
        IEnumerable<IMenuItem> Items { get; }
    }
}
