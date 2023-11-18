using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Bách Bảo Rương
    /// </summary>
    public class KTSeashellCircle
    {
        /// <summary>
        /// Loại ô quay trong khung Bách bảo rương
        /// </summary>
        public enum SeashellCircleCellType
        {
            /// <summary>
            /// Tinh hoạt lực
            /// </summary>
            GatherMakePoint = 0,
            /// <summary>
            /// Bạc
            /// </summary>
            Money = 1,
            /// <summary>
            /// KNB khóa
            /// </summary>
            BoundToken = 2,
            /// <summary>
            /// Huyền Tinh
            /// </summary>
            CrystalStone = 3,
        }

        /// <summary>
        /// Thông tin nhóm ô
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// Loại
            /// </summary>
            public SeashellCircleCellType Type { get; set; }

            /// <summary>
            /// Tên loại
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Giá trị theo tầng
            /// </summary>
            public List<int> ValuesByStage { get; set; }

            /// <summary>
            /// Các vị trí 1 sao
            /// </summary>
            public List<int> Position_1 { get; set; }

            /// <summary>
            /// Các vị trí 2 sao
            /// </summary>
            public List<int> Position_2 { get; set; }

            /// <summary>
            /// Các vị trí 3 sao
            /// </summary>
            public List<int> Position_3 { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static Cell Parse(XElement xmlNode)
            {
                /// Tạo mới đối tượng
                Cell cell = new Cell()
                {
                    Type = (SeashellCircleCellType) int.Parse(xmlNode.Attribute("Type").Value),
                    Name = xmlNode.Attribute("Name").Value,
                    ValuesByStage = new List<int>(),
                    Position_1 = new List<int>(),
                    Position_2 = new List<int>(),
                    Position_3 = new List<int>(),
                };

                /// Duyệt danh sách giá trị theo tầng
                foreach (string valueStr in xmlNode.Attribute("StageValues").Value.Split(';'))
                {
                    try
                    {
                        int value = int.Parse(valueStr);
                        cell.ValuesByStage.Add(value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        cell.ValuesByStage.Add(0);
                    }
                }

                /// Duyệt danh sách các vị trí 1 sao
                foreach (string valueStr in xmlNode.Attribute("Position_1").Value.Split(';'))
                {
                    try
                    {
                        int value = int.Parse(valueStr);
                        cell.Position_1.Add(value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        cell.Position_1.Add(-1);
                    }
                }

                /// Duyệt danh sách các vị trí 2 sao
                foreach (string valueStr in xmlNode.Attribute("Position_2").Value.Split(';'))
                {
                    try
                    {
                        int value = int.Parse(valueStr);
                        cell.Position_2.Add(value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        cell.Position_2.Add(-1);
                    }
                }

                /// Duyệt danh sách các vị trí 1 sao
                foreach (string valueStr in xmlNode.Attribute("Position_3").Value.Split(';'))
                {
                    try
                    {
                        int value = int.Parse(valueStr);
                        cell.Position_3.Add(value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        cell.Position_3.Add(-1);
                    }
                }

                /// Trả về đối tượng
                return cell;
            }
        }

        /// <summary>
        /// Quy đổi tầng tương ứng sang sò
        /// </summary>
        public class ExchangeSeashellStage
        {
            /// <summary>
            /// Thứ tự tầng
            /// </summary>
            public int Stage { get; set; }

            /// <summary>
            /// Giá trị
            /// </summary>
            public int Value { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static ExchangeSeashellStage Parse(XElement xmlNode)
            {
                return new ExchangeSeashellStage()
                {
                    Stage = int.Parse(xmlNode.Attribute("ID").Value),
                    Value = int.Parse(xmlNode.Attribute("Value").Value),
                };
            }
        }

        /// <summary>
        /// Tỷ lệ cược đổi được bao nhiêu lần phần thưởng
        /// </summary>
        public class BetRate
        {
            /// <summary>
            /// Cược 2 sò
            /// </summary>
            public int Bet_2 { get; set; }

            /// <summary>
            /// Cược 10 sò
            /// </summary>
            public int Bet_10 { get; set; }

            /// <summary>
            /// Cược 50 sò
            /// </summary>
            public int Bet_50 { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static BetRate Parse(XElement xmlNode)
            {
                return new BetRate()
                {
                    Bet_2 = int.Parse(xmlNode.Attribute("Bet_2").Value),
                    Bet_10 = int.Parse(xmlNode.Attribute("Bet_10").Value),
                    Bet_50 = int.Parse(xmlNode.Attribute("Bet_50").Value),
                };
            }
        }

        /// <summary>
        /// Tỷ lệ quay các ô cùng loại tương ứng theo tầng
        /// </summary>
        public class TurnRateByStage
        {
            /// <summary>
            /// Thứ tự tầng
            /// </summary>
            public int Stage { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào ô cùng loại lần trước
            /// </summary>
            public int RateToSameType { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào ô cùng loại 1 sao
            /// </summary>
            public int RateToPosition_1 { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào ô cùng loại 2 sao
            /// </summary>
            public int RateToPosition_2 { get; set; }

            /// <summary>
            /// Tỷ lệ quay vào ô cùng loại 3 sao
            /// </summary>
            public int RateToPosition_3 { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static TurnRateByStage Parse(XElement xmlNode)
            {
                return new TurnRateByStage()
                {
                    Stage = int.Parse(xmlNode.Attribute("ID").Value),
                    RateToSameType = int.Parse(xmlNode.Attribute("RateToSameType").Value),
                    RateToPosition_1 = int.Parse(xmlNode.Attribute("RateToPosition_1").Value),
                    RateToPosition_2 = int.Parse(xmlNode.Attribute("RateToPosition_2").Value),
                    RateToPosition_3 = int.Parse(xmlNode.Attribute("RateToPosition_3").Value),
                };
            }
        }

        /// <summary>
        /// Danh sách nhóm ô
        /// </summary>
        public List<Cell> Cells { get; set; }

        /// <summary>
        /// Danh sách đổi sò theo tầng
        /// </summary>
        public List<ExchangeSeashellStage> ExchangeSeashells { get; set; }

        /// <summary>
        /// Tỷ lệ cược đổi phần thưởng
        /// </summary>
        public BetRate BetRates { get; set; }

        /// <summary>
        /// Danh sách tỷ lệ quay vào ô cùng nhóm theo tầng
        /// </summary>
        public List<TurnRateByStage> TurnRateByStages { get; set; }

        /// <summary>
        /// Thời gian quay tối thiểu (Tick)
        /// </summary>
        public int MinTimeTick { get; set; }

        /// <summary>
        /// Thời gian quay tối đa (Tick)
        /// </summary>
        public int MaxTimeTick { get; set; }

        /// <summary>
        /// Số vòng quay tối thiểu
        /// </summary>
        public int MinRound { get; set; }

        /// <summary>
        /// Số vòng quay tối thiểu
        /// </summary>
        public int MaxRound { get; set; }

        /// <summary>
        /// Tỷ lệ quay vào Rương
        /// </summary>
        public int RateToTreasure { get; set; }

        /// <summary>
        /// Vị trí có rương
        /// </summary>
        public List<int> TreasurePosition { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static KTSeashellCircle Parse(XElement xmlNode)
        {
            /// Tạo mới đối tượng
            KTSeashellCircle circle = new KTSeashellCircle()
            {
                MinTimeTick = int.Parse(xmlNode.Attribute("MinTimeTick").Value),
                MaxTimeTick = int.Parse(xmlNode.Attribute("MaxTimeTick").Value),
                MinRound = int.Parse(xmlNode.Attribute("MinRound").Value),
                MaxRound = int.Parse(xmlNode.Attribute("MaxRound").Value),
                RateToTreasure = int.Parse(xmlNode.Attribute("RateToTreasure").Value),
                TreasurePosition = new List<int>(),
                Cells = new List<Cell>(),
                ExchangeSeashells = new List<ExchangeSeashellStage>(),
                BetRates = BetRate.Parse(xmlNode.Element("BetRate")),
                TurnRateByStages = new List<TurnRateByStage>(),
            };

            /// Duyệt danh sách các vị trí có rương
            foreach (string valueStr in xmlNode.Attribute("TreasurePosition").Value.Split(';'))
            {
                try
                {
                    int value = int.Parse(valueStr);
                    circle.TreasurePosition.Add(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    circle.TreasurePosition.Add(-1);
                }
            }

            /// Duyệt danh sách Cell
            foreach (XElement node in xmlNode.Element("Cells").Elements("Cell"))
            {
                /// Thêm vào danh sách
                circle.Cells.Add(Cell.Parse(node));
            }

            /// Duyệt danh sách ExchangeSeashell
            foreach (XElement node in xmlNode.Element("ExchangeSeashell").Elements("Stage"))
            {
                /// Thêm vào danh sách
                circle.ExchangeSeashells.Add(ExchangeSeashellStage.Parse(node));
            }

            /// Duyệt danh sách TurnRate
            foreach (XElement node in xmlNode.Element("TurnRate").Elements("Stage"))
            {
                /// Thêm vào danh sách
                circle.TurnRateByStages.Add(TurnRateByStage.Parse(node));
            }

            /// Trả về giá trị
            return circle;
        }
    }
}
