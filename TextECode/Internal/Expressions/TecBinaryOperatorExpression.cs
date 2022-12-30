using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecBinaryOperatorExpression : TecExpression
    {
        [Flags]
        private enum BinaryReducibleType
        {
            None = 0,
            Left = 1,
            Right = 2,
            Both = 1 | 2,
        }
        private struct BinaryOperatorInfo
        {
            public int CmdId;
            public BinaryReducibleType Reducible;
            public bool EqLike;
            public BinaryOperatorInfo(int cmdId, BinaryReducibleType reducible, bool eqLike)
            {
                CmdId = cmdId;
                Reducible = reducible;
                EqLike = eqLike;
            }
            public BinaryOperatorInfo(int cmdId, BinaryReducibleType reducible): this(cmdId, reducible, false)
            {
            }
        }
        private static readonly Dictionary<int, BinaryOperatorInfo> infoMap = new(){
            { EplParser.Mul, new BinaryOperatorInfo(15, BinaryReducibleType.Both) },
            { EplParser.Div, new BinaryOperatorInfo(16, BinaryReducibleType.Left) },
            { EplParser.IDiv, new BinaryOperatorInfo(17, BinaryReducibleType.Left) },
            { EplParser.Mod, new BinaryOperatorInfo(18, BinaryReducibleType.Left) },
            { EplParser.Plus, new BinaryOperatorInfo(19, BinaryReducibleType.Both) },
            { EplParser.Minus, new BinaryOperatorInfo(20, BinaryReducibleType.Left) },
            { EplParser.Equal, new BinaryOperatorInfo(38, BinaryReducibleType.None, true) },
            { EplParser.NotEqual, new BinaryOperatorInfo(39, BinaryReducibleType.None, true) },
            { EplParser.Less, new BinaryOperatorInfo(40, BinaryReducibleType.None, true) },
            { EplParser.Greater, new BinaryOperatorInfo(41, BinaryReducibleType.None, true) },
            { EplParser.LessOrEqual, new BinaryOperatorInfo(42, BinaryReducibleType.None, true) },
            { EplParser.GreaterOrEqual, new BinaryOperatorInfo(43, BinaryReducibleType.None, true) },
            { EplParser.ApproximatelyEqual, new BinaryOperatorInfo(44, BinaryReducibleType.None, true) },
            { EplParser.And, new BinaryOperatorInfo(45, BinaryReducibleType.Both) },
            { EplParser.Or, new BinaryOperatorInfo(46, BinaryReducibleType.Both) },
            { EplParser.Assign, new BinaryOperatorInfo(52, BinaryReducibleType.None) },
        };

        private readonly int operatorType;
        private readonly TecExpression lhsExpr;
        private readonly TecExpression rhsExpr;

        public TecBinaryOperatorExpression(CodeTranslatorContext c, int operatorType, TecExpression lhsExpr, TecExpression rhsExpr) : base(c)
        {
            this.operatorType = operatorType;
            this.lhsExpr = lhsExpr;
            this.rhsExpr = rhsExpr;
        }

        public override BaseDataTypeElem ResultDataType
        {
            get
            {
                if (infoMap[operatorType].EqLike)
                {
                    return P.SystemDataTypes.Bool;
                }
                return lhsExpr.ResultDataType;
            }
        }

        public void AddToParamList(ParamListExpression paramList)
        {
            paramList.Add(lhsExpr.ToNative());
            paramList.Add(rhsExpr.ToNative());
        }

        public override Expression ToNative()
        {
            var info = infoMap[operatorType];
            var paramList = new ParamListExpression();
            if (info.Reducible.HasFlag(BinaryReducibleType.Left) 
                && lhsExpr is TecBinaryOperatorExpression binaryExpr1 
                && binaryExpr1.operatorType == operatorType)
            {
                binaryExpr1.AddToParamList(paramList);
            }
            else
            {
                paramList.Add(lhsExpr.ToNative());
            }

            if (info.Reducible.HasFlag(BinaryReducibleType.Right)
                && rhsExpr is TecBinaryOperatorExpression binaryExpr2
                && binaryExpr2.operatorType == operatorType)
            {
                binaryExpr2.AddToParamList(paramList);
            }
            else
            {
                paramList.Add(rhsExpr.ToNative());
            }
            return new CallExpression(0, info.CmdId, paramList);
        }
    }
}
