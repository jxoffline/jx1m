using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Interface
{
    public interface IExecCommand
    {
        int ExecCommand(string[] args);
    }
}
