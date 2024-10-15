using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DREXCreateFunctionForTrussLink.Command.Handler
{
    internal enum TrussHandlerType
    {
        Unknown,
        SelectElement,
        TrussToRevit,
        RevitToTruss,
        TrussIDClear
    }

    internal class HandlerCmdForTruss : IExternalEventHandler
    {
        private object _valInput = null;
        private TrussHandlerType _type = TrussHandlerType.Unknown;

        public void SetData(TrussHandlerType type, object val)
        {
            _valInput = val;
            _type = type;
        }

        public void Execute(UIApplication app)
        {
            if (_type == TrussHandlerType.SelectElement)
            {
                HdlCmdSelectTrussRevit rvt = new HdlCmdSelectTrussRevit();
                rvt.sTrussId = (string)_valInput;
                rvt.Execute(app);
            }
            else if (_type == TrussHandlerType.RevitToTruss)
            {
                HdlCmdRevitToTruss rvt = new HdlCmdRevitToTruss();
                rvt.ProjectId = (int)_valInput;
                rvt.Execute(app);
            }
            else if (_type == TrussHandlerType.TrussToRevit)
            {
                HdlCmdTrussToRevit rvt = new HdlCmdTrussToRevit();
                rvt.ProjectId = (int)_valInput;
                rvt.Execute(app);
            }
            else if (_type == TrussHandlerType.TrussIDClear)
            {
                HdlCmdTrussIDClear rvt = new HdlCmdTrussIDClear();
                rvt.Execute(app);
            }

            _type = TrussHandlerType.Unknown;
            _valInput = null;
        }

        public string GetName()
        {
            return "HandlerCmdSelectTrussRevit";
        }
    }
}