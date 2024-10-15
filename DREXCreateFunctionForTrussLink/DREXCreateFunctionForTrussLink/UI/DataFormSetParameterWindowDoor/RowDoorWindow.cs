using Autodesk.Revit.DB;
using DREXCreateFunctionForTrussLink.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DREXCreateFunctionForTrussLink.UI.DataFormSetParameterWindowDoor
{
    public class RowDoorWindow
    {
        public List<FamilyInstance> ins = new List<FamilyInstance>();
        public bool IsExistError = false;
        public bool IsError = false;

        public RowDoorWindow(FamilyInstance sym)
        {
            ins.Add(sym);

            Update();
        }

        public bool IsSameFamilySymbol(FamilySymbol sym)
        {
            return sym.Id == ins.First().Symbol.Id;
        }

        public void AddInstance(FamilyInstance sym)
        {
            ins.Add(sym);
            Update();
        }

        private void Update()
        {
            FamilyInstance insFir = ins.FirstOrDefault();

            if (insFir.Category.Id == new ElementId(BuiltInCategory.OST_Windows))
            {
                ClmCategory = "窓";
            }
            else if (insFir.Category.Id == new ElementId(BuiltInCategory.OST_Doors))
            {
                Parameter param = Common.GetParameter(insFir.Symbol, "建具 シャッター類");
                if (param == null || param.AsInteger() == 0)
                {
                    ClmCategory = "ドア";
                }
                else
                {
                    ClmCategory = "シャッター";
                }
            }

            Parameter prmTrussType = Common.GetParameter(insFir.Symbol, Define.TRUSS_TYPE);
            if (prmTrussType != null)
            {
                ClmIsIgnoreType = prmTrussType.AsInteger() == 1;
            } else
            {
                ClmIsIgnoreType = false;
            }

            ClmFamilyName = insFir.Symbol.FamilyName;
            ClmTypeName = insFir.Symbol.Name;
            ClmCount = ins.Count.ToString();

            List<String> fromRoom = ins.ConvertAll<string>(x => x.FromRoom == null ? "" : x.FromRoom.Name).ToList();
            List<String> toRoom = ins.ConvertAll<string>(x => x.ToRoom == null ? "" : x.ToRoom.Name).ToList();

            fromRoom = fromRoom.Distinct().ToList();
            toRoom = toRoom.Distinct().ToList();

            ClmFromRoomName = String.Join(",", fromRoom);
            ClmToRoomName = String.Join(",", toRoom);

            Parameter prmShurui = Common.GetParameter(insFir.Symbol, "建具_種類");
            if (prmShurui != null)
            {
                ClmTateguShurui = prmShurui.AsString();
            }

            Parameter prmBango = Common.GetParameter(insFir.Symbol, "建具_番号");
            if (prmBango != null)
            {
                ClmTateguBango = prmBango.AsString();
            }
        }

        public FamilySymbol GetSymbol()
        {
            return ins.First().Symbol;
        }

        [DisplayName("連携対象外")]
        public bool ClmIsIgnoreType
        {
            get;
            set;
        }

        [DisplayName("分類")]
        [ReadOnly(true)]
        public string ClmCategory
        {
            get;
            set;
        }

        [DisplayName("ファミリ名")]
        [ReadOnly(true)]
        public string ClmFamilyName
        {
            get;
            set;
        }

        [DisplayName("タイプ名")]
        [ReadOnly(true)]
        public string ClmTypeName
        {
            get;
            set;
        }

        [DisplayName("個数")]
        [ReadOnly(true)]
        public string ClmCount
        {
            get;
            set;
        }

        [DisplayName("部屋から")]
        [ReadOnly(true)]
        public string ClmFromRoomName
        {
            get;
            set;
        }

        [DisplayName("部屋へ")]
        [ReadOnly(true)]
        public string ClmToRoomName
        {
            get;
            set;
        }

        [DisplayName("建具種類")]
        public string ClmTateguShurui
        {
            get;
            set;
        }

        [DisplayName("建具番号")]
        public string ClmTateguBango
        {
            get;
            set;
        }

        [DisplayName("メッセージ")]
        [ReadOnly(true)]
        public string ClmMessage
        {
            get;
            set;
        }

        public double GetParameter(FamilySymbol sym, string sParam1, string sParam2)
        {
            Parameter prm1 = sym.LookupParameter(sParam1);
            Parameter prm2 = sym.LookupParameter(sParam2);

            double valRet = -1.0;
            if (prm1 != null)
            {
                valRet = prm1.AsDouble();
            }
            if (prm2 != null)
            {
                valRet = Math.Max(valRet, prm2.AsDouble());
            }
            return valRet;
        }

        public bool IsTargetFamily()
        {
            FamilySymbol sym = this.GetSymbol();
            if (sym == null)
            {
                return false;
            }

            if (GetParameter(sym, "幅", "全幅") <= 0.0)
            {
                return false;
            }
            if (GetParameter(sym, "高さ", "全高") <= 0.0)
            {
                return false;
            }

            return true;
        }

        public static explicit operator RowDoorWindow(DataGridViewRow v)
        {
            throw new NotImplementedException();
        }
    }
}
