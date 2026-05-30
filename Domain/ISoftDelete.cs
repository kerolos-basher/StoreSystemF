using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; }
    }
}
