using Server.Data;
using Server.Tools;
using Server.Tools.Pattern;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Tmsk.Contract;

namespace GameServer.Logic.ProtoCheck
{
    interface ICheckerBase
    {
        bool Check(object obj1, object obj2);
    }

    class ICheckerWrapper<T> : ICheckerBase where T : class
    {
        public delegate bool CheckerCallback(T data1, T data2);
        CheckerCallback _cb = null;

        public ICheckerWrapper(CheckerCallback cb)
        {
            _cb = cb;
        }

        public bool Check(object obj1, object obj2)
        {
            T data1 = obj1 as T;
            T data2 = obj2 as T;

            return _cb(data1, data2);
        }
    }

    public class ProtoChecker : SingletonTemplate<ProtoChecker>
    {
        private ProtoChecker()
        {

            RegisterCheck<SpriteMoveData>(CheckConcrete.Checker_SpriteMoveData);
            RegisterCheck<SpritePositionData>(CheckConcrete.Checker_SpritePositionData);


            RegisterCheck<CS_ClickOn>(CheckConcrete.Checker_CS_ClickOn);
            RegisterCheck<SCClientHeart>(CheckConcrete.Checker_SCClientHeart);

            RegisterCheck<SCMoveEnd>(CheckConcrete.Checker_SCMoveEnd);
            RegisterCheck<CSPropAddPoint>(CheckConcrete.Checker_CSPropAddPoint);
        }

        private void RegisterCheck<T>(ICheckerWrapper<T>.CheckerCallback cb) where T : class
        {
            checkerDic[typeof(T).FullName] = new ICheckerWrapper<T>(cb);
        }

        private Dictionary<string, ICheckerBase> checkerDic = new Dictionary<string, ICheckerBase>();


        private bool _enableCheck = false;
        public bool EnableCheck
        {
            get { return _enableCheck; }
        }



        public bool Check<T>(byte[] data, int start, int count, Socket socket) where T : class, IProtoBuffData, new()
        {
            bool bRet = CheckImpl<T>(data, start, count, socket);
            if (!bRet)
            {
                if (data == null)
                {
                    LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + ", 反序列化的data为null");
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < data.Length; ++i)
                    {
                        sb.Append((int)data[i]).Append(' ');
                    }
                    LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + " 反序列化失败data=" + sb.ToString() + " ,start=" + start + " ,count=" + count);
                    LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + " 反序列化失败, 尝试检测是否是字符串类型 " + new UTF8Encoding().GetString(data, start, count));
                }
            }

            return bRet;
        }


        private bool CheckImpl<T>(byte[] data, int start, int count, Socket socket) where T : class, IProtoBuffData, new()
        {
            if (!EnableCheck)
            {
                return true;
            }

            ICheckerBase cb = null;
            if (!checkerDic.TryGetValue(typeof(T).FullName, out cb))
            {
                return true;
            }

            T oldData = null;
            T newData = null;

            try
            {
                oldData = DataHelper.BytesToObject<T>(data, 0, count);
            }
            catch (Exception) { }

            try
            {
                newData = DataHelper.BytesToObject2<T>(data, 0, count, socket);
            }
            catch (Exception) { }

            if (oldData == null && newData != null)
            {
                LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + "， protobuf.net 解析数据为null，但是新解析方式不为null");
                return false;
            }

            if (oldData != null && newData == null)
            {
                LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + "， protobuf.net 解析数据不为null，但是新解析方式为null");
                return false;
            }

            if (oldData == null && newData == null)
            {
                LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + "， protobuf.net 解析数据为null，新解析方式为null");
                return false;
            }

            if (!cb.Check(oldData, newData))
            {
                LogManager.WriteLog(LogTypes.Fatal, typeof(T).FullName + "， protobuf.net 解析数据不为null，新解析方式不为null，但是解析出来的数据不一致");
                return false;
            }

            return true;
        }
    }

    static class CheckConcrete
    {
        public static bool Checker_SpriteMoveData(SpriteMoveData data1, SpriteMoveData data2)
        {
            return data1.RoleID == data2.RoleID
                && data1.ToX == data2.ToX
                && data1.ToY == data2.ToY
                && data1.FromX == data2.FromX
                && data1.FromY == data2.FromY
                && data1.PathString == data2.PathString;
        }

        public static bool Checker_SpritePositionData(SpritePositionData data1, SpritePositionData data2)
        {
            return data1.RoleID == data2.RoleID
                && data1.PosX == data2.PosX
                && data1.PosY == data2.PosY;
        }

        public static bool Checker_CS_ClickOn(CS_ClickOn data1, CS_ClickOn data2)
        {
            return data1.RoleId == data2.RoleId
                && data1.MapCode == data2.MapCode
                && data1.NpcId == data2.NpcId
                && data1.ExtId == data2.ExtId;
        }

        public static bool Checker_SCClientHeart(SCClientHeart data1, SCClientHeart data2)
        {
            return data1.RoleID == data2.RoleID
                && data1.RandToken == data2.RandToken
                && data1.Ticks == data2.Ticks;
        }

        public static bool Checker_SCMoveEnd(SCMoveEnd data1, SCMoveEnd data2)
        {
            return data1.RoleID == data2.RoleID
                && data1.Action == data2.Action
                && data1.MapCode == data2.MapCode
                && data1.ToMapX == data2.ToMapX
                && data1.ToMapY == data2.ToMapY
                && data1.ToDiection == data2.ToDiection
                && data1.TryRun == data2.TryRun;
        }

        public static bool Checker_CSPropAddPoint(CSPropAddPoint data1, CSPropAddPoint data2)
        {
            return data1.RoleID == data2.RoleID
                && data1.Strength == data2.Strength
                && data1.Intelligence == data2.Intelligence
                && data1.Dexterity == data2.Dexterity
                && data1.Constitution == data2.Constitution;
        }
    }
}
