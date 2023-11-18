using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    public delegate void GameFuBenRoleCountChanged(HuanYingSiYuanFuBenData huanYingSiYuanFuBenData, int roleCount);

    public delegate void GameElementWarRoleCountChanged(ElementWarFuBenData fuBenData, int roleCount);
}
